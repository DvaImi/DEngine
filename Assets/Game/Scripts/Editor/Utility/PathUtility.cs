using System.IO;
using GameFramework;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public static class PathUtility
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

        public static bool DropPath(Rect dropArea, out string assetPath, bool files = false)
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
                            return files ? File.Exists(assetPath) : Directory.Exists(assetPath);
                        }
                    }
                }
            }
            return false;
        }

        public static void DropAssetPath(string label, ref string value)
        {
            EditorGUILayout.BeginHorizontal();
            {
                value = EditorGUILayout.TextField(label, value);
                Rect textFieldRect = GUILayoutUtility.GetLastRect();
                if (DropPath(textFieldRect, out string assetPath, true))
                {
                    if (assetPath != value)
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
            string absoluteFolderPath = Application.dataPath + "/" + inputPath[(inputPath.IndexOf("Assets/") + "Assets/".Length)..];
            return GameFramework.Utility.Path.GetRegularPath($"Assets{absoluteFolderPath[Application.dataPath.Length..]}");
        }
    }
}