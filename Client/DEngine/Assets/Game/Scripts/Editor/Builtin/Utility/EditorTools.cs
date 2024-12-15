using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Game.Editor
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public static class EditorTools
    {
        static EditorTools()
        {
            InitAssembly();
        }


        [MenuItem("Assets/Copy Asset Path", priority = 3)]
        private static void CopyAssetPath()
        {
            var selObj = Selection.activeObject;
            if (selObj)
            {
                string assetPath = AssetDatabase.GetAssetPath(selObj);
                EditorGUIUtility.systemCopyBuffer = assetPath;
                Debug.Log(assetPath);
            }
        }

        #region Assembly

#if UNITY_2019_4_OR_NEWER
        [DidReloadScripts]
        private static void InitAssembly()
        {
        }

        /// <summary>
        /// 获取带继承关系的所有类的类型
        /// </summary>
        public static List<Type> GetAssignableTypes(Type parentType)
        {
            TypeCache.TypeCollection collection = TypeCache.GetTypesDerivedFrom(parentType);
            return collection.ToList();
        }

        /// <summary>
        /// 获取带有指定属性的所有类的类型
        /// </summary>
        public static List<Type> GetTypesWithAttribute(Type attrType)
        {
            TypeCache.TypeCollection collection = TypeCache.GetTypesWithAttribute(attrType);
            return collection.ToList();
        }
#else
        private static readonly List<Type> _cacheTypes = new List<Type>(10000);
        private static void InitAssembly()
        {
            _cacheTypes.Clear();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                List<Type> types = assembly.GetTypes().ToList();
                _cacheTypes.AddRange(types);
            }
        }

        /// <summary>
        /// 获取带继承关系的所有类的类型
        /// </summary>
        public static List<Type> GetAssignableTypes(System.Type parentType)
        {
            List<Type> result = new List<Type>();
            for (int i = 0; i < _cacheTypes.Count; i++)
            {
                Type type = _cacheTypes[i];
                if (parentType.IsAssignableFrom(type))
                {
                    if (type.Name == parentType.Name)
                        continue;
                    result.Add(type);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取带有指定属性的所有类的类型
        /// </summary>
        public static List<Type> GetTypesWithAttribute(System.Type attrType)
        {
            List<Type> result = new List<Type>();
            for (int i = 0; i < _cacheTypes.Count; i++)
            {
                Type type = _cacheTypes[i];
                if (type.GetCustomAttribute(attrType) != null)
                {
                    result.Add(type);
                }
            }
            return result;
        }
#endif

        /// <summary>
        /// 调用私有的静态方法
        /// </summary>
        /// <param name="type">类的类型</param>
        /// <param name="method">类里要调用的方法名</param>
        /// <param name="parameters">调用方法传入的参数</param>
        public static object InvokeNonPublicStaticMethod(Type type, string method, params object[] parameters)
        {
            var methodInfo = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo == null)
            {
                Debug.LogError($"{type.FullName} not found method : {method}");
                return null;
            }

            return methodInfo.Invoke(null, parameters);
        }

        /// <summary>
        /// 调用公开的静态方法
        /// </summary>
        /// <param name="type">类的类型</param>
        /// <param name="method">类里要调用的方法名</param>
        /// <param name="parameters">调用方法传入的参数</param>
        public static object InvokePublicStaticMethod(Type type, string method, params object[] parameters)
        {
            var methodInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                Debug.LogError($"{type.FullName} not found method : {method}");
                return null;
            }

            return methodInfo.Invoke(null, parameters);
        }

        #endregion

        #region EditorUtility

        /// <summary>
        /// 保存资源
        /// </summary>
        /// <param name="asset"></param>
        public static void SaveAsset(Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 搜集资源
        /// </summary>
        /// <param name="searchType">搜集的资源类型</param>
        /// <param name="searchInFolders">指定搜索的文件夹列表</param>
        /// <returns>返回搜集到的资源路径列表</returns>
        internal static string[] FindAssets(EAssetSearchType searchType, string[] searchInFolders)
        {
            // 注意：AssetDatabase.FindAssets()不支持末尾带分隔符的文件夹路径
            for (int i = 0; i < searchInFolders.Length; i++)
            {
                string folderPath = searchInFolders[i];
                searchInFolders[i] = folderPath.TrimEnd('/');
            }

            // 注意：获取指定目录下的所有资源对象（包括子文件夹）
            var guids = AssetDatabase.FindAssets(searchType == EAssetSearchType.All ? string.Empty : $"t:{searchType}", searchInFolders);
            // 注意：AssetDatabase.FindAssets()可能会获取到重复的资源
            HashSet<string> result = new HashSet<string>();
            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                result.Add(assetPath);
            }

            // 返回结果
            return result.ToArray();
        }

        /// <summary>
        /// 搜集资源
        /// </summary>
        /// <param name="searchType">搜集的资源类型</param>
        /// <param name="searchInFolder">指定搜索的文件夹</param>
        /// <returns>返回搜集到的资源路径列表</returns>
        public static string[] FindAssets(EAssetSearchType searchType, string searchInFolder)
        {
            return FindAssets(searchType, new[] { searchInFolder });
        }

        /// <summary>
        /// 打开搜索面板
        /// </summary>
        /// <param name="title">标题名称</param>
        /// <param name="defaultPath">默认搜索路径</param>
        /// <param name="defaultName"></param>
        /// <returns>返回选择的文件夹绝对路径，如果无效返回NULL</returns>
        public static string OpenFolderPanel(string title, string defaultPath, string defaultName = "")
        {
            string openPath = EditorUtility.OpenFolderPanel(title, defaultPath, defaultName);
            if (string.IsNullOrEmpty(openPath))
                return null;

            if (openPath.Contains("/Assets") == false)
            {
                Debug.LogWarning("Please select unity assets folder.");
                return null;
            }

            return openPath;
        }

        /// <summary>
        /// 打开搜索面板
        /// </summary>
        /// <param name="title">标题名称</param>
        /// <param name="defaultPath">默认搜索路径</param>
        /// <param name="extension"></param>
        /// <returns>返回选择的文件绝对路径，如果无效返回NULL</returns>
        public static string OpenFilePath(string title, string defaultPath, string extension = "")
        {
            string openPath = EditorUtility.OpenFilePanel(title, defaultPath, extension);
            if (string.IsNullOrEmpty(openPath))
                return null;

            if (openPath.Contains("/Assets") == false)
            {
                Debug.LogWarning("Please select unity assets file.");
                return null;
            }

            return openPath;
        }

        /// <summary>
        /// 显示进度框
        /// </summary>
        public static void DisplayProgressBar(string tips, int progressValue, int totalValue)
        {
            EditorUtility.DisplayProgressBar("进度", $"{tips} : {progressValue}/{totalValue}", (float)progressValue / totalValue);
        }

        /// <summary>
        /// 隐藏进度框
        /// </summary>
        public static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 加载ScriptableObject
        /// </summary>
        /// <typeparam name="TScriptableObject"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static TScriptableObject LoadScriptableObject<TScriptableObject>() where TScriptableObject : ScriptableObject
        {
            var settingType = typeof(TScriptableObject);
            var guids = AssetDatabase.FindAssets($"t:{settingType.Name}");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"Create new {settingType.Name}.asset");
                var setting = ScriptableObject.CreateInstance<TScriptableObject>();
                string filePath = $"Assets/{settingType.Name}.asset";
                AssetDatabase.CreateAsset(setting, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return setting;
            }
            else
            {
                if (guids.Length != 1)
                {
                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Debug.LogWarning($"Found multiple file : {path}");
                    }

                    throw new Exception($"Found multiple {settingType.Name} files !");
                }

                string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var setting = AssetDatabase.LoadAssetAtPath<TScriptableObject>(filePath);
                return setting;
            }
        }

        /// <summary>
        /// 从指定路径加载ScriptableObject
        /// </summary>
        /// <param name="filePath"></param>
        /// <typeparam name="TScriptableObject"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static TScriptableObject LoadScriptableObject<TScriptableObject>(string filePath) where TScriptableObject : ScriptableObject
        {
            var settingType = typeof(TScriptableObject);
            var guids = AssetDatabase.FindAssets($"t:{settingType.Name}");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"Create new {settingType.Name}.asset");
                var setting = ScriptableObject.CreateInstance<TScriptableObject>();
                AssetDatabase.CreateAsset(setting, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return setting;
            }
            else
            {
                if (guids.Length != 1)
                {
                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Debug.LogWarning($"Found multiple file : {path}");
                    }

                    throw new Exception($"Found multiple {settingType.Name} files !");
                }

                var setting = AssetDatabase.LoadAssetAtPath<TScriptableObject>(filePath);
                return setting;
            }
        }

        /// <summary>
        /// 计算资源数组大小
        /// </summary>
        /// <param name="assetObjects"></param>
        /// <returns></returns>
        public static long CalculateAssetsSize(Object[] assetObjects)
        {
            long size = 0;
            if (assetObjects == null)
            {
                return size;
            }

            foreach (var asset in assetObjects)
            {
                size += CalculateAssetSize(asset);
            }

            return size;
        }

        /// <summary>
        /// 计算资源大小
        /// </summary>
        /// <param name="assetObject"></param>
        /// <returns></returns>
        public static long CalculateAssetSize(Object assetObject)
        {
            string assetPath = AssetDatabase.GetAssetPath(assetObject);
            return AssetDatabase.IsValidFolder(assetPath) ? GameUtility.IO.CalculateFolderSize(assetPath) : GameUtility.IO.CalculateFileSize(assetPath);
        }

        #endregion

        #region EditorWindow

        public static void FocusUnitySceneWindow()
        {
            EditorWindow.FocusWindowIfItsOpen<SceneView>();
        }

        public static void CloseUnityGameWindow()
        {
            Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(T, false, "GameView", true).Close();
        }

        public static void FocusUnityGameWindow()
        {
            Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(T, false, "GameView", true);
        }

        public static void FocusUnityProjectWindow()
        {
            var T = Assembly.Load("UnityEditor").GetType("UnityEditor.ProjectBrowser");
            EditorWindow.GetWindow(T, false, "Project", true);
        }

        public static void FocusUnityHierarchyWindow()
        {
            Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.SceneHierarchyWindow");
            EditorWindow.GetWindow(T, false, "Hierarchy", true);
        }

        public static void FocusUnityInspectorWindow()
        {
            Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.InspectorWindow");
            EditorWindow.GetWindow(T, false, "Inspector", true);
        }

        public static void FocusUnityConsoleWindow()
        {
            Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.ConsoleWindow");
            EditorWindow.GetWindow(T, false, "Console", true);
        }

        public static void EditorDisplay(string title, string message, string ok, string cancel, Action action)
        {
            if (EditorUtility.DisplayDialog(title, message, ok, cancel))
            {
                action?.Invoke();
            }
        }

        public static void CloseAllCustomEditorWindows()
        {
            EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();

            foreach (EditorWindow window in allWindows)
            {
                var ns = window.GetType().Namespace;
                if (ns is not null && !ns.StartsWith("UnityEditor"))
                {
                    window.Close();
                }
            }
        }

        public static Texture GetIcon(string name)
        {
            return EditorGUIUtility.IconContent(name).image as Texture2D;
        }

        public static void GUIAssetPath(ref string content, out bool isChanged)
        {
            isChanged = false;
            EditorGUILayout.BeginHorizontal("box");
            {
                GUIStyle warningLableGUIStyle = new(EditorStyles.label)
                {
                    normal = new GUIStyleState
                    {
                        textColor = Color.yellow
                    },
                };
                if (string.IsNullOrWhiteSpace(content))
                {
                    EditorGUILayout.LabelField("AssetPath is invalid", warningLableGUIStyle);
                }
                else
                {
                    bool invalid = !AssetDatabase.LoadAssetAtPath<Object>(content);
                    content = EditorGUILayout.TextField(content, invalid ? warningLableGUIStyle : EditorStyles.label);
                }

                Rect rect = GUILayoutUtility.GetLastRect();
                if (DropPathUtility.DropPathOutType(rect, out string path, out _))
                {
                    if (!string.Equals(path, content, StringComparison.Ordinal))
                    {
                        content = path;
                        isChanged = true;
                    }
                }

                if (GUILayout.Button("Reveal", GUILayout.Width(80)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(content));
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        public static void GUIAssetPath(string header, ref string content, out bool isFile)
        {
            EditorGUILayout.BeginHorizontal("box");
            {
                GUIStyle warningLableGUIStyle = new(EditorStyles.label)
                {
                    normal = new GUIStyleState
                    {
                        textColor = Color.yellow
                    },
                };
                if (string.IsNullOrWhiteSpace(content))
                {
                    EditorGUILayout.LabelField(header, "AssetPath is invalid", warningLableGUIStyle);
                }
                else
                {
                    bool invalid = !AssetDatabase.LoadAssetAtPath<Object>(content);
                    content = EditorGUILayout.TextField(header, content, invalid ? warningLableGUIStyle : EditorStyles.label);
                }

                Rect rect = GUILayoutUtility.GetLastRect();
                if (DropPathUtility.DropPathOutType(rect, out string path, out isFile))
                {
                    if (!string.Equals(path, content, StringComparison.Ordinal))
                    {
                        content = path;
                    }
                }

                if (GUILayout.Button("Reveal", GUILayout.Width(80)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(content));
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制unity 工程内部
        /// </summary>
        /// <param name="header"></param>
        /// <param name="content"></param>
        /// <param name="isFolder"></param>
        public static void GUIAssetPath(string header, ref string content, bool isFolder = false)
        {
            EditorGUILayout.BeginHorizontal("box");
            {
                GUIStyle warningLableGUIStyle = new(EditorStyles.label)
                {
                    normal = new GUIStyleState
                    {
                        textColor = Color.yellow
                    },
                };
                if (string.IsNullOrWhiteSpace(content))
                {
                    EditorGUILayout.LabelField(header, "AssetPath is invalid", warningLableGUIStyle);
                }
                else
                {
                    bool invalid = !AssetDatabase.LoadAssetAtPath<Object>(content);
                    content = EditorGUILayout.TextField(header, content, invalid ? warningLableGUIStyle : EditorStyles.label);
                }

                Rect rect = GUILayoutUtility.GetLastRect();
                if (DropPathUtility.DropPath(rect, out string path, isFolder))
                {
                    if (!string.Equals(path, content, StringComparison.Ordinal))
                    {
                        content = path;
                    }
                }

                if (GUILayout.Button("Reveal", GUILayout.Width(80), GUILayout.Height(20)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(content));
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制外部文件夹路径
        /// </summary>
        /// <param name="header"></param>
        /// <param name="path"></param>
        public static void GUIOutFolderPath(string header, ref string path)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUIStyle warningLableGUIStyle = new(EditorStyles.label)
                {
                    normal = new GUIStyleState
                    {
                        textColor = Color.yellow
                    }
                };

                bool invalid = !Directory.Exists(path);
                EditorGUILayout.LabelField(header, path, invalid ? warningLableGUIStyle : EditorStyles.label);

                if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
                {
                    string directory = EditorUtility.OpenFolderPanel("Select Output Directory", path, string.Empty);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        if (Directory.Exists(directory) && directory != path)
                        {
                            path = GameUtility.IO.AbsolutePathToProject(directory);
                        }
                    }
                }

                if (GUILayout.Button("Reveal", GUILayout.Width(80), GUILayout.Height(20)))
                {
                    EditorUtility.RevealInFinder(path);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制外部文件路径
        /// </summary>
        /// <param name="header"></param>
        /// <param name="path"></param>
        /// <param name="extension"></param>
        public static void GUIOutFilePath(string header, ref string path, string extension)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUIStyle warningLableGUIStyle = new(EditorStyles.label)
                {
                    normal = new GUIStyleState
                    {
                        textColor = Color.yellow
                    }
                };
                bool invalid = !File.Exists(path);
                EditorGUILayout.LabelField(header, path, invalid ? warningLableGUIStyle : EditorStyles.label);

                if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
                {
                    string file = EditorUtility.OpenFilePanel("Select Output File", path, extension);
                    if (!string.IsNullOrEmpty(file))
                    {
                        if (File.Exists(file) && file != path)
                        {
                            path = GameUtility.IO.AbsolutePathToProject(file);
                        }
                    }
                }

                if (GUILayout.Button("Reveal", GUILayout.Width(80), GUILayout.Height(20)))
                {
                    EditorUtility.OpenWithDefaultApp(path);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制 Toggle
        /// </summary>
        /// <param name="header"></param>
        /// <param name="isOn"></param>
        public static void GUIToggle(string header, ref bool isOn)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(header);
                isOn = EditorGUILayout.Toggle(isOn);
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region EditorConsole

        private static MethodInfo s_ClearConsoleMethod;

        private static MethodInfo ClearConsoleMethod
        {
            get
            {
                if (s_ClearConsoleMethod == null)
                {
                    Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                    Type logEntries = assembly.GetType("UnityEditor.LogEntries");
                    s_ClearConsoleMethod = logEntries.GetMethod("Clear");
                }

                return s_ClearConsoleMethod;
            }
        }

        /// <summary>
        /// 清空控制台
        /// </summary>
        public static void ClearUnityConsole()
        {
            ClearConsoleMethod.Invoke(new object(), null);
        }

        #endregion

        #region SceneUtility

        public static bool HasDirtyScenes()
        {
            var sceneCount = SceneManager.sceneCount;
            for (var i = 0; i < sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}