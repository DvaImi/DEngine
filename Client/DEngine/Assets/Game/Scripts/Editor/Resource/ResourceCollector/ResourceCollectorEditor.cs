using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DEngine;
using DEngine.Editor.ResourceTools;
using Game.Editor.ResourceTools;
using Game.Editor.Toolbar;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEvent = UnityEngine.Event;

namespace Game.Editor
{
    public class ResourceCollectorEditor : MenuTreeEditorWindow
    {
        private MenuTreeView<ResourceGroupsCollector> m_MenuTreePackagesView;
        private MenuTreeViewItem<ResourceGroupsCollector> m_PackageSelectedItem;
        private MenuTreeView<ResourceGroupCollector> m_MenuTreeGroupsView;
        private MenuTreeViewItem<ResourceGroupCollector> m_GroupSelectedItem;
        private ResourceCollectorTableView<ResourceCollector> m_AssetCollectorTableView;
        private List<TableColumn<ResourceCollector>> m_AssetCollectorColumns;
        private ResourceEditorController m_ResourceEditorController;

        private ResourcePackagesCollector m_AssetBundlePackageCollector;
        private ResourceGroupsCollector m_SelectAssetBundleCollector;
        private Rect m_ToolbarRect;
        private Rect m_PackageMenuRect;
        private Rect m_RuleRect;
        private GUIStyle m_LineStyle;
        private GUIStyle m_FolderBtnStyle;
        private GUIContent m_FolderBtnContent;

        private GUIContent m_EmptyContent;
        private GUIContent m_EnableContent;
        private GUIContent m_NameContent;
        private GUIContent m_LoadTypeContent;
        private GUIContent m_PackedContent;
        private GUIContent m_FileSystemContent;
        private GUIContent m_VariantContent;
        private GUIContent m_AssetContent;
        private GUIContent m_FilterTypeContent;
        private GUIContent m_AssetPathContent;
        private GUIContent m_CleanContent;
        private GUIContent m_ExportContent;
        private GUIContent m_SaveContent;
        private readonly float m_AddRectHeight = 50f;
        private bool m_IsDirty;
        private float PackageSpace => 200;

        [MenuItem("Game/Resource Tools/Resource Collector", false, 0)]
        [EditorToolbarMenu("Resource Collector", 0, 100)]
        public static void OpenWindow()
        {
            var window = GetWindow<ResourceCollectorEditor>("资源收集器");
            window.minSize = new Vector2(1300f, 420f);
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            m_EmptyContent = new GUIContent("");
            m_EnableContent = new GUIContent("Enable", "启用资产");
            m_NameContent = new GUIContent("Name", "资产命名");
            m_LoadTypeContent = new GUIContent("LoadType", "加载类型");
            m_PackedContent = new GUIContent("Packed", "是否为本地资源（这些资源将会跟随包体一起发布，作为基础资源）");
            m_FileSystemContent = new GUIContent("FileSystem", "文件系统（可为空）");
            m_VariantContent = new GUIContent("Variant", "资源变体（可为空）");
            m_AssetContent = new GUIContent("AssetObject", "资源目录");
            m_FilterTypeContent = new GUIContent("FilterType", "资源筛选类型");
            m_AssetPathContent = new GUIContent("AssetPath", "资源路径");
            m_CleanContent = EditorGUIUtility.TrTextContentWithIcon("Clean", "清理无效资源", "CloudConnect");
            m_ExportContent = EditorGUIUtility.TrTextContentWithIcon("Export", "导出配置", "Project");
            m_SaveContent = EditorGUIUtility.TrTextContentWithIcon("Save", "保存配置", "SaveAs");

            Load();

            if (m_ResourceEditorController == null)
            {
                m_ResourceEditorController = new ResourceEditorController();
                m_ResourceEditorController.Load();
            }

            m_MenuTreePackagesView = new MenuTreeView<ResourceGroupsCollector>(false, true);
            {
                m_MenuTreePackagesView.onDrawFoldout = DrawFoldoutCallback;
                m_MenuTreePackagesView.onDrawRowContent = DrawPackagesMenuRowContentCallback;
                m_MenuTreePackagesView.onSelectionChanged = OnPackageSelectionChanged;
            }
            // 绘制包裹资源分组列表
            m_MenuTreeGroupsView = new MenuTreeView<ResourceGroupCollector>(false, true);
            {
                m_MenuTreeGroupsView.onDrawFoldout = DrawFoldoutCallback;
                m_MenuTreeGroupsView.onDrawRowContent = DrawGroupMenuRowContentCallback;
                m_MenuTreeGroupsView.onSelectionChanged = OnGroupSelectionChanged;
            }

            m_AssetCollectorColumns ??= GetAssetCollectorColumns();

            m_AssetCollectorTableView = new ResourceCollectorTableView<ResourceCollector>(null, m_AssetCollectorColumns);
            {
                m_AssetCollectorTableView.OnRightAddRow = OnTreeViewRightAddRowCallback;
            }

            foreach (var package in m_AssetBundlePackageCollector.PackagesCollector)
            {
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
            m_AssetBundlePackageCollector = EditorTools.LoadScriptableObject<ResourcePackagesCollector>();
        }

        private void Save()
        {
            EditorUtility.SetDirty(m_AssetBundlePackageCollector);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void GUIToolbar()
        {
            m_ToolbarRect = new Rect(position.width - 100, 0, 100, 60);
            GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true), GUILayout.Height(50));
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(m_CleanContent, GUILayout.Height(30)))
                {
                    var unknownAssetCount = CleanUnknownAssets();
                    if (unknownAssetCount > 0)
                    {
                        Debug.Log(Utility.Text.Format("Clean complete, {0} unknown assets  has been removed.", unknownAssetCount));
                    }
                }

