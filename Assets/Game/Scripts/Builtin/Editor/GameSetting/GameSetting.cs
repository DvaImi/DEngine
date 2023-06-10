// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-15 11:24:21
// 版 本：1.0
// ========================================================
using GameFramework.Resource;
using UnityEditor;
using UnityEditorInternal;

namespace Game.Editor
{
    [GameFilePath("ProjectSettings/GameSetting.asset")]
    public class GameSetting : ScriptableSingleton<GameSetting>
    {
        /// <summary>
        /// 
        /// </summary>
        public string BuildSettingsConfig = "GameMain/Configs/Editor/BuildSettings.xml";
        /// <summary>
        /// 
        /// </summary>
        public string ResourceCollectionConfig = "GameMain/Configs/Editor/ResourceCollection.xml";
        /// <summary>
        /// 
        /// </summary>
        public string ResourceEditorConfig = "GameMain/Configs/Editor/ResourceEditor.xml";
        /// <summary>
        /// 
        /// </summary>
        public string ResourceBuilderConfig = "GameMain/Configs/Editor/ResourceBuilder.xml";

        /// <summary>
        /// 记录打包平台
        /// </summary>
        public int BuildPlatform;
        /// <summary>
        /// 资源打包模式
        /// </summary>
        public int ResourceModeIndex;

        /// <summary>
        /// 应用发布路径
        /// </summary>
        public string AppOutput = "Assets/../AppOutput";
        /// <summary>
        /// 资源打包路径
        /// </summary>
        public string BundlesOutput = "Assets/../BundlesOutput";
       
        /// <summary>
        /// 是否需要强制更新应用
        /// </summary>
        public bool ForceUpdateGame = false;
        /// <summary>
        /// 请求版本文件接口
        /// </summary>
        public string CheckVersionUrl;
        /// <summary>
        /// 
        /// </summary>
        public string WindowsAppUrl;
        /// <summary>
        /// 
        /// </summary>
        public string MacOSAppUrl;
        /// <summary>
        /// 
        /// </summary>
        public string IOSAppUrl;
        /// <summary>
        /// 
        /// </summary>
        public string AndroidAppUrl;
        /// <summary>
        /// 下载资源接口
        /// </summary>
        public string UpdatePrefixUri;
        /// <summary>
        /// 热更程序集生成路径
        /// </summary>
        public string HotupdateDllPath = "Assets/GameMain/HybridCLR/Dlls";
        /// <summary>
        /// 主热更程序集
        /// </summary>
        public AssemblyDefinitionAsset HotUpdateAssemblyDefinition;
        /// <summary>
        /// AOT 程序集
        /// </summary>
        public string[] AOTDllNames = { "mscorlib.dll", "System.dll", "System.Core.dll" };
        /// <summary>
        /// 虚拟服务器地址
        /// </summary>
        public string VirtualServerAddress;
        /// <summary>
        /// 自动拷贝最新资源包
        /// </summary>
        public bool AutoCopyToVirtualServer;

        /// <summary>
        /// 其他预留热更新程序集
        /// </summary>
        public string[] PreserveHotfixDllNames;
        public void SaveSetting()
        {
            Save();
        }
    }
}
