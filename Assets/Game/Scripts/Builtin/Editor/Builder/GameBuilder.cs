// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Game.Editor.ResourceTools;
using GameFramework;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Object = UnityEngine.Object;
using Path = System.IO.Path;

namespace Game.Editor
{
    public class GameBuilder : EditorWindow
    {
        private HybridCLRBuilderController m_HybridClrBuilderController;
        private int m_HotfixPlatformIndex;

        private int HotfixPlatformIndex
        {
            get => EditorPrefs.GetInt("BuildPlatform", 2);
            set
            {
                m_HotfixPlatformIndex = value;
                EditorPrefs.SetInt("BuildPlatform", m_HotfixPlatformIndex);
            }
        }

        private string PublishAppOutput
        {
            get => EditorPrefs.GetString("PublishAppOutput");
            set => EditorPrefs.SetString("PublishAppOutput", value);
        }

        private string AssetBundleOutput
        {
            get => EditorPrefs.GetString("AssetBundleOutput");
            set => EditorPrefs.SetString("AssetBundleOutput", value);
        }

        private bool m_BeginBuild = false;
        private bool m_IsAotGeneric = false;

        [MenuItem("Game/Builder", false, 0)]
        private static void Open()
        {
            GameBuilder window = GetWindow<GameBuilder>("Builder", true);
            window.minSize = new Vector2(800f, 300f);
        }

        private void OnEnable()
        {
            m_HybridClrBuilderController = new HybridCLRBuilderController();
            m_HotfixPlatformIndex = HotfixPlatformIndex;
            m_BeginBuild = false;
        }

        private void OnGUI()
        {
            // Builder
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);
            GUILayout.Space(5f);
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("打包平台设置", EditorStyles.boldLabel);
                    int hotfixPlatformIndex = EditorGUILayout.Popup(m_HotfixPlatformIndex, m_HybridClrBuilderController.PlatformNames, GUILayout.Width(100));
                    if (hotfixPlatformIndex != m_HotfixPlatformIndex)
                    {
                        HotfixPlatformIndex = hotfixPlatformIndex;
                    }

                    if (GUILayout.Button("AOT Generic", GUILayout.Width(100)))
                    {
                        m_IsAotGeneric = m_BeginBuild = true;
                    }

                    if (GUILayout.Button("Hotfix", GUILayout.Width(100)))
                    {
                        CompileHotfixDll();
                    }
                }
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(5f);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            EditorGUILayout.BeginVertical("box");
            {
                GUIResourcesTool();
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(5f);
            EditorGUILayout.BeginVertical("box");
            {
                GUIBuildPlayer();
            }
            EditorGUILayout.EndVertical();
        }

        private void Update()
        {
            if (m_BeginBuild)
            {
                m_BeginBuild = false;
                BuildPlayer(m_IsAotGeneric);
                m_IsAotGeneric = false;
            }
        }

        private void GUIResourcesTool()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("资源", EditorStyles.boldLabel);

