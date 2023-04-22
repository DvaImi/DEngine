//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameFramework;
using OfficeOpenXml;

namespace GeminiLion.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        public DataTableProcessor(ExcelWorksheet sheet, int nameRow, int typeRow, int? defaultValueRow, int? commentRow, int contentStartRow, int idColumn)
        {
            if (sheet == null)
            {
                throw new GameFrameworkException("ExcelWorksheet  is invalid.");
            }

            var rawRowCount = 0;
            var rawColumnCount = 0;
            var rawValues = new List<string[]>();
            rawColumnCount = sheet.Dimension.End.Column;
            if (sheet.Dimension.End.Row < 3)
            {
                throw new GameFrameworkException(Utility.Text.Format("{0} has wrong row num!", sheet.Dimension.End.Row));
            }

            int columnCount = sheet.Dimension.End.Column;
            for (int i = 1; i <= sheet.Dimension.End.Row; i++)
            {
                //内容 开始行
                if (i > 4)
                {
                    //索引从0开始但是EPPlus 需要从1开始遍历 使用时需要+1
                    if (sheet.Cells[i, 1 + 1].Value == null)
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
                throw new GameFrameworkException(Utility.Text.Format("Name row '{0}' is invalid.", nameRow));
            }

            if (typeRow < 0)
            {
                throw new GameFrameworkException(Utility.Text.Format("Type row '{0}' is invalid.", typeRow));
            }

            if (contentStartRow < 0)
            {
                throw new GameFrameworkException(Utility.Text.Format("Content start row '{0}' is invalid.", contentStartRow));
            }

            if (idColumn < 0)
            {
                throw new GameFrameworkException(Utility.Text.Format("Id column '{0}' is invalid.", idColumn));
            }

            if (nameRow >= rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Name row '{0}' >= raw row count '{1}' is not allow.", nameRow, rawRowCount));
            }

            if (typeRow >= rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Type row '{0}' >= raw row count '{1}' is not allow.", typeRow, rawRowCount));
            }

            if (defaultValueRow.HasValue && defaultValueRow.Value >= rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Default value row '{0}' >= raw row count '{1}' is not allow.", defaultValueRow.Value, rawRowCount));
            }

            if (commentRow.HasValue && commentRow.Value >= rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Comment row '{0}' >= raw row count '{1}' is not allow.", commentRow.Value, rawRowCount));
            }

            if (contentStartRow > rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Content start row '{0}' > raw row count '{1}' is not allow.", contentStartRow, rawRowCount));
            }

            if (idColumn >= rawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Id column '{0}' >= raw column count '{1}' is not allow.", idColumn, rawColumnCount));
            }

            m_NameRow = m_RawValues[nameRow];
            m_TypeRow = m_RawValues[typeRow];
            m_DefaultValueRow = defaultValueRow.HasValue ? m_RawValues[defaultValueRow.Value] : null;
            m_CommentRow = commentRow.HasValue ? m_RawValues[commentRow.Value] : null;
            m_ContentStartRow = contentStartRow;
            m_IdColumn = idColumn;

            m_DataProcessor = new DataProcessor[rawColumnCount];
            for (int i = 0; i < rawColumnCount; i++)
            {
                if (i == IdColumn)
                {
                    m_DataProcessor[i] = DataProcessorUtility.GetDataProcessor("id");
                }
                else
                {
                    m_DataProcessor[i] = DataProcessorUtility.GetDataProcessor(m_TypeRow[i]);
                }
            }

            Dictionary<string, int> strings = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = contentStartRow; i < rawRowCount; i++)
            {
                if (IsCommentRow(i))
                {
                    continue;
                }

                for (int j = 0; j < rawColumnCount; j++)
                {
                    if (m_DataProcessor[j].LanguageKeyword != "string")
                    {
                        continue;
                    }

                    string str = m_RawValues[i][j];
                    if (strings.ContainsKey(str))
                    {
                        strings[str]++;
                    }
                    else
                    {
                        strings[str] = 1;
                    }
                }
            }

            m_Strings = strings.OrderBy(value => value.Key).OrderByDescending(value => value.Value).Select(value => value.Key).ToArray();

            m_CodeTemplate = null;
            m_CodeGenerator = null;
        }
    }
}