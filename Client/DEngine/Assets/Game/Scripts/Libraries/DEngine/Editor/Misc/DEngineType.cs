using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace DEngine.Editor
{
    /// <summary>
    /// 类型相关的实用函数。
    /// </summary>
    internal static class DEngineType
    {
        private static readonly string s_ConfigurationPath = null;

        private static readonly List<string> RuntimeAssemblyNames = new()
        {
            "Assembly-CSharp",
            "DEngine.Runtime",
        };

        private static readonly List<string> RuntimeOrEditorAssemblyNames = new()
        {
            "Assembly-CSharp",
            "Assembly-CSharp-Editor",
            "DEngine.Editor",
            "DEngine.Runtime",
        };

        static DEngineType()
        {
            s_ConfigurationPath = GetConfigurationPath<DEngineTypeConfigPathAttribute>() ?? Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "DEngine/Configs/DEngineTypeSetting.xml"));
            if (!File.Exists(s_ConfigurationPath))
            {
                return;
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(s_ConfigurationPath);
                XmlNode xmlRoot = xmlDocument.SelectSingleNode("DEngine");
                XmlNode xmlTypeSetting = xmlRoot.SelectSingleNode("DEngineTypeSetting");

                XmlNode runtimeAssemblyNamesNode = xmlTypeSetting.SelectSingleNode("RuntimeAssemblyNames");
                XmlNode runtimeOrEditorAssemblyNamesNode = xmlTypeSetting.SelectSingleNode("RuntimeOrEditorAssemblyNames");
                XmlNodeList xmlNodeList = null;
                XmlNode xmlNode = null;

                xmlNodeList = runtimeAssemblyNamesNode.ChildNodes;
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    xmlNode = xmlNodeList.Item(i);
                    if (xmlNode.Name != "Assembly")
                    {
                        continue;
                    }

                    string runtimeAssembly = xmlNode.Attributes.GetNamedItem("Name").Value;
                    if (RuntimeAssemblyNames.Contains(runtimeAssembly))
                    {
                        continue;
                    }

                    RuntimeAssemblyNames.Add(runtimeAssembly);
                }

                xmlNodeList = runtimeOrEditorAssemblyNamesNode.ChildNodes;
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    xmlNode = xmlNodeList.Item(i);
                    if (xmlNode.Name != "Assembly")
                    {
                        continue;
                    }

                    string runtimeOrEditorAssembly = xmlNode.Attributes.GetNamedItem("Name").Value;
                    if (RuntimeOrEditorAssemblyNames.Contains(runtimeOrEditorAssembly))
                    {
                        continue;
                    }

                    RuntimeOrEditorAssemblyNames.Add(runtimeOrEditorAssembly);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        /// <summary>
        /// 获取配置路径。
        /// </summary>
        /// <typeparam name="T">配置类型。</typeparam>
        /// <returns>配置路径。</returns>
        internal static string GetConfigurationPath<T>() where T : ConfigPathAttribute
        {
            foreach (Type type in Utility.Assembly.GetTypes())
            {
                if (!type.IsAbstract || !type.IsSealed)
                {
                    continue;
                }

                foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (fieldInfo.FieldType == typeof(string) && fieldInfo.IsDefined(typeof(T), false))
                    {
                        return (string)fieldInfo.GetValue(null);
                    }
                }

                foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (propertyInfo.PropertyType == typeof(string) && propertyInfo.IsDefined(typeof(T), false))
                    {
                        return (string)propertyInfo.GetValue(null, null);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 在运行时程序集中获取指定基类的所有子类的名称。
        /// </summary>
        /// <param name="typeBase">基类类型。</param>
        /// <returns>指定基类的所有子类的名称。</returns>
        internal static string[] GetRuntimeTypeNames(Type typeBase)
        {
            return GetTypeNames(typeBase, RuntimeAssemblyNames);
        }

        /// <summary>
        /// 在运行时或编辑器程序集中获取指定基类的所有子类的名称。
        /// </summary>
        /// <param name="typeBase">基类类型。</param>
        /// <returns>指定基类的所有子类的名称。</returns>
        internal static string[] GetRuntimeOrEditorTypeNames(Type typeBase)
        {
            return GetTypeNames(typeBase, RuntimeOrEditorAssemblyNames);
        }

        private static string[] GetTypeNames(Type typeBase, List<string> assemblyNames)
        {
            List<string> typeNames = new List<string>();
            foreach (string assemblyName in assemblyNames)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch
                {
                    continue;
                }

                if (assembly == null)
                {
                    continue;
                }

                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsClass && !type.IsAbstract && typeBase.IsAssignableFrom(type))
                    {
                        typeNames.Add(type.FullName);
                    }
                }
            }

            typeNames.Sort();
            return typeNames.ToArray();
        }
    }
}