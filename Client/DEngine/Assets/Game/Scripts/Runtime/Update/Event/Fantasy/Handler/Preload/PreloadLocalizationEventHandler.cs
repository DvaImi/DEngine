using Fantasy.Async;
using Fantasy.Event;
using Game;
using Game.Update;
using GameEntry = Game.GameEntry;

public class PreloadLocalizationEventHandler : AsyncEventSystem<PreloadEventType>
{
    protected override async FTask Handler(PreloadEventType self)
    {
        await GameEntry.Localization.LoadDictionaryAsync(UpdateAssetUtility.GetDictionaryAsset(GameEntry.Localization.Language.ToString()));
    }
}