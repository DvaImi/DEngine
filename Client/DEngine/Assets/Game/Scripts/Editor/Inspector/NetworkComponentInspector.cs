using System;
using System.Net.Sockets;
using DEngine;
using DEngine.Editor;
using Fantasy.Network;
using Game.Network;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    [CustomEditor(typeof(NetworkComponent))]
    public class NetworkComponentInspector : DEngineInspector
    {
        private static readonly string[] ServiceTypeNames = { "None", "KCP", "TCP", "WebSocket", "HTTP" };

        private SerializedProperty m_ServiceType = null;
        private int m_ServiceTypeIndex;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            NetworkComponent t = (NetworkComponent)target;
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
                {
                    EditorGUILayout.EnumPopup("ServiceType", t.ServiceType);
                }
                else
                {
                    int selectedIndex = EditorGUILayout.Popup("ServiceType", m_ServiceTypeIndex, ServiceTypeNames);
                    if (selectedIndex != m_ServiceTypeIndex)
                    {
                        m_ServiceTypeIndex = selectedIndex;
                        m_ServiceType.enumValueIndex = selectedIndex;
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
            }
            else
            {
                if (IsPrefabInHierarchy(t.gameObject))
                {
                    EditorGUILayout.LabelField("Network");
                    DrawNetwork(t);
                }
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        private void DrawNetwork(NetworkComponent network)
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Network State", network.Connected ? "Connected" : "Disconnected");
                EditorGUILayout.LabelField("Network Session Ping", Utility.Text.Format("{0} ms", network.Ping));
                EditorGUILayout.LabelField("Address Family", network.Connected ? GetAddressFamily(network.Session.RemoteEndPoint.AddressFamily) : "Unavailable");
                EditorGUILayout.LabelField("Remote Address", network.Connected ? network.Session.RemoteEndPoint.ToString() : "Unavailable");
                EditorGUILayout.LabelField("Send Message", network.SendMessageCount.ToString());
                EditorGUILayout.LabelField("Send Route Message", network.SentRouteMessageCount.ToString());
                EditorGUILayout.LabelField("Call Request", network.CallRequestCount.ToString());
                EditorGUILayout.LabelField("Call Route Request", network.CallRouteRequestCount.ToString());
                EditorGUILayout.LabelField("Heart Beat LastTime", network.Heartbeat == null ? "0" : DateTimeOffset.FromUnixTimeMilliseconds(network.Heartbeat.LastTime).DateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                EditorGUI.BeginDisabledGroup(!network.Connected);
                {
                    if (GUILayout.Button("Disconnect"))
                    {
                        network.Disconnect();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
        }

        private static string GetAddressFamily(AddressFamily addressFamily)
        {
            return addressFamily switch
            {
                AddressFamily.InterNetwork => "IPv4",
                AddressFamily.InterNetworkV6 => "IPv6",
                _ => Utility.Text.Format("Not supported address family '{0}'.", addressFamily)
            };
        }

        private void OnEnable()
        {
            m_ServiceType = serializedObject.FindProperty("m_ServiceType");
            m_ServiceTypeIndex = m_ServiceType.enumValueIndex;
            serializedObject.ApplyModifiedProperties();
        }
    }
}