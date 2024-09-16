using System;
using System.Diagnostics;
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
#if UNITY_EDITOR_WIN
            Application.OpenURL(Path.Combine(Application.dataPath, "../../../Share/Luban/gen_bin_unitask.bat"));
#elif UNITY_EDITOR_OSX
            System.Diagnostics.Process.Start("/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal", Path.Combine(Application.dataPath, "/../../../Share/Luban/./gen_bin_unitask.sh"));
#endif
        }
    }
}