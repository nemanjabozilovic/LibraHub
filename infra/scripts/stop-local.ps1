# PowerShell script to stop all LibraHub services

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent (Split-Path -Parent $ScriptDir)

Write-Host "Stopping all LibraHub services..." -ForegroundColor Yellow

# Stop all service APIs
Write-Host "`nStopping service APIs..." -ForegroundColor Yellow

$Services = @(
    "services\Gateway",
    "services\Notifications",
    "services\Library",
    "services\Orders",
    "services\Content",
    "services\Catalog",
    "services\Identity"
)

foreach ($Service in $Services) {
    $ServiceName = Split-Path -Leaf $Service
    Write-Host "Stopping $ServiceName service..." -ForegroundColor Cyan
    Set-Location "$RootDir\$Service"
    docker-compose down
}

# Stop infrastructure services
Write-Host "`nStopping infrastructure services..." -ForegroundColor Yellow
Set-Location $RootDir
docker-compose down

Write-Host "`nAll services stopped successfully!" -ForegroundColor Green

