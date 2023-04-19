// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-19 21:29:17
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameFramework;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace Dvalmi.Editor
{
    public static class ExcelUtility
    {
        private static readonly string TextExtension = ".txt";
        private static readonly string ExcelExtension = ".xlsx";

        public static void BatchExcelToText(string excelDirectory, string textDirectory)
        {
            if (string.IsNullOrEmpty(excelDirectory) || string.IsNullOrEmpty(textDirectory))
            {
                return;
            }

            IOUtility.CreateDirectoryIfNotExists(textDirectory);

            List<string> names = new List<string>();
            List<FileInfo> fileInfos = IOUtility.GetFilesWithExtension(excelDirectory, ExcelExtension);
            for (int i = 0; i < fileInfos.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Excel to text", Utility.Text.Format("Converting {0}", fileInfos[i].Name), (float)i / fileInfos.Count);
                FileInfo fileInfo = fileInfos[i];
                if (fileInfo.Name.StartsWith("~$"))
                {
                    continue;
                }
                string excelFile = Utility.Path.GetRegularPath(fileInfo.FullName);
                string textFile = Utility.Path.GetRegularPath(Path.Combine(textDirectory, fileInfo.Name.Replace(ExcelExtension, TextExtension)));
                ExcelToText(excelFile, textFile);
                names.Add(fileInfo.Name.Split('.')[0]);
            }

            EditorUtility.ClearProgressBar();
        }

        public static void ExcelToText(string excelFileFullName, string textFileFullName)
        {
            if (!File.Exists(excelFileFullName))
            {
                Debug.LogError("File Not Exits : " + excelFileFullName);
                return;
            }

            if (File.Exists(textFileFullName))
            {
                File.Delete(textFileFullName);
            }

            try
            {
                var fileInfo = new FileInfo(excelFileFullName);
                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    int columnCount = worksheet.Dimension.Columns;
                    StringBuilder text = new StringBuilder();

                    for (int row = 1; row <= rowCount; row++)
                    {
                        if (row != 1)
                        {
                            text.Append("\r\n");
                        }
                        for (int column = 1; column <= columnCount; column++)
                        {
                            if (column != 1)
                            {
                                text.Append("\t");
                            }
                            var cellValue = worksheet.Cells[row, column].Value;
                            if (cellValue != null)
                            {
                                text.Append(cellValue.ToString());
                            }
                        }
                    }
                    IOUtility.SaveFileSafe(textFileFullName, text.ToString());
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.ToString());
            }
        }
    }
}
