cd /d %~dp0
set WORKSPACE=.

set GEN_CLIENT=%WORKSPACE%\Tools\Luban.dll
set CONF_ROOT=%WORKSPACE%\Client
set CODE_LAZY=%CONF_ROOT%\CustomTemplate_Client_LazyLoad

dotnet %GEN_CLIENT% ^
    -t client ^
    -c cs-bin ^
    -d bin ^
    --conf %CONF_ROOT%\luban.conf ^
    --customTemplateDir %CODE_LAZY% ^
    -x outputCodeDir=../../Client/DEngine/Assets/Game/Scripts/Runtime/Update/Generate/GameConfig ^
    -x bin.outputDataDir=Client/output/Bin/
pause
    