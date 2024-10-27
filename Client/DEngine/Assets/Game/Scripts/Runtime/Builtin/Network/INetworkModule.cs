using System.Collections.Generic;
using System.Reflection;
using Fantasy.Network;

namespace Game.Network
{
    public interface INetworkModule : IGameModule
    {
        /// <summary>
        /// 获取网络频道数量。
        /// </summary>
        int NetworkChannelCount { get; }

        /// <summary>
        /// 检查是否存在网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>是否存在网络频道。</returns>
        bool HasNetworkChannel(string name);

        /// <summary>
        /// 获取网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>要获取的网络频道。</returns>
        INetworkChannel GetNetworkChannel(string name);

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <returns>所有网络频道。</returns>
        INetworkChannel[] GetAllNetworkChannels();

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <param name="results">所有网络频道。</param>
        void GetAllNetworkChannels(List<INetworkChannel> results);

        /// <summary>
        /// 创建网络频道。
        /// </summary>
        /// <param name="name">网络频道名称</param>
        /// <param name="serviceType">网络协议类型</param>
        /// <param name="isHttps">是否是https</param>
        /// <param name="connectTimeout">连接超时时间</param>
        /// <param name="heartbeatInterval">心跳间隔</param>
        /// <param name="heartbeatTimeOut">心跳超时时间</param>
        /// <param name="heartbeatTimeOutInterval">心跳超时间隔</param>
        /// <returns></returns>
        INetworkChannel CreateNetworkChannel(string name, NetworkProtocolType serviceType, bool isHttps = false, int connectTimeout = 5000, int heartbeatInterval = 2000, int heartbeatTimeOut = 2000, int heartbeatTimeOutInterval = 3000);

        /// <summary>
        /// 销毁网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>是否销毁网络频道成功。</returns>
        bool DestroyNetworkChannel(string name);
    }
}