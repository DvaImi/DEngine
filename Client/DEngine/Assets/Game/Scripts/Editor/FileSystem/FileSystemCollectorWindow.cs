using System.Collections.Generic;
using System.IO;
using Game.Editor.BuildPipeline;
using SFB;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.FileSystem
{
    public class FileSystemCollectorWindow : EditorWindow
    {
        private FileSystemCollector m_FileSystemCollector;
        private Vector2 m_ScrollPosition;
        private GUIContent m_ExportContent;
        private GUIContent m_ExportAllContent;
        private bool m_ExportFlag;
        private readonly Dictionary<string, bool> m_FoldoutMap = new();
        private FileSystemData m_SelectedFileSystemData;
        private bool m_ContentChange;
        private const string FileNameTitle = "{0} {1}";

        [MenuItem("Game/File System/ Collector", false, 3)]
        public static void ShowWindow()
        {
            FileSystemCollectorWindow window = GetWindow<FileSystemCollectorWindow>("File System Collector Editor");
            window.minSize = new Vector2(600f, 420f);
        }

        private void OnEnable()
        {
            m_ExportFlag = false;
            m_ExportContent = EditorGUIUtility.TrTextContentWithIcon("Export", "导出当前配置", "Project");
            m_ExportAllContent = EditorGUIUtility.TrTextContentWithIcon("ExportAll", "导出全部配置", "Project");

            m_FileSystemCollector = FileSystemCollector.Instance;
            foreach (var fileSystemData in m_FileSystemCollector.FileSystemDatas)
            {
                m_FoldoutMap[fileSystemData.FileSystem] = true;
            }
        }

        private void Update()
        {
            if (m_ExportFlag)
            {
                m_ExportFlag = false;
                GameBuildPipeline.ProcessFileSystem();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("文件系统收集器", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("[+]", GUILayout.Width(45)))
                {
                    var newFileSystemData = new FileSystemData
                    {
                        FileSystem = $"new FileSystem({m_FileSystemCollector.FileSystemDatas.Count})"
                    };
                    m_FileSystemCollector.FileSystemDatas.Add(newFileSystemData);
                    m_FoldoutMap[newFileSystemData.FileSystem] = true;
                    m_FoldoutMap[string.Format(FileNameTitle, newFileSystemData.FileSystem, "文件列表")] = true;
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
                    m_FoldoutMap[fileSystemData.FileSystem] = EditorGUILayout.Foldout(isFoldout, fileSystemData.FileSystem);
                    {
                        if (m_FoldoutMap[fileSystemData.FileSystem])
                        {
                            m_SelectedFileSystemData = fileSystemData;
                            GUILayout.BeginHorizontal("box");
                            {
                                GUIFileSystemData(fileSystemData);
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();


            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button(m_ExportAllContent, GUILayout.Height(30)))
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
            FileSystemCollector.Save();
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

                string outputPath = fileSystemData.OutPutFolderPath;
                EditorTools.GUIAssetPath("Export Path", ref fileSystemData.OutPutFolderPath, true);

                if (outputPath != fileSystemData.OutPutFolderPath)
                {
                    m_ContentChange = true;
                }

                fileSystemData.FileFullPaths ??= new List<string>();
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Browse...", GUILayout.Width(80)))
                        {
                            string[] outSourceData = StandaloneFileBrowser.OpenFilePanel("选择构建文件系统的文件列表", "", "*", true);
                            if (outSourceData is { Length: > 0 })
                            {
                                foreach (var path in outSourceData)
                                {
                                    var absolutePathToAssetPath = EditorTools.AbsolutePathToProject(path);
                                    if (fileSystemData.FileFullPaths.Contains(absolutePathToAssetPath))
                                    {
                                        Debug.LogWarning($"There is already a file with the path '{path}'");
                                        continue;
                                    }

                                    fileSystemData.FileFullPaths.Add(absolutePathToAssetPath);
                                }

                                m_ContentChange = true;
                            }
                        }

                        if (GUILayout.Button(m_ExportContent, GUILayout.Width(80)))
                        {
                            GameBuildPipeline.ProcessFileSystem(fileSystemData);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    string key = string.Format(FileNameTitle, fileSystemData.FileSystem, "文件列表");
                    bool isFoldout = m_FoldoutMap.GetValueOrDefault(key, true);
                    m_FoldoutMap[key] = EditorGUILayout.Foldout(isFoldout, key);
                    {
                        if (m_FoldoutMap[key])
                        {
                            for (var i = 0; i < fileSystemData.FileFullPaths.Count; i++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    string assetPath = fileSystemData.FileFullPaths[i];

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
                                        EditorGUILayout.LabelField(assetPath, File.Exists(assetPath) ? EditorStyles.label : warningLableGUIStyle);
                                    }

                                    if (GUILayout.Button("x", GUILayout.Width(30)))
                                    {
                                        fileSystemData.FileFullPaths.RemoveAt(i);
                                        i--;
                                        m_ContentChange = true;
                                    }
                                }

                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    }

                    if (fileSystemData.FileFullPaths.Count == 0)
                    {
                        EditorGUILayout.HelpBox("文件列表为空", MessageType.Info);
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
                            if (fileSystemData.FileFullPaths.Contains(assetPath))
                            {
                                continue;
                            }

                            fileSystemData.FileFullPaths.Add(assetPath);
                        }

                        m_ContentChange = true;
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}