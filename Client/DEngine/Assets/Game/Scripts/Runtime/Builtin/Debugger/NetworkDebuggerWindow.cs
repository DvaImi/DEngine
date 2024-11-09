using System;
using System.Net.Sockets;
using DEngine;
using Game.Network;
using UnityEngine;

namespace Game.Debugger
{
    public class NetworkDebuggerWindow : ScrollableDebuggerWindowBase
    {
        private const float TitleWidth = 240f;

        private INetworkModule m_Network;

        public override void Initialize(params object[] args)
        {
            m_Network = GameEntry.GetModule<INetworkModule>();
        }

        protected override void OnDrawScrollableWindow()
        {
            GUILayout.Label("<b>Network Information</b>");
            GUILayout.BeginVertical("box");
            {
                DrawItem("Network Channel Count", m_Network.NetworkChannelCount.ToString());
            }
            GUILayout.EndVertical();

            INetworkChannel[] networkChannels = m_Network.GetAllNetworkChannels();
            foreach (INetworkChannel channel in networkChannels)
            {
                OnDrawNetworkWindow(channel);
            }
        }

        private static void OnDrawNetworkWindow(INetworkChannel networkChannel)
        {
            GUILayout.Label("<b>Network Information</b>");
            GUILayout.BeginVertical("box");
            {
                DrawItem("Network Name", networkChannel.Name);
                DrawItem("Network ServiceType", networkChannel.ServiceType.ToString());
                DrawItem("Network State", networkChannel.Connected ? "Connected" : "Disconnected");
                DrawItem("Network IsHttps", networkChannel.IsHttps.ToString());
                DrawItem("Network ConnectTimeout", networkChannel.ConnectTimeout.ToString());
                DrawItem("Network HeartbeatInterval", networkChannel.HeartbeatInterval.ToString());
                DrawItem("Network HeartbeatTimeOut", networkChannel.HeartbeatTimeOut.ToString());
                DrawItem("Network HeartbeatTimeOutInterval", networkChannel.HeartbeatTimeOutInterval.ToString());
                DrawItem("Network Session Ping", Utility.Text.Format("{0} ms", networkChannel.Ping));
                DrawItem("Address Family", networkChannel.Connected ? GetAddressFamily(networkChannel.Scene.Session.RemoteEndPoint.AddressFamily) : "Unavailable");
                DrawItem("Remote Address", networkChannel.Connected ? networkChannel.RemoteAddress : "Unavailable");
                DrawItem("Send Message", networkChannel.SendMessageCount.ToString());
                DrawItem("Send Route Message", networkChannel.SentRouteMessageCount.ToString());
                DrawItem("Call Request", networkChannel.CallRequestCount.ToString());
                DrawItem("Call Route Request", networkChannel.CallRouteRequestCount.ToString());
                DrawItem("Heart Beat LastTime", networkChannel.Heartbeat == null ? "0" : DateTimeOffset.FromUnixTimeMilliseconds(networkChannel.Heartbeat.LastTime).DateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));

                GUILayout.Space(5);
                if (networkChannel.Connected)
                {
                    if (GUILayout.Button("Disconnect"))
                    {
                        networkChannel.Disconnect();
                    }
                }

                GUILayout.Space(5);
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