using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;

namespace Game
{
    public class ProcedureClearCachePack : GameProcedureBase
    {
        private bool m_ClearCachePackComplete;
        private readonly List<FileInfo> m_DeleteCachePackFiles = new();
        private const string SearchPattern = "DEngineResourcePack-*-*.block";


        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_ClearCachePackComplete = false;
            StartClearCachePack();
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (!m_ClearCachePackComplete)
            {
                return;
            }

            ProcessAssembliesProcedure(procedureOwner);
        }

        private async void StartClearCachePack()
        {
            string[] files = Directory.GetFiles(GameEntry.Resource.ReadWritePath, SearchPattern);

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

                //差异两个版本则丢弃
                if (GameEntry.Setting.GetInt(Constant.ResourceVersion.InternalResourceVersion) - internalResourceVersion >= 2)
                {
                    m_DeleteCachePackFiles.Add(info);
                }
            }

            await UniTask.NextFrame();
            try
            {
                foreach (var fileInfo in m_DeleteCachePackFiles)
                {
                    if (fileInfo.Exists)
                    {
                        fileInfo.Delete();
                    }
                }
            }
            finally
            {
                m_ClearCachePackComplete = true;
            }
        }
    }
}