// ========================================================
// 作者：Dvalmi 
// 创建时间：2024-04-13 14:48:20
// ========================================================

using FairyGUI;
using DEngine.Runtime;

namespace Game.FairyGUI.Runtime
{
    public abstract class FairyGUIFormBase : UIFormLogic
    {
        public const int DepthFactor = 100;

        public UIPanel CachedUIPanel { get; private set; }

        public GComponent UI { get; private set; }

        public string PackageName { get; private set; }


        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            PackageName = GetType().Name;
            CachedUIPanel = gameObject.GetOrAddComponent<UIPanel>();
            CachedUIPanel.packageName = PackageName;
            CachedUIPanel.componentName = GetType().Name;
        }


        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            gameObject.SetLayerRecursively(Constant.Layer.UILayerId);

            CachedUIPanel.fitScreen = FairyGuiContentScaleHelper.GetFairyUIScaleType() == FairyUIScaleType.Width ? FitScreen.FitWidthAndSetMiddle : FitScreen.FitHeightAndSetCenter;
            CachedUIPanel.ApplyModifiedProperties(false, true);
            CachedUIPanel.CreateUI();
            UI = CachedUIPanel.ui;
            UI.MakeFullScreen();
        }


        protected override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            CachedUIPanel.SetSortingOrder(DepthFactor * uiGroupDepth + DepthFactor * depthInUIGroup, true);
        }

        internal virtual void ReleaseUIForm()
        {
            UI.Dispose();
            UI = null;
        }
    }
}