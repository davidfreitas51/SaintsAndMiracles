#!/usr/bin/env bash
set -Eeuo pipefail

ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

ENV_FILE="$ROOT/.env.dev"

if [[ ! -f "$ENV_FILE" ]]; then
    echo "ERRO: .env.dev não existe. Execute scripts/setup.sh."
    exit 1
fi

# Carrega as variáveis apenas para este script e seus processos filhos.
set -a
# shellcheck disable=SC1090
source "$ENV_FILE"
set +a

: "${DB_CONTAINER:?Configure DB_CONTAINER em .env.dev}"
: "${DB_IMAGE:?Configure DB_IMAGE em .env.dev}"
: "${DB_PORT:?Configure DB_PORT em .env.dev}"
: "${DB_NAME:?Configure DB_NAME em .env.dev}"
: "${DB_USER:?Configure DB_USER em .env.dev}"
: "${DB_PASSWORD:?Configure DB_PASSWORD em .env.dev}"
: "${DB_ROOT_PASSWORD:?Configure DB_ROOT_PASSWORD em .env.dev}"
: "${API_PORT:?Configure API_PORT em .env.dev}"
: "${FRONTEND_PORT:?Configure FRONTEND_PORT em .env.dev}"
: "${API_HOST:?Configure API_HOST em .env.dev}"
: "${FRONTEND_HOST:?Configure FRONTEND_HOST em .env.dev}"
: "${DEV_STATE_DIR:?Configure DEV_STATE_DIR em .env.dev}"
: "${ASPNETCORE_ENVIRONMENT:?Configure ASPNETCORE_ENVIRONMENT em .env.dev}"

for cmd in docker dotnet npm curl fuser; do
    command -v "$cmd" >/dev/null 2>&1 || {
        echo "ERRO: comando necessário não encontrado: $cmd"
        exit 1
    }
done

if ! docker info >/dev/null 2>&1; then
    echo "ERRO: Docker indisponível ou sem permissão."
    exit 1
fi

if [[ "$DB_PASSWORD" == CHANGE_ME* ||
      "$DB_ROOT_PASSWORD" == CHANGE_ME* ]]; then
    echo "ERRO: configure as senhas em .env.dev."
    exit 1
fi

mkdir -p "$DEV_STATE_DIR"
STATE_DIR="$ROOT/$DEV_STATE_DIR"
mkdir -p "$STATE_DIR"

# Evita iniciar duas cópias deste script simultaneamente.
exec 9>"$STATE_DIR/startdev.lock"
if ! flock -n 9; then
    echo "ERRO: startdev.sh já está em execução."
    exit 1
fi

# Descobre o projeto .NET sem exigir um caminho fixo.
API_PROJECT="Server/API/API.csproj"

if [[ ! -f "$API_PROJECT" ]]; then
    echo "ERRO: projeto da API não encontrado: $API_PROJECT"
    exit 1
fi

[[ -f Client/package.json ]] || {
    echo "ERRO: Client/package.json não encontrado."
    exit 1
}

# Libera somente portas reservadas para este ambiente.
# Não usa sudo: se o processo pertencer a outro usuário, falha.
stop_port() {
    local port="$1"
    local pids

    pids="$(fuser -n tcp "$port" 2>/dev/null || true)"

    if [[ -n "$pids" ]]; then
        echo "Encerrando processo(s) na porta TCP $port: $pids"
        # Portas configuradas para uso exclusivo deste ambiente.
        fuser -k -TERM -n tcp "$port" 2>/dev/null || true

        for _ in {1..30}; do
            fuser -n tcp "$port" >/dev/null 2>&1 || break
            sleep 0.2
        done

        if fuser -n tcp "$port" >/dev/null 2>&1; then
            echo "ERRO: a porta $port continua ocupada."
            return 1
        fi
    fi
}

echo "==> Verificando MySQL"

DEV_VOLUME="saints_mysql_dev_data"

# Se o container existir, valida se a porta publicada corresponde à configuração.
RECREATE=false

if docker container inspect "$DB_CONTAINER" >/dev/null 2>&1; then
    CURRENT_PORT="$(docker inspect \
        --format '{{with (index .NetworkSettings.Ports "3306/tcp")}}{{(index . 0).HostPort}}{{end}}' \
        "$DB_CONTAINER" 2>/dev/null || true)"

    if [[ "$CURRENT_PORT" != "$DB_PORT" ]]; then
        echo "Container existente com porta incorreta; recriando somente o container de desenvolvimento."
        RECREATE=true
    fi
else
    RECREATE=true
fi

