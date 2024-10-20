#if UNITY_2019_4_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Game.Editor
{
    public static class UxmlLoader
    {
        private static readonly Dictionary<Type, string> UxmlDic = new();

        /// <summary>
        /// 加载窗口的布局文件
        /// </summary>
        public static VisualTreeAsset LoadWindowUxml<TWindow>() where TWindow : class
        {
            var windowType = typeof(TWindow);

            // 缓存里查询并加载
            if (UxmlDic.TryGetValue(windowType, out string uxmlGuid))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(uxmlGuid);
                if (string.IsNullOrEmpty(assetPath))
                {
                    UxmlDic.Clear();
                    throw new Exception($"Invalid UXML GUID : {uxmlGuid} ! Please close the window and open it again !");
                }

                var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
                return treeAsset;
            }

            // 全局搜索并加载
            string[] guids = AssetDatabase.FindAssets(windowType.Name);
            if (guids.Length == 0)
            {
                throw new Exception($"Not found any assets : {windowType.Name}");
            }

            foreach (string assetGuid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (assetType == typeof(VisualTreeAsset))
                {
                    UxmlDic.Add(windowType, assetGuid);
                    var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
                    return treeAsset;
                }
            }

            throw new Exception($"Not found UXML file : {windowType.Name}");
        }
    }
}
#endif