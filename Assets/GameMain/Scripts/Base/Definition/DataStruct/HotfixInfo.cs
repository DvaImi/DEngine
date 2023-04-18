// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-15 23:39:33
// 版 本：1.0
// ========================================================

using GameFramework;
using Newtonsoft.Json;

public class HotfixInfo
{
    /// <summary>
    /// 热更程序集生成路径
    /// </summary>
    public string HotfixDllPath
    {
        get; set;
    }
    /// <summary>
    /// 主热更程序集
    /// </summary>
    public string HotfixDllNameMain
    {
        get; set;
    }

    /// <summary>
    /// AOT 程序集
    /// </summary>
    public string[] AOTDllNames
    {

        get;
        set;
    }

    /// <summary>
    /// 其他预留热更新程序集
    /// </summary>
    public string[] PreserveHotfixDllNames
    {
        get; set;
    }

    /// <summary>
    /// 热更程序集后缀
    /// </summary>
    public string HotfixDllSuffix
    {
        get; set;
    }

    /// <summary>
    /// 热更新启动对象路径
    /// </summary>
    public string HotfixLauncher
    {
        get;
        set;
    }

    [JsonIgnore]
    public string HotfixMainDllFullName
    {
        get => Utility.Path.GetRegularPath(System.IO.Path.Combine(HotfixDllPath, HotfixDllNameMain + HotfixDllSuffix));
    }
}
