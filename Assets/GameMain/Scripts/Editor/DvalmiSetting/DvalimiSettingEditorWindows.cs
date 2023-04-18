// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-15 19:31:59
// 版 本：1.0
// ========================================================
using System.IO;
using System.Text;
using Dvalmi;
using Dvalmi.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using DFilePath = Sirenix.OdinInspector.FilePathAttribute;
public class DvalimiSettingEditorWindows : OdinEditorWindow
{
    [MenuItem("Dvalmi/ Setting")]
    private static void OpenWindow()
    {
        var window = GetWindow<DvalimiSettingEditorWindows>("Dvalmi Setting");
        window.minSize = new Vector2(800, 600);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        BuildSettingsConfig = DvalmiSetting.Instance.BuildSettingsConfig;
        ResourceCollectionConfig = DvalmiSetting.Instance.ResourceCollectionConfig;
        ResourceEditorConfig = DvalmiSetting.Instance.ResourceEditorConfig;
        ResourceBuilderConfig = DvalmiSetting.Instance.ResourceBuilderConfig;
        AssetBundleOutput = DvalmiSetting.Instance.AssetBundleOutput;
        PublishAppOutput = DvalmiSetting.Instance.PublishAppOutput;
        BuildInfoPath = DvalmiSetting.Instance.BuildInfoPath;
        PreloadInfoPath = DvalmiSetting.Instance.PreloadInfoPath;
        EntityCodePath = DvalmiSetting.Instance.EntityCodePath;
        HotfixEntityCodePath = DvalmiSetting.Instance.HotfixEntityCodePath;
        UIFormCodePath = DvalmiSetting.Instance.UIFormCodePath;
        HotfixUIFormCodePath = DvalmiSetting.Instance.HotfixUIFormCodePath;
        EventCodePath = DvalmiSetting.Instance.EventCodePath;
        HotfixEventCodePath = DvalmiSetting.Instance.HotfixEventCodePath;
        DataTableExcelPath = DvalmiSetting.Instance.DataTableExcelPath;
        DataTablePath = DvalmiSetting.Instance.DataTablePath;
        CSharpCodePath = DvalmiSetting.Instance.CSharpCodePath;
        CSharpCodeTemplateFileName = DvalmiSetting.Instance.CSharpCodeTemplateFileName;
        HotfixDllPath = DvalmiSetting.Instance.HotfixDllPath;
        HotfixDllNameMain = DvalmiSetting.Instance.HotfixDllNameMain;
        AOTDllNames = DvalmiSetting.Instance.AOTDllNames;
        PreserveHotfixDllNames = DvalmiSetting.Instance.PreserveHotfixDllNames;
        HotfixDllSuffix = DvalmiSetting.Instance.HotfixDllSuffix;
        HotfixInfoPath = DvalmiSetting.Instance.HotfixInfoPath;
        HotfixLauncher = DvalmiSetting.Instance.HotfixLauncher;
        CheckVersionUrl = DvalmiSetting.Instance.CheckVersionUrl;
        WindowsAppUrl = DvalmiSetting.Instance.WindowsAppUrl;
        MacOSAppUrl = DvalmiSetting.Instance.MacOSAppUrl;
        IOSAppUrl = DvalmiSetting.Instance.IOSAppUrl;
        AndroidAppUrl = DvalmiSetting.Instance.AndroidAppUrl;
        UpdatePrefixUri = DvalmiSetting.Instance.UpdatePrefixUri;
    }


    protected override void OnDisable()
    {
        base.OnDisable();
        SaveSetting();
    }

    /// <summary>
    /// 
    /// </summary>
    public string GameVersion = "0.1.1";
    /// <summary>
    /// 
    /// </summary>
    public int InternalGameVersion = 0;
    /// <summary>
    /// 基础程序集
    /// </summary>
    public string NameSpace = "Dvalmi";
    /// <summary>
    /// 热更新程序集
    /// </summary>
    public string HotfixNameSpace = "Dvalmi.Hotfix";

