using System.IO;
using DEngine.Editor;
using Game.Editor.Toolbar;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public static class GenerateLuban
    {
        [MenuItem("DataTable/Generate/Luban", priority = 100)]
        [EditorToolMenu("Generate Luban", 0, 4)]
        public static void Generate()
        {
#if UNITY_EDITOR_WIN
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Share/Luban/gen_bin_unitask.bat"));
#elif UNITY_EDITOR_OSX
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Share/Luban/gen_bin_unitask.sh"));
#endif
        }

        [MenuItem("Game/Luban/Editor", priority = 101)]
        public static void EditorLuban()
        {
            OpenFolder.Execute(Path.Combine(Application.dataPath, "../../../Share/Luban/Configs/Datas"));
        }
    }
}