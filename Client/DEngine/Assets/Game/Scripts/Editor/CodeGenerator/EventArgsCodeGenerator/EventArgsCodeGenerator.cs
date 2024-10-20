using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// 事件参数类代码生成器
    /// </summary>
    public class EventArgsCodeGenerator : EditorWindow
    {
        [MenuItem("Game/CodeGenerator/EventArgs")]
        public static void OpenAutoGenWindow()
        {
            EventArgsCodeGenerator window = GetWindow<EventArgsCodeGenerator>(true, "EventArgs Code Generator");
            window.minSize = new Vector2(600f, 600f);
        }

        /// <summary>
        /// 事件参数数据列表
        /// </summary>
        private readonly List<EventArgsData> m_EventArgsDatas = new();

        /// <summary>
        /// 是否是热更新层事件
        /// </summary>
        private bool m_IsUpdateLayerEvent = false;

        /// <summary>
        /// 事件参数类名
        /// </summary>
        private string m_ClassName;

        //事件代码生成后的路径
        private const string EventCodePath = "Assets/Game/Scripts/Runtime/Builtin/Event/EventArgs";
        private const string UpdateEventCodePath = "Assets/Game/Scripts/Runtime/Update/Event/EventArgs";

        private string m_SubDirectory;
        private GUIContent m_GenerateContent;

        private void OnEnable()
        {
            m_EventArgsDatas.Clear();
            m_GenerateContent = EditorGUIUtility.TrTextContentWithIcon("Generate", "生成事件参数类代码", "Assembly Icon");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("事件参数类名：", GUILayout.Width(140f));
                m_ClassName = EditorGUILayout.TextField(m_ClassName);
                GUI.enabled = false;
                EditorGUILayout.TextField("EventArgs");
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("热更新层事件：", GUILayout.Width(140f));
                m_IsUpdateLayerEvent = EditorGUILayout.Toggle(m_IsUpdateLayerEvent);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("自动生成的代码路径：", GUILayout.Width(140f));
                EditorGUILayout.LabelField(m_IsUpdateLayerEvent ? UpdateEventCodePath : EventCodePath);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("子路径：", GUILayout.Width(140f));
                m_SubDirectory = EditorGUILayout.TextField(m_SubDirectory);
            }
            EditorGUILayout.EndHorizontal();

            //绘制事件参数相关按钮
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("添加事件参数", GUILayout.Width(140f)))
                {
                    m_EventArgsDatas.Add(new EventArgsData(null, null));
                }

                if (GUILayout.Button("删除所有事件参数", GUILayout.Width(140f)))
                {
                    m_EventArgsDatas.Clear();
                }

                if (GUILayout.Button("删除空事件参数", GUILayout.Width(140f)))
                {
                    for (int i = m_EventArgsDatas.Count - 1; i >= 0; i--)
                    {
                        EventArgsData data = m_EventArgsDatas[i];
                        if (string.IsNullOrWhiteSpace(data.Name))
                        {
                            m_EventArgsDatas.RemoveAt(i);
                        }
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            if (m_EventArgsDatas.Count == 0)
            {
                EditorGUILayout.HelpBox("参数列表为空", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("参数列表：", EditorStyles.boldLabel);
            }
            
            EditorGUILayout.BeginVertical("box");
            {
                //绘制事件参数数据
                for (int i = 0; i < m_EventArgsDatas.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EventArgsData data = m_EventArgsDatas[i];
                        EditorGUILayout.LabelField("参数类型：", GUILayout.Width(70f));
                        data.TypeEnum = (EventArgType)EditorGUILayout.EnumPopup(data.TypeEnum, GUILayout.Width(100f));
                        switch (data.TypeEnum)
                        {
                            case EventArgType.Object:
                            case EventArgType.Int:
                            case EventArgType.Float:
                            case EventArgType.Bool:
                            case EventArgType.Char:
                            case EventArgType.String:
                                data.Type = data.TypeEnum.ToString().ToLower();
                                break;

                            case EventArgType.UnityObject:
                                data.Type = "UnityEngine.Object";
                                break;

                            case EventArgType.Other:
                                data.Type = EditorGUILayout.TextField(data.Type, GUILayout.Width(140f));
                                break;

                            default:
                                data.Type = data.TypeEnum.ToString();
                                break;
                        }

                        EditorGUILayout.LabelField("参数字段名：", GUILayout.Width(70f));
                        data.Name = EditorGUILayout.TextField(data.Name, GUILayout.Width(140f));
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(m_GenerateContent, GUILayout.Height(30)))
                {
                    GenEventCode();
                    AssetDatabase.Refresh();
                }
            }
            GUILayout.EndHorizontal();
        }

        private void GenEventCode()
        {
            string codePath = string.IsNullOrWhiteSpace(m_SubDirectory) ? Path.Combine(m_IsUpdateLayerEvent ? UpdateEventCodePath : EventCodePath) : Path.Combine(m_IsUpdateLayerEvent ? UpdateEventCodePath : EventCodePath, m_SubDirectory);
            string nameSpace = m_IsUpdateLayerEvent ? "Game.Update" : "Game";
            string baseClass = "GameEventArgsBase<{0}>";

            if (!Directory.Exists($"{codePath}/"))
            {
                Directory.CreateDirectory($"{codePath}/");
            }

            if (!m_ClassName.EndsWith("EventArgs"))
            {
                m_ClassName += "EventArgs";
            }

            using (StreamWriter sw = new StreamWriter($"{codePath}/{m_ClassName}.cs"))
            {
                sw.WriteLine("using DEngine;");
                sw.WriteLine("using UnityEngine;");
                sw.WriteLine("using DEngine.Event;");
                sw.WriteLine("using Game.Event;");
                sw.WriteLine("");

                sw.WriteLine("//自动生成于：" + DateTime.Now);

                //命名空间
                sw.WriteLine("namespace " + nameSpace);
                sw.WriteLine("{");
                sw.WriteLine("");

                //类名
                sw.WriteLine($"\tpublic class {m_ClassName} : {string.Format(baseClass, m_ClassName)}");
                sw.WriteLine("\t{");
                sw.WriteLine("");

                //事件参数
                for (int i = 0; i < m_EventArgsDatas.Count; i++)
                {
                    EventArgsData data = m_EventArgsDatas[i];
                    data.Name = data.Name[0].ToString().ToUpper() + data.Name[1..];
                    sw.WriteLine($"\t\tpublic {data.Type} {data.Name}");
                    sw.WriteLine("\t\t{");
                    sw.WriteLine("\t\t\tget;");
                    sw.WriteLine("\t\t\tprivate set;");
                    sw.WriteLine("\t\t}");
                    sw.WriteLine("");
                }

                //清空参数数据方法
                sw.WriteLine($"\t\tpublic override void Clear()");
                sw.WriteLine("\t\t{");
                for (int i = 0; i < m_EventArgsDatas.Count; i++)
                {
                    EventArgsData data = m_EventArgsDatas[i];
                    sw.WriteLine($"\t\t\t{data.Name} = default({data.Type});");
                }

                sw.WriteLine("\t\t}");
                sw.WriteLine("");

                //填充参数数据方法
                sw.Write($"\t\tpublic {m_ClassName} Fill(");
                for (int i = 0; i < m_EventArgsDatas.Count; i++)
                {
                    EventArgsData data = m_EventArgsDatas[i];
                    sw.Write($"{data.Type} {data.Name.ToLower()}");
                    if (i != m_EventArgsDatas.Count - 1)
                    {
                        sw.Write(",");
                    }
                }

                sw.WriteLine(")");
                sw.WriteLine("\t\t{");
                for (int i = 0; i < m_EventArgsDatas.Count; i++)
                {
                    EventArgsData data = m_EventArgsDatas[i];
                    sw.WriteLine($"\t\t\t{data.Name} = {data.Name.ToLower()};");
                }

                sw.WriteLine("\t\t\treturn this;");
                sw.WriteLine("\t\t}");

                //创建参数类型方法
                sw.WriteLine($"\t\tpublic static {m_ClassName} Creat()");
                sw.WriteLine("\t\t{");

                sw.WriteLine($"\t\t\t return ReferencePool.Acquire<{m_ClassName}>();");

                sw.WriteLine("\t\t}");
                sw.WriteLine("");

                sw.WriteLine("\t}");
                sw.WriteLine("}");
            }
        }
    }
}