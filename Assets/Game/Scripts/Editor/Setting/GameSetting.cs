// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-15 11:24:21
// 版 本：1.0
// ========================================================
using UnityEngine;

namespace Game.Editor
{
    [GameFilePath("Assets/Game/Configuration/GameSetting.asset")]
    public class GameSetting : ScriptableSingleton<GameSetting>
    {
        /// <summary>
        /// 
        /// </summary>
        public string BuildSettingsConfig = "Assets/Game/Configuration/BuildSetting.xml";
        /// <summary>
        /// 
        /// </summary>
        public string ResourceCollectionConfig = "Assets/Game/Configuration/ResourceCollection.xml";
        /// <summary>
        /// 
        /// </summary>
        public string ResourceEditorConfig = "Assets/Game/Configuration/ResourceEditor.xml";
        /// <summary>
        /// 
        /// </summary>
        public string ResourceBuilderConfig = "Assets/Game/Configuration/ResourceBuilder.xml";

        /// <summary>
        /// 记录打包平台
        /// </summary>
        public int BuildPlatform = 1;
        /// <summary>
        /// 资源打包模式
        /// </summary>
        public int ResourceModeIndex = 1;
        /// <summary>
        /// 差异化打包
        /// </summary>
        public bool Difference;
        /// <summary>
        /// 强制重建资源包
        /// </summary>
        public bool ForceRebuild;
        /// <summary>
        /// 应用发布路径
        /// </summary>
        public string AppOutput => "Assets/../AppOutput";
        /// <summary>
        /// 资源打包路径
        /// </summary>
        public string BundlesOutput => "Assets/../BundlesOutput";
        /// <summary>
        /// 选择打包的包裹
        /// </summary>
        public int AssetBundleCollectorIndex;
        /// <summary>
        /// 是否需要强制更新应用
        /// </summary>
        public bool ForceUpdateGame = false;
        /// <summary>
        /// 内置资源版本
        /// </summary>
        public int InternalResourceVersion;
        /// <summary>
        /// 最新的游戏版本号
        /// </summary>
        public string LatestGameVersion = string.Empty;
        /// <summary>
        /// 资源更新下载地址
        /// </summary>
        public string UpdatePrefixUri = string.Empty;
        /// <summary>
        /// 内置信息
        /// </summary>
        public BuildInfo BuildInfo = new();
        /// <summary>
        /// 热更程序集生成路径
        /// </summary>
        public string HotupdateAssembliesPath = "Assets/Game/HybridCLRDate/HotUpdate";
        /// <summary>
        /// 主热更程序集
        /// </summary>
        public string[] HotUpdateAssemblies = new string[] { };
        /// <summary>
        /// AOT 元数据生成路径
        /// </summary>
        public string AOTAssembliesPath = "Assets/Game/HybridCLRData/AOT";
        /// <summary>
        /// AOT 程序集
        /// </summary>
        public string[] AOTAssemblies = { "mscorlib.dll", "System.dll", "System.Core.dll" };
        /// <summary>
        /// 其他预留程序集生成路径
        /// </summary>
        public string PreserveAssembliesPath = "Assets/Game/HybridCLRData/Preserve";
        /// <summary>
        /// 其他预留按需加载程序集
        /// </summary>
        public string[] PreserveAssemblies = new string[] { };

        public void SaveSetting()
        {
            Save();
            Debug.Log("Save setting success");
        }
    }
}
