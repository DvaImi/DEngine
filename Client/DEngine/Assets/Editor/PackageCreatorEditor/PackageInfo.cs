namespace Game.Editor.Package
{
    /// <summary>
    /// 定义包信息类，用于保存package.json中的数据
    /// </summary>
    [System.Serializable]
    public class PackageInfo
    {
        public string name = "com.dvalmi.newpackage";
        public string version = "1.0.0";
        public string displayName = "New Package";
        public string description = "A custom Unity package";
        public string unity = "2020.1";
        public string authorName = "Dvalmi";
    }
}