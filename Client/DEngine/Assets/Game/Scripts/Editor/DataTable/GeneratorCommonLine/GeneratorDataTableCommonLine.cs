namespace Game.Editor.DataTableTools
{
    public static partial class GeneratorDataTableCommonLine
    {
        /// <summary>
        /// 用于执行一键打包的操作
        /// </summary>
        public static void GenerateAll()
        {
            GenerateLuban();
            GenerateDataTablesFormExcel();
            GenerateLocalizationsFormExcel();
        }
    }
}