                if (GUILayout.Button("Assress", GUILayout.Width(100)))
                {
                    AssetBundleUtility.WriteAddress();
                }
                if (GUILayout.Button("Builtin", GUILayout.Width(100)))
                {
                    AssetBundleUtility.GenerateBuiltin();
                }
                if (GUILayout.Button("Edit", GUILayout.Width(100)))
                {
                    EditorWindow window = GetWindow(Type.GetType("Game.Editor.ResourceTools.AssetBundleCollector"));
                    window.minSize = new Vector2(1640f, 420f);
                }
            }

            EditorGUILayout.EndHorizontal();
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            GUILayout.Space(5f);
            if (GUILayout.Button("Build", GUILayout.Height(30)))
            {
                AssetBundleUtility.StartBuild();
            }
            GUI.backgroundColor = bc;
            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField(AssetBundleOutput);
                GUI.enabled = true;
                bool exists = Directory.Exists(AssetBundleOutput);
                if (GUILayout.Button(exists ? EditorGUIUtility.IconContent("Project") : EditorGUIUtility.IconContent("console.erroricon"), new GUIStyle() { imagePosition = ImagePosition.ImageOnly }, GUILayout.Width(20)))
                {
                    string value = EditorUtility.OpenFolderPanel("AssetBundle Output", "", "Output");
                    if (Directory.Exists(value) && value != AssetBundleOutput)
                    {
                        AssetBundleOutput = value;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void GUIBuildPlayer()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField($"构建{m_HybridClrBuilderController.PlatformNames[HotfixPlatformIndex]}工程。", EditorStyles.boldLabel);
            }
            EditorGUILayout.EndHorizontal();
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Build", GUILayout.Height(30)))
            {
                m_BeginBuild = true;
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField(PublishAppOutput);
                GUI.enabled = true;
                bool exists = Directory.Exists(PublishAppOutput);
                if (GUILayout.Button(exists ? EditorGUIUtility.IconContent("Project") : EditorGUIUtility.IconContent("console.erroricon"), new GUIStyle() { imagePosition = ImagePosition.ImageOnly }, GUILayout.Width(20)))
                {
                    string value = EditorUtility.OpenFolderPanel("PublishApp Output", "", "Output");
                    if (Directory.Exists(value) && value != PublishAppOutput)
                    {
                        PublishAppOutput = value;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CompileHotfixDll()
        {
            BuildTarget buildTarget = m_HybridClrBuilderController.GetBuildTarget(m_HotfixPlatformIndex);
            CompileDllCommand.CompileDll(buildTarget);
            CopyDllAssets(buildTarget);
        }

        private void BuildPlayer(bool aot = false)
        {
            BuildReport report = BuildApplicationForPlatform(m_HybridClrBuilderController.GetBuildTarget(m_HotfixPlatformIndex), aot);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + StringUtility.GetByteLengthString((long)summary.totalSize));

                if (!aot)
                {
                    OpenFolder.OpenFolderPublishAppPath();
                }
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }

        public BuildReport BuildApplicationForPlatform(BuildTarget platform, bool aot)
        {
            string outputExtension = GetFileExtensionForPlatform(platform);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = aot ? EditorBuildSettings.scenes.Select(x => x.path).ToArray() : new string[] { EditorBuildSettings.scenes[0].path },
                locationPathName = Path.Combine(PublishAppOutput, m_HybridClrBuilderController.PlatformNames[HotfixPlatformIndex], Application.productName + outputExtension),
                target = platform,
                options = BuildOptions.None
            };

            return BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        private string GetFileExtensionForPlatform(BuildTarget platform)
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

        private void CopyDllAssets(BuildTarget buildTarget)
        {
            if (string.IsNullOrEmpty(GameSetting.Instance.HotfixDllPath))
            {
                Debug.LogError("Directory path is null.");
                return;
            }

            if (!Directory.Exists(GameSetting.Instance.HotfixDllPath))
            {
                Directory.CreateDirectory(GameSetting.Instance.HotfixDllPath);
            }
            else
            {
                string[] files = AssetDatabase.FindAssets("", new string[] { GameSetting.Instance.HotfixDllPath });

                foreach (string file in files)
                {
                    string filePath = AssetDatabase.GUIDToAssetPath(file);
                    AssetDatabase.DeleteAsset(filePath);
                }

                AssetDatabase.Refresh();
            }

            //Copy Hotfix Dll
            string oriFileName = Path.Combine(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget), GameSetting.Instance.HotfixDllNameMain);
            string desFileName = Path.Combine(GameSetting.Instance.HotfixDllPath, GameSetting.Instance.HotfixDllNameMain + GameSetting.Instance.HotfixDllSuffix);
            File.Copy(oriFileName, desFileName, true);
            Debug.Log("Copy hotfix dll success.");

            // Copy AOT Dll
            string aotDllPath = SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget);
            foreach (var dllName in GameSetting.Instance.AOTDllNames)
            {
                oriFileName = Path.Combine(aotDllPath, dllName);
                if (!File.Exists(oriFileName))
                {
                    Debug.LogError($"AOT 补充元数据 dll: {oriFileName} 文件不存在。需要构建一次主包后才能生成裁剪后的 AOT dll.");
                    continue;
                }
                desFileName = Path.Combine(GameSetting.Instance.HotfixDllPath, dllName + GameSetting.Instance.HotfixDllSuffix);
                File.Copy(oriFileName, desFileName, true);
            }

            Debug.Log("Copy Aot dll success.");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}