using System;
using System.Net.Sockets;
using DEngine;
using DEngine.Editor;
using Game.Network;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    [CustomEditor(typeof(NetworkComponent))]
    public class NetworkComponentInspector : DEngineInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            NetworkComponent t = (NetworkComponent)target;

            if (IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Network");
                DrawNetworkSession(t);
            }

            Repaint();
        }

        private void DrawNetworkSession(NetworkComponent network)
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Network State", network.Connected ? "Connected" : "Disconnected");
                EditorGUILayout.LabelField("Network Session Ping", Utility.Text.Format("{0} ms", network.Ping));
                EditorGUILayout.LabelField("Service Type", network.ServiceType.ToString());
                EditorGUILayout.LabelField("Address Family", network.Connected ? GetAddressFamily(network.Session.RemoteEndPoint.AddressFamily) : "Unavailable");
                EditorGUILayout.LabelField("Remote Address", network.Connected ? network.Session.RemoteEndPoint.ToString() : "Unavailable");
                EditorGUILayout.LabelField("Send Message", network.SendMessageCount.ToString());
                EditorGUILayout.LabelField("Send Route Message", network.SentRouteMessageCount.ToString());
                EditorGUILayout.LabelField("Call Request", network.CallRequestCount.ToString());
                EditorGUILayout.LabelField("Call Route Request", network.CallRouteRequestCount.ToString());
                EditorGUILayout.LabelField("Heart Beat LastTime", network.Heartbeat == null ? "0" :  DateTimeOffset.FromUnixTimeMilliseconds(network.Heartbeat.LastTime).DateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
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
    }
}