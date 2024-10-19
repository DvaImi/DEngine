﻿using DEngine;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;

namespace Game
{
    public class ProcedureCheckResources : ProcedureBase
    {
        private bool m_CheckResourcesComplete = false;
        private bool m_NeedUpdateResources = false;
        private int m_UpdateResourceCount = 0;
        private long m_UpdateResourceTotalCompressedLength = 0L;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_CheckResourcesComplete = false;
            m_NeedUpdateResources = false;
            m_UpdateResourceCount = 0;
            m_UpdateResourceTotalCompressedLength = 0L;

            GameEntry.Resource.CheckResources(OnCheckResourcesComplete);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_CheckResourcesComplete)
            {
                return;
            }

            if (m_NeedUpdateResources)
            {
                procedureOwner.SetData<VarInt32>("UpdateResourceCount", m_UpdateResourceCount);
                procedureOwner.SetData<VarInt64>("UpdateResourceTotalCompressedLength", m_UpdateResourceTotalCompressedLength);
                ChangeState<ProcedureUpdateResources>(procedureOwner);
            }
            else
            {
#if ENABLE_HYBRIDCLR&& !UNITY_EDITOR
                ChangeState<ProcedureLoadAssemblies>(procedureOwner);
#else
                ChangeState<ProcedureLoadHotUpdateEntry>(procedureOwner);
#endif
            }
        }

        private void OnCheckResourcesComplete(int movedCount, int removedCount, int updateCount, long updateTotalLength, long updateTotalCompressedLength)
        {
            Log.Info("Check resources complete, '{0}' resources need to update, compressed length is '{1}', uncompressed length is '{2}'.", updateCount.ToString(), updateTotalCompressedLength.ToString(), updateTotalLength.ToString());

            string size = GameUtility.String.GetByteLengthString(updateTotalLength);
            if (updateCount > 0 && updateTotalCompressedLength > 0)
            {
                GameEntry.BuiltinData.OpenDialog(new DialogParams
                {
                    Mode = 2,
                    Message = Utility.Text.Format("Need update resource size :{0}", size),
                    ConfirmText = "Update",
                    OnClickConfirm = ConfirmUpdate,
                    CancelText = " Cancel",
                    OnClickCancel = delegate(object userData) { DEngine.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
                });

                return;
            }

            void ConfirmUpdate(object parm)
            {
                GameEntry.BuiltinData.DestroyDialog();
                m_CheckResourcesComplete = true;
                m_NeedUpdateResources = updateCount > 0;
                m_UpdateResourceCount = updateCount;
                m_UpdateResourceTotalCompressedLength = updateTotalCompressedLength;
            }

            ConfirmUpdate(null);
        }
    }
}