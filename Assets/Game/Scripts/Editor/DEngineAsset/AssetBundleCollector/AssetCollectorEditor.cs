using System.Collections.Generic;
using System.IO;
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
        private readonly string m_DefaultConfigurationPath = "Assets/Game/AssetConfiguration/AssetBundleCollector.asset";
        private MenuTreeView<AssetBundleGroupCollector> m_MenuTreeView;
        private MenuTreeViewItem<AssetBundleGroupCollector> m_SelectedItem;
        private AssetCollectorTableView<AssetCollector> m_AssetCollectorTableView;
        private List<TableColumn<AssetCollector>> m_AssetCollectorColumns;
        private ResourceEditorController m_ResourceEditorController;

        private AssetBundleCollector m_Configuration;
        private Rect m_ToolbarRect = new Rect();
        private Rect m_AddGroupRect = new Rect();
        private Rect m_RuleRect = new Rect();
        private GUIStyle m_LineStyle;
        private GUIStyle m_FolderBtnStyle;
        private GUIStyle m_AddMenuStyle;
        private GUIContent m_FolderBtnContent;
        private readonly float m_AddGroupRectHeight = 50f;
        private bool m_IsDirty = false;


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

            // 菜单树视图
            m_MenuTreeView = new MenuTreeView<AssetBundleGroupCollector>(false, true, true);
            {
                m_MenuTreeView.onDrawFoldout = DrawFoldoutCallback;
                m_MenuTreeView.onDrawRowContent = DrawMenuRowContentCallback;
                m_MenuTreeView.onSelectionChanged = SelectionChanged;
            }

            if (m_AssetCollectorColumns == null)
            {
                m_AssetCollectorColumns = GetAssetCollectorColumns();
            }

            m_AssetCollectorTableView = new AssetCollectorTableView<AssetCollector>(null, m_AssetCollectorColumns);
            {
                m_AssetCollectorTableView.OnRightAddRow = OnTreeViewRightAddRowCallback;
            }

            for (int i = 0; i < m_Configuration.Groups.Count; i++)
            {
                AssetBundleGroupCollector resourceGroup = m_Configuration.Groups[i];
                m_MenuTreeView.AddItem(GetDisplayName(resourceGroup), resourceGroup);
            }
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
            m_Configuration = LoadAssetAtPath<AssetBundleCollector>(m_DefaultConfigurationPath);
            if (m_Configuration == null)
            {
                m_Configuration = CreateInstance<AssetBundleCollector>();
                FileInfo fileInfo = new FileInfo(m_DefaultConfigurationPath);
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }
                Save();
            }
        }

        private void Save()
        {
            if (LoadAssetAtPath<AssetBundleCollector>(m_DefaultConfigurationPath) == null)
            {
                AssetDatabase.CreateAsset(m_Configuration, m_DefaultConfigurationPath);
            }
            else
            {
                EditorUtility.SetDirty(m_Configuration);
            }
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
                    EditorGUIUtility.PingObject(m_Configuration);
                }

                if (GUILayout.Button("导出"))
                {
                    AssetCollectorEditorUtility.RefreshResourceCollection(m_Configuration);
                }
                if (GUILayout.Button("Save"))
                {
                    Save();
                    AssetCollectorEditorUtility.RefreshResourceCollection(m_Configuration);
                }
                GUI.backgroundColor = originalColor;
            }
            GUILayout.EndHorizontal();
        }

        protected override void OnGUIMenuTree()
        {
            m_MenuRect = new Rect(0, m_ToolbarRect.height, m_MenuTreeWidth, position.height - m_AddGroupRectHeight - m_ToolbarRect.height);
            m_AddGroupRect = new Rect(0, m_ToolbarRect.height + m_MenuRect.height, m_MenuTreeWidth, m_AddGroupRectHeight);

            m_LineStyle ??= new GUIStyle("EyeDropperHorizontalLine");
            m_FolderBtnStyle ??= new GUIStyle("SettingsIconButton");
            m_FolderBtnContent ??= new GUIContent(SourceFolder.Icon);
            m_AddMenuStyle ??= new GUIStyle(GUI.skin.button);

            m_MenuTreeView.OnGUI(m_MenuRect);

            Rect btnRect = new Rect(m_AddGroupRect.width * 0.15f, m_MenuRect.height + m_AddGroupRect.height, 50f, 20f);
            m_AddMenuStyle.fontSize = 16;
            if (GUI.Button(btnRect, "+", m_AddMenuStyle))
            {
                AssetBundleGroupCollector resourceGroup = new();
                m_Configuration.Groups.Add(resourceGroup);
                m_MenuTreeView.AddItem(GetDisplayName(resourceGroup), resourceGroup);
                SetFocusAndEnsureSelectedItem();
            }
            btnRect.Set((m_AddGroupRect.width * 0.85f) - 50f, btnRect.y, btnRect.width, btnRect.height);
            if (GUI.Button(btnRect, "-", m_AddMenuStyle))
            {
                if (m_SelectedItem != null)
                {
                    m_MenuTreeView.RemoveItem(m_SelectedItem);
                    m_Configuration.Groups.Remove(m_SelectedItem.Data);
                    m_SelectedItem = null;
                }
                SetFocusAndEnsureSelectedItem();
            }

            Color color = new Color(0.6f, 0.6f, 0.6f, 1.333f);
            if (EditorGUIUtility.isProSkin)
            {
                color.r = 0.12f;
                color.g = 0.12f;
                color.b = 0.12f;
            }

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Color orgColor = GUI.color;
            GUI.color = GUI.color * color;
            GUI.DrawTexture(new Rect(m_AddGroupRect.x, m_AddGroupRect.y + 1, 1, m_AddGroupRect.height - 2 * 1), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(m_AddGroupRect.xMax - 1, m_AddGroupRect.y + 1, 1, m_AddGroupRect.height - 2 * 1), EditorGUIUtility.whiteTexture);
            GUI.color = orgColor;
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
            m_ContentRect = new Rect(m_MenuTreeWidth + m_SpaceWidth, m_ToolbarRect.height, position.width - m_MenuTreeWidth - m_SpaceWidth, position.height - m_ToolbarRect.height);

            m_RuleRect.Set(m_ContentRect.x, 120f, m_ContentRect.width, m_ContentRect.height - 90);

            if (m_SelectedItem != null)
            {
                AssetBundleGroupCollector group = m_SelectedItem.Data;
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
                        m_SelectedItem.displayName = GetDisplayName(group);
                    }

                    string description = EditorGUILayout.TextField("分组描述", group.Description);

                    if (group.Description != description)
                    {

                        group.Description = description;
                        m_SelectedItem.displayName = GetDisplayName(group);
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

        /// <summary>
        /// 菜单项选择改变
        /// </summary>
        /// <param name="selectedIds"></param>
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            m_SelectedItem = m_MenuTreeView.GetItemById(selectedIds[0]);

            m_AssetCollectorTableView.SetTableViewData(m_SelectedItem.Data.AssetCollectors, m_AssetCollectorColumns);
        }

        /// <summary>
        /// 绘制行内容回调
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="row"></param>
        /// <param name="item"></param>
        /// <param name="label"></param>
        /// <param name="selected"></param>
        /// <param name="focused"></param>
        /// <param name="useBoldFont"></param>
        /// <param name="isPinging"></param>
        private void DrawMenuRowContentCallback(Rect rect, int row, MenuTreeViewItem<AssetBundleGroupCollector> item, string label, bool selected, bool focused, bool useBoldFont, bool isPinging)
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
            m_MenuTreeView.SetFocusAndEnsureSelectedItem();
        }

        /// <summary>
        /// 获取显示名字
        /// </summary>
        /// <param name="resourceGroup"></param>
        /// <returns></returns>
        private string GetDisplayName(AssetBundleGroupCollector resourceGroup)
        {
            return string.IsNullOrEmpty(resourceGroup.Description) ? resourceGroup.GroupName : string.Format("{0}({1})", resourceGroup.GroupName, resourceGroup.Description);
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
                    data.AssetPath = EditorGUI.TextField(textFildRect, data.AssetPath);
                    Event currentEvent = Event.current;
                    if (currentEvent.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(currentEvent.mousePosition))
                    {
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(data.AssetPath));
                    }
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
        private TableColumn<AssetCollector> CreateColumn(GUIContent content, DrawCellMethod<AssetCollector> drawCellMethod, float width, float minWidth, float maxWidth, bool canSort = false, bool autoResize = true)
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
                Groups = m_SelectedItem.Data.GroupName,
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