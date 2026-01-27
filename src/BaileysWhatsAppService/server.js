const { default: makeWASocket, DisconnectReason, useMultiFileAuthState, fetchLatestBaileysVersion, isJidGroup } = require('@whiskeysockets/baileys');
const express = require('express');
const qrcode = require('qrcode-terminal');
const pino = require('pino');
const axios = require('axios');
const fs = require('fs');

// Custom Store Implementation
function makeInMemoryStore({ logger }) {
    const chats = {};
    const messages = {};

    function bind(ev) {
        ev.on('messages.upsert', ({ messages: newMessages, type }) => {
            if (type !== 'notify' && type !== 'append') return;

            for (const msg of newMessages) {
                const jid = msg.key.remoteJid;
                if (!messages[jid]) messages[jid] = [];

                // Avoid duplicates
                if (!messages[jid].find(m => m.key.id === msg.key.id)) {
                    messages[jid].push(msg);
                    // Limit to last 50 messages per chat to save memory
                    if (messages[jid].length > 50) messages[jid].shift();
                }
            }
        });
    }

    async function loadMessage(jid, id) {
        if (messages[jid]) {
            return messages[jid].find(m => m.key.id === id);
        }
        return undefined;
    }

    function writeToFile(path) {
        try {
            fs.writeFileSync(path, JSON.stringify(messages, null, 2));
        } catch (error) {
            if (logger) logger.error('Error writing store to file:', error);
        }
    }

    function readFromFile(path) {
        try {
            if (fs.existsSync(path)) {
                const data = JSON.parse(fs.readFileSync(path, 'utf8'));
                Object.assign(messages, data);
            }
        } catch (error) {
            if (logger) logger.error('Error reading store from file:', error);
        }
    }

    return { bind, loadMessage, writeToFile, readFromFile };
}

const app = express();
app.use(express.json());

const PORT = process.env.PORT || 3000;
const AUTH_DIR = './auth_info';
const WEBHOOK_URL = process.env.WEBHOOK_URL || 'http://ai-chat-service:8080/api/whatsapp/webhook';

// Logger configuration
const logger = pino({ level: 'info' });

// WhatsApp client state
let sock = null;
let qrCodeData = null;
let connectionState = 'disconnected';
let isConnecting = false;

// Store received messages for webhook forwarding
const messageQueue = [];

// Persistent store setup
const store = makeInMemoryStore({ logger });
const STORE_FILE = './baileys_store.json';
store.readFromFile(STORE_FILE);
// Save store every 10 seconds
setInterval(() => {
    store.writeToFile(STORE_FILE);
}, 10_000);

// Function to forward messages to AIChatService webhook
async function forwardToWebhook(messageData) {
    try {
        // Format in Meta API compatible structure
        const payload = {
            object: 'whatsapp_business_account',
            entry: [{
                id: 'unknown',
                changes: [{
                    value: {
                        messaging_product: 'whatsapp',
                        metadata: {
                            display_phone_number: 'unknown',
                            phone_number_id: 'unknown'
                        },
                        messages: [{
                            from: messageData.from,
                            text: {
                                body: messageData.message
                            },
                            id: messageData.id,
                            timestamp: String(messageData.timestamp),
                            type: 'text'
                        }]
                    },
                    field: 'messages'
                }]
            }]
        };

        logger.info(`Forwarding message to webhook: ${WEBHOOK_URL}`);

        await axios.post(WEBHOOK_URL, payload, {
            headers: {
                'Content-Type': 'application/json'
            },
            timeout: 5000
        });

        logger.info('Message forwarded successfully to AIChatService');
    } catch (error) {
        logger.error(`Error forwarding message to webhook: ${error.message}`);
        // Don't throw - keep processing other messages even if webhook fails
    }
}

