using Cysharp.Threading.Tasks;
using Game.Update;
using Luban;
using UnityEngine;

namespace Game.Config
{
    public sealed class LubanModule : ILubanModule
    {
        public int Priority => 1;

        public Tables Tables => m_Tables;

        private Tables m_Tables;

        public async UniTask LoadAsync()
        {
            m_Tables = new Tables();
            await m_Tables.LoadAsync(LubanLoader);
        }

        private async UniTask<ByteBuf> LubanLoader(string assetName)
        {
            var bytes = await GameEntry.Resource.LoadAssetAsync<TextAsset>(UpdateAssetUtility.GetConfigAsset(assetName, true));
            return new ByteBuf(bytes.bytes);
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Shutdown()
        {
            if (m_Tables == null)
            {
                return;
            }
        }
    }
}