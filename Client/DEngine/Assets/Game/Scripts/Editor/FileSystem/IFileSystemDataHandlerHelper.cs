namespace Game.Editor.ResourceTools
{
    public interface IFileSystemDataHandlerHelper
    {
        /// <summary>
        /// 获取文件的数据流
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        byte[] GetBytes(string fullPath);
    }
}