using Cysharp.Threading.Tasks;
using DEngine.Runtime;
using Fantasy;

namespace Game.Network
{
    public class NetworkComponent : DEngineComponent
    {
        private Scene m_Scene;
        private Session m_Session;

        public async UniTask Initialize()
        {
            m_Scene = await Entry.Initialize(GetType().Assembly);
            m_Session = m_Scene.Connect("127.0.0.1:20000", NetworkProtocolType.KCP, OnConnectComplete, OnConnectFail, OnConnectDisconnect, false, 5000);
        }

        private void OnConnectComplete()
        {
            m_Session.AddComponent<SessionHeartbeatComponent>().Start(2000);
        }

        private void OnConnectFail()
        {
        }

        private void OnConnectDisconnect()
        {
        }
    }
}