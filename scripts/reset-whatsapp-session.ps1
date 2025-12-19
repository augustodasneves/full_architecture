# Script PowerShell para resetar autenticação do WhatsApp (Baileys)

Write-Host "🔄 Resetando sessão WhatsApp..." -ForegroundColor Cyan
Write-Host ""

# Parar containers
Write-Host "⏹️  Parando containers..." -ForegroundColor Yellow
docker-compose down

# Remover volume de autenticação
Write-Host "🗑️  Removendo volume de autenticação..." -ForegroundColor Yellow
docker volume rm full_architecture_baileys_auth 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "   Volume já estava limpo" -ForegroundColor Gray
}

# Reiniciar containers
Write-Host "🚀 Reiniciando containers..." -ForegroundColor Green
docker-compose up -d

# Aguardar Baileys iniciar
Write-Host "⏳ Aguardando Baileys iniciar (10 segundos)..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Mostrar QR Code
Write-Host ""
Write-Host "📱 Obter QR Code para autenticação:" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Gray
Write-Host "Abra no navegador: http://localhost:3000/qr" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Gray
Write-Host ""

# Tentar mostrar QR via API
try {
    $response = Invoke-RestMethod -Uri "http://localhost:3000/qr" -TimeoutSec 5
    if ($response.qr) {
        Write-Host "QR Code disponível!" -ForegroundColor Green
        Write-Host "Acesse: http://localhost:3000/qr no navegador" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  Aguarde mais alguns segundos..." -ForegroundColor Yellow
    Write-Host "   Depois acesse: http://localhost:3000/qr" -ForegroundColor Gray
}

Write-Host ""
Write-Host "✅ Escaneie o QR Code com WhatsApp" -ForegroundColor Green
Write-Host "   WhatsApp > Configurações > Aparelhos conectados > Conectar um aparelho" -ForegroundColor Gray
Write-Host ""
Write-Host "🔍 Comandos úteis:" -ForegroundColor Cyan
Write-Host "   Ver status:  curl http://localhost:3000/status" -ForegroundColor Gray
Write-Host "   Ver logs:    docker-compose logs -f baileys-whatsapp-service" -ForegroundColor Gray
Write-Host "   Ver QR Code: start http://localhost:3000/qr" -ForegroundColor Gray
Write-Host ""
