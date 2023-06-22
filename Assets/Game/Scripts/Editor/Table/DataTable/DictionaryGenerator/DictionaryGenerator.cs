using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DEngine;
using DEngine.Editor;
using Game.Editor.ResourceTools;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public sealed class DictionaryGenerator
    {
        [MenuItem("Table/Generate/Localizations", priority = 2)]
        public static void GenerateLocalizationsFormExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<string> dictionaryNames = new List<string>();
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
                                string binaryDataFileName = Utility.Path.GetRegularPath(Path.Combine(DataTableSetting.Instance.LocalizationPath, dictionaryName + ".bytes"));
                                if (!processor.GenerateDataFile(binaryDataFileName) && File.Exists(binaryDataFileName))
                                {
                                    File.Delete(binaryDataFileName);
                                }
                                dictionaryNames.Add(dictionaryName);
                            }
                        }
                    }
                }
                string mainfest = Utility.Path.GetRegularPath(Path.Combine(DataTableSetting.Instance.LocalizationPath, "LocalizationMainfest" + ".bytes"));
                GameMainfestUitlity.CreatMainfest(dictionaryNames.ToArray(), mainfest);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AssetDatabase.Refresh();
                Debug.Log("Dictionary Data File Generated !");
            }
        }

        [MenuItem("Table/Generate/Config", priority = 3)]
        public static void GenerateConfigFormExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (Directory.Exists(DataTableSetting.Instance.ConfigExcelsFolder))
            {
                DirectoryInfo excelFolder = new(DataTableSetting.Instance.ConfigExcelsFolder);
                List<string> dictionaryNames = new List<string>();
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
                                dictionaryNames.Add(dictionaryName);
                            }
                        }
                    }
                }
                string mainfest = Utility.Path.GetRegularPath(Path.Combine(DataTableSetting.Instance.ConfigPath, "ConfigMainfest" + ".bytes"));
                GameMainfestUitlity.CreatMainfest(dictionaryNames.ToArray(), mainfest);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AssetDatabase.Refresh();
                Debug.Log("Config Data File Generated !");
            }
        }

        [MenuItem("Table/Editor/Localization", priority = 2)]
        public static void EditorLocalization()
        {
            OpenFolder.Execute(DataTableSetting.Instance.LocalizationExcelsFolder);
        }

        [MenuItem("Table/Editor/Config", priority = 3)]
        public static void EditorConfig()
        {
            OpenFolder.Execute(DataTableSetting.Instance.ConfigExcelsFolder);
        }
    }
}