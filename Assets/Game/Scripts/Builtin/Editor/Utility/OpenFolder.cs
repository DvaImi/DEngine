// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-16 08:49:54
// 版 本：1.0
// ========================================================
using UnityEditor;

namespace Game.Editor
{
    public static class OpenFolder
    {
        /// <summary>
        /// 打开 DataTable Path 文件夹。
        /// </summary>
        [MenuItem("Tools/Open Folder/DataTable Path", false, 10)]
        public static void OpenFolderDataTablePath()
        {
            UnityGameFramework.Editor.OpenFolder.Execute(DataTableSetting.Instance.ExcelsFolder);
        } 
        
        /// <summary>
        /// 打开 AssetBundle output Path 文件夹。
        /// </summary>
        [MenuItem("Tools/Open Folder/AssetBundle Output", false, 13)]
        public static void OpenFolderAssetBundlePath()
        {
            UnityGameFramework.Editor.OpenFolder.Execute(EditorPrefs.GetString("AssetBundleOutput"));
        }

        /// <summary>
        /// 打开 PublishAppOutput Path 文件夹。
        /// </summary>
        [MenuItem("Tools/Open Folder/Publish Output", false, 14)]
        public static void OpenFolderPublishAppPath()
        {
            UnityGameFramework.Editor.OpenFolder.Execute(EditorPrefs.GetString("PublishAppOutput"));
        }
    }
}