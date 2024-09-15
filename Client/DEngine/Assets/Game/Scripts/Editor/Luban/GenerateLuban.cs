using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public static class GenerateLuban
    {
        [MenuItem("Game/Gene Luban", priority = 1)]
        public static void ExportConfigBin()
        {
#if UNITY_STANDALONE_WIN
            Application.OpenURL(Path.Combine(Application.dataPath, "../../../Share/Luban/gen_bin__unitask.bat"));
#elif UNITY_STANDALONE_OSX
            System.Diagnostics.Process.Start("/bin/bash", Path.Combine(Application.dataPath, "../../../Share/Luban/gen_bin__unitask.sh"));
#endif
        }
    }
}