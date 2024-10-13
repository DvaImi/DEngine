using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Procedure;
using DEngine.Runtime;
using Game.DataTable;
using Game.Update.DataTable;
using UnityEngine;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update.Procedure
{
    public class ProcedurePreload : ProcedureBase
    {
        private bool complete = false;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            // await PreloadResourcesTask();
            complete = true;
        }


        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (!complete)
            {
                return;
            }

            ChangeState<ProcedureProcessingPreload>(procedureOwner);
        }

        private async UniTask PreloadResourcesTask()
        {
            try
            {
                complete = false;
                await UniTask.WhenAll(PreloadDataTable(), PreloadLocalization());
                complete = true;
            }
            catch (Exception e)
            {
                Log.Error("Preload resources exception : {0}.", e.Message);
            }

            Log.Info("Preload resources complete.");
        }

        private async UniTask PreloadDataTable()
        {
            TextAsset textAsset = await GameEntry.Resource.LoadAssetAsync<TextAsset>(UpdateAssetUtility.GetDataTableAsset(Constant.AssetVersion.DataTableVersion, true));
            if (!textAsset)
            {
                return;
            }

            var dataTableVersion = Utility.Json.ToObject<GameDataTableVersion>(textAsset.text);
            if (dataTableVersion != null)
            {
                List<UniTask> loadTask = new List<UniTask>();
                foreach (var dataTableName in dataTableVersion.PreloadDataTable)
                {
                    loadTask.Add(GameEntry.DataTable.LoadDataTableAsync(dataTableName));
                }

                await UniTask.WhenAll(loadTask);
            }
        }

        private async UniTask PreloadLocalization()
        {
            await GameEntry.Localization.LoadDictionaryAsync(UpdateAssetUtility.GetDictionaryAsset(GameEntry.Localization.Language.ToString(), true));
        }
    }
}