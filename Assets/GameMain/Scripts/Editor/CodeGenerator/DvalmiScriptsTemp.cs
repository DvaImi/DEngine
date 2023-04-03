using System.IO;
namespace Dvalmi.Editor
{
    /// <summary>
    /// 脚本头文件
    /// </summary>
    public class DvalmiScriptsTemp : UnityEditor.AssetModificationProcessor
    {
        /// <summary>
        /// 在资源创建时调用
        /// </summary>
        /// <param name="path">自动传入资源路径</param>
        public static void OnWillCreateAsset(string path)
        {
            path = path.Replace(".meta", "");
            if (!path.EndsWith(".cs")) return;
            string allText = "// ========================================================\r\n"
                             + "// 描述：\r\n"
                             + "// 作者：Dvalmi \r\n"
                             + "// 创建时间：#CreateTime#\r\n"
                             + "// ========================================================\r\n";
            allText += File.ReadAllText(path);
            allText = allText.Replace("#CreateTime#", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            File.WriteAllText(path, allText);
        }
    }
}


