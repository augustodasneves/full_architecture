# WhatsApp Registration System

Sistema de registro e atualização de dados de usuários via WhatsApp, construído com arquitetura de microserviços usando .NET 8 e containerizado com Docker.

## 📋 Visão Geral

Este projeto implementa um sistema inteligente de atendimento ao cliente que permite aos usuários registrar e atualizar suas informações pessoais (PII - Personally Identifiable Information) através de conversas via WhatsApp. O sistema utiliza um modelo de linguagem local (LLM) para processar interações naturais e uma arquitetura baseada em eventos para garantir escalabilidade e resiliência.

### Fluxo Principal

1. **Usuário** envia mensagem via WhatsApp
2. **WhatsApp Proxy API** recebe o webhook e encaminha para o AI Chat Service
3. **AI Chat Service** processa a mensagem usando:
   - LLM (Ollama) para entender a intenção do usuário
   - Redis para manter estado da conversa
   - Validadores para garantir qualidade dos dados
4. **Event Bus** (Azure Service Bus) comunica eventos entre serviços
5. **PII Update Worker** consome eventos e atualiza dados via User Account API
6. **User Account API** persiste dados no PostgreSQL

## 🏗️ Arquitetura

```
┌─────────────────┐
│   WhatsApp      │
│   Business API  │
└────────┬────────┘
         │ Webhook
         ▼
┌──────────────────────────────────────────────────────────────┐
│                    Docker Network                            │
│                                                              │
│  ┌──────────────────┐      ┌────────────────┐                │
│  │ WhatsApp Proxy   │──────│  AI Chat       │                │
│  │ API :8082        │      │  Service :8081 │                │
│  └──────────────────┘      └───────┬────────┘                │
│                                     │                        │
│                                     ├───► Redis :6379        │
│                                     │                        │
│                                     ├───► Ollama :11434      |
│                                     │    (LLM - phi3)        │
│                                     │                        │
│                                     ▼                        │
│                         ┌──────────────────────┐             │
│                         │  Service Bus         │             │
│                         │  Emulator :5672      │             │
│                         └──────────┬───────────┘             │
│                                    │                         │
│                                    ▼                         │
│                         ┌──────────────────────┐             │
│                         │  PII Update Worker   │             │
│                         └──────────┬───────────┘             │
│                                    │                         │
│                                    ▼                         │
│                         ┌──────────────────────┐             │
│                         │  User Account API    │             │
│                         │  :8080               │             │
│                         └──────────┬───────────┘             │
│                                    │                         │
│                                    ▼                         │
│                         ┌──────────────────────┐             │
│                         │  PostgreSQL :5432    │             │
│                         └──────────────────────┘             │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

## 🚀 Tecnologias Utilizadas

### Backend
- **.NET 8** - Framework principal
- **C#** - Linguagem de programação
- **ASP.NET Core Web API** - APIs RESTful

### Banco de Dados
- **PostgreSQL 15** - Banco de dados relacional (dados de usuários)
- **Redis** - Cache em memória (estado de conversação)
- **Entity Framework Core** - ORM

### Mensageria
- **Azure Service Bus Emulator** - Comunicação assíncrona entre serviços

### IA & Machine Learning
- **Ollama** - Servidor LLM local
- **Phi3** - Modelo de linguagem para processamento de linguagem natural

### Infraestrutura
- **Docker** & **Docker Compose** - Containerização e orquestração
- **Azure SQL Edge** - Dependência do Service Bus Emulator

### Integrações
- **Meta WhatsApp Business API** - Comunicação com usuários via WhatsApp

## 📁 Estrutura do Projeto

```
full_architecture/
├── src/
│   ├── AIChatService/              # Serviço de chat com IA
│   │   ├── Controllers/            # Endpoints da API
│   │   ├── Services/               # Lógica de negócio (FlowEngine, LLM)
│   │   ├── Validators/             # Validação de dados (email, telefone, endereço)
│   │   └── Program.cs
│   │
│   ├── WhatsAppProxyApi/           # Proxy para WhatsApp Business API
│   │   ├── Controllers/            # Webhook endpoints
│   │   ├── Services/               # Integração com Meta API
│   │   ├── Models/                 # DTOs e configurações
│   │   └── Program.cs
│   │
│   ├── UserAccountApi/             # API de gerenciamento de usuários
│   │   ├── Controllers/            # CRUD de usuários
│   │   ├── Data/                   # DbContext e configurações EF
│   │   ├── Models/                 # Entidades de domínio
│   │   └── Program.cs
│   │
│   ├── PiiUpdateWorker/            # Worker de processamento de eventos
│   │   ├── Services/               # Consumidor Service Bus
│   │   └── Program.cs
│   │
│   ├── Shared/                     # Código compartilhado
│   │   ├── Events/                 # Eventos de domínio
│   │   ├── Interfaces/             # Contratos de serviços
│   │   └── Models/                 # DTOs compartilhados
│   │
│   └── ServiceBusConfig.json       # Configuração do emulador
│
├── scripts/
│   └── init-ollama.sh              # Script de inicialização do Ollama
│
├── docker-compose.yml              # Orquestração dos containers
├── WhatsAppRegistration.sln        # Solução .NET
└── .env                            # Variáveis de ambiente (não versionado)
```

### Descrição dos Serviços

#### 🤖 AI Chat Service (Porta 8081)
- Processa mensagens recebidas do WhatsApp
- Gerencia fluxo de conversação (FlowEngine)
- Integra com LLM para entender intenção do usuário
- Valida dados coletados (telefone, email, endereço)
- Publica eventos no Service Bus
- Mantém estado da conversa no Redis

#### 📱 WhatsApp Proxy API (Porta 8082)
- Recebe webhooks da Meta WhatsApp Business API
- Envia mensagens de volta ao usuário via WhatsApp
- Abstrai detalhes da API do WhatsApp

#### 👤 User Account API (Porta 8080)
- CRUD de contas de usuário
- Persiste dados no PostgreSQL
- Gerencia informações pessoais (PII)

#### ⚙️ PII Update Worker
- Consome eventos de atualização de PII do Service Bus
- Processa atualizações de forma assíncrona
- Envia dados atualizados para User Account API

## 🔧 Pré-requisitos

Antes de executar o projeto, você precisa ter instalado:

- **Docker Desktop** (Windows/Mac) ou **Docker Engine** + **Docker Compose** (Linux)
  - Versão mínima: Docker 20.10+
  - Versão mínima Docker Compose: 2.0+
- **Git** (para clonar o repositório)
- **(Opcional)** **.NET 8 SDK** - apenas se quiser desenvolver localmente sem Docker

### Requisitos da Meta WhatsApp Business API

Para usar o WhatsApp:
1. Conta Meta for Developers
2. App configurado no Meta Business
3. WhatsApp Business API habilitada
4. Token de acesso (Access Token)
5. Phone Number ID

## 🐳 Executando Localmente com Docker

### 1. Clone o Repositório

```bash
git clone <url-do-repositorio>
cd full_architecture
```

### 2. Configure as Variáveis de Ambiente

Crie um arquivo `.env` na raiz do projeto com as seguintes variáveis:

```env
# WhatsApp Configuration
WHATSAPP_ACCESS_TOKEN=seu_token_de_acesso_meta
WHATSAPP_PHONE_NUMBER_ID=seu_phone_number_id

