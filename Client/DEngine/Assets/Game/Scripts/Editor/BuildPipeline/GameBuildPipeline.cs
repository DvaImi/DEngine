using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        public static string[] PackagesNames { get; private set; }
        public static string[] VariantNames { get; private set; }
        public static readonly GUIStyle ToolBarButtonGuiStyle;
        private const string ButtonStyleName = "Tab middle";

        static GameBuildPipeline()
        {
            RefreshPackages();

            ToolBarButtonGuiStyle = new GUIStyle(ButtonStyleName)
            {
                padding = new RectOffset(2, 8, 2, 2),
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }
    }
}