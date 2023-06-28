using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using DEngine.Editor.ResourceTools;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        public static void SaveBuildInfo()
        {
            //绑定到内置对象
            BuiltinDataComponent builtinDataComponent = Object.FindObjectOfType<BuiltinDataComponent>();
            if (builtinDataComponent != null)
            {
                Type type = typeof(BuiltinDataComponent);
                FieldInfo buildInfo = type.GetField("m_BuildInfo", BindingFlags.NonPublic | BindingFlags.Instance);
                buildInfo?.SetValue(builtinDataComponent, GameSetting.Instance.BuildInfo);
                EditorUtility.SetDirty(builtinDataComponent);
                EditorSceneManager.SaveOpenScenes();
            }
        }

        public static void BuildPlayer(bool aotGeneric)
        {
            SaveBuildInfo();
            BuildReport report = BuildApplication(GetBuildTarget(GameSetting.Instance.BuildPlatform), aotGeneric);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + StringUtility.GetByteLengthString((long)summary.totalSize));

                if (GameSetting.Instance.ForceUpdateGame)
                {
                    PutLastVserionApp(GetPlatform(GameSetting.Instance.BuildPlatform));
                }
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }

        public static BuildReport BuildApplication(BuildTarget platform, bool aotGeneric)
        {
            string outputExtension = GetFileExtensionForPlatform(platform);
            if (!Directory.Exists(GameSetting.Instance.AppOutput))
            {
                IOUtility.CreateDirectoryIfNotExists(GameSetting.Instance.AppOutput);
                GameSetting.Instance.SaveSetting();
            }

            string locationPath = Path.Combine(GameSetting.Instance.AppOutput, PlatformNames[GameSetting.Instance.BuildPlatform]);
            IOUtility.CreateDirectoryIfNotExists(locationPath);
            BuildPlayerOptions buildPlayerOptions = new()
            {
                scenes = new string[] { EditorBuildSettings.scenes[0].path },
                locationPathName = Path.Combine(locationPath, Application.productName + outputExtension),
                target = platform,
                options = BuildOptions.CompressWithLz4 | BuildOptions.ShowBuiltPlayer
            };

            if (aotGeneric)
            {
                buildPlayerOptions.options = BuildOptions.CompressWithLz4;
            }
            return UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        public static string GetFileExtensionForPlatform(BuildTarget platform)
        {
            return platform switch
            {
                BuildTarget.StandaloneWindows64 => ".exe",
                BuildTarget.StandaloneOSX => ".app",
                BuildTarget.Android => ".apk",
                BuildTarget.iOS => ".ipa",
                BuildTarget.WebGL => "",
                _ => ".exe",
            };
        }

        public static string GetBuildAppFullName()
        {
            return Path.Combine(GameSetting.Instance.AppOutput, PlatformNames[GameSetting.Instance.BuildPlatform], Application.productName + GetFileExtensionForPlatform(GetBuildTarget((int)m_OriginalPlatform)));
        }

        public static void PutLastVserionApp(Platform platform)
        {
            if (platform == Platform.Windows || platform == Platform.Windows64)
            {
                string virtualFilePath = GameSetting.Instance.VirtualServerAddress + "/" + platform.ToString() + "App/" + platform.ToString();
                IOUtility.CreateDirectoryIfNotExists(virtualFilePath);

                string sourceFileName = GameSetting.Instance.AppOutput + "/" + platform.ToString();
                if (!Directory.Exists(sourceFileName))
                {
                    return;
                }
                // 获取源路径的目录名作为压缩包名称
                string packageName = Application.productName + ".zip";
                string packagePath = Path.Combine(virtualFilePath, packageName);

                // 创建临时目录，用于保存过滤后的文件
                string tempPath = Path.Combine(Path.GetTempPath(), "PackageTemp");
                Directory.CreateDirectory(tempPath);

                // 复制源路径下的文件到临时目录，过滤掉指定文件夹
                IOUtility.CopyFiles(sourceFileName, tempPath, Application.productName + "_BackUpThisFolder_ButDontShipItWithYourGame");

                // 创建压缩包
                ZipFile.CreateFromDirectory(tempPath, packagePath);

                // 删除临时目录
                Directory.Delete(tempPath, true);

                Debug.Log($"Package '{packageName}' created at '{packagePath}'");
            }

            else if (platform == Platform.Android)
            {
                string virtualFilePath = GameSetting.Instance.VirtualServerAddress + "/" + platform.ToString() + "App/";
                IOUtility.CreateDirectoryIfNotExists(virtualFilePath);
                string sourceFileName = Path.Combine(GameSetting.Instance.AppOutput, platform.ToString(), Application.productName + ".apk");
                File.Copy(sourceFileName, virtualFilePath + "/" + Application.productName + ".apk", true);

                Debug.Log($"New Apk Put to '{sourceFileName}'");
            }
            else
            {
                Debug.Log("待扩展...");
            }
        }
    }
}