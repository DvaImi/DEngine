using Game.Update;

public struct ChangeSceneEventType
{
    public ChangeSceneEventType(SceneId sceneId)
    {
        SceneId = sceneId;
    }

    public SceneId SceneId { get; private set; }
}