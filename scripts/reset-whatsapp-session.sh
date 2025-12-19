#!/bin/bash
# Script para resetar autenticação do WhatsApp (Baileys)

echo "🔄 Resetando sessão WhatsApp..."
echo ""

# Parar containers
echo "⏹️  Parando containers..."
docker-compose down

# Remover volume de autenticação
echo "🗑️  Removendo volume de autenticação..."
docker volume rm full_architecture_baileys_auth 2>/dev/null || echo "Volume já estava limpo"

# Reiniciar containers
echo "🚀 Reiniciando containers..."
docker-compose up -d

# Aguardar Baileys iniciar
echo "⏳ Aguardando Baileys iniciar (10 segundos)..."
sleep 10

# Mostrar QR Code
echo ""
echo "📱 QR Code para autenticação:"
echo "============================================"
curl -s http://localhost:3000/qr | jq -r '.qr' 2>/dev/null || echo "Aguarde mais alguns segundos e tente: curl http://localhost:3000/qr"
echo "============================================"
echo ""
echo "✅ Escaneie o QR Code acima com WhatsApp"
echo "   WhatsApp > Configurações > Aparelhos conectados > Conectar um aparelho"
echo ""
echo "🔍 Verificar status: curl http://localhost:3000/status"
echo "📋 Ver logs: docker-compose logs -f baileys-whatsapp-service"
