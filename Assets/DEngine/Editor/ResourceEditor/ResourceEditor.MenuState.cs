using UnityEditor;

namespace DEngine.Editor.ResourceTools
{
    internal sealed partial class ResourceEditor : EditorWindow
    {
        private enum MenuState : byte
        {
            Normal,
            Add,
            Rename,
            Remove,
        }
    }
}
