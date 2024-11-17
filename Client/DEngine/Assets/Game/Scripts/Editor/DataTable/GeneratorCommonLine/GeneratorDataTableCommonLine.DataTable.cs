using System;
using System.IO;
using System.Linq;
using System.Text;
using DEngine;
using Game.Editor.Toolbar;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public static partial class GeneratorDataTableCommonLine
    {
        [MenuItem("DataTable/Generate/DataTables", priority = 1)]
        [EditorToolbarMenu("Generate DataTable", ToolBarMenuAlign.Left, 3)]
        public static void GenerateDataTablesFormExcel()
        {
            string[] excelFilePaths;

            if (Directory.Exists(DataTableSetting.Instance.DataTableExcelsFolder))
            {
                DirectoryInfo excelFolder = new(DataTableSetting.Instance.DataTableExcelsFolder);
                excelFilePaths = excelFolder.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly).Where(info => !info.Name.StartsWith("#") && !info.Name.EndsWith("~")).Select(info => Utility.Path.GetRegularPath(info.FullName)).ToArray();
            }
            else
            {
                Debug.LogWarning("Excel is not exist");
                return;
            }

            if (excelFilePaths.Length <= 0)
            {
                return;
            }

            DataTableProcessor.DataProcessorUtility.RefreshTypes();
            DataTableProcessor.DataProcessorUtility.SetCodeTemplate(DataTableSetting.Instance.CSharpCodeTemplateFileName, Encoding.UTF8);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExtensionsGenerate.GenerateExtensionByAnalysis(excelFilePaths, 2);
            foreach (var excelFile in excelFilePaths)
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
                            DataTableGenerator.GenerateDataEnumFile(dataTableProcessor, dataTableName);
                        }
                    }
                }
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("DataTable/Editor/DataTables", priority = 1)]
        public static void EditorDataTable()
        {
            EditorUtility.RevealInFinder(DataTableSetting.Instance.DataTableExcelsFolder);
        }
    }
}