using System.IO;
using DEngine.Editor;
using Game.Editor.Toolbar;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.NetworkProtocol
{
    public static class GenerateNetworkProtocol
    {
        [MenuItem("Fantasy/Generate", priority = 100)]
        [EditorToolbarMenu("Generate Protocol", 0, 20)]
        public static void GenerateAll()
        {
#if UNITY_EDITOR_WIN
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Server/Tools/NetworkProtocol/Run.bat"));
#elif UNITY_EDITOR_OSX
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Server/Tools/NetworkProtocol/Run.sh"));
#endif
        }

        [MenuItem("Fantasy/Editor", priority = 101)]
        public static void EditorProtocol()
        {
            OpenFolder.Execute(Path.Combine(Application.dataPath, "../../../Server/DEngineServer/Config"));
        }

        [MenuItem("Fantasy/Run", priority = 201)]
        public static void Run()
        {
            ShellHelper.Run("dotnet DEngine.dll --m Develop", Path.Combine(Application.dataPath, "../../../Server/Bin"));
        }
    }
}