    [Header("Build Setting")]
    [DFilePath(Extensions = "*.xml")]
    /// <summary>
    /// 
    /// </summary>
    public string BuildSettingsConfig;
    [DFilePath(Extensions = "*.xml")]
    /// <summary>
    /// 
    /// </summary>
    public string ResourceCollectionConfig;
    [DFilePath(Extensions = "*.xml")]
    /// <summary>
    /// 
    /// </summary>
    public string ResourceEditorConfig;
    [DFilePath(Extensions = "*.xml")]
    /// <summary>
    /// 
    /// </summary>
    public string ResourceBuilderConfig;
    /// <summary>
    /// 构建信息写入路径
    /// </summary>
    [DFilePath(Extensions = "*.txt")]
    public string BuildInfoPath;
    /// <summary>
    /// 数据表信息写入路径
    /// </summary>
    [DFilePath(Extensions = "*.txt")]
    public string PreloadInfoPath;
    /// <summary>
    /// AssetBundle 构建路径
    /// </summary>
    [FolderPath]
    public string AssetBundleOutput;
    /// <summary>
    /// 构建应用路径
    /// </summary>
    [FolderPath]
    public string PublishAppOutput;

    [Header("Scripts Generate Path")]
    [Space]
    [FolderPath]
    /// <summary>
    /// 实体脚本生成路径
    /// </summary>
    public string EntityCodePath;
    [FolderPath]
    /// <summary>
    ///  热更实体脚本生成路径
    /// </summary>
    public string HotfixEntityCodePath;
    [FolderPath]
    /// <summary>
    /// UI界面逻辑生成路径
    /// </summary>
    public string UIFormCodePath;
    [FolderPath]
    /// <summary>
    /// 热更UI界面逻辑生成路径
    /// </summary>
    public string HotfixUIFormCodePath;
    [FolderPath]
    /// <summary>
    /// 事件生成路径
    /// </summary>
    public string EventCodePath;
    [FolderPath]
    /// <summary>
    /// 热更事件生成路径
    /// </summary>
    public string HotfixEventCodePath;

    [Space]
    [Header("DataTable Path")]
    [FolderPath]
    public string DataTableExcelPath;
    [FolderPath]
    /// <summary>
    /// 游戏数据表路径
    /// </summary>
    public string DataTablePath;
    [FolderPath]
    /// <summary>
    /// 数据表逻辑类路径
    /// </summary>
    public string CSharpCodePath;
    [DFilePath(Extensions = "*.txt")]
    /// <summary>
    /// 数据表类模板路径
    /// </summary>
    public string CSharpCodeTemplateFileName;

    [Header("Hotfix Setting")]
    [Space]
    [FolderPath]
    /// <summary>
    /// 热更程序集生成路径
    /// </summary>
    public string HotfixDllPath;
    /// <summary>
    /// 主热更程序集
    /// </summary>
    public string HotfixDllNameMain;
    /// <summary>
    /// AOT 程序集
    /// </summary>
    public string[] AOTDllNames;
    /// <summary>
    /// 其他预留热更新程序集
    /// </summary>
    public string[] PreserveHotfixDllNames;
    /// <summary>
    /// 热更程序集后缀
    /// </summary>
    public string HotfixDllSuffix;
    [DFilePath(Extensions = "*.txt")]
    public string HotfixInfoPath;
    /// <summary>
    /// 热更新启动器资源
    /// </summary>
    [DFilePath(Extensions = "*.prefab")]
    public string HotfixLauncher;

    [Header("Resources Url")]
    [Space]
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

