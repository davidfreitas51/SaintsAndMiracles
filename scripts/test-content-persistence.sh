#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

ENV_FILE="${1:-.env.development}"
COMPOSE="docker compose --env-file $ENV_FILE"

wait_for_api_running() {
  local retries=30
  local sleep_seconds=2
  local api_id

  api_id="$($COMPOSE ps -q api)"
  if [[ -z "$api_id" ]]; then
    echo "ERROR: API container not found."
    return 1
  fi

  for ((i=1; i<=retries; i++)); do
    local state
    state="$(docker inspect -f '{{.State.Status}}' "$api_id")"
    if [[ "$state" == "running" ]]; then
      echo "API is running."
      return 0
    fi
    sleep "$sleep_seconds"
  done

  echo "ERROR: API did not reach running state in time."
  return 1
}

resolve_prayers_mount_source() {
  local api_id
  api_id="$($COMPOSE ps -q api)"
  docker inspect -f '{{range .Mounts}}{{if eq .Destination "/app/wwwroot/prayers"}}{{.Source}}{{end}}{{end}}' "$api_id"
}

echo "Starting stack using $ENV_FILE ..."
$COMPOSE up -d --build
wait_for_api_running

prayers_source="$(resolve_prayers_mount_source)"
if [[ -z "$prayers_source" ]]; then
  echo "ERROR: No mount found for /app/wwwroot/prayers."
  echo "Expected bind mount is missing. Check docker-compose volumes for api service."
  exit 1
fi

echo "Detected prayers mount source: $prayers_source"

# Ensure the directory exists
if [[ ! -d "$prayers_source" ]]; then
  echo "Mount source directory does not exist: $prayers_source. Creating it..."
  mkdir -p "$prayers_source" || {
    echo "ERROR: Failed to create mount source directory."
    exit 1
  }
fi

sentinel_slug="persistence-smoke-$(date +%s)"
host_dir="$prayers_source/$sentinel_slug"
host_file="$host_dir/markdown.md"
container_file="/app/wwwroot/prayers/$sentinel_slug/markdown.md"

# Try to create the test directory and file on the host
# If that fails due to permissions, create it from within the container
if ! mkdir -p "$host_dir" 2>/dev/null; then
  echo "Cannot create directory on host ($host_dir) - attempting from container..."
  api_id_before="$($COMPOSE ps -q api)"
  docker exec "$api_id_before" mkdir -p "$(dirname "$container_file")" || {
    echo "ERROR: Failed to create directory from within the container."
    exit 1
  }
else
  # Directory created on host successfully, write the file
  cat > "$host_file" <<'EOF'
# Persistence Smoke Test

This file verifies that prayer content survives API container recreation.
EOF
fi

# If file wasn't created on host, create it from the container
if [[ ! -f "$host_file" ]]; then
  echo "Creating sentinel file from within container..."
  api_id_before="$($COMPOSE ps -q api)"
  docker exec "$api_id_before" bash -c "cat > '$container_file' <<'EOFTEST'
# Persistence Smoke Test

This file verifies that prayer content survives API container recreation.
EOFTEST" || {
    echo "ERROR: Failed to create file within the container."
    exit 1
  }
fi

echo "Checking sentinel file is visible inside current API container ..."
api_id_before="$($COMPOSE ps -q api)"
docker exec "$api_id_before" test -f "$container_file"

echo "Recreating API container ..."
$COMPOSE up -d --force-recreate api
wait_for_api_running

api_id_after="$($COMPOSE ps -q api)"

# Verify the file still exists in the container after recreate
docker exec "$api_id_after" test -f "$container_file" || {
  echo "ERROR: Sentinel file missing from container after API recreate."
  exit 1
}

# Check if file exists on host (may not be accessible if created in container)
if [[ -f "$host_file" ]]; then
  echo "Host file verified: $host_file"
elif [[ -d "$host_dir" ]]; then
  echo "Host directory exists ($host_dir) but file may be inaccessible due to permissions."
else
  echo "WARNING: Host files may not be accessible, but persistence verified in container."
fi

echo "PASS: content persistence verified."
echo "Container file: $container_file"

echo "Cleaning up sentinel folder ..."
rm -rf "$host_dir" 2>/dev/null || {
  echo "Note: Could not remove host directory (may lack permissions), but test passed."
}

echo "Done."
