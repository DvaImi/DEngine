// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-15 19:31:59
// 版 本：1.0
// ========================================================
using System.IO;
using System.Text;
using GeminiLion;
using GeminiLion.Editor;
using GeminiLion.Editor.ResourceTools;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using DFilePath = Sirenix.OdinInspector.FilePathAttribute;
public class GeminiLionSettingEditorWindows : OdinEditorWindow
{
    [MenuItem("GeminiLion/ Setting", priority = 20)]
    private static void OpenWindow()
    {
        var window = GetWindow<GeminiLionSettingEditorWindows>("GeminiLion Setting");
        window.minSize = new Vector2(800, 600);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GetValue();
    }
    protected override void OnGUI()
    {
        base.OnGUI();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reload"))
        {
            GetValue();
        }
        if (GUILayout.Button("Save"))
        {
            SaveSetting();
        }
        EditorGUILayout.EndHorizontal();
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
    public string NameSpace = "GeminiLion";
    /// <summary>
    /// 热更新程序集
    /// </summary>
    public string HotfixNameSpace = "GeminiLion.Hotfix";

    [Header("Build Setting")]
    [InlineButton(nameof(PingObject), "Go")]
    [DFilePath(Extensions = "*.xml", RequireExistingPath = true)]
    /// <summary>
    /// 
    /// </summary>
    public string BuildSettingsConfig;
    [InlineButton(nameof(PingObject), "Go")]
    [DFilePath(Extensions = "*.xml", RequireExistingPath = true)]
    /// <summary>
    /// 
    /// </summary>
    public string ResourceCollectionConfig;
    [DFilePath(Extensions = "*.xml", RequireExistingPath = true)]
    [InlineButton(nameof(PingObject), "Go")]
    /// <summary>
    /// 
    /// </summary>
    public string ResourceEditorConfig;
    [InlineButton(nameof(PingObject), "Go")]
    [DFilePath(Extensions = "*.xml", RequireExistingPath = true)]
    /// <summary>
    /// 
    /// </summary>
    public string ResourceBuilderConfig;
    /// <summary>
    /// 构建信息写入路径
    /// </summary>
    [DFilePath(Extensions = "*.txt", RequireExistingPath = true)]
    [InlineButton(nameof(PingObject), "Go")]
    public string BuildInfoPath;
    [InlineButton(nameof(PingObject), "Go")]
    /// <summary>
    /// 数据表信息写入路径
    /// </summary>
    [DFilePath(Extensions = "*.txt", RequireExistingPath = true)]
    public string PreloadInfoPath;
    /// <summary>
    /// AssetBundle 构建路径
    /// </summary>
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(OpenOutFolder), "Go")]
    public string AssetBundleOutput;
    /// <summary>
    /// 构建应用路径
    /// </summary>
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(OpenOutFolder), "Go")]
    public string PublishAppOutput;

    [Header("Scripts Generate Path")]
    [Space]
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(PingObject), "Go")]
    /// <summary>
    /// 实体脚本生成路径
    /// </summary>
    public string EntityCodePath;
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(PingObject), "Go")]
    /// <summary>
    ///  热更实体脚本生成路径
    /// </summary>
    public string HotfixEntityCodePath;
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(PingObject), "Go")]
    /// <summary>
    /// UI界面逻辑生成路径
    /// </summary>
    public string UIFormCodePath;
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(PingObject), "Go")]
    /// <summary>
    /// 热更UI界面逻辑生成路径
    /// </summary>
    public string HotfixUIFormCodePath;
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(PingObject), "Go")]
    /// <summary>
    /// 事件生成路径
    /// </summary>
    public string EventCodePath;
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(PingObject), "Go")]
    /// <summary>
    /// 热更事件生成路径
    /// </summary>
    public string HotfixEventCodePath;
    [Space]
    [Header("Config Path")]
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(OpenOutFolder), "Go")]
    public string ConfigExcelPath;

    [Space]
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(PingObject), "Go")]
    public string ConfigDataPath;

    [Space]
    [Header("DataTable Path")]
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(OpenOutFolder), "Go")]
    public string DataTableExcelPath;
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(PingObject), "Go")]
    /// <summary>
    /// 游戏数据表路径
    /// </summary>
    public string DataTablePath;
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(PingObject), "Go")]
    /// <summary>
    /// 数据表逻辑类路径
    /// </summary>
    public string CSharpCodePath;
    [DFilePath(Extensions = "*.txt", RequireExistingPath = true)]
    [InlineButton(nameof(PingObject), "Go")]
    /// <summary>
    /// 数据表类模板路径
    /// </summary>
    public string CSharpCodeTemplateFileName;

    [Space]
    [Header("Dictionary Path")]
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(OpenOutFolder), "Go")]
    public string DictionaryExclePath;
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(OpenOutFolder), "Go")]
    public string DictionaryDataPath;

    [Space]
    [Header("Hotfix Setting")]
    [FolderPath(RequireExistingPath = true)]
    [InlineButton(nameof(PingObject), "Go")]
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
    [InlineButton(nameof(SelectAOTDlls), "Select")]
    public string[] AOTDllNames;
    /// <summary>
    /// 其他预留热更新程序集
    /// </summary>
    public string[] PreserveHotfixDllNames;
    /// <summary>
    /// 热更程序集后缀
    /// </summary>
    public string HotfixDllSuffix;
    [DFilePath(Extensions = "*.txt", RequireExistingPath = true)]
    public string HotfixInfoPath;
    [InlineButton(nameof(PingObject), "Go")]
    /// <summary>
    /// 热更新启动器资源
    /// </summary>
    [DFilePath(Extensions = "*.prefab", RequireExistingPath = true)]
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

    public void SaveSetting()
    {
        if (EditorGUI.EndChangeCheck())
        {
            SaveValue();

            BuildInfo buildInfo = new()
            {
                GameVersion = GameVersion,
                InternalGameVersion = InternalGameVersion,
                CheckVersionUrl = GeminiLionSetting.Instance.CheckVersionUrl,
                WindowsAppUrl = GeminiLionSetting.Instance.WindowsAppUrl,
                MacOSAppUrl = GeminiLionSetting.Instance.MacOSAppUrl,
                IOSAppUrl = GeminiLionSetting.Instance.IOSAppUrl,
                AndroidAppUrl = GeminiLionSetting.Instance.AndroidAppUrl,
                UpdatePrefixUri = GeminiLionSetting.Instance.UpdatePrefixUri
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
                HotfixDllPath = GeminiLionSetting.Instance.HotfixDllPath,
                HotfixDllNameMain = GeminiLionSetting.Instance.HotfixDllNameMain,
                AOTDllNames = GeminiLionSetting.Instance.AOTDllNames,
                PreserveHotfixDllNames = GeminiLionSetting.Instance.PreserveHotfixDllNames,
                HotfixDllSuffix = GeminiLionSetting.Instance.HotfixDllSuffix,
                HotfixLauncher = GeminiLionSetting.Instance.HotfixLauncher
            };
            string hotfixJson = Newtonsoft.Json.JsonConvert.SerializeObject(hotfixInfo);

            using (FileStream stream = new(HotfixInfoPath, FileMode.Create, FileAccess.Write))
            {
                UTF8Encoding utf8Encoding = new(false);
                using StreamWriter writer = new(stream, utf8Encoding);
                writer.Write(hotfixJson);
            }

            ResourceBuildHelper.SaveOutputDirectory(AssetBundleOutput);
            AssetDatabase.Refresh();
        }
    }

    private void SelectAOTDlls()
    {
        SelectAssembly odinEditor = GetWindow<SelectAssembly>();
        odinEditor.Open();
    }

    private void PingObject(string assetPath)
    {
        Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
        EditorGUIUtility.PingObject(obj);
    }

    private void OpenOutFolder(string path)
    {
        OpenFolder.InternalOpenFolder(path);
    }

    private void GetValue()
    {
        BuildSettingsConfig = GeminiLionSetting.Instance.BuildSettingsConfig;
        ResourceCollectionConfig = GeminiLionSetting.Instance.ResourceCollectionConfig;
        ResourceEditorConfig = GeminiLionSetting.Instance.ResourceEditorConfig;
        ResourceBuilderConfig = GeminiLionSetting.Instance.ResourceBuilderConfig;
        AssetBundleOutput = GeminiLionSetting.Instance.AssetBundleOutput;
        PublishAppOutput = GeminiLionSetting.Instance.PublishAppOutput;
        BuildInfoPath = GeminiLionSetting.Instance.BuildInfoPath;
        PreloadInfoPath = GeminiLionSetting.Instance.PreloadInfoPath;
        EntityCodePath = GeminiLionSetting.Instance.EntityCodePath;
        HotfixEntityCodePath = GeminiLionSetting.Instance.HotfixEntityCodePath;
        UIFormCodePath = GeminiLionSetting.Instance.UIFormCodePath;
        HotfixUIFormCodePath = GeminiLionSetting.Instance.HotfixUIFormCodePath;
        EventCodePath = GeminiLionSetting.Instance.EventCodePath;
        HotfixEventCodePath = GeminiLionSetting.Instance.HotfixEventCodePath;
        ConfigExcelPath = GeminiLionSetting.Instance.ConfigExcelPath;
        ConfigDataPath = GeminiLionSetting.Instance.ConfigDataPath;
        DataTableExcelPath = GeminiLionSetting.Instance.DataTableExcelPath;
        DataTablePath = GeminiLionSetting.Instance.DataTablePath;
        DictionaryExclePath = GeminiLionSetting.Instance.DictionaryExclePath;
        DictionaryDataPath = GeminiLionSetting.Instance.DictionaryDataPath;
        CSharpCodePath = GeminiLionSetting.Instance.CSharpCodePath;
        CSharpCodeTemplateFileName = GeminiLionSetting.Instance.CSharpCodeTemplateFileName;
        HotfixDllPath = GeminiLionSetting.Instance.HotfixDllPath;
        HotfixDllNameMain = GeminiLionSetting.Instance.HotfixDllNameMain;
        AOTDllNames = GeminiLionSetting.Instance.AOTDllNames;
        PreserveHotfixDllNames = GeminiLionSetting.Instance.PreserveHotfixDllNames;
        HotfixDllSuffix = GeminiLionSetting.Instance.HotfixDllSuffix;
        HotfixInfoPath = GeminiLionSetting.Instance.HotfixInfoPath;
        HotfixLauncher = GeminiLionSetting.Instance.HotfixLauncher;
        CheckVersionUrl = GeminiLionSetting.Instance.CheckVersionUrl;
        WindowsAppUrl = GeminiLionSetting.Instance.WindowsAppUrl;
        MacOSAppUrl = GeminiLionSetting.Instance.MacOSAppUrl;
        IOSAppUrl = GeminiLionSetting.Instance.IOSAppUrl;
        AndroidAppUrl = GeminiLionSetting.Instance.AndroidAppUrl;
        UpdatePrefixUri = GeminiLionSetting.Instance.UpdatePrefixUri;
    }

    private void SaveValue()
    {
        GeminiLionSetting.Instance.BuildSettingsConfig = BuildSettingsConfig;
        GeminiLionSetting.Instance.ResourceCollectionConfig = ResourceCollectionConfig;
        GeminiLionSetting.Instance.ResourceEditorConfig = ResourceEditorConfig;
        GeminiLionSetting.Instance.ResourceBuilderConfig = ResourceBuilderConfig;
        GeminiLionSetting.Instance.AssetBundleOutput = AssetBundleOutput;
        GeminiLionSetting.Instance.PublishAppOutput = PublishAppOutput;
        GeminiLionSetting.Instance.BuildInfoPath = BuildInfoPath;
        GeminiLionSetting.Instance.PreloadInfoPath = PreloadInfoPath;
        GeminiLionSetting.Instance.EntityCodePath = EntityCodePath;
        GeminiLionSetting.Instance.HotfixEntityCodePath = HotfixEntityCodePath;
        GeminiLionSetting.Instance.UIFormCodePath = UIFormCodePath;
        GeminiLionSetting.Instance.HotfixUIFormCodePath = HotfixUIFormCodePath;
        GeminiLionSetting.Instance.EventCodePath = EventCodePath;
        GeminiLionSetting.Instance.HotfixEventCodePath = HotfixEventCodePath;
        GeminiLionSetting.Instance.ConfigExcelPath = ConfigExcelPath;
        GeminiLionSetting.Instance.ConfigDataPath = ConfigDataPath;
        GeminiLionSetting.Instance.DataTableExcelPath = DataTableExcelPath;
        GeminiLionSetting.Instance.DataTablePath = DataTablePath;
        GeminiLionSetting.Instance.DictionaryExclePath = DictionaryExclePath;
        GeminiLionSetting.Instance.DictionaryDataPath = DictionaryDataPath;
        GeminiLionSetting.Instance.CSharpCodePath = CSharpCodePath;
        GeminiLionSetting.Instance.CSharpCodeTemplateFileName = CSharpCodeTemplateFileName;
        GeminiLionSetting.Instance.HotfixDllPath = HotfixDllPath;
        GeminiLionSetting.Instance.HotfixDllNameMain = HotfixDllNameMain;
        GeminiLionSetting.Instance.AOTDllNames = AOTDllNames;
        GeminiLionSetting.Instance.PreserveHotfixDllNames = PreserveHotfixDllNames;
        GeminiLionSetting.Instance.HotfixDllSuffix = HotfixDllSuffix;
        GeminiLionSetting.Instance.HotfixInfoPath = HotfixInfoPath;
        GeminiLionSetting.Instance.HotfixLauncher = HotfixLauncher;
        GeminiLionSetting.Instance.CheckVersionUrl = CheckVersionUrl;
        GeminiLionSetting.Instance.WindowsAppUrl = WindowsAppUrl;
        GeminiLionSetting.Instance.MacOSAppUrl = MacOSAppUrl;
        GeminiLionSetting.Instance.IOSAppUrl = IOSAppUrl;
        GeminiLionSetting.Instance.AndroidAppUrl = AndroidAppUrl;
        GeminiLionSetting.Instance.UpdatePrefixUri = UpdatePrefixUri;
        GeminiLionSetting.Save();
    }
}
