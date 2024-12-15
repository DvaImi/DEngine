using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// 编辑器内置图标辅助器
    /// </summary>
    public static class EditorBuiltinIconHelper
    {
        public static GUIContent GetPlatformIconContent(string text, string tooltip)
        {
            BuildTarget currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            Texture2D icon = null;

            // 根据平台名称获取对应的图标
            switch (currentBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    icon = EditorGUIUtility.IconContent("BuildSettings.Standalone").image as Texture2D;
                    break;
                case BuildTarget.Android:
                    icon = EditorGUIUtility.IconContent("BuildSettings.Android").image as Texture2D;
                    break;
                case BuildTarget.iOS:
                    icon = EditorGUIUtility.IconContent("BuildSettings.iPhone").image as Texture2D;
                    break;
                case BuildTarget.WebGL:
                    icon = EditorGUIUtility.IconContent("BuildSettings.WebGL").image as Texture2D;
                    break;
                default:
                    icon = EditorGUIUtility.IconContent("BuildSettings.Standalone").image as Texture2D;
                    break;
            }

            return EditorGUIUtility.TrTextContentWithIcon(text, tooltip, icon);
        }

        public static GUIContent GetSave(string text, string tooltip)
        {
            return EditorGUIUtility.TrTextContentWithIcon(text, tooltip, "SaveAs@2x");
        }

        public static GUIContent GetIcon(string iconName)
        {
            return EditorGUIUtility.IconContent(iconName);
        }
    }
}