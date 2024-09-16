using System.Collections.Generic;
using System.IO;
using System.Text;
using DEngine.Event;
using DEngine.Procedure;
using DEngine.Resource;
using DEngine.Runtime;
using UnityEngine;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update
{
    public class ProcedurePreload : ProcedureBase
    {
        private Dictionary<string, bool> m_LoadedFlag = new Dictionary<string, bool>();


        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameEntry.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            GameEntry.Event.Subscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            GameEntry.Event.Subscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);

            m_LoadedFlag.Clear();
            PreloadResources();
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            GameEntry.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            GameEntry.Event.Unsubscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            GameEntry.Event.Unsubscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            foreach (KeyValuePair<string, bool> loadedFlag in m_LoadedFlag)
            {
                if (!loadedFlag.Value)
                {
                    return;
                }
            }

            PreloadCompleteHandle();
            ChangeState<ProcedureMenu>(procedureOwner);
        }

        private void PreloadResources()
        {
            m_LoadedFlag.Add(UpdateAssetUtility.GetDataTableAsset(Constant.AssetVersion.DataTableVersion, true), false);
            GameEntry.Resource.LoadAsset(UpdateAssetUtility.GetDataTableAsset(Constant.AssetVersion.DataTableVersion, true), new LoadAssetCallbacks(new LoadAssetSuccessCallback(OnDataTableVersionLoadSuccess)));
            LoadLocalization(GameEntry.Localization.Language.ToString());
            LoadLubanDataTable();
        }

        private void PreloadCompleteHandle()
        {
            DRUIGroup[] uiGroups = GameEntry.DataTable.GetDataTable<DRUIGroup>().GetAllDataRows();

            foreach (var group in uiGroups)
            {
                if (GameEntry.UI.AddUIGroup(group.UIGroupName, group.UIGroupDepth))
                {
                    Log.Info("Add ui group [{0}] success", group.UIGroupName);
                }
                else
                {
                    Log.Warning("Add ui group [{0}] failure", group.UIGroupName);
                }
            }
        }

        private void OnDataTableVersionLoadSuccess(string assetName, object asset, float duration, object userData)
        {
            if (asset is TextAsset textAsset)
            {
                using (Stream stream = new MemoryStream(textAsset.bytes))
                {
                    using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        int dataTable = binaryReader.ReadInt32();
                        for (int i = 0; i < dataTable; i++)
                        {
                            LoadDataTable(binaryReader.ReadString());
                        }
                    }

                    m_LoadedFlag[assetName] = true;
                }
            }
        }

        private void LoadDataTable(string dataTableName)
        {
            string dataTableAssetName = UpdateAssetUtility.GetDataTableAsset(dataTableName, true);
            m_LoadedFlag.Add(dataTableAssetName, false);
            GameEntry.DataTable.LoadDataTable(dataTableName, dataTableAssetName, this);
        }

        private void LoadLocalization(string dictionaryName)
        {
            string dictionaryAssetName = UpdateAssetUtility.GetDictionaryAsset(dictionaryName, true);
            m_LoadedFlag.Add(dictionaryAssetName, false);
            GameEntry.Localization.ReadData(dictionaryAssetName, this);
        }

        private async void LoadLubanDataTable()
        {
            foreach (var table in UpdateEntry.Luban.Tables.TableNames)
            {
                m_LoadedFlag[table] = false;
            }

            await UpdateEntry.Luban.LoadAsync();

            foreach (var table in UpdateEntry.Luban.Tables.TableNames)
            {
                m_LoadedFlag[table] = true;
            }
        }

        private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            LoadDataTableSuccessEventArgs ne = (LoadDataTableSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_LoadedFlag[ne.DataTableAssetName] = true;
            Log.Info("Load data table '{0}' OK.", ne.DataTableAssetName);
        }

        private void OnLoadDataTableFailure(object sender, GameEventArgs e)
        {
            LoadDataTableFailureEventArgs ne = (LoadDataTableFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load data table '{0}' from '{1}' with error message '{2}'.", ne.DataTableAssetName, ne.DataTableAssetName, ne.ErrorMessage);
        }

        private void OnLoadDictionarySuccess(object sender, GameEventArgs e)
        {
            LoadDictionarySuccessEventArgs ne = (LoadDictionarySuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_LoadedFlag[ne.DictionaryAssetName] = true;
            Log.Info("Load dictionary '{0}' OK.", ne.DictionaryAssetName);
        }

        private void OnLoadDictionaryFailure(object sender, GameEventArgs e)
        {
            LoadDictionaryFailureEventArgs ne = (LoadDictionaryFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load dictionary '{0}' from '{1}' with error message '{2}'.", ne.DictionaryAssetName, ne.DictionaryAssetName, ne.ErrorMessage);
        }
    }
}