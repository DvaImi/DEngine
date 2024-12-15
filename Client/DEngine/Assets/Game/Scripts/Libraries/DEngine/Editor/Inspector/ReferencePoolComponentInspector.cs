using DEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using DEngine.Runtime;

namespace DEngine.Editor
{
    [CustomEditor(typeof(ReferencePoolComponent))]
    internal sealed class ReferencePoolComponentInspector : DEngineInspector
    {
        private readonly Dictionary<string, List<ReferencePoolInfo>> m_ReferencePoolInfos = new Dictionary<string, List<ReferencePoolInfo>>(StringComparer.Ordinal);
        private readonly HashSet<string> m_OpenedItems = new HashSet<string>();

        private SerializedProperty m_EnableStrictCheck = null;

        private bool m_ShowFullClassName = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ReferencePoolComponent t = (ReferencePoolComponent)target;

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                bool enableStrictCheck = EditorGUILayout.Toggle("Enable Strict Check", t.EnableStrictCheck);
                if (enableStrictCheck != t.EnableStrictCheck)
                {
                    t.EnableStrictCheck = enableStrictCheck;
                }

                EditorGUILayout.LabelField("Reference Pool Count", ReferencePool.Count.ToString());
                m_ShowFullClassName = EditorGUILayout.Toggle("Show Full Class Name", m_ShowFullClassName);
                m_ReferencePoolInfos.Clear();
                ReferencePoolInfo[] referencePoolInfos = ReferencePool.GetAllReferencePoolInfos();
                foreach (ReferencePoolInfo referencePoolInfo in referencePoolInfos)
                {
                    string assemblyName = referencePoolInfo.Type.Assembly.GetName().Name;
                    List<ReferencePoolInfo> results = null;
                    if (!m_ReferencePoolInfos.TryGetValue(assemblyName, out results))
                    {
                        results = new List<ReferencePoolInfo>();
                        m_ReferencePoolInfos.Add(assemblyName, results);
                    }

                    results.Add(referencePoolInfo);
                }

                foreach (KeyValuePair<string, List<ReferencePoolInfo>> assemblyReferencePoolInfo in m_ReferencePoolInfos)
                {
                    bool lastState = m_OpenedItems.Contains(assemblyReferencePoolInfo.Key);
                    bool currentState = EditorGUILayout.Foldout(lastState, assemblyReferencePoolInfo.Key);
                    if (currentState != lastState)
                    {
                        if (currentState)
                        {
                            m_OpenedItems.Add(assemblyReferencePoolInfo.Key);
                        }
                        else
                        {
                            m_OpenedItems.Remove(assemblyReferencePoolInfo.Key);
                        }
                    }

                    if (currentState)
                    {
                        EditorGUILayout.BeginVertical("box");
                        {
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(m_ShowFullClassName ? "Full Class Name" : "Class Name");
                                EditorGUILayout.LabelField("Unused", GUILayout.Width(60f));
                                EditorGUILayout.LabelField("Using", GUILayout.Width(60f));
                                EditorGUILayout.LabelField("Acquire", GUILayout.Width(60f));
                                EditorGUILayout.LabelField("Release", GUILayout.Width(60f));
                                EditorGUILayout.LabelField("Add", GUILayout.Width(60f));
                                EditorGUILayout.LabelField("Remove", GUILayout.Width(60f));
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();

                            //EditorGUILayout.LabelField(m_ShowFullClassName ? "Full Class Name" : "Class Name", "Unused\tUsing\tAcquire\tRelease\tAdd\tRemove");
                            assemblyReferencePoolInfo.Value.Sort(Comparison);
                            foreach (ReferencePoolInfo referencePoolInfo in assemblyReferencePoolInfo.Value)
                            {
                                DrawReferencePoolInfo(referencePoolInfo);
                            }

                            if (GUILayout.Button("Export CSV Data"))
                            {
                                string exportFileName = EditorUtility.SaveFilePanel("Export CSV Data", string.Empty, Utility.Text.Format("Reference Pool Data - {0}.csv", assemblyReferencePoolInfo.Key), string.Empty);
                                if (!string.IsNullOrEmpty(exportFileName))
                                {
                                    try
                                    {
                                        int index = 0;
                                        string[] data = new string[assemblyReferencePoolInfo.Value.Count + 1];
                                        data[index++] = "Class Name,Full Class Name,Unused,Using,Acquire,Release,Add,Remove";
                                        foreach (ReferencePoolInfo referencePoolInfo in assemblyReferencePoolInfo.Value)
                                        {
                                            data[index++] = Utility.Text.Format("{0},{1},{2},{3},{4},{5},{6},{7}", referencePoolInfo.Type.Name, referencePoolInfo.Type.FullName, referencePoolInfo.UnusedReferenceCount, referencePoolInfo.UsingReferenceCount, referencePoolInfo.AcquireReferenceCount, referencePoolInfo.ReleaseReferenceCount, referencePoolInfo.AddReferenceCount, referencePoolInfo.RemoveReferenceCount);
                                        }

                                        File.WriteAllLines(exportFileName, data, Encoding.UTF8);
                                        Debug.Log(Utility.Text.Format("Export reference pool CSV data to '{0}' success.", exportFileName));
                                    }
                                    catch (Exception exception)
                                    {
                                        Debug.LogError(Utility.Text.Format("Export reference pool CSV data to '{0}' failure, exception is '{1}'.", exportFileName, exception));
                                    }
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.Separator();
                    }
                }
            }
            else
            {
                EditorGUILayout.PropertyField(m_EnableStrictCheck);
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        private void OnEnable()
        {
            m_EnableStrictCheck = serializedObject.FindProperty("m_EnableStrictCheck");
        }

        private void DrawReferencePoolInfo(ReferencePoolInfo referencePoolInfo)
        {
            //EditorGUILayout.LabelField(m_ShowFullClassName ? referencePoolInfo.Type.FullName : referencePoolInfo.Type.Name, Utility.Text.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", referencePoolInfo.UnusedReferenceCount, referencePoolInfo.UsingReferenceCount, referencePoolInfo.AcquireReferenceCount, referencePoolInfo.ReleaseReferenceCount, referencePoolInfo.AddReferenceCount, referencePoolInfo.RemoveReferenceCount));
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(m_ShowFullClassName ? referencePoolInfo.Type.FullName : referencePoolInfo.Type.Name);
                EditorGUILayout.LabelField(referencePoolInfo.UnusedReferenceCount.ToString(), GUILayout.Width(60f));
                EditorGUILayout.LabelField(referencePoolInfo.UsingReferenceCount.ToString(), GUILayout.Width(60f));
                EditorGUILayout.LabelField(referencePoolInfo.AcquireReferenceCount.ToString(), GUILayout.Width(60f));
                EditorGUILayout.LabelField(referencePoolInfo.ReleaseReferenceCount.ToString(), GUILayout.Width(60f));
                EditorGUILayout.LabelField(referencePoolInfo.AddReferenceCount.ToString(), GUILayout.Width(60f));
                EditorGUILayout.LabelField(referencePoolInfo.RemoveReferenceCount.ToString(), GUILayout.Width(60f));
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        private int Comparison(ReferencePoolInfo a, ReferencePoolInfo b)
        {
            return m_ShowFullClassName ? string.Compare(a.Type.FullName, b.Type.FullName, StringComparison.Ordinal) : string.Compare(a.Type.Name, b.Type.Name, StringComparison.Ordinal);
        }
    }
}