cd /d %~dp0
set WORKSPACE=.

set GEN_CLIENT=%WORKSPACE%\Tools\Luban.dll
set CONF_ROOT=%WORKSPACE%\Client
set CODE_UNITASK=%CONF_ROOT%\CustomTemplate_Client_UniTask

dotnet %GEN_CLIENT% ^
    -t client ^
    -c cs-bin ^
    -d bin  ^
    --conf %CONF_ROOT%\luban.conf ^
    --customTemplateDir %CODE_UNITASK% ^
    -x outputCodeDir=../../Client/DEngine/Assets/Game/Scripts/Runtime/Update/Generate/GameConfig ^
    -x outputDataDir=../../Client/DEngine/Assets/Game/Bundles/Configs/Bin/
pause
    