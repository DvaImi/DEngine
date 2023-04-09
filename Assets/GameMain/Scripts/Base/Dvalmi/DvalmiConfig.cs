using System.Linq;
using GameFramework;
using UnityEngine;

namespace Dvalmi
{
    public static class DvalmiConfig
    {
        private static GameFrameworkSettings GameFrameworkSettings { get; set; }

        static DvalmiConfig()
        {
            if (GameFrameworkSettings == null)
            {
                GameFrameworkSettings = Resources.LoadAll<GameFrameworkSettings>("GameFrameworkSettings").FirstOrDefault();
            }
        }

        /// <summary>
        /// 基础程序集
        /// </summary>
        public static string NameSpace => GameFrameworkSettings.NameSpace;
        /// <summary>
        /// 热更新程序集
        /// </summary>
        public static string HotfixNameSpace => GameFrameworkSettings.HotfixNameSpace;

        /// <summary>
        /// 游戏版本
        /// </summary>
        public static string GameVersion => GameFrameworkSettings.GameVersion;

        /// <summary>
        /// 游戏内部版本
        /// </summary>
        public static int InternalGameVersion => GameFrameworkSettings.InternalGameVersion;

        /// <summary>
        /// 
        /// </summary>
        public static string BuildSettingsConfig => GameFrameworkSettings.BuildSettingsConfig;
        /// <summary>
        /// 
        /// </summary>
        public static string ResourceCollectionConfig => GameFrameworkSettings.ResourceCollectionConfig;
        /// <summary>
        /// 
        /// </summary>
        public static string ResourceEditorConfig => GameFrameworkSettings.ResourceEditorConfig;
        /// <summary>
        /// 
        /// </summary>
        public static string ResourceBuilderConfig => GameFrameworkSettings.ResourceBuilderConfig;

        /// <summary>
        /// 实体脚本生成路径
        /// </summary>
        public static string EntityCodePath => GameFrameworkSettings.EntityCodePath;
        /// <summary>
        ///  热更实体脚本生成路径
        /// </summary>
        public static string HotfixEntityCodePath => GameFrameworkSettings.HotfixEntityCodePath;
        /// <summary>
        /// UI界面逻辑生成路径
        /// </summary>
        public static string UIFormCodePath => GameFrameworkSettings.UIFormCodePath;
        /// <summary>
        /// 热更UI界面逻辑生成路径
        /// </summary>
        public static string HotfixUIFormCodePath => GameFrameworkSettings.HotfixUIFormCodePath;
        /// <summary>
        /// 事件生成路径
        /// </summary>
        public static string EventCodePath => GameFrameworkSettings.EventCodePath;
        /// <summary>
        /// 热更事件生成路径
        /// </summary>
        public static string HotfixEventCodePath => GameFrameworkSettings.HotfixEventCodePath;

        /// <summary>
        /// 游戏配置路径
        /// </summary>
        public static string ConfigPath => GameFrameworkSettings.ConfigPath;

        /// <summary>
        /// 游戏数据表路径
        /// </summary>
        public static string DataTablePath => GameFrameworkSettings.DataTablePath;

        /// <summary>
        /// 数据表类路径
        /// </summary>
        public static string CSharpCodePath => GameFrameworkSettings.CSharpCodePath;

        /// <summary>
        /// 数据表类模板路径
        /// </summary>
        public static string CSharpCodeTemplateFileName => GameFrameworkSettings.CSharpCodeTemplateFileName;

        /// <summary>
        /// 数据表
        /// </summary>
        public static string DataRowClassPrefixName => GameFrameworkSettings.DataRowClassPrefixName;
        /// <summary>
        /// 
        /// </summary>
        public static string DataRowClassHotfixPrefixName => GameFrameworkSettings.DataRowClassHotfixPrefixName;

        /// <summary>
        /// 热更程序集生成路径
        /// </summary>
        public static string HotfixDllPath => GameFrameworkSettings.HotfixDllPath;
        /// <summary>
        /// 主热更程序集
        /// </summary>
        public static string HotfixDllNameMain => GameFrameworkSettings.HotfixDllNameMain;

        /// <summary>
        /// 其他预留热更新程序集
        /// </summary>
        public static string[] PreserveHotfixDllNames => GameFrameworkSettings.PreserveHotfixDllNames;
        /// <summary>
        /// 热更程序集后缀
        /// </summary>
        public static string HotfixDllSuffix => GameFrameworkSettings.HotfixDllSuffix;
        /// <summary>
        /// 获取热更dll 的文件资源
        /// </summary>
        public static string HotfixDllAssetName => HotfixDllPath + "/{0}.bytes";


        /// <summary>
        /// 请求版本文件接口
        /// </summary>
        public static string CheckVersionUrl => GameFrameworkSettings.CheckVersionUrl;
        /// <summary>
        /// 
        /// </summary>
        public static string WindowsAppUrl => GameFrameworkSettings.WindowsAppUrl;
        /// <summary>
        /// 
        /// </summary>
        public static string MacOSAppUrl => GameFrameworkSettings.MacOSAppUrl;
        /// <summary>
        /// 
        /// </summary>
        public static string IOSAppUrl => GameFrameworkSettings.IOSAppUrl;
        /// <summary>
        /// 
        /// </summary>
        public static string AndroidAppUrl => GameFrameworkSettings.AndroidAppUrl;

        /// <summary>
        /// 下载资源接口
        /// </summary>
        public static string UpdatePrefixUri => GameFrameworkSettings.UpdatePrefixUri;
    }
}