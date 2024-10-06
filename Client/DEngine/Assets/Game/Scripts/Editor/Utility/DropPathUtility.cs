using System;
using System.IO;
using DEngine;
using UnityEditor;
using UnityEngine;
using Event = UnityEngine.Event;
using Object = UnityEngine.Object;

namespace Game.Editor
{
    public static class DropPathUtility
    {
        public static bool DropPathOutType(Rect dropArea, out string assetPath, out bool isFile)
        {
            Event currentEvent = Event.current;
            assetPath = string.Empty;
            if (currentEvent.type == EventType.DragUpdated)
            {
                if (dropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    currentEvent.Use();
                }
            }
            else if (currentEvent.type == EventType.DragPerform)
            {
                if (dropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        assetPath = AssetDatabase.GetAssetPath(draggedObject);
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            currentEvent.Use();
                            Debug.Log(assetPath);
                            isFile = File.Exists(assetPath);
                            return true;
                        }
                    }
                }
            }

            isFile = false;
            return isFile;
        }

        public static bool DropPath(Rect dropArea, out string assetPath, bool isFolder = false)
        {
            Event currentEvent = Event.current;
            assetPath = string.Empty;
            if (currentEvent.type == EventType.DragUpdated)
            {
                if (dropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    currentEvent.Use();
                }
            }
            else if (currentEvent.type == EventType.DragPerform)
            {
                if (dropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        assetPath = AssetDatabase.GetAssetPath(draggedObject);
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            currentEvent.Use();
                            return isFolder ? Directory.Exists(assetPath) : File.Exists(assetPath);
                        }
                    }
                }
            }

            return false;
        }

        public static void DropAssetPath(string label, ref string value, bool isFolder = false)
        {
            EditorGUILayout.BeginHorizontal();
            {
                bool valid = !AssetDatabase.LoadAssetAtPath<Object>(GameSetting.Instance.BuildSettingsConfig);
                GUIStyle style = new GUIStyle(EditorStyles.label);
                style.normal.textColor = Color.yellow;
                value = EditorGUILayout.TextField(label, value, valid ? style : EditorStyles.label);
                Rect textFieldRect = GUILayoutUtility.GetLastRect();
                if (DropPath(textFieldRect, out string assetPath, isFolder))
                {
                    if (assetPath != null && assetPath != value)
                    {
                        value = assetPath;
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(value));
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 转换为相对UnityAssets 的路径
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns></returns>
        public static string ConvertToAssetPath(string inputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                return null;
            }

            var absoluteFolderPath = Application.dataPath + "/" + inputPath[(inputPath.IndexOf("Assets/", StringComparison.Ordinal) + "Assets/".Length)..];
            return Utility.Path.GetRegularPath($"Assets{absoluteFolderPath[Application.dataPath.Length..]}");
        }
    }
}