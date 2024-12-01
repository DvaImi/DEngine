using System.Collections.Generic;
using System.Linq;
using DEngine.Runtime;
using Fantasy.Async;
using Game.Update.Sound;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Game.Update.Input
{
    public class GameInputComponent : Fantasy.Entitas.Entity
    {
        private InputActionAsset m_ActionAsset;
        private Vector2 m_LastMousePosition;
        private bool m_IsDragging;
        private int m_GlobalUIClickSoundId = -1;

        public async FTask Awake()
        {
            m_ActionAsset = await GameEntry.Resource.LoadAssetAsync<InputActionAsset>(UpdateAssetUtility.GetScriptableAsset("GameInputActions", "inputactions"));
            if (!m_ActionAsset)
            {
                Log.Error("Failed to load GameInputActions asset");
                return;
            }

            m_ActionAsset.Enable();
        }

        public void Destroy()
        {
            m_ActionAsset.Disable();
        }

        /// <summary>
        /// 获取鼠标屏幕坐标
        /// </summary>
        public Vector2 GetMousePosition()
        {
            return Mouse.current?.position.ReadValue() ?? Vector2.zero;
        }

        /// <summary>
        /// 获取鼠标滚轮滚动值
        /// </summary>
        public Vector2 GetMouseScrollDelta()
        {
            return Mouse.current?.scroll.ReadValue() ?? Vector2.zero;
        }

        /// <summary>
        /// 检查鼠标是否点击了指定按钮
        /// </summary>
        public bool IsMouseButtonPressed(int button)
        {
            switch (button)
            {
                case 0:  return Mouse.current?.leftButton.isPressed ?? false;
                case 1:  return Mouse.current?.rightButton.isPressed ?? false;
                case 2:  return Mouse.current?.middleButton.isPressed ?? false;
                default: return false;
            }
        }

        /// <summary>
        /// 检查鼠标是否刚刚点击（单击）
        /// </summary>
        public bool IsMouseClicked(int button)
        {
            switch (button)
            {
                case 0:  return Mouse.current?.leftButton.wasPressedThisFrame ?? false;
                case 1:  return Mouse.current?.rightButton.wasPressedThisFrame ?? false;
                case 2:  return Mouse.current?.middleButton.wasPressedThisFrame ?? false;
                default: return false;
            }
        }

        /// <summary>
        /// 检查鼠标是否刚刚释放
        /// </summary>
        public bool IsMouseReleased(int button)
        {
            switch (button)
            {
                case 0:  return Mouse.current?.leftButton.wasReleasedThisFrame ?? false;
                case 1:  return Mouse.current?.rightButton.wasReleasedThisFrame ?? false;
                case 2:  return Mouse.current?.middleButton.wasReleasedThisFrame ?? false;
                default: return false;
            }
        }

        /// <summary>
        /// 检查是否正在拖拽
        /// </summary>
        public bool IsDragging()
        {
            if (IsMouseClicked(0))
            {
                m_IsDragging = true;
                m_LastMousePosition = GetMousePosition();
            }
            else if (IsMouseReleased(0))
            {
                m_IsDragging = false;
            }
            else if (m_IsDragging)
            {
                Vector2 currentMousePosition = GetMousePosition();
                if (m_LastMousePosition != currentMousePosition)
                {
                    m_LastMousePosition = currentMousePosition;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 鼠标是否悬停在UI上
        /// </summary>
        public bool IsMouseOverUI()
        {
            return EventSystem.current?.IsPointerOverGameObject() ?? false;
        }

        /// <summary>
        /// 是否点击到UI
        /// </summary>
        public bool IsClickOverUI()
        {
            if (Mouse.current == null)
            {
                return false;
            }

            return Mouse.current.leftButton.wasPressedThisFrame && EventSystem.current?.IsPointerOverGameObject(PointerInputModule.kMouseLeftId) == true;
        }

        /// <summary>
        /// 判断鼠标或触控点击的是否是可交互的UI
        /// </summary>
        public bool IsClickOverInteractableUI()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            {
                return false;
            }

            var eventSystem = EventSystem.current;
            if (!eventSystem)
            {
                return false;
            }

            var pointerCurrentOverUI = eventSystem.currentSelectedGameObject;

            if (pointerCurrentOverUI)
            {
                var selectable = pointerCurrentOverUI.GetComponent<Selectable>();
                if (selectable && selectable.interactable)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsKeyPressed(Key key)
        {
            return Keyboard.current[key].wasPressedThisFrame;
        }

        public bool IsKeyDown(Key key)
        {
            return Keyboard.current[key].isPressed;
        }

        public bool IsKeyUp(Key key)
        {
            return Keyboard.current[key].wasReleasedThisFrame;
        }

        public List<Key> GetPressedKeys()
        {
            List<Key> pressedKeys = new List<Key>();
            foreach (var key in Keyboard.current.allKeys)
            {
                if (key.isPressed)
                {
                    pressedKeys.Add(key.keyCode);
                }
            }

            return pressedKeys;
        }

        public List<Vector2> GetTouchPositions()
        {
            List<Vector2> touchPositions = new List<Vector2>();
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.IsPressed())
                {
                    touchPositions.Add(touch.position.ReadValue());
                }
            }

            return touchPositions;
        }

        /// <summary>
        /// 使用鼠标进行射线检测
        /// </summary>
        /// <param name="layerMask">指定的层级掩码</param>
        /// <param name="hit">碰撞信息</param>
        /// <returns>是否击中了物体</returns>
        public bool RaycastFromMouse(out RaycastHit hit, int layerMask = ~0)
        {
            hit = default;

            if (!GameEntry.Scene.MainCamera)
            {
                Debug.LogWarning("No main camera found.");
                return false;
            }

            var ray = GameEntry.Scene.MainCamera.ScreenPointToRay(GetMousePosition());
            return Physics.Raycast(ray, out hit, float.MaxValue, layerMask);
        }

        /// <summary>
        /// 使用鼠标进行2D射线检测
        /// </summary>
        /// <param name="layerMask">指定的层级掩码</param>
        /// <returns>是否击中了物体</returns>
        public Collider2D RaycastFromMouse2D(int layerMask = ~0)
        {
            Vector2 mousePos = GameEntry.Scene.MainCamera?.ScreenToWorldPoint(GetMousePosition()) ?? Vector2.zero;
            return Physics2D.OverlapPoint(mousePos, layerMask);
        }

        /// <summary>
        /// 设置UI的全局点击音效
        /// </summary>
        /// <param name="soundId"></param>
        public void SetGlobalUIClickSound(int soundId)
        {
            if (soundId < 0)
            {
                Log.Warning("Global UI Click Sound is invalid.");
                return;
            }

            m_GlobalUIClickSoundId = soundId;

            var uiActionMap = m_ActionAsset.FindActionMap("UI");
            if (uiActionMap == null)
            {
                Log.Error("ActionMap 'UI' not found.");
                return;
            }

            var clickAction = uiActionMap.FindAction("Click");
            if (clickAction == null)
            {
                Log.Error("Action 'Click' not found in 'UI' ActionMap.");
                return;
            }

            clickAction.performed -= InternalPlayGlobalUIClickSound;
            clickAction.performed += InternalPlayGlobalUIClickSound;
            uiActionMap.Enable();
            clickAction.Enable();
        }

        private void InternalPlayGlobalUIClickSound(InputAction.CallbackContext context)
        {
            if (!context.ReadValueAsButton())
            {
                return;
            }

            var eventSystem = EventSystem.current;
            if (!eventSystem)
            {
                Log.Warning("EventSystem is not available. Ensure it is initialized in the scene.");
                return;
            }

            Vector2 inputPosition;
            if (Mouse.current != null && Mouse.current.position.IsPressed())
            {
                inputPosition = Mouse.current.position.ReadValue();
            }
            else
            {
                return;
            }

            var pointerEventData = new PointerEventData(eventSystem)
            {
                position = inputPosition
            };

            var raycastResults = ListPool<RaycastResult>.Get();
            eventSystem.RaycastAll(pointerEventData, raycastResults);

            bool uiClicked = raycastResults.Select(result => result.gameObject.GetComponent<Button>()).Any(button => button && button.interactable);
            if (uiClicked)
            {
                GameEntry.Sound.PlayUISound(m_GlobalUIClickSoundId);
            }

            ListPool<RaycastResult>.Release(raycastResults);
        }
    }
}