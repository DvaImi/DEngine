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
    }
}