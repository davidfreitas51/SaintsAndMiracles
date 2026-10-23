#!/usr/bin/env bash
set -Eeuo pipefail

ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

echo "==> Verificando dependências"

for cmd in git docker dotnet node npm; do
    command -v "$cmd" >/dev/null 2>&1 || {
        echo "ERRO: '$cmd' não está instalado."
        exit 1
    }
done

if [[ ! -f .env.dev ]]; then
    if [[ ! -f .env.example ]]; then
        echo "ERRO: .env.example não encontrado."
        exit 1
    fi

    cp .env.example .env.dev
    chmod 600 .env.dev
    echo "Crie suas configurações em .env.dev antes de continuar."
    exit 0
fi

if ! docker info >/dev/null 2>&1; then
    echo "ERRO: Docker indisponível ou sem permissão."
    exit 1
fi

if [[ ! -f Client/package.json ]]; then
    echo "ERRO: Client/package.json não encontrado."
    exit 1
fi

if [[ ! -f Client/package-lock.json ]]; then
    echo "ERRO: Client/package-lock.json não encontrado."
    exit 1
fi

API_PROJECT="Server/API/API.csproj"

if [[ ! -f "$API_PROJECT" ]]; then
    echo "ERRO: projeto da API não encontrado: $API_PROJECT"
    exit 1
fi

echo "==> Restaurando dependências .NET"
dotnet restore "$API_PROJECT"

echo "==> Instalando dependências Angular"
npm --prefix Client ci

echo "Setup concluído."
echo "Confira o .env.dev e execute scripts/startdev.sh."