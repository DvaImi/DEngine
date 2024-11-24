using DEngine;
using UnityEngine;

namespace Game.AssetObject
{
    public class SetSpriteRendererObject : ISetAssetObject
    {
        private Sprite m_Sprite;
        private SpriteRenderer m_SpriteRenderer;
        public string AssetName { get; private set; }

        public void SetAsset(Object asset)
        {
            m_Sprite = (Sprite)asset;

            if (m_SpriteRenderer)
            {
                m_SpriteRenderer.sprite = m_Sprite;
            }
        }

        public bool IsCanRelease()
        {
            return !m_SpriteRenderer || !m_SpriteRenderer.sprite || (m_Sprite && m_SpriteRenderer.sprite != m_Sprite);
        }

        public void Clear()
        {
            m_SpriteRenderer = null;
            m_Sprite = null;
        }

        public static SetSpriteRendererObject Create(SpriteRenderer spriteRenderer, string assetName)
        {
            var item = ReferencePool.Acquire<SetSpriteRendererObject>();
            item.m_SpriteRenderer = spriteRenderer;
            item.AssetName = assetName;
            return item;
        }
    }
}