using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DEngine;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        public static class DataProcessorUtility
        {
            private static readonly IDictionary<string, DataProcessor> s_DataProcessors = new SortedDictionary<string, DataProcessor>();

            static DataProcessorUtility()
            {
                RefreshTypes();
            }

            public static string CodeTemplate { get; private set; }

            public static void RefreshTypes()
            {
                s_DataProcessors.Clear();
                var dataProcessorBaseType = typeof(DataProcessor);

                var types = Assembly.GetExecutingAssembly().GetTypes();
                var addList = new List<DataProcessor>();
                foreach (var t in types)
                {
                    if (!t.IsClass || t.IsAbstract || t.ContainsGenericParameters) continue;

                    if (dataProcessorBaseType.IsAssignableFrom(t))
                    {
                        DataProcessor dataProcessor = null;
                        dataProcessor = (DataProcessor)Activator.CreateInstance(t);
                        if (dataProcessor.IsEnum)
                        {
                            continue;
                        }

                        foreach (var typeString in dataProcessor.GetTypeStrings())
                        {
                            s_DataProcessors.Add(typeString.ToLower(), dataProcessor);
                        }

                        addList.Add(dataProcessor);
                    }
                }

                AddEnumType(addList);
                AddListType(addList);
                AddArrayType(addList);
                AddDictionary(addList);
            }

            public static bool SetCodeTemplate(string codeTemplateFileName, Encoding encoding)
            {
                try
                {
                    CodeTemplate = File.ReadAllText(codeTemplateFileName, encoding);
                    Debug.Log(Utility.Text.Format("Set code template '{0}' success.", codeTemplateFileName));
                    return true;
                }
                catch (Exception exception)
                {
                    Debug.LogError(Utility.Text.Format("Set code template '{0}' failure, exception is '{1}'.", codeTemplateFileName, exception.ToString()));
                    return false;
                }
            }

            private static void AddEnumType(List<DataProcessor> addList)
            {
                foreach (var assemblyName in DataTableSetting.Instance.AssemblyNames)
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.Load(assemblyName);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarning($"Error loading assembly: {assemblyName}. Exception: {ex.Message}");
                        continue;
                    }

                    var types = assembly?.GetTypes();
                    if (types == null) continue;

                    foreach (var type in types)
                    {
                        // 跳过开放的泛型类型
                        if (!type.IsEnum || type.ContainsGenericParameters)
                        {
                            continue;
                        }

                        Type enumProcessorType = typeof(EnumProcessor<>).MakeGenericType(type);
                        DataProcessor dataProcessor = (DataProcessor)Activator.CreateInstance(enumProcessorType);

                        foreach (var typeString in dataProcessor.GetTypeStrings())
                        {
                            if (s_DataProcessors.TryGetValue(typeString.ToLower(), out var existingProcessor))
                            {
                                var stringBuilder = new StringBuilder(256)
                                    .AppendLine($"程序集:{type.Assembly.GetName().Name} 存在同名枚举:{type.FullName}");

                                if (existingProcessor.GetType().GetProperty("EnumType")?.GetValue(existingProcessor) is Type repeatType)
                                {
                                    stringBuilder.AppendLine($"程序集:{repeatType.Assembly.GetName().Name} 存在同名枚举:{type.FullName}");
                                }

                                throw new Exception("不同程序集中存在同名枚举,请修改后重试.\n" + stringBuilder);
                            }

                            s_DataProcessors.Add(typeString.ToLower(), dataProcessor);
                        }

                        addList.Add(dataProcessor);
                    }
                }
            }

            private static void AddArrayType(List<DataProcessor> addList)
            {
                var dataProcessorBaseType = typeof(DataProcessor);

                var type = typeof(ArrayProcessor<,>);

                for (var i = 0; i < addList.Count; i++)
                {
                    Type dataProcessorType = addList[i].GetType();
                    if (!dataProcessorType.HasImplementedRawGeneric(typeof(GenericDataProcessor<>))) continue;

                    var memberInfo = dataProcessorType.BaseType;

                    if (memberInfo != null)
                    {
                        Type[] typeArgs =
                        {
                            dataProcessorType,
                            memberInfo.GenericTypeArguments[0]
                        };
                        var arrayType = type.MakeGenericType(typeArgs);
                        if (dataProcessorBaseType.IsAssignableFrom(arrayType))
                        {
                            var dataProcessor = (DataProcessor)Activator.CreateInstance(arrayType);
                            var tDataProcessor = addList[i];
                            foreach (var typeString in dataProcessor.GetTypeStrings())
                            foreach (var tTypeString in tDataProcessor.GetTypeStrings())
                            {
                                var key = DEngine.Utility.Text.Format(typeString.ToLower(), tTypeString);
                                s_DataProcessors.Add(key, dataProcessor);
                            }
                        }
                    }
                }
            }

            private static void AddListType(List<DataProcessor> addList)
            {
                var dataProcessorBaseType = typeof(DataProcessor);

                var type = typeof(ListProcessor<,>);

                for (var i = 0; i < addList.Count; i++)
                {
                    Type dataProcessorType = addList[i].GetType();

                    if (!dataProcessorType.HasImplementedRawGeneric(typeof(GenericDataProcessor<>))) continue;

                    var memberInfo = dataProcessorType.BaseType;

                    if (memberInfo != null)
                    {
                        Type[] typeArgs =
                        {
                            dataProcessorType,
                            memberInfo.GenericTypeArguments[0]
                        };
                        var listType = type.MakeGenericType(typeArgs);
                        if (dataProcessorBaseType.IsAssignableFrom(listType))
                        {
                            var dataProcessor =
                                (DataProcessor)Activator.CreateInstance(listType);
                            foreach (var typeString in dataProcessor.GetTypeStrings())
                            foreach (var tTypeString in addList[i].GetTypeStrings())
                            {
                                var key = DEngine.Utility.Text.Format(typeString.ToLower(), tTypeString);
                                s_DataProcessors.Add(key, dataProcessor);
                            }
                        }
                    }
                }
            }

            private static void AddDictionary(List<DataProcessor> addList)
            {
                var dataProcessorBaseType = typeof(DataProcessor);
                var type = typeof(DictionaryProcessor<,,,>);
                var list = new List<DataProcessor>();
                for (var i = 0; i < addList.Count; i++)
                {
                    Type dataProcessorType = addList[i].GetType();

                    if (!dataProcessorType.HasImplementedRawGeneric(typeof(GenericDataProcessor<>))) continue;
                    var memberInfo = dataProcessorType.BaseType;

                    if (memberInfo != null) list.Add(addList[i]);
                }


                var keyValueList = PermutationAndCombination<DataProcessor>.GetCombination(list.ToArray(), 2).ToList();
                var reverseList = keyValueList.Select(types => new[] { types[1], types[0] }).ToList();
                keyValueList.AddRange(reverseList);
                foreach (var value in list) keyValueList.Add(new[] { value, value });
                foreach (var keyValue in keyValueList)
                {
                    var keyType = keyValue[0].GetType().BaseType;
                    var valueType = keyValue[1].GetType().BaseType;
                    if (keyType != null && valueType != null)
                    {
                        Type[] typeArgs =
                        {
                            keyValue[0].GetType(),
                            keyValue[1].GetType(),
                            keyType.GenericTypeArguments[0],
                            valueType.GenericTypeArguments[0]
                        };
                        var dictionaryType = type.MakeGenericType(typeArgs);
                        if (dataProcessorBaseType.IsAssignableFrom(dictionaryType))
                        {
                            var dataProcessor = (DataProcessor)Activator.CreateInstance(dictionaryType);
                            foreach (var typeString in dataProcessor.GetTypeStrings())
                            {
                                foreach (var key in keyValue[0].GetTypeStrings())
                                {
                                    foreach (var value in keyValue[1].GetTypeStrings())
                                    {
                                        var str = DEngine.Utility.Text.Format(typeString.ToLower(), key, value);
                                        s_DataProcessors.Add(str, dataProcessor);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public static DataProcessor GetDataProcessor(string type)
            {
                type ??= string.Empty;
                if (s_DataProcessors.TryGetValue(type.ToLower(), out var dataProcessor))
                {
                    return dataProcessor;
                }

                throw new DEngineException(DEngine.Utility.Text.Format("Not supported data processor type '{0}'.", type));
            }
        }
    }
}