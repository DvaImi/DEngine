using DEngine.Procedure;
using Geek.Server.Proto;
using MessagePack.Resolvers;
using PolymorphicMessagePack;
using UnityEngine;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update
{
    public class ProcedureMenu : ProcedureBase
    {
        private DEngine.Network.INetworkChannel m_GeekChannel;
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            PolymorphicResolver.AddInnerResolver(GeneratedResolver.Instance);
            PolymorphicTypeMapper.Register<Message>();
            PolymorphicTypeMapper.Register<ReqLogin>();
            PolymorphicResolver.Instance.Init();
            m_GeekChannel = GameEntry.Network.GetNetworkChannel("Geek");
            //m_GeekChannel.Connect(IPAddress.Parse("192.168.86.4"), 8899);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_GeekChannel == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                var req = new ReqLogin();
                req.SdkType = 0;
                req.SdkToken = "";
                req.UserName = "dvalim";
                req.Device = SystemInfo.deviceUniqueIdentifier;
                req.Platform = "unity";
                m_GeekChannel.Send(req);
            }
        }
    }
}
