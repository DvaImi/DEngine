#!/bin/bash

# 设置工作空间
WORKSPACE=$(pwd)

# 设置路径
GEN_CLIENT="$WORKSPACE/Tools/Luban.dll"
CONF_ROOT="$WORKSPACE/Configs"
CODE_UNITASK="$CONF_ROOT/CustomTemplate_Client_UniTask"

# 运行 Luban 工具
dotnet "$GEN_CLIENT" \
    -t client \
    -c cs-bin \
    -d bin \
    --conf "$CONF_ROOT/luban.conf" \
    --customTemplateDir "$CODE_UNITASK" \
    -x outputCodeDir="../../Client/DEngine/Assets/Game/Scripts/Update/Generate/GameConfig" \
    -x outputDataDir="../../Client/DEngine/Assets/Game/Bundles/Configs/Bin/"

# 暂停
read -p "Press any key to continue... "
