using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DEngine;
using DEngine.Editor;
using Game.Archive;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Archive
{
    [CustomEditor(typeof(ArchiveComponent))]
    public class ArchiveComponentInspector : DEngineInspector
    {
        private readonly HashSet<string> m_OpenedItems = new HashSet<string>();

        private const string NoneOptionName = "<None>";
        private SerializedProperty m_ArchiveSerializerTypeName = null;
        private SerializedProperty m_ArchiveHelperTypeName = null;
        private SerializedProperty m_EncryptorTypeName = null;
        private SerializedProperty m_MaxSlotCount = null;
        private SerializedProperty m_UserIdentifier = null;
        private SerializedProperty m_UserEncryptor = null;

        private string[] m_ArchiveSerializerTypeNames = null;
        private int m_ArchiveSerializerTypeNameIndex = 0;
        private string[] m_ArchiveHelperTypeNames = null;
        private int m_ArchiveHelperTypeNameIndex = 0;
        private string[] m_EncryptorTypeNames = null;
        private int m_EncryptorTypeNameIndex = 0;
        private int m_PathOptionIndex;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ArchiveComponent t = (ArchiveComponent)target;


            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
                {
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.PropertyField(m_UserEncryptor);

                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Archive Helpers", EditorStyles.boldLabel);

                    int serializerTypeNameIndex = EditorGUILayout.Popup("Serializer Helper", m_ArchiveSerializerTypeNameIndex, m_ArchiveSerializerTypeNames);
                    if (serializerTypeNameIndex != m_ArchiveSerializerTypeNameIndex)
                    {
                        m_ArchiveSerializerTypeNameIndex = serializerTypeNameIndex;
                        m_ArchiveSerializerTypeName.stringValue = serializerTypeNameIndex <= 0 ? null : m_ArchiveSerializerTypeNames[serializerTypeNameIndex];
                    }

                    if (serializerTypeNameIndex == 0)
                    {
                        var displayName = FieldNameForDisplay("Serializer");
                        EditorGUILayout.HelpBox(Utility.Text.Format("You must set a {0} Helper.", displayName), MessageType.Error);
                    }

                    int archiveHelperTypeNameIndex = EditorGUILayout.Popup("Archive Helper", m_ArchiveHelperTypeNameIndex, m_ArchiveHelperTypeNames);
                    if (archiveHelperTypeNameIndex != m_ArchiveHelperTypeNameIndex)
                    {
                        m_ArchiveHelperTypeNameIndex = archiveHelperTypeNameIndex;
                        m_ArchiveHelperTypeName.stringValue = archiveHelperTypeNameIndex <= 0 ? null : m_ArchiveHelperTypeNames[archiveHelperTypeNameIndex];
                    }

                    if (archiveHelperTypeNameIndex == 0)
                    {
                        var displayName = FieldNameForDisplay("Archive");
                        EditorGUILayout.HelpBox(Utility.Text.Format("You must set a {0} Helper.", displayName), MessageType.Error);
                    }

                    if (m_UserEncryptor.boolValue)
                    {
                        int encryptorTypeNameIndex = EditorGUILayout.Popup("Encryptor Helper", m_EncryptorTypeNameIndex, m_EncryptorTypeNames);
                        if (encryptorTypeNameIndex != m_EncryptorTypeNameIndex)
                        {
                            m_EncryptorTypeNameIndex = encryptorTypeNameIndex;
                            m_EncryptorTypeName.stringValue = encryptorTypeNameIndex <= 0 ? null : m_EncryptorTypeNames[encryptorTypeNameIndex];
                        }

                        if (encryptorTypeNameIndex == 0)
                        {
                            var displayName = FieldNameForDisplay("Encryptor");
                            EditorGUILayout.HelpBox(Utility.Text.Format("You must set a {0} Helper.", displayName), MessageType.Error);
                        }
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(5);

                int maxSlotCount = EditorGUILayout.IntSlider("MaxSlot", m_MaxSlotCount.intValue, 1, 16);
                if (maxSlotCount != m_MaxSlotCount.intValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.MaxSlotCount = maxSlotCount;
                    }
                    else
                    {
                        m_MaxSlotCount.intValue = maxSlotCount;
                    }
                }

                EditorGUILayout.PropertyField(m_UserIdentifier);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying)
            {
                if (t.CurrentSlot != null)
                {
                    EditorGUILayout.LabelField("Current Slot", t.CurrentSlot.Name);
                    EditorGUILayout.Space(5);

                    ArchiveSlot[] archiveSlots = t.GetArchiveSlots();
                    foreach (var archiveSlot in archiveSlots)
                    {
                        DrawArchiveSlot(archiveSlot);
                    }
                }
            }

            EditorGUILayout.Space(10);
            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            m_ArchiveSerializerTypeName = serializedObject.FindProperty("m_ArchiveSerializerTypeName");
            m_ArchiveHelperTypeName = serializedObject.FindProperty("m_ArchiveHelperTypeName");
            m_EncryptorTypeName = serializedObject.FindProperty("m_EncryptorTypeName");
            m_MaxSlotCount = serializedObject.FindProperty("m_MaxSlotCount");
            m_UserIdentifier = serializedObject.FindProperty("m_UserIdentifier");
            m_UserEncryptor = serializedObject.FindProperty("m_UserEncryptor");
            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            List<string> archiveSerializerTypeNames = new List<string>
            {
                NoneOptionName
            };

            archiveSerializerTypeNames.AddRange(GameType.GetRuntimeTypeNames(typeof(IArchiveSerializerHelper)));
            m_ArchiveSerializerTypeNames = archiveSerializerTypeNames.ToArray();
            m_ArchiveSerializerTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_ArchiveSerializerTypeName.stringValue))
            {
                m_ArchiveSerializerTypeNameIndex = archiveSerializerTypeNames.IndexOf(m_ArchiveSerializerTypeName.stringValue);
                if (m_ArchiveSerializerTypeNameIndex <= 0)
                {
                    m_ArchiveSerializerTypeNameIndex = 0;
                    m_ArchiveSerializerTypeName.stringValue = null;
                }
            }

            List<string> archiveHelperTypeNames = new List<string>
            {
                NoneOptionName
            };

            archiveHelperTypeNames.AddRange(GameType.GetRuntimeTypeNames(typeof(IArchiveHelper)));
            m_ArchiveHelperTypeNames = archiveHelperTypeNames.ToArray();
            m_ArchiveHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_ArchiveHelperTypeName.stringValue))
            {
                m_ArchiveHelperTypeNameIndex = archiveHelperTypeNames.IndexOf(m_ArchiveHelperTypeName.stringValue);
                if (m_ArchiveHelperTypeNameIndex <= 0)
                {
                    m_ArchiveHelperTypeNameIndex = 0;
                    m_ArchiveHelperTypeName.stringValue = null;
                }
            }

            List<string> encryptorTypeNames = new List<string>
            {
                NoneOptionName
            };

            encryptorTypeNames.AddRange(GameType.GetRuntimeTypeNames(typeof(IEncryptorHelper)));
            m_EncryptorTypeNames = encryptorTypeNames.ToArray();
            m_EncryptorTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_EncryptorTypeName.stringValue))
            {
                m_EncryptorTypeNameIndex = encryptorTypeNames.IndexOf(m_EncryptorTypeName.stringValue);
                if (m_EncryptorTypeNameIndex <= 0)
                {
                    m_EncryptorTypeNameIndex = 0;
                    m_EncryptorTypeName.stringValue = null;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private string FieldNameForDisplay(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return string.Empty;
            }

            string str = Regex.Replace(fieldName, @"^m_", string.Empty);
            str = Regex.Replace(str, @"((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", @" $1").TrimStart();
            return str;
        }

        private void DrawArchiveSlot(ArchiveSlot archiveSlot)
        {
            bool lastState = m_OpenedItems.Contains(archiveSlot.Name);
            bool currentState = EditorGUILayout.Foldout(lastState, archiveSlot.Name);
            if (currentState != lastState)
            {
                if (currentState)
                {
                    m_OpenedItems.Add(archiveSlot.Name);
                }
                else
                {
                    m_OpenedItems.Remove(archiveSlot.Name);
                }
            }

            if (lastState)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField(archiveSlot.Name, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Identifier", archiveSlot.Identifier);
                    EditorGUILayout.LabelField("UserEncryptor", archiveSlot.UserEncryptor.ToString());
                    EditorGUILayout.LabelField("Index", archiveSlot.Index.ToString());
                    EditorGUILayout.LabelField("Description", archiveSlot.Description);
                    EditorGUILayout.LabelField("Timestamp", new DateTime(archiveSlot.Timestamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                    EditorGUILayout.LabelField("Version", archiveSlot.Version.ToString());
                    EditorGUILayout.LabelField("ThumbnailIdentifier", archiveSlot.ThumbnailIdentifier);
                    EditorGUILayout.LabelField("IsAutoSave", archiveSlot.IsAutoSave.ToString());

                    EditorGUILayout.LabelField("DataCatalog", EditorStyles.boldLabel);
                    foreach (var addition in archiveSlot.DataCatalog)
                    {
                        EditorGUILayout.LabelField(addition.Key, addition.Value);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Separator();
        }
    }
}