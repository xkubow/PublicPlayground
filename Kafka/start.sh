#!/usr/bin/env bash
set -e
cd "$(dirname "$0")"
docker compose up -d
docker ps --format "table {{.Names}}\t{{.Status}}" | sed -n '1p;/kafka\|zookeeper/p'