using System;

namespace Game.Editor.Toolbar
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EditorToolMenuAttribute : Attribute
    {
        /// <summary>
        /// 按钮路径
        /// </summary>
        public string MenuName { get; private set; }

        /// <summary>
        /// 排列方位 0 left 1 right
        /// </summary>
        public int Align { get; private set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; private set; }

        public EditorToolMenuAttribute(string menu, int align, int order)
        {
            MenuName = menu;
            Align = align;
            Order = order;
        }
    }
}