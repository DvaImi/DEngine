using Cysharp.Threading.Tasks;
using DEngine.Localization;
using DEngine.Runtime;

namespace Game.Update.Localization
{
    public static class LocalizationExtension
    {
        /// <summary>
        /// 热重载本地化字典
        /// 不支持变体资源设置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="language">将要修改的语言</param>
        /// <param name="userData"></param>
        /// <returns>是否修改成功</returns>
        public static async UniTask<Language> GetDictionaryAsync(this LocalizationComponent self, Language language, object userData = null)
        {
            self.RemoveAllRawStrings();
            var result = await self.LoadDictionaryAsync(UpdateAssetUtility.GetDictionaryAsset(language.ToString(), true), userData);
            if (result != Language.Unspecified)
            {
                Log.Info("Change language success");
                GameEntry.Setting.SetString(Constant.Setting.Language, language.ToString());
                GameEntry.Setting.Save();
            }

            return result;
        }
    }
}