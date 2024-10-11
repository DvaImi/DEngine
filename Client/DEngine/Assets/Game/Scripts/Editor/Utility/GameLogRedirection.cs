// using System;
// using System.Reflection;
// using System.Text.RegularExpressions;
// using UnityEditor;
// using UnityEditor.Callbacks;
// using UnityEditorInternal;
// using UnityEngine;
//
// namespace Game.Editor
// {
//     /// <summary>
//     /// 日志重定向相关的实用函数。
//     /// </summary>
//     internal static class GameLogRedirection
//     {
//         /// <summary>
//         /// 最大匹配检索深度
//         /// </summary>
//         private const int MaxRegexMatch = 20;
//
//         [OnOpenAsset(0)]
//         private static bool OnOpenAsset(int instance, int line)
//         {
//             // 自定义函数，用来获取stacktrace
//             string stackTrace = GetStackTrace();
//
//             // 通过stacktrace来判断是否是自定义Log
//             if (!string.IsNullOrEmpty(stackTrace))
//             {
//                 if (stackTrace.StartsWith("* ")) //这里的“* ”是从堆栈中筛选自定义的Log
//                 {
//                     //匹配所有Log行
//                     Match matches = Regex.Match(stackTrace, @"\(at(.+)\)", RegexOptions.IgnoreCase);
//                     if (matches.Success)
//                     {
//                         /* 找到跳转目标层：
//                          * 需要分别判断点击为首层还是其它层。
//                          * 首层时:跳过自定义Log层，向下一层跳转。
//                          * 其它层：直接跳转。
//                          */
//                         if (matches.Groups[1].Value.EndsWith(line.ToString())) //首层
//                         {
//                             matches = matches.NextMatch();
//                         }
//                         else
//                         {
//                             for (int i = 0; i < MaxRegexMatch; i++) //其他层
//                             {
//                                 if (matches.Groups[1].Value.EndsWith(line.ToString()))
//                                 {
//                                     break;
//                                 }
//
//                                 matches = matches.NextMatch();
//                             }
//                         }
//
//                         //跳转逻辑
//                         if (matches.Success)
//                         {
//                             var pathName = matches.Groups[1].Value;
//                             pathName = pathName.Replace(" ", "");
//
//                             //找到代码及行数
//                             int splitIndex = pathName.LastIndexOf(":", StringComparison.Ordinal);
//                             string path = pathName[..splitIndex];
//                             line = Convert.ToInt32(pathName[(splitIndex + 1)..]);
//                             string fullpaths = Application.dataPath[..Application.dataPath.LastIndexOf("Assets", StringComparison.Ordinal)];
//                             fullpaths += path;
//                             string strPath = fullpaths.Replace('/', '\\');
//                             InternalEditorUtility.OpenFileAtLineExternal(strPath, line);
//                         }
//                         else
//                         {
//                             Debug.LogError("DebugCodeLocation OnOpenAsset, Error StackTrace");
//                         }
//
//                         matches.NextMatch();
//                     }
//
//                     return true;
//                 }
//             }
//
//             return false;
//         }
//
//         private static string GetStackTrace()
//         {
//             var assemblyUnityEditor = Assembly.GetAssembly(typeof(EditorWindow));
//             if (assemblyUnityEditor == null)
//             {
//                 return null;
//             }
//
//             var typeConsoleWindow = assemblyUnityEditor.GetType("UnityEditor.ConsoleWindow");
//             if (typeConsoleWindow == null)
//             {
//                 return null;
//             }
//
//             var fieldConsoleWindow = typeConsoleWindow.GetField("ms_ConsoleWindow",
//                 BindingFlags.Static | BindingFlags.NonPublic);
//             if (fieldConsoleWindow == null)
//             {
//                 return null;
//             }
//
//             var instanceConsoleWindow = fieldConsoleWindow.GetValue(null);
//             if (instanceConsoleWindow == null)
//             {
//                 return null;
//             }
//
//             if (EditorWindow.focusedWindow == (EditorWindow)instanceConsoleWindow)
//             {
//                 var typeListViewState = assemblyUnityEditor.GetType("UnityEditor.ListViewState");
//                 if (typeListViewState == null)
//                 {
//                     return null;
//                 }
//
//                 var fieldListView = typeConsoleWindow.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
//                 if (fieldListView == null)
//                 {
//                     return null;
//                 }
//
//                 var valueListView = fieldListView.GetValue(instanceConsoleWindow);
//                 if (valueListView == null)
//                 {
//                     return null;
//                 }
//
//                 var fieldActiveText = typeConsoleWindow.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
//                 if (fieldActiveText == null)
//                 {
//                     return null;
//                 }
//
//                 var valueActiveText = fieldActiveText.GetValue(instanceConsoleWindow).ToString();
//                 return valueActiveText;
//             }
//
//             return null;
//         }
//     }
// }