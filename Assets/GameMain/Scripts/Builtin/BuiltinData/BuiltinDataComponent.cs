using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Dvalmi
{
    public class BuiltinDataComponent : GameFrameworkComponent
    {
        [SerializeField]
        private TextAsset m_BuildInfoTextAsset = null;

        [SerializeField]
        private TextAsset m_PreloadInfoTextAsset = null;

        [SerializeField]
        private TextAsset m_DefaultDictionaryTextAsset = null;

        [SerializeField]
        private TextAsset m_HotfixTextAsset = null;

        [SerializeField]
        private UpdateResourceForm m_UpdateResourceFormTemplate = null;

        public UpdateResourceForm UpdateResourceFormTemplate
        {
            get
            {
                return m_UpdateResourceFormTemplate;
            }
        }

        public BuildInfo BuildInfo { get; private set; } = null;

        public PreloadInfo PreloadInfo { get; private set; } = null;

        public HotfixInfo HotfixInfo { get; private set; } = null;

        private NativeDialogForm m_NativeDialogForm;
        [SerializeField]
        private NativeDialogForm m_NativeDialogFormTemplate = null;

        public void InitHotfixInfo()
        {
            if (m_HotfixTextAsset == null || string.IsNullOrEmpty(m_HotfixTextAsset.text))
            {
                Log.Info("Hotfix info can not be found or empty.");
                return;
            }

            HotfixInfo = Utility.Json.ToObject<HotfixInfo>(m_HotfixTextAsset.text);
            if (HotfixInfo == null)
            {
                Log.Warning("Parse hotfix info failure.");
                return;
            }
        }

        public void InitBuildInfo()
        {
            if (m_BuildInfoTextAsset == null || string.IsNullOrEmpty(m_BuildInfoTextAsset.text))
            {
                Log.Info("Build info can not be found or empty.");
                return;
            }

            BuildInfo = Utility.Json.ToObject<BuildInfo>(m_BuildInfoTextAsset.text);
            if (BuildInfo == null)
            {
                Log.Warning("Parse build info failure.");
                return;
            }
        }

        public void InitDefaultDictionary()
        {
            if (m_DefaultDictionaryTextAsset == null || string.IsNullOrEmpty(m_DefaultDictionaryTextAsset.text))
            {
                Log.Info("Default dictionary can not be found or empty.");
                return;
            }

            if (!GameEntry.Localization.ParseData(m_DefaultDictionaryTextAsset.text))
            {
                Log.Warning("Parse default dictionary failure.");
                return;
            }
        }

        public void InitPreloadInfo()
        {
            if (m_PreloadInfoTextAsset == null || string.IsNullOrEmpty(m_PreloadInfoTextAsset.text))
            {
                Log.Info("Preload info can not be found or empty.");
                return;
            }

            PreloadInfo = Utility.Json.ToObject<PreloadInfo>(m_PreloadInfoTextAsset.text);
            if (PreloadInfo == null)
            {
                Log.Warning("Parse preload info failure.");
                return;
            }
        }

        /// <summary>
        /// 打开原生对话框。
        /// </summary>
        /// <param name="dialogParams"></param>
        public void OpenDialog(DialogParams dialogParams)
        {
            if (m_NativeDialogForm == null)
            {
                m_NativeDialogForm = Instantiate(m_NativeDialogFormTemplate);
            }

            m_NativeDialogForm.OnOpen(dialogParams);
        }

        /// <summary>
        /// 删除原生对话框。
        /// </summary>
        public void DestroyDialog()
        {
            if (m_NativeDialogForm == null)
            {
                return;
            }

            Destroy(m_NativeDialogForm);
            m_NativeDialogForm = null;
        }
    }
}
