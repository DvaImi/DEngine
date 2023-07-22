namespace Game
{
    public static class AssetUtility
    {

        public static string GetConfigAsset(string assetName, bool fromBytes)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Configs/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }


        public static string GetDataTableAsset(string assetName, bool fromBytes)
        {
            return DEngine.Utility.Text.Format("Assets/Game/DataTables/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetDictionaryAsset(string assetName, bool fromBytes)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Localization/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetFontAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Fonts/{0}.ttf", assetName);
        }

        public static string GetSceneAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Scenes/{0}/{1}.unity", assetName, assetName);
        }

        public static string GetMusicAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Music/{0}.mp3", assetName);
        }

        public static string GetSoundAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Sounds/{0}.mp3", assetName);
        }

        public static string GetEntityAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/Entities/{0}.prefab", assetName);
        }

        public static string GetUIFormAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/UI/UIForms/{0}.prefab", assetName);
        }

        public static string GetUISoundAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/UI/UISounds/{0}.mp3", assetName);
        }

        public static string GetCLRUpdateAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/HybridCLRData/HotUpdate/{0}.{1}", assetName, "bytes");
        }

        public static string GetCLRAOTAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/HybridCLRData/AOT/{0}.{1}", assetName, "bytes");
        }

        public static string GetCLRLanuchAsset(string assetName)
        {
            return DEngine.Utility.Text.Format("Assets/Game/HybridCLRData/Luncher/{0}.{1}", assetName, "prefab");
        }
    }
}
