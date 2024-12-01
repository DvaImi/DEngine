using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DEngine;
using FairyGUI;
using Game.FairyGUI.Runtime;
using Game.Update;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Editor.FairyGUI
{
    public class GameFairyGUISettingWindow : EditorWindow
    {
        private static string[] m_GroupNames;
        private FairyGUIFormRuntimeData m_FairyGUIFormRuntimeData;
        private ReorderableList m_GroupReorderableList;
        private int[] m_SelectGroupIndex;
        private ReorderableList m_UIFormReorderableList;

        [MenuItem("FairyGUI/Setting", false, 200)]
        private static void Open()
        {
            GameFairyGUISettingWindow window = GetWindow<GameFairyGUISettingWindow>("Game FairyGUI Setting Window", true);
            window.minSize = new Vector2(1600f, 400f);
        }

        [MenuItem("FairyGUI/Generate", false, 300)]
        private static void RefreshFairy()
        {
            var runtimeData = GetFairyGUIData();
            LoadFairyGUIFormRuntimeData(runtimeData);
            GeneraPrefab(runtimeData);
            GenerateUIFormScript(runtimeData);
            GeneraConstCode(runtimeData);
            AssetDatabase.Refresh();
        }

        [MenuItem("FairyGUI/Import Builtin Shader", false, 400)]
        public static void ImportBuiltinShaderMenu()
        {
            ImportBuiltinShader();
        }

        private void OnEnable()
        {
            FairyGUIEditorSetting.LoadOrCreate();
            m_FairyGUIFormRuntimeData = GetFairyGUIData();

            LoadFairyGUIFormRuntimeData(m_FairyGUIFormRuntimeData);

            m_GroupReorderableList = new ReorderableList(m_FairyGUIFormRuntimeData.FairyGroups, typeof(FairyGroup), true, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Fairy Groups"); },
                drawElementCallback = DrawFairyGroupElement,
                onAddCallback = _ => { m_FairyGUIFormRuntimeData.FairyGroups.Add(new FairyGroup("New Group", 0)); },
                onRemoveCallback = list => { m_FairyGUIFormRuntimeData.FairyGroups.RemoveAt(list.index); },
                elementHeightCallback = (index) =>
                {
                    if (string.IsNullOrEmpty(m_FairyGUIFormRuntimeData.FairyGroups[index].groupName))
                    {
                        return EditorGUIUtility.singleLineHeight * 3F + 2;
                    }

                    return EditorGUIUtility.singleLineHeight + 6F;
                }
            };

            m_UIFormReorderableList = new ReorderableList(m_FairyGUIFormRuntimeData.FairyForms, typeof(FairyForm), true, false, false, false)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Fairy Forms"); },
                drawElementCallback = DrawFairyFormElement,
                onSelectCallback = list =>
                {
                    var selectedAsset = AssetDatabase.LoadAssetAtPath<Object>(m_FairyGUIFormRuntimeData.FairyForms[list.index].assetName);
                    Selection.activeObject = selectedAsset;
                }
            };
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal("toolbar");
            {
                GUILayout.FlexibleSpace();

                GUI.backgroundColor = Color.green;

                if (GUILayout.Button("Reload", GUILayout.Width(100)))
                {
                    LoadFairyGUIFormRuntimeData(m_FairyGUIFormRuntimeData);
                }

                if (GUILayout.Button("Save", GUILayout.Width(100)))
                {
                    EditorUtility.SetDirty(m_FairyGUIFormRuntimeData);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    FairyGUIEditorSetting.Save();
                }

                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Fairy Project", GUILayout.Width(150f));
                FairyGUIEditorSetting.Instance.FairyGUIProject = EditorGUILayout.TextField(FairyGUIEditorSetting.Instance.FairyGUIProject);
                if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
                {
                    BrowseFairyProjectDirectory();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            {
                if (!AssetDatabase.IsValidFolder(FairyGUIEditorSetting.Instance.FairyGUIDataPath))
                {
                    EditorGUILayout.HelpBox("The fairygui data path cannot be empty.", MessageType.Warning);
                }

                DropPathUtility.DropAndPingAssetPath("FairyGUI Data Path", ref FairyGUIEditorSetting.Instance.FairyGUIDataPath, true);

                if (!AssetDatabase.IsValidFolder(FairyGUIEditorSetting.Instance.GeneralCodePath))
                {
                    EditorGUILayout.HelpBox("The general code path cannot be empty.", MessageType.Warning);
                }

                DropPathUtility.DropAndPingAssetPath("General Code Path", ref FairyGUIEditorSetting.Instance.GeneralCodePath, true);

                if (!AssetDatabase.IsValidFolder(FairyGUIEditorSetting.Instance.GeneralObjectAssetName))
                {
                    EditorGUILayout.HelpBox("The general object asset path cannot be empty.", MessageType.Warning);
                }

                DropPathUtility.DropAndPingAssetPath("General Object Asset Path", ref FairyGUIEditorSetting.Instance.GeneralObjectAssetName, true);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                GUILayout.Space(5f);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("UIGroupName", EditorStyles.boldLabel, GUILayout.Width(250));
                    EditorGUILayout.LabelField("Depth", EditorStyles.boldLabel, GUILayout.Width(100));
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5f);
                m_GroupReorderableList.DoLayoutList();
                EditorGUILayout.BeginHorizontal("box");
                {
                    GUILayout.Space(15f);
                    EditorGUILayout.LabelField("Id", EditorStyles.boldLabel, GUILayout.Width(50));
                    EditorGUILayout.LabelField("PackageName", EditorStyles.boldLabel, GUILayout.Width(120));
                    EditorGUILayout.LabelField("UIGroupName", EditorStyles.boldLabel, GUILayout.Width(100));
                    EditorGUILayout.LabelField("AllowMultiInstance", EditorStyles.boldLabel, GUILayout.Width(150));
                    EditorGUILayout.LabelField("PauseCoveredUIForm", EditorStyles.boldLabel, GUILayout.Width(150));
                    EditorGUILayout.LabelField("AssetName", EditorStyles.boldLabel, GUILayout.Width(450));
                    EditorGUILayout.LabelField("ObjectAsset", EditorStyles.boldLabel);
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5f);
                m_UIFormReorderableList.DoLayoutList();
            }
            EditorGUI.EndDisabledGroup();
            if (GUI.changed)
            {
                Repaint();
            }
        }


        private void BrowseFairyProjectDirectory()
        {
            string directory = EditorUtility.OpenFilePanelWithFilters("Select Fairy Project", FairyGUIEditorSetting.Instance.FairyGUIProject, new[] { "Fairy file", "fairy" });
            if (!string.IsNullOrEmpty(directory))
            {
                FairyGUIEditorSetting.Instance.FairyGUIProject = GameUtility.IO.AbsolutePathToProject(directory);
            }
        }

        private void DrawFairyGroupElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < m_FairyGUIFormRuntimeData.FairyGroups.Count)
            {
                var fairyGroup = m_FairyGUIFormRuntimeData.FairyGroups[index];

                // 将整个元素宽度限制为300
                float totalWidth = 300;
                float nameFieldWidth = totalWidth - 70; // 名称字段的宽度
                float depthFieldWidth = 60;             // 深度字段的宽度
                float spacing = 5;                      // 间距

                // 调整 rect 以符合总宽度
                rect.width = totalWidth;

                // 绘制名称字段
                fairyGroup.groupName = EditorGUI.DelayedTextField(new Rect(rect.x, rect.y, nameFieldWidth, EditorGUIUtility.singleLineHeight), fairyGroup.groupName);

                // 绘制深度字段
                fairyGroup.depth = EditorGUI.DelayedIntField(new Rect(rect.x + nameFieldWidth + spacing, rect.y, depthFieldWidth, EditorGUIUtility.singleLineHeight), fairyGroup.depth);

                // 显示错误提示信息
                if (string.IsNullOrEmpty(fairyGroup.groupName))
                {
                    EditorGUI.HelpBox(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + spacing, totalWidth, 25), "The group name cannot be empty.", MessageType.Error);
                }
            }
        }

        private void DrawFairyFormElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < m_FairyGUIFormRuntimeData.FairyForms.Count)
            {
                var fairyForm = m_FairyGUIFormRuntimeData.FairyForms[index];

                // 元素布局
                Rect idRect = new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(idRect, fairyForm.id.ToString());

                Rect packageNameRect = new Rect(rect.x + 55, rect.y, 150, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(packageNameRect, fairyForm.packageName);

                Rect popupRect = new Rect(rect.x + 180, rect.y, 100, EditorGUIUtility.singleLineHeight);
                int selectedIndex = EditorGUI.Popup(popupRect, fairyForm.GroupIndex, m_GroupNames);
                if (selectedIndex != fairyForm.GroupIndex || fairyForm.uiGroupName != m_GroupNames[selectedIndex])
                {
                    fairyForm.GroupIndex = selectedIndex;
                    fairyForm.uiGroupName = m_GroupNames[selectedIndex];
                }

                Rect allowMultiInstanceRect = new Rect(rect.x + 315, rect.y, 25, EditorGUIUtility.singleLineHeight);
                fairyForm.allowMultiInstance = EditorGUI.Toggle(allowMultiInstanceRect, fairyForm.allowMultiInstance);

                Rect pauseCoveredUIFormRect = new Rect(rect.x + 470, rect.y, 25, EditorGUIUtility.singleLineHeight);
                fairyForm.pauseCoveredUIForm = EditorGUI.Toggle(pauseCoveredUIFormRect, fairyForm.pauseCoveredUIForm);

                Rect assetNameRect = new Rect(rect.x + 565, rect.y, rect.width - 625, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(assetNameRect, fairyForm.assetName);

                Rect objectAssetRect = new Rect(rect.x + 565 + 450, rect.y, rect.width - 625, EditorGUIUtility.singleLineHeight);
                bool valid = !AssetDatabase.LoadAssetAtPath<GameObject>(fairyForm.objectAssetName);
                GUIStyle style = new GUIStyle(EditorStyles.label);
                style.normal.textColor = Color.yellow;
                EditorGUI.LabelField(objectAssetRect, fairyForm.objectAssetName, valid ? style : EditorStyles.label);
            }
        }

        private static void GeneraConstCode(FairyGUIFormRuntimeData runtimeData)
        {
            if (!runtimeData || runtimeData.FairyForms == null)
            {
                return;
            }

            string fileName = "FairyGUIFormId";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("//this file is generate by tools,do not alter it...")
                         .AppendLine($"namespace Game.FairyGUI.Runtime")
                         .AppendLine("{")
                         .AppendLine($"\tpublic enum {fileName} : byte")
                         .AppendLine("\t{");

            for (int i = 0; i < runtimeData.FairyForms.Count; i++)
            {
                var fairyForm = runtimeData.FairyForms[i];
                var enumName = fairyForm.packageName;
                if (!CodeGenerator.IsValidLanguageIndependentIdentifier(enumName))
                {
                    Debug.LogWarning($"Warning:  PackageName='{fairyForm.packageName}' '{enumName}' is not a valid enum name.");
                    return;
                }

                stringBuilder.AppendLine($"\t\t// {fairyForm.packageName}").AppendLine($"\t\t{enumName} = {fairyForm.id},");
            }

            stringBuilder.AppendLine("\t}").AppendLine("}");

            string outputFileName = Utility.Path.GetRegularPath(Path.Combine(FairyGUIEditorSetting.Instance.GeneralCodePath, "FairyEnum", fileName + ".cs"));
            FileInfo fileInfo = new FileInfo(outputFileName);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
                FileInfo meta = new FileInfo(outputFileName + ".meta");
                if (meta.Exists)
                {
                    meta.Delete();
                }
            }

            if (fileInfo.Directory is { Exists: false })
            {
                fileInfo.Directory.Create();
            }

            using (FileStream fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter stream = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    stream.Write(stringBuilder.ToString());
                }
            }

            Debug.Log(Utility.Text.Format("Generate code file '{0}' success.", outputFileName));
            AssetDatabase.Refresh();
        }

        private static void GenerateUIFormScript(FairyGUIFormRuntimeData runtimeData)
        {
            if (!runtimeData || runtimeData.FairyForms == null)
            {
                return;
            }

            string scriptFolder = FairyGUIEditorSetting.Instance.GeneralCodePath + "/{0}";
            for (int i = 0; i < runtimeData.FairyForms.Count; i++)
            {
                string packageName = runtimeData.FairyForms[i].packageName;
                string outputFileName = Path.Combine(string.Format(scriptFolder, packageName), $"{packageName}.cs");

                if (File.Exists(outputFileName))
                {
                    Debug.Log($"Script '{packageName}Form.cs' already exists.");
                    return;
                }

                string scriptContent = $@"
using FairyGUI;
using Game.Update;
using DEngine.Runtime;

namespace Game.FairyGUI.Runtime
{{
    public partial class {packageName} : FairyGUIFormBase
    {{
       
    }}
}}
";
                FileInfo fileInfo = new FileInfo(outputFileName);
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                    FileInfo meta = new FileInfo(outputFileName + ".meta");
                    if (meta.Exists)
                    {
                        meta.Delete();
                    }
                }

                if (fileInfo.Directory is { Exists: false })
                {
                    fileInfo.Directory.Create();
                }

                using (FileStream fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter stream = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        stream.Write(scriptContent.ToString());
                    }
                }

                Debug.Log($"Script '{packageName}Form.cs' generated at '{outputFileName}'.");
            }

            EditorPrefs.SetBool("GenerateFairyGUIFormScript", true);
            AssetDatabase.Refresh();
        }

        private static void GeneraPrefab(FairyGUIFormRuntimeData runtimeData)
        {
            if (!runtimeData || runtimeData.FairyForms == null)
            {
                return;
            }

            List<GameObject> tempObjs = new List<GameObject>();
            string prefabFolder = FairyGUIEditorSetting.Instance.GeneralObjectAssetName;

            try
            {
                foreach (FairyForm fairyForm in runtimeData.FairyForms)
                {
                    string prefabPath = Path.Combine(prefabFolder, $"{fairyForm.packageName}.prefab");

                    if (File.Exists(prefabPath))
                    {
                        continue;
                    }

                    GameObject tempPrefab = new(fairyForm.packageName);
                    PrefabUtility.SaveAsPrefabAsset(tempPrefab, prefabPath);
                    tempObjs.Add(tempPrefab);
                    Debug.Log($"Prefab '{fairyForm}' generated at '{prefabPath}'.");
                }
            }
            finally
            {
                for (int i = 0; i < tempObjs.Count; i++)
                {
                    DestroyImmediate(tempObjs[i]);
                }
            }
        }

        [DidReloadScripts]
        private static void AddUIFormComponentScripts()
        {
            if (!EditorPrefs.GetBool("GenerateFairyGUIFormScript"))
            {
                return;
            }

            EditorPrefs.SetBool("GenerateFairyGUIFormScript", false);
            var runtimeData = EditorTools.LoadScriptableObject<FairyGUIFormRuntimeData>(UpdateAssetUtility.GetScriptableAsset(nameof(FairyGUIFormRuntimeData)));
            if (!runtimeData || runtimeData.FairyForms == null)
            {
                return;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //获取到热更程序集
            HashSet<string> assembliesNames = new(DEngineSetting.Instance.UpdateAssemblies.Select(item => item.Replace(".dll", null)));
            var hotUpdateAssembly = assemblies.FirstOrDefault(assembly => assembliesNames.Contains(assembly.GetName().Name));
            if (hotUpdateAssembly == null)
            {
                Debug.LogWarning("获取到热更新程序集失败");
                return;
            }

            string prefabFolder = FairyGUIEditorSetting.Instance.GeneralObjectAssetName;

            foreach (FairyForm fairyForm in runtimeData.FairyForms)
            {
                Type type = hotUpdateAssembly.GetType($"Game.FairyGUI.Runtime.{fairyForm.packageName}");
                if (type == null)
                {
                    continue;
                }

                string prefabPath = Path.Combine(prefabFolder, $"{fairyForm.packageName}.prefab");

                GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);

                if (prefab != null)
                {
                    if (!prefab.GetComponent(fairyForm.packageName))
                    {
                        prefab.AddComponent(type);
                        Debug.Log($"Prefab AddUIFormComponent '{type.Name}' to'{prefabPath}'.");
                        PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, prefabPath, InteractionMode.AutomatedAction);
                    }

                    PrefabUtility.UnloadPrefabContents(prefab);
                }
            }
        }

        private static void LoadFairyGUIFormRuntimeData(FairyGUIFormRuntimeData runtimeData)
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(FairyGUIEditorSetting.Instance.FairyGUIDataPath))
            {
                return;
            }

            UIPackage.RemoveAllPackages();
            UIPackage.branch = null;
            FontManager.Clear();
            NTexture.DisposeEmpty();
            UIObjectFactory.Clear();

            string[] ids = AssetDatabase.FindAssets("_fui t:textAsset");
            Dictionary<string, string> rootDirectories = new();
            int cnt = ids.Length;
            for (int i = 0; i < cnt; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(ids[i]);
                int pos = assetPath.LastIndexOf("_fui", StringComparison.Ordinal);
                if (pos == -1)
                {
                    continue;
                }

                assetPath = assetPath.Substring(0, pos);
                if (AssetDatabase.AssetPathToGUID(assetPath) != null)
                {
                    var pkg = UIPackage.AddPackage(assetPath, (string name, string extension, Type type, out DestroyMethod destroyMethod) =>
                    {
                        destroyMethod = DestroyMethod.Unload;
                        return AssetDatabase.LoadAssetAtPath(name + extension, type);
                    });
                    rootDirectories.Add(pkg.name, assetPath.Substring(0, pos));
                }
            }

            List<UIPackage> pkgs = UIPackage.GetPackages();
            pkgs.Sort(CompareUIPackage);

            for (int i = 0; i < pkgs.Count; i++)
            {
                int index = i;
                FairyForm item = runtimeData.FairyForms.Find(o => o.id == index);
                if (item != null)
                {
                    item.GroupIndex = Array.IndexOf(m_GroupNames, item.uiGroupName);
                    if (item.GroupIndex < 0)
                    {
                        item.GroupIndex = 0;
                    }

                    continue;
                }

                item = new FairyForm();
                item.id = index;
                item.packageName = pkgs[index].name;
                item.assetName = pkgs[index].assetPath + "_fui.bytes";
                item.objectAssetName = FairyGUIEditorSetting.Instance.GeneralObjectAssetName + "/" + pkgs[index].name + ".prefab";
                item.allowMultiInstance = false;
                item.uiGroupName = m_GroupNames[0];
                item.pauseCoveredUIForm = false;
                foreach (Dictionary<string, string> dependency in pkgs[i].dependencies)
                {
                    item.dependencyPackages.Add(dependency["name"]);
                }

                string[] allAssetPaths = AssetDatabase.FindAssets(item.packageName, new[] { FairyGUIEditorSetting.Instance.FairyGUIDataPath });
                foreach (string guid in allAssetPaths)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    //排除自身
                    if (path == item.assetName)
                    {
                        continue;
                    }

                    if (path.StartsWith(FairyGUIEditorSetting.Instance.FairyGUIDataPath + "/" + item.packageName + "_"))
                    {
                        if (item.dependencyAssets.Contains(path))
                        {
                            continue;
                        }

                        item.dependencyAssets.Add(path);
                    }
                }

                runtimeData.FairyForms.Add(item);
            }
        }

        private static FairyGUIFormRuntimeData GetFairyGUIData()
        {
            var data = EditorTools.LoadScriptableObject<FairyGUIFormRuntimeData>(UpdateAssetUtility.GetScriptableAsset(nameof(FairyGUIFormRuntimeData)));
            m_GroupNames = data.FairyGroups.Select(o => o.groupName).ToArray();
            return data;
        }

        private static int CompareUIPackage(UIPackage u1, UIPackage u2)
        {
            return string.Compare(u1.name, u2.name, StringComparison.Ordinal);
        }


        public static string packageFullPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_PackageFullPath))
                    m_PackageFullPath = GetPackageFullPath();

                return m_PackageFullPath;
            }
        }

        private static string m_PackageFullPath;

        private static string GetPackageFullPath()
        {
            // Check for potential UPM package
            string packagePath = Path.GetFullPath("Packages/com.dvalmi.fairygui");
            if (Directory.Exists(packagePath))
            {
                return packagePath;
            }

            packagePath = Path.GetFullPath("Assets/..");
            if (Directory.Exists(packagePath))
            {
                if (Directory.Exists(packagePath + "/Assets/Packages/com.dvalmi.fairygui/Shader"))
                {
                    return packagePath + "/Assets/Packages/com.dvalmi.fairygui";
                }

                if (Directory.Exists(packagePath + "/Assets/FairyGUI/Shader"))
                {
                    return packagePath + "/Assets/FairyGUI";
                }

                string[] matchingPaths = Directory.GetDirectories(packagePath, "FairyGUI", SearchOption.AllDirectories);
                string path = ValidateLocation(matchingPaths, packagePath);
                if (path != null)
                {
                    return packagePath + path;
                }
            }

            return null;
        }

        private static string ValidateLocation(string[] paths, string projectPath)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (Directory.Exists(paths[i] + "/Shader"))
                {
                    string folderPath = paths[i].Replace(projectPath, "");
                    folderPath = folderPath.TrimStart('\\', '/');
                    return folderPath;
                }
            }

            return null;
        }

        private static void ImportBuiltinShader()
        {
            AssetDatabase.ImportPackage(packageFullPath + "/Shader/FairyShader.unitypackage", true);
        }
    }
}