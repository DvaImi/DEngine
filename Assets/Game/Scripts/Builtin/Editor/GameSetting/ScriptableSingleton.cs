// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-15 11:40:26
// 版 本：1.0
// ========================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
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
                    LoadOrCreate();
                }
                return m_Instance;
            }
        }
        public static T LoadOrCreate()
        {
            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                var arr = InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                m_Instance = arr.Length > 0 ? arr[0] as T : m_Instance ?? CreateInstance<T>();
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
        /// <param name="saveAsText"></param>
        public static void Save(bool saveAsText = true)
        {
            if (!m_Instance)
            {
                Debug.LogError("Cannot save ScriptableSingleton: no instance!");
                return;
            }

            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                string directoryName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                UnityEngine.Object[] obj = new T[1] { m_Instance };
                InternalEditorUtility.SaveToSerializedFileAndForget(obj, filePath, saveAsText);
            }
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