# Opcional - Registry Docker (se usar)
DOCKER_REGISTRY=
```

> ⚠️ **Importante**: Substitua `seu_token_de_acesso_meta` e `seu_phone_number_id` pelos valores reais obtidos no Meta for Developers.

### 3. Build das Imagens

```bash
docker-compose build
```

Este comando irá:
- Compilar os 4 microserviços (.NET)
- Criar imagens Docker customizadas
- Baixar imagens base necessárias (PostgreSQL, Redis, etc.)

**Tempo estimado**: 3-5 minutos na primeira execução

### 4. Inicie os Containers

```bash
docker-compose up -d
```

Este comando irá:
- Iniciar todos os containers em modo detached (background)
- Criar a rede Docker `app-network`
- Criar volumes persistentes para PostgreSQL e Ollama
- Baixar o modelo Phi3 automaticamente no Ollama

**Tempo estimado**: 2-3 minutos (+ tempo para download do modelo Phi3: ~2GB)

### 5. Verifique o Status dos Containers

```bash
docker-compose ps
```

Todos os serviços devem mostrar status `Up`:

```
NAME                    STATUS              PORTS
postgres                Up                  0.0.0.0:5432->5432/tcp
redis                   Up                  0.0.0.0:6379->6379/tcp
servicebus-emulator     Up                  0.0.0.0:5672->5672/tcp
sql-edge                Up                  0.0.0.0:1433->1433/tcp
ollama                  Up                  0.0.0.0:11434->11434/tcp
user-account-api        Up                  0.0.0.0:8080->8080/tcp
whatsapp-proxy-api      Up                  0.0.0.0:8082->8080/tcp
ai-chat-service         Up                  0.0.0.0:8081->8080/tcp
pii-update-worker       Up
```

### 6. Visualize os Logs

Para acompanhar os logs de todos os serviços:

```bash
docker-compose logs -f
```

Para logs de um serviço específico:

```bash
docker-compose logs -f ai-chat-service
```

## 🧪 Testando a Aplicação

### 1. Verifique as APIs

#### User Account API (Swagger)
```
http://localhost:8080/swagger
```

#### AI Chat Service (Swagger)
```
http://localhost:8081/swagger
```

#### WhatsApp Proxy API (Swagger)
```
http://localhost:8082/swagger
```

### 2. Teste o Ollama (LLM)

```bash
curl http://localhost:11434/api/tags
```

Deve retornar a lista de modelos, incluindo `phi3`.

### 3. Teste com WhatsApp (Produção)

Para testar com WhatsApp real, você precisa:

1. **Expor o webhook localmente** usando ngrok ou similar:
   ```bash
   ngrok http 8082
   ```

2. **Configurar o webhook no Meta for Developers**:
   - URL: `https://seu-dominio-ngrok.ngrok.io/api/webhook`
   - Verify Token: (conforme configurado no código)

