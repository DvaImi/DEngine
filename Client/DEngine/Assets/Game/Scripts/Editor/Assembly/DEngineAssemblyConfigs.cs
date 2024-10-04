using DEngine;
using DEngine.Editor;

namespace Game.Editor
{
    public static class DEngineAssemblyConfigs
    {
        [DEngineTypeConfigPath] 
        public static string DEngineTypeConfig = Utility.Path.GetRegularPath(GameSetting.Instance.DEngineTypeConfig);
    }
}