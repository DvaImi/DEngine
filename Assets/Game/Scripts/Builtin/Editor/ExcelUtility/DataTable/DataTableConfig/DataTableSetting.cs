using System;
using System.IO;
using System.Linq;
using GameFramework;
using UnityEditor;

namespace Game.Editor
{
    /// <summary>
    /// 数据表编辑器配置相关数据
    /// </summary>
    [GameFilePath("ProjectSettings/DataTableSetting.asset")]
    public class DataTableSetting : ScriptableSingleton<DataTableSetting>
    {
        #region DataTable
        /// <summary>
        /// 数据表存放文件夹路径
        /// </summary>
        public string DataTableFolderPath;

        /// <summary>
        /// Excel存放的文件夹路径
        /// </summary>
        public string ExcelsFolder;

        /// <summary>
        /// 数据表C#实体类生成文件夹路径
        /// </summary>
        public string CSharpCodePath;

        /// <summary>
        /// 数据表C#实体类模板存放路径
        /// </summary>
        public string CSharpCodeTemplateFileName;

        /// <summary>
        /// 数据表扩展类文件夹路径
        /// </summary>
        public string ExtensionDirectoryPath;

        /// <summary>
        /// 数据表命名空间
        /// </summary>
        public string NameSpace;

        /// <summary>
        /// 数据表中使用类型 所在的所有程序集
        /// </summary>
        public string[] AssemblyNames =
        {
            "Assembly-CSharp"
        };

        /// <summary>
        /// 编辑器中使用到的程序集
        /// </summary>
        public string[] EditorAssemblyNames =
        {
            "UnityGameFramework.Editor",
            "Game.Editor",
            "Assembly-CSharp-Editor"
        };

        /// <summary>
        /// 数据表文件路径
        /// </summary>
        [NonSerialized]
        public string[] TxtFilePaths;

        /// <summary>
        /// 数据表文件名
        /// </summary>
        [NonSerialized]
        public string[] DataTableNames;

        /// <summary>
        /// Excel表文件路径
        /// </summary>
        [NonSerialized]
        public string[] ExcelFilePaths;


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

        public void RefreshDataTables(string searchPattern = "*.txt")
        {
            if (Directory.Exists(DataTableFolderPath))
            {
                DirectoryInfo txtFolder = new(DataTableFolderPath);
                TxtFilePaths = txtFolder.GetFiles(searchPattern, SearchOption.TopDirectoryOnly).Select(_ => Utility.Path.GetRegularPath(_.FullName)).ToArray();
                DataTableNames = txtFolder.GetFiles(searchPattern, SearchOption.TopDirectoryOnly).Select(file => Path.GetFileNameWithoutExtension(file.Name)).ToArray();
            }

            if (Directory.Exists(ExcelsFolder))
            {
                DirectoryInfo excelFolder = new(ExcelsFolder);
                ExcelFilePaths = excelFolder.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly).Where(_ => !_.Name.StartsWith("~$")).Select(_ => Utility.Path.GetRegularPath(_.FullName)).ToArray();
            }
        }
        #endregion

        #region Localization

        /// <summary>
        /// 数据表存放文件夹路径
        /// </summary>
        public string LocalizationPath;
        /// <summary>
        /// Excel存放的文件夹路径
        /// </summary>
        public string LocalizationExcelsFolder;

        #endregion

        #region Config
        /// <summary>
        /// 数据表存放文件夹路径
        /// </summary>
        public string ConfigPath;
        /// <summary>
        /// Excel存放的文件夹路径
        /// </summary>
        public string ConfigExcelsFolder;
        #endregion
    }
}