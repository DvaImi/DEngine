using System;
using System.Net.Sockets;
using DEngine;
using DEngine.Debugger;
using Game.Network;
using UnityEngine;

namespace Game.Debugger
{
    public class NetworkDebuggerWindow : IDebuggerWindow
    {
        private const float TitleWidth = 240f;

        private INetworkModule m_Network;

        private string m_RemoteAddress;
        private string m_Port;

        public void Initialize(params object[] args)
        {
            m_Network = GameEntry.Network;
        }

        public void Shutdown()
        {
        }

        public void OnEnter()
        {
        }

        public void OnLeave()
        {
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void OnDraw()
        {
            OnDrawNetworkWindow();
        }

        private void OnDrawNetworkWindow()
        {
            GUILayout.Label("<b>Network Information</b>");
            GUILayout.BeginVertical("box");
            {
                DrawItem("Network State", m_Network.Connected ? "Connected" : "Disconnected");
                DrawItem("Network Session Ping", Utility.Text.Format("{0} ms", m_Network.Ping));
                DrawItem("Address Family", m_Network.Connected ? GetAddressFamily(m_Network.Session.RemoteEndPoint.AddressFamily) : "Unavailable");
                DrawItem("Remote Address", m_Network.Connected ? m_Network.Session.RemoteEndPoint.ToString() : "Unavailable");
                DrawItem("Send Message", m_Network.SendMessageCount.ToString());
                DrawItem("Send Route Message", m_Network.SentRouteMessageCount.ToString());
                DrawItem("Call Request", m_Network.CallRequestCount.ToString());
                DrawItem("Call Route Request", m_Network.CallRouteRequestCount.ToString());
                DrawItem("Heart Beat LastTime", m_Network.Heartbeat == null ? "0" : DateTimeOffset.FromUnixTimeMilliseconds(m_Network.Heartbeat.LastTime).DateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));

                GUILayout.Space(5);
                GUILayout.Label("IP", GUILayout.Width(TitleWidth));
                m_RemoteAddress = GUILayout.TextField(m_RemoteAddress);
                GUILayout.Label("Port", GUILayout.Width(TitleWidth));
                m_Port = GUILayout.TextField(m_Port);
                GUILayout.Space(5);
                if (GUILayout.Button("Connect"))
                {
                    m_Network.Connect(Utility.Text.Format("{0}/{1}", m_RemoteAddress, m_Port));
                }

                GUILayout.Space(5);
                if (GUILayout.Button("Disconnect"))
                {
                    m_Network.Disconnect();
                }
            }
            GUILayout.EndVertical();
        }

        private static void DrawItem(string title, string content)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(title, GUILayout.Width(TitleWidth));
                GUILayout.Label(content);
            }
            GUILayout.EndHorizontal();
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