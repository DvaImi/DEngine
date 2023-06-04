using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameFramework;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public sealed class DictionaryGenerator
    {
        [MenuItem("DataTable/Generate Localizations", priority = 3)]
        public static void GenerateLocalizationsFormExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<string> dictionary = new List<string>();
            if (Directory.Exists(DataTableSetting.Instance.LocalizationExcelsFolder))
            {
                DirectoryInfo excelFolder = new(DataTableSetting.Instance.LocalizationExcelsFolder);
                string[] ExcelFilePaths = excelFolder.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly).Where(_ => !_.Name.StartsWith("~$")).Select(_ => Utility.Path.GetRegularPath(_.FullName)).ToArray();
                foreach (var excelFile in ExcelFilePaths)
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
                                string dictionaryName = workCount > 1 ? excelName + "_" + sheet.Name : excelName;
                                DictionaryProcessor processor = new DictionaryProcessor(sheet, Encoding.UTF8, 0, 1);
                                string folder = Path.Combine(DataTableSetting.Instance.LocalizationPath, dictionaryName);
                                IOUtility.CreateDirectoryIfNotExists(folder);
                                string binaryDataFileName = Utility.Path.GetRegularPath(Path.Combine(folder, dictionaryName + ".bytes"));
                                if (!processor.GenerateDataFile(binaryDataFileName) && File.Exists(binaryDataFileName))
                                {
                                    File.Delete(binaryDataFileName);
                                }
                            }
                        }
                    }
                    dictionary.Add(excelName);
                }
                PreloadUtility.GenerateDictionaryInfoFile(dictionary);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AssetDatabase.Refresh();
                Debug.Log("Dictionary Data File Generated !");
            }
        }

        [MenuItem("DataTable/Generate Config", priority = 4)]
        public static void GenerateConfigFormExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<string> config = new List<string>();
            if (Directory.Exists(DataTableSetting.Instance.ConfigExcelsFolder))
            {
                DirectoryInfo excelFolder = new(DataTableSetting.Instance.ConfigExcelsFolder);
                string[] ExcelFilePaths = excelFolder.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly).Where(_ => !_.Name.StartsWith("~$")).Select(_ => Utility.Path.GetRegularPath(_.FullName)).ToArray();
                foreach (var excelFile in ExcelFilePaths)
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
                                string dictionaryName = workCount > 1 ? excelName + "_" + sheet.Name : excelName;
                                DictionaryProcessor processor = new DictionaryProcessor(sheet, Encoding.UTF8, 0, 1);
                                string binaryDataFileName = Utility.Path.GetRegularPath(Path.Combine(DataTableSetting.Instance.ConfigPath, dictionaryName + ".bytes"));
                                if (!processor.GenerateDataFile(binaryDataFileName) && File.Exists(binaryDataFileName))
                                {
                                    File.Delete(binaryDataFileName);
                                }
                            }
                        }
                    }
                    config.Add(excelName);
                }
                PreloadUtility.GenerateConfigInfoFile(config);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AssetDatabase.Refresh();
                Debug.Log("Config Data File Generated !");
            }
        }
    }
}