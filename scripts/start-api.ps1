#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "供电切换API - 启动脚本 (Windows)" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

Set-Location "$PSScriptRoot\..\PowerSwitchApi"

Write-Host "[1/4] 检查 .NET SDK..." -ForegroundColor Yellow
try {
    $version = dotnet --version
    Write-Host "   版本: $version" -ForegroundColor Green
} catch {
    Write-Host "❌ 未安装 .NET 8.0 SDK" -ForegroundColor Red
    Write-Host "   下载: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "[2/4] 还原 NuGet 包..." -ForegroundColor Yellow
dotnet restore

Write-Host ""
Write-Host "[3/4] 构建项目..." -ForegroundColor Yellow
dotnet build --no-restore

Write-Host ""
Write-Host "[4/4] 启动 API (端口 5000)..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Swagger UI: http://localhost:5000" -ForegroundColor Cyan
Write-Host "健康检查: http://localhost:5000/health" -ForegroundColor Cyan
Write-Host ""
Write-Host "默认连接 LocalDB: (localdb)\MSSQLLocalDB" -ForegroundColor Magenta
Write-Host "如果 LocalDB 未安装，安装 SQL Server Express LocalDB:" -ForegroundColor Magenta
Write-Host "https://learn.microsoft.com/zh-cn/sql/database-engine/configure-windows/sql-server-express-localdb" -ForegroundColor Magenta
Write-Host ""

dotnet run --no-build --urls "http://0.0.0.0:5000"
