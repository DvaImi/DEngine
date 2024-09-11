using System;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Procedure;
using Fantasy;
using Fantasy.Network;
using UnityEngine;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update
{
    public class ProcedureMenu : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            TestNetwork().Forget();
        }

        private async UniTaskVoid TestNetwork()
        {
            var network = GameEntry.DataTable.GetDataTable<DRNetwork>().GetDataRow((int)NetworkId.Main);
            if (network == null)
            {
                Log.Warning("can exist mian session.");
                return;
            }

            await GameEntry.Network.ConnectMainSession(network.Name, Utility.Text.Format("{0}:{1}", network.Address, network.Port), (NetworkProtocolType)network.ProtocolType);

            GameEntry.Network.Send(new C2G_TestMessage()
            {
                Tag = nameof(DEngine)
            });


            var creatAddress = await GameEntry.Network.Call(new C2G_CreateAddressableRequest()
            {
            });
            M2C_TestResponse response = await GameEntry.Network.Call(new C2M_TestRequest()
            {
                Tag = nameof(DEngine),
            }) as M2C_TestResponse;

            Log.Info($"Response: {response.Tag}");
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        }
    }
}