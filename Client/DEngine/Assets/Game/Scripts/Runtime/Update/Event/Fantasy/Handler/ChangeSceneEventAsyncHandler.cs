using Fantasy.Async;
using Fantasy.Event;
using Game;
using Game.Update.Scene;
using Game.Update.Sound;

public class ChangeSceneEventAsyncHandler : AsyncEventSystem<ChangeSceneEventType>
{
    protected override async FTask Handler(ChangeSceneEventType self)
    {
        GameEntry.Sound.StopAllLoadingSounds();
        GameEntry.Sound.StopAllLoadedSounds();

        GameEntry.Entity.HideAllLoadingEntities();
        GameEntry.Entity.HideAllLoadedEntities();

        string[] loadedSceneAssetNames = GameEntry.Scene.GetLoadedSceneAssetNames();
        foreach (var t in loadedSceneAssetNames)
        {
            GameEntry.Scene.UnloadScene(t);
        }

        GameEntry.Base.ResetNormalGameSpeed();
        var drScene = await GameEntry.Scene.LoadSceneAsync(self.SceneId, this);

        if (drScene.BackgroundMusicId > 0)
        {
            GameEntry.Sound.PlayMusic(drScene.BackgroundMusicId);
        }
    }
}