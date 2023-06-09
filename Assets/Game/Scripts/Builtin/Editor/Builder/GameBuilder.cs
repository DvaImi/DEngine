// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Game.Editor.ResourceTools;
using GameFramework.Resource;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditorInternal;
using UnityEngine;
using UnityGameFramework.Runtime;
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

        private bool m_BeginBuildPlayer = false;
        private bool m_BeginBuildResources = false;
        private bool m_IsAotGeneric = false;
        private bool m_FoldoutBuildConfigGroup = false;
        private bool m_FoldoutBuiltInfoGroup = false;
        private bool m_FoldoutSimulatorGroup = false;
        private bool m_FoldoutPatchAOTDllGroup = false;
        private Vector2 m_ScrollPosition;

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
            m_BeginBuildPlayer = false;
            m_ScrollPosition = Vector2.zero;
        }

        private void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false);
            {
                GUILayout.Space(5f);
                GUIPlatform();
                GUILayout.Space(5f);
                EditorGUILayout.BeginVertical("box");
                {
                    GUIHyBridCLR();
                }
                GUILayout.Space(5f);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5f);
                EditorGUILayout.BeginVertical("box");
                {
                    GUIResources();
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);
                EditorGUILayout.BeginVertical("box");
                {
                    GUIBuildPlayer();
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);
            }
            EditorGUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save"))
                {
                    GameSetting.Instance.SaveSetting();
                    Debug.Log("Save success");
                }
            }
            GUILayout.EndHorizontal();
        }

        private void Update()
        {
            if (m_BeginBuildPlayer)
            {
                m_BeginBuildPlayer = false;
                BuildPlayer(m_IsAotGeneric);
                m_IsAotGeneric = false;
            }

            if (m_BeginBuildResources)
            {
                m_BeginBuildResources = false;
                GameAssetBundleBuilder.BuildBundle();
            }
        }

        private void GUIPlatform()
        {
            GUILayout.Space(5f);
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Platform", EditorStyles.boldLabel);
                    int hotfixPlatformIndex = EditorGUILayout.Popup(HotfixPlatformIndex, m_HybridClrBuilderController.PlatformNames, GUILayout.Width(100));
                    if (hotfixPlatformIndex != m_HotfixPlatformIndex)
                    {
                        HotfixPlatformIndex = hotfixPlatformIndex;
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void GUIHyBridCLR()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("HyBridCLR", EditorStyles.boldLabel);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            {
                GameSetting.Instance.HotupdateDllPath = EditorGUILayout.TextField("HotUpdate Dll Path", GameSetting.Instance.HotupdateDllPath);
                Rect textFieldRect = GUILayoutUtility.GetLastRect();
                if (PathUtility.DropPath(textFieldRect, out string assetPath))
                {
                    if (assetPath != GameSetting.Instance.HotupdateDllPath)
                    {
                        GameSetting.Instance.HotupdateDllPath = assetPath;
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(GameSetting.Instance.HotupdateDllPath));
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            {
                GameSetting.Instance.HotUpdateAssemblyDefinition = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("HotUpdateAssembly", GameSetting.Instance.HotUpdateAssemblyDefinition, typeof(AssemblyDefinitionAsset), false);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            m_FoldoutPatchAOTDllGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutPatchAOTDllGroup, "PatchAOTDll");
            {
                if (m_FoldoutPatchAOTDllGroup)
                {
                    foreach (var item in GameSetting.Instance.AOTDllNames)
                    {
                        EditorGUILayout.TextField(item);
                    }

                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        if (GUILayout.Button("Editor"))
                        {
                            SelectAssembly odinEditor = GetWindow<SelectAssembly>();
                            odinEditor.Open();
                            odinEditor.SetSaveCallBack((aot) =>
                            {
                                GameSetting.Instance.AOTDllNames = aot;
                                GameSetting.Instance.SaveSetting();
                            });
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("AOT Generic", GUILayout.Height(30)))
            {
                m_IsAotGeneric = m_BeginBuildPlayer = true;
            }

            if (GUILayout.Button("Compile", GUILayout.Height(30)))
            {
                CompileHotfixDll();
            }
            GUI.backgroundColor = bc;
        }

        private void GUIResources()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Resources", EditorStyles.boldLabel);

                int resourceModeIndexEnum = GameSetting.Instance.ResourceModeIndex - 1;
                int resourceModeIndex = EditorGUILayout.Popup(resourceModeIndexEnum, GameAssetBundleBuilder.ResourceMode, GUILayout.Width(200));
                if (resourceModeIndex != resourceModeIndexEnum)
                {
                    //由于跳过了 ResourceMode.Unspecified 保存时索引+1
                    GameSetting.Instance.ResourceModeIndex = resourceModeIndex + 1;
                    GameSetting.Instance.SaveSetting();

                    ResourceComponent resourceComponent = FindObjectOfType<ResourceComponent>();
                    if (resourceComponent != null)
                    {
                        Type type = typeof(ResourceComponent);
                        FieldInfo resourceField = type.GetField("m_ResourceMode", BindingFlags.NonPublic | BindingFlags.Instance);
                        resourceField?.SetValue(resourceComponent, (ResourceMode)GameSetting.Instance.ResourceModeIndex);
                        EditorUtility.SetDirty(resourceComponent.gameObject);
                    }
                }

                if (GUILayout.Button("Edit", GUILayout.Width(100)))
                {
                    EditorWindow window = GetWindow(Type.GetType("Game.Editor.ResourceTools.AssetBundleCollector"));
                    window.minSize = new Vector2(1640f, 420f);
                    EditorUtility.ClearProgressBar();
                }

                if (GUILayout.Button("Clear", GUILayout.Width(100)))
                {
                    GameAssetBundleBuilder.ClearVersion();
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField(GameAssetBundleBuilder.BundlesOutput);
                GUI.enabled = true;
                bool exists = Directory.Exists(GameAssetBundleBuilder.BundlesOutput);
                if (GUILayout.Button(exists ? EditorGUIUtility.IconContent("Project") : EditorGUIUtility.IconContent("console.erroricon"), new GUIStyle() { imagePosition = ImagePosition.ImageLeft }, GUILayout.Width(20)))
                {
                    string value = EditorUtility.OpenFolderPanel("AssetBundle Output", "", "Output");
                    if (Directory.Exists(value) && value != GameAssetBundleBuilder.BundlesOutput)
                    {
                        GameAssetBundleBuilder.BundlesOutput = value;
                    }
                    GameAssetBundleBuilder.SaveOutputDirectory(GameAssetBundleBuilder.BundlesOutput);
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    UnityGameFramework.Editor.OpenFolder.Execute(GameAssetBundleBuilder.BundlesOutput);
                }
            }
            EditorGUILayout.EndHorizontal();


            GUILayout.Space(5f);
            EditorGUILayout.BeginVertical("box");
            {
                m_FoldoutBuildConfigGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutBuildConfigGroup, "BuildConfig");

                if (m_FoldoutBuildConfigGroup)
                {
                    PathUtility.DropAssetPath("BuildSetting", ref GameSetting.Instance.BuildSettingsConfig);
                    PathUtility.DropAssetPath("ResourceCollectionConfig", ref GameSetting.Instance.ResourceCollectionConfig);
                    PathUtility.DropAssetPath("ResourceEditorConfig", ref GameSetting.Instance.ResourceEditorConfig);
                    PathUtility.DropAssetPath("ResourceBuilderConfig", ref GameSetting.Instance.ResourceBuilderConfig);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                GUILayout.Space(5f);
                m_FoldoutBuiltInfoGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutBuiltInfoGroup, "BuildInfo");
                if (m_FoldoutBuiltInfoGroup)
                {
                    GameSetting.Instance.ForceUpdateGame = EditorGUILayout.Toggle("强制更新应用", GameSetting.Instance.ForceUpdateGame);
                    GameSetting.Instance.CheckVersionUrl = EditorGUILayout.TextField("版本检查地址", GameSetting.Instance.CheckVersionUrl);
                    GameSetting.Instance.UpdatePrefixUri = EditorGUILayout.TextField("资源更新地址", GameSetting.Instance.UpdatePrefixUri);
                    GameSetting.Instance.WindowsAppUrl = EditorGUILayout.TextField("Windows下载应用地址", GameSetting.Instance.WindowsAppUrl);
                    GameSetting.Instance.AndroidAppUrl = EditorGUILayout.TextField("Android应用下载地址", GameSetting.Instance.AndroidAppUrl);
                    GameSetting.Instance.MacOSAppUrl = EditorGUILayout.TextField("MacOS下载应用地址", GameSetting.Instance.MacOSAppUrl);
                    GameSetting.Instance.IOSAppUrl = EditorGUILayout.TextField("IOS下载应用地址", GameSetting.Instance.IOSAppUrl);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                GUILayout.Space(5f);
                m_FoldoutSimulatorGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutSimulatorGroup, "Simulator");
                if (m_FoldoutSimulatorGroup)
                {
                    GameSetting.Instance.AutoCopyToVirtualServer = EditorGUILayout.Toggle("开启自动拷贝资源", GameSetting.Instance.AutoCopyToVirtualServer);
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUI.enabled = false;
                        EditorGUILayout.LabelField("本地虚拟服务器地址", GameSetting.Instance.VirtualServerAddress);
                        GUI.enabled = true;
                        bool exists = Directory.Exists(GameSetting.Instance.VirtualServerAddress);
                        if (GUILayout.Button(exists ? EditorGUIUtility.IconContent("Project") : EditorGUIUtility.IconContent("console.erroricon"), new GUIStyle() { imagePosition = ImagePosition.ImageOnly }, GUILayout.Width(20)))
                        {
                            string value = EditorUtility.OpenFolderPanel("VirtualServerAddress", "", "ServerAddress");
                            if (Directory.Exists(value) && value != GameSetting.Instance.VirtualServerAddress)
                            {
                                GameSetting.Instance.VirtualServerAddress = value;
                            }
                        }
                        if (GUILayout.Button("Go", GUILayout.Width(30)))
                        {
                            UnityGameFramework.Editor.OpenFolder.Execute(GameSetting.Instance.VirtualServerAddress);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            EditorGUILayout.EndVertical();

            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            GUILayout.Space(5f);
            if (GUILayout.Button("Build", GUILayout.Height(30)))
            {
                m_BeginBuildResources = true;
            }
            GUI.backgroundColor = bc;
        }

        private void GUIBuildPlayer()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("BuildPlayer", EditorStyles.boldLabel);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField(GameAssetBundleBuilder.AppOutput);
                GUI.enabled = true;
                bool exists = Directory.Exists(GameAssetBundleBuilder.AppOutput);
                if (GUILayout.Button(exists ? EditorGUIUtility.IconContent("Project") : EditorGUIUtility.IconContent("console.erroricon"), new GUIStyle() { imagePosition = ImagePosition.ImageOnly }, GUILayout.Width(20)))
                {
                    string value = EditorUtility.OpenFolderPanel("PublishApp Output", "", "Output");
                    if (Directory.Exists(value) && value != GameAssetBundleBuilder.AppOutput)
                    {
                        GameAssetBundleBuilder.AppOutput = value;
                    }
                }
                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    UnityGameFramework.Editor.OpenFolder.Execute(GameAssetBundleBuilder.AppOutput);
                }
            }
            EditorGUILayout.EndHorizontal();
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Build", GUILayout.Height(30)))
            {
                m_BeginBuildPlayer = true;
            }
            GUI.backgroundColor = bc;
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
                    UnityGameFramework.Editor.OpenFolder.Execute(GameAssetBundleBuilder.AppOutput);
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
            if (!Directory.Exists(GameAssetBundleBuilder.AppOutput))
            {
                IOUtility.CreateDirectoryIfNotExists(GameAssetBundleBuilder.AppOutput);
                GameSetting.Instance.SaveSetting();
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = aot ? EditorBuildSettings.scenes.Select(x => x.path).ToArray() : new string[] { EditorBuildSettings.scenes[0].path },
                locationPathName = Path.Combine(GameAssetBundleBuilder.AppOutput, m_HybridClrBuilderController.PlatformNames[HotfixPlatformIndex], Application.productName + outputExtension),
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

        private void CompileHotfixDll()
        {
            BuildTarget buildTarget = m_HybridClrBuilderController.GetBuildTarget(m_HotfixPlatformIndex);
            CompileDllCommand.CompileDll(buildTarget);
            CopyDllAssets(buildTarget);
        }

        private void CopyDllAssets(BuildTarget buildTarget)
        {
            if (string.IsNullOrEmpty(GameSetting.Instance.HotupdateDllPath))
            {
                Debug.LogError("Directory path is null.");
                return;
            }

            if (!Directory.Exists(GameSetting.Instance.HotupdateDllPath))
            {
                Directory.CreateDirectory(GameSetting.Instance.HotupdateDllPath);
            }
            else
            {
                string[] files = AssetDatabase.FindAssets("", new string[] { GameSetting.Instance.HotupdateDllPath });

                foreach (string file in files)
                {
                    string filePath = AssetDatabase.GUIDToAssetPath(file);
                    AssetDatabase.DeleteAsset(filePath);
                }

                AssetDatabase.Refresh();
            }

            string hotUpdateAssemblyDefinitionFullName = GameSetting.Instance.HotUpdateAssemblyDefinition.name + ".dll";
            //Copy Hotfix Dll
            string oriFileName = Path.Combine(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget), hotUpdateAssemblyDefinitionFullName);

            //加bytes 后缀让Unity识别为TextAsset 文件
            string desFileName = Path.Combine(GameSetting.Instance.HotupdateDllPath, hotUpdateAssemblyDefinitionFullName + ".bytes");
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
                desFileName = Path.Combine(GameSetting.Instance.HotupdateDllPath, dllName + ".bytes");
                File.Copy(oriFileName, desFileName, true);
            }

            Debug.Log("Copy Aot dll success.");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}