3. **Enviar mensagem** do seu WhatsApp para o número de teste

## 🛠️ Comandos Úteis

### Parar todos os containers
```bash
docker-compose down
```

### Parar e remover volumes (⚠️ apaga dados)
```bash
docker-compose down -v
```

### Rebuild de um serviço específico
```bash
docker-compose up -d --build ai-chat-service
```

### Acessar container
```bash
docker exec -it ai-chat-service bash
```

### Ver uso de recursos
```bash
docker stats
```

## 📊 Portas Utilizadas

| Serviço                  | Porta Host | Porta Container |
|--------------------------|------------|-----------------|
| User Account API         | 8080       | 8080            |
| AI Chat Service          | 8081       | 8080            |
| WhatsApp Proxy API       | 8082       | 8080            |
| PostgreSQL               | 5432       | 5432            |
| Redis                    | 6379       | 6379            |
| Service Bus Emulator     | 5672       | 5672            |
| SQL Edge                 | 1433       | 1433            |
| Ollama                   | 11434      | 11434           |

## 🔍 Troubleshooting

### Container não inicia

```bash
# Ver logs detalhados
docker-compose logs [nome-do-servico]

# Verificar recursos do Docker
docker system df
```

### Erro de conexão com banco de dados

- Aguarde alguns segundos após `docker-compose up` para o PostgreSQL inicializar completamente
- Verifique se a porta 5432 não está em uso por outro processo

### Modelo Phi3 não baixou

```bash
# Entre no container do Ollama
docker exec -it ollama bash

# Execute manualmente
ollama pull phi3
```

### Service Bus não conecta

- Certifique-se que o SQL Edge está rodando corretamente
- Verifique os logs: `docker-compose logs servicebus-emulator`

## 🔐 Segurança

> ⚠️ **Este projeto é para desenvolvimento local**

Para produção, considere:
- Usar secrets management (Azure Key Vault, HashiCorp Vault)
- Configurar HTTPS/TLS
- Implementar autenticação e autorização robustas
- Usar variáveis de ambiente seguras
- Não versionar `.env` no Git
- Configurar rate limiting
- Implementar logging e monitoring adequados

## 📝 Licença

[Especifique a licença do projeto]

## 👥 Contribuindo

[Instruções para contribuição, se aplicável]

## 📞 Suporte

[Informações de contato ou canal de suporte]
