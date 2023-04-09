//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using System.IO;
using UnityEngine;
using UnityGameFramework.Editor;
using UnityGameFramework.Editor.ResourceTools;

namespace Dvalmi.Editor
{
    public static class GameFrameworkConfigs
    {
        [BuildSettingsConfigPath]
        public static string BuildSettingsConfig = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, DvalmiConfig.BuildSettingsConfig));

        [ResourceCollectionConfigPath]
        public static string ResourceCollectionConfig = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, DvalmiConfig.ResourceCollectionConfig));

        [ResourceEditorConfigPath]
        public static string ResourceEditorConfig = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, DvalmiConfig.ResourceEditorConfig));

        [ResourceBuilderConfigPath]
        public static string ResourceBuilderConfig = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, DvalmiConfig.ResourceBuilderConfig));
    }
}
