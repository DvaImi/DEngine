﻿using UnityEditor;
using DEngine.Runtime;

namespace DEngine.Editor
{
    [CustomEditor(typeof(EventComponent))]
    internal sealed class EventComponentInspector : DEngineInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            EventComponent t = (EventComponent)target;

            if (IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Event Handler Count", t.EventHandlerCount.ToString());
                EditorGUILayout.LabelField("Event Count", t.EventCount.ToString());
            }

            Repaint();
        }

        private void OnEnable()
        {
        }
    }
}
