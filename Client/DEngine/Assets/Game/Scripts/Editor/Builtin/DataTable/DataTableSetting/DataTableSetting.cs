namespace Game.Editor.DataTableTools
{
    /// <summary>
    /// 数据表编辑器配置相关数据
    /// </summary>
    [GameFilePath("Assets/Game/Configuration/DataTableSetting.asset")]
    public class DataTableSetting : ScriptableSingleton<DataTableSetting>
    {
        #region DataTable

        /// <summary>
        /// 数据表导出文件夹路径
        /// </summary>
        public string OutputDataTableFolder = "Assets/../Excels/Output/DataTable";

        /// <summary>
        /// Excel存放的文件夹路径
        /// </summary>
        public string DataTableExcelsFolder = "Assets/../Excels/DataTable";

        /// <summary>
        /// 数据表C#实体类生成文件夹路径
        /// </summary>
        public string CSharpCodePath = "Assets/Game/Scripts/Runtime/Update/Generate/DataTable/DRCode";

        /// <summary>
        /// 数据表C#实体类模板存放路径
        /// </summary>
        public string CSharpCodeTemplateFileName = "Assets/Game/Scripts/Editor/Builtin/DataTable/TableCodeTemplate/DataTableCodeTemplate.txt";

        /// <summary>
        /// 数据表扩展类文件夹路径
        /// </summary>
        public string ExtensionDirectoryPath = "Assets/Game/Scripts/Runtime/Update/Generate/DataTable/Extensions";

        /// <summary>
        /// 生成枚举的路径
        /// </summary>
        public string DataTableEnumPath = "Assets/Game/Scripts/Runtime/Update/Generate/DataTable/DataTableEnum";

        /// <summary>
        /// 数据表命名空间
        /// </summary>
        public string NameSpace = "Game.Update";

        /// <summary>
        /// 数据表中使用类型 所在的所有程序集
        /// </summary>
        public string[] AssemblyNames =
        {
            "Assembly-CSharp",
            "DEngine",
            "DEngine.Runtime",
        };

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
        
        /// <summary>
        /// 生成枚举列
        /// </summary>
        public int EnumNameColumn = 3;

        #endregion

        #region Localization

        /// <summary>
        /// 数据表存放文件夹路径
        /// </summary>
        public string LocalizationPath = "Assets/Game/Bundles/Localization";

        /// <summary>
        /// Excel存放的文件夹路径
        /// </summary>
        public string LocalizationExcelsFolder = "Assets/../Excels/Localization";

        #endregion
    }
}