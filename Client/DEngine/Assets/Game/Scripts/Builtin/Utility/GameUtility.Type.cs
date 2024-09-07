using System.Collections.Generic;
using System.Reflection;

namespace Game
{
    public static partial class GameUtility
    {
        public class Type
        {
            private static readonly string[] RuntimeAssemblyNames = { "DEngine.Runtime", "Assembly-CSharp", "Game.Runtime", };

            /// <summary>
            /// 在运行时程序集中获取指定基类的所有子类的名称。
            /// </summary>
            /// <param name="typeBase">基类类型。</param>
            /// <returns>指定基类的所有子类的名称。</returns>
            public static string[] GetRuntimeTypeNames(System.Type typeBase)
            {
                return GetTypeNames(typeBase, RuntimeAssemblyNames);
            }

            public static string[] GetTypeNames(System.Type typeBase, string[] assemblyNames)
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

                    System.Type[] types = assembly.GetTypes();
                    foreach (System.Type type in types)
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
}