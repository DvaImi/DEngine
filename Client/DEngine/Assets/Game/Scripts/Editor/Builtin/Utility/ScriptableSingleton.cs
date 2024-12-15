// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-15 11:40:26
// 版 本：1.0
// ========================================================

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T m_Instance;

        public static T Instance
        {
            get
            {
                if (!m_Instance)
                {
                    m_Instance = LoadOrCreate();
                    Save();
                }

                return m_Instance;
            }
        }

        public static T LoadOrCreate()
        {
            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                return EditorTools.LoadScriptableObject<T>(filePath);
            }
            else
            {
                Debug.LogError($"{nameof(ScriptableSingleton<T>)}: 请指定单例存档路径！ ");
            }

            return m_Instance;
        }

        /// <summary>
        /// 保存配置信息
        /// </summary>
        public static void Save()
        {
            if (!m_Instance)
            {
                Debug.LogError("Cannot save ScriptableSingleton: no instance!");
                return;
            }

            EditorUtility.SetDirty(m_Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        protected static string GetFilePath()
        {
            return typeof(T).GetCustomAttributes(inherit: true)
                .Cast<GameFilePathAttribute>()
                .FirstOrDefault(v => v != null)
                ?.filepath;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class GameFilePathAttribute : Attribute
    {
        internal string filepath;

        /// <summary>
        /// 单例存放路径
        /// </summary>
        /// <param name="path">相对 Project 路径</param>
        public GameFilePathAttribute(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid relative path (it is empty)");
            }

            if (path[0] == '/')
            {
                path = path[1..];
            }

            filepath = path;
        }
    }
}