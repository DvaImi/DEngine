using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public static class GameEditorUtility
    {
        public static T GetWindowDontShow<T>() where T : EditorWindow
        {
            Object[] array = Resources.FindObjectsOfTypeAll(typeof(T));
            return (array.Length != 0) ? ((T)array[0]) : ScriptableObject.CreateInstance<T>();
        }
    }
}
