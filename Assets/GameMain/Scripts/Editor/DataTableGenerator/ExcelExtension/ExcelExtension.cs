// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 08:29:45
// 版 本：1.0
// ========================================================
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using UnityEngine;

namespace Dvalmi.Editor
{
    public static class ExcelExtension
    {
        private static readonly Regex NameRegex = new Regex(@"^[A-Z][A-Za-z0-9_]*$");

        /// <summary>
        /// Id 开始列
        /// </summary>
        private const int IdColumn = 1;
        public static void ExcelToTxtConverter(string excelFolder, string txtFolder)
        {
            string[] excelFiles = Directory.GetFiles(excelFolder);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            foreach (var excelFile in excelFiles)
            {
                if (!excelFile.EndsWith(".xlsx") || excelFile.Contains("~$"))
                {
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
                            string fileName = WorksheetsCount > 1 ? sheet.Name : Path.GetFileNameWithoutExtension(excelFile);
                            if (string.IsNullOrWhiteSpace(fileName))
                            {
                                Debug.LogErrorFormat("{0} has not datable name!", fileName);
                                continue;
                            }

                            if (!NameRegex.IsMatch(fileName))
                            {
                                Debug.LogErrorFormat("{0} has wrong datable name!", fileName);
                                continue;
                            }

                            string fileFullPath = $"{txtFolder}/{fileName}.txt";
                            Debug.Log("FullPasth" + fileFullPath);
                            if (File.Exists(fileFullPath))
                            {
                                File.Delete(fileFullPath);
                            }

                            List<string> sContents = new List<string>();
                            StringBuilder sb = new StringBuilder();
                            if (sheet.Dimension.End.Row < 3)
                            {
                                Debug.LogErrorFormat("{0} has wrong row num!", fileFullPath);
                                continue;
                            }

                            int columnCount = sheet.Dimension.End.Column;
                            for (int i = 1; i <= sheet.Dimension.End.Row; i++)
                            {
                                //内容 开始行
                                if (i > 4)
                                {
                                    //索引从0开始但是EPPlus 需要从1开始遍历 使用时需要+1
                                    if (sheet.Cells[i, IdColumn + 1].Value == null)
                                    {
                                        continue;
                                    }
                                }

                                sb.Clear();
                                for (int j = 1; j <= columnCount; j++)
                                {
                                    if (sheet.Cells[i, j] == null)
                                    {
                                        sb.Append("");
                                    }
                                    else
                                    {
                                        sb.Append(sheet.Cells[i, j].Value);
                                    }

                                    if (j != columnCount)
                                    {
                                        sb.Append('\t');
                                    }
                                }

                                sContents.Add(sb.ToString());
                            }

                            File.WriteAllLines(fileFullPath, sContents, Encoding.UTF8);
                            Debug.LogFormat("更新Excel表格：{0}", fileFullPath);
                        }
                    }
                }
            }
        }


        public static void ConvertExcelToTxt(string excelFileName, string outputDirectory)
        {
            // Load the Excel file
            FileInfo fileInfo = new FileInfo(excelFileName);
            ExcelPackage package = new ExcelPackage(fileInfo);

            // Get the output file name
            string outputFileName = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(excelFileName) + ".txt");

            // Create a StreamWriter to write the output to the txt file
            StreamWriter writer = new StreamWriter(outputFileName);

            // Loop through each sheet in the workbook
            foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
            {
                // Get the sheet name
                string sheetName = worksheet.Name;

                // If there is only one sheet, use the file name as the sheet name
                if (package.Workbook.Worksheets.Count == 1)
                {
                    sheetName = Path.GetFileNameWithoutExtension(excelFileName);
                }

                // Get the dimension of the sheet
                int startRow = worksheet.Dimension.Start.Row;
                int endRow = worksheet.Dimension.End.Row;
                int startColumn = worksheet.Dimension.Start.Column;
                int endColumn = worksheet.Dimension.End.Column;

                // Find the last row with content
                for (int i = endRow; i >= startRow; i--)
                {
                    bool hasContent = false;

                    for (int j = startColumn; j <= endColumn; j++)
                    {
                        if (worksheet.Cells[i, j].Value != null)
                        {
                            hasContent = true;
                            break;
                        }
                    }

                    if (hasContent)
                    {
                        endRow = i;
                        break;
                    }
                }

                // Find the last column with content
                for (int j = endColumn; j >= startColumn; j--)
                {
                    bool hasContent = false;

                    for (int i = startRow; i <= endRow; i++)
                    {
                        if (worksheet.Cells[i, j].Value != null)
                        {
                            hasContent = true;
                            break;
                        }
                    }

                    if (hasContent)
                    {
                        endColumn = j;
                        break;
                    }
                }

                // Loop through each row in the sheet
                for (int i = startRow; i <= endRow; i++)
                {
                    bool hasContent = false;

                    // Loop through each cell in the row
                    for (int j = startColumn; j <= endColumn; j++)
                    {
                        object cellValue = worksheet.Cells[i, j].Value;

                        if (cellValue != null)
                        {
                            writer.Write(cellValue.ToString().Trim() + "\t");
                            hasContent = true;
                        }
                    }

                    if (hasContent)
                    {
                        writer.Write("\n");
                    }
                }
            }

            // Close the StreamWriter
            writer.Close();
        }


        public static void ConvertExcelToBinary(string excelFileName, string outputDirectory)
        {
            FileInfo fileInfo = new FileInfo(excelFileName);
            ExcelPackage package = new ExcelPackage(fileInfo);

            string binaryFileName = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(excelFileName) + ".byte");
            BinaryWriter writer = new BinaryWriter(File.Open(binaryFileName, FileMode.Create));

            foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
            {
                for (int i = worksheet.Dimension.Start.Row; i <= worksheet.Dimension.End.Row; i++)
                {
                    for (int j = worksheet.Dimension.Start.Column; j <= worksheet.Dimension.End.Column; j++)
                    {
                        object cellValue = worksheet.Cells[i, j].Value;
                        if (cellValue != null)
                        {
                            string cellValueString = cellValue.ToString();
                            byte[] cellValueBytes = System.Text.Encoding.UTF8.GetBytes(cellValueString);
                            writer.Write(cellValueBytes.Length);
                            writer.Write(cellValueBytes);
                        }
                        else
                        {
                            writer.Write(0);
                        }
                    }
                }
            }

            writer.Close();

            package.Dispose();

            Debug.Log("Excel file exported to binary successfully.");
        }
    }
}
