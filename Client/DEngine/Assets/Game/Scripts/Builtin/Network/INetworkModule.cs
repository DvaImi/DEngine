using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Fantasy;
using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;

namespace Game.Network
{
    public interface INetworkModule
    {
        /// <summary>
        /// 是否已连接。
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// 获取网络延迟
        /// </summary>
        int Ping { get; }

        /// <summary>
        /// 
        /// </summary>
        int SendMessageCount { get; }

        /// <summary>
        /// 
        /// </summary>
        int SentRouteMessageCount { get; }

        /// <summary>
        /// 
        /// </summary>
        int CallRequestCount { get; }

        /// <summary>
        /// 
        /// </summary>
        int CallRouteRequestCount { get; }

        /// <summary>
        /// 网络会话对象
        /// </summary>
        Session Session { get; }

        /// <summary>
        /// 网络会话心跳包组件
        /// </summary>
        SessionHeartbeatComponent Heartbeat { get; }

        /// <summary>
        /// 获取网络协议类型
        /// </summary>
        NetworkProtocolType ServiceType { get; }

        /// <summary>
        /// 初始化网络模块
        /// </summary>
        /// <param name="assemblies"></param>
        void Initialize(params Assembly[] assemblies);

        /// <summary>
        /// 连接到远程主机
        /// </summary>
        /// <param name="address"></param>
        /// <param name="serviceType"></param>
        /// <param name="isHttps"></param>
        /// <param name="connectTimeout"></param>
        /// <param name="interval"></param>
        /// <param name="timeOut"></param>
        /// <param name="timeOutInterval"></param>
        UniTask<bool> Connect(string address, NetworkProtocolType serviceType = NetworkProtocolType.TCP, bool isHttps = false, int connectTimeout = 5000, int interval = 2000, int timeOut = 2000, int timeOutInterval = 3000);

        /// <summary>
        /// 发送消息到会话。
        /// </summary>
        /// <param name="message">要发送的消息。</param>
        void Send(IMessage message);

        /// <summary>
        /// 发送消息到会话。
        /// </summary>
        /// <param name="messages">要发送的消息列表。</param>
        void Send(IList<IMessage> messages);

        /// <summary>
        /// 发送消息到会话。
        /// </summary>
        /// <param name="messages">要发送的消息列表。</param>
        void Send(params IMessage[] messages);

        /// <summary>
        /// 发送路由消息到会话。
        /// </summary>
        /// <param name="routeMessage">要发送的路由消息。</param>
        /// <param name="rpcId">RPC 标识符。</param>
        /// <param name="routeId">路由标识符。</param>
        void Send(IRouteMessage routeMessage, uint rpcId = 0, long routeId = 0);

        /// <summary>
        /// 发送路由消息到会话。
        /// </summary>
        /// <param name="routeMessages">要发送的路由消息。</param>
        /// <param name="rpcId">RPC 标识符。</param>
        /// <param name="routeId">路由标识符。</param>
        void Send(IList<IRouteMessage> routeMessages, uint rpcId = 0, long routeId = 0);

        /// <summary>
        /// 调用请求并等待响应。
        /// </summary>
        /// <param name="request"></param>
        /// <param name="routeId"></param>
        /// <returns></returns>
        FTask<IResponse> Call(IRequest request, long routeId = 0);

        /// <summary>
        /// 调用请求并等待响应。
        /// </summary>
        /// <param name="request"></param>
        /// <param name="routeId"></param>
        /// <returns></returns>
        FTask<IResponse> Call(IRouteRequest request, long routeId = 0);

        /// <summary>
        /// 释放会话
        /// </summary>
        void Disconnect();
    }
}