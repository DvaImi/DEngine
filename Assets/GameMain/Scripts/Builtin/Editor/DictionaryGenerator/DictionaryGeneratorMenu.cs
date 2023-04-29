// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-19 23:18:18
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GeminiLion.Editor.DictionaryTools
{
    public class DictionaryGeneratorMenu : MonoBehaviour
    {
        [MenuItem("GeminiLion/Generate Dictionaries/To Binary")]
        internal static void GenerateDictionariesToBinary()
        {
            ExcelUtility.ConvertExcelToBinary(GeminiLionSetting.Instance.DictionaryExclePath, GeminiLionSetting.Instance.DictionaryDataPath, out List<string> dictionary, Predicate, "Runtime");
            PreloadUtility.GenerateDictionaryInfoFile(dictionary);
            AssetDatabase.Refresh();
        }

        [MenuItem("GeminiLion/Generate Dictionaries/To Text")]
        internal static void GenerateDictionariesToText()
        {
            ExcelUtility.ConvertExcelToTxt(GeminiLionSetting.Instance.DictionaryExclePath, GeminiLionSetting.Instance.DictionaryDataPath, out List<string> dictionary, Predicate, "Runtime");
            PreloadUtility.GenerateDictionaryInfoFile(dictionary);
            AssetDatabase.Refresh();
        }
        /// <summary>
        /// 过滤Builtin 表
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool Predicate(string obj)
        {
            return obj.Contains("Builtin");
        }

        [MenuItem("GeminiLion/Generate Config/To Binary")]
        internal static void GenerateConfigToBinary()
        {
            ExcelUtility.ConvertExcelToBinary(GeminiLionSetting.Instance.ConfigExcelPath, GeminiLionSetting.Instance.ConfigDataPath, out List<string> config);
            PreloadUtility.GenerateConfigInfoFile(config);
            AssetDatabase.Refresh();
        }

        [MenuItem("GeminiLion/Generate Config/To Text")]
        internal static void GenerateConfigToText()
        {
            ExcelUtility.ConvertExcelToTxt(GeminiLionSetting.Instance.ConfigExcelPath, GeminiLionSetting.Instance.ConfigDataPath, out List<string> config);
            PreloadUtility.GenerateConfigInfoFile(config);
            AssetDatabase.Refresh();
        }
    }
}
