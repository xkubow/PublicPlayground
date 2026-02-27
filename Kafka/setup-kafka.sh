#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="${1:-$HOME/docker/kafka}"
mkdir -p "$PROJECT_DIR"
cd "$PROJECT_DIR"

# Copy compose file next to the script if needed
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [[ ! -f docker-compose.yml ]]; then
  cp "$SCRIPT_DIR/docker-compose.yml" docker-compose.yml
fi

echo "==> Checking docker..."
if ! command -v docker >/dev/null 2>&1; then
  echo "Docker CLI not found. Install Docker Desktop on Windows and enable WSL integration for this distro."
  exit 1
fi

if ! docker ps >/dev/null 2>&1; then
  echo "Docker daemon not accessible. Ensure Docker Desktop is running and WSL integration is enabled."
  echo "If you see permission denied: sudo usermod -aG docker \$USER && restart WSL."
  exit 1
fi

echo "==> Starting Kafka (docker compose up -d)..."
docker compose up -d

echo "==> Waiting for Kafka to be ready..."
# simple readiness loop
for i in {1..40}; do
  if docker exec -i kafka-kafka-1 kafka-topics --bootstrap-server localhost:9092 --list >/dev/null 2>&1; then
    echo "Kafka is ready."
    break
  fi
  sleep 2
done

echo "==> Containers:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Image}}" | sed -n '1p;/kafka\|zookeeper/p'

echo "==> Done. Compose folder: $PROJECT_DIR"