    private void SaveSetting()
    {
        if (EditorGUI.EndChangeCheck())
        {
            DvalmiSetting.Instance.BuildSettingsConfig = BuildSettingsConfig;
            DvalmiSetting.Instance.ResourceCollectionConfig = ResourceCollectionConfig;
            DvalmiSetting.Instance.ResourceEditorConfig = ResourceEditorConfig;
            DvalmiSetting.Instance.ResourceBuilderConfig = ResourceBuilderConfig;
            DvalmiSetting.Instance.AssetBundleOutput = AssetBundleOutput;
            DvalmiSetting.Instance.PublishAppOutput = PublishAppOutput;
            DvalmiSetting.Instance.BuildInfoPath = BuildInfoPath;
            DvalmiSetting.Instance.PreloadInfoPath = PreloadInfoPath;
            DvalmiSetting.Instance.EntityCodePath = EntityCodePath;
            DvalmiSetting.Instance.HotfixEntityCodePath = HotfixEntityCodePath;
            DvalmiSetting.Instance.UIFormCodePath = UIFormCodePath;
            DvalmiSetting.Instance.HotfixUIFormCodePath = HotfixUIFormCodePath;
            DvalmiSetting.Instance.EventCodePath = EventCodePath;
            DvalmiSetting.Instance.HotfixEventCodePath = HotfixEventCodePath;
            DvalmiSetting.Instance.DataTableExcelPath = DataTableExcelPath;
            DvalmiSetting.Instance.DataTablePath = DataTablePath;
            DvalmiSetting.Instance.CSharpCodePath = CSharpCodePath;
            DvalmiSetting.Instance.CSharpCodeTemplateFileName = CSharpCodeTemplateFileName;
            DvalmiSetting.Instance.HotfixDllPath = HotfixDllPath;
            DvalmiSetting.Instance.HotfixDllNameMain = HotfixDllNameMain;
            DvalmiSetting.Instance.AOTDllNames = AOTDllNames;
            DvalmiSetting.Instance.PreserveHotfixDllNames = PreserveHotfixDllNames;
            DvalmiSetting.Instance.HotfixDllSuffix = HotfixDllSuffix;
            DvalmiSetting.Instance.HotfixInfoPath = HotfixInfoPath;
            DvalmiSetting.Instance.HotfixLauncher = HotfixLauncher;
            DvalmiSetting.Instance.CheckVersionUrl = CheckVersionUrl;
            DvalmiSetting.Instance.WindowsAppUrl = WindowsAppUrl;
            DvalmiSetting.Instance.MacOSAppUrl = MacOSAppUrl;
            DvalmiSetting.Instance.IOSAppUrl = IOSAppUrl;
            DvalmiSetting.Instance.AndroidAppUrl = AndroidAppUrl;
            DvalmiSetting.Instance.UpdatePrefixUri = UpdatePrefixUri;

            DvalmiSetting.Save();

            BuildInfo buildInfo = new()
            {
                GameVersion = GameVersion,
                InternalGameVersion = InternalGameVersion,
                CheckVersionUrl = DvalmiSetting.Instance.CheckVersionUrl,
                WindowsAppUrl = DvalmiSetting.Instance.WindowsAppUrl,
                MacOSAppUrl = DvalmiSetting.Instance.MacOSAppUrl,
                IOSAppUrl = DvalmiSetting.Instance.IOSAppUrl,
                AndroidAppUrl = DvalmiSetting.Instance.AndroidAppUrl,
                UpdatePrefixUri = DvalmiSetting.Instance.UpdatePrefixUri
            };

            string buildInfoJson = Newtonsoft.Json.JsonConvert.SerializeObject(buildInfo);


            using (FileStream stream = new(BuildInfoPath, FileMode.Create, FileAccess.Write))
            {
                UTF8Encoding utf8Encoding = new(false);
                using StreamWriter writer = new(stream, utf8Encoding);
                writer.Write(buildInfoJson);
            }
            HotfixInfo hotfixInfo = new()
            {
                HotfixDllPath = DvalmiSetting.Instance.HotfixDllPath,
                HotfixDllNameMain = DvalmiSetting.Instance.HotfixDllNameMain,
                AOTDllNames = DvalmiSetting.Instance.AOTDllNames,
                PreserveHotfixDllNames = DvalmiSetting.Instance.PreserveHotfixDllNames,
                HotfixDllSuffix = DvalmiSetting.Instance.HotfixDllSuffix,
                HotfixLauncher = DvalmiSetting.Instance.HotfixLauncher
            };
            string hotfixJson = Newtonsoft.Json.JsonConvert.SerializeObject(hotfixInfo);

            using (FileStream stream = new(HotfixInfoPath, FileMode.Create, FileAccess.Write))
            {
                UTF8Encoding utf8Encoding = new(false);
                using StreamWriter writer = new(stream, utf8Encoding);
                writer.Write(hotfixJson);
            }

            AssetDatabase.Refresh();
        }
    }
}
