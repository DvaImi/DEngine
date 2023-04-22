//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using UnityGameFramework.Editor;
using UnityGameFramework.Editor.ResourceTools;

namespace GeminiLion.Editor
{
    public static class GameFrameworkConfigs
    {
        [BuildSettingsConfigPath]
        public static string BuildSettingsConfig = Utility.Path.GetRegularPath(GeminiLionSetting.Instance.BuildSettingsConfig);

        [ResourceCollectionConfigPath]
        public static string ResourceCollectionConfig = Utility.Path.GetRegularPath(GeminiLionSetting.Instance.ResourceCollectionConfig);

        [ResourceEditorConfigPath]
        public static string ResourceEditorConfig = Utility.Path.GetRegularPath(GeminiLionSetting.Instance.ResourceEditorConfig);

        [ResourceBuilderConfigPath]
        public static string ResourceBuilderConfig = Utility.Path.GetRegularPath(GeminiLionSetting.Instance.ResourceBuilderConfig);
    }
}
