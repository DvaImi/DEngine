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
        [EditorToolMenu("Generate Protocol", 0, 20)]
        public static void Generate()
        {
#if UNITY_EDITOR_WIN
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Server/DEngineServer/Tools/Exporter/Run.bat"));
#elif UNITY_EDITOR_OSX
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Server/DEngineServer/Tools/Exporter/Run.sh"));
#endif
        }

        [MenuItem("Fantasy/Editor", priority = 101)]
        public static void EditorLuban()
        {
            OpenFolder.Execute(Path.Combine(Application.dataPath, "../../../Server/DEngineServer/Config"));
        }
    }
}