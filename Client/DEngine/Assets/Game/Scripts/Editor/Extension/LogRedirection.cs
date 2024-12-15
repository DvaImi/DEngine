using System;
using System.Reflection;
using System.Text.RegularExpressions;
using DEngine.Runtime;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

namespace DEngine.Editor.Extension
{
    /// <summary>
    /// 日志重定向相关的实用函数。
    /// </summary>
    internal static class LogRedirection
    {
        private static readonly string LogScripts = $"{nameof(Log)}.cs";
        private static readonly string DEngineLogScripts = $"{nameof(DEngineLog)}.cs";
        private static readonly string DefaultLogHelperScripts = $"{nameof(DefaultLogHelper)}.cs";

        [OnOpenAsset(-1)]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            if (line <= 0)
            {
                return false;
            }

            string assetPath = AssetDatabase.GetAssetPath(instanceID);

            if (!assetPath.EndsWith(".cs"))
            {
                return false;
            }

            bool autoFirstMatch = assetPath.Contains(DefaultLogHelperScripts) || assetPath.Contains(DEngineLogScripts) || assetPath.Contains(LogScripts);
            var stackTrace = GetSelectedStackTrace();
            if (!string.IsNullOrEmpty(stackTrace))
            {
                if (!autoFirstMatch)
                {
                    var fullPath = Application.dataPath[..Application.dataPath.LastIndexOf("Assets", StringComparison.Ordinal)];
                    fullPath = $"{fullPath}{assetPath}";
                    InternalEditorUtility.OpenFileAtLineExternal(fullPath.Replace('/', '\\'), line);
                    return true;
                }

                var matches = Regex.Match(stackTrace, @"\(at (.+)\)", RegexOptions.IgnoreCase);
                while (matches.Success)
                {
                    var pathLine = matches.Groups[1].Value;
                    if (!pathLine.Contains(DefaultLogHelperScripts) && !pathLine.Contains(DEngineLogScripts) && !pathLine.Contains(LogScripts))
                    {
                        var splitIndex = pathLine.LastIndexOf(":", StringComparison.Ordinal);
                        var path = pathLine[..splitIndex];
                        line = Convert.ToInt32(pathLine[(splitIndex + 1)..]);
                        var fullPath = Application.dataPath[..Application.dataPath.LastIndexOf("Assets", StringComparison.Ordinal)];
                        fullPath = $"{fullPath}{path}";
                        InternalEditorUtility.OpenFileAtLineExternal(fullPath.Replace('/', '\\'), line);
                        break;
                    }

                    matches = matches.NextMatch();
                }

                return true;
            }

            return false;
        }

        private static string GetSelectedStackTrace()
        {
            var editorWindowAssembly = typeof(EditorWindow).Assembly;
            var consoleWindowType = editorWindowAssembly.GetType("UnityEditor.ConsoleWindow");
            if (consoleWindowType == null)
            {
                return null;
            }

            var consoleWindowFieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            if (consoleWindowFieldInfo == null)
            {
                return null;
            }

            var consoleWindow = consoleWindowFieldInfo.GetValue(null) as EditorWindow;
            if (consoleWindow == null)
            {
                return null;
            }

            if (consoleWindow != EditorWindow.focusedWindow)
            {
                return null;
            }

            var activeTextFieldInfo = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
            if (activeTextFieldInfo == null)
            {
                return null;
            }

            return (string)activeTextFieldInfo.GetValue(consoleWindow);
        }
    }
}