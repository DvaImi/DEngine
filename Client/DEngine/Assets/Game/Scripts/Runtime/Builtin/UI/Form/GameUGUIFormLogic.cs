using System.Collections.Generic;
using DEngine;
using DEngine.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// 可更新界面基类
    /// </summary>
    public abstract class GameUGUIFormLogic : UIFormLogic
    {
        protected const int DepthFactor = 100;
        protected Canvas CachedCanvas;
        protected CanvasGroup CanvasGroup;
        protected readonly List<Canvas> CachedCanvasContainer = new();
        protected int OriginalDepth { get; set; }
        protected int Depth => CachedCanvas.sortingOrder;

        /// <summary>
        /// UI事件容器
        /// </summary>
        public EventContainer EventContainer { get; private set; }

        /// <summary>
        /// 缓存的RectTransform
        /// </summary>
        public RectTransform CachedRectTransform { get; private set; }

        /// <summary>
        /// 初始化界面，设置组件和默认状态
        /// </summary>
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            CachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            CachedCanvas.overrideSorting = true;
            OriginalDepth = CachedCanvas.sortingOrder;

            CanvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

            CachedRectTransform = GetComponent<RectTransform>();
            CachedRectTransform.anchorMin = Vector2.zero;
            CachedRectTransform.anchorMax = Vector2.one;
            CachedRectTransform.anchoredPosition = Vector2.zero;
            CachedRectTransform.sizeDelta = Vector2.zero;

            gameObject.GetOrAddComponent<GraphicRaycaster>();

            EventContainer = EventContainer.Create(this);
        }

        /// <summary>
        /// 恢复界面时调用，恢复原本的UI状态并执行显示过渡
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();
        }

        /// <summary>
        /// 每帧更新时调用，用于处理UI的更新逻辑
        /// </summary>
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 暂停UI时调用，通常用于暂停交互等
        /// </summary>
        protected override void OnPause()
        {
            base.OnPause();
        }

        /// <summary>
        /// UI被遮挡时调用，用于处理遮挡状态
        /// </summary>
        protected override void OnCover()
        {
            base.OnCover();
        }

        /// <summary>
        /// UI恢复显示时调用，用于恢复显示状态
        /// </summary>
        protected override void OnReveal()
        {
            base.OnReveal();
        }

        /// <summary>
        /// UI获得焦点时调用，通常在多UI切换时使用
        /// </summary>
        protected override void OnRefocus(object userData)
        {
            base.OnRefocus(userData);
        }

        /// <summary>
        /// 关闭界面时调用，执行清理工作
        /// </summary>
        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            // 释放事件容器
            if (EventContainer != null)
            {
                EventContainer.UnSubscribeAll();
                ReferencePool.Release(EventContainer);
                EventContainer = null;
            }
        }

        /// <summary>
        /// 深度变化时调用，调整UI的显示顺序
        /// </summary>
        protected override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            int oldDepth = Depth;
            base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            int deltaDepth = UGUIGroupHelper.DepthFactor * uiGroupDepth + DepthFactor * depthInUIGroup - oldDepth + OriginalDepth;

            // 更新所有子Canvas的深度
            GetComponentsInChildren(true, CachedCanvasContainer);
            foreach (var canvas in CachedCanvasContainer)
            {
                canvas.sortingOrder += deltaDepth;
            }

            CachedCanvasContainer.Clear();
        }
    }
}