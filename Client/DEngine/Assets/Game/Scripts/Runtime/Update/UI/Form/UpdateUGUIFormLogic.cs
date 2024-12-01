using System.Threading;
using Cysharp.Threading.Tasks;
using Game.UI;
using Game.Update.Sound;

namespace Game.Update.UI
{
    /// <summary>
    /// 可更新界面基类
    /// </summary>
    public abstract class UpdateUGUIFormLogic : GameUGUIFormLogic
    {
        private CancellationToken m_Token;
        private const float FadeTime = 0.3F;

        /// <summary>
        /// 初始化界面，设置组件和默认状态
        /// </summary>
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_Token = this.GetCancellationTokenOnDestroy();
            m_Token.Register(ResetCanvasGroup);
        }

        /// <summary>
        /// 打开界面时调用，执行界面显示及过渡效果
        /// </summary>
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            FadeOpen();
        }

        /// <summary>
        /// 恢复界面时调用，恢复原本的UI状态并执行显示过渡
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();
            FadeOpen();
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
            ResetCanvasGroup();
        }

        /// <summary>
        /// 回收界面时调用，释放资源
        /// </summary>
        protected override void OnRecycle()
        {
            base.OnRecycle();
        }

        /// <summary>
        /// 深度变化时调用，调整UI的显示顺序
        /// </summary>
        protected override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
        }

        /// <summary>
        /// 执行打开界面的渐变效果
        /// </summary>
        protected virtual void FadeOpen()
        {
            ResetCanvasGroup();
            CanvasGroup.FadeToAlpha(0f, FadeTime, m_Token).Forget();
        }

        /// <summary>
        /// 重置CanvasGroup，隐藏界面
        /// </summary>
        protected virtual void ResetCanvasGroup()
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
        }

        /// <summary>
        /// 关闭UI界面并执行渐变效果
        /// </summary>
        protected virtual async UniTask Close(float duration)
        {
            await CanvasGroup.FadeToAlpha(0f, duration, m_Token);
            GameEntry.UI.CloseUIForm(this);
        }

        /// <summary>
        /// 播放UI界面音效
        /// </summary>
        protected void PlayUISound(UISoundId uiSoundId)
        {
            GameEntry.Sound.PlayUISound(uiSoundId);
        }
    }
}