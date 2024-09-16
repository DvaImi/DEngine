#!/bin/bash

# 设置工作目录
WORKSPACE=.

# 设置生成客户端的工具和配置文件路径
GEN_CLIENT="$WORKSPACE/Tools/Luban.dll"
CONF_ROOT="$WORKSPACE/Configs"
CODE_UNITASK="$CONF_ROOT/CustomTemplate_Client_UniTask"

# 使用 dotnet 执行 Luban 的生成命令
dotnet "$GEN_CLIENT" \
    -t client \
    -c cs-bin \
    -d bin \
    --conf "$CONF_ROOT/luban.conf" \
    --customTemplateDir "$CODE_UNITASK" \
    -x outputCodeDir=../../Client/DEngine/Assets/Game/Scripts/Update/Generate/GameConfig \
    -x outputDataDir=../../Client/DEngine/Assets/Game/Bundles/Configs/Bin/

# 暂停（可选）
read -p "Press any key to continue..."

