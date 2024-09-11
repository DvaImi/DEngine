using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using DEngine;
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
        private Scene m_Scene;
        private Session m_MainSession;
        private Dictionary<string, Session> m_Sessions;
        private Dictionary<string, UniTaskCompletionSource<Session>> m_SessionResult;

        protected override void Awake()
        {
            base.Awake();
            m_Sessions = new Dictionary<string, Session>();
            m_SessionResult = new Dictionary<string, UniTaskCompletionSource<Session>>();
        }

        /// <summary>
        /// 初始化网络组件
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public async UniTask Initialize(params Assembly[] assemblies)
        {
            m_Scene = await Entry.Initialize(assemblies);
        }

        /// <summary>
        ///  连接到主会话
        /// </summary>
        /// <param name="sessionName"></param>
        /// <param name="address"></param>
        /// <param name="protocolType"></param>
        /// <param name="isHttps"></param>
        /// <param name="connectTimeout"></param>
        public async UniTask ConnectMainSession(string sessionName, string address, NetworkProtocolType protocolType, bool isHttps = false, int connectTimeout = 10000)
        {
            m_MainSession = await Connect(sessionName, address, protocolType, isHttps, connectTimeout);
            m_MainSession.AddComponent<SessionHeartbeatComponent>().Start(2000);
        }

        /// <summary>
        /// 连接到远程主机
        /// </summary>
        /// <param name="sessionName"></param>
        /// <param name="address"></param>
        /// <param name="protocolType"></param>
        /// <param name="isHttps"></param>
        /// <param name="connectTimeout"></param>
        /// <returns></returns>
        public UniTask<Session> Connect(string sessionName, string address, NetworkProtocolType protocolType, bool isHttps = false, int connectTimeout = 5000)
        {
            var session = m_Scene.Connect(address, protocolType, () => OnConnectComplete(sessionName), () => OnConnectFailure(sessionName), () => OnConnectDisconnect(sessionName), isHttps, connectTimeout);
            m_Sessions[sessionName] = session;
            m_SessionResult[sessionName] = new UniTaskCompletionSource<Session>();
            return m_SessionResult[sessionName].Task;
        }

        /// <summary>
        /// 连接完成回调
        /// </summary>
        private void OnConnectComplete(string sessionName)
        {
            if (!HasSession(sessionName))
            {
                Log.Error("Can not contains session '{0}'.", sessionName);
                return;
            }

            Log.Info("Network channel connected, remote address '{0}'.", m_Sessions[sessionName].RemoteEndPoint.ToString());
            m_SessionResult[sessionName].TrySetResult(m_Sessions[sessionName]);
            m_SessionResult.Remove(sessionName);
        }

        /// <summary>
        /// 连接失败回调
        /// </summary>
        private void OnConnectFailure(string sessionName)
        {
            if (!HasSession(sessionName))
            {
                Log.Error("Can not contains session '{0}'.", sessionName);
                return;
            }

            Log.Warning("Network channel connect failure, remote address '{0}'.", m_Sessions[sessionName].RemoteEndPoint.ToString());
            m_SessionResult[sessionName].TrySetException(new DEngineException(Utility.Text.Format("Network channel connect failure, remote address '{0}'.", m_Sessions[sessionName].RemoteEndPoint.ToString())));
            m_SessionResult.Remove(sessionName);
        }

        /// <summary>
        /// 断开连接回调
        /// </summary>
        private void OnConnectDisconnect(string sessionName)
        {
            if (!HasSession(sessionName))
            {
                Log.Error("Can not contains session '{0}'.", sessionName);
                return;
            }

            Log.Warning("Network channel disconnect, remote address '{0}'.", m_Sessions[sessionName].RemoteEndPoint.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionName"></param>
        /// <returns></returns>
        public bool HasSession(string sessionName)
        {
            return m_Sessions.ContainsKey(sessionName);
        }

        /// <summary>
        /// 发送消息到会话。
        /// </summary>
        /// <param name="message">要发送的消息。</param>
        public void Send(IMessage message)
        {
            m_MainSession.Send(message);
        }

        /// <summary>
        /// 发送消息到指定会话
        /// </summary>
        /// <param name="sessionName"></param>
        /// <param name="message"></param>
        public void Send(string sessionName, IMessage message)
        {
            if (!HasSession(sessionName))
            {
                Log.Error("Can not contains session '{0}'.", sessionName);
                return;
            }

            m_Sessions[sessionName].Send(message);
        }

        /// <summary>
        /// 发送路由消息到会话。
        /// </summary>
        /// <param name="routeMessage">要发送的路由消息。</param>
        /// <param name="rpcId">RPC 标识符。</param>
        /// <param name="routeId">路由标识符。</param>
        public void Send(IRouteMessage routeMessage, uint rpcId = 0, long routeId = 0)
        {
            m_MainSession.Send(routeMessage: routeMessage, rpcId, routeId);
        }

        /// <summary>
        /// 发送消息到指定会话
        /// </summary>
        /// <param name="sessionName"></param>
        /// <param name="routeMessage"></param>
        /// <param name="rpcId"></param>
        /// <param name="routeId"></param>
        public void Send(string sessionName, IRouteMessage routeMessage, uint rpcId = 0, long routeId = 0)
        {
            if (!HasSession(sessionName))
            {
                Log.Error("Can not contains session '{0}'.", sessionName);
                return;
            }

            m_Sessions[sessionName].Send(routeMessage: routeMessage, rpcId, routeId);
        }

        /// <summary>
        /// 调用请求并等待响应。
        /// </summary>
        /// <param name="request"></param>
        /// <param name="routeId"></param>
        /// <returns></returns>
        public FTask<IResponse> Call(IRouteRequest request, long routeId = 0)
        {
            return m_MainSession.Call(request, routeId);
        }

        /// <summary>
        /// 使用指定会话调用请求并等待响应。
        /// </summary>
        /// <param name="sessionName"></param>
        /// <param name="request"></param>
        /// <param name="routeId"></param>
        /// <returns></returns>
        public FTask<IResponse> Call(string sessionName, IRouteRequest request, long routeId = 0)
        {
            if (!HasSession(sessionName))
            {
                Log.Error("Can not contains session '{0}'.", sessionName);
                return null;
            }

            return m_Sessions[sessionName].Call(request, routeId);
        }

        /// <summary>
        /// 调用请求并等待响应。
        /// </summary>
        /// <param name="request"></param>
        /// <param name="routeId"></param>
        /// <returns></returns>
        public FTask<IResponse> Call(IRequest request, long routeId = 0)
        {
            return m_MainSession.Call(request, routeId);
        }

        /// <summary>
        /// 使用指定会话调用请求并等待响应。
        /// </summary>
        /// <param name="sessionName"></param>
        /// <param name="request"></param>
        /// <param name="routeId"></param>
        /// <returns></returns>
        public FTask<IResponse> Call(string sessionName, IRequest request, long routeId = 0)
        {
            if (!HasSession(sessionName))
            {
                Log.Error("Can not contains session '{0}'.", sessionName);
                return null;
            }

            return m_Sessions[sessionName].Call(request, routeId);
        }

        /// <summary>
        /// 释放会话
        /// </summary>
        public void DisposeAllSession()
        {
            m_MainSession?.Dispose();

            foreach (var session in m_Sessions)
            {
                session.Value?.Dispose();
            }

            m_Sessions.Clear();
        }

        /// <summary>
        /// 释放指定会话
        /// </summary>
        /// <param name="sessionName"></param>
        public void DisposeSession(string sessionName)
        {
            if (!HasSession(sessionName))
            {
                Log.Error("Can not contains session '{0}'.", sessionName);
                return;
            }

            m_Sessions[sessionName]?.Dispose();
            m_Sessions.Remove(sessionName);
        }
    }
}