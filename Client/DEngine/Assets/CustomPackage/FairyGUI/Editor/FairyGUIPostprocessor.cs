using UnityEditor;
using UnityEngine;

namespace Game.Editor.FairyGUI
{
    public class FairyGUIPostprocessor : AssetPostprocessor
    {
        private void OnPostprocessTexture(Texture2D texture)
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;

            // 处理FairyGUI中的纹理
            if (assetPath.StartsWith(FairyGUIEditorSetting.Instance.FairyGUIProject))
            {
                if (!textureImporter.mipmapEnabled)
                {
                    textureImporter.mipmapEnabled = true;
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                }
            }
        }
    }
}