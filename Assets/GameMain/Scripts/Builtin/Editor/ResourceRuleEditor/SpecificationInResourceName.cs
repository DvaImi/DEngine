// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-30 20:58:44
// 版 本：1.0
// ========================================================
// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-30 18:04:16
// 版 本：1.0
// ========================================================

using System;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GeminiLion.Editor.ResourceTools
{
    public class SpecificationInResourceName : OdinEditorWindow
    {
        [FolderPath(RequireExistingPath = true)]
        public string[] folderPaths;

        private readonly char[] illegalCharacters =
        {
            ' ', '\\', '/', ':', '*', '?', '\"', '<', '>', '|', '+', '[', ']', '{', '}', '%', '#', '&', '`', '~', '\'',
            '@', '-', '\''
        };

        [MenuItem("GeminiLion/Resource Name Check", priority = 30)]
        static void Open()
        {
            var window = GetWindow<SpecificationInResourceName>("Resource Name Check");
            window.minSize = new Vector2(200, 400);
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            bool invalid = folderPaths == null || folderPaths.Length == 0;
            foreach (var folderPath in folderPaths)
            {
                invalid = string.IsNullOrEmpty(folderPath);
            }

            EditorGUI.BeginDisabledGroup(invalid);
            if (GUILayout.Button("Check"))
            {
                for (int i = 0; i < folderPaths.Length; i++)
                {
                    string folderPath = folderPaths[i];

                    if (!Directory.Exists(folderPath))
                    {
                        Debug.LogErrorFormat("Directory does not exist: {0}  index:{1} ", folderPath, i);
                        return;
                    }

                    string[] assetGUIDs = AssetDatabase.FindAssets("", new string[] { folderPath });
                    foreach (string guid in assetGUIDs)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                        if (asset == null)
                        {
                            continue;
                        }

                        // 判断是否包含非法字符
                        bool hasIllegalCharacters = illegalCharacters.Any(c => asset.name.Contains(c));

                        // 替换非法字符并重命名文件
                        if (hasIllegalCharacters)
                        {
                            string illegalChar = illegalCharacters.FirstOrDefault(c => asset.name.Contains(c))
                                .ToString();
                            Debug.LogWarningFormat("{0} has an illegal character '{1}'", asset.name, illegalChar);
                        }

                        // 重命名文件夹
                        string assetFolder = Path.GetDirectoryName(assetPath);
                        string folderName = Path.GetFileName(assetFolder);
                        bool hasFolderIllegalCharacters = illegalCharacters.Any(c => folderName.Contains(c));
                        if (hasFolderIllegalCharacters)
                        {
                            string illegalChar = illegalCharacters.FirstOrDefault(c => asset.name.Contains(c))
                                .ToString();
                            Debug.LogWarningFormat("Folder {0} has an illegal character '{1}'", asset.name,
                                illegalChar);
                        }
                    }
                }
            }

            if (GUILayout.Button("Rename"))
            {
                foreach (var folderPath in folderPaths)
                {
                    if (!Directory.Exists(folderPath))
                    {
                        Debug.LogError("Directory does not exist: " + folderPath);
                        return;
                    }

                    string[] assetGUIDs = AssetDatabase.FindAssets("", new string[] { folderPath });


                    foreach (string guid in assetGUIDs)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                        // 判断是否包含非法字符
                        bool hasIllegalCharacters = illegalCharacters.Any(c => asset.name.Contains(c));

                        // 替换非法字符并重命名文件
                        if (hasIllegalCharacters)
                        {
                            string newAssetName = string.Concat(asset.name.Split(illegalCharacters,
                                StringSplitOptions.RemoveEmptyEntries));
                            AssetDatabase.RenameAsset(assetPath, newAssetName);
                        }

                        // 重命名文件夹
                        string assetFolder = Path.GetDirectoryName(assetPath);
                        string folderName = Path.GetFileName(assetFolder);
                        bool hasFolderIllegalCharacters = illegalCharacters.Any(c => folderName.Contains(c));
                        if (hasFolderIllegalCharacters)
                        {
                            string newFolderName = string.Concat(folderName.Split(illegalCharacters,
                                StringSplitOptions.RemoveEmptyEntries));
                            string newFolderPath = Path.Combine(Path.GetDirectoryName(assetPath), newFolderName);
                            AssetDatabase.RenameAsset(assetFolder, newFolderPath);
                        }
                    }
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Rename completed.");
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}