// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-15 11:24:21
// 版 本：1.0
// ========================================================
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
        /// 构建信息路径
        /// </summary>
        public string BuildInfoPath = "GameMain/Configs/Runtime/BuildInfo.txt";
        /// <summary>
        /// 数据表关联路径
        /// </summary>
        public string PreloadInfoPath = "GameMain/Configs/Runtime/PreloadInfo.txt";
        /// <summary>
        /// 热更程序集生成路径
        /// </summary>
        public string HotfixDllPath = "Assets/GameMain/HybridCLR/Dlls";
        /// <summary>
        /// 主热更程序集
        /// </summary>
        public string HotfixDllNameMain = "Game.Hotfix.dll";
        /// <summary>
        /// AOT 程序集
        /// </summary>
        public string[] AOTDllNames = { "mscorlib.dll", "System.dll", "System.Core.dll" };

        /// <summary>
        /// 其他预留热更新程序集
        /// </summary>
        public string[] PreserveHotfixDllNames;
        /// <summary>
        /// 热更程序集后缀
        /// </summary>
        public string HotfixDllSuffix = ".bytes";
        /// <summary>
        /// 热更配置信息
        /// </summary>
        public string HotfixInfoPath;
        /// <summary>
        /// 热更新启动器资源
        /// </summary>
        public string HotfixLauncher;
        /// <summary>
        /// 请求版本文件接口
        /// </summary>
        public string CheckVersionUrl = "http://192.168.1.102/{0}Version.txt";
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
        public string UpdatePrefixUri = "http://192.168.1.102/{0}_{1}/{2}";
    }
}
