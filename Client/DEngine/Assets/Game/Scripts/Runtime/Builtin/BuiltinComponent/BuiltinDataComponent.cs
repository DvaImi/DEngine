﻿using System;
using DEngine;
using DEngine.Localization;
using DEngine.Runtime;
using Game.Debugger;
using Game.Sound;
using UnityEngine;

namespace Game
{
    public class BuiltinDataComponent : DEngineComponent
    {
        [SerializeField] private BuiltinData m_BuiltinData;

        private NativeDialogForm m_NativeDialogForm;

        public BuiltinData Builtin => m_BuiltinData;

        public void InitLanguageSettings()
        {
            if (GameEntry.Base.EditorResourceMode && GameEntry.Base.EditorLanguage != Language.Unspecified)
            {
                // 编辑器资源模式直接使用 Inspector 上设置的语言
                return;
            }

            Language language = GameEntry.Localization.Language;
            if (GameEntry.Setting.HasSetting(Constant.Setting.Language))
            {
                try
                {
                    string languageString = GameEntry.Setting.GetString(Constant.Setting.Language);
                    language = (Language)Enum.Parse(typeof(Language), languageString);
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            }

            if (language != Language.English && language != Language.ChineseSimplified && language != Language.ChineseTraditional && language != Language.Korean)
            {
                // 若是暂不支持的语言，则使用英语
                language = Language.English;

                GameEntry.Setting.SetString(Constant.Setting.Language, language.ToString());
                GameEntry.Setting.Save();
            }

            GameEntry.Localization.Language = language;
            Log.Info("Init language settings complete, current language is '{0}'.", language.ToString());
            ReadLanguage(language);
        }

        private void ReadLanguage(Language language)
        {
            if (!Builtin.BuildinLanguage)
            {
                return;
            }

            GameEntry.Localization.ParseData(Builtin.BuildinLanguage[language]);
        }

        public static void InitSoundSettings()
        {
            GameEntry.Sound.Mute("Music", GameEntry.Setting.GetBool(Constant.Setting.MusicMuted, false));
            GameEntry.Sound.SetVolume("Music", GameEntry.Setting.GetFloat(Constant.Setting.MusicVolume, 0.3f));
            GameEntry.Sound.Mute("Sound", GameEntry.Setting.GetBool(Constant.Setting.SoundMuted, false));
            GameEntry.Sound.SetVolume("Sound", GameEntry.Setting.GetFloat(Constant.Setting.SoundVolume, 1f));
            GameEntry.Sound.Mute("UISound", GameEntry.Setting.GetBool(Constant.Setting.UISoundMuted, false));
            GameEntry.Sound.SetVolume("UISound", GameEntry.Setting.GetFloat(Constant.Setting.UISoundVolume, 1f));
            Log.Info("Init sound settings complete.");
        }

        public static void InitExtensionEventHandle()
        {
            UniTaskExtension.Unsubscribe();
            UniTaskExtension.Subscribe();
        }

        public static void InitCurrentVariant()
        {
            if (GameEntry.Base.EditorResourceMode)
            {
                // 编辑器资源模式不使用 AssetBundle，也就没有变体了
                return;
            }

            GameEntry.Resource.SetCurrentVariant(VariantHelper.GetVariant(GameEntry.Localization.Language));
            Log.Info("Init current variant complete.");
            Log.Info($"Current variant is {GameEntry.Resource.CurrentVariant}.");
        }

        public void OpenDialog(DialogParams dialogParams)
        {
            if (!m_NativeDialogForm)
            {
                m_NativeDialogForm = Instantiate(Builtin.NativeDialogFormTemplate);
            }

            m_NativeDialogForm.OnOpen(dialogParams);
        }

        public void DestroyDialog()
        {
            if (!m_NativeDialogForm)
            {
                return;
            }

            Destroy(m_NativeDialogForm.gameObject);
        }
    }
}