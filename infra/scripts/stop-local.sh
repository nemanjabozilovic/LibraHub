#!/bin/bash

# Bash script to stop all LibraHub services

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"

echo "Stopping all LibraHub services..."

# Stop all service APIs
echo ""
echo "Stopping service APIs..."

SERVICES=(
    "services/Gateway"
    "services/Notifications"
    "services/Library"
    "services/Orders"
    "services/Content"
    "services/Catalog"
    "services/Identity"
)

for SERVICE in "${SERVICES[@]}"; do
    SERVICE_NAME=$(basename "$SERVICE")
    echo "Stopping $SERVICE_NAME service..."
    cd "$ROOT_DIR/$SERVICE"
    docker-compose down
done

# Stop infrastructure services
echo ""
echo "Stopping infrastructure services..."
cd "$ROOT_DIR"
docker-compose down

echo ""
echo "All services stopped successfully!"

