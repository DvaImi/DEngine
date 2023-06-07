// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-15 19:31:59
// 版 本：1.0
// ========================================================
using System.IO;
using System.Text;
using Game;
using Game.Editor;
using Game.Editor.ResourceTools;
using HybridCLR.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using DFilePath = Sirenix.OdinInspector.FilePathAttribute;
public class GameSettingEditorWindows : OdinEditorWindow
{
    [MenuItem("Game/ Setting", priority = 20)]
    private static void OpenWindow()
    {
        var window = GetWindow<GameSettingEditorWindows>("Game Setting");
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
            AssetDatabase.Refresh();
        }
    }

    private void SelectAOTDlls()
    {
        SelectAssembly odinEditor = GetWindow<SelectAssembly>();
        odinEditor.Open();
        odinEditor.SetSaveCallBack((aot) =>
        {
            AOTDllNames = aot;
            SaveSetting();
        });
    }

    private void PingObject(string assetPath)
    {
        Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
        EditorGUIUtility.PingObject(obj);
    }

    private void GetValue()
    {
        BuildSettingsConfig = GameSetting.Instance.BuildSettingsConfig;
        ResourceCollectionConfig = GameSetting.Instance.ResourceCollectionConfig;
        ResourceEditorConfig = GameSetting.Instance.ResourceEditorConfig;
        ResourceBuilderConfig = GameSetting.Instance.ResourceBuilderConfig;
        BuildInfoPath = GameSetting.Instance.BuildInfoPath;
        PreloadInfoPath = GameSetting.Instance.PreloadInfoPath;
        HotfixDllPath = GameSetting.Instance.HotfixDllPath;
        HotfixDllNameMain = GameSetting.Instance.HotfixDllNameMain;
        AOTDllNames = GameSetting.Instance.AOTDllNames;
        PreserveHotfixDllNames = GameSetting.Instance.PreserveHotfixDllNames;
        HotfixDllSuffix = GameSetting.Instance.HotfixDllSuffix;
        HotfixInfoPath = GameSetting.Instance.HotfixInfoPath;
        HotfixLauncher = GameSetting.Instance.HotfixLauncher;
        CheckVersionUrl = GameSetting.Instance.CheckVersionUrl;
        WindowsAppUrl = GameSetting.Instance.WindowsAppUrl;
        MacOSAppUrl = GameSetting.Instance.MacOSAppUrl;
        IOSAppUrl = GameSetting.Instance.IOSAppUrl;
        AndroidAppUrl = GameSetting.Instance.AndroidAppUrl;
        UpdatePrefixUri = GameSetting.Instance.UpdatePrefixUri;
    }

    private void SaveValue()
    {
        GameSetting.Instance.BuildSettingsConfig = BuildSettingsConfig;
        GameSetting.Instance.ResourceCollectionConfig = ResourceCollectionConfig;
        GameSetting.Instance.ResourceEditorConfig = ResourceEditorConfig;
        GameSetting.Instance.ResourceBuilderConfig = ResourceBuilderConfig;
        GameSetting.Instance.BuildInfoPath = BuildInfoPath;
        GameSetting.Instance.PreloadInfoPath = PreloadInfoPath;
        GameSetting.Instance.HotfixDllPath = HotfixDllPath;
        GameSetting.Instance.HotfixDllNameMain = HotfixDllNameMain;
        GameSetting.Instance.AOTDllNames = AOTDllNames;
        GameSetting.Instance.PreserveHotfixDllNames = PreserveHotfixDllNames;
        GameSetting.Instance.HotfixDllSuffix = HotfixDllSuffix;
        GameSetting.Instance.HotfixInfoPath = HotfixInfoPath;
        GameSetting.Instance.HotfixLauncher = HotfixLauncher;
        GameSetting.Instance.CheckVersionUrl = CheckVersionUrl;
        GameSetting.Instance.WindowsAppUrl = WindowsAppUrl;
        GameSetting.Instance.MacOSAppUrl = MacOSAppUrl;
        GameSetting.Instance.IOSAppUrl = IOSAppUrl;
        GameSetting.Instance.AndroidAppUrl = AndroidAppUrl;
        GameSetting.Instance.UpdatePrefixUri = UpdatePrefixUri;
        GameSetting.Save();
        Debug.Log("Save success");
    }
}
