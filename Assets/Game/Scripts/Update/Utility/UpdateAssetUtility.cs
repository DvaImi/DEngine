namespace Game.Update
{
    /// <summary>
    /// 可更新资源路径工具
    /// </summary>
    public static class UpdateAssetUtility
    {
        /// <summary>
        /// 获取数据表路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="fromBytes"></param>
        /// <returns></returns>
        public static string GetDataTableAsset(string assetName, bool fromBytes)
        {
            return DEngine.Utility.Text.Format("Assets/Game/DataTables/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        /// <summary>
        /// 获取字典路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="fromBytes"></param>
        /// <returns></returns>
        public static string GetDictionaryAsset(string assetName, bool fromBytes)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Localization/{0}/{1}.{2}", assetName, assetName, fromBytes ? "bytes" : "txt");
        }

        /// <summary>
        /// 获取字体路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string GetFontAsset(string assetName, string suffix = "ttf")
        {
            return DEngine.Utility.Text.Format("Assets/Game/Fonts/{0}.{1}", assetName, suffix);
        }

        /// <summary>
        /// 获取场景路径
        /// </summary>
        /// <param name="assetName"></param>
        /// 
        /// <returns></returns>
        public static string GetSceneAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Scenes/{0}/{1}.unity", assetName, assetName);
        }

        /// <summary>
        /// 获取音乐路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string GetMusicAsset(string assetName, string suffix = "mp3")
        {
            return DEngine.Utility.Text.Format("Assets/Game/Musics/{0}.{1}", assetName, suffix);
        }

        /// <summary>
        /// 获取声音路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetSoundAsset(string assetName, string suffix = "mp3")
        {
            return DEngine.Utility.Text.Format("Assets/Game/Sounds/{0}.{1}", assetName, suffix);
        }

        /// <summary>
        /// 获取实体路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetEntityAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Entities/{0}.prefab", assetName);
        }

        /// <summary>
        /// 获取UI界面路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetUIFormAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/UI/UIForms/{0}.prefab", assetName);
        }

        /// <summary>
        /// 获取UI声音路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetUISoundAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/UI/UISounds/{0}.mp3", assetName);
        }
    }
}
