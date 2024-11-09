// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-15 11:24:21
// 版 本：1.0
// ========================================================

using System;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using UnityEngine.Serialization;

namespace Game.Editor
{
    [GameFilePath("Assets/Game/Configuration/DEngineSetting.asset")]
    public class DEngineSetting : ScriptableSingleton<DEngineSetting>
    {
        /// <summary>
        /// 应用发布路径
        /// </summary>
        public static string AppOutput => "Assets/../AppOutput";

        /// <summary>
        /// 资源打包路径
        /// </summary>
        public static string BundlesOutput => "Assets/../BundlesOutput";

        /// <summary>
        /// 
        /// </summary>
        public string DEngineTypeConfig = "Assets/Game/Configuration/DEngineTypeSetting.xml";

        /// <summary>
        /// 
        /// </summary>
        public string BuildSettingsConfig = "Assets/Game/Configuration/BuildSettings.xml";

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
        /// 资源打包模式
        /// </summary>
        public ResourceMode ResourceMode;

        /// <summary>
        /// 构建项目事件处理接口
        /// </summary>
        public string BuildPlayerEventHandlerTypeName;

        /// <summary>
        /// 强制重建资源包
        /// </summary>
        public bool ForceRebuildAssetBundle;

        /// <summary>
        ///  默认构建场景
        /// </summary>
        public string[] DefaultSceneNames = Array.Empty<string>();

        /// <summary>
        /// 搜索场景路径
        /// </summary>
        public string[] SearchScenePaths = { "Assets" };

        /// <summary>
        /// 选择打包的包裹
        /// </summary>
        public int AssetBundleCollectorIndex;

        /// <summary>
        /// 是否需要强制更新应用
        /// </summary>
        public bool ForceUpdateGame = false;

        /// <summary>
        /// 构建补丁包
        /// </summary>
        public bool BuildResourcePack = false;

        /// <summary>
        /// 补丁包基础版本
        /// </summary>
        public string SourceVersion;

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string HostURL = "http://localhost";

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int HostingServicePort = 8899;

        /// <summary>
        /// 内置资源版本
        /// </summary>
        public int InternalResourceVersion;

        /// <summary>
        /// 最新的游戏版本号
        /// </summary>
        public string LatestGameVersion = string.Empty;

        /// <summary>
        /// 游戏内部版本号
        /// </summary>
        public int InternalGameVersion = 0;

        /// <summary>
        /// 
        /// </summary>
        public string WindowsAppUrl = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public string MacOSAppUrl = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public string IOSAppUrl = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public string AndroidAppUrl = string.Empty;

        /// <summary>
        /// 是否开启本地资源服务
        /// </summary>
        public bool EnableHostingService = false;

        /// <summary>
        /// 热更程序集生成路径
        /// </summary>
        public string UpdateAssembliesPath = "Assets/../HybridCLRData/Output/HotUpdate";

        /// <summary>
        /// 主热更程序集
        /// </summary>
        public string[] UpdateAssemblies = Array.Empty<string>();

        /// <summary>
        /// AOT 元数据生成路径
        /// </summary>
        public string AOTAssembliesPath = "Assets/../HybridCLRData/Output/AOT";

        /// <summary>
        /// AOT 程序集
        /// </summary>
        public string[] AOTAssemblies = { "mscorlib.dll", "System.dll", "System.Core.dll" };

        /// <summary>
        /// 其他预留程序集生成路径
        /// </summary>
        public string PreserveAssembliesPath = "Assets/../HybridCLRData/Output/Preserve";

        /// <summary>
        /// 其他预留按需加载程序集
        /// </summary>
        public string[] PreserveAssemblies = Array.Empty<string>();
    }
}