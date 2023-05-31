using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public class DataTableSettingEditorWindows : OdinEditorWindow
    {

        /// <summary>
        /// 数据表存放文件夹路径
        /// </summary>
        [FolderPath(RequireExistingPath = true)]
        public string DataTableFolderPath;

        /// <summary>
        /// Excel存放的文件夹路径
        /// </summary>
        [FolderPath(RequireExistingPath = true)]
        public string ExcelsFolder;

        /// <summary>
        /// 数据表C#实体类生成文件夹路径
        /// </summary>
        [FolderPath(RequireExistingPath = true)]
        public string CSharpCodePath;

        /// <summary>
        /// 数据表C#实体类模板存放路径
        /// </summary>
        [Sirenix.OdinInspector.FilePath(RequireExistingPath = true)]
        public string CSharpCodeTemplateFileName;

        /// <summary>
        /// 数据表扩展类文件夹路径
        /// </summary>
        [FolderPath(RequireExistingPath = true)]
        public string ExtensionDirectoryPath;

        /// <summary>
        /// 数据表命名空间
        /// </summary>
        public string NameSpace;

        /// <summary>
        /// 数据表中使用类型 所在的所有程序集
        /// </summary>
        public string[] AssemblyNames;

        /// <summary>
        /// 编辑器中使用到的程序集
        /// </summary>
        public string[] EditorAssemblyNames;

        //所有行列 是逻辑行列从0 开始 但是eppplus 需要从1开始遍历 使用时需要+1
        /// <summary>
        /// 字段名所在行
        /// </summary>
        public int NameRow = 1;
        /// <summary>
        /// 类型名所在行
        /// </summary>
        public int TypeRow = 2;
        /// <summary>
        /// 注释所在行
        /// </summary>
        public int CommentRow = 3;
        /// <summary>
        /// 内容开始行
        /// </summary>
        public int ContentStartRow = 4;
        /// <summary>
        /// id所在列
        /// </summary>
        public int IdColumn = 1;

        [MenuItem("DataTable/Setting", priority = 10)]
        private static void OpenWindow()
        {
            var window = GetWindow<DataTableSettingEditorWindows>("Game Setting");
            window.minSize = new Vector2(800, 600);
           
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ReadSetting();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reload"))
            {
                ReadSetting();
            }
            if (GUILayout.Button("Save"))
            {
                SaveSetting();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ReadSetting()
        {
            DataTableFolderPath = DataTableSetting.Instance.DataTableFolderPath;
            ExcelsFolder = DataTableSetting.Instance.ExcelsFolder;
            CSharpCodePath = DataTableSetting.Instance.CSharpCodePath;
            CSharpCodeTemplateFileName = DataTableSetting.Instance.CSharpCodeTemplateFileName;
            ExtensionDirectoryPath = DataTableSetting.Instance.ExtensionDirectoryPath;
            NameSpace = DataTableSetting.Instance.NameSpace;
            AssemblyNames = DataTableSetting.Instance.AssemblyNames;
            EditorAssemblyNames = DataTableSetting.Instance.EditorAssemblyNames;
            NameRow = DataTableSetting.Instance.NameRow;
            TypeRow = DataTableSetting.Instance.TypeRow;
            CommentRow = DataTableSetting.Instance.CommentRow;
            ContentStartRow = DataTableSetting.Instance.ContentStartRow;
            IdColumn = DataTableSetting.Instance.IdColumn;
        }

        private void SaveSetting()
        {
            DataTableSetting.Instance.DataTableFolderPath = DataTableFolderPath;
            DataTableSetting.Instance.ExcelsFolder = ExcelsFolder;
            DataTableSetting.Instance.CSharpCodePath = CSharpCodePath;
            DataTableSetting.Instance.CSharpCodeTemplateFileName = CSharpCodeTemplateFileName;
            DataTableSetting.Instance.ExtensionDirectoryPath = ExtensionDirectoryPath;
            DataTableSetting.Instance.NameSpace = NameSpace;
            DataTableSetting.Instance.AssemblyNames = AssemblyNames;
            DataTableSetting.Instance.EditorAssemblyNames = EditorAssemblyNames;
            DataTableSetting.Instance.NameRow = NameRow;
            DataTableSetting.Instance.TypeRow = TypeRow;
            DataTableSetting.Instance.CommentRow = CommentRow;
            DataTableSetting.Instance.ContentStartRow = ContentStartRow;
            DataTableSetting.Instance.IdColumn = IdColumn;
            DataTableSetting.Save();
        }
    }
}
