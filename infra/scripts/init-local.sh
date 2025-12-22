#!/bin/bash

# Bash script to initialize local development environment

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
echo "Starting infrastructure services (PostgreSQL, RabbitMQ, Redis)..."
cd "$(dirname "$0")/../docker"
docker-compose up -d

echo "Waiting for services to be ready..."
sleep 10

# Check service health
echo ""
echo "Checking service health..."
if docker exec librahub-postgres pg_isready -U postgres > /dev/null 2>&1; then
    echo "PostgreSQL is ready"
else
    echo "PostgreSQL is not ready yet"
fi

# Run database migrations
echo ""
echo "Running database migrations..."
cd ../../../services/Identity/src/LibraHub.Identity.Infrastructure
dotnet ef database update --project ../LibraHub.Identity.Infrastructure.csproj --startup-project ../../LibraHub.Identity.Api/LibraHub.Identity.Api.csproj

echo ""
echo "Initialization complete!"
echo ""
echo "Services available at:"
echo "  - PostgreSQL: localhost:5432"
echo "  - RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo "  - Redis: localhost:6379"
echo ""
echo "To start the Identity service, run:"
echo "  cd services/Identity/src/LibraHub.Identity.Api"
echo "  dotnet run"

