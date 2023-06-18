using Cysharp.Threading.Tasks;
using DEngine.Localization;
using DEngine.Runtime;

namespace Game
{
    public static class LocalizationExtension
    {
        /// <summary>
        /// 修改系统本地化语言
        /// </summary>
        /// <param name="localization"></param>
        /// <param name="language">将要修改的语言</param>
        /// <returns>是否修改成功</returns>
        public static async UniTask<bool> ChangeLanguage(this LocalizationComponent localization, Language language)
        {
            localization.RemoveAllRawStrings();
            Language result = await localization.LoadDictionaryAsync(language);
            if (result == language)
            {
                GameEntry.Setting.SetString(Constant.Setting.Language, language.ToString());
                GameEntry.Setting.Save();
                return true;
            }

            return false;
        }
    }
}