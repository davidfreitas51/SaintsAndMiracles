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

# Gitleaks é uma dependência do sistema operacional.
if ! command -v gitleaks >/dev/null 2>&1; then
    echo "ERRO: Gitleaks não está instalado."
    echo "Instale o Gitleaks: https://github.com/gitleaks/gitleaks"
    exit 1
fi

if [[ ! -f .env.dev ]]; then
    if [[ ! -f .env.example ]]; then
        echo "ERRO: .env.example não encontrado."
        exit 1
    fi

    cp .env.example .env.dev
    chmod 600 .env.dev
    echo "Criei .env.dev. Configure suas variáveis antes de continuar."
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

echo "==> Configurando Husky"

if [[ -d Client/.git ]]; then
    echo "ERRO: Husky está em um subdiretório de um repositório diferente."
    exit 1
fi

if [[ -f Client/node_modules/husky/bin.js ]]; then
    (
        cd Client
        npx husky
    )
else
    echo "ERRO: Husky não está instalado em Client."
    echo "Execute: npm --prefix Client install --save-dev husky"
    exit 1
fi

echo "==> Verificando configuração do Gitleaks"

if [[ ! -f .gitleaks.toml ]]; then
    echo "AVISO: .gitleaks.toml não encontrado."
fi

echo
echo "Setup concluído."
echo "Confira o .env.dev e execute scripts/startdev.sh."