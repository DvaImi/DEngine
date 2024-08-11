using UnityEditor;
using UnityEngine;
namespace Game.Editor.ProtoTool
{
    public class Proto2CSWindow : EditorWindow
    {
        [MenuItem("Game/Proto2CS", false, 1)]
        private static void Open()
        {
            Proto2CSWindow window = GetWindow<Proto2CSWindow>("Proto2CS", true);
            window.minSize = new Vector2(800f, 600f);
        }
    }
}