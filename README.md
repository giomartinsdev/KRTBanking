# 🏦 KRT Banking - Sistema de Gestão de Limites PIX

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![AWS DynamoDB](https://img.shields.io/badge/AWS-DynamoDB-orange.svg)](https://aws.amazon.com/dynamodb/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://www.docker.com/)

Sistema completo de gestão de limites para transações PIX desenvolvido para o Banco KRT, implementando Domain-Driven Design (DDD) e Clean Architecture.

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura](#-arquitetura)
- [Pré-requisitos](#-pré-requisitos)
- [Como Executar](#-como-executar)
- [Casos de Uso do Analista de Fraudes](#-casos-de-uso-do-analista-de-fraudes)
- [API Documentation](#-api-documentation)
- [Testes](#-testes)
- [Estrutura do Projeto](#-estrutura-do-projeto)

## 🎯 Visão Geral

O **KRT Banking System** é uma API RESTful que permite aos analistas de fraudes do Banco KRT gerenciar limites para transações PIX de clientes, incluindo:

- **Cadastro de Clientes**: Registro completo com documento, agência, conta e limite PIX
- **Gestão de Limites**: Consulta, alteração e controle de limites de transações
- **Processamento de Transações**: Validação automática de limites para aprovação/negação
- **Auditoria Completa**: Histórico de todas as operações para compliance

### ✨ Principais Funcionalidades

- ✅ **Cadastro de Limite**: Gestão completa de informações de limite PIX
- ✅ **Consulta de Informações**: Busca paginada e por ID de clientes
- ✅ **Alteração de Limites**: Aumento/redução com histórico de mudanças
- ✅ **Remoção Segura**: Soft delete para manter compliance
- ✅ **Processamento PIX**: Validação e consumo automático de limites
- ✅ **Auditoria**: Logs completos de todas as operações

## 🏗️ Arquitetura

O sistema segue os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**:

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                   │
│                   (KRTBanking.API)                      │
├─────────────────────────────────────────────────────────┤
│                   Application Layer                     │
│                (KRTBanking.Application)                 │
├─────────────────────────────────────────────────────────┤
│                     Domain Layer                        │
│                  (KRTBanking.Domain)                    │
├─────────────────────────────────────────────────────────┤
│                 Infrastructure Layer                    │
│               (KRTBanking.Infrastructure)               │
└─────────────────────────────────────────────────────────┘
```

- **API Layer**: Controllers, DTOs e configurações de endpoints
- **Application Layer**: Services, interfaces e orquestração de casos de uso
- **Domain Layer**: Entidades, Value Objects, Events e regras de negócio
- **Infrastructure Layer**: Repositórios, persistência DynamoDB e integrações

Para mais detalhes, consulte [ARCHITECTURE.md](docs/ARCHITECTURE.md).

## 📋 Pré-requisitos

### Desenvolvimento Local

- **.NET 8.0 SDK** ou superior
- **Docker Desktop** (para DynamoDB local)
- **VS Code** ou **Visual Studio 2022**
- **Git**

### Extensões Recomendadas (VS Code)

- [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) - Para executar arquivos `.http`
- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- [Docker](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-docker)

## 🚀 Como Executar

### Opção 1: Docker (Recomendado)

1. **Clone o repositório**:
   ```bash
   git clone <repository-url>
   cd KRTBanking
   ```

2. **Execute com Docker Compose**:
   ```bash
   docker-compose up -d
   ```

3. **Acesse a aplicação**:
   - **API**: http://localhost:5000
   - **Swagger**: http://localhost:5000/swagger
   - **DynamoDB Admin**: http://localhost:8001

### Opção 2: Desenvolvimento Local

1. **Clone o repositório**:
   ```bash
   git clone <repository-url>
   cd KRTBanking
   ```

2. **Inicie o DynamoDB Local**:
   ```bash
   docker-compose up dynamodb-local -d
   ```

3. **Execute a aplicação**:
   ```bash
   dotnet run --project src/KRTBanking.API
   ```

4. **Acesse a aplicação**:
   - **API**: http://localhost:5261 (ou https://localhost:7236)
   - **Swagger**: http://localhost:5261/swagger

### Verificação da Instalação

Execute o health check para verificar se todos os serviços estão funcionando:

```bash
curl http://localhost:5000/health
```

## 👨‍💼 Casos de Uso do Analista de Fraudes

O arquivo [`analista-fraude-krt.http`](analista-fraude-krt.http) contém todos os cenários de teste para os casos de uso do analista de fraudes.

### 🔧 Como Usar o Arquivo HTTP

1. **Abra o VS Code** na pasta do projeto
2. **Instale a extensão REST Client** se ainda não tiver
3. **Abra o arquivo** `analista-fraude-krt.http`
4. **Configure as variáveis** no topo do arquivo:
   ```http
   @baseUrl = http://localhost:5000
   @apiVersion = 1
   @customerId = [será obtido após criar um cliente]
   ```

### 📝 Cenários Disponíveis

#### 1️⃣ **Cadastrar Limite PIX**
```http
### Cadastrar cliente com limite PIX
POST {{baseUrl}}/api/v{{apiVersion}}/Customer
Content-Type: application/json

{
  "documentNumber": "08517601041",
  "name": "João Silva Santos",
  "email": "joao.silva@email.com",
  "agency": 1,
  "accountNumber": 123456,
  "limitAmount": 5000.00,
  "limitDescription": "Limite PIX - Pessoa Física"
}
```

#### 2️⃣ **Buscar Informações de Limite**
```http
### Buscar cliente por ID
GET {{baseUrl}}/api/v{{apiVersion}}/Customer/{{customerId}}

### Buscar todos os clientes (paginado)
GET {{baseUrl}}/api/v{{apiVersion}}/Customer?pageSize=10
```

#### 3️⃣ **Alterar Limite PIX**
```http
### Aumentar limite PIX
POST {{baseUrl}}/api/v{{apiVersion}}/Limit/{{customerId}}
Content-Type: application/json

{
  "amount": 2000.00,
  "description": "Aumento de limite PIX - Solicitação do cliente"
}
```

#### 4️⃣ **Remover Registro de Limite**
```http
### Remover cliente/limite (soft delete)
DELETE {{baseUrl}}/api/v{{apiVersion}}/Customer/{{customerId}}
```

#### 5️⃣ **Processar Transações PIX**
```http
### Transação PIX - Verificação de limite
POST {{baseUrl}}/api/v{{apiVersion}}/Customer/execute-transaction
Content-Type: application/json

{
  "merchantDocument": "08517601041",
  "value": 1000.00
}
```

### 🎯 Fluxo de Teste Recomendado

1. **Criar um cliente** usando o primeiro endpoint
2. **Copiar o ID** retornado e atualizar a variável `@customerId`
3. **Buscar o cliente** para verificar os dados
4. **Executar transações PIX** para testar a validação de limites
5. **Ajustar limites** conforme necessário
6. **Testar cenários de erro** (valores inválidos, documentos inexistentes)

## 📖 API Documentation

### Endpoints Principais

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/v1/Customer` | Cadastra novo cliente com limite |
| `GET` | `/api/v1/Customer/{id}` | Busca cliente por ID |
| `GET` | `/api/v1/Customer` | Lista clientes (paginado) |
| `POST` | `/api/v1/Limit/{id}` | Ajusta limite de cliente |
| `DELETE` | `/api/v1/Customer/{id}` | Remove cliente (soft delete) |
| `POST` | `/api/v1/Customer/execute-transaction` | Processa transação PIX |

### Códigos de Status

- `200 OK` - Operação realizada com sucesso
- `201 Created` - Cliente criado com sucesso
- `204 No Content` - Cliente removido com sucesso
- `400 Bad Request` - Dados inválidos
- `404 Not Found` - Cliente não encontrado
- `409 Conflict` - Cliente já existe

### Swagger UI

Acesse a documentação interativa da API em:
- **Local**: http://localhost:5261/swagger
- **Docker**: http://localhost:5000/swagger

## 🧪 Testes

### Executar Todos os Testes

```bash
dotnet test
```

### Executar Testes por Projeto

```bash
# Testes de Domínio
dotnet test tests/KRTBanking.Domain.Tests

# Testes de Aplicação
dotnet test tests/KRTBanking.Application.Tests

# Testes de Infraestrutura
dotnet test tests/KRTBanking.Infrastructure.Tests

# Testes da API
dotnet test tests/KRTBanking.API.Tests
```

### Cobertura de Testes

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📁 Estrutura do Projeto

```
KRTBanking/
├── src/
│   ├── KRTBanking.API/              # Controllers e configuração da API
│   ├── KRTBanking.Application/      # Services e DTOs
│   ├── KRTBanking.Domain/           # Entidades e regras de negócio
│   └── KRTBanking.Infrastructure/   # Repositórios e persistência
├── tests/
│   ├── KRTBanking.API.Tests/        # Testes da API
│   ├── KRTBanking.Application.Tests/ # Testes dos Services
│   ├── KRTBanking.Domain.Tests/     # Testes do Domínio
│   └── KRTBanking.Infrastructure.Tests/ # Testes da Infraestrutura
├── docs/
│   └── ARCHITECTURE.md             # Documentação da arquitetura
├── analista-fraude-krt.http        # Cenários de teste HTTP
├── docker-compose.yaml             # Configuração Docker
└── README.md                       # Este arquivo
```

## 🔧 Configuração

### Variáveis de Ambiente

- `ASPNETCORE_ENVIRONMENT`: Ambiente de execução (Development/Production)
- `AwsConfig__ServiceURL`: URL do DynamoDB (local ou AWS)
- `AwsConfig__AccessKey`: Chave de acesso AWS
- `AwsConfig__SecretKey`: Chave secreta AWS

### DynamoDB Local

O projeto está configurado para usar DynamoDB Local para desenvolvimento:
- **Endpoint**: http://localhost:8000
- **Admin Interface**: http://localhost:8001
- **Tabelas**: Criadas automaticamente na primeira execução

## 📝 Licença

Este projeto está licenciado sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

**Desenvolvido com ❤️ por giomartinsdev :D**
