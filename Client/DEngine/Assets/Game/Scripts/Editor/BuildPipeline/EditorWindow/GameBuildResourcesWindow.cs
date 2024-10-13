// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using System.IO;
using DEngine.Editor;
using DEngine.Resource;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public class GameBuildResourcesWindow : EditorWindow
    {
        private bool m_BeginBuildResources = false;
        private bool m_FoldoutBuildConfigGroup = true;
        private bool m_FoldoutBuiltInfoGroup = true;
        private Vector2 m_ScrollPosition;
        private GUIContent m_EditorContent;
        private GUIContent m_BuildContent;
        private GUIContent m_SaveContent;

        [MenuItem("Game/Build Pipeline/Resource", false, 2)]
        private static void Open()
        {
            GameBuildResourcesWindow window = GetWindow<GameBuildResourcesWindow>("Build Resource", true);
            window.minSize = new Vector2(800f, 600f);
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
                    GameBuildPipeline.SaveResource();
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
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Resources", EditorStyles.boldLabel);
                var resourceModeIndexEnum = DEngineSetting.Instance.ResourceMode;
                var resourceModeIndex = (ResourceMode)EditorGUILayout.EnumPopup(DEngineSetting.Instance.ResourceMode, GUILayout.Width(160));
                if (resourceModeIndex != resourceModeIndexEnum)
                {
                    DEngineSetting.Instance.ResourceMode = resourceModeIndex;
                    DEngineSetting.Save();
                }

                int assetBundleCollectorIndex = DEngineSetting.Instance.AssetBundleCollectorIndex;
                int tempBundleCollectorIndex = EditorGUILayout.Popup(assetBundleCollectorIndex, GameBuildPipeline.PackagesNames, GUILayout.Width(160));
                if (tempBundleCollectorIndex != assetBundleCollectorIndex)
                {
                    DEngineSetting.Instance.AssetBundleCollectorIndex = tempBundleCollectorIndex;
                    DEngineSetting.Save();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                bool forceRebuildAssetBundle = EditorGUILayout.ToggleLeft("ForceRebuildAssetBundle", DEngineSetting.Instance.ForceRebuildAssetBundle, GUILayout.Width(200));
                if (!forceRebuildAssetBundle.Equals(DEngineSetting.Instance.ForceRebuildAssetBundle))
                {
                    DEngineSetting.Instance.ForceRebuildAssetBundle = forceRebuildAssetBundle;
                }

                bool canDifference = GameBuildPipeline.CanDifference();
                GUI.enabled = canDifference;
                bool diff = EditorGUILayout.ToggleLeft("Difference", canDifference && DEngineSetting.Instance.Difference, GUILayout.Width(120));
                if (!diff.Equals(DEngineSetting.Instance.Difference))
                {
                    DEngineSetting.Instance.Difference = diff;
                }

                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
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
                    DEngineSetting.Instance.ForceUpdateGame = EditorGUILayout.Toggle("强制更新应用", DEngineSetting.Instance.ForceUpdateGame);
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (DEngineSetting.Instance.ForceUpdateGame)
                        {
                            GUI.enabled = false;
                            EditorGUILayout.LabelField($"强制更新应用将以{DEngineSetting.Instance.LatestGameVersion} 为最后一个版本号");
                            GUI.enabled = true;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    DEngineSetting.Instance.UpdatePrefixUri = EditorGUILayout.TextField("资源更新地址", DEngineSetting.Instance.UpdatePrefixUri);
                    GUI.enabled = false;
                    EditorGUILayout.IntField("内置资源版本", DEngineSetting.Instance.InternalResourceVersion);
                    EditorGUILayout.TextField("最新的游戏版本号", Application.version);
                    GUI.enabled = true;
                    DEngineSetting.Instance.BuildInfo.CheckVersionUrl = EditorGUILayout.TextField("版本检查文件地址", DEngineSetting.Instance.BuildInfo.CheckVersionUrl);
                    DEngineSetting.Instance.BuildInfo.WindowsAppUrl = EditorGUILayout.TextField("Windows下载应用地址", DEngineSetting.Instance.BuildInfo.WindowsAppUrl);
                    DEngineSetting.Instance.BuildInfo.AndroidAppUrl = EditorGUILayout.TextField("Android应用下载地址", DEngineSetting.Instance.BuildInfo.AndroidAppUrl);
                    DEngineSetting.Instance.BuildInfo.MacOSAppUrl = EditorGUILayout.TextField("MacOS下载应用地址", DEngineSetting.Instance.BuildInfo.MacOSAppUrl);
                    DEngineSetting.Instance.BuildInfo.IOSAppUrl = EditorGUILayout.TextField("IOS下载应用地址", DEngineSetting.Instance.BuildInfo.IOSAppUrl);
                    DEngineSetting.Instance.BuildInfo.LatestGameVersion = DEngineSetting.Instance.LatestGameVersion;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            EditorGUILayout.EndVertical();
        }
    }
}