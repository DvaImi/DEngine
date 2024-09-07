using System.Collections.Generic;
using System.IO;
using System.Text;
using DEngine;
using DEngine.Editor;
using Game.Editor.ResourceTools;
using OfficeOpenXml;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Game.Editor.DataTableTools
{
    public sealed class DataTableGeneratorMenu
    {

        [MenuItem("DataTable/Generate/DataTables", priority = 1)]
        public static void GenerateDataTablesFormExcel()
        {
            DataTableSetting.Instance.RefreshDataTables("*.bytes");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<string> dataTableNames = new List<string>();
            ExtensionsGenerate.GenerateExtensionByAnalysis(ExtensionsGenerate.DataTableType.Excel, DataTableSetting.Instance.ExcelFilePaths, 2);
            foreach (var excelFile in DataTableSetting.Instance.ExcelFilePaths)
            {
                string excelName = Path.GetFileNameWithoutExtension(excelFile);
                using (FileStream fileStream = new FileStream(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (ExcelPackage excelPackage = new ExcelPackage(fileStream))
                    {
                        int workCount = excelPackage.Workbook.Worksheets.Count;
                        for (int i = 0; i < workCount; i++)
                        {
                            ExcelWorksheet sheet = excelPackage.Workbook.Worksheets[i];
                            string dataTableName = workCount > 1 ? sheet.Name : excelName;
                            var dataTableProcessor = DataTableGenerator.CreateExcelDataTableProcessor(sheet);
                            if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                            {
                                Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                                break;
                            }

                            DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
                            DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
                            dataTableNames.Add(dataTableName);
                            if (DataTableSetting.Instance.GenerateDataTableEnum)
                            {
                                GenerateDataTableEnumFile(dataTableProcessor, dataTableName);
                            }
                        }
                    }
                }
            }
            string version = Utility.Path.GetRegularPath(Path.Combine(DataTableSetting.Instance.DataTableFolderPath, Constant.AssetVersion.DataTableVersion + ".bytes"));
            GameAssetVersionUitlity.CreateAssetVersion(dataTableNames.ToArray(), version);
            AssetDatabase.Refresh();
        }

        [MenuItem("DataTable/Editor/DataTables", priority = 1)]
        public static void EditorDataTable()
        {
            OpenFolder.Execute(DataTableSetting.Instance.DataTableExcelsFolder);
        }

        public static void GenerateDataTableEnumFile(DataTableProcessor dataTableProcessor, string dataTableName)
        {
            string fileName = $"{dataTableName}Id";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("//this file is generate by tools,do not alter it...")
                .AppendLine($"namespace {DataTableSetting.Instance.NameSpace}")
                .AppendLine("{")
                .AppendLine($"\t// {dataTableProcessor.GetValue(3, 1)}")
                .AppendLine($"\tpublic enum {fileName} : byte")
                .AppendLine("\t{");

            for (int i = 4; i < dataTableProcessor.RawRowCount; i++)
            {
                stringBuilder.AppendLine($"\t\t// {dataTableProcessor.GetValue(i, 2)}").AppendLine($"\t\t{dataTableProcessor.GetValue(i, 3)} = {dataTableProcessor.GetValue(i, 1)},");
            }
            stringBuilder.AppendLine("\t}").AppendLine("}");

            string outputFileName = Utility.Path.GetRegularPath(Path.Combine(DataTableSetting.Instance.ExtensionDirectoryPath, "TableEnum", fileName + ".cs"));
            FileInfo fileInfo = new FileInfo(outputFileName);
            if (!fileInfo.Directory.Exists)
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
        }
    }
}