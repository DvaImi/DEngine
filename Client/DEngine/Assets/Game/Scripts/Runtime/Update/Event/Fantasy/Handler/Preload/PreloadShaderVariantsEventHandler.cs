using DEngine.Runtime;
using Fantasy.Async;
using Fantasy.Event;
using Game;
using Game.Update;
using UnityEngine;
using GameEntry = Game.GameEntry;

public class PreloadShaderVariantsEventHandler : AsyncEventSystem<PreloadEventType>
{
    protected override async FTask Handler(PreloadEventType self)
    {
        var shaderVariantCollection = await GameEntry.Resource.LoadAssetAsync<ShaderVariantCollection>(UpdateAssetUtility.GetShaderVariantsAsset("GameShaderVariants"));
        if (!shaderVariantCollection || shaderVariantCollection.isWarmedUp)
        {
            return;
        }

        shaderVariantCollection.WarmUp();
        Log.Info("Game ShaderVariants has WarmUp.");
    }
}