using System.IO;
using Game.Editor.Toolbar;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.NetworkProtocol
{
    public static class GenerateNetworkProtocol
    {
        [MenuItem("Fantasy/Generate/Protocol", priority = 100)]
        public static void GenerateProtocol()
        {
#if UNITY_EDITOR_WIN
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Server/Tools/NetworkProtocol/Run.bat"));
#elif UNITY_EDITOR_OSX
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Server/Tools/NetworkProtocol/Run.sh"));
#endif
        }

        [MenuItem("Fantasy/Generate/DataTable", priority = 200)]
        public static void GenerateDataTable()
        {
#if UNITY_EDITOR_WIN
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Server/Tools/ConfigTable/Run.bat"));
#elif UNITY_EDITOR_OSX
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Server/Tools/ConfigTable/Run.sh"));
#endif
        }

        [MenuItem("Fantasy/Editor", priority = 101)]
        public static void EditorProtocol()
        {
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Server/DEngineServer/DEngineServer.sln"));
        }

        [MenuItem("Fantasy/Run", priority = 201)]
        public static void Run()
        {
            ShellHelper.Run("dotnet DEngine.dll --m Develop", Path.Combine(Application.dataPath, "../../../Server/Bin"));
        }
    }
}