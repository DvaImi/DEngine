using System;
using System.Collections.Generic;
using System.Linq;
using DEngine;
using DEngine.Event;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Resource;
using DEngine.Runtime;
using Object = UnityEngine.Object;
using ResourceUpdateChangedEventArgs = DEngine.Runtime.ResourceUpdateChangedEventArgs;
using ResourceUpdateFailureEventArgs = DEngine.Runtime.ResourceUpdateFailureEventArgs;
using ResourceUpdateStartEventArgs = DEngine.Runtime.ResourceUpdateStartEventArgs;
using ResourceUpdateSuccessEventArgs = DEngine.Runtime.ResourceUpdateSuccessEventArgs;

namespace Game
{
    /// <summary>
    /// 使用可更新模式更新所有资源流程
    /// </summary>
    public class ProcedureUpdateResources : GameProcedureBase
    {
        private bool m_UpdateResourcesComplete;
        private int m_UpdateCount;
        private long m_UpdateTotalCompressedLength;
        private int m_UpdateSuccessCount;
        private readonly List<UpdateLengthData> m_UpdateLengthData = new();
        private UpdateResourceForm m_UpdateResourceForm;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_UpdateResourcesComplete = false;
            m_UpdateCount             = procedureOwner.GetData<VarInt32>(Constant.ResourceVersion.UpdateResourceCount);
            procedureOwner.RemoveData(Constant.ResourceVersion.UpdateResourceCount);
            m_UpdateTotalCompressedLength = procedureOwner.GetData<VarInt64>(Constant.ResourceVersion.UpdateResourceTotalCompressedLength);
            procedureOwner.RemoveData(Constant.ResourceVersion.UpdateResourceTotalCompressedLength);
            m_UpdateSuccessCount = 0;
            m_UpdateLengthData.Clear();
            m_UpdateResourceForm = null;

            GameEntry.Event.Subscribe(ResourceUpdateStartEventArgs.EventId, OnResourceUpdateStart);
            GameEntry.Event.Subscribe(ResourceUpdateChangedEventArgs.EventId, OnResourceUpdateChanged);
            GameEntry.Event.Subscribe(ResourceUpdateSuccessEventArgs.EventId, OnResourceUpdateSuccess);
            GameEntry.Event.Subscribe(ResourceUpdateFailureEventArgs.EventId, OnResourceUpdateFailure);
            StartUpdateResources(null);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            if (m_UpdateResourceForm)
            {
                Object.Destroy(m_UpdateResourceForm.gameObject);
                m_UpdateResourceForm = null;
            }

            GameEntry.Event.Unsubscribe(ResourceUpdateStartEventArgs.EventId, OnResourceUpdateStart);
            GameEntry.Event.Unsubscribe(ResourceUpdateChangedEventArgs.EventId, OnResourceUpdateChanged);
            GameEntry.Event.Unsubscribe(ResourceUpdateSuccessEventArgs.EventId, OnResourceUpdateSuccess);
            GameEntry.Event.Unsubscribe(ResourceUpdateFailureEventArgs.EventId, OnResourceUpdateFailure);

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_UpdateResourcesComplete)
            {
                return;
            }

