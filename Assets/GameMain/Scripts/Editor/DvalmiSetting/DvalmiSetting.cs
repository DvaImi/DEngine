// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-15 11:24:21
// 版 本：1.0
// ========================================================
namespace Dvalmi.Editor
{
    [DvalmiFilePath("ProjectSettings/DvalmiSetting.asset")]
    public class DvalmiSetting : ScriptableSingleton<DvalmiSetting>
    {
        /// <summary>
        /// 基础程序集
        /// </summary>
        public string NameSpace = "Dvalmi";
        /// <summary>
        /// 热更新程序集
        /// </summary>
        public string HotfixNameSpace = "Dvalmi.Hotfix";
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
        /// AB包构建路径
        /// </summary>
        public string AssetBundleOutput = "";
        /// <summary>
        /// 构建应用路径
        /// </summary>
        public string PublishAppOutput = "";
        /// <summary>
        /// 构建信息路径
        /// </summary>
        public string BuildInfoPath = "GameMain/Configs/Runtime/BuildInfo.txt"; 
        /// <summary>
        /// 数据表关联路径
        /// </summary>
        public string PreloadInfoPath = "GameMain/Configs/Runtime/PreloadInfo.txt";
        /// <summary>
        /// 实体脚本生成路径
        /// </summary>
        public string EntityCodePath = "Assets/GameMain/Scripts/Base/Entity";
        /// <summary>
        ///  热更实体脚本生成路径
        /// </summary>
        public string HotfixEntityCodePath = "Assets/GameMain/Scripts/Hotfix/Entity";
        /// <summary>
        /// UI界面逻辑生成路径
        /// </summary>
        public string UIFormCodePath = "Assets/GameMain/Scripts/Base/UI";
        /// <summary>
        /// 热更UI界面逻辑生成路径
        /// </summary>
        public string HotfixUIFormCodePath = "Assets/GameMain/Scripts/Hotfix/UI";
        /// <summary>
        /// 事件生成路径
        /// </summary>
        public string EventCodePath = "Assets/GameMain/Scripts/EventArgs";
        /// <summary>
        /// 热更事件生成路径
        /// </summary>
        public string HotfixEventCodePath = "Assets/GameMain/Scripts/Hotfix/EventArgs";
        /// <summary>
        /// 数据表格路径
        /// </summary>
        public string DataTableExcelPath = "";
        /// <summary>
        /// 游戏数据表生成路径路径
        /// </summary>
        public string DataTablePath = "Assets/GameMain/DataTables";
        /// <summary>
        /// 数据表类路径
        /// </summary>
        public string CSharpCodePath = "Assets/GameMain/Scripts/Hotfix/DataTable";
        /// <summary>
        /// 数据表类模板路径
        /// </summary>
        public string CSharpCodeTemplateFileName = "Assets/GameMain/Configs/Editor/DataTableCodeTemplate.txt";
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
