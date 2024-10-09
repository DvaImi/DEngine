using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Event;
using DEngine.Runtime;
using DEngine.Sound;
using PlaySoundFailureEventArgs = DEngine.Runtime.PlaySoundFailureEventArgs;
using PlaySoundSuccessEventArgs = DEngine.Runtime.PlaySoundSuccessEventArgs;

namespace Game
{
    public static partial class UniTaskExtension
    {
        private static readonly Dictionary<int, UniTaskCompletionSource<ISoundAgent>> SoundResult = new();

        public static UniTask<ISoundAgent> PlaySoundAsync(this SoundComponent self, string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams, object userData)
        {
            int serialId = self.PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, userData);
            UniTaskCompletionSource<ISoundAgent> result = new UniTaskCompletionSource<ISoundAgent>();
            SoundResult.Add(serialId, result);
            return result.Task;
        }

        private static void OnPlaySoundSuccess(object sender, GameEventArgs e)
        {
            if (e is PlaySoundSuccessEventArgs eventArgs && SoundResult.Remove(eventArgs.SerialId, out var result))
            {
                if (result == null)
                {
                    return;
                }

                result.TrySetResult(eventArgs.SoundAgent);
            }
        }

        private static void OnPlaySoundFailure(object sender, GameEventArgs e)
        {
            if (e is PlaySoundFailureEventArgs eventArgs && SoundResult.Remove(eventArgs.SerialId, out var result))
            {
                if (result == null)
                {
                    return;
                }

                result.TrySetException(new DEngineException(eventArgs.ErrorMessage));
            }
        }
    }
}