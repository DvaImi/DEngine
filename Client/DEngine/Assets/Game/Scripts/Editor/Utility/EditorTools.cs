using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Text;
using DEngine.Editor;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
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


        [MenuItem("Assets/Get Asset Path", priority = 3)]
        static void GetAssetPath()
        {
            UnityEngine.Object selObj = Selection.activeObject;

            if (selObj != null)
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
        public static List<Type> GetAssignableTypes(System.Type parentType)
        {
            TypeCache.TypeCollection collection = TypeCache.GetTypesDerivedFrom(parentType);
            return collection.ToList();
        }

        /// <summary>
        /// 获取带有指定属性的所有类的类型
        /// </summary>
        public static List<Type> GetTypesWithAttribute(System.Type attrType)
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
        public static object InvokeNonPublicStaticMethod(System.Type type, string method, params object[] parameters)
        {
            var methodInfo = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo == null)
            {
                UnityEngine.Debug.LogError($"{type.FullName} not found method : {method}");
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
        public static object InvokePublicStaticMethod(System.Type type, string method, params object[] parameters)
        {
            var methodInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                UnityEngine.Debug.LogError($"{type.FullName} not found method : {method}");
                return null;
            }

            return methodInfo.Invoke(null, parameters);
        }

        #endregion

        #region EditorUtility

        /// <summary>
        /// 搜集资源
        /// </summary>
        /// <param name="searchType">搜集的资源类型</param>
        /// <param name="searchInFolders">指定搜索的文件夹列表</param>
        /// <returns>返回搜集到的资源路径列表</returns>
        public static string[] FindAssets(EAssetSearchType searchType, string[] searchInFolders)
        {
            // 注意：AssetDatabase.FindAssets()不支持末尾带分隔符的文件夹路径
            for (int i = 0; i < searchInFolders.Length; i++)
            {
                string folderPath = searchInFolders[i];
                searchInFolders[i] = folderPath.TrimEnd('/');
            }

            // 注意：获取指定目录下的所有资源对象（包括子文件夹）
            string[] guids;
            if (searchType == EAssetSearchType.All)
                guids = AssetDatabase.FindAssets(string.Empty, searchInFolders);
            else
                guids = AssetDatabase.FindAssets($"t:{searchType}", searchInFolders);

            // 注意：AssetDatabase.FindAssets()可能会获取到重复的资源
            HashSet<string> result = new HashSet<string>();
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (result.Contains(assetPath) == false)
                {
                    result.Add(assetPath);
                }
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
            return FindAssets(searchType, new string[] { searchInFolder });
        }

        /// <summary>
        /// 打开搜索面板
        /// </summary>
        /// <param name="title">标题名称</param>
        /// <param name="defaultPath">默认搜索路径</param>
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

                    throw new System.Exception($"Found multiple {settingType.Name} files !");
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

                    throw new System.Exception($"Found multiple {settingType.Name} files !");
                }

                var setting = AssetDatabase.LoadAssetAtPath<TScriptableObject>(filePath);
                return setting;
            }
        }

        #endregion

        #region EditorWindow

        public static void FocusUnitySceneWindow()
        {
            EditorWindow.FocusWindowIfItsOpen<SceneView>();
        }

        public static void CloseUnityGameWindow()
        {
            System.Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(T, false, "GameView", true).Close();
        }

        public static void FocusUnityGameWindow()
        {
            System.Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(T, false, "GameView", true);
        }

        public static void FocueUnityProjectWindow()
        {
            System.Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.ProjectBrowser");
            EditorWindow.GetWindow(T, false, "Project", true);
        }

        public static void FocusUnityHierarchyWindow()
        {
            System.Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.SceneHierarchyWindow");
            EditorWindow.GetWindow(T, false, "Hierarchy", true);
        }

        public static void FocusUnityInspectorWindow()
        {
            System.Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.InspectorWindow");
            EditorWindow.GetWindow(T, false, "Inspector", true);
        }

        public static void FocusUnityConsoleWindow()
        {
            System.Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.ConsoleWindow");
            EditorWindow.GetWindow(T, false, "Console", true);
        }

        public static void EditorDisplay(string title, string message, string ok, string cancel, System.Action action)
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

        /// <summary>
        /// 绘制unity 工程内部
        /// </summary>
        /// <param name="header"></param>
        /// <param name="content"></param>
        /// <param name="isFolder"></param>
        public static void GUIAssetPath(string header, ref string content, bool isFolder = false)
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
                bool invalid = !AssetDatabase.LoadAssetAtPath<Object>(content);
                content = EditorGUILayout.TextField(header, content, invalid ? warningLableGUIStyle : EditorStyles.label);
                Rect rect = GUILayoutUtility.GetLastRect();
                if (DropPathUtility.DropPath(rect, out string path, isFolder))
                {
                    if (!string.Equals(path, content, StringComparison.Ordinal))
                    {
                        content = path;
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
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
                            path = directory;
                        }
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    OpenFolder.Execute(path);
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
                            path = file;
                        }
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
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

        private static MethodInfo _clearConsoleMethod;

        private static MethodInfo ClearConsoleMethod
        {
            get
            {
                if (_clearConsoleMethod == null)
                {
                    Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                    System.Type logEntries = assembly.GetType("UnityEditor.LogEntries");
                    _clearConsoleMethod = logEntries.GetMethod("Clear");
                }

                return _clearConsoleMethod;
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
            var sceneCount = EditorSceneManager.sceneCount;
            for (var i = 0; i < sceneCount; ++i)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (scene.isDirty)
                    return true;
            }

            return false;
        }

        #endregion

        #region StringUtility

        public static string RemoveFirstChar(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            return str.Substring(1);
        }

        public static string RemoveLastChar(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            return str.Substring(0, str.Length - 1);
        }

        public static List<string> StringToStringList(string str, char separator)
        {
            List<string> result = new List<string>();
            if (!String.IsNullOrEmpty(str))
            {
                string[] splits = str.Split(separator);
                foreach (string split in splits)
                {
                    string value = split.Trim(); //移除首尾空格
                    if (!String.IsNullOrEmpty(value))
                    {
                        result.Add(value);
                    }
                }
            }

            return result;
        }

        public static T NameToEnum<T>(string name)
        {
            if (Enum.IsDefined(typeof(T), name) == false)
            {
                throw new ArgumentException($"Enum {typeof(T)} is not defined name {name}");
            }

            return (T)Enum.Parse(typeof(T), name);
        }

        #endregion

        #region 文件

        /// <summary>
        /// 创建文件所在的目录
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void CreateFileDirectory(string filePath)
        {
            string destDirectory = Path.GetDirectoryName(filePath);
            CreateDirectory(destDirectory);
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        public static bool CreateDirectory(string directory)
        {
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 删除文件夹及子目录
        /// </summary>
        public static bool DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 文件重命名
        /// </summary>
        public static void FileRename(string filePath, string newName)
        {
            string dirPath = Path.GetDirectoryName(filePath);
            string destPath;
            if (Path.HasExtension(filePath))
            {
                string extentsion = Path.GetExtension(filePath);
                destPath = $"{dirPath}/{newName}{extentsion}";
            }
            else
            {
                destPath = $"{dirPath}/{newName}";
            }

            FileInfo fileInfo = new FileInfo(filePath);
            fileInfo.MoveTo(destPath);
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        public static void MoveFile(string filePath, string destPath)
        {
            if (File.Exists(destPath))
                File.Delete(destPath);

            FileInfo fileInfo = new FileInfo(filePath);
            fileInfo.MoveTo(destPath);
        }

        /// <summary>
        /// 拷贝文件夹
        /// 注意：包括所有子目录的文件
        /// </summary>
        public static void CopyDirectory(string sourcePath, string destPath)
        {
            sourcePath = EditorTools.GetRegularPath(sourcePath);

            // If the destination directory doesn't exist, create it.
            if (Directory.Exists(destPath) == false)
                Directory.CreateDirectory(destPath);

            string[] fileList = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
            foreach (string file in fileList)
            {
                string temp = EditorTools.GetRegularPath(file);
                string savePath = temp.Replace(sourcePath, destPath);
                CopyFile(file, savePath, true);
            }
        }

        /// <summary>
        /// 拷贝文件
        /// </summary>
        public static void CopyFile(string sourcePath, string destPath, bool overwrite)
        {
            if (File.Exists(sourcePath) == false)
                throw new FileNotFoundException(sourcePath);

            // 创建目录
            CreateFileDirectory(destPath);

            // 复制文件
            File.Copy(sourcePath, destPath, overwrite);
        }

        /// <summary>
        /// 清空文件夹
        /// </summary>
        /// <param name="folderPath">要清理的文件夹路径</param>
        public static void ClearFolder(string directoryPath)
        {
            if (Directory.Exists(directoryPath) == false)
                return;

            // 删除文件
            string[] allFiles = Directory.GetFiles(directoryPath);
            for (int i = 0; i < allFiles.Length; i++)
            {
                File.Delete(allFiles[i]);
            }

            // 删除文件夹
            string[] allFolders = Directory.GetDirectories(directoryPath);
            for (int i = 0; i < allFolders.Length; i++)
            {
                Directory.Delete(allFolders[i], true);
            }
        }

        /// <summary>
        /// 获取文件字节大小
        /// </summary>
        public static long GetFileSize(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }

        /// <summary>
        /// 读取文件的所有文本内容
        /// </summary>
        public static string ReadFileAllText(string filePath)
        {
            if (File.Exists(filePath) == false)
                return string.Empty;

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// 读取文本的所有文本内容
        /// </summary>
        public static string[] ReadFileAllLine(string filePath)
        {
            if (File.Exists(filePath) == false)
                return null;

            return File.ReadAllLines(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// 检测AssetBundle文件是否合法
        /// </summary>
        public static bool CheckBundleFileValid(byte[] fileData)
        {
            string signature = ReadStringToNull(fileData, 20);
            if (signature == "UnityFS" || signature == "UnityRaw" || signature == "UnityWeb" || signature == "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA")
                return true;
            else
                return false;
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
            return AssetDatabase.IsValidFolder(assetPath) ? CalculateFolderSize(assetPath) : CalculateFileSize(assetPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static long CalculateFolderSize(string folderPath)
        {
            DirectoryInfo directoryInfo = new(folderPath);
            FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

            long totalSize = 0;

            foreach (FileInfo fileInfo in fileInfos)
            {
                totalSize += fileInfo.Length;
            }

            return totalSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static long CalculateFileSize(string filePath)
        {
            FileInfo fileInfo = new(filePath);
            return fileInfo.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string ReadStringToNull(byte[] data, int maxLength)
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < data.Length; i++)
            {
                if (i >= maxLength)
                    break;

                byte bt = data[i];
                if (bt == 0)
                    break;

                bytes.Add(bt);
            }

            if (bytes.Count == 0)
                return string.Empty;
            else
                return Encoding.UTF8.GetString(bytes.ToArray());
        }

        #endregion

        #region 路径

        /// <summary>
        /// 获取规范的路径
        /// </summary>
        public static string GetRegularPath(string path)
        {
            return path.Replace('\\', '/').Replace("\\", "/"); //替换为Linux路径格式
        }

        /// <summary>
        /// 移除路径里的后缀名
        /// </summary>
        public static string RemoveExtension(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            int index = str.LastIndexOf('.');
            if (index == -1)
                return str;
            else
                return str.Remove(index); //"assets/config/test.unity3d" --> "assets/config/test"
        }

        /// <summary>
        /// 获取项目工程路径
        /// </summary>
        public static string GetProjectPath()
        {
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            return GetRegularPath(projectPath);
        }

        /// <summary>
        /// 转换文件的绝对路径为Unity资源路径
        /// 例如 D:\\YourPorject\\Assets\\Works\\file.txt 替换为 Assets/Works/file.txt
        /// </summary>
        public static string AbsolutePathToAssetPath(string absolutePath)
        {
            string content = GetRegularPath(absolutePath);
            return Substring(content, "Assets/", true);
        }

        /// <summary>
        /// 转换Unity资源路径为文件的绝对路径
        /// 例如：Assets/Works/file.txt 替换为 D:\\YourPorject/Assets/Works/file.txt
        /// </summary>
        public static string AssetPathToAbsolutePath(string assetPath)
        {
            string projectPath = GetProjectPath();
            return $"{projectPath}/{assetPath}";
        }

        /// <summary>
        /// 递归查找目标文件夹路径
        /// </summary>
        /// <param name="root">搜索的根目录</param>
        /// <param name="folderName">目标文件夹名称</param>
        /// <returns>返回找到的文件夹路径，如果没有找到返回空字符串</returns>
        public static string FindFolder(string root, string folderName)
        {
            DirectoryInfo rootInfo = new DirectoryInfo(root);
            DirectoryInfo[] infoList = rootInfo.GetDirectories();
            for (int i = 0; i < infoList.Length; i++)
            {
                string fullPath = infoList[i].FullName;
                if (infoList[i].Name == folderName)
                    return fullPath;

                string result = FindFolder(fullPath, folderName);
                if (string.IsNullOrEmpty(result) == false)
                    return result;
            }

            return string.Empty;
        }

        /// <summary>
        /// 截取字符串
        /// 获取匹配到的后面内容
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="key">关键字</param>
        /// <param name="includeKey">分割的结果里是否包含关键字</param>
        /// <param name="searchBegin">是否使用初始匹配的位置，否则使用末尾匹配的位置</param>
        public static string Substring(string content, string key, bool includeKey, bool firstMatch = true)
        {
            if (string.IsNullOrEmpty(key))
                return content;

            int startIndex = -1;
            if (firstMatch)
                startIndex = content.IndexOf(key); //返回子字符串第一次出现位置		
            else
                startIndex = content.LastIndexOf(key); //返回子字符串最后出现的位置

            // 如果没有找到匹配的关键字
            if (startIndex == -1)
                return content;

            if (includeKey)
                return content.Substring(startIndex);
            else
                return content.Substring(startIndex + key.Length);
        }

        #endregion
    }
}