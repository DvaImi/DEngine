using DEngine.Runtime;

namespace Game
{
    public static partial class UniTaskExtension
    {
        public static void Subscribe()
        {
            EventComponent eventComponent = DEngine.Runtime.GameEntry.GetComponent<EventComponent>();

            if (eventComponent == null)
            {
                Log.Fatal("Event manager is invalid.");
                return;
            }

            eventComponent.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            eventComponent.Subscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);

            eventComponent.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            eventComponent.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);

            eventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            eventComponent.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);

            eventComponent.Subscribe(UnloadSceneSuccessEventArgs.EventId, OnUnloadSceneSuccess);
            eventComponent.Subscribe(UnloadSceneFailureEventArgs.EventId, OnUnloadSceneFailure);

            eventComponent.Subscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
            eventComponent.Subscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

            eventComponent.Subscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            eventComponent.Subscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);

            eventComponent.Subscribe(PlaySoundSuccessEventArgs.EventId, OnPlaySoundSuccess);
            eventComponent.Subscribe(PlaySoundFailureEventArgs.EventId, OnPlaySoundFailure);

            eventComponent.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            eventComponent.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);

            eventComponent.Subscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            eventComponent.Subscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);
        }

        public static void Unsubscribe()
        {
            EventComponent eventComponent = DEngine.Runtime.GameEntry.GetComponent<EventComponent>();

            if (eventComponent == null)
            {
                Log.Fatal("Event manager is invalid.");
                return;
            }

            eventComponent.SafeUnSubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            eventComponent.SafeUnSubscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);

            eventComponent.SafeUnSubscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            eventComponent.SafeUnSubscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);

            eventComponent.SafeUnSubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            eventComponent.SafeUnSubscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);

            eventComponent.SafeUnSubscribe(UnloadSceneSuccessEventArgs.EventId, OnUnloadSceneSuccess);
            eventComponent.SafeUnSubscribe(UnloadSceneFailureEventArgs.EventId, OnUnloadSceneFailure);

            eventComponent.SafeUnSubscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
            eventComponent.SafeUnSubscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

            eventComponent.SafeUnSubscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            eventComponent.SafeUnSubscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);

            eventComponent.SafeUnSubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            eventComponent.SafeUnSubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);

            eventComponent.SafeUnSubscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            eventComponent.SafeUnSubscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);
        }
    }
}