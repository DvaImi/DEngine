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
        protected override async void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            await PreloadResourcesTask();
            ChangeState<ProcedureProcessingPreload>(procedureOwner);
        }

        private async UniTask PreloadResourcesTask()
        {
            try
            {
                await UniTask.WhenAll(PreloadDataTable(), PreloadLocalization(), PreloadLubanDataTable());
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
                foreach (var dataTableName in dataTableVersion.StaticDataTable)
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

        private async UniTask PreloadLubanDataTable()
        {
            await Entry.Luban.LoadAsync();
        }
    }
}