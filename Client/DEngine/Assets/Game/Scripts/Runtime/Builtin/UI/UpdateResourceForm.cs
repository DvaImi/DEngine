// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-03-30 23:42:36
// 版 本：1.0
// ========================================================

using UnityEngine;
using UnityEngine.UI;

//自动生成于：2023/3/30 23:42:36
namespace Game
{
    public class UpdateResourceForm : MonoBehaviour
    {
        [SerializeField]
        private Slider m_Slider;
        [SerializeField]
        private Text m_Text;
        public void SetProgress(float progressTotal, string descriptionText)
        {
            m_Slider.value = progressTotal;
            m_Text.text = descriptionText;
        }
    }
}