using System.IO;
using DEngine.Editor;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public static class GenerateLuban
    {
        [MenuItem("Game/Luban/Generate", priority = 100)]
        public static void Generate()
        {
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Share/Luban/gen_bin_unitask.sh"));
        }

        [MenuItem("Game/Luban/Editor", priority = 101)]
        public static void EditorLuban()
        {
            OpenFolder.Execute(Path.Combine(Application.dataPath, "../../../Share/Luban/Configs/Datas"));
        }
    }
}