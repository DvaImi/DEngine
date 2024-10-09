#!/bin/bash

# 获取脚本所在的目录并赋值给 WORKSPACE
WORKSPACE=$(cd "$(dirname "$0")" && pwd)

# 切换到 WORKSPACE 目录
cd "$WORKSPACE"

# 打印 WORKSPACE 目录以确认
echo "Current WORKSPACE directory: $WORKSPACE"

# 设置路径
GEN_CLIENT="$WORKSPACE/Tools/Luban.dll"
CONF_ROOT="$WORKSPACE/Client"
CODE_UNITASK="$CONF_ROOT/CustomTemplate_Client_UniTask"

# 运行 Luban 工具
dotnet "$GEN_CLIENT" \
    -t client \
    -c cs-bin \
    -d bin \
    --conf "$CONF_ROOT/luban.conf" \
    --customTemplateDir "$CODE_UNITASK" \
    -x outputCodeDir="../../Client/DEngine/Assets/Game/Scripts/Runtime/Update/Generate/GameConfig" \
    -x outputDataDir="../../Client/DEngine/Assets/Game/Bundles/Configs/Bin/"

# 暂停
read -p "Press any key to continue... "

exit
