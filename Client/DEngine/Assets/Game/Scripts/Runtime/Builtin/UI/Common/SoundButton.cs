using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    public class SoundButton : Button
    {
        public static event Action GlobalUIClickSoundHandler;

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            // 只响应左键点击
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            // 确保按钮处于激活和可交互状态
            if (!IsActive() || !IsInteractable())
            {
                return;
            }

            // 触发全局点击音效事件
            GlobalUIClickSoundHandler?.Invoke();
        }
    }
}