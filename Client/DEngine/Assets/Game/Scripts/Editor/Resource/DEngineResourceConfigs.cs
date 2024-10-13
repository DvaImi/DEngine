using DEngine;
using DEngine.Editor;
using DEngine.Editor.ResourceTools;

namespace Game.Editor
{
    public static class DEngineResourceConfigs
    {
        [BuildSettingsConfigPath]
        public static string BuildSettingsConfig = Utility.Path.GetRegularPath(DEngineSetting.Instance.BuildSettingsConfig);

        [ResourceCollectionConfigPath]
        public static string ResourceCollectionConfig = Utility.Path.GetRegularPath(DEngineSetting.Instance.ResourceCollectionConfig);

        [ResourceEditorConfigPath]
        public static string ResourceEditorConfig = Utility.Path.GetRegularPath(DEngineSetting.Instance.ResourceEditorConfig);

        [ResourceBuilderConfigPath]
        public static string ResourceBuilderConfig = Utility.Path.GetRegularPath(DEngineSetting.Instance.ResourceBuilderConfig);
    }
}
