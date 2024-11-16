namespace Game.Update
{
    /// <summary>
    /// 可更新资源路径工具
    /// </summary>
    public static class UpdateAssetUtility
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="fromBytes"></param>
        /// <returns></returns>
        public static string GetConfigAsset(string assetName, bool fromBytes = true)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/Configs/{0}.{1}", assetName, fromBytes ? "bytes" : "json");
        }

        /// <summary>
        /// 获取数据表路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="fromBytes"></param>
        /// <returns></returns>
        public static string GetDataTableAsset(string assetName, bool fromBytes = true)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/DataTables/{0}.{1}", assetName, fromBytes ? "bytes" : "json");
        }

        /// <summary>
        /// 获取字典路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="fromBytes"></param>
        /// <returns></returns>
        public static string GetDictionaryAsset(string assetName, bool fromBytes = true)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/Localization/{0}/{1}.{2}", assetName, GameEntry.Localization.Language, fromBytes ? "bytes" : "txt");
        }

        /// <summary>
        /// 获取字体路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string GetFontAsset(string assetName, string suffix = "ttf")
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/Fonts/{0}.{1}", assetName, suffix);
        }

        /// <summary>
        /// 获取场景路径
        /// </summary>
        /// <param name="assetName"></param>
        /// 
        /// <returns></returns>
        public static string GetSceneAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/Scenes/{0}/{1}.unity", assetName, assetName);
        }

        /// <summary>
        /// 获取音乐路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetMusicAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/Audios/Musics/{0}", assetName);
        }

        /// <summary>
        /// 获取声音路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetSoundAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/Audios/Sounds/{0}", assetName);
        }

        /// <summary>
        /// 获取实体路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetEntityAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/Entities/{0}.prefab", assetName);
        }

        /// <summary>
        /// 获取UI界面路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetUIFormAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/UI/UIForms/{0}.prefab", assetName);
        }

        /// <summary>
        /// 获取UI声音路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetUISoundAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/Audios/UISounds/{0}", assetName);
        }

        /// <summary>
        /// 获取ScriptableAssets 资源路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetScriptableAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/ScriptableAssets/{0}.asset", assetName);
        }

        /// <summary>
        /// 获取着色器变体资源路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetShaderVariantsAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Bundles/ShaderVariants/{0}.shadervariants", assetName);
        }
    }
}