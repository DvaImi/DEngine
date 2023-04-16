// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 08:49:54
// 版 本：1.0
// ========================================================
using UnityEditor;

namespace Dvalmi.Editor
{
    public static class OpenFolder
    {
        /// <summary>
        /// 打开 DataTable Path 文件夹。
        /// </summary>
        [MenuItem("Dvalmi/Open Folder/DataTable Path", false, 10)]
        public static void OpenFolderDataTablePath()
        {
            UnityGameFramework.Editor.OpenFolder.Execute(DvalmiSetting.Instance.DataTableExcelPath);
        }

        /// <summary>
        /// 打开 AssetBundle output Path 文件夹。
        /// </summary>
        [MenuItem("Dvalmi/Open Folder/AssetBundle Output", false, 11)]
        public static void OpenFolderAssetBundlePath()
        {
            UnityGameFramework.Editor.OpenFolder.Execute(DvalmiSetting.Instance.AssetBundleOutput);
        }

        /// <summary>
        /// 打开 PublishAppOutput Path 文件夹。
        /// </summary>
        [MenuItem("Dvalmi/Open Folder/Publish Output", false, 12)]
        public static void OpenFolderPublishAppPath()
        {
            UnityGameFramework.Editor.OpenFolder.Execute(DvalmiSetting.Instance.PublishAppOutput);
        }

    }
}