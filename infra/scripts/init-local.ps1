# PowerShell script to initialize local development environment

Write-Host "Initializing LibraHub local development environment..." -ForegroundColor Green

# Check if Docker is running
Write-Host "`nChecking Docker..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    Write-Host "Docker is running" -ForegroundColor Green
} catch {
    Write-Host "Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

# Start infrastructure services
Write-Host "`nStarting infrastructure services (PostgreSQL, RabbitMQ, Redis)..." -ForegroundColor Yellow
Set-Location "$PSScriptRoot\..\docker"
docker-compose up -d

Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check service health
Write-Host "`nChecking service health..." -ForegroundColor Yellow
$postgresHealthy = docker exec librahub-postgres pg_isready -U postgres 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "PostgreSQL is ready" -ForegroundColor Green
} else {
    Write-Host "PostgreSQL is not ready yet" -ForegroundColor Yellow
}

# Run database migrations
Write-Host "`nRunning database migrations..." -ForegroundColor Yellow
Set-Location "$PSScriptRoot\..\..\..\services\Identity\src\LibraHub.Identity.Infrastructure"
dotnet ef database update --project ..\LibraHub.Identity.Infrastructure.csproj --startup-project ..\..\LibraHub.Identity.Api\LibraHub.Identity.Api.csproj

Write-Host "`nInitialization complete!" -ForegroundColor Green
Write-Host "`nServices available at:" -ForegroundColor Cyan
Write-Host "  - PostgreSQL: localhost:5432" -ForegroundColor Cyan
Write-Host "  - RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor Cyan
Write-Host "  - Redis: localhost:6379" -ForegroundColor Cyan
Write-Host "`nTo start the Identity service, run:" -ForegroundColor Cyan
Write-Host "  cd services\Identity\src\LibraHub.Identity.Api" -ForegroundColor Cyan
Write-Host "  dotnet run" -ForegroundColor Cyan

