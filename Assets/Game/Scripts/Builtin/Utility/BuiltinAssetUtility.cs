namespace Game
{
    /// <summary>
    /// 内置资源路径工具
    /// </summary>
    public static class BuiltinAssetUtility
    {
        /// <summary>
        ///获取热更新资源路径 
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetCLRUpdateAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/HybridCLRData/HotUpdate/{0}.{1}", assetName, "bytes");
        }

        /// <summary>
        /// 获取热更新AOT资源路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetCLRAOTAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/HybridCLRData/AOT/{0}.{1}", assetName, "bytes");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetCLRLanuchAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/HybridCLRData/Luncher/{0}.{1}", assetName, "prefab");
        }
    }
}
