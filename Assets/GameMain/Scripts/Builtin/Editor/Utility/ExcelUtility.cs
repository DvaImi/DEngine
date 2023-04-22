// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-19 21:29:17
// 版 本：1.0
// ========================================================
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameFramework;
using OfficeOpenXml;
using UnityEngine;

namespace GeminiLion.Editor
{
    public static class ExcelUtility
    {
        private static readonly string TextExtension = ".txt";
        private static readonly string ExcelExtension = ".xlsx";
        private static readonly string BinaryExtension = ".bytes";

        public static void ConvertExcelToTxt(string excelDirectory, string outputDirectory, out List<string> collection)
        {
            collection = new List<string>();
            if (string.IsNullOrEmpty(excelDirectory) || string.IsNullOrEmpty(outputDirectory))
            {
                return;
            }

            IOUtility.CreateDirectoryIfNotExists(outputDirectory);

            List<FileInfo> fileInfos = IOUtility.GetFilesWithExtension(excelDirectory, ExcelExtension);

            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Name.StartsWith("~$"))
                {
                    continue;
                }
                ExcelPackage package = new ExcelPackage(fileInfo);
                foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    string txtFileName = package.Workbook.Worksheets.Count > 1 ? (Path.GetFileNameWithoutExtension(fileInfo.Name) + "_" + worksheet.Name) : Path.GetFileNameWithoutExtension(fileInfo.Name);
                    txtFileName = Path.Combine(outputDirectory, txtFileName + TextExtension);
                    StringBuilder text = new StringBuilder();

                    for (int row = 1; row <= worksheet.Dimension.Rows; row++)
                    {
                        if (row != 1)
                        {
                            text.Append("\r\n");
                        }
                        for (int column = 1; column <= worksheet.Dimension.Columns; column++)
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
                    IOUtility.SaveFileSafe(txtFileName, text.ToString());
                }
                package.Dispose();
                Debug.LogFormat("Excel {0} file converted to txt successfully.", fileInfo.Name);
                collection.Add(fileInfo.Name.Replace(ExcelExtension, null));
            }
        }

        public static void ConvertExcelToBinary(string excelDirectory, string outputDirectory, out List<string> collection)
        {
            collection = new List<string>();
            if (string.IsNullOrEmpty(excelDirectory) || string.IsNullOrEmpty(outputDirectory))
            {
                return;
            }
            IOUtility.CreateDirectoryIfNotExists(outputDirectory);
            List<FileInfo> fileInfos = IOUtility.GetFilesWithExtension(excelDirectory, ExcelExtension);

            foreach (var fileInfo in fileInfos)
            {
                ExcelPackage package = new ExcelPackage(fileInfo);
                foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    string binaryFileName = package.Workbook.Worksheets.Count > 1 ? (Path.GetFileNameWithoutExtension(fileInfo.Name) + "_" + worksheet.Name) : Path.GetFileNameWithoutExtension(fileInfo.Name);
                    binaryFileName = Path.Combine(outputDirectory, binaryFileName + BinaryExtension);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        for (int row = 1; row <= worksheet.Dimension.Rows; row++)
                        {
                            for (int column = 1; column <= worksheet.Dimension.Columns; column++)
                            {
                                var cellValue = worksheet.Cells[row, column].Value;
                                if (cellValue != null)
                                {
                                    var bytes = Utility.Converter.GetBytes(cellValue.ToString());
                                    ms.Write(bytes, 0, bytes.Length);
                                }
                                ms.WriteByte((byte)'\t');
                            }
                            ms.WriteByte((byte)'\n');
                        }
                        byte[] buffer = ms.ToArray();
                        IOUtility.SaveFileSafe(binaryFileName, buffer);
                    }
                }
                package.Dispose();
                Debug.LogFormat("Excel {0} file converted to binary successfully.", fileInfo.Name);
                collection.Add(fileInfo.Name.Replace(ExcelExtension, null));
            }
        }
    }
}
