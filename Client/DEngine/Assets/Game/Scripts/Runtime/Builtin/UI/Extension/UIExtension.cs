using DEngine.Runtime;
using UnityEngine;
using UnityEngine.UI;

public static partial class UIExtension
{
    /// <summary>
    /// 设置屏幕安全区域（异形屏支持）。
    /// </summary>
    /// <param name="self"></param>
    /// <param name="safeRect">安全区域</param>
    public static void ApplyScreenSafeRect(this UIComponent self, Rect safeRect)
    {
        RectTransform rectTransform = null;
        CanvasScaler scaler = self.GetComponentInChildren<CanvasScaler>();
        if (!scaler)
        {
            Log.Error($"Not found {nameof(CanvasScaler)} !");
            return;
        }

        rectTransform = scaler.GetComponent<RectTransform>();
        // Convert safe area rectangle from absolute pixels to UGUI coordinates
        float rateX = scaler.referenceResolution.x / Screen.width;
        float rateY = scaler.referenceResolution.y / Screen.height;
        float posX = (int)(safeRect.position.x * rateX);
        float posY = (int)(safeRect.position.y * rateY);
        float width = (int)(safeRect.size.x * rateX);
        float height = (int)(safeRect.size.y * rateY);

        float offsetMaxX = scaler.referenceResolution.x - width - posX;
        float offsetMaxY = scaler.referenceResolution.y - height - posY;

        // 注意：安全区坐标系的原点为左下角	
        if (rectTransform)
        {
            rectTransform.offsetMin = new Vector2(posX, posY); //锚框状态下的屏幕左下角偏移向量
            rectTransform.offsetMax = new Vector2(-offsetMaxX, -offsetMaxY); //锚框状态下的屏幕右上角偏移向量
        }
    }
}