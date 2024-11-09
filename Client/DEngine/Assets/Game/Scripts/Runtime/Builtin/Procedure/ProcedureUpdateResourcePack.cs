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
    /// 使用可更新模式更新补丁包流程
    /// </summary>
    public class ProcedureUpdateResourcePack : GameProcedureBase
    {
        private string m_PatchResourcePackName = null;
        private bool m_PatchResourcePackComplete = false;
        private UpdateResourceForm m_UpdateResourceForm = null;
        private long m_PatchTotalCompressedLength;
        private long m_CurrentTotalUpdateLength;
        private string m_PatchResourcePackPath;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_PatchResourcePackComplete = false;
            m_PatchResourcePackName = procedureOwner.GetData<VarString>("PatchResourcePackName");
            m_PatchTotalCompressedLength = procedureOwner.GetData<VarInt64>("PatchTotalCompressedLength");
            procedureOwner.RemoveData("PatchTotalCompressedLength");
            m_PatchResourcePackPath = Utility.Path.GetRegularCombinePath(GameEntry.Resource.ReadWritePath, m_PatchResourcePackName);

            GameEntry.Event.Subscribe(DownloadStartEventArgs.EventId, OnDownloadStart);
            GameEntry.Event.Subscribe(DownloadUpdateEventArgs.EventId, OnDownloadUpdate);
            GameEntry.Event.Subscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            GameEntry.Event.Subscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);

            if (File.Exists(m_PatchResourcePackPath))
            {
                m_PatchResourcePackComplete = true;
                return;
            }

            StartUpdateResourcePack();
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (!m_PatchResourcePackComplete)
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
            string downloadPath = m_PatchResourcePackPath;
            string downloadUri = Utility.Path.GetRemotePath(Utility.Text.Format("{0}/{1}", GameEntry.Resource.UpdatePrefixUri, m_PatchResourcePackName));
            GameEntry.Download.AddDownload(downloadPath, downloadUri, m_PatchResourcePackName, this);
        }

        private void RefreshProgress()
        {
            float progressTotal = (float)m_CurrentTotalUpdateLength / m_PatchTotalCompressedLength;
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
                m_PatchResourcePackComplete = true;
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