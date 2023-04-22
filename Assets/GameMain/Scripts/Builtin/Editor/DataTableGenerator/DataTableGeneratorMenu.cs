//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeminiLion.Editor.DictionaryTools;
using GameFramework;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

namespace GeminiLion.Editor.DataTableTools
{
    public sealed class DataTableGeneratorMenu
    {
        [MenuItem("GeminiLion/Generate DataTables")]
        internal static void GenerateDataTables()
        {
            string[] dataTableExcelFiles = Directory.GetFiles(GeminiLionSetting.Instance.DataTableExcelPath, "*.xlsx");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<string> dataTableinfo = new List<string>();
            foreach (var excelFile in dataTableExcelFiles)
            {
                if (excelFile.Contains("~$"))
                {
                    Debug.LogWarningFormat("Excle {0} 进程占用，请先关闭以完成操作。", excelFile);
                    continue;
                }
                using (FileStream fileStream = new FileStream(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (ExcelPackage excelPackage = new ExcelPackage(fileStream))
                    {
                        int WorksheetsCount = excelPackage.Workbook.Worksheets.Count;
                        for (int s = 0; s < WorksheetsCount; s++)
                        {
                            var sheet = excelPackage.Workbook.Worksheets[s];
                            if (sheet.Dimension.End.Row < 1)
                            {
                                continue;
                            }
                            string dataTableName = WorksheetsCount > 1 ? (Path.GetFileNameWithoutExtension(excelFile) + "_" + sheet.Name) : Path.GetFileNameWithoutExtension(excelFile);
                            if (string.IsNullOrWhiteSpace(dataTableName))
                            {
                                Debug.LogErrorFormat("{0} has not datable name!", dataTableName);
                                continue;
                            }

                            DataTableProcessor dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(sheet);
                            // DataTableProcessor dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(dataTableName);
                            if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                            {
                                Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                                break;
                            }
                            DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
                            DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
                            dataTableinfo.Add(dataTableName);
                        }
                    }
                }
            }
            PreloadUtility.GenerateDataTableInfoFile(dataTableinfo);
            AssetDatabase.Refresh();
        }
    }
}
