using DEngine;
using UnityEngine;
using UnityEngine.UI;

namespace Game.AssetObject
{
    public class SetImageSpriteObject : ISetAssetObject
    {
        private Image m_Image;
        private Sprite m_Sprite;
        private bool m_SetNativeSize;
        public string AssetName { get; private set; }

        public void SetAsset(Object asset)
        {
            m_Sprite = (Sprite)asset;

            if (m_Image != null)
            {
                m_Image.sprite = m_Sprite;
                if (m_SetNativeSize)
                {
                    m_Image.SetNativeSize();
                }
            }
        }

        public bool IsCanRelease()
        {
            return !m_Image || !m_Image.sprite || (m_Sprite && m_Image.sprite != m_Sprite);
        }


        public void Clear()
        {
            m_Image         = null;
            m_Sprite        = null;
            m_SetNativeSize = false;
        }

        public static SetImageSpriteObject Create(Image image, string assetName, bool setNativeSize = false)
        {
            var item = ReferencePool.Acquire<SetImageSpriteObject>();
            item.m_Image         = image;
            item.m_SetNativeSize = setNativeSize;
            item.AssetName       = assetName;
            return item;
        }
    }
}