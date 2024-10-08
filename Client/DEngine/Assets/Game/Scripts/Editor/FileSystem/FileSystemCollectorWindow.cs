using System;
using System.Collections.Generic;
using System.Linq;
using DEngine;
using Game.Editor.BuildPipeline;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Editor.ResourceTools
{
    public class FileSystemCollectorWindow : EditorWindow
    {
        private const string NoneOptionName = "<None>";
        private FileSystemCollector m_FileSystemCollector;
        private Vector2 m_ScrollPosition;
        private GUIContent m_BuildContent;
        private bool m_ExportFlag;
        private IFileSystemDataHandlerHelper m_FileSystemDataHandlerHelper;
        private string[] m_FileSystemHandlerTypeNames;
        private readonly Dictionary<string, bool> m_FoldoutMap = new();
        private FileSystemData m_SelectedFileSystemData;
        private bool m_ContentChange = false;

        [MenuItem("Game/File System/ Collector", false, 3)]
        public static void ShowWindow()
        {
            FileSystemCollectorWindow window = GetWindow<FileSystemCollectorWindow>("File System Collector Editor");
            window.minSize = new Vector2(600f, 420f);
        }

        private void OnEnable()
        {
            m_ExportFlag = false;
            m_BuildContent = EditorGUIUtility.TrTextContentWithIcon("Export", "导出配置", "Project");

            m_FileSystemCollector = EditorTools.LoadScriptableObject<FileSystemCollector>();
            foreach (var fileSystemData in m_FileSystemCollector.FileSystemDatas)
            {
                m_FoldoutMap[fileSystemData.FileSystem] = true;
            }
            List<string> temp = new List<string> { NoneOptionName };
            temp.AddRange(GameEditorAssembly.GetRuntimeOrEditorTypeNames(typeof(IFileSystemDataHandlerHelper)));
            m_FileSystemHandlerTypeNames = temp.ToArray();
            m_FileSystemDataHandlerHelper = null;
            RefreshRawFileHandler();
        }

        private void Update()
        {
            if (m_ExportFlag)
            {
                m_ExportFlag = false;
                GameBuildPipeline.ExportFileSystem(new RawFileSystemHelper(), m_FileSystemDataHandlerHelper, m_FileSystemCollector);
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("File System Collector", EditorStyles.boldLabel);

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("File System Handler", GUILayout.Width(160f));
                int selectedIndex = EditorGUILayout.Popup(m_FileSystemCollector.FileSystemHandlerTypeNameIndex, m_FileSystemHandlerTypeNames);
                if (selectedIndex != m_FileSystemCollector.FileSystemHandlerTypeNameIndex)
                {
                    m_ContentChange = true;
                    m_FileSystemCollector.FileSystemHandlerTypeNameIndex = selectedIndex;
                    m_FileSystemCollector.FileSystemHelperTypeName = selectedIndex <= 0 ? string.Empty : m_FileSystemHandlerTypeNames[selectedIndex];
                    if (RefreshRawFileHandler())
                    {
                        Debug.Log("Set file system handler success.");
                    }
                    else
                    {
                        Debug.LogWarning("Set  file system handler failure.");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("[+]", GUILayout.Width(45)))
                {
                    var newFileSystemData = new FileSystemData
                    {
                        FileSystem = $"new FileSystem({m_FileSystemCollector.FileSystemDatas.Count})"
                    };
                    m_FileSystemCollector.FileSystemDatas.Add(newFileSystemData);
                    m_FoldoutMap[newFileSystemData.FileSystem] = true;
                    m_ContentChange = true;
                }

                if (GUILayout.Button("[-]", GUILayout.Width(45)))
                {
                    if (m_SelectedFileSystemData != null)
                    {
                        if (m_FileSystemCollector.FileSystemDatas.Remove(m_SelectedFileSystemData) && m_FoldoutMap.Remove(m_SelectedFileSystemData.FileSystem))
                        {
                            Debug.Log("Delete success");
                        }
                    }

                    m_ContentChange = true;
                }
            }
            GUILayout.EndHorizontal();
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false);
            {
                for (int i = 0; i < m_FileSystemCollector.FileSystemDatas.Count; i++)
                {
                    var fileSystemData = m_FileSystemCollector.FileSystemDatas[i];
                    bool isFoldout = m_FoldoutMap.GetValueOrDefault(fileSystemData.FileSystem, true);
                    bool isSelect = m_SelectedFileSystemData != null && m_SelectedFileSystemData.FileSystem == fileSystemData.FileSystem && m_FoldoutMap.GetValueOrDefault(m_SelectedFileSystemData.FileSystem, false);
                    string foldoutName = Utility.Text.Format("{0} {1}", fileSystemData.FileSystem, isFoldout ? "▼" : "▶");
                    m_FoldoutMap[fileSystemData.FileSystem] = EditorGUILayout.BeginFoldoutHeaderGroup(isFoldout, foldoutName, isSelect ? new GUIStyle(EditorStyles.label) { normal = new GUIStyleState { textColor = Color.blue }, } : EditorStyles.label);
                    {
                        if (m_FoldoutMap[fileSystemData.FileSystem])
                        {
                            m_SelectedFileSystemData = fileSystemData;
                            GUILayout.BeginHorizontal();
                            {
                                GUIFileSystemData(fileSystemData);
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }
            EditorGUILayout.EndScrollView();


            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button(m_BuildContent, GUILayout.Height(30)))
                {
                    m_ExportFlag = true;
                }

                if (GUILayout.Button(EditorGUIUtility.TrTextContentWithIcon(m_ContentChange ? "Save *" : "Save", "保存配置", "SaveAs"), GUILayout.Height(30)))
                {
                    SaveCollector();
                    Debug.Log("Save success.");
                }
            }
            GUILayout.EndHorizontal();
        }

        private void SaveCollector()
        {
            EditorUtility.SetDirty(m_FileSystemCollector);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            m_ContentChange = false;
        }

        private void GUIFileSystemData(FileSystemData fileSystemData)
        {
            EditorGUILayout.BeginVertical("box");
            {
                string fileSystem = EditorGUILayout.DelayedTextField("文件系统名称", fileSystemData.FileSystem);
                if (fileSystem != fileSystemData.FileSystem)
                {
                    fileSystemData.FileSystem = fileSystem;
                    m_ContentChange = true;
                }

                if (string.IsNullOrWhiteSpace(fileSystemData.FileSystem))
                {
                    EditorGUILayout.HelpBox("名称不可为空", MessageType.Warning);
                }

                string outputPath = fileSystemData.OutPutPath;
                EditorTools.GUIAssetPath("Export Path", ref fileSystemData.OutPutPath, true);

                if (outputPath != fileSystemData.OutPutPath)
                {
                    m_ContentChange = true;
                }

                fileSystemData.AssetPaths ??= new List<string>();
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        EditorGUILayout.LabelField("资源列表", EditorStyles.boldLabel);
                        if (GUILayout.Button("[+]", GUILayout.Width(45)))
                        {
                            fileSystemData.AssetPaths.Add(null);
                            m_ContentChange = true;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    for (var i = 0; i < fileSystemData.AssetPaths.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            string assetPath = fileSystemData.AssetPaths[i];

                            GUIStyle warningLableGUIStyle = new(EditorStyles.label)
                            {
                                normal = new GUIStyleState
                                {
                                    textColor = Color.yellow
                                },
                            };
                            if (string.IsNullOrWhiteSpace(assetPath))
                            {
                                EditorGUILayout.LabelField("AssetPath is invalid", warningLableGUIStyle);
                            }
                            else
                            {
                                Object target = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                                target = EditorGUILayout.ObjectField(target, typeof(DefaultAsset), false);
                                assetPath = AssetDatabase.GetAssetPath(target);
                            }

                            Rect rect = GUILayoutUtility.GetLastRect();
                            if (DropPathUtility.DropPathOutType(rect, out string path, out _))
                            {
                                if (!string.Equals(path, assetPath, StringComparison.Ordinal))
                                {
                                    fileSystemData.AssetPaths[i] = path;
                                    m_ContentChange = true;
                                }
                            }

                            if (GUILayout.Button("x", GUILayout.Width(30)))
                            {
                                fileSystemData.AssetPaths.RemoveAt(i);
                                i--;
                                m_ContentChange = true;
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    if (fileSystemData.AssetPaths.Count == 0)
                    {
                        EditorGUILayout.HelpBox("资源列表为空", MessageType.Info);
                    }
                }
                GUILayout.EndVertical();

                Rect assetPathRect = GUILayoutUtility.GetLastRect();
                if (DropPathUtility.DropPath(assetPathRect, out string[] assetPaths))
                {
                    if (assetPaths != null && assetPaths.Length != 0)
                    {
                        foreach (var assetPath in assetPaths)
                        {
                            if (fileSystemData.AssetPaths.Contains(assetPath))
                            {
                                continue;
                            }

                            fileSystemData.AssetPaths.Add(assetPath);
                        }

                        m_ContentChange = true;
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        private bool RefreshRawFileHandler()
        {
            if (!string.IsNullOrEmpty(m_FileSystemCollector.FileSystemHelperTypeName) && m_FileSystemHandlerTypeNames.Contains(m_FileSystemCollector.FileSystemHelperTypeName))
            {
                Type buildEventHandlerType = Utility.Assembly.GetType(m_FileSystemCollector.FileSystemHelperTypeName);
                if (buildEventHandlerType != null)
                {
                    IFileSystemDataHandlerHelper buildEventSystemDataHandler = (IFileSystemDataHandlerHelper)Activator.CreateInstance(buildEventHandlerType);
                    if (buildEventSystemDataHandler != null)
                    {
                        m_FileSystemDataHandlerHelper = buildEventSystemDataHandler;
                        return true;
                    }
                }
            }

            m_FileSystemCollector.FileSystemHelperTypeName = string.Empty;
            m_FileSystemDataHandlerHelper = null;
            return false;
        }
    }
}