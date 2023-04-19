// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-19 23:14:36
// 版 本：1.0
// ========================================================
using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml;
using System.Text;
using UnityEngine;
using GameFramework;
using System.Text.RegularExpressions;
using System.IO;

namespace Dvalmi.Editor.DictionaryTools
{
    public sealed class DictionaryGenerator
    {
        private static readonly Regex EndWithNumberRegex = new Regex(@"\d+$");
        private static readonly Regex NameRegex = new Regex(@"^[A-Z][A-Za-z0-9_]*$");

        public static bool CheckRawData(DictionaryProcessor dictionaryProcessor, string dictionaryName)
        {
            for (int i = 0; i < dictionaryProcessor.RawColumnCount; i++)
            {
                string name = dictionaryProcessor.GetName(i);
                if (string.IsNullOrEmpty(name) || name == "#")
                {
                    continue;
                }

                if (!NameRegex.IsMatch(name))
                {
                    Debug.LogWarning(Utility.Text.Format("Check raw data failure. DictionaryName='{0}' Name='{1}'", dictionaryName, name));
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 从excle 生成字典
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        public static DictionaryProcessor CreatDictionaryProcessor(ExcelWorksheet sheet)
        {
            return new DictionaryProcessor(sheet, Encoding.UTF8, 0, 1);
        }

        public static void GenerateDictionaryDataFile(DictionaryProcessor dictionaryProcessor, string dictionaryName)
        {
            string binaryDataFileName = Utility.Path.GetRegularPath(Path.Combine(DvalmiSetting.Instance.DictionaryDataPath, dictionaryName + ".bytes"));
            if (!dictionaryProcessor.GenerateDataFile(binaryDataFileName) && File.Exists(binaryDataFileName))
            {
                File.Delete(binaryDataFileName);
            }
        }
    }
}
