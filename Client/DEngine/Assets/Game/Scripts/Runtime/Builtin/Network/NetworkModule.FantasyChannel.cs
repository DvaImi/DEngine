using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fantasy;
using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Log = Fantasy.Log;

namespace Game.Network
{
    public partial class NetworkModule
    {
        private class NetworkChannel : INetworkChannel
        {
            /// <summary>
            /// 网络频道名称
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// 网络频道Scene
            /// </summary>
            public Scene Scene { get; private set; }

            /// <summary>
            /// 获取网络协议类型
            /// </summary>
            public NetworkProtocolType ServiceType { get; private set; }

            /// <summary>
            ///  是否是https
            /// </summary>
            public bool IsHttps { get; private set; }

            /// <summary>
            ///  连接超时时间 单位（秒）
            /// </summary>
            public int ConnectTimeout { get; private set; }

            /// <summary>
            ///  心跳间隔
            /// </summary>
            public int HeartbeatInterval { get; private set; }

            /// <summary>
            ///  心跳超时时间
            /// </summary>
            public int HeartbeatTimeOut { get; private set; }

            /// <summary>
            /// 心跳超时间隔
            /// </summary>
            public int HeartbeatTimeOutInterval { get; private set; }

            /// <summary>
            /// 是否已连接。
            /// </summary>
            public bool Connected { get; private set; }

            /// <summary>
            /// 远程主机地址
            /// </summary>
            public string RemoteAddress { get; private set; }

            /// <summary>
            /// 获取网络延迟
            /// </summary>
            public int Ping => Heartbeat?.Ping ?? 0;

