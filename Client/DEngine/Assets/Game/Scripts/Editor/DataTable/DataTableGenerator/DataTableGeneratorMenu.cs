using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DEngine;
using DEngine.Editor;
using Game.Editor.ResourceTools;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Editor.DataTableTools
{
    public sealed class DataTableGeneratorMenu
    {
        [MenuItem("DataTable/Generate/DataTables", priority = 1)]
        public static void GenerateDataTablesFormExcel()
        {
            DataTableSetting.Instance.RefreshDataTables("*.bytes");
            DataTableProcessor.DataProcessorUtility.RefreshTypes();
            DataTableProcessor.DataProcessorUtility.SetCodeTemplate(DataTableSetting.Instance.CSharpCodeTemplateFileName, Encoding.UTF8);
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
                            if (sheet.Name.StartsWith("#"))
                            {
                                continue;
                            }

                            string dataTableName = workCount > 1 ? sheet.Name : excelName;
                            if (sheet.Dimension.Rows < DataTableSetting.Instance.TypeRow)
                            {
                                throw new Exception($"The format of the data table DataTableName='{dataTableName}' is incorrect. Please check.");
                            }

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

            AssetDatabase.Refresh();
        }

        [MenuItem("DataTable/Editor/DataTables", priority = 1)]
        public static void EditorDataTable()
        {
            OpenFolder.Execute(DataTableSetting.Instance.DataTableExcelsFolder);
        }

        private static void GenerateDataTableEnumFile(DataTableProcessor dataTableProcessor, string dataTableName)
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
                var enumName = dataTableProcessor.GetValue(i, 3);
                if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(enumName))
                {
                    Debug.LogWarning($"Warning:  DataTableName='{dataTableName}' '{enumName}' is not a valid enum name at row {i + 1}.");
                    return;
                }

                stringBuilder.AppendLine($"\t\t// {dataTableProcessor.GetValue(i, 2)}").AppendLine($"\t\t{enumName} = {dataTableProcessor.GetValue(i, 1)},");
            }

            stringBuilder.AppendLine("\t}").AppendLine("}");

            string outputFileName = Utility.Path.GetRegularPath(Path.Combine(DataTableSetting.Instance.DataTableEnumPath, fileName + ".cs"));
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
        }
    }
}