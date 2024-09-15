using Cysharp.Threading.Tasks;
using DEngine.Procedure;
using DEngine.Runtime;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update
{
    public class ProcedureHotfixLaunch : ProcedureBase
    {
        protected override async void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("ProcedureHotfix  Launch  ");
            await UniTask.WhenAll(InitializeArchive(), InitializeNetwork());
            ChangeState<ProcedurePreload>(procedureOwner);
        }

        private async UniTask InitializeArchive()
        {
            await GameEntry.Archive.Initialize();
            Log.Info("Init Archive complete.");
        }

        private async UniTask InitializeNetwork()
        {
            await GameEntry.Network.Initialize(GetType().Assembly);
            Log.Info("Init Network complete.");
        }
    }
}