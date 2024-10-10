using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DEngine;
using Game.Editor.DataTableTools;
using OfficeOpenXml;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        public static void GeneratePackingParameterFormExcel()
        {
            string excelFile = GameSetting.Instance.MultiChannelExcelPath;
            if (!File.Exists(excelFile))
            {
                Debug.LogError($"{excelFile} 文件不存在.");
                return;
            }

            DataTableProcessor.DataProcessorUtility.RefreshTypes();
            DataTableProcessor.DataProcessorUtility.SetCodeTemplate(DataTableSetting.Instance.CSharpCodeTemplateFileName, Encoding.UTF8);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExtensionsGenerate.GenerateExtensionByAnalysis(new[] { excelFile }, 2);
            string excelName = Path.GetFileNameWithoutExtension(excelFile);
            using (FileStream fileStream = new FileStream(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage excelPackage = new ExcelPackage(fileStream))
                {
                    ExcelWorksheet sheet = excelPackage.Workbook.Worksheets[0];
                    string dataTableName = excelName;
                    if (sheet.Dimension.Rows < DataTableSetting.Instance.TypeRow)
                    {
                        throw new Exception($"The format of the data table DataTableName='{dataTableName}' is incorrect. Please check.");
                    }

                    var dataTableProcessor = DataTableGenerator.CreateExcelDataTableProcessor(sheet);
                    if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                    {
                        Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                    }

                    DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName, Path.Combine(GameSetting.Instance.MultiChannelDataPath, dataTableName + ".bytes"));
                    DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName, Path.Combine(GameSetting.Instance.MultiChannelCodePath, "DR" + dataTableName + ".cs"), "Game.Editor");
                }
            }

            AssetDatabase.Refresh();
        }

        public static List<DRPackingParameter> GetPackingParameterset()
        {
            var binaryDataFileName = Utility.Path.GetRegularPath(Path.Combine(GameSetting.Instance.MultiChannelDataPath, "PackingParameter.bytes"));
            if (!File.Exists(binaryDataFileName))
            {
                Debug.LogError($"{binaryDataFileName} 文件不存在.");
                return null;
            }

            List<DRPackingParameter> result = new List<DRPackingParameter>();
            byte[] dataTableBytes = File.ReadAllBytes(binaryDataFileName);

            using (MemoryStream memoryStream = new MemoryStream(dataTableBytes, 0, dataTableBytes.Length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                    {
                        int dataRowBytesLength = binaryReader.Read7BitEncodedInt32();
                        DRPackingParameter drPackingParameter = new DRPackingParameter();
                        if (drPackingParameter.ParseDataRow(dataTableBytes, (int)binaryReader.BaseStream.Position, dataRowBytesLength, null))
                        {
                            result.Add(drPackingParameter);
                        }

                        binaryReader.BaseStream.Position += dataRowBytesLength;
                    }
                }
            }

            return result;
        }

        public static void ApplyPackingParameter(DRPackingParameter parameter)
        {
        }
    }
}