            /// <summary>
            /// 
            /// </summary>
            public int SendMessageCount { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public int SentRouteMessageCount { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public int CallRequestCount { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public int CallRouteRequestCount { get; private set; }

            /// <summary>
            /// 网络会话心跳包组件
            /// </summary>
            public SessionHeartbeatComponent Heartbeat { get; private set; }

            /// <summary>
            /// 构建新实例
            /// </summary>
            /// <param name="name"></param>
            /// <param name="serviceType"></param>
            /// <param name="isHttps"></param>
            /// <param name="connectTimeout"></param>
            /// <param name="heartbeatInterval"></param>
            /// <param name="heartbeatTimeOut"></param>
            /// <param name="heartbeatTimeOutInterval"></param>
            public NetworkChannel(string name, NetworkProtocolType serviceType, bool isHttps, int connectTimeout, int heartbeatInterval, int heartbeatTimeOut, int heartbeatTimeOutInterval)
            {
                Name = name;
                ServiceType = serviceType;
                IsHttps = isHttps;
                ConnectTimeout = connectTimeout;
                HeartbeatInterval = heartbeatInterval;
                HeartbeatTimeOut = heartbeatTimeOut;
                HeartbeatTimeOutInterval = heartbeatTimeOutInterval;
            }

            /// <summary>
            /// 连接到远程主机
            /// </summary>
            /// <param name="remoteAddress"></param>
            public async UniTask Connect(string remoteAddress)
            {
                Connected = false;
                RemoteAddress = remoteAddress;
                Scene?.Dispose();
                Scene = await Scene.Create();
                Scene.Connect(remoteAddress, ServiceType, OnNetworkConnectedHandle, OnNetworkConnectFailureHandle, OnNetworkDisconnectHandle, IsHttps, ConnectTimeout);
            }

            /// <summary>
            /// 连接完成回调
            /// </summary>
            private void OnNetworkConnectedHandle()
            {
                Log.Info("Network connected, remote address '{0}'.", RemoteAddress);
                Connected = true;
                Heartbeat = Scene.Session.AddComponent<SessionHeartbeatComponent>();
                Heartbeat.Start(HeartbeatInterval, HeartbeatTimeOut, HeartbeatTimeOutInterval);
                Scene.EventComponent.Publish(new OnNetworkConnectedEventArg(this));
            }

            /// <summary>
            /// 连接失败回调
            /// </summary>
            private void OnNetworkConnectFailureHandle()
            {
                Connected = false;
                Log.Warning("Network connect failure, remote address '{0}'.", RemoteAddress);
                Scene.EventComponent.Publish(new OnNetworkConnectFailureEventArg(this));
            }

            /// <summary>
            /// 断开连接回调
            /// </summary>
            private void OnNetworkDisconnectHandle()
            {
                Connected = false;
                Log.Warning("Network disconnect, remote address '{0}'.", RemoteAddress);
                Scene.EventComponent.Publish(new OnNetworkDisconnectEventArg(this));
            }

            /// <summary>
            /// 发送消息到会话。
            /// </summary>
            /// <param name="message">要发送的消息。</param>
            public void Send(IMessage message)
            {
                if (!Connected)
                {
                    return;
                }

                try
                {
                    Scene.Session.Send(message);
                }
                finally
                {
                    SendMessageCount++;
                }
            }

            /// <summary>
            /// 发送消息到会话。
            /// </summary>
            /// <param name="messages">要发送的消息列表。</param>
            public void Send(IList<IMessage> messages)
            {
                if (!Connected)
                {
                    return;
                }

                if (messages == null)
                {
                    throw new ArgumentNullException();
                }

                foreach (var message in messages)
                {
                    Send(message);
                }
            }

            /// <summary>
            /// 发送消息到会话。
            /// </summary>
            /// <param name="messages">要发送的消息列表。</param>
            public void Send(params IMessage[] messages)
            {
                if (!Connected)
                {
                    return;
                }

                if (messages == null)
                {
                    throw new ArgumentNullException();
                }

                foreach (var message in messages)
                {
                    Send(message);
                }
            }

            /// <summary>
            /// 发送路由消息到会话。
            /// </summary>
            /// <param name="routeMessage">要发送的路由消息。</param>
            /// <param name="rpcId">RPC 标识符。</param>
            /// <param name="routeId">路由标识符。</param>
            public void Send(IRouteMessage routeMessage, uint rpcId = 0, long routeId = 0)
            {
                if (!Connected)
                {
                    return;
                }

                try
                {
                    Scene.Session.Send(routeMessage: routeMessage, rpcId, routeId);
                }
                finally
                {
                    SentRouteMessageCount++;
                }
            }

            /// <summary>
            /// 发送路由消息到会话。
            /// </summary>
            /// <param name="routeMessages">要发送的路由消息。</param>
            /// <param name="rpcId">RPC 标识符。</param>
            /// <param name="routeId">路由标识符。</param>
            public void Send(IList<IRouteMessage> routeMessages, uint rpcId = 0, long routeId = 0)
            {
                if (!Connected)
                {
                    return;
                }

                if (routeMessages == null)
                {
                    throw new ArgumentNullException();
                }

                foreach (var message in routeMessages)
                {
                    Send(message);
                }
            }

            /// <summary>
            /// 调用请求并等待响应。
            /// </summary>
            /// <param name="request"></param>
            /// <param name="routeId"></param>
            /// <returns></returns>
            public FTask<IResponse> Call(IRequest request, long routeId = 0)
            {
                if (!Connected)
                {
                    return null;
                }

                try
                {
                    return Scene.Session.Call(request, routeId);
                }
                finally
                {
                    CallRequestCount++;
                }
            }

            /// <summary>
            /// 调用请求并等待响应。
            /// </summary>
            /// <param name="request"></param>
            /// <param name="routeId"></param>
            /// <returns></returns>
            public FTask<IResponse> Call(IRouteRequest request, long routeId = 0)
            {
                if (!Connected)
                {
                    return null;
                }

                try
                {
                    return Scene.Session.Call(request, routeId);
                }
                finally
                {
                    CallRouteRequestCount++;
                }
            }

            /// <summary>
            /// 释放会话
            /// </summary>
            public void Disconnect()
            {
                SendMessageCount = 0;
                SentRouteMessageCount = 0;
                CallRequestCount = 0;
                CallRouteRequestCount = 0;
                Scene?.Session?.Dispose();
                Scene?.Dispose();
            }
        }
    }
}