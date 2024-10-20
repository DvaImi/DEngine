namespace Game
{
    /// <summary>
    /// 内置资源路径工具
    /// </summary>
    public static class BuiltinAssetUtility
    {
        ///  <summary>
        /// 获取程序集资源路径 
        ///  </summary>
        ///  <param name="assetName"></param>
        ///  <param name="suffix"></param>
        ///  <returns></returns>
        public static string GetAssembliesAsset(string assetName, string suffix = "bytes")
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/Assemblies/{0}.{1}", assetName, suffix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetUpdateLauncherAsset()
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/Launcher/{0}.{1}", "UpdateLauncher", "prefab");
        }
    }
}