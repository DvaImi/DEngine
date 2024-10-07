using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DEngine;
using Game.Editor.BuildPipeline;
using Game.Update;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Editor.ResourceTools
{
    public class FileSystemCollectorWindow : EditorWindow
    {
        private const string NoneOptionName = "<None>";
        private FileSystemCollector m_FileSystemCollector;
        private SerializedObject m_SerializedCollector;
        private SerializedProperty m_FileSystemDatasProperty;
        private Vector2 m_ScrollPosition;
        private GUIContent m_BuildContent;
        private GUIContent m_SaveContent;
        private Object m_ExportPath;
        private bool m_ExportFlag;
        private IFileSystemDataHandlerHelper m_FileSystemDataHandlerHelper;
        private string[] m_FileSystemHandlerTypeNames;

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
            m_SaveContent = EditorGUIUtility.TrTextContentWithIcon("Save", "保存配置", "SaveAs");
            m_FileSystemCollector = EditorTools.LoadScriptableObject<FileSystemCollector>();
            m_SerializedCollector = new SerializedObject(m_FileSystemCollector);
            m_FileSystemDatasProperty = m_SerializedCollector.FindProperty("FileSystemDatas");
            string folder = EditorTools.GetRegularPath(Path.GetDirectoryName(UpdateAssetUtility.GetFileSystemAsset("")));
            if (EditorTools.CreateDirectory(folder))
            {
                AssetDatabase.Refresh();
            }
            m_ExportPath = AssetDatabase.LoadAssetAtPath<Object>(folder);
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
            m_SerializedCollector.Update();

            GUILayout.Label("File System Collector", EditorStyles.boldLabel);

            GUILayout.Space(5);
            EditorGUILayout.ObjectField("Export Path", m_ExportPath, typeof(DefaultAsset), false);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("File System Handler", GUILayout.Width(160f));
                int selectedIndex = EditorGUILayout.Popup(m_FileSystemCollector.FileSystemHandlerTypeNameIndex, m_FileSystemHandlerTypeNames);
                if (selectedIndex != m_FileSystemCollector.FileSystemHandlerTypeNameIndex)
                {
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
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false);
            {
                EditorGUILayout.PropertyField(m_FileSystemDatasProperty, new GUIContent("File System Defines"), true);
            }
            EditorGUILayout.EndScrollView();

            m_SerializedCollector.ApplyModifiedProperties();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button(m_BuildContent, GUILayout.Height(30)))
                {
                    m_ExportFlag = true;
                }

                if (GUILayout.Button(m_SaveContent, GUILayout.Height(30)))
                {
                    EditorUtility.SetDirty(m_FileSystemCollector);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("Save success.");
                }
            }
            GUILayout.EndHorizontal();
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