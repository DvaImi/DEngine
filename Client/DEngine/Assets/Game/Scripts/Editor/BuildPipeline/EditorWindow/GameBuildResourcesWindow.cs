// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using System;
using System.Collections.Generic;
using System.IO;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using Game.Editor.ResourceTools;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public class GameBuildResourcesWindow : EditorWindow
    {
        private const string NoneOptionName = "<None>";
        private bool m_BeginBuildResources;
        private GUIContent m_BuildContent;
        private GUIContent m_EditorContent;
        private bool m_FoldoutBuildConfigGroup = true;
        private bool m_FoldoutVersionInfoGroup = true;
        private string m_HostingServiceTypeName;
        private int m_HostingServiceTypeNameIndex;
        private string[] m_HostingServiceTypeNames;
        private GUIContent m_SaveContent;
        private Vector2 m_ScrollPosition;
        private string[] m_VersionNamesForSourceDisplay;
        private int m_VersionNamesForSourceDisplayIndex;

        private void Update()
        {
            if (m_BeginBuildResources)
            {
                m_BeginBuildResources = false;
                GameBuildPipeline.BuildResource();
            }
        }

        private void OnEnable()
        {
            m_ScrollPosition = Vector2.zero;
            m_EditorContent = EditorGUIUtility.TrTextContentWithIcon("Editor", "", "Settings");
            m_BuildContent = EditorBuiltinIconHelper.GetPlatformIconContent("Build", "构建当前平台资源");
            m_SaveContent = EditorBuiltinIconHelper.GetSave("Save", "");

            m_HostingServiceTypeName = EditorPrefs.GetString("File Hosting Service Type Name", NoneOptionName);

            var helperTypeNames = new List<string>
            {
                NoneOptionName
            };

            helperTypeNames.AddRange(GameType.GetRuntimeOrEditorTypeNames(typeof(IHostingService)));
            m_HostingServiceTypeNames = helperTypeNames.ToArray();
            m_HostingServiceTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_HostingServiceTypeName))
            {
                m_HostingServiceTypeNameIndex = helperTypeNames.IndexOf(m_HostingServiceTypeName);
                if (m_HostingServiceTypeNameIndex <= 0)
                {
                    m_HostingServiceTypeNameIndex = 0;
                    m_HostingServiceTypeName = null;
                }
            }

            var controller = new ResourcePackBuilderController();
            if (controller.Load())
            {
                controller.Platform = GameBuildPipeline.GetCurrentPlatform();
                var versionNames = controller.GetVersionNames();
                m_VersionNamesForSourceDisplay = new string[versionNames.Length + 1];
                m_VersionNamesForSourceDisplay[0] = "<None>";
                for (int i = 0; i < versionNames.Length; i++)
                {
                    string versionNameForDisplay = versionNames[i];
                    m_VersionNamesForSourceDisplay[i + 1] = versionNameForDisplay;
                }

                m_VersionNamesForSourceDisplayIndex = Array.IndexOf(m_VersionNamesForSourceDisplay, DEngineSetting.Instance.SourceVersion);
            }

            DEngineSetting.Instance.BuildResourcePack = DEngineSetting.Instance.ResourceMode >= ResourceMode.Updatable && DEngineSetting.Instance.InternalResourceVersion > 1;

            if (!Directory.Exists(DEngineSetting.AppOutput))
            {
                Directory.CreateDirectory(DEngineSetting.AppOutput);
            }

            if (!Directory.Exists(DEngineSetting.BundlesOutput))
            {
                Directory.CreateDirectory(DEngineSetting.BundlesOutput);
            }

            GameBuildPipeline.RefreshPackages();
            GameBuildPipeline.CheckEnableHybridCLR();
        }

        private void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false);
            {
                GUILayout.Space(5f);
                EditorGUILayout.BeginVertical("box");
                {
                    GUIResources();
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);

                EditorGUILayout.BeginVertical("box");
                {
                    GUIHostingService();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(m_EditorContent, GUILayout.Height(30)))
                {
                    ResourceCollectorEditor.OpenWindow();
                }

                if (GUILayout.Button(m_BuildContent, GUILayout.Height(30)))
                {
                    m_BeginBuildResources = true;
                }

                if (GUILayout.Button(m_SaveContent, GUILayout.Height(30)))
                {
                    DEngineSetting.Save();
                    Debug.Log("Save success.");
                }
            }
            GUILayout.EndHorizontal();

            if (GUI.changed) Repaint();
        }

        [MenuItem("Game/Build Pipeline/Resource", false, 2)]
        private static void Open()
        {
            var window = GetWindow<GameBuildResourcesWindow>("Build Resource", true);
            window.minSize = new Vector2(800f, 800f);
        }

        private void GUIResources()
        {
            EditorGUILayout.LabelField("Resources", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GameBuildPipeline.SwitchResourceMode();
                    GUILayout.Space(10f);
                    var assetBundleCollectorIndex = DEngineSetting.Instance.AssetBundleCollectorIndex;
                    var tempBundleCollectorIndex = EditorGUILayout.Popup("构建资源包裹", assetBundleCollectorIndex, GameBuildPipeline.PackagesNames);
                    if (tempBundleCollectorIndex != assetBundleCollectorIndex)
                    {
                        DEngineSetting.Instance.AssetBundleCollectorIndex = tempBundleCollectorIndex;
                        DEngineSetting.Save();
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(20f);

                EditorGUILayout.BeginHorizontal();
                {
                    var forceRebuildAssetBundle = EditorGUILayout.ToggleLeft("ForceRebuildAssetBundle", DEngineSetting.Instance.ForceRebuildAssetBundle);
                    if (!forceRebuildAssetBundle.Equals(DEngineSetting.Instance.ForceRebuildAssetBundle))
                    {
                        DEngineSetting.Instance.ForceRebuildAssetBundle = forceRebuildAssetBundle;
                    }

                    var forceUpdateGame = EditorGUILayout.ToggleLeft("需要更新主包", DEngineSetting.Instance.ForceUpdateGame);
                    if (!forceUpdateGame.Equals(DEngineSetting.Instance.ForceUpdateGame))
                    {
                        DEngineSetting.Instance.ForceUpdateGame = forceUpdateGame;
                    }

                    GUI.enabled = DEngineSetting.Instance.ResourceMode >= ResourceMode.Updatable && DEngineSetting.Instance.InternalResourceVersion > 1;
                    if (!GUI.enabled && DEngineSetting.Instance.BuildResourcePack)
                    {
                        DEngineSetting.Instance.BuildResourcePack = false;
                        DEngineSetting.Save();
                    }

                    var buildResourcePack = EditorGUILayout.ToggleLeft("构建补丁包", DEngineSetting.Instance.BuildResourcePack);
                    if (!buildResourcePack.Equals(DEngineSetting.Instance.BuildResourcePack))
                    {
                        DEngineSetting.Instance.BuildResourcePack = buildResourcePack;
                    }

                    GUI.enabled = true;
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(10f);
            if (DEngineSetting.Instance.BuildResourcePack)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    GUIPatchPack();
                }
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(10f);
            EditorGUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField(DEngineSetting.BundlesOutput);
                GUI.enabled = true;

                if (GUILayout.Button("Reveal", GUILayout.Width(80), GUILayout.Height(20)))
                {
                    EditorUtility.RevealInFinder(DEngineSetting.BundlesOutput);
                }

                if (GUILayout.Button("Clear", GUILayout.Width(80), GUILayout.Height(20)))
                {
                    GameBuildPipeline.ClearResource();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5f);
            EditorGUILayout.BeginVertical("box");
            {
                m_FoldoutBuildConfigGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutBuildConfigGroup, "BuildConfig");

                if (m_FoldoutBuildConfigGroup)
                {
                    DropPathUtility.DropAndPingAssetPath("ResourceCollectionConfig", ref DEngineSetting.Instance.ResourceCollectionConfig);
                    DropPathUtility.DropAndPingAssetPath("ResourceEditorConfig", ref DEngineSetting.Instance.ResourceEditorConfig);
                    DropPathUtility.DropAndPingAssetPath("ResourceBuilderConfig", ref DEngineSetting.Instance.ResourceBuilderConfig);
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
                GUILayout.Space(5f);
                m_FoldoutVersionInfoGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutVersionInfoGroup, "VersionInfo");
                if (m_FoldoutVersionInfoGroup)
                {
                    EditorGUILayout.LabelField("内置资源版本", DEngineSetting.Instance.InternalResourceVersion.ToString());
                    EditorGUILayout.LabelField("最新的游戏版本号", DEngineSetting.Instance.LatestGameVersion);
                    DEngineSetting.Instance.HostURL = EditorGUILayout.TextField("网络CDN地址", DEngineSetting.Instance.HostURL);
                    DEngineSetting.Instance.HostingServicePort = EditorGUILayout.IntField("网络CDN地址端口号", DEngineSetting.Instance.HostingServicePort);
                    GUI.enabled = false;
                    EditorGUILayout.TextField("资源版本地址格式", GameBuildPipeline.GetCheckVersionUrlFormat());
                    EditorGUILayout.TextField("资源下载地址格式", GameBuildPipeline.GetUpdatePrefixUriFormat());
                    GUI.enabled = true;
                    DEngineSetting.Instance.InternalGameVersion = EditorGUILayout.IntField("内部游戏版本号", DEngineSetting.Instance.InternalGameVersion);
                    DEngineSetting.Instance.WindowsAppUrl = EditorGUILayout.TextField("Windows下载应用地址", DEngineSetting.Instance.WindowsAppUrl);
                    DEngineSetting.Instance.AndroidAppUrl = EditorGUILayout.TextField("Android应用下载地址", DEngineSetting.Instance.AndroidAppUrl);
                    DEngineSetting.Instance.MacOSAppUrl = EditorGUILayout.TextField("MacOS下载应用地址", DEngineSetting.Instance.MacOSAppUrl);
                    DEngineSetting.Instance.IOSAppUrl = EditorGUILayout.TextField("IOS下载应用地址", DEngineSetting.Instance.IOSAppUrl);
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            EditorGUILayout.EndVertical();
        }

        private void GUIHostingService()
        {
            GUILayout.Label("Hosting Service", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical();
            {
                EditorGUI.BeginDisabledGroup(DEngineSetting.Instance.ResourceMode <= ResourceMode.Package);
                {
                    var enableHostingService = EditorGUILayout.Toggle("Enable", DEngineSetting.Instance.EnableHostingService);

                    if (!enableHostingService.Equals(DEngineSetting.Instance.EnableHostingService)) DEngineSetting.Instance.EnableHostingService = enableHostingService;

                    var hostingServiceTypeNameIndex = EditorGUILayout.Popup("HostingService", m_HostingServiceTypeNameIndex, m_HostingServiceTypeNames);
                    if (hostingServiceTypeNameIndex != m_HostingServiceTypeNameIndex)
                    {
                        m_HostingServiceTypeNameIndex = hostingServiceTypeNameIndex;
                        m_HostingServiceTypeName = hostingServiceTypeNameIndex <= 0 ? null : m_HostingServiceTypeNames[hostingServiceTypeNameIndex];
                        EditorPrefs.SetString("File Hosting Service Type Name", m_HostingServiceTypeName);
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("本地路径", HostingServiceManager.HostingServicePath);
                        if (GUILayout.Button("Reveal", GUILayout.Width(80), GUILayout.Height(20))) EditorUtility.RevealInFinder(HostingServiceManager.HostingServicePath);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndVertical();
        }

        private void GUIPatchPack()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Source Version", GUILayout.Width(160f));
                int value = EditorGUILayout.Popup(m_VersionNamesForSourceDisplayIndex, m_VersionNamesForSourceDisplay);
                if (m_VersionNamesForSourceDisplayIndex != value)
                {
                    m_VersionNamesForSourceDisplayIndex = value;
                    DEngineSetting.Instance.SourceVersion = m_VersionNamesForSourceDisplay[m_VersionNamesForSourceDisplayIndex];
                    DEngineSetting.Save();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}