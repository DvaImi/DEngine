// ========================================================
// 作者：Dvalmi 
// 创建时间：2024-04-13 14:47:57
// ========================================================

using DEngine.Runtime;
using FairyGUI;

namespace Game.FairyGUI.Runtime
{
    public class FairyGUIGroupHelper : UIGroupHelperBase
    {
        public const int DepthFactor = 10000;
        private int m_Depth = 0;

        /// <summary>
        /// 设置界面组深度。
        /// </summary>
        /// <param name="depth">界面组深度。</param>
        public override void SetDepth(int depth)
        {
            m_Depth = depth;
        }
    }
}