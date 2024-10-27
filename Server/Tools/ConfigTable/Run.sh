#!/bin/bash
# 获取脚本所在的目录并赋值给 WORKSPACE
WORKSPACE=$(cd "$(dirname "$0")" && pwd)

# 切换到 WORKSPACE 目录
cd "$WORKSPACE"

dotnet Fantasy.Tools.ConfigTable.dll --ExportType 2
