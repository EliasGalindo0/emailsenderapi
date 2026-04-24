project := "EmailSenderApi"
compose  := "docker compose"

# Lista todas as receitas disponíveis
default:
    @just --list

# ── Setup ────────────────────────────────────────────────────────────────────

# Configuração inicial: copia .env e appsettings de exemplo
setup:
    @echo "==> Copiando .env.example → .env..."
    cp -n .env.example .env || true
    @echo "==> Copiando appsettings de exemplo..."
    cp -n {{project}}/appsettings.json {{project}}/appsettings.Local.json || true
    @echo ""
    @echo "Pronto! Edite .env com suas credenciais antes de subir o projeto."
    @echo "Em seguida rode: just up   (Docker) ou: just restore && just run   (local)"

# ── Desenvolvimento local (sem Docker) ───────────────────────────────────────

# Restaura os pacotes NuGet
restore:
    cd {{project}} && dotnet restore

# Compila o projeto
build:
    cd {{project}} && dotnet build -c Release

# Executa localmente (hot-reload)
run:
    cd {{project}} && dotnet watch run

# Remove artefatos de build
clean:
    cd {{project}} && dotnet clean
    rm -rf {{project}}/bin {{project}}/obj

# ── Docker ───────────────────────────────────────────────────────────────────

# Sobe toda a stack (API + MailHog) reconstruindo a imagem
up:
    {{compose}} up --build -d
    @echo ""
    @echo "Stack no ar:"
    @echo "  API      → http://localhost:8080/swagger"
    @echo "  Hangfire → http://localhost:8080/hangfire"
    @echo "  MailHog  → http://localhost:8025"

# Sobe a stack sem reconstruir a imagem
start:
    {{compose}} up -d

# Para e remove os containers
down:
    {{compose}} down

# Para, remove e apaga volumes
down-clean:
    {{compose}} down -v

# Reinicia apenas a API (útil após alterações sem rebuild)
restart:
    {{compose}} restart api

# Reconstrói e reinicia somente a API
rebuild:
    {{compose}} up --build -d api

# ── Observabilidade ──────────────────────────────────────────────────────────

# Exibe logs de todos os serviços em tempo real
logs:
    {{compose}} logs -f

# Exibe logs apenas da API
logs-api:
    {{compose}} logs -f api

# Exibe logs apenas do MailHog
logs-mail:
    {{compose}} logs -f mailhog

# Lista o status dos containers
ps:
    {{compose}} ps

# ── Atalhos de URL ───────────────────────────────────────────────────────────

# Abre o Swagger no navegador padrão
swagger:
    @echo "http://localhost:8080/swagger"

# Abre o dashboard do Hangfire no navegador padrão
hangfire:
    @echo "http://localhost:8080/hangfire"

# Abre o MailHog no navegador padrão
mailhog:
    @echo "http://localhost:8025"
