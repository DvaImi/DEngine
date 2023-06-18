using DEngine;
using DEngine.Editor;
using DEngine.Editor.ResourceTools;

namespace Game.Editor
{
    public static class DEngineConfigs
    {
        [BuildSettingsConfigPath]
        public static string BuildSettingsConfig = Utility.Path.GetRegularPath(GameSetting.Instance.BuildSettingsConfig);

        [ResourceCollectionConfigPath]
        public static string ResourceCollectionConfig = Utility.Path.GetRegularPath(GameSetting.Instance.ResourceCollectionConfig);

        [ResourceEditorConfigPath]
        public static string ResourceEditorConfig = Utility.Path.GetRegularPath(GameSetting.Instance.ResourceEditorConfig);

        [ResourceBuilderConfigPath]
        public static string ResourceBuilderConfig = Utility.Path.GetRegularPath(GameSetting.Instance.ResourceBuilderConfig);
    }
}
