using UnityEngine;
using UnityGameFramework.Runtime;

namespace Dvalmi
{
    public class BuiltinDataComponent : GameFrameworkComponent
    {
        [SerializeField]
        private TextAsset m_DefaultDictionaryTextAsset = null;

        [SerializeField]
        private UpdateResourceForm m_UpdateResourceFormTemplate = null;

        public UpdateResourceForm UpdateResourceFormTemplate
        {
            get
            {
                return m_UpdateResourceFormTemplate;
            }
        }

        private NativeDialogForm m_NativeDialogForm;
        [SerializeField]
        private NativeDialogForm m_NativeDialogFormTemplate = null;
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

        /// <summary>
        /// （游戏加载前）打开原生对话框。
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
        /// （游戏加载后）删除原生对话框。
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
