using System.Collections.Generic;
using System.IO;
using System.Linq;
using DEngine;
using DEngine.Editor.ResourceTools;
using Game.Editor.ResourceTools;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Editor
{
    public class AssetCollectorEditor : MenuTreeEditorWindow
    {
        private readonly string m_DefaultConfigurationPath = "Assets/Game/AssetConfiguration/AssetBundlePackageCollector.asset";
        private MenuTreeView<AssetBundleCollector> m_MenuTreePackagesView;
        private MenuTreeViewItem<AssetBundleCollector> m_PackageSelectedItem;
        private MenuTreeView<AssetBundleGroupCollector> m_MenuTreeGroupsView;
        private MenuTreeViewItem<AssetBundleGroupCollector> m_GroupSelectedItem;
        private AssetCollectorTableView<AssetCollector> m_AssetCollectorTableView;
        private List<TableColumn<AssetCollector>> m_AssetCollectorColumns;
        private ResourceEditorController m_ResourceEditorController;

        private AssetBundlePackageCollector m_AssetBundlePackageCollector;
        private AssetBundleCollector m_SelectAssetBundleCollector;
        private Rect m_ToolbarRect = new Rect();
        private Rect m_PackageMenuRect = new Rect();
        private Rect m_RuleRect = new Rect();
        private GUIStyle m_LineStyle;
        private GUIStyle m_FolderBtnStyle;
        private GUIContent m_FolderBtnContent;
        private readonly float m_AddRectHeight = 50f;
        private bool m_IsDirty = false;
        private float PackageSpace => 200;

        [MenuItem("Game/Asset Collector", false, 1)]
        public static void OpenWindow()
        {
            var window = GetWindow<AssetCollectorEditor>("资源收集器");
            window.minSize = new Vector2(1300f, 420f);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Load();

            if (m_ResourceEditorController == null)
            {
                m_ResourceEditorController = new ResourceEditorController();
                m_ResourceEditorController.Load();
            }

            m_MenuTreePackagesView = new MenuTreeView<AssetBundleCollector>(false, true, true);
            {
                m_MenuTreePackagesView.onDrawFoldout = DrawFoldoutCallback;
                m_MenuTreePackagesView.onDrawRowContent = DrawPackagesMenuRowContentCallback;
                m_MenuTreePackagesView.onSelectionChanged = OnPackageSelectionChanged;
            }
            // 绘制包裹资源分组列表
            m_MenuTreeGroupsView = new MenuTreeView<AssetBundleGroupCollector>(false, true, true);
            {
                m_MenuTreeGroupsView.onDrawFoldout = DrawFoldoutCallback;
                m_MenuTreeGroupsView.onDrawRowContent = DrawGroupMenuRowContentCallback;
                m_MenuTreeGroupsView.onSelectionChanged = OnGroupSelectionChanged;
            }

            if (m_AssetCollectorColumns == null)
            {
                m_AssetCollectorColumns = GetAssetCollectorColumns();
            }

            m_AssetCollectorTableView = new AssetCollectorTableView<AssetCollector>(null, m_AssetCollectorColumns);
            {
                m_AssetCollectorTableView.OnRightAddRow = OnTreeViewRightAddRowCallback;
                m_AssetCollectorTableView.OnSelectionChanged += OnTreeViewSelectionChanged;
            }

            for (int i = 0; i < m_AssetBundlePackageCollector.PackagesCollector.Count; i++)
            {
                AssetBundleCollector package = m_AssetBundlePackageCollector.PackagesCollector[i];
                m_MenuTreePackagesView.AddItem(GetPackageDisplayName(package), package);
            }
            m_SelectAssetBundleCollector = m_AssetBundlePackageCollector.PackagesCollector.FirstOrDefault();
            RefreshAssetGroups();
            SetFocusAndEnsureSelectedItem();
        }



        protected override void OnGUI()
        {
            GUIToolbar();
            base.OnGUI();
            if (GUI.changed)
            {
                m_IsDirty = true;
            }
            Repaint();
        }


        private void Update()
        {
            if (m_IsDirty)
            {
                m_IsDirty = false;
                Save();
            }
        }

        private void Load()
        {
            m_AssetBundlePackageCollector = LoadAssetAtPath<AssetBundlePackageCollector>(m_DefaultConfigurationPath);
            if (m_AssetBundlePackageCollector == null)
            {
                m_AssetBundlePackageCollector = CreateInstance<AssetBundlePackageCollector>();
                m_AssetBundlePackageCollector.PackagesCollector.Add(new AssetBundleCollector());
                AssetDatabase.CreateAsset(m_AssetBundlePackageCollector, m_DefaultConfigurationPath);
                AssetDatabase.SaveAssets();
            }
        }

        private void Save()
        {
            EditorUtility.SetDirty(m_AssetBundlePackageCollector);
            AssetDatabase.SaveAssets();
        }

        private T LoadAssetAtPath<T>(string path) where T : Object
        {
            return (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
        }

        private void GUIToolbar()
        {
            m_ToolbarRect = new Rect(position.width - 100, 0, 100, 42);
            GUILayout.BeginHorizontal("Toolbar", GUILayout.ExpandWidth(true), GUILayout.Height(40));
            {
                GUILayout.FlexibleSpace();
                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.green;

                if (GUILayout.Button("Ping"))
                {
                    EditorGUIUtility.PingObject(m_AssetBundlePackageCollector);
                }

                if (GUILayout.Button("导出"))
                {
                    AssetCollectorEditorUtility.RefreshResourceCollection(m_SelectAssetBundleCollector);
                }
                if (GUILayout.Button("Save"))
                {
                    Save();
                    AssetCollectorEditorUtility.RefreshResourceCollection(m_SelectAssetBundleCollector);
                }
                GUI.backgroundColor = originalColor;
            }
            GUILayout.EndHorizontal();
        }

        protected override void OnGUIMenuTree()
        {
            m_LineStyle ??= new GUIStyle("EyeDropperHorizontalLine");
            m_FolderBtnStyle ??= new GUIStyle("SettingsIconButton");
            m_FolderBtnContent ??= new GUIContent(SourceFolder.Icon);

            EditorGUILayout.BeginVertical("box", GUILayout.Height(40));
            {
                GUILayout.Label("PackageName");
                if (m_SelectAssetBundleCollector != null)
                {
                    string packageName = EditorGUILayout.DelayedTextField(m_SelectAssetBundleCollector.PackageName, GUILayout.Width(150));
                    if (packageName != m_SelectAssetBundleCollector.PackageName)
                    {
                        m_SelectAssetBundleCollector.PackageName = packageName;
                        m_PackageSelectedItem.displayName = GetPackageDisplayName(m_SelectAssetBundleCollector);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("<None>", GUILayout.Width(150));
                }
                GUILayout.Label("Description");
                if (m_SelectAssetBundleCollector != null)
                {
                    string description = EditorGUILayout.DelayedTextField(m_SelectAssetBundleCollector.Description, GUILayout.Width(150));
                    if (description != m_SelectAssetBundleCollector.Description)
                    {
                        m_SelectAssetBundleCollector.Description = description;
                        m_PackageSelectedItem.displayName = GetPackageDisplayName(m_SelectAssetBundleCollector);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("<None>", GUILayout.Width(150));
                }
            }
            EditorGUILayout.EndVertical();
            m_PackageMenuRect = new Rect(0, m_ToolbarRect.height + m_AddRectHeight + 40, m_MenuTreeWidth, position.height - m_AddRectHeight - m_ToolbarRect.height - 90);
            m_MenuTreePackagesView.OnGUI(m_PackageMenuRect);

            if (!m_MenuTreePackagesView.HasSelection())
            {
                SetFocusAndEnsureSelectedItem();
            }
            GUILayout.BeginArea(new Rect(m_PackageMenuRect.width * 0.2F, m_PackageMenuRect.height + m_AddRectHeight + 90, m_PackageMenuRect.width * 0.6F, position.height - m_AddRectHeight - m_ToolbarRect.height));
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("+", GUILayout.Width(30)))
                    {
                        AssetBundleCollector package = new AssetBundleCollector();
                        m_AssetBundlePackageCollector.PackagesCollector.Add(package);
                        m_MenuTreePackagesView.AddItem(GetPackageDisplayName(package), package);
                        SetFocusAndEnsureSelectedItem();
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(30)))
                    {
                        if (m_SelectAssetBundleCollector != null)
                        {
                            m_AssetBundlePackageCollector.PackagesCollector.Remove(m_SelectAssetBundleCollector);
                            m_MenuTreePackagesView.RemoveItem(m_PackageSelectedItem);
                        }
                        SetFocusAndEnsureSelectedItem();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndArea();


            m_MenuGroupRect = new Rect(PackageSpace, m_ToolbarRect.height, m_MenuTreeWidth, position.height - m_AddRectHeight - m_ToolbarRect.height);
            m_MenuTreeGroupsView.OnGUI(m_MenuGroupRect);

            GUILayout.BeginArea(new Rect(PackageSpace + m_MenuGroupRect.width * 0.2F, m_MenuGroupRect.height + m_AddRectHeight, m_MenuGroupRect.width * 0.6F, position.height - m_AddRectHeight - m_ToolbarRect.height));
            {

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("+", GUILayout.Width(30)))
                    {
                        AssetBundleGroupCollector resourceGroup = new();
                        m_SelectAssetBundleCollector.Groups.Add(resourceGroup);
                        m_MenuTreeGroupsView.AddItem(GetGroupDisplayName(resourceGroup), resourceGroup);
                        SetFocusAndEnsureSelectedItem();
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(30)))
                    {
                        if (m_GroupSelectedItem != null)
                        {
                            m_MenuTreeGroupsView.RemoveItem(m_GroupSelectedItem);
                            m_SelectAssetBundleCollector.Groups.Remove(m_GroupSelectedItem.Data);
                            m_GroupSelectedItem = null;
                        }
                        SetFocusAndEnsureSelectedItem();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }

        protected override void OnGUISpace()
        {
            m_SpaceRect = new Rect(m_MenuTreeWidth, m_ToolbarRect.height, m_SpaceWidth, position.height - m_ToolbarRect.height);

            EditorGUIUtility.AddCursorRect(m_SpaceRect, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.MouseDown && m_SpaceRect.Contains(Event.current.mousePosition))
            {
                m_ResizingHorizontalSplitter = true;
            }

            if (m_ResizingHorizontalSplitter)
            {
                m_MenuTreeWidth = Event.current.mousePosition.x;
                m_SpaceRect.x = m_MenuTreeWidth;
                Repaint();
            }

            if (Event.current.type == EventType.MouseUp)
            {
                m_ResizingHorizontalSplitter = false;
            }
        }

        protected override void OnGUIContent()
        {
            m_ContentRect = new Rect(m_MenuTreeWidth + m_SpaceWidth + PackageSpace, m_ToolbarRect.height, position.width - m_MenuTreeWidth - m_SpaceWidth, position.height - m_ToolbarRect.height);

            m_RuleRect.Set(m_ContentRect.x, 120, m_ContentRect.width, m_ContentRect.height - 90);

            if (m_GroupSelectedItem != null)
            {
                AssetBundleGroupCollector group = m_GroupSelectedItem.Data;
                GUILayout.BeginArea(m_ContentRect);
                {
                    if (group.EnableGroup != EditorGUILayout.Toggle("启用分组", group.EnableGroup))
                    {
                        group.EnableGroup = !group.EnableGroup;
                    }
                    string groupName = EditorGUILayout.DelayedTextField("分组名(按 ; , | 分割)", group.GroupName);
                    if (group.GroupName != groupName)
                    {
                        group.SetGroupName(groupName);
                        m_GroupSelectedItem.displayName = GetGroupDisplayName(group);
                    }

                    string description = EditorGUILayout.TextField("分组描述", group.Description);

                    if (group.Description != description)
                    {

                        group.Description = description;
                        m_GroupSelectedItem.displayName = GetGroupDisplayName(group);
                    }
                }
                GUILayout.EndArea();

                //如果鼠标拖拽结束时，并且鼠标所在位置在文本输入框内 
                if (Event.current.type == EventType.DragExited && m_RuleRect.Contains(Event.current.mousePosition))
                {
                    if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                    {
                        for (int i = 0; i < DragAndDrop.paths.Length; i++)
                        {
                            string path = DragAndDrop.paths[i];
                            if (IsFolderPath(path))
                            {
                                AddAssetCollectorRow(path);
                            }
                        }
                    }
                }
                m_AssetCollectorTableView.OnGUI(m_RuleRect);
            }
        }

        private void OnPackageSelectionChanged(IList<int> selectedIds)
        {
            m_PackageSelectedItem = m_MenuTreePackagesView.GetItemById(selectedIds[0]);
            m_SelectAssetBundleCollector = m_PackageSelectedItem.Data;
            RefreshAssetGroups();
            SetFocusAndEnsureSelectedItem();
        }

        /// <summary>
        /// 菜单项选择改变
        /// </summary>
        /// <param name="selectedIds"></param>
        protected override void OnGroupSelectionChanged(IList<int> selectedIds)
        {
            m_GroupSelectedItem = m_MenuTreeGroupsView.GetItemById(selectedIds[0]);

            m_AssetCollectorTableView.SetTableViewData(m_GroupSelectedItem.Data.AssetCollectors, m_AssetCollectorColumns);
        }

        private void OnTreeViewSelectionChanged(List<AssetCollector> list)
        {
            if (list != null && list.Count > 0)
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(list[0].AssetPath));
            }
        }

        private void RefreshAssetGroups()
        {
            m_MenuTreeGroupsView.RemoveAll();
            if (m_SelectAssetBundleCollector != null)
            {
                for (int i = 0; i < m_SelectAssetBundleCollector.Groups.Count; i++)
                {
                    AssetBundleGroupCollector resourceGroup = m_SelectAssetBundleCollector.Groups[i];
                    m_MenuTreeGroupsView.AddItem(GetGroupDisplayName(resourceGroup), resourceGroup);
                }
            }
        }

        /// <summary>
        /// 绘制包裹行内容回调
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="row"></param>
        /// <param name="item"></param>
        /// <param name="label"></param>
        /// <param name="selected"></param>
        /// <param name="focused"></param>
        /// <param name="useBoldFont"></param>
        /// <param name="isPinging"></param>
        private void DrawPackagesMenuRowContentCallback(Rect rect, int row, MenuTreeViewItem<AssetBundleCollector> item, string label, bool selected, bool focused, bool useBoldFont, bool isPinging)
        {
            GUI.Box(rect, "", m_LineStyle);
            float space = 5f + item.depth * 15f;
            Rect lableRect = new Rect(rect.x + space, rect.y, rect.width - space, rect.height);
            GUI.Label(lableRect, item.displayName);
        }

        /// <summary>
        /// 绘制分组行内容回调
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="row"></param>
        /// <param name="item"></param>
        /// <param name="label"></param>
        /// <param name="selected"></param>
        /// <param name="focused"></param>
        /// <param name="useBoldFont"></param>
        /// <param name="isPinging"></param>
        private void DrawGroupMenuRowContentCallback(Rect rect, int row, MenuTreeViewItem<AssetBundleGroupCollector> item, string label, bool selected, bool focused, bool useBoldFont, bool isPinging)
        {
            GUI.Box(rect, "", m_LineStyle);
            float space = 5f + item.depth * 15f;
            Rect lableRect = new Rect(rect.x + space, rect.y, rect.width - space, rect.height);
            EditorGUI.BeginDisabledGroup(!item.Data.EnableGroup);
            GUI.Label(lableRect, item.displayName);
            EditorGUI.EndDisabledGroup();
        }

        private void OnTreeViewRightAddRowCallback()
        {
            AddAssetCollectorRow(string.Empty);
        }

        private void SetFocusAndEnsureSelectedItem()
        {
            m_MenuTreePackagesView.SetFocusAndEnsureSelectedItem();
            m_MenuTreeGroupsView.SetFocusAndEnsureSelectedItem();
        }

        /// <summary>
        /// 获取包裹显示名字
        /// </summary>
        /// <param name="packages"></param>
        /// <returns></returns>
        private string GetPackageDisplayName(AssetBundleCollector packages)
        {
            return string.IsNullOrEmpty(packages.Description) ? packages.PackageName : string.Format("{0}({1})", packages.PackageName, packages.Description);
        }

        /// <summary>
        /// 获取分组显示名字
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private string GetGroupDisplayName(AssetBundleGroupCollector group)
        {
            return string.IsNullOrEmpty(group.Description) ? group.GroupName : string.Format("{0}({1})", group.GroupName, group.Description);
        }

        /// <summary>
        /// 获取资产列
        /// </summary>
        /// <returns></returns>
        private List<TableColumn<AssetCollector>> GetAssetCollectorColumns()
        {
            var columns = new List<TableColumn<AssetCollector>>();

            TableColumn<AssetCollector> column1 = CreateColumn(new GUIContent("Enable", "启用资产"),
                (cellRect, data, rowIndex, isSelected, isFocused) =>
                {
                    data.Enable = EditorGUI.Toggle(cellRect, data.Enable);
                }, 50, 50, 60);
            columns.Add(column1);

            TableColumn<AssetCollector> column2 = CreateColumn(new GUIContent("Name", "资产命名"),
                (cellRect, data, rowIndex, isSelected, isFocused) =>
                {
                    data.Name = EditorGUI.TextField(cellRect, data.Name);
                }, 100, 50, 150);
            columns.Add(column2);

            TableColumn<AssetCollector> column3 = CreateColumn(new GUIContent("LoadType", "加载类型"),
                (cellRect, data, rowIndex, isSelected, isFocused) =>
                {
                    data.LoadType = (LoadType)EditorGUI.EnumPopup(cellRect, data.LoadType);
                }, 150, 110, 200);
            columns.Add(column3);

            TableColumn<AssetCollector> column4 = CreateColumn(new GUIContent("Packed", "是否为本地资源（这些资源将会跟随包体一起发布，作为基础资源）"),
                (cellRect, data, rowIndex, isSelected, isFocused) =>
                {
                    data.Packed = EditorGUI.Toggle(cellRect, data.Packed);
                }, 50, 50, 60);
            columns.Add(column4);

            TableColumn<AssetCollector> column5 = CreateColumn(new GUIContent("FileSystem", "文件系统（可为空）"),
                (cellRect, data, rowIndex, isSelected, isFocused) =>
                {
                    data.FileSystem = EditorGUI.TextField(cellRect, data.FileSystem);
                }, 100, 50, 150);
            columns.Add(column5);
            TableColumn<AssetCollector> column6 = CreateColumn(new GUIContent("Variant", "资源变体（可为空）"),
                (cellRect, data, rowIndex, isSelected, isFocused) =>
                {
                    data.Variant = EditorGUI.TextField(cellRect, data.Variant);
                }, 100, 50, 150);
            columns.Add(column6);
            TableColumn<AssetCollector> column7 = CreateColumn(new GUIContent("AssetPath", "资源目录"),
                (cellRect, data, rowIndex, isSelected, isFocused) =>
                {
                    Rect textFildRect = new Rect(cellRect.x, cellRect.y, cellRect.width, cellRect.height);
                    data.AssetPath = EditorGUI.DelayedTextField(textFildRect, data.AssetPath);
                }, 300, 200, 400);
            columns.Add(column7);

            TableColumn<AssetCollector> column8 = CreateColumn(new GUIContent("FilterType", "资源筛选类型"),
                (cellRect, data, rowIndex, isSelected, isFocused) =>
                {
                    data.FilterType = (FilterType)EditorGUI.EnumPopup(cellRect, data.FilterType);
                }, 150, 120, 160);
            columns.Add(column8);

            TableColumn<AssetCollector> column9 = CreateColumn(new GUIContent("SearchPatterns", "资源筛选模式"),
                (cellRect, data, rowIndex, isSelected, isFocused) =>
                {
                    data.SearchPatterns = EditorGUI.TextField(cellRect, data.SearchPatterns);
                }, 100, 100, 160);
            columns.Add(column9);
            return columns;
        }

        /// <summary>
        /// 创建列
        /// </summary>
        /// <param name="content"></param>
        /// <param name="drawCellMethod"></param>
        /// <param name="width"></param>
        /// <param name="minWidth"></param>
        /// <param name="maxWidth"></param>
        /// <param name="canSort"></param>
        /// <param name="autoResize"></param>
        /// <returns></returns>
        private TableColumn<AssetCollector> CreateColumn(GUIContent content, DrawCellMethod<AssetCollector> drawCellMethod, float width, float minWidth, float maxWidth, bool canSort = true, bool autoResize = true)
        {
            TableColumn<AssetCollector> column = new TableColumn<AssetCollector>();
            {
                column.headerContent = content;
                column.width = width;
                column.minWidth = minWidth;
                column.maxWidth = maxWidth;
                column.canSort = canSort;
                column.autoResize = autoResize;
                column.DrawCell = drawCellMethod;
            }
            return column;
        }

        /// <summary>
        /// 添加规则行
        /// </summary>
        /// <param name="path"></param>
        private void AddAssetCollectorRow(string path)
        {
            AssetCollector assetCollector = new()
            {
                Groups = m_GroupSelectedItem.Data.GroupName,
                AssetPath = path
            };
            m_AssetCollectorTableView.AddData(assetCollector);
            m_IsDirty = true;
        }

        /// <summary>
        /// 是否是文件夹路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsFolderPath(string path)
        {
            return Directory.Exists(path);
        }

        private void OnDestroy()
        {
            Save();
        }
    }
}