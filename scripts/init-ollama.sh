#!/bin/bash

# Script para inicializar Ollama e baixar o modelo phi3

echo "Aguardando Ollama iniciar..."
sleep 5

# Verifica se o Ollama está rodando
until curl -s http://localhost:11434/api/tags > /dev/null 2>&1; do
    echo "Aguardando Ollama estar pronto..."
    sleep 2
done

echo "Ollama está pronto. Baixando modelo phi3..."

# Baixa o modelo phi3
ollama pull phi3

echo "Modelo phi3 baixado com sucesso!"

# Mantém o script rodando
tail -f /dev/null
