﻿using DEngine.DataNode;
using UnityEditor;
using DEngine.Runtime;

namespace DEngine.Editor
{
    [CustomEditor(typeof(DataNodeComponent))]
    internal sealed class DataNodeComponentInspector : DEngineInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            DataNodeComponent t = (DataNodeComponent)target;

            if (IsPrefabInHierarchy(t.gameObject))
            {
                DrawDataNode(t.Root);
            }

            Repaint();
        }

        private void OnEnable()
        {
        }

        private void DrawDataNode(IDataNode dataNode)
        {
            EditorGUILayout.LabelField(dataNode.FullName, dataNode.ToDataString());
            IDataNode[] child = dataNode.GetAllChild();
            foreach (IDataNode c in child)
            {
                DrawDataNode(c);
            }
        }
    }
}
