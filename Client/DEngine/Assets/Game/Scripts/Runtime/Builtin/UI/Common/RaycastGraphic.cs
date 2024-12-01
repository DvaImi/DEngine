using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// 用于只响应点击等操作事件而不绘制的组件（替换透明图片以降低 DC）。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/RaycastGraphic")]
    public sealed class RaycastGraphic : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 是否在编辑模式下显示调试边框，仅供调试使用。
        /// </summary>
        [SerializeField] private bool showDebugOutline = true;

        /// <summary>
        /// 调试边框的颜色，仅供调试使用。
        /// </summary>
        [SerializeField] private Color debugOutlineColor = Color.green;

        private void OnDrawGizmos()
        {
            if (showDebugOutline)
            {
                var corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);

                Gizmos.color = debugOutlineColor;
                for (int i = 0; i < 4; i++)
                {
                    var start = corners[i];
                    var end = corners[(i + 1) % 4];
                    Gizmos.DrawLine(start, end);
                }
            }
        }
#endif
    }
}