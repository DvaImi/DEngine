#!/bin/bash

# 获取脚本所在的目录并赋值给 WORKSPACE
WORKSPACE=$(cd "$(dirname "$0")" && pwd)

# 切换到 WORKSPACE 目录
cd "$WORKSPACE"

echo "1. Client"
echo "2. Server"
echo "3. All"

read -n 1 -p "Please select an option:" choice
echo ""
echo ""
case $choice in
    1)
        dotnet Fantasy.Tools.NetworkProtocol.dll --ExportPlatform 1
        ;;
    2)
        dotnet Fantasy.Tools.NetworkProtocol.dll --ExportPlatform 2
        ;;
    3)
        dotnet Fantasy.Tools.NetworkProtocol.dll --ExportPlatform 3
        ;;
    *)
        echo "Invalid option"
        ;;
esac
