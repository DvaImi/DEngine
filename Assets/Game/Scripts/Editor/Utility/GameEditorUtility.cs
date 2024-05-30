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
        public static TScriptableObject LoadScriptableObject<TScriptableObject>() where TScriptableObject : ScriptableObject
        {
            var settingType = typeof(TScriptableObject);
            var guids = AssetDatabase.FindAssets($"t:{settingType.Name}");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"Create new {settingType.Name}.asset");
                var setting = ScriptableObject.CreateInstance<TScriptableObject>();
                string filePath = $"Assets/{settingType.Name}.asset";
                AssetDatabase.CreateAsset(setting, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return setting;
            }
            else
            {
                if (guids.Length != 1)
                {
                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Debug.LogWarning($"Found multiple file : {path}");
                    }
                    throw new System.Exception($"Found multiple {settingType.Name} files !");
                }

                string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var setting = AssetDatabase.LoadAssetAtPath<TScriptableObject>(filePath);
                return setting;
            }
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
