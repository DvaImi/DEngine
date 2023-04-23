// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-16 08:49:54
// 版 本：1.0
// ========================================================
using UnityEditor;

namespace GeminiLion.Editor
{
    public static class OpenFolder
    {
        /// <summary>
        /// 打开 DataTable Path 文件夹。
        /// </summary>
        [MenuItem("GeminiLion/Open Folder/DataTable Path", false, 10)]
        public static void OpenFolderDataTablePath()
        {
            InternalOpenFolder(GeminiLionSetting.Instance.DataTableExcelPath);
        } 
        
        /// <summary>
        /// 打开 DataTable Path 文件夹。
        /// </summary>
        [MenuItem("GeminiLion/Open Folder/Dictionary Path", false, 11)]
        public static void OpenFolderDictionaryPath()
        {
            InternalOpenFolder(GeminiLionSetting.Instance.DictionaryExclePath);
        } 
        
        /// <summary>
        /// 打开 DataTable Path 文件夹。
        /// </summary>
        [MenuItem("GeminiLion/Open Folder/Config Path", false, 12)]
        public static void OpenFolderConfigPath()
        {
            InternalOpenFolder(GeminiLionSetting.Instance.ConfigExcelPath);
        }

        /// <summary>
        /// 打开 AssetBundle output Path 文件夹。
        /// </summary>
        [MenuItem("GeminiLion/Open Folder/AssetBundle Output", false, 13)]
        public static void OpenFolderAssetBundlePath()
        {
            InternalOpenFolder(GeminiLionSetting.Instance.AssetBundleOutput);
        }

        /// <summary>
        /// 打开 PublishAppOutput Path 文件夹。
        /// </summary>
        [MenuItem("GeminiLion/Open Folder/Publish Output", false, 14)]
        public static void OpenFolderPublishAppPath()
        {
            InternalOpenFolder(GeminiLionSetting.Instance.PublishAppOutput);
        }

        internal static void InternalOpenFolder(string path)
        {
            UnityGameFramework.Editor.OpenFolder.Execute(path);
        }
    }
}