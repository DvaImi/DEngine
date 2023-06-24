// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using System.IO;
using Game.Editor.ResourceTools;
using DEngine;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;
using Path = System.IO.Path;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Game.Editor.BuildPipeline
{
    public class BuildPipelineWindow : EditorWindow
    {
        private bool m_BeginBuildPlayer = false;
        private bool m_BeginBuildResources = false;
        private bool m_IsAotGeneric = false;
        private bool m_FoldoutBuildConfigGroup = false;
        private bool m_FoldoutBuiltInfoGroup = false;
        private bool m_FoldoutSimulatorGroup = false;
        private bool m_FoldoutPatchAOTDllGroup = false;
        private Vector2 m_ScrollPosition;

        [MenuItem("Game/BuildPipeline", false, 0)]
        private static void Open()
        {
            BuildPipelineWindow window = GetWindow<BuildPipelineWindow>("BuildPipeline", true);
            window.minSize = new Vector2(800f, 300f);
        }

        private void OnEnable()
        {
            m_BeginBuildPlayer = false;
            m_IsAotGeneric = false;
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
                    BuildPipeline.SaveBuildInfo();
                    Debug.Log("Save success");
                }
            }
            GUILayout.EndHorizontal();

            if (GUI.changed)
            {
                GameSetting.Instance.SaveSetting();
                Repaint();
            }
        }

        private void Update()
        {
            if (m_BeginBuildPlayer)
            {
                m_BeginBuildPlayer = false;
                BuildPlayer();
            }

            if (m_BeginBuildResources)
            {
                m_BeginBuildResources = false;
                BuildPipeline.BuildBundle(GameSetting.Instance.Difference);
            }

            if (m_IsAotGeneric)
            {
                m_IsAotGeneric = false;
                BuildPlayer(false);
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
                    int hotfixPlatformIndex = EditorGUILayout.Popup(GameSetting.Instance.BuildPlatform, BuildPipeline.PlatformNames, GUILayout.Width(100));

                    if (hotfixPlatformIndex != GameSetting.Instance.BuildPlatform)
                    {
                        GameSetting.Instance.BuildPlatform = hotfixPlatformIndex;
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
                Rect hotUpdateRect = GUILayoutUtility.GetLastRect();
                if (PathUtility.DropPath(hotUpdateRect, out string hotDatePath))
                {
                    if (hotDatePath != GameSetting.Instance.HotupdateDllPath)
                    {
                        GameSetting.Instance.HotupdateDllPath = hotDatePath;
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
            EditorGUILayout.BeginHorizontal();
            {
                GameSetting.Instance.AOtDllPath = EditorGUILayout.TextField("AOT Dll Path", GameSetting.Instance.AOtDllPath);
                Rect aotPathRect = GUILayoutUtility.GetLastRect();
                if (PathUtility.DropPath(aotPathRect, out string aotPath))
                {
                    if (aotPath != GameSetting.Instance.AOtDllPath)
                    {
                        GameSetting.Instance.AOtDllPath = aotPath;
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(GameSetting.Instance.AOtDllPath));
                }
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
                            SelectAssembly assemblyEditor = GetWindow<SelectAssembly>();

                            void Save(string[] aotdll)
                            {
                                GameSetting.Instance.AOTDllNames = aotdll;
                                GameSetting.Instance.SaveSetting();
                                Repaint();
                            }

                            bool WherePredicate(Assembly assembly)
                            {
                                return !assembly.FullName.Contains("Editor");
                            }
                            HashSet<string> hasSelect = new(GameSetting.Instance.AOTDllNames.Select(item => item.Replace(".dll", null)));
                            assemblyEditor.Open(hasSelect, Save, WherePredicate);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("AOT Generic"))
                {
                    m_IsAotGeneric = true;
                }

                if (GUILayout.Button("Compile"))
                {
                    CompileHotfixDll();
                }
                GUI.backgroundColor = bc;
            }
            EditorGUILayout.EndHorizontal();
        }


        private void GUIResources()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Resources", EditorStyles.boldLabel);

                GUI.enabled = BuildPipeline.CanDifference();
                GameSetting.Instance.Difference = EditorGUILayout.ToggleLeft("Difference", GameSetting.Instance.Difference, GUILayout.Width(120));
                GUI.enabled = true;
                int resourceModeIndexEnum = GameSetting.Instance.ResourceModeIndex - 1;
                int resourceModeIndex = EditorGUILayout.Popup(resourceModeIndexEnum, BuildPipeline.ResourceMode, GUILayout.Width(160));
                if (resourceModeIndex != resourceModeIndexEnum)
                {
                    //由于跳过了 ResourceMode.Unspecified 保存时索引+1
                    GameSetting.Instance.ResourceModeIndex = resourceModeIndex + 1;
                    GameSetting.Instance.SaveSetting();
                }
                if (GUILayout.Button("Edit", GUILayout.Width(100)))
                {
                    EditorWindow window = GetWindow<AssetBundleCollectorWindow>(false, "AssetBundleCollector");
                    window.minSize = new Vector2(1640f, 420f);
                    window.Show();
                    EditorUtility.ClearProgressBar();
                }

                if (GUILayout.Button("Clear", GUILayout.Width(100)))
                {
                    BuildPipeline.ClearBundles();
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField(GameSetting.Instance.BundlesOutput);
                GUI.enabled = true;

                if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
                {
                    string directory = EditorUtility.OpenFolderPanel("Select Output Directory", GameSetting.Instance.BundlesOutput, string.Empty);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        if (Directory.Exists(directory) && directory != GameSetting.Instance.BundlesOutput)
                        {
                            GameSetting.Instance.BundlesOutput = directory;
                        }
                        BuildPipeline.SaveOutputDirectory(GameSetting.Instance.BundlesOutput);
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    DEngine.Editor.OpenFolder.Execute(GameSetting.Instance.BundlesOutput);
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
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GameSetting.Instance.ForceUpdateGame)
                        {
                            GUI.enabled = false;
                            EditorGUILayout.LabelField($"强制更新应用将以{GameSetting.Instance.LatestGameVersion} 为最后一个版本号");
                            GUI.enabled = true;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    GameSetting.Instance.UpdatePrefixUri = EditorGUILayout.TextField("资源更新地址", GameSetting.Instance.UpdatePrefixUri);
                    GUI.enabled = false;
                    GameSetting.Instance.InternalResourceVersion = EditorGUILayout.IntField("内置资源版本", GameSetting.Instance.InternalResourceVersion);
                    GameSetting.Instance.LatestGameVersion = EditorGUILayout.TextField("最新的游戏版本号", Application.version);
                    GUI.enabled = true;
                    GameSetting.Instance.BuildInfo.CheckVersionUrl = EditorGUILayout.TextField("版本检查文件地址", GameSetting.Instance.BuildInfo.CheckVersionUrl);
                    GameSetting.Instance.BuildInfo.WindowsAppUrl = EditorGUILayout.TextField("Windows下载应用地址", GameSetting.Instance.BuildInfo.WindowsAppUrl);
                    GameSetting.Instance.BuildInfo.AndroidAppUrl = EditorGUILayout.TextField("Android应用下载地址", GameSetting.Instance.BuildInfo.AndroidAppUrl);
                    GameSetting.Instance.BuildInfo.MacOSAppUrl = EditorGUILayout.TextField("MacOS下载应用地址", GameSetting.Instance.BuildInfo.MacOSAppUrl);
                    GameSetting.Instance.BuildInfo.IOSAppUrl = EditorGUILayout.TextField("IOS下载应用地址", GameSetting.Instance.BuildInfo.IOSAppUrl);
                    GameSetting.Instance.BuildInfo.LatestGameVersion = GameSetting.Instance.LatestGameVersion;
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                GUILayout.Space(5f);
                m_FoldoutSimulatorGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutSimulatorGroup, "Simulator");
                if (m_FoldoutSimulatorGroup)
                {
                    GUI.enabled = GameSetting.Instance.ResourceModeIndex > 1;
                    GameSetting.Instance.AutoCopyToVirtualServer = EditorGUILayout.Toggle("开启自动拷贝资源", GameSetting.Instance.AutoCopyToVirtualServer);
                    if (GameSetting.Instance.ResourceModeIndex <= 1)
                    {
                        GameSetting.Instance.AutoCopyToVirtualServer = false;
                    }
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("本地虚拟服务器地址", GameSetting.Instance.VirtualServerAddress);
                        GUI.enabled = true;
                        if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
                        {
                            string directory = EditorUtility.OpenFolderPanel("Select Output Directory", GameSetting.Instance.VirtualServerAddress, string.Empty);
                            if (!string.IsNullOrEmpty(directory))
                            {
                                if (Directory.Exists(directory) && directory != GameSetting.Instance.VirtualServerAddress)
                                {
                                    GameSetting.Instance.VirtualServerAddress = directory;
                                }
                            }
                        }
                        if (GUILayout.Button("Go", GUILayout.Width(30)))
                        {
                            DEngine.Editor.OpenFolder.Execute(GameSetting.Instance.VirtualServerAddress);
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
                EditorGUILayout.LabelField(GameSetting.Instance.AppOutput);
                GUI.enabled = true;

                if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
                {
                    string directory = EditorUtility.OpenFolderPanel("Select Output Directory", GameSetting.Instance.AppOutput, string.Empty);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        if (Directory.Exists(directory) && directory != GameSetting.Instance.AppOutput)
                        {
                            GameSetting.Instance.AppOutput = directory;
                        }
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    DEngine.Editor.OpenFolder.Execute(GameSetting.Instance.AppOutput);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                string locationPathName = BuildPipeline.GetBuildAppFullName();
                if (File.Exists(locationPathName))
                {
                    GUI.enabled = false;
                    EditorGUILayout.LabelField(locationPathName);
                    GUI.enabled = true;

                    if (GUILayout.Button("Run", GUILayout.Width(100)))
                    {
                        System.Diagnostics.Process.Start(locationPathName);
                    }
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

        private void BuildPlayer(bool completeOpenFolder = true)
        {
            BuildPipeline.SaveBuildInfo();
            BuildReport report = BuildPipeline.BuildApplication(BuildPipeline.GetBuildTarget(GameSetting.Instance.BuildPlatform));
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + StringUtility.GetByteLengthString((long)summary.totalSize));
                if (completeOpenFolder)
                {
                    DEngine.Editor.OpenFolder.Execute(GameSetting.Instance.AppOutput);
                }

                if (GameSetting.Instance.ForceUpdateGame)
                {
                    BuildPipeline.PutLastVserionApp(BuildPipeline.GetPlatform(GameSetting.Instance.BuildPlatform));
                }
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }

        private void CompileHotfixDll()
        {
            BuildTarget buildTarget = BuildPipeline.GetBuildTarget(GameSetting.Instance.BuildPlatform);
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

            if (Directory.Exists(GameSetting.Instance.HotupdateDllPath))
            {
                IOUtility.Delete(GameSetting.Instance.HotupdateDllPath);
            }
            else
            {
                Directory.CreateDirectory(GameSetting.Instance.HotupdateDllPath);
            }

            if (Directory.Exists(GameSetting.Instance.AOtDllPath))
            {
                IOUtility.Delete(GameSetting.Instance.AOtDllPath);
            }
            else
            {
                Directory.CreateDirectory(GameSetting.Instance.AOtDllPath);
            }


            string hotUpdateAssemblyDefinitionFullName = GameSetting.Instance.HotUpdateAssemblyDefinition.name + ".dll";
            //Copy Hotfix Dll
            string oriFileName = Path.Combine(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget), hotUpdateAssemblyDefinitionFullName);

            //加bytes 后缀让Unity识别为TextAsset 文件
            string desFileName = Path.Combine(GameSetting.Instance.HotupdateDllPath, hotUpdateAssemblyDefinitionFullName + ".bytes");
            File.Copy(oriFileName, desFileName, true);
            string updataDllMainfest = Path.Combine(GameSetting.Instance.HotupdateDllPath, "UpdataDllMainfest" + ".bytes");
            GameMainfestUitlity.CreatMainfest(hotUpdateAssemblyDefinitionFullName, updataDllMainfest);
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
                desFileName = Path.Combine(GameSetting.Instance.AOtDllPath, dllName + ".bytes");
                File.Copy(oriFileName, desFileName, true);
            }

            Debug.Log("Copy Aot dll success.");

            string aotMainfest = Path.Combine(GameSetting.Instance.AOtDllPath, "AOTMetadataMainfest" + ".bytes");
            GameMainfestUitlity.CreatMainfest(GameSetting.Instance.AOTDllNames, aotMainfest);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}