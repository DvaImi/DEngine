using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// 更新包帮助类
    /// </summary>
    public static class UpdateAllPackageHelper
    {
        private static AddRequest _addRequest;
        private static readonly Queue<(string name, string url)> PackagesToUpdate = new Queue<(string, string)>();
        private static int _allPackagesCount = 0;
        private static int _updatingPackagesIndex = 0;

        /// <summary>
        /// 更新包列表
        /// </summary>
        [MenuItem("Game/Update All Packages", false, 2000)]
        public static void UpdatePackages()
        {
            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            try
            {
                UpdatePackagesFromManifest(manifestPath);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }

        private static void UpdatePackagesFromManifest(string manifestPath)
        {
            string jsonContent = File.ReadAllText(manifestPath);
            JObject manifest = JObject.Parse(jsonContent);
            JObject dependencies = (JObject)manifest["dependencies"];

            if (dependencies != null)
            {
                foreach (var package in dependencies)
                {
                    string packageName = package.Key;
                    string packageUrl = package.Value.ToString();
                    if (packageUrl.EndsWith(".git"))
                    {
                        PackagesToUpdate.Enqueue((packageName, packageUrl));
                    }
                }
            }

            _allPackagesCount = PackagesToUpdate.Count;
            _updatingPackagesIndex = 0;
            if (PackagesToUpdate.Count > 0)
            {
                UpdateNextPackage();
            }
            else
            {
                Debug.Log("No packages to update.");
            }
        }

        private static void UpdateNextPackage()
        {
            if (PackagesToUpdate.Count > 0)
            {
                _updatingPackagesIndex++;
                var (packageName, packageUrl) = PackagesToUpdate.Dequeue();
                Debug.Log($"Updating package: {packageName} from {packageUrl}");
                _addRequest = Client.Add(packageUrl);
                var isCancelableProgressBar = EditorUtility.DisplayCancelableProgressBar("正在更新包", $"{_updatingPackagesIndex}/{_allPackagesCount} ({packageName})", (float)_updatingPackagesIndex / _allPackagesCount);
                EditorApplication.update += UpdatingProgressHandler;
                if (isCancelableProgressBar)
                {
                    EditorUtility.DisplayProgressBar("正在取消更新", "请等待...", 0.5f);
                    PackagesToUpdate.Clear();
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update -= UpdatingProgressHandler;
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                EditorUtility.ClearProgressBar();
                Debug.Log("All packages updated.");
                AssetDatabase.Refresh();
            }
        }

        private static void UpdatingProgressHandler()
        {
            if (_addRequest.IsCompleted)
            {
                if (_addRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"Updated package: {_addRequest.Result.packageId}");
                }
                else if (_addRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError($"Failed to update package: {_addRequest.Error.message}");
                }

                _addRequest = null;
                EditorApplication.update -= UpdatingProgressHandler;
                UpdateNextPackage();
            }
        }
    }
}