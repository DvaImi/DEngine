using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DEngine;
using OfficeOpenXml;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private const string CommentLineSeparator = "#";
        private readonly string[] m_CommentRow;

        private readonly DataProcessor[] m_DataProcessor;
        private readonly string[] m_DefaultValueRow;

        private readonly string[] m_NameRow;
        private readonly string[][] m_RawValues;
        private readonly string[] m_Strings;
        private DataTableCodeGenerator m_CodeGenerator;
        private readonly int m_EnumNameColumn;

        /// <summary>
        /// 解析 Excel 表格并提取数据。
        /// </summary>
        /// <param name="sheet">要解析的工作表。</param>
        /// <param name="nameRow">包含字段名称的行索引。</param>
        /// <param name="typeRow">包含字段类型的行索引。</param>
        /// <param name="defaultValueRow">包含字段默认值的行索引。</param>
        /// <param name="commentRow">包含字段注释的行索引。</param>
        /// <param name="contentStartRow">数据内容起始行索引。</param>
        /// <param name="idColumn">ID 列索引，用于标识每条记录。</param>
        /// <param name="enumNameColumn">枚举名称列，用于生成枚举类型的字段。</param>
        public DataTableProcessor(ExcelWorksheet sheet, int nameRow, int typeRow, int? defaultValueRow, int? commentRow, int contentStartRow, int idColumn, int enumNameColumn)
        {
            var rawRowCount = 0;
            var rawValues = new List<string[]>();
            var rawColumnCount = sheet.Dimension.End.Column;

            var validColumns = new List<int>();
            for (int col = 1; col <= rawColumnCount; col++)
            {
                bool hasData = false;
                for (int row = 1; row <= sheet.Dimension.End.Row; row++)
                {
                    if (sheet.Cells[row, col].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[row, col].Value.ToString()))
                    {
                        hasData = true;
                        break;
                    }
                }

                if (hasData)
                {
                    validColumns.Add(col);
                }
            }

            rawColumnCount = validColumns.Count;
            m_EnumNameColumn = enumNameColumn;
            for (int i = 1; i <= sheet.Dimension.End.Row; i++)
            {
                if (i > DataTableSetting.Instance.ContentStartRow)
                {
                    //跳过没有id的空行
                    if (sheet.Cells[i, DataTableSetting.Instance.IdColumn + 1].Value == null)
                    {
                        continue;
                    }
                }

                var rawValue = new string[rawColumnCount];
                for (int j = 1; j <= validColumns.Count; j++)
                {
                    //跳过生成枚举列
                    if (validColumns[j - 1] == enumNameColumn)
                    {
                        continue;
                    }

                    if (sheet.Cells[i, j].Value == null)
                    {
                        rawValue[j - 1] = string.Empty;
                    }
                    else
                    {
                        rawValue[j - 1] = sheet.Cells[i, j].Value.ToString();
                    }
                }

                rawRowCount++;

                rawValues.Add(rawValue);
            }

            m_RawValues = rawValues.ToArray();


            if (nameRow < 0)
            {
                throw new DEngineException(Utility.Text.Format("Name row '{0}' is invalid.", nameRow.ToString()));
            }

            if (typeRow < 0)
            {
                throw new DEngineException(Utility.Text.Format("Type row '{0}' is invalid.", typeRow.ToString()));
            }

            if (contentStartRow < 0)
            {
                throw new DEngineException(Utility.Text.Format("Content start row '{0}' is invalid.", contentStartRow.ToString()));
            }

            if (idColumn < 0)
            {
                throw new DEngineException(Utility.Text.Format("Id column '{0}' is invalid.", idColumn.ToString()));
            }

            if (nameRow >= rawRowCount)
            {
                throw new DEngineException(Utility.Text.Format("Name row '{0}' >= raw row count '{1}' is not allow.", nameRow.ToString(), rawRowCount.ToString()));
            }

            if (typeRow >= rawRowCount)
            {
                throw new DEngineException(Utility.Text.Format("Type row '{0}' >= raw row count '{1}' is not allow.", typeRow.ToString(), rawRowCount.ToString()));
            }

            if (defaultValueRow >= rawRowCount)
            {
                throw new DEngineException(Utility.Text.Format("Default value row '{0}' >= raw row count '{1}' is not allow.", defaultValueRow.Value.ToString(), rawRowCount.ToString()));
            }

            if (commentRow >= rawRowCount)
            {
                throw new DEngineException(Utility.Text.Format("Comment row '{0}' >= raw row count '{1}' is not allow.", commentRow.Value.ToString(), rawRowCount.ToString()));
            }

            if (contentStartRow > rawRowCount)
            {
                throw new DEngineException(Utility.Text.Format("Content start row '{0}' > raw row count '{1}' is not allow.", contentStartRow.ToString(), rawRowCount.ToString()));
            }

            if (idColumn >= rawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Id column '{0}' >= raw column count '{1}' is not allow.", idColumn.ToString(), rawColumnCount.ToString()));
            }

            m_NameRow = m_RawValues[nameRow];
            var typeRow1 = m_RawValues[typeRow];
            m_DefaultValueRow = defaultValueRow.HasValue ? m_RawValues[defaultValueRow.Value] : null;
            m_CommentRow = commentRow.HasValue ? m_RawValues[commentRow.Value] : null;
            ContentStartRow = contentStartRow;
            IdColumn = idColumn;

            m_DataProcessor = new DataProcessor[rawColumnCount];
            for (var i = 0; i < rawColumnCount; i++)
            {
                if (i == IdColumn)
                {
                    m_DataProcessor[i] = DataProcessorUtility.GetDataProcessor("id");
                }
                else
                {
                    m_DataProcessor[i] = DataProcessorUtility.GetDataProcessor(typeRow1[i]);
                }
            }

            var strings = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = contentStartRow; i < rawRowCount; i++)
            {
                if (IsCommentRow(i))
                {
                    continue;
                }

                for (var j = 0; j < rawColumnCount; j++)
                {
                    if (m_DataProcessor[j] is ICollectionProcessor collectionProcessor)
                    {
                        if (collectionProcessor.ItemLanguageKeyword != "string")
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (m_DataProcessor[j].LanguageKeyword != "string")
                        {
                            continue;
                        }
                    }

                    var str = m_RawValues[i][j];
                    var values = str.Split(',');
                    foreach (var value in values)
                    {
                        if (!strings.TryAdd(value, 1))
                        {
                            strings[value]++;
                        }
                    }
                }
            }

            m_Strings = strings.OrderBy(value => value.Key).ThenByDescending(value => value.Value).Select(value => value.Key).ToArray();
            m_CodeGenerator = null;
        }

        public int RawRowCount => m_RawValues.Length;

        public int RawColumnCount => m_RawValues.Length > 0 ? m_RawValues[0].Length : 0;

        public int StringCount => m_Strings.Length;

        public int ContentStartRow { get; }

        public int IdColumn { get; }

        public bool IsListColumn(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return m_DataProcessor[rawColumn].GetTypeStrings()[0].Equals("List<{0}>");
        }

        public bool IsArrayColumn(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return m_DataProcessor[rawColumn].GetTypeStrings()[0].Equals("{0}[]");
        }

        public bool IsEnumColumn(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return m_DataProcessor[rawColumn].IsEnum;
        }

        public bool IsDictionaryColumn(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return m_DataProcessor[rawColumn].GetTypeStrings()[0].Equals("Dictionary<{0},{1}>");
        }

        public bool IsIdColumn(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return m_DataProcessor[rawColumn].IsId;
        }

        public bool IsCommentRow(int rawRow)
        {
            if (rawRow < 0 || rawRow >= RawRowCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw row '{0}' is out of range.", rawRow.ToString()));
            }

            return GetValue(rawRow, 0).StartsWith(CommentLineSeparator, StringComparison.Ordinal);
        }

        public bool IsCommentColumn(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return string.IsNullOrEmpty(GetName(rawColumn)) || m_DataProcessor[rawColumn].IsComment;
        }

        public bool IsEnumNameColumn(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return string.IsNullOrEmpty(GetName(rawColumn)) || rawColumn == m_EnumNameColumn;
        }

        public string GetName(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return IsIdColumn(rawColumn) ? "Id" : m_NameRow[rawColumn];
        }

        public bool IsSystem(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return m_DataProcessor[rawColumn].IsSystem;
        }

        public Type GetType(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return m_DataProcessor[rawColumn].Type;
        }

        public string GetLanguageKeyword(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return m_DataProcessor[rawColumn].LanguageKeyword;
        }

        public string[] GetTypeStrings(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return m_DataProcessor[rawColumn].GetTypeStrings();
        }

        public string GetDefaultValue(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return m_DefaultValueRow != null ? m_DefaultValueRow[rawColumn] : null;
        }

        public string GetComment(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return m_CommentRow != null ? m_CommentRow[rawColumn] : null;
        }

        public string GetValue(int rawRow, int rawColumn)
        {
            if (rawRow < 0 || rawRow >= RawRowCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw row '{0}' is out of range.", rawRow.ToString()));
            }

            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn.ToString()));
            }

            return m_RawValues[rawRow][rawColumn];
        }

        public string GetString(int index)
        {
            if (index < 0 || index >= StringCount)
            {
                throw new DEngineException(Utility.Text.Format("String index '{0}' is out of range.", index.ToString()));
            }

            return m_Strings[index];
        }

        public int GetStringIndex(string str)
        {
            for (var i = 0; i < StringCount; i++)
            {
                if (m_Strings[i] == str)
                {
                    return i;
                }
            }

            return -1;
        }

        public bool GenerateDataFile(string outputFileName)
        {
            if (string.IsNullOrEmpty(outputFileName))
            {
                throw new DEngineException("Output file name is invalid.");
            }

            try
            {
                using (var fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
                {
                    using (var binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                    {
                        for (var rawRow = ContentStartRow; rawRow < RawRowCount; rawRow++)
                        {
                            if (IsCommentRow(rawRow))
                            {
                                continue;
                            }

                            var bytes = GetRowBytes(outputFileName, rawRow);
                            binaryWriter.Write7BitEncodedInt32(bytes.Length);
                            binaryWriter.Write(bytes);
                        }
                    }
                }

                Debug.Log(Utility.Text.Format("Parse data table '{0}' success.", outputFileName));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(Utility.Text.Format("Parse data table '{0}' failure, exception is '{1}'.", outputFileName, exception.ToString()));
                return false;
            }
        }

        public void SetCodeGenerator(DataTableCodeGenerator codeGenerator)
        {
            m_CodeGenerator = codeGenerator;
        }

        public bool GenerateCodeFile(string outputFileName, Encoding encoding, object userData = null)
        {
            if (string.IsNullOrEmpty(DataProcessorUtility.CodeTemplate))
            {
                throw new DEngineException("You must set code template first.");
            }

            if (string.IsNullOrEmpty(outputFileName))
            {
                throw new DEngineException("Output file name is invalid.");
            }

            try
            {
                var stringBuilder = new StringBuilder(DataProcessorUtility.CodeTemplate);
                m_CodeGenerator?.Invoke(this, stringBuilder, userData);
                using (var fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
                {
                    using (var stream = new StreamWriter(fileStream, encoding))
                    {
                        stream.Write(stringBuilder.ToString());
                    }
                }

                Debug.Log(Utility.Text.Format("Generate code file '{0}' success.", outputFileName));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(Utility.Text.Format("Generate code file '{0}' failure, exception is '{1}'.", outputFileName, exception.ToString()));
                return false;
            }
        }

        private byte[] GetRowBytes(string outputFileName, int rawRow)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8))
                {
                    for (var rawColumn = 0; rawColumn < RawColumnCount; rawColumn++)
                    {
                        if (IsCommentColumn(rawColumn))
                        {
                            continue;
                        }

                        if (IsEnumNameColumn(rawColumn))
                        {
                            continue;
                        }

                        try
                        {
                            m_DataProcessor[rawColumn].WriteToStream(this, binaryWriter, GetValue(rawRow, rawColumn));
                        }
                        catch
                        {
                            if (m_DataProcessor[rawColumn].IsId || string.IsNullOrEmpty(GetDefaultValue(rawColumn)))
                            {
                                Debug.LogError(Utility.Text.Format("Parse raw value failure. OutputFileName='{0}' RawRow='{1}' RowColumn='{2}' Name='{3}' Type='{4}' RawValue='{5}'", outputFileName, rawRow.ToString(), rawColumn.ToString(), GetName(rawColumn), GetLanguageKeyword(rawColumn), GetValue(rawRow, rawColumn)));
                                return null;
                            }

                            Debug.LogWarning(Utility.Text.Format("Parse raw value failure, will try default value. OutputFileName='{0}' RawRow='{1}' RowColumn='{2}' Name='{3}' Type='{4}' RawValue='{5}'", outputFileName, rawRow.ToString(), rawColumn.ToString(), GetName(rawColumn), GetLanguageKeyword(rawColumn), GetValue(rawRow, rawColumn)));
                            try
                            {
                                m_DataProcessor[rawColumn].WriteToStream(this, binaryWriter, GetDefaultValue(rawColumn));
                            }
                            catch
                            {
                                Debug.LogError(Utility.Text.Format("Parse default value failure. OutputFileName='{0}' RawRow='{1}' RowColumn='{2}' Name='{3}' Type='{4}' RawValue='{5}'", outputFileName, rawRow.ToString(), rawColumn.ToString(), GetName(rawColumn), GetLanguageKeyword(rawColumn), GetComment(rawColumn)));
                                return null;
                            }
                        }
                    }

                    return memoryStream.ToArray();
                }
            }
        }

        public DataProcessor GetDataProcessor(int rawColumn)
        {
            return m_DataProcessor[rawColumn];
        }

        public string GetNameSpace(int rawColumn)
        {
            return m_DataProcessor[rawColumn].GetType().GetProperty("NameSpace")?.GetValue(m_DataProcessor[rawColumn]) as string;
        }
    }
}