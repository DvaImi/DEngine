// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-05 09:00:33
// 版 本：1.0
// ========================================================
using UnityEngine;

[CreateAssetMenu(fileName = "GameFrameworkSettings", menuName = "Dvalmi/GameFrameworkSettings")]
public class GameFrameworkSettings : ScriptableObject
{
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

    [Space]
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

    [Space]
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

    [Space]
    /// <summary>
    /// 游戏配置路径
    /// </summary>
    public string ConfigPath = "Assets/GameMain/Configs";

    /// <summary>
    /// 游戏数据表路径
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
    /// 数据表
    /// </summary>
    public string DataRowClassPrefixName = "Dvalmi.DR";
    /// <summary>
    /// 
    /// </summary>
    public string DataRowClassHotfixPrefixName = "Dvalmi.Hotfix.DR";

    [Space]
    /// <summary>
    /// 热更程序集生成路径
    /// </summary>
    public string HotfixDllPath = "Assets/GameMain/HybridCLR/Dlls";
    /// <summary>
    /// 主热更程序集
    /// </summary>
    public string HotfixDllNameMain = "Game.Hotfix.dll";
    /// <summary>
    /// 其他预留热更新程序集
    /// </summary>
    public string[] PreserveHotfixDllNames;
    /// <summary>
    /// 热更程序集后缀
    /// </summary>
    public string HotfixDllSuffix = ".bytes";
    [Space]
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

    [Space]
    /// <summary>
    /// 下载资源接口
    /// </summary>
    public string UpdatePrefixUri = "http://192.168.1.102/{0}_{1}/{2}";
}
