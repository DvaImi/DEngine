namespace Game.Editor.ResourceTools
{
    public enum FilterType
    {
        /// <summary>
        /// 将指定文件夹打成一个Bundle
        /// </summary>
        Root,
        /// <summary>
        /// 指定文件夹下的文件分别打成一个Bundle,会过滤该文件下的子文件夹
        /// </summary>
        Children,
        /// <summary>
        /// 指定文件夹下的子文件夹分别打成一个Bundle
        /// </summary>
        ChildrenFoldersOnly,
        /// <summary>
        /// 指定文件夹下的子文件夹的文件分别打成一个Bundle
        /// </summary>
        ChildrenFilesOnly,
    }
}