using Game.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Editor.UI
{
    public static class RaycastGraphicMenu
    {
        [MenuItem("GameObject/UI/RaycastGraphic", false, 2000)]
        public static void CreateRaycastGraphic(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("RaycastGraphic");
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                Canvas canvas = Object.FindFirstObjectByType<Canvas>();
                if (!canvas)
                {
                    canvas = CreateDefaultCanvas();
                }

                parent = canvas.gameObject;
            }

            GameObjectUtility.SetParentAndAlign(go, parent);

            go.AddComponent<RaycastGraphic>();

            Undo.RegisterCreatedObjectUndo(go, "Create RaycastGraphic");
            Selection.activeGameObject = go;
        }

        private static Canvas CreateDefaultCanvas()
        {
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
            return canvas;
        }
    }
}