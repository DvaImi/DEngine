using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    /// <summary>
    /// 资源包预览窗口
    /// </summary>
    public class AssetBundlePreviewWindow : EditorWindow
    {
        public const float GAP = 2;
        private AssetBundlePreview m_AssetBundlePreview;
        private bool[] m_AssetsFoldouts;
        private GUIStyle m_MidLabelStyle;
        private GUIStyle m_ToolBarLabelStyle;
        private Vector2 m_ScrollPosition;

        [MenuItem("Game/AssetPreview", false, 2)]
        internal static void Open()
        {
            EditorWindow window = GetWindow<AssetBundlePreviewWindow>(false, "Preview");
            window.minSize = new Vector2(1200f, 420f);
            window.Show();
        }

        private void OnEnable()
        {
            m_AssetBundlePreview ??= GetAssetBundlePreview();
            m_AssetsFoldouts = new bool[m_AssetBundlePreview.assetPreviews.Count];
            m_MidLabelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
            };
            m_ToolBarLabelStyle = new GUIStyle(EditorStyles.toolbarTextField)
            {
                alignment = TextAnchor.MiddleCenter,
            };
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                DrawElementLabelGUI();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);


            if (m_AssetBundlePreview == null)
            {
                return;
            }
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false);
            {
                GUILayout.BeginVertical();
                {
                    // 绘制 assetPreviews 中的每个 AssetPreview
                    for (int i = 0; i < m_AssetBundlePreview.assetPreviews.Count; i++)
                    {
                        AssetPreview assetPreview = m_AssetBundlePreview.assetPreviews[i];

                        GUILayout.BeginHorizontal();
                        {
                            // 绘制 assetPath 作为折叠按钮
                            m_AssetsFoldouts[i] = EditorGUILayout.Foldout(m_AssetsFoldouts[i], assetPreview.assetPath);
                            EditorGUILayout.TextField(assetPreview.groups, m_MidLabelStyle, GUILayout.Width(200));
                            EditorGUILayout.LabelField(assetPreview.Count.ToString(), m_MidLabelStyle, GUILayout.Width(80));
                            EditorGUILayout.LabelField(StringUtility.GetByteLengthString(assetPreview.Length), m_MidLabelStyle, GUILayout.Width(80));
                        }
                        GUILayout.EndHorizontal();
                        // 如果折叠按钮展开，则绘制 assets
                        if (m_AssetsFoldouts[i])
                        {
                            EditorGUI.indentLevel++;
                            foreach (Object obj in assetPreview.assets)
                            {
                                string assetPath = AssetDatabase.GetAssetPath(obj);
                                GUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.ObjectField(obj, typeof(Object), false, GUILayout.Width(400));
                                    EditorGUILayout.LabelField(StringUtility.GetByteLengthString(GameEditorUtility.CalculateAssetSize(obj)), GUILayout.Width(80));
                                }
                                GUILayout.EndHorizontal();
                            }
                            EditorGUI.indentLevel--;
                        }

                        GUILayout.Space(5);
                    }
                }
                GUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        private static AssetBundlePreview GetAssetBundlePreview()
        {
            AssetBundleCollector assetBundleCollector = GameEditorUtility.GetScriptableObject<AssetBundleCollector>();
            if (assetBundleCollector == null)
            {
                return null;
            }
            AssetBundlePreview assetBundlePreview = new AssetBundlePreview();

            foreach (AssetCollector collector in assetBundleCollector.Collector)
            {
                AssetPreview assetPreview = new()
                {
                    assetPath = collector.assetPath,
                    groups = collector.groups
                };
                assetPreview.assets = GetAssets(collector, out assetPreview.Count, out assetPreview.Length);
                assetBundlePreview.assetPreviews.Add(assetPreview);
            }
            return assetBundlePreview;
        }

        private static Object[] GetAssets(AssetCollector collector, out int count, out long length)
        {
            length = 0;

            string[] allAssetPaths = AssetDatabase.FindAssets("t:Object", new[] { collector.assetPath })
                                                  .Select(AssetDatabase.GUIDToAssetPath)
                                                  .ToArray();

            Object[] objects = new Object[allAssetPaths.Length];
            count = objects.Length;
            for (int i = 0; i < count; i++)
            {
                objects[i] = AssetDatabase.LoadAssetAtPath<Object>(allAssetPaths[i]);

                if (File.Exists(allAssetPaths[i]))
                {
                    length += new FileInfo(allAssetPaths[i]).Length;
                }
                else
                {
                    // 处理文件夹的计算逻辑
                    DirectoryInfo directoryInfo = new DirectoryInfo(allAssetPaths[i]);
                    FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        length += fileInfo.Length;
                    }
                }
            }
            return collector == null ? (new Object[0]) : objects;
        }

        private void DrawElementLabelGUI()
        {
            GUI.enabled = false;
            GUILayoutOption[] textFieldOptions = { GUILayout.ExpandWidth(true), GUILayout.MinWidth(100f), GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - (2 * GAP)) };
            EditorGUILayout.TextField("Name", textFieldOptions);
            GUILayout.Space(GAP);
            EditorGUILayout.TextField("Groups", m_ToolBarLabelStyle, GUILayout.Width(200), GUILayout.Height(20));
            GUILayout.Space(GAP);
            EditorGUILayout.TextField("Count", m_ToolBarLabelStyle, GUILayout.Width(80), GUILayout.Height(20));
            GUILayout.Space(GAP);
            EditorGUILayout.TextField("Length", m_ToolBarLabelStyle, GUILayout.Width(80), GUILayout.Height(20));
            GUI.enabled = true;
        }
    }
}
