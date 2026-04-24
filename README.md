# EmailSenderApi

API REST em ASP.NET Core para disparo automático de e-mails com suporte a templates personalizados por destinatário, fila de envio com retry automático e agendamento.

## Tecnologias

- .NET 8
- ASP.NET Core Web API
- MailKit / MimeKit (envio SMTP)
- Hangfire (fila, agendamento e retry)
- Swagger / OpenAPI (documentação)

## Estrutura do Projeto

```
EmailSenderApi/
├── Controllers/
│   ├── EmailController.cs         # POST /api/email/send (envio imediato)
│   ├── EmailScheduleController.cs # POST /api/email/queue (fila + agendamento)
│   └── TemplateController.cs      # CRUD de templates
├── Jobs/
│   └── EmailSendJob.cs            # Job do Hangfire com retry automático
├── Models/
│   ├── EmailRequest.cs            # Requisição de envio imediato
│   ├── EmailResult.cs             # Resposta com detalhe por destinatário
│   ├── EmailTemplate.cs           # Template com suporte a variáveis
│   ├── EmailJobPayload.cs         # Payload serializado pelo Hangfire
│   ├── Recipient.cs               # Destinatário simples
│   ├── RecipientWithVariables.cs  # Destinatário com variáveis de template
│   ├── TemplateEmailRequest.cs    # Requisição de envio via template
│   └── QueueResponse.cs          # Resposta do enfileiramento
├── Services/
│   ├── IEmailService.cs / EmailService.cs         # Envio via SMTP
│   ├── ITemplateService.cs / TemplateService.cs   # Gerenciamento de templates
│   └── IEmailQueueService.cs / EmailQueueService.cs # Fila via Hangfire
├── Settings/
│   └── SmtpSettings.cs            # Configurações tipadas do SMTP
├── Program.cs                     # Configuração da aplicação
└── appsettings.json               # Configurações do ambiente
```

## Configuração

Edite o arquivo `appsettings.json` com as credenciais do seu servidor SMTP:

```json
{
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "seu-email@gmail.com",
    "Password": "sua-senha-de-app",
    "SenderName": "Email Sender API",
    "SenderEmail": "seu-email@gmail.com"
  }
}
```

### Gmail

Para usar com o Gmail, é necessário gerar uma **Senha de App**:

1. Acesse sua Conta Google
2. Vá em **Segurança > Verificação em duas etapas**
3. Role até **Senhas de app** e gere uma nova
4. Use essa senha no campo `Password` do `appsettings.json`

### Outros provedores

| Provedor | Host | Porta |
|----------|------|-------|
| Gmail | smtp.gmail.com | 587 |
| Outlook / Hotmail | smtp.office365.com | 587 |
| Yahoo | smtp.mail.yahoo.com | 587 |

## Como Executar

### Pré-requisitos

