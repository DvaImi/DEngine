using UnityEngine;
using UnityEngine.UI;

namespace Game.Localization
{
    public class TranslatedText : MonoBehaviour
    {
        [SerializeField]
        private Text m_Text;
        public string phraseKey;
        public bool setTextOnStart = true;

        private void Awake()
        {
            if (m_Text == null)
            {
                m_Text = GetComponent<Text>();
            }
        }

        private void Start()
        {
            if (setTextOnStart)
            {
                SetText();
            }
        }

        public void SetText()
        {
            m_Text.text = GameEntry.Localization.GetString(phraseKey);
        }
    }
}