            ProcessAssembliesProcedure(procedureOwner);
        }

        private void StartUpdateResources(string resourceGroupName)
        {
            if (!m_UpdateResourceForm)
            {
                m_UpdateResourceForm = Object.Instantiate(GameEntry.BuiltinData.Builtin.UpdateResourceFormTemplate);
            }

            Log.Info("Start update resources...");
            GameEntry.Resource.UpdateResources(resourceGroupName, OnUpdateResourcesComplete);
        }

        private void RefreshProgress()
        {
            long   currentTotalUpdateLength = m_UpdateLengthData.Aggregate(0L, (current, t) => current + t.Length);
            float  progressTotal            = (float)currentTotalUpdateLength / m_UpdateTotalCompressedLength;
            string descriptionText          = Utility.Text.Format("{0}/{1},{2}/{3}, {4:P0},{5}/s", m_UpdateSuccessCount.ToString(), m_UpdateCount.ToString(), GameUtility.String.GetByteLengthString(currentTotalUpdateLength), GameUtility.String.GetByteLengthString(m_UpdateTotalCompressedLength), progressTotal.ToString("P2"), GameUtility.String.GetByteLengthString((long)GameEntry.Download.CurrentSpeed));
            descriptionText += "\n";
            int      needTime   = (int)((m_UpdateTotalCompressedLength - currentTotalUpdateLength) / GameEntry.Download.CurrentSpeed);
            TimeSpan timespan   = new(0, 0, needTime);
            string   timeFormat = timespan.ToString(@"mm\:ss");
            descriptionText += $"剩余时间 {timeFormat}({GameUtility.String.GetByteLengthString((long)GameEntry.Download.CurrentSpeed)}/s)";
            m_UpdateResourceForm.SetProgress(progressTotal, descriptionText);
        }

        private void OnUpdateResourcesComplete(IResourceGroup resourceGroup, bool result)
        {
            if (result)
            {
                m_UpdateResourcesComplete = true;
                Log.Info("Update resources complete with no errors.");
            }
            else
            {
                Log.Error("Update resources complete with errors.");
            }
        }

        private void OnResourceUpdateStart(object sender, GameEventArgs e)
        {
            ResourceUpdateStartEventArgs ne = (ResourceUpdateStartEventArgs)e;

            for (int i = 0; i < m_UpdateLengthData.Count; i++)
            {
                if (m_UpdateLengthData[i].Name == ne.Name)
                {
                    Log.Warning("Update resource '{0}' is invalid.", ne.Name);
                    m_UpdateLengthData[i].Length = 0;
                    RefreshProgress();
                    return;
                }
            }

            m_UpdateLengthData.Add(new UpdateLengthData(ne.Name));
        }

        private void OnResourceUpdateChanged(object sender, GameEventArgs e)
        {
            ResourceUpdateChangedEventArgs ne = (ResourceUpdateChangedEventArgs)e;

            for (int i = 0; i < m_UpdateLengthData.Count; i++)
            {
                if (m_UpdateLengthData[i].Name == ne.Name)
                {
                    m_UpdateLengthData[i].Length = ne.CurrentLength;
                    RefreshProgress();
                    return;
                }
            }

            Log.Warning("Update resource '{0}' is invalid.", ne.Name);
        }

        private void OnResourceUpdateSuccess(object sender, GameEventArgs e)
        {
            ResourceUpdateSuccessEventArgs ne = (ResourceUpdateSuccessEventArgs)e;
            Log.Info("Update resource '{0}' success.", ne.Name);

            for (int i = 0; i < m_UpdateLengthData.Count; i++)
            {
                if (m_UpdateLengthData[i].Name == ne.Name)
                {
                    m_UpdateLengthData[i].Length = ne.CompressedLength;
                    m_UpdateSuccessCount++;
                    RefreshProgress();
                    return;
                }
            }

            Log.Warning("Update resource '{0}' is invalid.", ne.Name);
        }

        private void OnResourceUpdateFailure(object sender, GameEventArgs e)
        {
            ResourceUpdateFailureEventArgs ne = (ResourceUpdateFailureEventArgs)e;
            if (ne.RetryCount >= ne.TotalRetryCount)
            {
                Log.Error("Update resource '{0}' failure from '{1}' with error message '{2}', retry count '{3}'.", ne.Name, ne.DownloadUri, ne.ErrorMessage, ne.RetryCount.ToString());
                return;
            }
            else
            {
                Log.Info("Update resource '{0}' failure from '{1}' with error message '{2}', retry count '{3}'.", ne.Name, ne.DownloadUri, ne.ErrorMessage, ne.RetryCount.ToString());
            }

            for (int i = 0; i < m_UpdateLengthData.Count; i++)
            {
                if (m_UpdateLengthData[i].Name == ne.Name)
                {
                    m_UpdateLengthData.Remove(m_UpdateLengthData[i]);
                    RefreshProgress();
                    return;
                }
            }

            Log.Warning("Update resource '{0}' is invalid.", ne.Name);
        }

        private class UpdateLengthData
        {
            public UpdateLengthData(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public int Length { get; set; }
        }
    }
}