using System;

namespace Game.Editor.Toolbar
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EditorToolbarMenuAttribute : Attribute
    {
        /// <summary>
        /// 按钮路径
        /// </summary>
        public string MenuName { get; private set; }

        /// <summary>
        /// 排列方位
        /// </summary>
        public ToolBarMenuAlign Align { get; private set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// 使用自定义绘制
        /// </summary>
        public bool UseCustomGUI { get; private set; }

        public EditorToolbarMenuAttribute(string menu, ToolBarMenuAlign align, int order)
        {
            MenuName = menu;
            Align = align;
            Order = order;
        }

        public EditorToolbarMenuAttribute(string menu, ToolBarMenuAlign align, int order, bool useCustomGUI)
        {
            MenuName = menu;
            Align = align;
            Order = order;
            UseCustomGUI = useCustomGUI;
        }
    }
}