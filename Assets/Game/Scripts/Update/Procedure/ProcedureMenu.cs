using Cysharp.Threading.Tasks;
using DEngine.Procedure;
using DEngine.Runtime;
using Game.Archive;
using UnityEngine;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update
{
    public class ProcedureMenu : ProcedureBase
    {
        protected override async void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);


            ArchivePlayer archivePlayer = new("zpf", "zpf")
            {
                Name = "zzz",
                Hp = 100,
                Pos = new Vector3(1, 2, 3),
                Quaternion = Quaternion.identity
            };

            ArchivePlayer archivePlayer2 = new("zpf", "xrl")
            {
                Name = "xxx",
                Hp = 200,
                Pos = new Vector3(3, 2, 3),
                Quaternion = Quaternion.LookRotation(Vector3.up)
            };

            TestData2 testData2 = new("zpf", "xrl2")
            {
                Speed = 100,
                Frequency = 0.01F
            };

            GameEntry.Archive.SelectSlot(0);
            GameEntry.Archive.SetData(archivePlayer);
            GameEntry.Archive.SetData(archivePlayer2);
            GameEntry.Archive.SetData(testData2);
            await GameEntry.Archive.Save();

            return;
            await GameEntry.Archive.Load();



            Log.Info(GameEntry.Archive.GetData<ArchivePlayer>("zpf"));
            Log.Info(GameEntry.Archive.GetData<ArchivePlayer>("xrl"));
            Log.Info(GameEntry.Archive.GetData<TestData2>("xrl2"));
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        }
    }
}