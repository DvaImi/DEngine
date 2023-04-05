// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-04 20:49:55
// 版 本：1.0
// ========================================================
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using GameFramework;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor;
using UnityGameFramework.Editor.ResourceTools;
using Object = UnityEngine.Object;

namespace Dvalmi.Editor.ResourceTools
{
    public class EditorResourceCollection : EditorWindow
    {
        private int m_LoadTypeIndex;

        private int LoadTypeIndex
        {
            get
            {
                return EditorPrefs.GetInt("LoadTypeIndex", 1);
            }
            set
            {
                m_LoadTypeIndex = value;
                EditorPrefs.SetInt("LoadTypeIndex", m_LoadTypeIndex);
            }
        }

        private bool m_Packaged;
        public bool Packed
        {
            get
            {
                return EditorPrefs.GetBool("Packaged", false);
            }
            set
            {
                m_Packaged = value;
                EditorPrefs.SetBool("Packaged", m_Packaged);
            }
        }
        public string[] LoadTypeNames { get; private set; }

        private string m_ResourceCollectionPath;

        [MenuItem("Dvalmi/Resources", false, 0)]
        public static void ModifyLoadType()
        {
            EditorResourceCollection window = GetWindow<EditorResourceCollection>("Resources", true);
            window.minSize = new Vector2(800f, 300f);
        }

        private void OnEnable()
        {
            m_ResourceCollectionPath = GetResourceCollectionPath<ResourceCollectionConfigPathAttribute>() ?? Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "GameFramework/Configs/ResourceCollection.xml"));
            LoadTypeNames = Enum.GetNames(typeof(LoadType));
        }

        private void OnGUI()
        {
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("一键修改资源加载类型", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                int selectLoadTypeIndex = EditorGUILayout.Popup("选择LoadType", LoadTypeIndex, LoadTypeNames);
                if (selectLoadTypeIndex != LoadTypeIndex)
                {
                    LoadTypeIndex = selectLoadTypeIndex;
                }
                EditorGUILayout.LabelField("Is Packaged", EditorStyles.boldLabel);
                Packed = EditorGUILayout.Toggle(Packed);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("修改加载类型");
                    if (GUILayout.Button("Modify", GUILayout.Width(100)))
                    {
                        Modify();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void Modify()
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(m_ResourceCollectionPath);
                XmlNode xmlRoot = xmlDocument.SelectSingleNode("UnityGameFramework");
                XmlNode xmlCollection = xmlRoot.SelectSingleNode("ResourceCollection");
                XmlNode xmlResources = xmlCollection.SelectSingleNode("Resources");
                XmlNodeList xmlNodeList = xmlResources.ChildNodes;
                int count = xmlNodeList.Count;
                for (int i = 0; i < count; i++)
                {
                    XmlNode xmlNode = xmlNodeList.Item(i);
                    if (xmlNode.Name != "Resource")
                    {
                        continue;
                    }

                    if (xmlNode.Attributes.GetNamedItem("LoadType") != null)
                    {
                        xmlNode.Attributes["LoadType"].Value = LoadTypeIndex.ToString();
                        Debug.Log(Utility.Text.Format("Modify resource '{0}' LoadType =>{1}.", xmlNode.Attributes["Name"].Value, LoadTypeIndex.ToString()));
                    }

                    if (xmlNode.Attributes.GetNamedItem("Packed") != null)
                    {
                        xmlNode.Attributes["Packed"].Value = Packed.ToString();
                        Debug.Log(Utility.Text.Format("Modify resource '{0}' Packed =>{1}.", xmlNode.Attributes["Name"].Value, Packed.ToString()));
                    }
                }

                xmlDocument.Save(m_ResourceCollectionPath);
                string absolutePath = Utility.Path.GetRegularPath($"Assets/{m_ResourceCollectionPath.Replace(Application.dataPath, null)}");
                Object resourceCollection = AssetDatabase.LoadAssetAtPath(absolutePath, typeof(TextAsset));
                EditorUtility.SetDirty(resourceCollection);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
                EditorGUIUtility.PingObject(resourceCollection);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            Debug.Log("Save configuration success.");

        }

        private static string GetResourceCollectionPath<T>() where T : ConfigPathAttribute
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
    }
}
