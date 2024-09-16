using System;
using System.Diagnostics;
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
// #if UNITY_EDITOR_WIN
//             Application.OpenURL(Path.Combine(Application.dataPath, "../../../Share/Luban/gen_bin_unitask.bat"));
// #elif UNITY_EDITOR_OSX
//             System.Diagnostics.Process.Start("/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal", Path.Combine(Application.dataPath, "/../../../Share/Luban/./gen_bin_unitask.sh"));
// #endif
            Process.Start(new ProcessStartInfo("gen_bin_unitask.bat")
            {
                Arguments = "",
                CreateNoWindow = false,
                UseShellExecute = true,
                RedirectStandardError = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                WorkingDirectory = Path.Combine(Application.dataPath, "../../../Share/Luban/")
            });
        }
        
        [MenuItem("Game/Luban/Editor", priority = 101)]
        public static void EditorLuban()
        {
            OpenFolder.Execute(Path.Combine(Application.dataPath, "../../../Share/Luban/Configs/Datas"));
        }
    }
}