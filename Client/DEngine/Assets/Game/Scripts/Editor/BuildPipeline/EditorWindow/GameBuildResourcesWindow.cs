// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using System.Collections.Generic;
using System.IO;
using DEngine.Editor;
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
        private bool m_FoldoutBuildConfigGroup = true;
        private bool m_FoldoutBuiltInfoGroup = true;
        private Vector2 m_ScrollPosition;
        private GUIContent m_EditorContent;
        private GUIContent m_BuildContent;
        private GUIContent m_SaveContent;
        private string[] m_HostingServiceTypeNames;
        private int m_HostingServiceTypeNameIndex;
        private string m_HostingServiceTypeName;

        [MenuItem("Game/Build Pipeline/Resource", false, 2)]
        private static void Open()
        {
            GameBuildResourcesWindow window = GetWindow<GameBuildResourcesWindow>("Build Resource", true);
            window.minSize = new Vector2(800f, 800f);
        }

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

            List<string> helperTypeNames = new List<string>
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

            if (GUI.changed)
            {
                Repaint();
            }
        }


        private void GUIResources()
        {
            EditorGUILayout.LabelField("Resources", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var resourceModeIndexEnum = DEngineSetting.Instance.ResourceMode;
                    var resourceModeIndex = (ResourceMode)EditorGUILayout.EnumPopup("资源打包模式", DEngineSetting.Instance.ResourceMode);
                    if (resourceModeIndex != resourceModeIndexEnum)
                    {
                        DEngineSetting.Instance.ResourceMode = resourceModeIndex;
                        DEngineSetting.Save();
                    }

                    int assetBundleCollectorIndex = DEngineSetting.Instance.AssetBundleCollectorIndex;
                    int tempBundleCollectorIndex = EditorGUILayout.Popup("构建资源包裹", assetBundleCollectorIndex, GameBuildPipeline.PackagesNames);
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
                    bool forceRebuildAssetBundle = EditorGUILayout.Toggle("ForceRebuildAssetBundle", DEngineSetting.Instance.ForceRebuildAssetBundle);
                    if (!forceRebuildAssetBundle.Equals(DEngineSetting.Instance.ForceRebuildAssetBundle))
                    {
                        DEngineSetting.Instance.ForceRebuildAssetBundle = forceRebuildAssetBundle;
                    }

                    bool forceUpdateGame = EditorGUILayout.Toggle("需要更新主包", DEngineSetting.Instance.ForceUpdateGame);
                    if (!forceUpdateGame.Equals(DEngineSetting.Instance.ForceUpdateGame))
                    {
                        DEngineSetting.Instance.ForceUpdateGame = forceUpdateGame;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            GameBuildPipeline.GUIPlatform();
            GUILayout.Space(10f);
            EditorGUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField(DEngineSetting.BundlesOutput);
                GUI.enabled = true;

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    OpenFolder.Execute(DEngineSetting.BundlesOutput);
                }

                if (GUILayout.Button("Clear", GUILayout.Width(80)))
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
                    DropPathUtility.DropAssetPath("ResourceCollectionConfig", ref DEngineSetting.Instance.ResourceCollectionConfig);
                    DropPathUtility.DropAssetPath("ResourceEditorConfig", ref DEngineSetting.Instance.ResourceEditorConfig);
                    DropPathUtility.DropAssetPath("ResourceBuilderConfig", ref DEngineSetting.Instance.ResourceBuilderConfig);
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
                GUILayout.Space(5f);
                m_FoldoutBuiltInfoGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutBuiltInfoGroup, "BuildInfo");
                if (m_FoldoutBuiltInfoGroup)
                {
                    EditorGUILayout.LabelField("内置资源版本", DEngineSetting.Instance.InternalResourceVersion.ToString());
                    EditorGUILayout.LabelField("最新的游戏版本号", DEngineSetting.Instance.LatestGameVersion);
                    DEngineSetting.Instance.HostURL = EditorGUILayout.TextField("网络CDN地址", DEngineSetting.Instance.HostURL);
                    DEngineSetting.Instance.HostingServicePort = EditorGUILayout.IntField("网络CDN地址端口号", DEngineSetting.Instance.HostingServicePort);
                    GUI.enabled = false;
                    EditorGUILayout.TextField("资源版本地址", GameBuildPipeline.GetCheckVersionUrl());
                    EditorGUILayout.TextField("资源下载地址", GameBuildPipeline.GetUpdatePrefixUri());
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
                bool enableHostingService = EditorGUILayout.Toggle("Enable", DEngineSetting.Instance.EnableHostingService);

                if (!enableHostingService.Equals(DEngineSetting.Instance.EnableHostingService))
                {
                    DEngineSetting.Instance.EnableHostingService = enableHostingService;
                }

                int hostingServiceTypeNameIndex = EditorGUILayout.Popup("HostingService", m_HostingServiceTypeNameIndex, m_HostingServiceTypeNames);
                if (hostingServiceTypeNameIndex != m_HostingServiceTypeNameIndex)
                {
                    m_HostingServiceTypeNameIndex = hostingServiceTypeNameIndex;
                    m_HostingServiceTypeName = hostingServiceTypeNameIndex <= 0 ? null : m_HostingServiceTypeNames[hostingServiceTypeNameIndex];
                    EditorPrefs.SetString("File Hosting Service Type Name", m_HostingServiceTypeName);
                }

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("本地路径", HostingServiceManager.HostingServicePath);
                    if (GUILayout.Button("Open", GUILayout.Width(100)))
                    {
                        OpenFolder.Execute(HostingServiceManager.HostingServicePath);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}