using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DEngine;
using OfficeOpenXml;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public sealed class DataTableGenerator
    {
        private static readonly Regex EndWithNumberRegex = new(@"\d+$");
        private static readonly Regex NameRegex = new("^[A-Z][A-Za-z0-9_]*$");
        private static List<string> s_NameSpace = new();

        public static DataTableProcessor CreateExcelDataTableProcessor(ExcelWorksheet sheet)
        {
            int  nameRow         = DataTableSetting.Instance.NameRow;
            int  typeRow         = DataTableSetting.Instance.TypeRow;
            int? commentRow      = DataTableSetting.Instance.CommentRow;
            int  contentStartRow = DataTableSetting.Instance.ContentStartRow;
            int  idColumn        = DataTableSetting.Instance.IdColumn;
            int  enumNameColumn  = DataTableSetting.Instance.EnumNameColumn;
            return new DataTableProcessor(sheet, nameRow, typeRow, null, commentRow, contentStartRow, idColumn, enumNameColumn);
        }

        public static bool CheckRawData(DataTableProcessor dataTableProcessor, string dataTableName)
        {
            for (var i = 0; i < dataTableProcessor.RawColumnCount; i++)
            {
                var name = dataTableProcessor.GetName(i);
                if (string.IsNullOrEmpty(name) || name == "#" || name.StartsWith("#"))
                {
                    continue;
                }

                if (!NameRegex.IsMatch(name))
                {
                    Debug.LogWarning(Utility.Text.Format("Check raw data failure. DataTableName='{0}' Name='{1}'", dataTableName, name));
                    return false;
                }
            }

            return true;
        }

        public static void GenerateDataFile(DataTableProcessor dataTableProcessor, string dataTableName)
        {
            var binaryDataFileName = Utility.Path.GetRegularPath(Path.Combine(DataTableSetting.Instance.OutputDataTableFolder, dataTableName + ".bytes"));

            FileInfo fileInfo = new FileInfo(binaryDataFileName);
            if (fileInfo.Directory is { Exists: false })
            {
                fileInfo.Directory.Create();
            }

            if (!dataTableProcessor.GenerateDataFile(binaryDataFileName) && File.Exists(binaryDataFileName))
            {
                File.Delete(binaryDataFileName);
            }
        }

        public static void GenerateDataFile(DataTableProcessor dataTableProcessor, string folderPath, string binaryDataFileName)
        {
            FileInfo fileInfo = new FileInfo(Utility.Path.GetRegularPath(Path.Combine(folderPath, binaryDataFileName)));
            if (fileInfo.Directory is { Exists: false })
            {
                fileInfo.Directory.Create();
            }

            if (!dataTableProcessor.GenerateDataFile(binaryDataFileName) && File.Exists(binaryDataFileName))
            {
                File.Delete(binaryDataFileName);
            }
        }

        public static void GenerateCodeFile(DataTableProcessor dataTableProcessor, string dataTableName)
        {
            dataTableProcessor.SetCodeGenerator(DataTableCodeGenerator);
            bool isChanged = CheckIsChanged(dataTableProcessor, dataTableName);
            if (!isChanged)
            {
                Debug.Log($"DR{dataTableName} is not Changed,don't have to regenerate it");
                return;
            }

            var      csharpCodeFileName = Utility.Path.GetRegularPath(Path.Combine(DataTableSetting.Instance.CSharpCodePath, "DR" + dataTableName + ".cs"));
            FileInfo fileInfo           = new FileInfo(csharpCodeFileName);
            if (fileInfo.Directory is { Exists: false })
            {
                fileInfo.Directory.Create();
            }

            if (!dataTableProcessor.GenerateCodeFile(csharpCodeFileName, Encoding.UTF8, dataTableName) && File.Exists(csharpCodeFileName))
            {
                File.Delete(csharpCodeFileName);
            }
        }

        public static void GenerateCodeFile(DataTableProcessor dataTableProcessor, string dataTableName, string csharpCodeFileName, string nameSpace)
        {
            dataTableProcessor.SetCodeGenerator(DataTableCodeGeneratorV2);
            bool isChanged = CheckIsChanged(dataTableProcessor, dataTableName);
            if (!isChanged)
            {
                Debug.Log($"DR{dataTableName} is not Changed,don't have to regenerate it");
                return;
            }

            FileInfo fileInfo = new FileInfo(csharpCodeFileName);
            if (fileInfo.Directory is { Exists: false })
            {
                fileInfo.Directory.Create();
            }

            if (!dataTableProcessor.GenerateCodeFile(csharpCodeFileName, Encoding.UTF8, (dataTableName, nameSpace)) && File.Exists(csharpCodeFileName))
            {
                File.Delete(csharpCodeFileName);
            }
        }

        public static void GenerateDataEnumFile(DataTableProcessor dataTableProcessor, string dataTableName)
        {
            string fileName       = $"{dataTableName}Id";
            string outputFileName = Utility.Path.GetRegularPath(Path.Combine(DataTableSetting.Instance.DataTableEnumPath, fileName + ".cs"));
            var    fileInfo       = new FileInfo(outputFileName);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
                var meta = new FileInfo(outputFileName + ".meta");
                if (meta.Exists)
                {
                    meta.Delete();
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("//------------------------------------------------------------")
                         .AppendLine("// This file is automatically generated by tools. Do not modify directly.")
                         .AppendLine("//------------------------------------------------------------")
                         .AppendLine($"namespace {DataTableSetting.Instance.NameSpace}")
                         .AppendLine("{")
                         .AppendLine($"\t// {dataTableProcessor.GetValue(3, 1)}")
                         .AppendLine($"\tpublic enum {fileName} : byte")
                         .AppendLine("\t{");

            for (int i = dataTableProcessor.ContentStartRow; i < dataTableProcessor.RawRowCount; i++)
            {
                var enumColumnValue = dataTableProcessor.GetValue(i, DataTableSetting.Instance.EnumNameColumn);
                if (string.IsNullOrEmpty(enumColumnValue))
                {
                    continue;
                }

                if (!CodeGenerator.IsValidLanguageIndependentIdentifier(enumColumnValue))
                {
                    Debug.LogWarning($"Warning:  DataTableName='{dataTableName}' '{enumColumnValue}' is not a valid enum name at row {i} column {DataTableSetting.Instance.EnumNameColumn}.");
                    continue;
                }

                stringBuilder.AppendLine($"\t\t// {dataTableProcessor.GetValue(i, 2)}").AppendLine($"\t\t{enumColumnValue} = {dataTableProcessor.GetValue(i, DataTableSetting.Instance.IdColumn)},");
            }

            stringBuilder.AppendLine("\t}").AppendLine("}");

            if (fileInfo.Directory is { Exists: false })
            {
                fileInfo.Directory.Create();
            }

            using (var fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
            {
                using (var stream = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    stream.Write(stringBuilder.ToString());
                }
            }

            Debug.Log(Utility.Text.Format("Generate code file '{0}' success.", outputFileName));
        }

        private static bool CheckIsChanged(DataTableProcessor dataTableProcessor, string dataTableName)
        {
            string oldCsharpCodePath = Path.Combine(DataTableSetting.Instance.CSharpCodePath, "DR" + dataTableName + ".cs");
            if (!File.Exists(oldCsharpCodePath))
            {
                return true;
            }

            var stringBuilder = new StringBuilder(File.ReadAllText(DataTableSetting.Instance.CSharpCodeTemplateFileName, Encoding.UTF8));
            DataTableCodeGenerator(dataTableProcessor, stringBuilder, dataTableName);
            string csharpCode    = GetNotHeadString(stringBuilder.ToString());
            string oldCsharpCode = GetNotHeadString(File.ReadAllText(oldCsharpCodePath));
            return csharpCode != oldCsharpCode;
        }

        private static string GetNotHeadString(string str)
        {
            int index = str.IndexOf("using", StringComparison.Ordinal);
            str = str[index..];
            return str;
        }

        private static void DataTableCodeGenerator(DataTableProcessor dataTableProcessor, StringBuilder codeContent, object userData)
        {
            var dataTableName = (string)userData;

            codeContent.Replace("__DATA_TABLE_CREATE_TIME__", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            codeContent.Replace("__DATA_TABLE_NAME_SPACE__", DataTableSetting.Instance.NameSpace);
            codeContent.Replace("__DATA_TABLE_CLASS_NAME__", "DR" + dataTableName);
            codeContent.Replace("__DATA_TABLE_COMMENT__", dataTableProcessor.GetValue(0, 1) + "。");
            codeContent.Replace("__DATA_TABLE_ID_COMMENT__", "获取" + dataTableProcessor.GetComment(dataTableProcessor.IdColumn) + "。");
            codeContent.Replace("__DATA_TABLE_PROPERTIES__", GenerateDataTableProperties(dataTableProcessor));
            codeContent.Replace("__DATA_TABLE_PARSER__", GenerateDataTableParser(dataTableProcessor));
            codeContent.Replace("__DATA_TABLE_PROPERTY_ARRAY__", GenerateDataTablePropertyArray(dataTableProcessor));
            codeContent.Replace("__DATA_TABLE_TOSTRING__", GenerateDataTableToString(dataTableProcessor));
            s_NameSpace = s_NameSpace.Distinct().ToList();
            StringBuilder nameSpaceBuilder = new StringBuilder();
            foreach (string nameSpace in s_NameSpace)
            {
                nameSpaceBuilder.AppendLine($"using {nameSpace};");
            }

            codeContent.Replace("__DATA_TABLE_PROPERTIES_NAMESPACE__", nameSpaceBuilder.ToString());
        }

        private static string GenerateDataTableToString(DataTableProcessor dataTableProcessor)
        {
            StringBuilder codeContent = new StringBuilder();
            codeContent.AppendLine();

            codeContent.AppendLine("        public override string ToString()");
            codeContent.AppendLine("        {");

            codeContent.Append("            return string.Concat(");
            for (var i = 0; i < dataTableProcessor.RawColumnCount; i++)
            {
                if (dataTableProcessor.IsCommentColumn(i) || dataTableProcessor.IsEnumNameColumn(i))
                {
                    continue;
                }

                if (dataTableProcessor.IsListColumn(i) || dataTableProcessor.IsArrayColumn(i) || dataTableProcessor.IsDictionaryColumn(i))
                {
                    codeContent.Append($"$\"{dataTableProcessor.GetName(i)}: {{GameUtility.String.CollectionToString({dataTableProcessor.GetName(i)})}}\"");
                }
                else
                {
                    codeContent.Append($"$\"{dataTableProcessor.GetName(i)}: {{{dataTableProcessor.GetName(i)}}}\"");
                }

                if (i < dataTableProcessor.RawColumnCount - 1)
                {
                    codeContent.Append(", ");
                }
            }

            codeContent.AppendLine(");");
            codeContent.AppendLine("        }");
            return codeContent.ToString();
        }


        /// <summary>
        ///  数据类型生成版本2
        /// </summary>
        /// <param name="dataTableProcessor"></param>
        /// <param name="codeContent"></param>
        /// <param name="userData"> (dataTableName, customNameSpace)</param>
        private static void DataTableCodeGeneratorV2(DataTableProcessor dataTableProcessor, StringBuilder codeContent, object userData)
        {
            var (dataTableName, customNameSpace) = ((string, string))userData;
            codeContent.Replace("__DATA_TABLE_CREATE_TIME__", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            codeContent.Replace("__DATA_TABLE_NAME_SPACE__", customNameSpace);
            codeContent.Replace("__DATA_TABLE_CLASS_NAME__", "DR" + dataTableName);
            codeContent.Replace("__DATA_TABLE_COMMENT__", dataTableProcessor.GetValue(0, 1) + "。");
            codeContent.Replace("__DATA_TABLE_ID_COMMENT__", "获取" + dataTableProcessor.GetComment(dataTableProcessor.IdColumn) + "。");
            codeContent.Replace("__DATA_TABLE_PROPERTIES__", GenerateDataTableProperties(dataTableProcessor));
            codeContent.Replace("__DATA_TABLE_PARSER__", GenerateDataTableParser(dataTableProcessor));
            codeContent.Replace("__DATA_TABLE_PROPERTY_ARRAY__", GenerateDataTablePropertyArray(dataTableProcessor));
            s_NameSpace = s_NameSpace.Distinct().ToList();
            StringBuilder nameSpaceBuilder = new StringBuilder();
            foreach (string nameSpace in s_NameSpace)
            {
                nameSpaceBuilder.AppendLine($"using {nameSpace};");
            }

            // 添加配置路径
            nameSpaceBuilder.AppendLine($"using Game.Update;");
            codeContent.Replace("__DATA_TABLE_PROPERTIES_NAMESPACE__", nameSpaceBuilder.ToString());
        }

        private static string GenerateDataTableProperties(DataTableProcessor dataTableProcessor)
        {
            var stringBuilder = new StringBuilder();
            var firstProperty = true;
            for (var i = 0; i < dataTableProcessor.RawColumnCount; i++)
            {
                // 编号列
                if (dataTableProcessor.IsIdColumn(i))
                {
                    continue;
                }

                // 注释列
                if (dataTableProcessor.IsCommentColumn(i))
                {
                    continue;
                }

                // 枚举列
                if (dataTableProcessor.IsEnumNameColumn(i))
                {
                    continue;
                }

                if (firstProperty)
                {
                    firstProperty = false;
                }
                else
                {
                    stringBuilder.AppendLine().AppendLine();
                }

                stringBuilder
                   .AppendLine("        /// <summary>")
                   .AppendFormat("        /// 获取{0}。", dataTableProcessor.GetComment(i)).AppendLine()
                   .AppendLine("        /// </summary>")
                   .AppendFormat("        public {0} {1}", dataTableProcessor.GetLanguageKeyword(i), dataTableProcessor.GetName(i)).AppendLine()
                   .AppendLine("        {")
                   .AppendLine("            get;")
                   .AppendLine("            private set;")
                   .Append("        }");
            }

            return stringBuilder.ToString();
        }

        private static string GenerateDataTableParser(DataTableProcessor dataTableProcessor)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder
               .AppendLine("        public override bool ParseDataRow(string dataRowString, object userData)")
               .AppendLine("        {")
               .AppendLine(
                    "            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);")
               .AppendLine("            for (int i = 0; i < columnStrings.Length; i++)")
               .AppendLine("            {")
               .AppendLine(
                    "                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);")
               .AppendLine("            }")
               .AppendLine()
               .AppendLine("            int index = 0;");
            for (var i = 0; i < dataTableProcessor.RawColumnCount; i++)
            {
                // 编号列
                if (dataTableProcessor.IsIdColumn(i))
                {
                    stringBuilder.AppendLine("            m_Id = int.Parse(columnStrings[index++]);");
                    continue;
                }

                // 注释列
                if (dataTableProcessor.IsCommentColumn(i))
                {
                    stringBuilder.AppendLine("            index++;");
                    continue;
                }

                if (dataTableProcessor.IsEnumNameColumn(i))
                {
                    continue;
                }

                if (dataTableProcessor.IsSystem(i))
                {
                    var languageKeyword = dataTableProcessor.GetLanguageKeyword(i);

                    if (languageKeyword == "string")
                    {
                        stringBuilder.AppendFormat("\t\t\t{0} = columnStrings[index++];", dataTableProcessor.GetName(i)).AppendLine();
                    }
                    else
                    {
                        stringBuilder.AppendFormat("\t\t\t{0} = {1}.Parse(columnStrings[index++]);", dataTableProcessor.GetName(i), languageKeyword).AppendLine();
                    }
                }
                else
                {
                    if (dataTableProcessor.IsListColumn(i))
                    {
                        var t = dataTableProcessor.GetDataProcessor(i).GetType().GetGenericArguments();
                        if (Activator.CreateInstance(t[0]) is DataTableProcessor.DataProcessor dataProcessor)
                        {
                            string typeName = dataProcessor.Type.Name;

                            if (dataProcessor.IsEnum)
                            {
                                typeName = DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessor.LanguageKeyword);
                            }

                            stringBuilder.AppendFormat("\t\t\t{0} = DataTableExtension.Parse{1}List(columnStrings[index++]);", dataTableProcessor.GetName(i), typeName).AppendLine();
                        }

                        continue;
                    }

                    if (dataTableProcessor.IsArrayColumn(i))
                    {
                        var t = dataTableProcessor.GetDataProcessor(i).GetType().GetGenericArguments();
                        if (Activator.CreateInstance(t[0]) is DataTableProcessor.DataProcessor dataProcessor)
                        {
                            string typeName = dataProcessor.Type.Name;

                            if (dataProcessor.IsEnum)
                            {
                                typeName = DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessor.LanguageKeyword);
                            }

                            stringBuilder.AppendFormat("\t\t\t{0} = DataTableExtension.Parse{1}Array(columnStrings[index++]);", dataTableProcessor.GetName(i), typeName).AppendLine();
                        }

                        continue;
                    }

                    if (dataTableProcessor.IsDictionaryColumn(i))
                    {
                        var t               = dataTableProcessor.GetDataProcessor(i).GetType().GetGenericArguments();
                        var dataProcessorT1 = Activator.CreateInstance(t[0]) as DataTableProcessor.DataProcessor;
                        var dataProcessorT2 = Activator.CreateInstance(t[1]) as DataTableProcessor.DataProcessor;
                        if (dataProcessorT1 != null)
                        {
                            var dataProcessorT1TypeName = dataProcessorT1.Type.Name;
                            if (dataProcessorT1.IsEnum)
                            {
                                dataProcessorT1TypeName = DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessorT1.LanguageKeyword);
                            }

                            if (dataProcessorT2 != null)
                            {
                                var dataProcessorT2TypeName = dataProcessorT2.Type.Name;
                                if (dataProcessorT2.IsEnum)
                                {
                                    dataProcessorT2TypeName = DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessorT2.LanguageKeyword);
                                }

                                stringBuilder.AppendFormat("\t\t\t{0} = DataTableExtension.Parse{1}{2}Dictionary(columnStrings[index++]);", dataTableProcessor.GetName(i), dataProcessorT1TypeName, dataProcessorT2TypeName).AppendLine();
                            }
                        }

                        continue;
                    }

                    if (dataTableProcessor.IsEnumColumn(i))
                    {
                        stringBuilder.AppendLine($"\t\t\t{dataTableProcessor.GetName(i)} = DataTableExtension.EnumParse<{dataTableProcessor.GetLanguageKeyword(i)}>(columnStrings[index++]);");
                        continue;
                    }

                    stringBuilder.AppendFormat("\t\t\t{0} = DataTableExtension.Parse{1}(columnStrings[index++]);", dataTableProcessor.GetName(i), dataTableProcessor.GetType(i).Name).AppendLine();
                }
            }

            stringBuilder
               .AppendLine("            GeneratePropertyArray();")
               .AppendLine("            return true;")
               .AppendLine("        }")
               .AppendLine()
               .AppendLine(
                    "        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)")
               .AppendLine("        {")
               .AppendLine(
                    "            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))")
               .AppendLine("            {")
               .AppendLine(
                    "                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))")
               .AppendLine("                {");

            for (var i = 0; i < dataTableProcessor.RawColumnCount; i++)
            {
                // 注释列
                if (dataTableProcessor.IsCommentColumn(i))
                {
                    continue;
                }

                // 编号列
                if (dataTableProcessor.IsIdColumn(i))
                {
                    stringBuilder.AppendLine("                    m_Id = binaryReader.Read7BitEncodedInt32();");
                    continue;
                }

                // 编号列
                if (dataTableProcessor.IsIdColumn(i))
                {
                    stringBuilder.AppendLine("                m_Id = binaryReader.ReadInt32();");
                    continue;
                }

                if (dataTableProcessor.IsEnumNameColumn(i))
                {
                    continue;
                }

                var languageKeyword = dataTableProcessor.GetLanguageKeyword(i);
                if (dataTableProcessor.IsListColumn(i))
                {
                    var t = dataTableProcessor.GetDataProcessor(i).GetType().GetGenericArguments();
                    if (Activator.CreateInstance(t[0]) is DataTableProcessor.DataProcessor dataProcessor)
                    {
                        string typeName = dataProcessor.Type.Name;
                        if (dataProcessor.IsEnum)
                        {
                            typeName = DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessor.LanguageKeyword);
                        }

                        stringBuilder.AppendFormat("\t\t\t\t\t{0} = binaryReader.Read{1}List();", dataTableProcessor.GetName(i), typeName).AppendLine();
                    }

                    continue;
                }

                if (dataTableProcessor.IsArrayColumn(i))
                {
                    var t = dataTableProcessor.GetDataProcessor(i).GetType().GetGenericArguments();
                    if (Activator.CreateInstance(t[0]) is DataTableProcessor.DataProcessor dataProcessor)
                    {
                        string typeName = dataProcessor.Type.Name;

                        if (dataProcessor.IsEnum)
                        {
                            typeName = DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessor.LanguageKeyword);
                        }

                        stringBuilder.AppendFormat("\t\t\t\t\t{0} = binaryReader.Read{1}Array();", dataTableProcessor.GetName(i), typeName).AppendLine();
                    }

                    continue;
                }

                if (dataTableProcessor.IsDictionaryColumn(i))
                {
                    var t               = dataTableProcessor.GetDataProcessor(i).GetType().GetGenericArguments();
                    var dataProcessorT1 = Activator.CreateInstance(t[0]) as DataTableProcessor.DataProcessor;
                    var dataProcessorT2 = Activator.CreateInstance(t[1]) as DataTableProcessor.DataProcessor;
                    if (dataProcessorT1 != null)
                    {
                        var dataProcessorT1TypeName = dataProcessorT1.Type.Name;
                        if (dataProcessorT1.IsEnum)
                        {
                            dataProcessorT1TypeName = DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessorT1.LanguageKeyword);
                        }

                        if (dataProcessorT2 != null)
                        {
                            var dataProcessorT2TypeName = dataProcessorT2.Type.Name;
                            if (dataProcessorT2.IsEnum)
                            {
                                dataProcessorT2TypeName = DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessorT2.LanguageKeyword);
                            }

                            stringBuilder.AppendFormat("\t\t\t\t\t{0} = binaryReader.Read{1}{2}Dictionary();", dataTableProcessor.GetName(i), dataProcessorT1TypeName, dataProcessorT2TypeName).AppendLine();
                        }
                    }

                    continue;
                }

                if (dataTableProcessor.IsEnumColumn(i))
                {
                    stringBuilder.AppendFormat("\t\t\t\t\t{0} = ({1})binaryReader.Read7BitEncodedInt32();", dataTableProcessor.GetName(i), dataTableProcessor.GetLanguageKeyword(i)).AppendLine();
                    continue;
                }


                if (languageKeyword == "int" || languageKeyword == "uint" || languageKeyword == "long" || languageKeyword == "ulong")
                {
                    stringBuilder.AppendFormat("                    {0} = binaryReader.Read7BitEncoded{1}();", dataTableProcessor.GetName(i), dataTableProcessor.GetType(i).Name).AppendLine();
                }
                else
                {
                    stringBuilder.AppendFormat("                    {0} = binaryReader.Read{1}();", dataTableProcessor.GetName(i), dataTableProcessor.GetType(i).Name).AppendLine();
                }
            }

            stringBuilder
               .AppendLine("                }")
               .AppendLine("            }")
               .AppendLine()
               .AppendLine("            GeneratePropertyArray();")
               .AppendLine("            return true;")
               .Append("        }");

            return stringBuilder.ToString();
        }

        private static string GenerateDataTablePropertyArray(DataTableProcessor dataTableProcessor)
        {
            var propertyCollections = new List<PropertyCollection>();
            for (var i = 0; i < dataTableProcessor.RawColumnCount; i++)
            {
                // 注释列
                if (dataTableProcessor.IsCommentColumn(i))
                {
                    continue;
                }

                // 编号列
                if (dataTableProcessor.IsIdColumn(i))
                {
                    continue;
                }

                if (dataTableProcessor.IsEnumNameColumn(i))
                {
                    continue;
                }

                var name = dataTableProcessor.GetName(i);
                if (!EndWithNumberRegex.IsMatch(name))
                {
                    continue;
                }

                var propertyCollectionName = name;
                var id                     = int.Parse(EndWithNumberRegex.Match(name).Value);

                PropertyCollection propertyCollection = null;
                foreach (var pc in propertyCollections)
                {
                    if (pc.Name == propertyCollectionName)
                    {
                        propertyCollection = pc;
                        break;
                    }
                }

                if (propertyCollection == null)
                {
                    propertyCollection = new PropertyCollection(propertyCollectionName, dataTableProcessor.GetLanguageKeyword(i));
                    propertyCollections.Add(propertyCollection);
                }
                else
                {
                    if (propertyCollection.LanguageKeyword != dataTableProcessor.GetLanguageKeyword(i))
                    {
                        Debug.LogWarning($"GenerateDataTablePropertyArray failed, type mismatch, need type {propertyCollection.LanguageKeyword}," + $"{name} type is {dataTableProcessor.GetLanguageKeyword(i)}");
                        continue;
                    }
                }

                propertyCollection.AddItem(id, name);
            }

            var stringBuilder = new StringBuilder();
            var firstProperty = true;
            foreach (var propertyCollection in propertyCollections)
            {
                if (firstProperty)
                {
                    firstProperty = false;
                }
                else
                {
                    stringBuilder.AppendLine().AppendLine();
                }

                stringBuilder
                   .AppendFormat("        private KeyValuePair<int, {1}>[] m_{0} = null;", propertyCollection.Name, propertyCollection.LanguageKeyword).AppendLine()
                   .AppendLine()
                   .AppendFormat("        public int {0}Count", propertyCollection.Name).AppendLine()
                   .AppendLine("        {")
                   .AppendLine("            get")
                   .AppendLine("            {")
                   .AppendFormat("                return m_{0}.Length;", propertyCollection.Name).AppendLine()
                   .AppendLine("            }")
                   .AppendLine("        }")
                   .AppendLine()
                   .AppendFormat("        public {1} Get{0}(int id)", propertyCollection.Name, propertyCollection.LanguageKeyword).AppendLine()
                   .AppendLine("        {")
                   .AppendFormat("            foreach (KeyValuePair<int, {1}> i in m_{0})", propertyCollection.Name, propertyCollection.LanguageKeyword).AppendLine()
                   .AppendLine("            {")
                   .AppendLine("                if (i.Key == id)")
                   .AppendLine("                {")
                   .AppendLine("                    return i.Value;")
                   .AppendLine("                }")
                   .AppendLine("            }")
                   .AppendLine()
                   .AppendFormat(
                        "            throw new DEngineException(Utility.Text.Format(\"Get{0} with invalid id '{{0}}'.\", id.ToString()));", propertyCollection.Name).AppendLine()
                   .AppendLine("        }")
                   .AppendLine()
                   .AppendFormat("        public {1} Get{0}At(int index)", propertyCollection.Name, propertyCollection.LanguageKeyword).AppendLine()
                   .AppendLine("        {")
                   .AppendFormat("            if (index < 0 || index >= m_{0}.Length)", propertyCollection.Name)
                   .AppendLine()
                   .AppendLine("            {")
                   .AppendFormat(
                        "                throw new DEngineException(Utility.Text.Format(\"Get{0}At with invalid index '{{0}}'.\", index.ToString()));", propertyCollection.Name).AppendLine()
                   .AppendLine("            }")
                   .AppendLine()
                   .AppendFormat("            return m_{0}[index].Value;", propertyCollection.Name).AppendLine()
                   .Append("        }");
            }

            if (propertyCollections.Count > 0)
            {
                stringBuilder.AppendLine().AppendLine();
            }

            stringBuilder
               .AppendLine("        private void GeneratePropertyArray()")
               .AppendLine("        {");

            firstProperty = true;
            foreach (var propertyCollection in propertyCollections)
            {
                if (firstProperty)
                {
                    firstProperty = false;
                }
                else
                {
                    stringBuilder.AppendLine().AppendLine();
                }

                stringBuilder.AppendFormat("            m_{0} = new KeyValuePair<int, {1}>[]", propertyCollection.Name, propertyCollection.LanguageKeyword).AppendLine()
                             .AppendLine("            {");

                var itemCount = propertyCollection.ItemCount;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = propertyCollection.GetItem(i);
                    stringBuilder.AppendFormat("                new KeyValuePair<int, {0}>({1}, {2}),", propertyCollection.LanguageKeyword, item.Key.ToString(), item.Value).AppendLine();
                }

                stringBuilder.Append("            };");
            }

            stringBuilder
               .AppendLine()
               .Append("        }");

            return stringBuilder.ToString();
        }


        private sealed class PropertyCollection
        {
            private readonly List<KeyValuePair<int, string>> m_Items;

            public PropertyCollection(string name, string languageKeyword)
            {
                Name            = name;
                LanguageKeyword = languageKeyword;
                m_Items         = new List<KeyValuePair<int, string>>();
            }

            public string Name { get; }

            public string LanguageKeyword { get; }

            public int ItemCount => m_Items.Count;

            public KeyValuePair<int, string> GetItem(int index)
            {
                if (index < 0 || index >= m_Items.Count)
                {
                    throw new DEngineException(Utility.Text.Format("GetItem with invalid index '{0}'.", index.ToString()));
                }

                return m_Items[index];
            }

            public void AddItem(int id, string propertyName)
            {
                m_Items.Add(new KeyValuePair<int, string>(id, propertyName));
            }
        }
    }
}