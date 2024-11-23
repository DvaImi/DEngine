using System.IO;
using Game.Editor.Toolbar;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public static partial class GeneratorDataTableCommonLine
    {
#if UNITY_EDITOR_WIN
        private const string LUBANSHELL = "gen_bin_client_lazyload.bat";
#elif UNITY_EDITOR_OSX
        private const string LUBANSHELL = "gen_bin_client_lazyload.sh";
#endif

        [MenuItem("DataTable/Generate/Luban", priority = 3)]
        [EditorToolbarMenu("Generate Luban", ToolBarMenuAlign.Left, 4)]
        public static void GenerateLuban()
        {
            ShellHelper.RunV2(LUBANSHELL, Path.Combine(Application.dataPath, "../../../Share/Luban/"));
        }

        [MenuItem("DataTable/Editor/Luban", priority = 3)]
        public static void EditorLuban()
        {
            EditorUtility.RevealInFinder(Path.Combine(Application.dataPath, "../../../Share/Luban/Client/Datas"));
        }
    }
}