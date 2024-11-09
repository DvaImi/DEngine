using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fantasy;
using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;

namespace Game.Network
{
    public interface INetworkChannel
    {
        /// <summary>
        /// 网络频道名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 网络频道Scene
        /// </summary>
        Scene Scene { get; }

        /// <summary>
        /// 远程主机地址
        /// </summary>
        string RemoteAddress { get; }

        /// <summary>
        /// 获取网络协议类型
        /// </summary>
        NetworkProtocolType ServiceType { get; }

        /// <summary>
        ///  是否是https
        /// </summary>
        bool IsHttps { get; }

        /// <summary>
        ///  连接超时时间 单位（秒）
        /// </summary>
        int ConnectTimeout { get; }

        /// <summary>
        ///  心跳间隔
        /// </summary>
        int HeartbeatInterval { get; }

        /// <summary>
        ///  心跳超时时间
        /// </summary>
        int HeartbeatTimeOut { get; }

        /// <summary>
        /// 心跳超时间隔
        /// </summary>
        int HeartbeatTimeOutInterval { get; }

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
        /// 网络会话心跳包组件
        /// </summary>
        SessionHeartbeatComponent Heartbeat { get; }


        /// <summary>
        /// 连接到远程主机
        /// </summary>
        /// <param name="remoteAddress"></param>
        UniTask Connect(string remoteAddress);

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
        /// 断开连接
        /// </summary>
        void Disconnect();
    }
}