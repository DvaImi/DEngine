using System;
using System.IO;
using DEngine;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEvent = UnityEngine.Event;

namespace Game.Editor
{
    public static class DropPathUtility
    {
        public static bool DropPathOutType(Rect dropArea, out string assetPath, out bool isFile)
        {
            UnityEvent currentEvent = UnityEvent.current;
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
            UnityEvent currentEvent = UnityEvent.current;
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

        public static bool DropPath(Rect dropArea, out string[] assetPaths)
        {
            UnityEvent currentEvent = UnityEvent.current;
            assetPaths = null;
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
                    assetPaths = new string[DragAndDrop.objectReferences.Length];
                    int index = 0;
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        assetPaths[index++] = AssetDatabase.GetAssetPath(draggedObject);
                        currentEvent.Use();
                    }

                    return true;
                }
            }

            return false;
        }

        public static void DropAndPingAssetPath(string label, ref string value, bool isFolder = false)
        {
            EditorGUILayout.BeginHorizontal();
            {
                bool valid = !AssetDatabase.LoadAssetAtPath<Object>(DEngineSetting.Instance.BuildSettingsConfig);
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

                if (GUILayout.Button("Reveal", GUILayout.Width(80),GUILayout.Height(20)))
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