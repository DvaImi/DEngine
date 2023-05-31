using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public class DataTableSettingEditorWindows : OdinEditorWindow
    {

        /// <summary>
        /// ���ݱ����ļ���·��
        /// </summary>
        [FolderPath(RequireExistingPath = true)]
        public string DataTableFolderPath;

        /// <summary>
        /// Excel��ŵ��ļ���·��
        /// </summary>
        [FolderPath(RequireExistingPath = true)]
        public string ExcelsFolder;

        /// <summary>
        /// ���ݱ�C#ʵ���������ļ���·��
        /// </summary>
        [FolderPath(RequireExistingPath = true)]
        public string CSharpCodePath;

        /// <summary>
        /// ���ݱ�C#ʵ����ģ����·��
        /// </summary>
        [Sirenix.OdinInspector.FilePath(RequireExistingPath = true)]
        public string CSharpCodeTemplateFileName;

        /// <summary>
        /// ���ݱ���չ���ļ���·��
        /// </summary>
        [FolderPath(RequireExistingPath = true)]
        public string ExtensionDirectoryPath;

        /// <summary>
        /// ���ݱ������ռ�
        /// </summary>
        public string NameSpace;

        /// <summary>
        /// ���ݱ���ʹ������ ���ڵ����г���
        /// </summary>
        public string[] AssemblyNames;

        /// <summary>
        /// �༭����ʹ�õ��ĳ���
        /// </summary>
        public string[] EditorAssemblyNames;

        //�������� ���߼����д�0 ��ʼ ����eppplus ��Ҫ��1��ʼ���� ʹ��ʱ��Ҫ+1
        /// <summary>
        /// �ֶ���������
        /// </summary>
        public int NameRow = 1;
        /// <summary>
        /// ������������
        /// </summary>
        public int TypeRow = 2;
        /// <summary>
        /// ע��������
        /// </summary>
        public int CommentRow = 3;
        /// <summary>
        /// ���ݿ�ʼ��
        /// </summary>
        public int ContentStartRow = 4;
        /// <summary>
        /// id������
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
