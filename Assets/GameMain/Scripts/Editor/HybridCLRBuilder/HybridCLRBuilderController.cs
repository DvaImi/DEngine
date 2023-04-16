// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using System;
using System.IO;
using Dvalmi.Hotfix;
using GameFramework;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace Dvalmi.Editor
{
    public class HybridCLRBuilderController
    {
        public string[] PlatformNames { get; }

        public HybridCLRBuilderController()
        {
            PlatformNames = Enum.GetNames(typeof(Platform));
        }

        /// <summary>
        /// 由 UnityGameFramework.Editor.ResourceTools.Platform 得到 BuildTarget。
        /// </summary>
        /// <param name="platformIndex"></param>
        /// <returns>BuildTarget。</returns>
        public BuildTarget GetBuildTarget(int platformIndex)
        {
            Platform platform = (Platform)Enum.Parse(typeof(Platform), PlatformNames[platformIndex]);
            switch (platform)
            {
                case Platform.Windows:
                    return BuildTarget.StandaloneWindows;

                case Platform.Windows64:
                    return BuildTarget.StandaloneWindows64;

                case Platform.MacOS:
#if UNITY_2017_3_OR_NEWER
                    return BuildTarget.StandaloneOSX;
#else
                    return BuildTarget.StandaloneOSXUniversal;
#endif
                case Platform.Linux:
                    return BuildTarget.StandaloneLinux64;

                case Platform.IOS:
                    return BuildTarget.iOS;

                case Platform.Android:
                    return BuildTarget.Android;

                case Platform.WindowsStore:
                    return BuildTarget.WSAPlayer;

                case Platform.WebGL:
                    return BuildTarget.WebGL;

                default:
                    throw new GameFrameworkException("Platform is invalid.");
            }
        }

        /// <summary>
        /// 将 dll 文件拷贝至项目目录，用于 GameFramework 资源模块的编辑和打包。
        /// </summary>
        /// <param name="buildTarget"></param>
        public void CopyDllAssets(BuildTarget buildTarget)
        {
            IOUtility.CreateDirectoryIfNotExists(DvalmiSetting.Instance.HotfixDllPath);

            // Copy Hotfix Dll
            string oriFileName = Path.Combine(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget), DvalmiSetting.Instance.HotfixDllNameMain);
            string desFileName = Path.Combine(DvalmiSetting.Instance.HotfixDllPath, DvalmiSetting.Instance.HotfixDllNameMain + DvalmiSetting.Instance.HotfixDllSuffix);
            File.Copy(oriFileName, desFileName, true);

            // Copy AOT Dll
            string aotDllPath = SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget);
            foreach (var dllName in DvalmiSetting.Instance.AOTDllNames)
            {
                oriFileName = Path.Combine(aotDllPath, dllName);
                if (!File.Exists(oriFileName))
                {
                    Debug.LogError($"AOT 补充元数据 dll: {oriFileName} 文件不存在。需要构建一次主包后才能生成裁剪后的 AOT dll.");
                    continue;
                }
                desFileName = Path.Combine(DvalmiSetting.Instance.HotfixDllPath, dllName + DvalmiSetting.Instance.HotfixDllSuffix);
                File.Copy(oriFileName, desFileName, true);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
