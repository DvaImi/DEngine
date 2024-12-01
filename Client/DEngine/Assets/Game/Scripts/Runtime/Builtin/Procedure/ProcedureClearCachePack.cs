using System.Collections.Generic;
using System.IO;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;

namespace Game
{
    public class ProcedureClearCachePack : GameProcedureBase
    {
        private bool m_ClearCachePackComplete;
        private bool m_ShouldDeleteCachePacks;
        private readonly List<FileInfo> m_DeleteCachePackFiles = new();

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_ClearCachePackComplete = false;
            m_ShouldDeleteCachePacks = false;
            CollectCachePackFilesToDelete();
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_ClearCachePackComplete)
            {
                if (m_ShouldDeleteCachePacks)
                {
                    DeleteCachePackFiles();
                    m_ClearCachePackComplete = true;
                }

                return;
            }

            ProcessAssembliesProcedure(procedureOwner);
        }

        private void CollectCachePackFilesToDelete()
        {
            string[] files = Directory.GetFiles(GameEntry.Resource.ReadWritePath, Constant.Resource.ResourcePackSearchPattern);

            foreach (var file in files)
            {
                var info = new FileInfo(file);
                var versions = info.Name.Split('-');
                if (versions.Length < 2)
                {
                    continue;
                }

                string firstVersion = versions[1];
                string latestGameVersion = firstVersion[..firstVersion.LastIndexOf('.')];
                int internalResourceVersion = int.Parse(firstVersion.Split('.')[^1]);

                if (latestGameVersion != GameEntry.BuiltinData.Builtin.BuildInfo.LatestGameVersion)
                {
                    m_DeleteCachePackFiles.Add(info);
                    continue;
                }

                // 差异两个版本则丢弃
                if (GameEntry.Setting.GetInt(Constant.Resource.InternalResourceVersion, 0) - internalResourceVersion >= 2)
                {
                    m_DeleteCachePackFiles.Add(info);
                }
            }

            m_ShouldDeleteCachePacks = true;
        }

        private void DeleteCachePackFiles()
        {
            try
            {
                foreach (var fileInfo in m_DeleteCachePackFiles)
                {
                    if (fileInfo.Exists)
                    {
                        Log.Info("Deleting file: " + fileInfo.FullName);
                        fileInfo.Delete();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("Failed to delete cache pack files: " + ex.Message);
            }
        }
    }
}