                GUILayout.Space(10);
                if (GUILayout.Button(m_ExportContent, GUILayout.Height(30)))
                {
                    ResourceCollectorEditorUtility.RefreshResourceCollection(m_SelectAssetBundleCollector);
                }

                GUILayout.Space(10);
                if (GUILayout.Button(m_SaveContent, GUILayout.Height(30)))
                {
                    Save();
                }
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
                GUILayout.Space(10);
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
                        ResourceGroupsCollector package = new ResourceGroupsCollector();
                        m_AssetBundlePackageCollector.PackagesCollector.Add(package);
                        m_MenuTreePackagesView.AddItem(GetPackageDisplayName(package), package);
                    }

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(30)))
                    {
                        void ConfirmDeletion()
                        {
                            if (m_SelectAssetBundleCollector != null)
                            {
                                m_AssetBundlePackageCollector.PackagesCollector.Remove(m_SelectAssetBundleCollector);
                                m_MenuTreePackagesView.RemoveItem(m_PackageSelectedItem);
                            }

                            SetFocusAndEnsureSelectedItem();
                        }

                        EditorTools.EditorDisplay("提示", $"确定要删除{m_PackageSelectedItem?.Data?.PackageName}?", "确认", "取消", ConfirmDeletion);
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
                        ResourceGroupCollector resourceGroup = new();
                        m_SelectAssetBundleCollector?.Groups.Add(resourceGroup);
                        m_MenuTreeGroupsView.AddItem(GetGroupDisplayName(resourceGroup), resourceGroup);
                        SetFocusAndEnsureSelectedItem();
                    }

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(30)))
                    {
                        void ConfirmDeletion()
                        {
                            if (m_GroupSelectedItem != null)
                            {
                                m_MenuTreeGroupsView.RemoveItem(m_GroupSelectedItem);
                                m_SelectAssetBundleCollector.Groups.Remove(m_GroupSelectedItem.Data);
                                m_GroupSelectedItem = null;
                            }

                            SetFocusAndEnsureSelectedItem();
                        }

                        EditorTools.EditorDisplay("提示", $"确定要删除{m_GroupSelectedItem?.Data?.GroupName}?", "确认", "取消", ConfirmDeletion);
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

            if (UnityEvent.current.type == EventType.MouseDown && m_SpaceRect.Contains(UnityEvent.current.mousePosition))
            {
                m_ResizingHorizontalSplitter = true;
            }

            if (m_ResizingHorizontalSplitter)
            {
                m_MenuTreeWidth = UnityEvent.current.mousePosition.x;
                m_SpaceRect.x = m_MenuTreeWidth;
                Repaint();
            }

            if (UnityEvent.current.type == EventType.MouseUp)
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
                ResourceGroupCollector group = m_GroupSelectedItem.Data;
                GUILayout.BeginArea(m_ContentRect);
                {
                    if (group.EnableGroup != EditorGUILayout.Toggle("启用分组", group.EnableGroup))
                    {
                        group.EnableGroup = !group.EnableGroup;
                    }

                    string groupName = EditorGUILayout.DelayedTextField("分组名(按 | 分割)", group.GroupName);
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
                if (UnityEvent.current.type == EventType.DragExited && m_RuleRect.Contains(UnityEvent.current.mousePosition))
                {
                    if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                    {
                        foreach (var path in DragAndDrop.paths)
                        {
                            if (!CanAddAssetCollectorRow(path))
                            {
                                Debug.LogWarning($"Asset path '{path}' is duplicated");
                                continue;
                            }

                            AddAssetCollectorRow(path);
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

        private void RefreshAssetGroups()
        {
            m_MenuTreeGroupsView.RemoveAll();
            if (m_SelectAssetBundleCollector != null)
            {
                int groupsCount = m_SelectAssetBundleCollector.Groups.Count;
                if (groupsCount > 0)
                {
                    for (int i = 0; i < groupsCount; i++)
                    {
                        ResourceGroupCollector resourceGroup = m_SelectAssetBundleCollector.Groups[i];
                        m_MenuTreeGroupsView.AddItem(GetGroupDisplayName(resourceGroup), resourceGroup);
                    }
                }
                else
                {
                    m_AssetCollectorTableView.SetTableViewData(null, m_AssetCollectorColumns);
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
        private void DrawPackagesMenuRowContentCallback(Rect rect, int row, MenuTreeViewItem<ResourceGroupsCollector> item, string label, bool selected, bool focused, bool useBoldFont, bool isPinging)
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
        private void DrawGroupMenuRowContentCallback(Rect rect, int row, MenuTreeViewItem<ResourceGroupCollector> item, string label, bool selected, bool focused, bool useBoldFont, bool isPinging)
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
        private string GetPackageDisplayName(ResourceGroupsCollector packages)
        {
            return string.IsNullOrEmpty(packages.Description) ? packages.PackageName : string.Format("{0}({1})", packages.PackageName, packages.Description);
        }

        /// <summary>
        /// 获取分组显示名字
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private string GetGroupDisplayName(ResourceGroupCollector group)
        {
            return string.IsNullOrEmpty(group.Description) ? group.GroupName : string.Format("{0}({1})", group.GroupName, group.Description);
        }

        #region DrawItem

        /// <summary>
        /// 获取资产列
        /// </summary>
        /// <returns></returns>
        private List<TableColumn<ResourceCollector>> GetAssetCollectorColumns()
        {
            var columns = new List<TableColumn<ResourceCollector>>();
            TableColumn<ResourceCollector> column1 = CreateColumn(m_EnableContent, DrawEnableItem, 50, 50, 60);
            columns.Add(column1);
            TableColumn<ResourceCollector> column2 = CreateColumn(m_NameContent, DrawNameItem, 100, 50, 150);
            columns.Add(column2);
            TableColumn<ResourceCollector> column3 = CreateColumn(m_LoadTypeContent, DrawLoadTypeItem, 150, 110, 200);
            columns.Add(column3);
            TableColumn<ResourceCollector> column4 = CreateColumn(m_PackedContent, DrawPackedItem, 50, 50, 60);
            columns.Add(column4);
            TableColumn<ResourceCollector> column5 = CreateColumn(m_FileSystemContent, DrawFileSystemItem, 100, 50, 150);
            columns.Add(column5);
            TableColumn<ResourceCollector> column6 = CreateColumn(m_VariantContent, DrawVariantItem, 100, 50, 150);
            columns.Add(column6);
            TableColumn<ResourceCollector> column7 = CreateColumn(m_AssetContent, DrawAssetObjectItem, 150, 120, 160);
            columns.Add(column7);
            TableColumn<ResourceCollector> column8 = CreateColumn(m_FilterTypeContent, DrawFilterTypeItem, 150, 120, 160);
            columns.Add(column8);
            TableColumn<ResourceCollector> column9 = CreateColumn(m_AssetPathContent, DrawAssetPathItem, 300, 200, 400);
            columns.Add(column9);
            return columns;
        }

        private void DrawEnableItem(Rect cellRect, ResourceCollector data, int rowIndex, bool isSelected, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            Rect enableRect = new Rect(cellRect.x + (cellRect.width / 2), cellRect.y, cellRect.width, cellRect.height);
            data.Enable = EditorGUI.Toggle(enableRect, data.Enable);
            data.Enable = data.Enable && data.IsValid;
            m_IsDirty = EditorGUI.EndChangeCheck();
        }

        private void DrawNameItem(Rect cellRect, ResourceCollector data, int rowIndex, bool isSelected, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            data.Name = EditorGUI.TextField(cellRect, data.Name);
            m_IsDirty = EditorGUI.EndChangeCheck();
        }

        private void DrawLoadTypeItem(Rect cellRect, ResourceCollector data, int rowIndex, bool isSelected, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            data.LoadType = (LoadType)EditorGUI.EnumPopup(cellRect, data.LoadType);
            m_IsDirty = EditorGUI.EndChangeCheck();
        }

        private void DrawPackedItem(Rect cellRect, ResourceCollector data, int rowIndex, bool isSelected, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            Rect packedRect = new Rect(cellRect.x + (cellRect.width / 2), cellRect.y, cellRect.width, cellRect.height);
            data.Packed = EditorGUI.Toggle(packedRect, data.Packed);
            m_IsDirty = EditorGUI.EndChangeCheck();
        }

        private void DrawFileSystemItem(Rect cellRect, ResourceCollector data, int rowIndex, bool isSelected, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            data.FileSystem = EditorGUI.TextField(cellRect, data.FileSystem);
            m_IsDirty = EditorGUI.EndChangeCheck();
        }

        private void DrawVariantItem(Rect cellRect, ResourceCollector data, int rowIndex, bool isSelected, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            data.Variant = EditorGUI.TextField(cellRect, data.Variant);
            m_IsDirty = EditorGUI.EndChangeCheck();
        }

        private void DrawAssetObjectItem(Rect cellRect, ResourceCollector data, int rowIndex, bool isSelected, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            data.Asset = EditorGUI.ObjectField(cellRect, m_EmptyContent, data.Asset, typeof(Object), false);
            m_IsDirty = EditorGUI.EndChangeCheck();
        }

        private void DrawFilterTypeItem(Rect cellRect, ResourceCollector data, int rowIndex, bool isSelected, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            bool isValidFolderAsset = AssetDatabase.IsValidFolder(data.AssetPath);
            GUI.enabled = isValidFolderAsset;
            if (!isValidFolderAsset)
            {
                data.FilterRule = nameof(CollectAll);
            }

            int index = Array.IndexOf(ResourceCollectorEditorUtility.FilterRules, data.FilterRule);
            if (index == -1)
            {
                index = 0;
            }

            var tempIndex = EditorGUI.Popup(cellRect, index, ResourceCollectorEditorUtility.FilterRules);
            if (tempIndex != index)
            {
                data.FilterRule = ResourceCollectorEditorUtility.FilterRules[tempIndex];
            }

            GUI.enabled = true;
            m_IsDirty = EditorGUI.EndChangeCheck();
        }

        private void DrawAssetPathItem(Rect cellRect, ResourceCollector data, int rowIndex, bool isSelected, bool isFocused)
        {
            EditorGUI.LabelField(cellRect, data.AssetPath);
        }

        #endregion

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
        private TableColumn<ResourceCollector> CreateColumn(GUIContent content, DrawCellMethod<ResourceCollector> drawCellMethod, float width, float minWidth, float maxWidth, bool canSort = true, bool autoResize = true)
        {
            TableColumn<ResourceCollector> column = new TableColumn<ResourceCollector>();
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
        /// 处理拖拽添加时候的冗余
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool CanAddAssetCollectorRow(string path)
        {
            return string.IsNullOrEmpty(path) || m_SelectAssetBundleCollector.Groups.All(group => group.AssetCollectors.All(assetCollector => assetCollector.AssetPath != path));
        }

        /// <summary>
        /// 添加规则行
        /// </summary>
        /// <param name="path"></param>
        private void AddAssetCollectorRow(string path)
        {
            ResourceCollector assetCollector = new()
            {
                Groups = m_GroupSelectedItem.Data.GroupName,
                Asset = AssetDatabase.LoadAssetAtPath<Object>(path)
            };
            m_AssetCollectorTableView.AddData(assetCollector);
            m_IsDirty = true;
        }

        private int CleanUnknownAssets()
        {
            return m_SelectAssetBundleCollector.Groups.Sum(resourceGroup => resourceGroup.AssetCollectors.RemoveAll(o => !IsValidAssetPath(o.AssetPath)));
        }

        private static bool IsValidAssetPath(string assetPath)
        {
            return AssetDatabase.IsValidFolder(assetPath) ? Directory.Exists(assetPath) : File.Exists(assetPath);
        }

        private void OnDestroy()
        {
            Save();
        }
    }
}