if [[ "$RECREATE" == true ]]; then
    if docker container inspect "$DB_CONTAINER" >/dev/null 2>&1; then
        docker rm -f "$DB_CONTAINER" >/dev/null
    fi

    docker run -d \
        --name "$DB_CONTAINER" \
        --restart unless-stopped \
        -p "127.0.0.1:${DB_PORT}:3306" \
        -e MYSQL_DATABASE="$DB_NAME" \
        -e MYSQL_USER="$DB_USER" \
        -e MYSQL_PASSWORD="$DB_PASSWORD" \
        -e MYSQL_ROOT_PASSWORD="$DB_ROOT_PASSWORD" \
        -v "$DEV_VOLUME:/var/lib/mysql" \
        "$DB_IMAGE" >/dev/null
else
    docker start "$DB_CONTAINER" >/dev/null 2>&1 || true
fi

# Aguarda o MySQL ficar pronto.
READY=false
for _ in {1..60}; do
    if docker exec "$DB_CONTAINER" \
        mysqladmin ping -h 127.0.0.1 \
        -uroot "-p${DB_ROOT_PASSWORD}" --silent >/dev/null 2>&1; then
        READY=true
        break
    fi
    sleep 1
done

if [[ "$READY" != true ]]; then
    echo "ERRO: MySQL não ficou disponível ou a senha root não corresponde."
    echo "Logs: docker logs $DB_CONTAINER"
    exit 1
fi

# Confirma que o usuário e o banco configurados podem conectar.
if ! docker exec "$DB_CONTAINER" \
    mysql -h 127.0.0.1 \
    -u"$DB_USER" "-p${DB_PASSWORD}" "$DB_NAME" \
    -e 'SELECT 1' >/dev/null 2>&1; then

    echo "Credenciais incompatíveis. Recriando banco de desenvolvimento..."

    # Segurança: só permite remover o volume dedicado ao desenvolvimento.
    DEV_VOLUME="saints_mysql_dev_data"

    USERS="$(docker ps -a --filter "volume=$DEV_VOLUME" \
        --format '{{.Names}}')"

    if [[ "$USERS" != "$DB_CONTAINER" ]]; then
        echo "ERRO: volume utilizado por outro container. Nada foi apagado."
        exit 1
    fi

    docker rm -f "$DB_CONTAINER" >/dev/null
    docker volume rm "$DEV_VOLUME" >/dev/null

    docker run -d \
        --name "$DB_CONTAINER" \
        --restart unless-stopped \
        -p "127.0.0.1:${DB_PORT}:3306" \
        -e MYSQL_DATABASE="$DB_NAME" \
        -e MYSQL_USER="$DB_USER" \
        -e MYSQL_PASSWORD="$DB_PASSWORD" \
        -e MYSQL_ROOT_PASSWORD="$DB_ROOT_PASSWORD" \
        -v "$DEV_VOLUME:/var/lib/mysql" \
        "$DB_IMAGE" >/dev/null

    READY=false
    for _ in {1..60}; do
        if docker exec "$DB_CONTAINER" \
            mysqladmin ping -h 127.0.0.1 \
            -uroot "-p${DB_ROOT_PASSWORD}" --silent \
            >/dev/null 2>&1; then
            READY=true
            break
        fi
        sleep 1
    done

    if [[ "$READY" != true ]]; then
        echo "ERRO: MySQL não iniciou após recriar o banco."
        exit 1
    fi

    if ! docker exec "$DB_CONTAINER" \
        mysql -h 127.0.0.1 \
        -u"$DB_USER" "-p${DB_PASSWORD}" "$DB_NAME" \
        -e 'SELECT 1' >/dev/null 2>&1; then
        echo "ERRO: não foi possível autenticar no banco recriado."
        exit 1
    fi
fi

echo "==> Verificando portas"

stop_port "$API_PORT"
stop_port "$FRONTEND_PORT"

# A connection string é composta a partir das variáveis centralizadas.
export ConnectionStrings__DefaultConnection="Server=127.0.0.1;Port=${DB_PORT};Database=${DB_NAME};User=${DB_USER};Password=${DB_PASSWORD};"

API_LOG="$STATE_DIR/api.log"
FRONTEND_LOG="$STATE_DIR/frontend.log"

echo "==> Iniciando API"
nohup dotnet run --project "$API_PROJECT" \
    --urls "http://${API_HOST}:${API_PORT}" \
    >"$API_LOG" 2>&1 </dev/null 9>&- &
echo "$!" > "$STATE_DIR/api.pid"

echo "==> Iniciando Angular"
nohup npm --prefix Client run start -- \
    --host "$FRONTEND_HOST" --port "$FRONTEND_PORT" \
    >"$FRONTEND_LOG" 2>&1 </dev/null 9>&- &
echo "$!" > "$STATE_DIR/frontend.pid"

echo
echo "Ambiente iniciado."
echo "Angular: http://localhost:${FRONTEND_PORT}"
echo "API:     http://localhost:${API_PORT}"
echo "Logs:    $STATE_DIR/"
echo
echo "Para acompanhar:"
echo "tail -f '$FRONTEND_LOG' '$API_LOG'"
echo
echo "Os logs devem ser conferidos para confirmar que os serviços subiram."
