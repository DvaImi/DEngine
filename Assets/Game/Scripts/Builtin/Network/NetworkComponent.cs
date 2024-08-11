using System.IO;
using System.Reflection;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DEngine.Runtime;
using Fantasy;
using Log = Fantasy.Log;

namespace Game.Network
{
    public class NetworkComponent : DEngineComponent
    {
        [SerializeField]
        private string m_RemoteAddress = "127.0.0.1:20000";
        private Scene m_Scene;
        private Session m_Session;

        /// <summary>
        /// 初始化网络组件
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public async UniTask Initialize(params Assembly[] assemblies)
        {
            m_Scene = await Entry.Initialize(assemblies);
            m_Session = m_Scene.Connect(m_RemoteAddress, NetworkProtocolType.KCP, OnConnectComplete, OnConnectFail, OnConnectDisconnect, false, 5000);
        }

        /// <summary>
        /// 连接完成回调
        /// </summary>
        private void OnConnectComplete()
        {
            Log.Info("Network channel connected, remote address '{0}'.", m_Session.RemoteEndPoint.ToString());
            m_Session.AddComponent<SessionHeartbeatComponent>().Start(2000);
        }

        /// <summary>
        /// 连接失败回调
        /// </summary>
        private void OnConnectFail()
        {
            Log.Info("Network channel connect failure, remote address '{0}'.", m_Session.RemoteEndPoint.ToString());
        }

        /// <summary>
        /// 断开连接回调
        /// </summary>
        private void OnConnectDisconnect()
        {
            Log.Info("Network channel disconnect, remote address '{0}'.", m_Session.RemoteEndPoint.ToString());
        }

        /// <summary>
        /// 发送消息到会话。
        /// </summary>
        /// <param name="message">要发送的消息。</param>
        /// <param name="rpcId">RPC 标识符。</param>
        /// <param name="routeId">路由标识符。</param>
        public void Send(IMessage message)
        {
            m_Session.Send(message);
        }

        /// <summary>
        /// 发送路由消息到会话。
        /// </summary>
        /// <param name="routeMessage">要发送的路由消息。</param>
        /// <param name="rpcId">RPC 标识符。</param>
        /// <param name="routeId">路由标识符。</param>
        public void Send(IRouteMessage routeMessage, uint rpcId = 0, long routeId = 0)
        {
            m_Session.Send(routeMessage: routeMessage, rpcId, routeId);
        }

        /// <summary>
        /// 发送内存流到会话。
        /// </summary>
        /// <param name="memoryStream">要发送的内存流。</param>
        /// <param name="rpcId">RPC 标识符。</param>
        /// <param name="routeTypeOpCode">路由类型和操作码。</param>
        /// <param name="routeId">路由标识符。</param>
        public void Send(MemoryStream memoryStream, uint rpcId = 0, long routeTypeOpCode = 0, long routeId = 0)
        {
            m_Session.Send(memoryStream, rpcId, routeTypeOpCode, routeId);
        }

        /// <summary>
        /// 调用请求并等待响应。
        /// </summary>
        /// <param name="request"></param>
        /// <param name="routeId"></param>
        /// <returns></returns>
        public FTask<IResponse> Call(IRouteRequest request, long routeId = 0)
        {
            return m_Session.Call(request, routeId);
        }

        /// <summary>
        /// 调用请求并等待响应。
        /// </summary>
        /// <param name="request"></param>
        /// <param name="routeId"></param>
        /// <returns></returns>
        public FTask<IResponse> Call(IRequest request, long routeId = 0)
        {
            return m_Session.Call(request, routeId);
        }

        /// <summary>
        /// 释放会话
        /// </summary>
        public void Dispose()
        {
            m_Session?.Dispose();
        }
    }
}