// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Editor.BuildPipeline
{
    public class GameBuildPipelineWindow : EditorWindow
    {
        private bool m_BeginBuildPlayer = false;
        private bool m_BeginBuildResources = false;
        private bool m_IsAotGeneric = false;
        private bool m_EnableHybridCLR = false;
        private bool m_FoldoutBuildConfigGroup = false;
        private bool m_FoldoutBuiltInfoGroup = false;
        private bool m_FoldoutFileServerGroup = false;
        private bool m_FoldoutHotUpdateAssembliesGroup = false;
        private bool m_FoldoutPreserveAssembliesGroup = false;
        private bool m_FoldoutPatchAOTAssembliesGroup = false;
        private Vector2 m_ScrollPosition;

        [MenuItem("Game/Build Pipeline", false, 0)]
        private static void Open()
        {
            GameBuildPipelineWindow window = GetWindow<GameBuildPipelineWindow>("BuildPipeline", true);
            window.minSize = new Vector2(800f, 600f);
        }

        private void OnEnable()
        {
            m_BeginBuildPlayer = false;
            m_IsAotGeneric = false;
            m_EnableHybridCLR = SettingsUtil.Enable;
            m_ScrollPosition = Vector2.zero;

            if (!Directory.Exists(GameSetting.Instance.AppOutput))
            {
                Directory.CreateDirectory(GameSetting.Instance.AppOutput);
            }

            if (!Directory.Exists(GameSetting.Instance.BundlesOutput))
            {
                Directory.CreateDirectory(GameSetting.Instance.BundlesOutput);
            }

            GameBuildPipeline.RefreshPackages();
            GameBuildPipeline.CheckEnableHybridCLR();
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
                    SaveAll();
                }
            }
            GUILayout.EndHorizontal();

            if (GUI.changed)
            {
                GameSetting.Instance.SaveSetting();
            }
        }

        private void Update()
        {
            if (m_BeginBuildPlayer)
            {
                m_BeginBuildPlayer = false;
                GameBuildPipeline.BuildPlayer(false);
            }

            if (m_BeginBuildResources)
            {
                m_BeginBuildResources = false;
                GameBuildPipeline.BuildBundle(GameSetting.Instance.BundlesOutput, GameSetting.Instance.Difference);
            }

            if (m_IsAotGeneric)
            {
                m_IsAotGeneric = false;
                HybridCLR.Editor.Commands.Il2CppDefGeneratorCommand.GenerateIl2CppDef();
                HybridCLR.Editor.Commands.LinkGeneratorCommand.GenerateLinkXml();
                HybridCLR.Editor.Commands.StripAOTDllCommand.GenerateStripedAOTDlls();
                HybridCLR.Editor.Commands.MethodBridgeGeneratorCommand.CompileAndGenerateMethodBridge();
                HybridCLR.Editor.Commands.AOTReferenceGeneratorCommand.CompileAndGenerateAOTGenericReference();
            }
        }

        private void SaveAll()
        {
            GameSetting.Instance.SaveSetting();
            GameBuildPipeline.SaveBuildInfo();
            GameBuildPipeline.SaveHybridCLR();
            GameBuildPipeline.SaveResource();
        }

        private void GUIPlatform()
        {
            GUILayout.Space(5f);
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Platform", EditorStyles.boldLabel);
                    int hotfixPlatformIndex = EditorGUILayout.Popup(GameSetting.Instance.BuildPlatform, GameBuildPipeline.PlatformNames, GUILayout.Width(100));

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
                bool enableHybridCLR = EditorGUILayout.ToggleLeft("EnableHybridCLR", m_EnableHybridCLR);
                if (m_EnableHybridCLR != enableHybridCLR)
                {
                    if (EditorUtility.DisplayDialog("HybridCLR", $"{(enableHybridCLR ? "开启" : "关闭")} HybridCLR时，请{(enableHybridCLR ? "激活" : "关闭")}资源收集器有关HybridCLR的资源", "确定", "取消"))
                    {
                        m_EnableHybridCLR = enableHybridCLR;
                        if (m_EnableHybridCLR)
                        {
                            GameBuildPipeline.EnableHybridCLR();
                        }
                        else
                        {
                            GameBuildPipeline.DisableHybridCLR();
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);

            EditorGUILayout.BeginHorizontal();
            {
                GameSetting.Instance.HotupdateAssembliesPath = EditorGUILayout.TextField("HotUpdate Dll Path", GameSetting.Instance.HotupdateAssembliesPath);
                Rect hotUpdateRect = GUILayoutUtility.GetLastRect();
                if (PathUtility.DropPath(hotUpdateRect, out string hotDatePath))
                {
                    if (hotDatePath != GameSetting.Instance.HotupdateAssembliesPath)
                    {
                        GameSetting.Instance.HotupdateAssembliesPath = hotDatePath;
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(GameSetting.Instance.HotupdateAssembliesPath));
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);

            m_FoldoutHotUpdateAssembliesGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutHotUpdateAssembliesGroup, "HotUpdateAssemblies");
            {
                if (m_FoldoutHotUpdateAssembliesGroup)
                {
                    foreach (var item in GameSetting.Instance.HotUpdateAssemblies)
                    {
                        EditorGUILayout.TextField(item);
                    }

                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        if (GUILayout.Button("Editor"))
                        {
                            SelectAssembly assemblyEditor = GetWindow<SelectAssembly>();

                            void Save(string[] hotUpdateAssemblies)
                            {
                                GameSetting.Instance.HotUpdateAssemblies = hotUpdateAssemblies.Select(item => item.Replace(".dll", null)).ToArray();
                                GameSetting.Instance.SaveSetting();
                                Repaint();
                            }

                            bool WherePredicate(Assembly assembly)
                            {
                                return !assembly.FullName.Contains("Editor");
                            }
                            HashSet<string> hasSelect = new(GameSetting.Instance.HotUpdateAssemblies.Select(item => item.Replace(".dll", null)));
                            assemblyEditor.Open(hasSelect, Save, WherePredicate);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            {
                GameSetting.Instance.PreserveAssembliesPath = EditorGUILayout.TextField("PreserveDll Path", GameSetting.Instance.PreserveAssembliesPath);
                Rect preservePathRect = GUILayoutUtility.GetLastRect();
                if (PathUtility.DropPath(preservePathRect, out string preservePath))
                {
                    if (preservePath != GameSetting.Instance.PreserveAssembliesPath)
                    {
                        GameSetting.Instance.PreserveAssembliesPath = preservePath;
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(GameSetting.Instance.PreserveAssembliesPath));
                }
            }
            EditorGUILayout.EndHorizontal();

            m_FoldoutPreserveAssembliesGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutPreserveAssembliesGroup, "PreserveAssemblies");
            {
                if (m_FoldoutPreserveAssembliesGroup)
                {
                    foreach (var item in GameSetting.Instance.PreserveAssemblies)
                    {
                        EditorGUILayout.TextField(item);
                    }

                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        if (GUILayout.Button("Editor"))
                        {
                            SelectAssembly assemblyEditor = GetWindow<SelectAssembly>();

                            void Save(string[] preserveDll)
                            {
                                GameSetting.Instance.PreserveAssemblies = preserveDll.Select(item => item.Replace(".dll", null)).ToArray();
                                GameSetting.Instance.SaveSetting();
                                Repaint();
                            }

                            bool WherePredicate(Assembly assembly)
                            {
                                return !assembly.FullName.Contains("Editor");
                            }
                            HashSet<string> hasSelect = new(GameSetting.Instance.PreserveAssemblies.Select(item => item.Replace(".dll", null)));
                            assemblyEditor.Open(hasSelect, Save, WherePredicate);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            {
                GameSetting.Instance.AOTAssembliesPath = EditorGUILayout.TextField("AOT Dll Path", GameSetting.Instance.AOTAssembliesPath);
                Rect aotPathRect = GUILayoutUtility.GetLastRect();
                if (PathUtility.DropPath(aotPathRect, out string aotPath))
                {
                    if (aotPath != GameSetting.Instance.AOTAssembliesPath)
                    {
                        GameSetting.Instance.AOTAssembliesPath = aotPath;
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(GameSetting.Instance.AOTAssembliesPath));
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            m_FoldoutPatchAOTAssembliesGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutPatchAOTAssembliesGroup, "PatchAOTAssemblies");
            {
                if (m_FoldoutPatchAOTAssembliesGroup)
                {
                    foreach (var item in GameSetting.Instance.AOTAssemblies)
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
                                GameSetting.Instance.AOTAssemblies = aotdll.Select(item => item.Replace(".dll", null)).ToArray();
                                GameSetting.Instance.SaveSetting();
                                Repaint();
                            }

                            bool WherePredicate(Assembly assembly)
                            {
                                return !assembly.FullName.Contains("Editor");
                            }
                            HashSet<string> hasSelect = new(GameSetting.Instance.AOTAssemblies.Select(item => item.Replace(".dll", null)));
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
                    GameBuildPipeline.CompileHotfixDll();
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
                bool canDifference = GameBuildPipeline.CanDifference();
                GUI.enabled = canDifference;
                bool diff = EditorGUILayout.ToggleLeft("Difference", canDifference && GameSetting.Instance.Difference, GUILayout.Width(120));
                if (diff != GameSetting.Instance.Difference)
                {
                    GameSetting.Instance.Difference = diff;
                }
                GUI.enabled = true;

                int resourceModeIndexEnum = GameSetting.Instance.ResourceModeIndex - 1;
                int resourceModeIndex = EditorGUILayout.Popup(resourceModeIndexEnum, GameBuildPipeline.ResourceMode, GUILayout.Width(160));
                if (resourceModeIndex != resourceModeIndexEnum)
                {
                    //由于跳过了 ResourceMode.Unspecified 保存时索引+1
                    GameSetting.Instance.ResourceModeIndex = resourceModeIndex + 1;
                    GameSetting.Instance.SaveSetting();
                }

                int assetBundleCollectorIndex = GameSetting.Instance.AssetBundleCollectorIndex;
                int tempBundleCollectorIndex = EditorGUILayout.Popup(assetBundleCollectorIndex, GameBuildPipeline.PackagesNames, GUILayout.Width(160));
                if (tempBundleCollectorIndex != assetBundleCollectorIndex)
                {
                    GameSetting.Instance.AssetBundleCollectorIndex = tempBundleCollectorIndex;
                    GameSetting.Instance.SaveSetting();
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField(GameSetting.Instance.BundlesOutput);
                GUI.enabled = true;

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    DEngine.Editor.OpenFolder.Execute(GameSetting.Instance.BundlesOutput);
                }

                if (GUILayout.Button("Clear", GUILayout.Width(80)))
                {
                    GameBuildPipeline.ClearBundles();
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
                m_FoldoutFileServerGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutFileServerGroup, "FileServer");
                if (m_FoldoutFileServerGroup)
                {
                    GUI.enabled = GameSetting.Instance.ResourceModeIndex > 1;
                    GameSetting.Instance.AutoCopyToFileServer = EditorGUILayout.Toggle("自动上传资源", GameSetting.Instance.AutoCopyToFileServer);
                    if (GameSetting.Instance.ResourceModeIndex <= 1)
                    {
                        GameSetting.Instance.AutoCopyToFileServer = false;
                    }
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("本地虚拟服务器地址", GameSetting.Instance.FileServerAddress);
                        GUI.enabled = true;
                        if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
                        {
                            string directory = EditorUtility.OpenFolderPanel("Select Output Directory", GameSetting.Instance.FileServerAddress, string.Empty);
                            if (!string.IsNullOrEmpty(directory))
                            {
                                if (Directory.Exists(directory) && directory != GameSetting.Instance.FileServerAddress)
                                {
                                    GameSetting.Instance.FileServerAddress = directory;
                                }
                            }
                        }
                        if (GUILayout.Button("Go", GUILayout.Width(30)))
                        {
                            DEngine.Editor.OpenFolder.Execute(GameSetting.Instance.FileServerAddress);
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

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    DEngine.Editor.OpenFolder.Execute(GameSetting.Instance.AppOutput);
                }

                if (GUILayout.Button("Clear", GUILayout.Width(80f)))
                {
                    IOUtility.ClearFolder(GameSetting.Instance.AppOutput);
                    Debug.Log($"Clear{GameSetting.Instance.AppOutput} success !");
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                string locationPathName = GameBuildPipeline.GetBuildAppFullName();
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
                BuildTarget buildTarget = GameBuildPipeline.GetBuildTarget(GameSetting.Instance.BuildPlatform);
                if (buildTarget != EditorUserBuildSettings.activeBuildTarget)
                {
                    if (EditorUtility.DisplayDialog("提示", "当前平台与目标平台不符，是否进行切换?", "确认", "取消"))
                    {
                        if (EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(buildTarget), buildTarget))
                        {
                            m_BeginBuildPlayer = true;
                        }
                    }
                }
                else
                {
                    m_BeginBuildPlayer = true;
                }
            }
            GUI.backgroundColor = bc;
        }
    }
}