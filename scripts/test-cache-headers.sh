#!/usr/bin/env bash
set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

API_URL="${API_URL:-http://localhost:5215}"
API_PROCESS_PID=""
CLEANUP_REQUIRED=false

# Cleanup function
cleanup() {
  if [ "$CLEANUP_REQUIRED" = true ] && [ -n "$API_PROCESS_PID" ]; then
    echo -e "${YELLOW}Stopping API (PID: $API_PROCESS_PID)...${NC}"
    kill "$API_PROCESS_PID" 2>/dev/null || true
    wait "$API_PROCESS_PID" 2>/dev/null || true
  fi
}

trap cleanup EXIT

echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}   SPA Cache Headers Test${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo ""

# Step 1: Build Angular app
echo -e "${YELLOW}Step 1: Building Angular app in production mode...${NC}"
cd "$ROOT_DIR/Client"
npm run build

cd "$ROOT_DIR"
echo -e "${GREEN}✓ Angular build complete${NC}"
echo ""

# Step 2: Check if wwwroot exists and has expected files
echo -e "${YELLOW}Step 2: Verifying wwwroot build output...${NC}"
WWWROOT_PATH="$ROOT_DIR/Server/API/wwwroot"
if [ ! -d "$WWWROOT_PATH" ]; then
  echo -e "${RED}✗ wwwroot directory not found at $WWWROOT_PATH${NC}"
  exit 1
fi

if [ ! -f "$WWWROOT_PATH/index.html" ]; then
  echo -e "${RED}✗ index.html not found in wwwroot${NC}"
  exit 1
fi

# Check for hashed bundles
HASHED_BUNDLE=$(find "$WWWROOT_PATH" -maxdepth 1 -name 'main-*.js' | head -n 1)
if [ -z "$HASHED_BUNDLE" ]; then
  echo -e "${RED}✗ No hashed bundles found (expected main-HASH.js)${NC}"
  exit 1
fi

echo -e "${GREEN}✓ wwwroot structure valid${NC}"
echo "  - index.html: $(ls -lh "$WWWROOT_PATH/index.html" | awk '{print $5}')"
echo "  - sample hashed bundle: $(basename "$HASHED_BUNDLE") ($(ls -lh "$HASHED_BUNDLE" | awk '{print $5}'))"
echo ""

# Step 3: Check if API is already running
echo -e "${YELLOW}Step 3: Starting API server...${NC}"
if curl -s "$API_URL/api/saints" > /dev/null 2>&1; then
  echo -e "${GREEN}✓ API already running at $API_URL${NC}"
  CLEANUP_REQUIRED=false
else
  echo "Starting API with: dotnet run --project Server/API/API.csproj"
  cd "$ROOT_DIR"
  dotnet run --project Server/API/API.csproj &
  API_PROCESS_PID=$!
  CLEANUP_REQUIRED=true
  
  # Wait for API to be ready (max 30 seconds)
  echo "Waiting for API to start..."
  RETRIES=30
  RETRY_COUNT=0
  while ! curl -s "$API_URL/api/saints" > /dev/null 2>&1; do
    RETRY_COUNT=$((RETRY_COUNT + 1))
    if [ $RETRY_COUNT -gt $RETRIES ]; then
      echo -e "${RED}✗ API failed to start after ${RETRIES}s${NC}"
      exit 1
    fi
    echo -n "."
    sleep 1
  done
  echo ""
  echo -e "${GREEN}✓ API ready${NC}"
fi
echo ""

# Step 4: Test cache headers
echo -e "${YELLOW}Step 4: Testing cache headers...${NC}"
echo ""

# Test root/index.html
echo -e "${BLUE}Testing: GET / (index.html)${NC}"
RESPONSE=$(curl -sI "$API_URL/" 2>&1)
CACHE_CONTROL=$(echo "$RESPONSE" | grep -i '^cache-control:' | cut -d' ' -f2- | tr -d '\r')
PRAGMA=$(echo "$RESPONSE" | grep -i '^pragma:' | cut -d' ' -f2- | tr -d '\r')
EXPIRES=$(echo "$RESPONSE" | grep -i '^expires:' | cut -d' ' -f2- | tr -d '\r')

echo "  Cache-Control: $CACHE_CONTROL"
echo "  Pragma: ${PRAGMA:-(not set)}"
echo "  Expires: ${EXPIRES:-(not set)}"

# Validate index.html cache headers
INDEX_VALID=true
if echo "$CACHE_CONTROL" | grep -q -i "no-cache\|no-store\|must-revalidate"; then
  echo -e "  ${GREEN}✓ index.html has no-cache policy${NC}"
else
  echo -e "  ${RED}✗ index.html should have 'no-cache' or 'no-store'${NC}"
  INDEX_VALID=false
fi

echo ""

# Extract a hashed bundle name from the actual index.html
echo -e "${BLUE}Testing: Hashed bundles${NC}"
HASHED_BUNDLE_NAME=$(curl -s "$API_URL/" | grep -oE '(main|polyfills|styles|chunk)-[A-Za-z0-9]+\.(js|css)' | head -n 1)

if [ -z "$HASHED_BUNDLE_NAME" ]; then
  echo -e "  ${RED}✗ Could not extract hashed bundle name from index.html${NC}"
  BUNDLE_VALID=false
else
  echo "  Found hashed bundle: $HASHED_BUNDLE_NAME"
  
  RESPONSE=$(curl -sI "$API_URL/$HASHED_BUNDLE_NAME" 2>&1)
  CACHE_CONTROL=$(echo "$RESPONSE" | grep -i '^cache-control:' | cut -d' ' -f2- | tr -d '\r')
  
  echo "  Cache-Control: $CACHE_CONTROL"
  
  # Validate hashed bundle cache headers
  BUNDLE_VALID=true
  if echo "$CACHE_CONTROL" | grep -q "public"; then
    echo -e "  ${GREEN}✓ Hashed bundle has 'public'${NC}"
  else
    echo -e "  ${RED}✗ Hashed bundle should have 'public'${NC}"
    BUNDLE_VALID=false
  fi
  
  if echo "$CACHE_CONTROL" | grep -q "immutable"; then
    echo -e "  ${GREEN}✓ Hashed bundle has 'immutable'${NC}"
  else
    echo -e "  ${RED}✗ Hashed bundle should have 'immutable'${NC}"
    BUNDLE_VALID=false
  fi
  
  if echo "$CACHE_CONTROL" | grep -qE "max-age=[0-9]{7,}"; then
    echo -e "  ${GREEN}✓ Hashed bundle has long max-age (1 year)${NC}"
  else
    echo -e "  ${YELLOW}⚠ Hashed bundle max-age seems short (expected ~31536000)${NC}"
  fi
fi

echo ""

# Step 5: Summary
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}   Test Summary${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"

if [ "$INDEX_VALID" = true ] && [ "$BUNDLE_VALID" = true ]; then
  echo -e "${GREEN}✓ All cache header tests PASSED${NC}"
  echo ""
  echo "Expected behavior:"
  echo "  1. Browsers will always revalidate index.html (no hard refresh needed)"
  echo "  2. After deploy, users see new bundles on normal F5 refresh"
  echo "  3. Hashed bundles are cached aggressively for 1 year"
  exit 0
else
  echo -e "${RED}✗ Some cache header tests FAILED${NC}"
  echo ""
  if [ "$INDEX_VALID" = false ]; then
    echo "  - index.html cache policy is incorrect"
  fi
  if [ "$BUNDLE_VALID" = false ]; then
    echo "  - Hashed bundle cache policy is incorrect"
  fi
  exit 1
fi
