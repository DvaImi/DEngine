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
        [MenuItem("DataTable/Generate/Localizations", priority = 2)]
        public static void GenerateLocalizationsFormExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (Directory.Exists(DataTableSetting.Instance.LocalizationExcelsFolder))
            {
                DirectoryInfo excelFolder    = new(DataTableSetting.Instance.LocalizationExcelsFolder);
                string[]      excelFilePaths = excelFolder.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly).Where(info => !info.Name.Contains("~") && !info.Name.Contains("#")).Select(o => Utility.Path.GetRegularPath(o.FullName)).ToArray();
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

                                string              dictionaryName     = workCount > 1 ? sheet.Name : excelName;
                                DictionaryProcessor processor          = new DictionaryProcessor(sheet, Encoding.UTF8, 0, 1);
                                string              binaryDataFileName = Utility.Path.GetRegularPath(Path.Combine(DataTableSetting.Instance.LocalizationPath, dictionaryName, dictionaryName + ".bytes"));
                                FileInfo            fileInfo           = new(binaryDataFileName);
                                if (fileInfo.Directory is { Exists: false })
                                {
                                    fileInfo.Directory.Create();
                                }

                                if (!processor.GenerateDataFile(binaryDataFileName) && File.Exists(binaryDataFileName))
                                {
                                    File.Delete(binaryDataFileName);
                                }
                            }
                        }
                    }
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AssetDatabase.Refresh();
                Debug.Log("Dictionary Data File Generated !");
            }
        }

        [MenuItem("DataTable/Editor/Localization", priority = 2)]
        public static void EditorLocalization()
        {
            EditorUtility.RevealInFinder(DataTableSetting.Instance.LocalizationExcelsFolder);
        }
    }
}