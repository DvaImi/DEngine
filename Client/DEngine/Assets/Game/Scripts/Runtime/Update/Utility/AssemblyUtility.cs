// ========================================================
// 作者：Dvalmi 
// 创建时间：2024-04-14 11:51:23
// ========================================================
// ========================================================
// 作者：Dvalmi 
// 创建时间：2024-04-14 11:48:58
// ========================================================

using System;
using System.Collections.Generic;
using DEngine;

namespace Game.Update
{
    public static class AssemblyUtility
    {
        private static System.Reflection.Assembly[] s_Assemblies = null;
        private static readonly Dictionary<string, Type> CachedTypes = new(StringComparer.Ordinal);

        public static void Initialize()
        {
            s_Assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// 获取已加载的程序集。
        /// </summary>
        /// <returns>已加载的程序集。</returns>
        public static System.Reflection.Assembly[] GetAssemblies()
        {
            return s_Assemblies;
        }

        /// <summary>
        /// 获取已加载的程序集中的所有类型。
        /// </summary>
        /// <returns>已加载的程序集中的所有类型。</returns>
        public static Type[] GetTypes()
        {
            List<Type> results = new List<Type>();
            foreach (System.Reflection.Assembly assembly in s_Assemblies)
            {
                results.AddRange(assembly.GetTypes());
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取已加载的程序集中的所有类型。
        /// </summary>
        /// <param name="results">已加载的程序集中的所有类型。</param>
        public static void GetTypes(List<Type> results)
        {
            if (results == null)
            {
                throw new DEngineException("Results is invalid.");
            }

            results.Clear();
            foreach (System.Reflection.Assembly assembly in s_Assemblies)
            {
                results.AddRange(assembly.GetTypes());
            }
        }

        /// <summary>
        /// 获取已加载的程序集中的指定类型。
        /// </summary>
        /// <param name="typeName">要获取的类型名。</param>
        /// <returns>已加载的程序集中的指定类型。</returns>
        public static Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new DEngineException("Type name is invalid.");
            }

            Type type = null;
            if (CachedTypes.TryGetValue(typeName, out type))
            {
                return type;
            }

            type = Type.GetType(typeName);
            if (type != null)
            {
                CachedTypes.Add(typeName, type);
                return type;
            }

            foreach (System.Reflection.Assembly assembly in s_Assemblies)
            {
                type = Type.GetType(Utility.Text.Format("{0}, {1}", typeName, assembly.FullName));
                if (type != null)
                {
                    CachedTypes.Add(typeName, type);
                    return type;
                }
            }

            return null;
        }
    }
}