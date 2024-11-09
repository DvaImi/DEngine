﻿using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// 用来只想响应点击等操作事件而不绘制的组件（替换透明图片降低DC）
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RaycastGraphic : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}