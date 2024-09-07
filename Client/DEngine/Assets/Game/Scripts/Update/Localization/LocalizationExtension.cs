using DEngine;
using DEngine.Event;
using DEngine.Localization;
using DEngine.Runtime;

namespace Game.Update
{
    public static class LocalizationExtension
    {
        public static event DEngineAction<Language> OnLanguageChanged;

        public static void Subscribe()
        {
            EventComponent eventComponent = DEngine.Runtime.GameEntry.GetComponent<EventComponent>();

            if (eventComponent == null)
            {
                Log.Fatal("Event manager is invalid.");
                return;
            }

            eventComponent.Subscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            eventComponent.Subscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);
        }

        /// <summary>
        /// 热重载本地化
        /// 不支持变体资源设置
        /// </summary>
        /// <param name="localization"></param>
        /// <param name="language">将要修改的语言</param>
        /// <returns>是否修改成功</returns>
        public static void HotReloadLocalization(this LocalizationComponent localization, Language language, bool readBuiltinLanguage = true)
        {
            localization.RemoveAllRawStrings();
            localization.ReadData(UpdateAssetUtility.GetDictionaryAsset(language.ToString(), true), language);
            if (readBuiltinLanguage)
            {
                GameEntry.BuiltinData.ReadLanguage(language);
            }
        }

        private static void OnLoadDictionarySuccess(object sender, GameEventArgs e)
        {
            LoadDictionarySuccessEventArgs ne = (LoadDictionarySuccessEventArgs)e;

            if (ne.UserData is Language language)
            {
                Log.Info("Change language success");
                GameEntry.Setting.SetString(Constant.Setting.Language, language.ToString());
                GameEntry.Setting.Save();
                OnLanguageChanged?.Invoke(language);
            }
        }

        private static void OnLoadDictionaryFailure(object sender, GameEventArgs e)
        {
            LoadDictionaryFailureEventArgs ne = (LoadDictionaryFailureEventArgs)e;
            if (ne.UserData is not Language language)
            {
                return;
            }

            Log.Error("Can not change {0} language with error message '{1}'.", language, ne.ErrorMessage);
        }
    }
}