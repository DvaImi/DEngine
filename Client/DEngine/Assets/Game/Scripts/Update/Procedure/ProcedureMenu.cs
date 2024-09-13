using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Event;
using DEngine.Procedure;
using DEngine.Runtime;
using Fantasy;
using Fantasy.Network;
using Game.Network;
using Log = Fantasy.Log;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update
{
    public class ProcedureMenu : ProcedureBase
    {
        protected override async void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameEntry.Event.Subscribe(OnNetworkDisconnectEventArg.EventId, OnNetworkDisconnectHandle);
            GameEntry.Event.Subscribe(OnNetworkConnectFailureEventArg.EventId, OnNetworkDisconnectHandle);
            if (await TestConnectNetwork())
            {
                await TestNetwork();
            }
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.Event.Unsubscribe(OnNetworkDisconnectEventArg.EventId, OnNetworkDisconnectHandle);
            GameEntry.Event.Unsubscribe(OnNetworkConnectFailureEventArg.EventId, OnNetworkDisconnectHandle);
        }

        private async UniTask<bool> TestConnectNetwork()
        {
            var network = GameEntry.DataTable.GetDataTable<DRNetwork>().GetDataRow((int)NetworkId.Main);
            if (network == null)
            {
                return false;
            }

            return await GameEntry.Network.Connect(Utility.Text.Format("{0}:{1}", network.Address, network.Port), (NetworkProtocolType)network.ProtocolType);
        }

        private async UniTask TestNetwork()
        {
            GameEntry.Network.Send(new C2G_TestMessage()
            {
                Tag = nameof(DEngine) + "1"
            });


            await GameEntry.Network.Call(new C2G_CreateAddressableRequest());
            M2C_TestResponse response = await GameEntry.Network.Call(new C2M_TestRequest()
            {
                Tag = nameof(DEngine),
            }) as M2C_TestResponse;

            Log.Info($"Response: {response.Tag}");

            GameEntry.Network.Session.Send(new C2G_TestMessage()
            {
                Tag = nameof(DEngine) + "2"
            });
            // var httpResponse = await GameEntry.WebRequest.Get(Utility.Text.Format("http://{0}:{1}/api/Hello/greet", network.Address, network.Port));
            // Log.Info("HttpResponse: ", httpResponse.ToString());
        }

        private void OnNetworkDisconnectHandle(object sender, GameEventArgs e)
        {
            GameEntry.BuiltinData.OpenDialog(new DialogParams
            {
                Mode = 2,
                Message = "The device is not connected to the network",
                ConfirmText = "Retry",
                CancelText = "Cancel",
                OnClickConfirm = delegate { TestConnectNetwork().Forget(); },
                OnClickCancel = delegate { DEngine.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
            });
        }
    }
}