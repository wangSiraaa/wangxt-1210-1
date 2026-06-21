#!/bin/bash
set -e

echo "======================================"
echo "数据中心供电切换API - 启动脚本"
echo "======================================"
echo ""

cd "$(dirname "$0")/../PowerSwitchApi"

echo "[1/4] 检查 .NET SDK 版本..."
if ! command -v dotnet &> /dev/null; then
    echo "❌ 未找到 dotnet SDK，请安装 .NET 8.0 SDK"
    echo "下载: https://dotnet.microsoft.com/download"
    exit 1
fi
dotnet --version

echo ""
echo "[2/4] 还原 NuGet 包..."
dotnet restore

echo ""
echo "[3/4] 构建项目..."
dotnet build --no-restore

echo ""
echo "[4/4] 启动后端 API (端口 5000)..."
echo ""
echo "Swagger UI: http://localhost:5000"
echo "健康检查: http://localhost:5000/health"
echo ""
echo "数据库说明:"
echo "  - 默认使用 (localdb)\\MSSQLLocalDB"
echo "  - 如需使用 SQL Server，请修改 appsettings.json 的 ConnectionStrings"
echo "  - 如需使用 Docker SQL Server，设置 ConnectionStrings__DefaultConnection 环境变量"
echo ""

read -p "是否立即启动? (y/n) " -n 1 -r
echo ""
if [[ $REPLY =~ ^[Yy]$ ]]; then
    dotnet run --no-build --urls "http://0.0.0.0:5000"
fi
