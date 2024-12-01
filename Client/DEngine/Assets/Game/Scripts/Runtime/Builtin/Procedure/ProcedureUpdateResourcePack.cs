using System.IO;
using DEngine;
using DEngine.Event;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 使用可更新模式更新Pack流程
    /// </summary>
    public class ProcedureUpdateResourcePack : GameProcedureBase
    {
        private string m_CompressedPackName = null;
        private bool m_UpdateCompressedPackComplete = false;
        private UpdateResourceForm m_UpdateResourceForm = null;
        private long m_CompressedPackLength;
        private long m_CurrentTotalUpdateLength;
        private string m_ResourcePackPath;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_UpdateCompressedPackComplete  = false;
            m_CompressedPackName      = procedureOwner.GetData<VarString>(Constant.Resource.ResourcePackName);
            m_CompressedPackLength = procedureOwner.GetData<VarInt64>(Constant.Resource.ResourcePackLength);
            procedureOwner.RemoveData(Constant.Resource.ResourcePackLength);
            m_ResourcePackPath = Utility.Path.GetRegularCombinePath(GameEntry.Resource.ReadWritePath, m_CompressedPackName);

            GameEntry.Event.Subscribe(DownloadStartEventArgs.EventId, OnDownloadStart);
            GameEntry.Event.Subscribe(DownloadUpdateEventArgs.EventId, OnDownloadUpdate);
            GameEntry.Event.Subscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            GameEntry.Event.Subscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);

            if (File.Exists(m_ResourcePackPath))
            {
                m_UpdateCompressedPackComplete = true;
                return;
            }

            StartUpdateResourcePack();
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (!m_UpdateCompressedPackComplete)
            {
                return;
            }


            ChangeState<ProcedureApplyResourcePack>(procedureOwner);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            if (m_UpdateResourceForm)
            {
                Object.Destroy(m_UpdateResourceForm.gameObject);
                m_UpdateResourceForm = null;
            }

            GameEntry.Event.Unsubscribe(DownloadStartEventArgs.EventId, OnDownloadStart);
            GameEntry.Event.Unsubscribe(DownloadUpdateEventArgs.EventId, OnDownloadUpdate);
            GameEntry.Event.Unsubscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            GameEntry.Event.Unsubscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);
        }

        private void StartUpdateResourcePack()
        {
            if (!m_UpdateResourceForm)
            {
                m_UpdateResourceForm = Object.Instantiate(GameEntry.BuiltinData.Builtin.UpdateResourceFormTemplate);
            }

            Log.Info("Start patch resources...");
            string downloadPath = m_ResourcePackPath;
            string downloadUri = Utility.Path.GetRemotePath(Utility.Text.Format("{0}/{1}", GameEntry.Resource.UpdatePrefixUri, m_CompressedPackName));
            GameEntry.Download.AddDownload(downloadPath, downloadUri, m_CompressedPackName, this);
        }

        private void RefreshProgress()
        {
            float progressTotal = (float)m_CurrentTotalUpdateLength / m_CompressedPackLength;
            m_UpdateResourceForm.SetProgress(progressTotal, null);
        }

        private void OnDownloadStart(object sender, GameEventArgs e)
        {
            if (e is DownloadStartEventArgs args && args.UserData == this)
            {
                Log.Info("Start download patch Resource Pack {0}", args.DownloadUri);
            }
        }

        private void OnDownloadUpdate(object sender, GameEventArgs e)
        {
            if (e is DownloadUpdateEventArgs args && args.UserData == this)
            {
                m_CurrentTotalUpdateLength = args.CurrentLength;
                RefreshProgress();
            }
        }

        private void OnDownloadSuccess(object sender, GameEventArgs e)
        {
            if (e is DownloadSuccessEventArgs args && args.UserData == this)
            {
                Log.Info("Download patch resource pack {0} success.", args.DownloadUri);
                m_UpdateCompressedPackComplete = true;
            }
        }

        private void OnDownloadFailure(object sender, GameEventArgs e)
        {
            if (e is DownloadFailureEventArgs args && args.UserData == this)
            {
                GameEntry.BuiltinData.OpenDialog(new DialogParams
                {
                    Mode = 1,
                    Message = $"Download patch resource pack failure. ErrorMessage is {args.ErrorMessage}",
                    ConfirmText = "Quit",
                    OnClickConfirm = delegate { DEngine.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
                });
            }
        }
    }
}