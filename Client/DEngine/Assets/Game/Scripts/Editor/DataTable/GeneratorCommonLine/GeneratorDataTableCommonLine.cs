using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DEngine;
using DEngine.Editor;
using Game.Editor.FileSystem;
using Game.Editor.Toolbar;
using Newtonsoft.Json;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public static class GeneratorDataTableCommonLine
    {
        public static void GenerateAll()
        {
            GenerateLuban();
            GenerateDataTablesFormExcel();
            GenerateLocalizationsFormExcel();
        }

        [MenuItem("DataTable/Generate/DataTables", priority = 1)]
        [EditorToolbarMenu("Generate DataTable", 0, 3)]
        public static void GenerateDataTablesFormExcel()
        {
            string[] excelFilePaths;

            if (Directory.Exists(DataTableSetting.Instance.DataTableExcelsFolder))
            {
                DirectoryInfo excelFolder = new(DataTableSetting.Instance.DataTableExcelsFolder);
                excelFilePaths = excelFolder.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly).Where(info => !info.Name.Contains("~")).Select(info => Utility.Path.GetRegularPath(info.FullName)).ToArray();
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
                string md5 = GameUtility.IO.GetFileMD5(excelFile);

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

        [MenuItem("DataTable/Generate/Localizations", priority = 2)]
        public static void GenerateLocalizationsFormExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (Directory.Exists(DataTableSetting.Instance.LocalizationExcelsFolder))
            {
                DirectoryInfo excelFolder = new(DataTableSetting.Instance.LocalizationExcelsFolder);
                string[] excelFilePaths = excelFolder.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly).Where(info => !info.Name.Contains("~") && !info.Name.Contains("#")).Select(o => Utility.Path.GetRegularPath(o.FullName)).ToArray();
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

                                string dictionaryName = workCount > 1 ? sheet.Name : excelName;
                                DictionaryProcessor processor = new DictionaryProcessor(sheet, Encoding.UTF8, 0, 1);
                                string binaryDataFileName = Utility.Path.GetRegularPath(Path.Combine(DataTableSetting.Instance.LocalizationPath, dictionaryName, dictionaryName + ".bytes"));
                                FileInfo fileInfo = new(binaryDataFileName);
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
            OpenFolder.Execute(DataTableSetting.Instance.LocalizationExcelsFolder);
        }

        [MenuItem("DataTable/Generate/Luban", priority = 3)]
        [EditorToolbarMenu("Generate Luban", 0, 4)]
        public static void GenerateLuban()
        {
#if UNITY_EDITOR_WIN
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Share/Luban/gen_bin_client_lazyload.bat"));
#elif UNITY_EDITOR_OSX
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../../../Share/Luban/gen_bin_client_lazyload.sh"));
#endif
        }

        [MenuItem("DataTable/Editor/Luban", priority = 3)]
        public static void EditorLuban()
        {
            OpenFolder.Execute(Path.Combine(Application.dataPath, "../../../Share/Luban/Client/Datas"));
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
                if (!CodeGenerator.IsValidLanguageIndependentIdentifier(enumName))
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