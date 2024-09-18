// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 12:44:18
// 版 本：1.0
// ========================================================

using System.IO;
using System.Reflection;
using System.Text;
using Cysharp.Threading.Tasks;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;
using UnityEngine;

namespace Game
{
    public class ProcedureLoadHotUpdate : ProcedureBase
    {
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            string[] HotUpdateAssemblies = await LoadHotUpdateAssembliesVersion();
            int loadCount = await LoadHotUpdateAssemblies(HotUpdateAssemblies);
            Log.Info("HotUpdateAssemblies Load Complete. need load count {0}，load success {1}", HotUpdateAssemblies.Length, loadCount);
            ChangeState<ProcedureLoadHotUpdateEntry>(procedureOwner);
        }

        private async UniTask<string[]> LoadHotUpdateAssembliesVersion()
        {
            string[] hotUpdateAssemblies = null;
            TextAsset updateMainfest = await GameEntry.Resource.LoadAssetAsync<TextAsset>(BuiltinAssetUtility.GetCLRUpdateAsset(Constant.AssetVersion.HotUpdateAssembliesVersion));
            if (updateMainfest != null || updateMainfest.bytes != null)
            {
                using Stream stream = new MemoryStream(updateMainfest.bytes);
                using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
                int count = binaryReader.ReadInt32();
                hotUpdateAssemblies = new string[count];
                for (int i = 0; i < count; i++)
                {
                    hotUpdateAssemblies[i] = BuiltinAssetUtility.GetCLRUpdateAsset(binaryReader.ReadString());
                }
            }


            return hotUpdateAssemblies;
        }

        private async UniTask<int> LoadHotUpdateAssemblies(string[] hotUpdateAssemblies)
        {
            TextAsset[] hotUpdates = await GameEntry.Resource.LoadAssetsAsync<TextAsset>(hotUpdateAssemblies);
            int hotUpdateCount = 0;
            if (hotUpdates == null)
            {
                return hotUpdateCount;
            }

            foreach (TextAsset hotUpdate in hotUpdates)
            {
                if (hotUpdate == null || hotUpdate.bytes == null)
                {
                    continue;
                }

                Assembly.Load(hotUpdate.bytes);
                Log.Info("{0} Load Success.", hotUpdate.name);
                hotUpdateCount++;
            }

            return hotUpdateCount;
        }
    }
}