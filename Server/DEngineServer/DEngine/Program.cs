using Fantasy.Helper;
using Fantasy.Platform.Net;


var machineConfigText = await FileHelper.GetTextByRelativePath("../Config/Json/Server/MachineConfigData.Json");
var processConfigText = await FileHelper.GetTextByRelativePath("../Config/Json/Server/ProcessConfigData.Json");
var worldConfigText = await FileHelper.GetTextByRelativePath("../Config/Json/Server/WorldConfigData.Json");
var sceneConfigText = await FileHelper.GetTextByRelativePath("../Config/Json/Server/SceneConfigData.Json");
// 初始化配置文件
// 如果重复初始化方法会覆盖掉上一次的数据，非常适合热重载时使用
MachineConfigData.Initialize(machineConfigText);
ProcessConfigData.Initialize(processConfigText);
WorldConfigData.Initialize(worldConfigText);
SceneConfigData.Initialize(sceneConfigText);

Fantasy.Log.Register(new Fantasy.NLog("Server"));

await Entry.Start(Fantasy.AssemblyHelper.Assemblies);
