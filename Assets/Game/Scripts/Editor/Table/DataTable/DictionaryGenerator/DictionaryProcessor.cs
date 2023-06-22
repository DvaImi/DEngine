using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DEngine;
using OfficeOpenXml;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public sealed class DictionaryProcessor
    {
        /// <summary>
        /// 注释行分隔符。
        /// </summary>
        private const string CommentLineSeparator = "#";

        /// <summary>
        /// 数据分隔符。
        /// </summary>
        private static readonly char[] DataSplitSeparators = new char[] { '\t' };

        /// <summary>
        /// 数据修剪符。
        /// </summary>
        private static readonly char[] DataTrimSeparators = new char[] { '\"' };

        /// <summary>
        /// 名称行字符数组。
        /// </summary>
        private readonly string[] m_NameRow;

        /// <summary>
        /// 内容起始行索引。
        /// </summary>
        private readonly int m_ContentStartRow;

        /// <summary>
        /// 数据表的所有原始值。
        /// </summary>
        private readonly string[][] m_RawValues;

        public DictionaryProcessor(ExcelWorksheet sheet, Encoding encoding, int nameRow, int contentStartRow)
        {
            var rawRowCount = 0;
            var rawColumnCount = 0;
            var rawValues = new List<string[]>();
            rawColumnCount = sheet.Dimension.End.Column;
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
                for (int j = 1; j <= rawColumnCount; j++)
                {
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
                throw new DEngineException(Utility.Text.Format("Name row '{0}' is invalid.", nameRow));
            }

            if (contentStartRow < 0)
            {
                throw new DEngineException(Utility.Text.Format("Content start row '{0}' is invalid.", contentStartRow.ToString()));
            }

            if (nameRow >= rawRowCount)
            {
                throw new DEngineException(Utility.Text.Format("Name row '{0}' >= raw row count '{1}' is not allow.", nameRow, rawRowCount));
            }

            if (contentStartRow > rawRowCount)
            {
                throw new DEngineException(Utility.Text.Format("Content start row '{0}' > raw row count '{1}' is not allow.", contentStartRow, rawRowCount));
            }

            m_NameRow = m_RawValues[nameRow];
            m_ContentStartRow = contentStartRow;
        }

        /// <summary>
        /// 原始行数。
        /// </summary>
        public int RawRowCount
        {
            get
            {
                return m_RawValues.Length;
            }
        }

        /// <summary>
        /// 原始列数。
        /// </summary>
        public int RawColumnCount
        {
            get
            {
                return m_RawValues.Length > 0 ? m_RawValues[0].Length : 0;
            }
        }

        /// <summary>
        /// 内容起始行索引。
        /// </summary>
        public int ContentStartRow
        {
            get
            {
                return m_ContentStartRow;
            }
        }

        /// <summary>
        /// 是否是注释行。
        /// </summary>
        /// <param name="rawRow"></param>
        /// <returns></returns>
        /// <exception cref="DEngineException"></exception>
        public bool IsCommentRow(int rawRow)
        {
            if (rawRow < 0 || rawRow >= RawRowCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw row '{0}' is out of range.", rawRow));
            }

            return GetValue(rawRow, 0).StartsWith(CommentLineSeparator, StringComparison.Ordinal);
        }

        /// <summary>
        /// 是否是注释列。
        /// </summary>
        /// <param name="rawColumn"></param>
        /// <returns></returns>
        /// <exception cref="DEngineException"></exception>
        public bool IsCommentColumn(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            string columnName = GetName(rawColumn);
            return string.IsNullOrEmpty(columnName) || columnName.Contains(CommentLineSeparator);
        }

        /// <summary>
        /// 获取列的名称。
        /// </summary>
        /// <param name="rawColumn"></param>
        /// <returns></returns>
        /// <exception cref="DEngineException"></exception>
        public string GetName(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_NameRow[rawColumn];
        }

        /// <summary>
        /// 获取值。
        /// </summary>
        /// <param name="rawRow">原始行索引。</param>
        /// <param name="rawColumn">原始列索引。</param>
        /// <returns>值。</returns>
        /// <exception cref="DEngineException"></exception>
        public string GetValue(int rawRow, int rawColumn)
        {
            if (rawRow < 0 || rawRow >= RawRowCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw row '{0}' is out of range.", rawRow));
            }

            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new DEngineException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_RawValues[rawRow][rawColumn];
        }

        public bool GenerateDataFile(string outputFileName)
        {
            if (string.IsNullOrEmpty(outputFileName))
            {
                throw new DEngineException("Output file name is invalid.");
            }

            try
            {
                using (FileStream fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                    {
                        for (int rawRow = ContentStartRow; rawRow < RawRowCount; rawRow++)
                        {
                            if (IsCommentRow(rawRow))
                            {
                                continue;
                            }

                            for (int rawColumn = 0; rawColumn < RawColumnCount; rawColumn++)
                            {
                                if (IsCommentColumn(rawColumn))
                                {
                                    continue;
                                }

                                try
                                {
                                    string value = GetValue(rawRow, rawColumn);
                                    binaryWriter.Write(value);
                                }
                                catch
                                {
                                    Debug.LogError(Utility.Text.Format("Generate config file failure. OutputFileName='{0}' RawRow='{1}' RowColumn='{2}'.", outputFileName, rawRow.ToString(), rawColumn.ToString()));
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(Utility.Text.Format("Parse '{0}' failure, exception is '{1}'.", outputFileName, exception));
                return false;
            }
        }
    }
}
