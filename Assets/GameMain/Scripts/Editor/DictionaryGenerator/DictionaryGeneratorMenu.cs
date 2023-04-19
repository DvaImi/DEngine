// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-19 23:18:18
// 版 本：1.0
// ========================================================
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

namespace Dvalmi.Editor.DictionaryTools
{
    public class DictionaryGeneratorMenu : MonoBehaviour
    {
        [MenuItem("Dvalmi/Generate Dictionaries")]
        internal static void GenerateDictionaries()
        {
            string[] dictionaryExcleFiles = Directory.GetFiles(DvalmiSetting.Instance.DictionaryExclePath, "*.xlsx");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<string> dictionary = new List<string>();
            foreach (var excelFile in dictionaryExcleFiles)
            {
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
                            string dictionaryName = WorksheetsCount > 1 ? (Path.GetFileNameWithoutExtension(excelFile) + "_" + sheet.Name) : Path.GetFileNameWithoutExtension(excelFile);
                            if (string.IsNullOrWhiteSpace(dictionaryName))
                            {
                                Debug.LogErrorFormat("{0} has not dictionary name!", dictionaryName);
                                continue;
                            }

                            DictionaryProcessor processor = DictionaryGenerator.CreatDictionaryProcessor(sheet);
                            DictionaryGenerator.GenerateDictionaryDataFile(processor, dictionaryName);
                            dictionary.Add(dictionaryName);
                        }
                    }
                }

            }
            PreloadUtility.GenerateDictionaryInfoFile(dictionary);
            AssetDatabase.Refresh();
        }
    }
}
