using System.Collections.Generic;
using DEngine;
using DEngine.Runtime;
using Fantasy.Network;

namespace Game.Network
{
    public partial class NetworkModule : Fantasy.Entitas.Entity
    {
        public int Priority { get; } = 0;

        private readonly Dictionary<string, INetworkChannel> m_NetworkChannels = new();

        public int NetworkChannelCount => m_NetworkChannels.Count;

        public bool HasNetworkChannel(string name)
        {
            return m_NetworkChannels.ContainsKey(name ?? string.Empty);
        }

        public INetworkChannel GetNetworkChannel(string name)
        {
            return m_NetworkChannels.GetValueOrDefault(name ?? string.Empty);
        }

        public INetworkChannel[] GetAllNetworkChannels()
        {
            int index = 0;
            var results = new INetworkChannel[m_NetworkChannels.Count];
            foreach (var networkChannel in m_NetworkChannels)
            {
                results[index++] = networkChannel.Value;
            }

            return results;
        }

        public void GetAllNetworkChannels(List<INetworkChannel> results)
        {
            if (results == null)
            {
                throw new DEngineException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, INetworkChannel> networkChannel in m_NetworkChannels)
            {
                results.Add(networkChannel.Value);
            }
        }

        public INetworkChannel CreateNetworkChannel(string name, NetworkProtocolType serviceType, bool isHttps = false, int connectTimeout = 5000, int heartbeatInterval = 2000, int heartbeatTimeOut = 2000, int heartbeatTimeOutInterval = 3000)
        {
            if (HasNetworkChannel(name))
            {
                throw new DEngineException(Utility.Text.Format("Already exist network channel '{0}'.", name ?? string.Empty));
            }

            INetworkChannel networkChannel = new NetworkChannel(name, serviceType, isHttps, connectTimeout, heartbeatInterval, heartbeatTimeOut, heartbeatTimeOutInterval);
            m_NetworkChannels.Add(name, networkChannel);
            return networkChannel;
        }

        public bool DestroyNetworkChannel(string name)
        {
            if (m_NetworkChannels.TryGetValue(name ?? string.Empty, out var networkChannel))
            {
                networkChannel.Disconnect();
                return name != null && m_NetworkChannels.Remove(name);
            }

            return false;
        }

        public void Shutdown()
        {
            foreach (var networkChannel in m_NetworkChannels.Values)
            {
                networkChannel.Disconnect();
            }

            m_NetworkChannels.Clear();
            Log.Info("Network channels have been destroyed.");
        }
    }
}