| Ferramenta | Uso |
|------------|-----|
| [Docker](https://docs.docker.com/get-docker/) + [Docker Compose](https://docs.docker.com/compose/) | Rodar a stack completa |
| [just](https://github.com/casey/just) | Executar as receitas de automação |
| [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | Desenvolvimento local sem Docker |

Instalar o `just` no Windows:
```bash
winget install Casey.Just
# ou
choco install just
```

### Receitas disponíveis (`just`)

```
just setup        # Configuração inicial (copia appsettings + restaura pacotes)
just up           # Sobe toda a stack com Docker (API + MailHog)
just down         # Para e remove os containers
just rebuild      # Reconstrói e reinicia apenas a API
just logs         # Logs em tempo real de todos os serviços
just logs-api     # Logs apenas da API
just run          # Executa localmente com hot-reload (sem Docker)
just build        # Compila o projeto
just clean        # Remove bin/ e obj/
just ps           # Status dos containers
```

### Com Docker (recomendado)

Suba toda a stack com um único comando a partir da raiz do repositório:

```bash
just setup   # apenas na primeira vez
just up
```

| URL | Descrição |
|-----|-----------|
| `http://localhost:8080/swagger` | Documentação Swagger |
| `http://localhost:8080/hangfire` | Dashboard da fila de jobs |
| `http://localhost:8025` | MailHog — visualize os e-mails enviados |

O **MailHog** funciona como servidor SMTP local: intercepta todos os e-mails e os exibe na interface web, sem entregar para destinatários reais. Ideal para desenvolvimento e testes.

Para parar:
```bash
just down
```

### Sem Docker (local)

```bash
just setup   # apenas na primeira vez
just run
```

| URL | Descrição |
|-----|-----------|
| `http://localhost:5000/swagger` | Documentação Swagger |
| `http://localhost:5000/hangfire` | Dashboard da fila de jobs |

> **Produção:** substitua o `UseInMemoryStorage` por `Hangfire.SqlServer` ou `Hangfire.LiteDB` em `Program.cs` para persistir os jobs entre reinicializações.

---

## Endpoints

### `POST /api/email/send`

Envia e-mail imediatamente para uma lista de destinatários (sem template, sem fila).

**Request:**
```json
{
  "subject": "Assunto",
  "body": "Corpo do e-mail",
  "isHtml": false,
  "recipients": [
    { "name": "João Silva", "email": "joao@exemplo.com" },
    { "name": "Maria Santos", "email": "maria@exemplo.com" }
  ]
}
```

**Response `200`:**
```json
{
  "success": true,
  "totalSent": 2,
  "totalFailed": 0,
  "details": [
    { "email": "joao@exemplo.com", "sent": true, "error": null },
    { "email": "maria@exemplo.com", "sent": true, "error": null }
  ]
}
```

---

### `POST /api/template`

Cria um template de e-mail com variáveis no formato `{{NomeDaVariavel}}`.

**Request:**
```json
{
  "name": "Boas-vindas",
  "subject": "Bem-vindo, {{Nome}}!",
  "body": "<h1>Olá, {{Nome}}!</h1><p>Sua empresa <b>{{Empresa}}</b> está cadastrada.</p>",
  "isHtml": true
}
```

**Response `201`:**
```json
{
  "id": "a1b2c3d4",
  "name": "Boas-vindas",
  "subject": "Bem-vindo, {{Nome}}!",
  "body": "<h1>Olá, {{Nome}}!</h1>...",
  "isHtml": true,
  "createdAt": "2026-04-24T10:00:00Z"
}
```

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/template` | Lista todos os templates |
| `GET` | `/api/template/{id}` | Busca um template por ID |
| `POST` | `/api/template` | Cria um novo template |
| `DELETE` | `/api/template/{id}` | Remove um template |

---

### `POST /api/email/queue`

Enfileira o envio usando um template, com variáveis personalizadas por destinatário.
O corpo e assunto são renderizados individualmente antes de entrar na fila.

- **Sem `scheduledAt`** → enfileira para execução imediata em background
- **Com `scheduledAt`** → agenda para a data/hora informada (UTC)
- **Retry automático**: em caso de falha total, reexecuta após **5min**, **15min** e **30min**

**Request — envio imediato:**
```json
{
  "templateId": "a1b2c3d4",
  "recipients": [
    {
      "name": "João Silva",
      "email": "joao@exemplo.com",
      "variables": {
        "{{Nome}}": "João",
        "{{Empresa}}": "ACME Ltda"
      }
    },
    {
      "name": "Maria Santos",
      "email": "maria@exemplo.com",
      "variables": {
        "{{Nome}}": "Maria",
        "{{Empresa}}": "Beta Corp"
      }
    }
  ]
}
```

**Request — envio agendado:**
```json
{
  "templateId": "a1b2c3d4",
  "scheduledAt": "2026-04-25T14:30:00Z",
  "recipients": [
    {
      "name": "João Silva",
      "email": "joao@exemplo.com",
      "variables": { "{{Nome}}": "João", "{{Empresa}}": "ACME" }
    }
  ]
}
```

**Response `202`:**
```json
{
  "jobId": "1",
  "status": "enqueued",
  "scheduledAt": null,
  "totalRecipients": 2
}
```

| Campo `status` | Descrição |
|----------------|-----------|
| `enqueued` | Na fila para envio imediato |
| `scheduled` | Agendado para `scheduledAt` |

---

## Retry Automático

O job de envio usa a política de retry do Hangfire:

| Tentativa | Intervalo |
|-----------|-----------|
| 1ª retry | 5 minutos |
| 2ª retry | 15 minutos |
| 3ª retry | 30 minutos |

O retry é acionado **somente quando todos os envios do job falharem**. Falhas parciais são registradas em log mas não reativam o job. Acompanhe o status em `http://localhost:5000/hangfire`.

---

## Exemplo com cURL

```bash
# 1. Criar template
curl -X POST http://localhost:5000/api/template \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Boas-vindas",
    "subject": "Olá, {{Nome}}!",
    "body": "<h1>Bem-vindo, {{Nome}}!</h1><p>Empresa: {{Empresa}}</p>",
    "isHtml": true
  }'

# 2. Enfileirar envio (use o id retornado acima)
curl -X POST http://localhost:5000/api/email/queue \
  -H "Content-Type: application/json" \
  -d '{
    "templateId": "SEU_ID_AQUI",
    "recipients": [
      { "name": "João", "email": "joao@exemplo.com", "variables": { "{{Nome}}": "João", "{{Empresa}}": "ACME" } },
      { "name": "Maria", "email": "maria@exemplo.com", "variables": { "{{Nome}}": "Maria", "{{Empresa}}": "Beta" } }
    ]
  }'
```
