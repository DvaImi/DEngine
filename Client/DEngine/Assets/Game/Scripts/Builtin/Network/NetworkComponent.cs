using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using DEngine.Runtime;
using Fantasy;
using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Fantasy.Platform.Unity;
using Log = DEngine.Runtime.Log;

namespace Game.Network
{
    public class NetworkComponent : DEngineComponent
    {
        /// <summary>
        /// 
        /// </summary>
        private UniTaskCompletionSource m_ConnectTask;

        /// <summary>
        /// 获取网络协议类型
        /// </summary>
        public NetworkProtocolType ServiceType { get; private set; }

        /// <summary>
        /// 是否已连接。
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        /// 获取网络延迟
        /// </summary>
        public int Ping
        {
            get { return Heartbeat?.Ping ?? 0; }
        }

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
        /// 
        /// </summary>
        public Scene Scene { get; private set; }

        /// <summary>
        /// 网络会话对象
        /// </summary>
        public Session Session { get; private set; }

        /// <summary>
        /// 网络会话心跳包组件
        /// </summary>
        public SessionHeartbeatComponent Heartbeat { get; private set; }

        public async UniTask Initialize(params Assembly[] assemblies)
        {
            Entry.Initialize(assemblies);
            Scene = await Entry.CreateScene();
            Log.Info("Init Network complete.");
        }

        /// <summary>
        /// 连接到远程主机
        /// </summary>
        /// <param name="address"></param>
        /// <param name="protocolType"></param>
        /// <param name="isHttps"></param>
        /// <param name="connectTimeout"></param>
        /// <param name="interval"></param>
        /// <param name="timeOut"></param>
        /// <param name="timeOutInterval"></param>
        public async UniTask<bool> Connect(string address, NetworkProtocolType protocolType, bool isHttps = false, int connectTimeout = 5000, int interval = 2000, int timeOut = 2000, int timeOutInterval = 3000)
        {
            Connected = false;
            Session = Scene.Connect(address, protocolType, OnNetworkConnectedHandle, OnNetworkConnectFailureHandle, OnNetworkDisconnectHandle, isHttps, connectTimeout);
            ServiceType = protocolType;
            m_ConnectTask = new UniTaskCompletionSource();
            await m_ConnectTask.Task;
            Heartbeat = Session.AddComponent<SessionHeartbeatComponent>();
            Heartbeat.Start(interval, timeOut, timeOutInterval);
            return Connected;
        }

        /// <summary>
        /// 连接完成回调
        /// </summary>
        private void OnNetworkConnectedHandle()
        {
            Log.Info("Network connected, remote address '{0}'.", Session?.RemoteEndPoint.ToString());
            m_ConnectTask?.TrySetResult();
            m_ConnectTask = null;
            Connected = true;
            GameEntry.Event.Fire(this, new OnNetworkConnectedEventArg());
        }

        /// <summary>
        /// 连接失败回调
        /// </summary>
        private void OnNetworkConnectFailureHandle()
        {
            Log.Warning("Network connect failure, remote address '{0}'.", Session?.RemoteEndPoint.ToString());
            GameEntry.Event.Fire(this, new OnNetworkConnectFailureEventArg());
        }

        /// <summary>
        /// 断开连接回调
        /// </summary>
        private void OnNetworkDisconnectHandle()
        {
            Log.Warning("Network disconnect, remote address '{0}'.", Session?.RemoteEndPoint.ToString());
            GameEntry.Event.Fire(this, new OnNetworkDisconnectEventArg());
        }

        /// <summary>
        /// 发送消息到会话。
        /// </summary>
        /// <param name="message">要发送的消息。</param>
        public void Send(IMessage message)
        {
            try
            {
                Session.Send(message);
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
            try
            {
                Session.Send(routeMessage: routeMessage, rpcId, routeId);
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
            try
            {
                return Session.Call(request, routeId);
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
            try
            {
                return Session.Call(request, routeId);
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
            m_ConnectTask?.TrySetCanceled();
            Session?.Dispose();
        }
    }
}