async function connectToWhatsApp() {
    if (isConnecting) {
        logger.info('Connection already in progress...');
        return;
    }

    isConnecting = true;

    try {
        const { state, saveCreds } = await useMultiFileAuthState(AUTH_DIR);
        const { version } = await fetchLatestBaileysVersion();

        sock = makeWASocket({
            version,
            logger: pino({ level: 'info' }),
            printQRInTerminal: true,
            auth: state,
            browser: ['Baileys WhatsApp Service', 'Chrome', '1.0.0'],
            // Improved configuration for stability and retries
            connectTimeoutMs: 60000,
            defaultQueryTimeoutMs: 0,
            keepAliveIntervalMs: 10000,
            emitOwnEvents: true,
            getMessage: async (key) => {
                if (store) {
                    const msg = await store.loadMessage(key.remoteJid, key.id);
                    return msg?.message || undefined;
                }
                return undefined;
            }
        });

        // Bind store to socket events
        store.bind(sock.ev);

        // Connection update handler
        sock.ev.on('connection.update', async (update) => {
            const { connection, lastDisconnect, qr } = update;

            if (qr) {
                qrCodeData = qr;
                logger.info('QR Code updated. Scan with WhatsApp to authenticate.');
                qrcode.generate(qr, { small: true });
            }

            if (connection === 'close') {
                const shouldReconnect = lastDisconnect?.error?.output?.statusCode !== DisconnectReason.loggedOut;
                logger.info(`Connection closed. Reconnecting: ${shouldReconnect}`);
                connectionState = 'disconnected';

                if (shouldReconnect) {
                    setTimeout(connectToWhatsApp, 3000);
                } else {
                    qrCodeData = null;
                }
                isConnecting = false;
            } else if (connection === 'open') {
                logger.info('WhatsApp connection opened successfully!');
                connectionState = 'connected';
                qrCodeData = null;
                isConnecting = false;
            } else if (connection === 'connecting') {
                connectionState = 'connecting';
                logger.info('Connecting to WhatsApp...');
            }

            if (update.receivedPendingNotifications) {
                logger.info('Received all pending notifications');
            }
        });

        // Credentials update handler
        sock.ev.on('creds.update', saveCreds);

        // Message handler
        sock.ev.on('messages.upsert', async ({ messages, type }) => {
            // DEBUG: Log every incoming event
            logger.info({ msg: 'DEBUG: messages.upsert received', type, count: messages.length, raw: messages });

            if (type === 'notify') {
                for (const msg of messages) {
                    const jid = msg.key.remoteJid;
                    const isGroup = isJidGroup(jid);
                    const isStatus = jid === 'status@broadcast' || jid.endsWith('@broadcast');
                    const isNewsletter = jid.endsWith('@newsletter');

                    if (!msg.key.fromMe && msg.message && !isGroup && !isStatus && !isNewsletter) {
                        const messageData = {
                            id: msg.key.id,
                            from: msg.key.remoteJid,
                            timestamp: msg.messageTimestamp,
                            message: msg.message?.conversation ||
                                msg.message?.extendedTextMessage?.text ||
                                '',
                            type: Object.keys(msg.message)[0]
                        };

                        logger.info(`Received message from ${messageData.from}: ${messageData.message}`);
                        messageQueue.push(messageData);

                        // Forward to AIChatService webhook
                        await forwardToWebhook(messageData);
                    }
                }
            }
        });

    } catch (error) {
        logger.error('Error connecting to WhatsApp:', error);
        connectionState = 'error';
        isConnecting = false;
        // Retry connection after error
        setTimeout(connectToWhatsApp, 5000);
    }
}

// API Endpoints

// Health check
app.get('/health', (req, res) => {
    res.json({
        status: 'healthy',
        service: 'BaileysWhatsAppService',
        connectionState
    });
});

// Get connection status
app.get('/status', (req, res) => {
    res.json({
        connected: connectionState === 'connected',
        state: connectionState,
        hasQR: qrCodeData !== null
    });
});

// Get QR code for authentication
app.get('/qr', (req, res) => {
    if (qrCodeData) {
        res.json({
            success: true,
            qr: qrCodeData,
            message: 'Scan this QR code with WhatsApp'
        });
    } else if (connectionState === 'connected') {
        res.json({
            success: false,
            message: 'Already authenticated'
        });
    } else {
        res.json({
            success: false,
            message: 'QR code not available yet. Please wait...'
        });
    }
});

// Send text message
app.post('/send', async (req, res) => {
    try {
        const { to, message } = req.body;

        if (!to || !message) {
            return res.status(400).json({
                success: false,
                error: 'Missing required fields: to, message'
            });
        }

        if (connectionState !== 'connected' || !sock) {
            return res.status(503).json({
                success: false,
                error: 'WhatsApp not connected'
            });
        }

        // Format phone number (ensure it has @s.whatsapp.net suffix if none present)
        const formattedNumber = to.includes('@') ? to : `${to}@s.whatsapp.net`;

        const result = await sock.sendMessage(formattedNumber, {
            text: message
        });

        logger.info(`Message sent to ${to}: ${message}`);

        res.json({
            success: true,
            messageId: result.key.id,
            timestamp: result.messageTimestamp
        });

    } catch (error) {
        logger.error('Error sending message:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

// Get received messages (webhook alternative)
app.get('/messages', (req, res) => {
    const messages = [...messageQueue];
    messageQueue.length = 0; // Clear the queue
    res.json({
        success: true,
        messages,
        count: messages.length
    });
});

// Webhook endpoint for receiving messages
app.post('/webhook', (req, res) => {
    logger.info('Webhook called:', req.body);
    res.json({ success: true });
});

// Start server
app.listen(PORT, async () => {
    logger.info(`Baileys WhatsApp Service running on port ${PORT}`);
    logger.info('Starting WhatsApp connection...');
    await connectToWhatsApp();
});

// Graceful shutdown
process.on('SIGINT', async () => {
    logger.info('Shutting down gracefully...');
    if (sock) {
        await sock.logout();
    }
    process.exit(0);
});
