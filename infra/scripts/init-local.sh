#!/bin/bash

# Bash script to initialize local development environment and start all services

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"

echo "Initializing LibraHub local development environment..."

# Check if Docker is running
echo ""
echo "Checking Docker..."
if ! docker ps > /dev/null 2>&1; then
    echo "Docker is not running. Please start Docker."
    exit 1
fi
echo "Docker is running"

# Start infrastructure services
echo ""
echo "Starting infrastructure services (PostgreSQL, RabbitMQ, Redis, pgAdmin)..."
cd "$ROOT_DIR"
docker-compose up -d

echo "Waiting for infrastructure services to be ready..."
sleep 15

# Check service health
echo ""
echo "Checking infrastructure service health..."
if docker exec librahub-postgres pg_isready -U postgres > /dev/null 2>&1; then
    echo "PostgreSQL is ready"
else
    echo "PostgreSQL is not ready yet"
fi

if docker exec librahub-rabbitmq rabbitmq-diagnostics -q ping > /dev/null 2>&1; then
    echo "RabbitMQ is ready"
else
    echo "RabbitMQ is not ready yet"
fi

# Start all service APIs
echo ""
echo "Starting all service APIs..."

SERVICES=(
    "services/Identity"
    "services/Catalog"
    "services/Content"
    "services/Orders"
    "services/Library"
    "services/Notifications"
    "services/Gateway"
)

for SERVICE in "${SERVICES[@]}"; do
    SERVICE_NAME=$(basename "$SERVICE")
    echo ""
    echo "Starting $SERVICE_NAME service..."
    cd "$ROOT_DIR/$SERVICE"
    if docker-compose up -d; then
        echo "$SERVICE_NAME started successfully"
    else
        echo "Failed to start $SERVICE_NAME"
    fi
done

echo ""
echo "Waiting for all services to initialize..."
sleep 10

echo ""
echo "=========================================="
echo "Initialization complete!"
echo "=========================================="
echo ""
echo "Infrastructure services:"
echo "  - PostgreSQL: localhost:5432"
echo "  - RabbitMQ Management: http://localhost:15672 (librahub_mq/R@bb1tMQ_L1br@Hub_2026!S3cur3_P@ss)"
echo "  - Redis: localhost:6379"
echo "  - pgAdmin: http://localhost:5050 (admin@librahub.com/admin)"
echo ""
echo "API Services:"
echo "  - Gateway: http://localhost:5000"
echo "  - Identity: http://localhost:60950"
echo "  - Catalog: http://localhost:60960"
echo "  - Content: http://localhost:60970"
echo "  - Orders: http://localhost:60980"
echo "  - Library: http://localhost:60990"
echo "  - Notifications: http://localhost:61000"
echo ""
echo "To view logs, run:"
echo "  docker-compose logs -f [service-name]"
echo ""
echo "To stop all services, run:"
echo "  ./infra/scripts/stop-local.sh"

