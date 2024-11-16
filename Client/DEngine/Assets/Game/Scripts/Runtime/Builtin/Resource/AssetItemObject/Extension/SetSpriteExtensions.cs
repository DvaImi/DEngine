using UnityEngine;
using UnityEngine.UI;

namespace Game.AssetItemObject
{
    public static class SetSpriteExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="assetName"></param>
        /// <param name="setNativeSize"></param>
        public static void SetSprite(this Image image, string assetName, bool setNativeSize = false)
        {
            GameEntry.Item.SetAssetByResources<Sprite>(SetImageSpriteObject.Create(image, assetName, setNativeSize));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteRenderer"></param>
        /// <param name="assetName"></param>
        public static void SetSprite(this SpriteRenderer spriteRenderer, string assetName)
        {
            GameEntry.Item.SetAssetByResources<Sprite>(SetSpriteRendererObject.Create(spriteRenderer, assetName));
        }
    }
}