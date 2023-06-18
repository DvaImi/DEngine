﻿using System.Collections.Generic;
using System.IO;
using DEngine.Editor;
using Game.Editor.ResourceTools;
using OfficeOpenXml;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Game.Editor.DataTableTools
{
    public sealed class DataTableGeneratorMenu
    {
        // [MenuItem("DataTable/Generate DataTables/From Txt", priority = 2)]
        public static void GenerateDataTablesFromTxt()
        {
            DataTableSetting.Instance.RefreshDataTables();

            ExtensionsGenerate.GenerateExtensionByAnalysis(ExtensionsGenerate.DataTableType.Txt,
                DataTableSetting.Instance.TxtFilePaths, 2);
            foreach (var dataTableName in DataTableSetting.Instance.DataTableNames)
            {
                var dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(dataTableName);
                if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                {
                    Debug.LogError(DEngine.Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                    break;
                }

                DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
                DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
            }

            AssetDatabase.Refresh();
        }

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
                            string dataTableName = workCount > 1 ? excelName + "_" + sheet.Name : excelName;
                            var dataTableProcessor = DataTableGenerator.CreateExcelDataTableProcessor(sheet);
                            if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                            {
                                Debug.LogError(DEngine.Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                                break;
                            }

                            DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
                            DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
                            dataTableNames.Add(dataTableName);
                        }
                    }
                }
            }
            string mainfest = DEngine.Utility.Path.GetRegularPath(Path.Combine(DataTableSetting.Instance.DataTableFolderPath, "DataTableMainfest" + ".bytes"));
            GameMainfestUitlity.CreatMainfest(dataTableNames.ToArray(), mainfest);
            AssetDatabase.Refresh();
        }

        [MenuItem("DataTable/Editor/DataTables", priority = 1)]
        public static void EditorDataTable()
        {
            OpenFolder.Execute(DataTableSetting.Instance.DataTableExcelsFolder);
        }
        //[MenuItem("DataTable/Generate DataTables/Excel To Txt", priority = 32)]
        public static void ExcelToTxt()
        {
            DataTableSetting.Instance.RefreshDataTables();
            if (!Directory.Exists(DataTableSetting.Instance.DataTableExcelsFolder))
            {
                Debug.LogError($"{DataTableSetting.Instance.DataTableExcelsFolder} is not exist!");
                return;
            }

            ExcelExtension.ExcelToTxt(DataTableSetting.Instance.DataTableExcelsFolder, DataTableSetting.Instance.DataTableFolderPath);
            AssetDatabase.Refresh();
        }
    }
}