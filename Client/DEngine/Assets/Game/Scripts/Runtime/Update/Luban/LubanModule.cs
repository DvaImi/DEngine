using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DEngine.Runtime;
using Game.Update;
using Luban;
using UnityEngine;

namespace Game.LubanTable
{
    public sealed class LubanModule : ILubanModule
    {
        public int Priority => 1;

        public Tables Tables => m_Tables;
        private Tables m_Tables;

        public ILubanModule Initialize()
        {
            m_Tables = new Tables();
            return this;
        }

        public async UniTask LoadAsync()
        {
            await m_Tables.LoadAsync(LubanLoader);
        }

        private async UniTask<ByteBuf> LubanLoader(string assetName)
        {
            var bytes = await GameEntry.Resource.LoadAssetAsync<TextAsset>(UpdateAssetUtility.GetConfigAsset(assetName, true));
            Log.Info("Load luban data table '{0}' OK.", assetName);
            return new ByteBuf(bytes.bytes);
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Shutdown()
        {
        }
    }
}