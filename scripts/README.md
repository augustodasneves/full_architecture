# Scripts para Gestão do Sistema

Este diretório contém scripts utilitários para gerenciar o sistema.

## reset-whatsapp-session.ps1 / .sh

Reseta a sessão de autenticação do WhatsApp (Baileys) quando houver erros de criptografia.

### Windows (PowerShell)
```powershell
.\scripts\reset-whatsapp-session.ps1
```

### Linux/Mac (Bash)
```bash
chmod +x scripts/reset-whatsapp-session.sh
./scripts/reset-whatsapp-session.sh
```

### O que o script faz:
1. ✅ Para todos os containers
2. ✅ Remove volume de autenticação (`baileys_auth`)
3. ✅ Reinicia containers
4. ✅ Aguarda Baileys iniciar
5. ✅ Mostra link para QR Code

### Quando usar:
- ❌ Erros "Bad MAC Error" nos logs
- ❌ "Decrypted message with closed session"
- ❌ WhatsApp desconectado no celular
- ❌ Precisa re-autenticar

### Após executar:
1. Abra http://localhost:3000/qr no navegador
2. Escaneie o QR Code com WhatsApp
3. Aguarde conexão ser estabelecida
4. Teste enviando mensagem

---

## Comandos Rápidos

```bash
# Ver QR Code
curl http://localhost:3000/qr

# Verificar status da conexão
curl http://localhost:3000/status

# Ver logs do Baileys
docker-compose logs -f baileys-whatsapp-service

# Reiniciar apenas Baileys (sem limpar sessão)
docker-compose restart baileys-whatsapp-service
```
