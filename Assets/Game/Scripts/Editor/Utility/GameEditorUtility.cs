using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Editor
{
    public static class GameEditorUtility
    {
        public static T GetScriptableObject<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            if (guids.Length != 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                T obj = AssetDatabase.LoadAssetAtPath<T>(path);
                if (obj != null)
                {
                    return obj;
                }
            }

            T newObject = ScriptableObject.CreateInstance<T>();
            if (newObject == null)
            {
                Debug.LogError("Failed to create a new ScriptableObject of type " + typeof(T));
            }
            return newObject;
        }

        public static T[] GetScriptableObjects<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            T[] result = new T[] { };
            if (guids.Length != 0)
            {
                result = new T[guids.Length];
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    result[i] = AssetDatabase.LoadAssetAtPath<T>(path);
                }
            }
            return result;
        }

        public static long CalculateAssetsSize(Object[] assetObjects)
        {
            long size = 0;
            if (assetObjects == null)
            {
                return size;
            }

            foreach (var asset in assetObjects)
            {
                size += CalculateAssetSize(asset);
            }

            return size;
        }

        public static long CalculateAssetSize(Object assetObject)
        {
            string assetPath = AssetDatabase.GetAssetPath(assetObject);
            return AssetDatabase.IsValidFolder(assetPath) ? CalculateFolderSize(assetPath) : CalculateFileSize(assetPath);
        }

        public static long CalculateFolderSize(string folderPath)
        {
            DirectoryInfo directoryInfo = new(folderPath);
            FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

            long totalSize = 0;

            foreach (FileInfo fileInfo in fileInfos)
            {
                totalSize += fileInfo.Length;
            }

            return totalSize;
        }

        public static long CalculateFileSize(string filePath)
        {
            FileInfo fileInfo = new(filePath);
            return fileInfo.Length;
        }

        public static void EditorDisplay(string title, string message, string ok, string cancel, System.Action action)
        {
            if (EditorUtility.DisplayDialog(title, message, ok, cancel))
            {
                action?.Invoke();
            }
        }
     
        public static List<Type> GetAssignableTypes(Type parentType)
        {
            TypeCache.TypeCollection collection = TypeCache.GetTypesDerivedFrom(parentType);
            return collection.ToList();
        }
    }
}
