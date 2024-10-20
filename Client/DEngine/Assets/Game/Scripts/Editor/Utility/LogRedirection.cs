using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// 日志重定向相关的实用函数。
    /// </summary>
    internal static class LogRedirection
    {
        private static readonly Regex LogRegex = new Regex(@" \(at (.+)\:(\d+)\)\r?\n");

        [OnOpenAsset(0)]
        private static bool OnOpenAsset(int instanceId, int line)
        {
            if (line <= 0)
            {
                return false;
            }

            // 获取资源路径
            string assetPath = AssetDatabase.GetAssetPath(instanceId);
            
            // 判断资源类型
            if (!assetPath.EndsWith(".cs"))
            {
                return false;
            }
            
            string selectedStackTrace = GetSelectedStackTrace();
            if (string.IsNullOrEmpty(selectedStackTrace))
            {
                return false;
            }

            if (!selectedStackTrace.Contains("DefaultLogHelper.cs"))
            {
                return false;
            }

            Match match = LogRegex.Match(selectedStackTrace);
            if (!match.Success)
            {
                return false;
            }

            match = match.NextMatch();
            if (!match.Success)
            {
                return false;
            }

            if (match.Groups[1].Value.Contains("DefaultLogHelper.cs"))
            {
                match = match.NextMatch();
                if (!match.Success)
                {
                    return false;
                }
            }

            if (match.Groups[1].Value.Contains("DEngineLog.cs"))
            {
                match = match.NextMatch();
                if (!match.Success)
                {
                    return false;
                }
            }

            if (match.Groups[1].Value.Contains("Log.cs"))
            {
                match = match.NextMatch();
                if (!match.Success)
                {
                    return false;
                }
            }

            InternalEditorUtility.OpenFileAtLineExternal(Path.Combine(Application.dataPath, match.Groups[1].Value[7..]), int.Parse(match.Groups[2].Value));
            return true;
        }

        private static string GetSelectedStackTrace()
        {
            Assembly editorWindowAssembly = typeof(EditorWindow).Assembly;
            if (editorWindowAssembly == null)
            {
                return null;
            }

            System.Type consoleWindowType = editorWindowAssembly.GetType("UnityEditor.ConsoleWindow");
            if (consoleWindowType == null)
            {
                return null;
            }

            FieldInfo consoleWindowFieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            if (consoleWindowFieldInfo == null)
            {
                return null;
            }

            EditorWindow consoleWindow = consoleWindowFieldInfo.GetValue(null) as EditorWindow;
            if (consoleWindow == null)
            {
                return null;
            }

            if (consoleWindow != EditorWindow.focusedWindow)
            {
                return null;
            }

            FieldInfo activeTextFieldInfo = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
            if (activeTextFieldInfo == null)
            {
                return null;
            }

            return (string)activeTextFieldInfo.GetValue(consoleWindow);
        }
    }
}