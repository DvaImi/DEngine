using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Fantasy.Platform.Unity;
using Game.Debugger;
using Log = Fantasy.Log;

namespace Game.Network
{
    public class NetworkModule : INetworkModule
    {
        private UniTaskCompletionSource<bool> m_ConnectTask;
        private NetworkProtocolType m_ServiceType = NetworkProtocolType.TCP;

        /// <summary>
        /// Debug 模块
        /// </summary>
        private NetworkDebuggerWindow m_NetworkDebuggerWindow;

        /// <summary>
        /// 重连次数
        /// </summary>
        private int m_ReconnectCount;

        /// <summary>
        /// 获取或设置自动重连
        /// </summary>
        private bool m_AutoReconnect;

        /// <summary>
        /// <see cref="m_AutoReconnect"/> 设置为true的时候，重连的次数，默认为 5
        /// </summary>
        private int m_MaxReconnects;

        /// <summary>
        /// 开始重连时间
        /// </summary>
        private float m_ReconnectTime;

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
        /// 网络会话对象
        /// </summary>
        public Session Session { get; private set; }

        /// <summary>
        /// 网络会话心跳包组件
        /// </summary>
        public SessionHeartbeatComponent Heartbeat { get; private set; }

        /// <summary>
        /// 获取网络协议类型
        /// </summary>
        public NetworkProtocolType ServiceType => m_ServiceType;

        /// <summary>
        /// 初始化网络模块
        /// </summary>
        /// <param name="autoReconnect"></param>
        /// <param name="maxReconnects"><see cref="autoReconnect"/> 设置为true的时候，重连的次数，默认为 5</param>
        /// <param name="assemblies">装载的程序集</param>
        public void Initialize(bool autoReconnect, int maxReconnects = 5, params Assembly[] assemblies)
        {
            m_NetworkDebuggerWindow = new NetworkDebuggerWindow();
            GameEntry.Debugger.RegisterDebuggerWindow("Profiler/Network", m_NetworkDebuggerWindow);
            m_AutoReconnect = autoReconnect;
            m_MaxReconnects = maxReconnects <= 0 ? 5 : maxReconnects;
            Fantasy.Log.Register(new NetworkLog());
            if (Entry.Scene == null)
            {
                Entry.Initialize(assemblies);
                Entry.CreateScene();
                return;
            }

            Log.Info("Init Network complete.");
        }

        /// <summary>
        /// 连接到远程主机
        /// </summary>
        /// <param name="remoteAddress"></param>
        /// <param name="serviceType"></param>
        /// <param name="isHttps"></param>
        /// <param name="connectTimeout"></param>
        /// <param name="interval"></param>
        /// <param name="timeOut"></param>
        /// <param name="timeOutInterval"></param>
        public async UniTask Connect(string remoteAddress, NetworkProtocolType serviceType = NetworkProtocolType.KCP, bool isHttps = false, int connectTimeout = 5000, int interval = 2000, int timeOut = 2000, int timeOutInterval = 3000)
        {
            while (true)
            {
                Connected = false;
                m_ServiceType = serviceType;
                RemoteAddress = remoteAddress;
                Session = Entry.Scene.Connect(remoteAddress, serviceType, OnNetworkConnectedHandle, OnNetworkConnectFailureHandle, OnNetworkDisconnectHandle, isHttps, connectTimeout);
                m_ConnectTask = new UniTaskCompletionSource<bool>();
                Connected = await m_ConnectTask.Task;

                if (Connected)
                {
                    Heartbeat = Session.AddComponent<SessionHeartbeatComponent>();
                    Heartbeat.Start(interval, timeOut, timeOutInterval);
                    m_ReconnectCount = 0;
                    return;
                }

                if (!m_AutoReconnect)
                {
                    return;
                }

                if (m_ReconnectCount >= m_MaxReconnects)
                {
                    Log.Warning("重连次数超过限制");
                    return;
                }

                m_ReconnectCount++;
                Log.Warning("开始第{0} 次重连", m_ReconnectCount);
            }
        }

        /// <summary>
        /// 连接完成回调
        /// </summary>
        private void OnNetworkConnectedHandle()
        {
            Log.Info("Network connected, remote address '{0}'.", RemoteAddress);
            Connected = true;
            m_ConnectTask?.TrySetResult(Connected);
            GameEntry.Event.Fire(this, new OnNetworkConnectedEventArg());
        }

        /// <summary>
        /// 连接失败回调
        /// </summary>
        private void OnNetworkConnectFailureHandle()
        {
            Connected = false;
            m_ConnectTask?.TrySetResult(Connected);
            Log.Warning("Network connect failure, remote address '{0}'.", RemoteAddress);
            GameEntry.Event.Fire(this, new OnNetworkConnectFailureEventArg());
        }

        /// <summary>
        /// 断开连接回调
        /// </summary>
        private void OnNetworkDisconnectHandle()
        {
            Connected = false;
            m_ConnectTask?.TrySetResult(Connected);
            Log.Warning("Network disconnect, remote address '{0}'.", RemoteAddress);
            GameEntry.Event.Fire(this, new OnNetworkDisconnectEventArg());
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
            if (!Connected)
            {
                return null;
            }

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

        public int Priority => 0;

        public void Shutdown()
        {
            Disconnect();
        }
    }
}