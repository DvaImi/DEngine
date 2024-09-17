// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using System.IO;
using DEngine.Editor;
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
                GameBuildPipeline.BuildResource(GameSetting.Instance.BundlesOutput, GameSetting.Instance.Difference);
            }
        }

        private void OnEnable()
        {
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
                EditorGUILayout.BeginVertical("box");
                {
                    GUIResources();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save", GUILayout.Height(30)))
                {
                    GameBuildPipeline.SaveResource();
                    GameSetting.Save();
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
                    OpenFolder.Execute(GameSetting.Instance.BundlesOutput);
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
                    DropPathUtility.DropAssetPath("ResourceCollectionConfig", ref GameSetting.Instance.ResourceCollectionConfig);
                    DropPathUtility.DropAssetPath("ResourceEditorConfig", ref GameSetting.Instance.ResourceEditorConfig);
                    DropPathUtility.DropAssetPath("ResourceBuilderConfig", ref GameSetting.Instance.ResourceBuilderConfig);
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
                    EditorGUILayout.IntField("内置资源版本", GameSetting.Instance.InternalResourceVersion);
                    EditorGUILayout.TextField("最新的游戏版本号", Application.version);
                    GUI.enabled = true;
                    GameSetting.Instance.BuildInfo.CheckVersionUrl = EditorGUILayout.TextField("版本检查文件地址", GameSetting.Instance.BuildInfo.CheckVersionUrl);
                    GameSetting.Instance.BuildInfo.WindowsAppUrl = EditorGUILayout.TextField("Windows下载应用地址", GameSetting.Instance.BuildInfo.WindowsAppUrl);
                    GameSetting.Instance.BuildInfo.AndroidAppUrl = EditorGUILayout.TextField("Android应用下载地址", GameSetting.Instance.BuildInfo.AndroidAppUrl);
                    GameSetting.Instance.BuildInfo.MacOSAppUrl = EditorGUILayout.TextField("MacOS下载应用地址", GameSetting.Instance.BuildInfo.MacOSAppUrl);
                    GameSetting.Instance.BuildInfo.IOSAppUrl = EditorGUILayout.TextField("IOS下载应用地址", GameSetting.Instance.BuildInfo.IOSAppUrl);
                    GameSetting.Instance.BuildInfo.LatestGameVersion = GameSetting.Instance.LatestGameVersion;
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
    }
}