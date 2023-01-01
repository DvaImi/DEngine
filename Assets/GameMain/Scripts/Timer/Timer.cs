// ========================================================
// 描述：
// 作者：JuvenileGemini 
// 创建时间：2023-01-01 00:29:15
// 版 本：1.0
// ========================================================
using System;
using GameFramework;
using UnityEngine;

namespace Juvenile
{
    public class Timer : IReference
    {
        public float Duration { get; private set; }

        public bool IsLooped { get; set; }

        public bool IsCompleted { get; private set; }

        public bool UsesRealTime { get; private set; }

        public bool IsPaused
        {
            get { return _timeElapsedBeforePause.HasValue; }
        }

        public bool IsCancelled
        {
            get { return _timeElapsedBeforeCancel.HasValue; }
        }

        public bool IsDone
        {
            get { return IsCompleted || IsCancelled; }
        }

        private Action _onComplete;
        private Action<float> _onUpdate;
        private float _startTime;
        private float _lastUpdateTime;

        private float? _timeElapsedBeforeCancel;
        private float? _timeElapsedBeforePause;
     
        public void Cancel()
        {
            if (IsDone)
            {
                return;
            }

            _timeElapsedBeforeCancel = GetTimeElapsed();
            _timeElapsedBeforePause = null;
        }

        public void Pause()
        {
            if (IsPaused || IsDone)
            {
                return;
            }

            _timeElapsedBeforePause = GetTimeElapsed();
        }

        public void Resume()
        {
            if (!IsPaused || IsDone)
            {
                return;
            }

            _timeElapsedBeforePause = null;
        }

        public float GetTimeElapsed()
        {
            if (IsCompleted || GetWorldTime() >= GetFireTime())
            {
                return Duration;
            }

            return _timeElapsedBeforeCancel ??
                   _timeElapsedBeforePause ??
                   GetWorldTime() - _startTime;
        }

        public float GetTimeRemaining()
        {
            return Duration - GetTimeElapsed();
        }

        public float GetRatioComplete()
        {
            return GetTimeElapsed() / Duration;
        }

        public float GetRatioRemaining()
        {
            return GetTimeRemaining() / Duration;
        }

        private Timer Register(float duration, Action onComplete, Action<float> onUpdate, bool isLooped, bool usesRealTime)
        {
            Duration = duration;
            _onComplete = onComplete;
            _onUpdate = onUpdate;

            IsLooped = isLooped;
            UsesRealTime = usesRealTime;

            _startTime = GetWorldTime();
            _lastUpdateTime = _startTime;
            return this;
        }

        private float GetWorldTime()
        {
            return UsesRealTime ? Time.realtimeSinceStartup : Time.time;
        }

        private float GetFireTime()
        {
            return _startTime + Duration;
        }

        private float GetTimeDelta()
        {
            return GetWorldTime() - _lastUpdateTime;
        }

        public void Update()
        {
            if (IsDone)
            {
                return;
            }

            if (IsPaused)
            {
                _startTime += GetTimeDelta();
                _lastUpdateTime = GetWorldTime();
                return;
            }

            _lastUpdateTime = GetWorldTime();

            _onUpdate?.Invoke(GetTimeElapsed());

            if (GetWorldTime() >= GetFireTime())
            {

                _onComplete?.Invoke();

                if (IsLooped)
                {
                    _startTime = GetWorldTime();
                }
                else
                {
                    IsCompleted = true;
                }
            }
        }

        public static Timer Creat(float duration, Action onComplete, Action<float> onUpdate, bool isLooped, bool useRealTime)
        {
            return ReferencePool.Acquire<Timer>().Register(duration, onComplete, onUpdate, isLooped, useRealTime);
        }

        public void Clear()
        {
            Duration = 0;
            _onComplete = null;
            _onUpdate = null;

            IsLooped = false;
            UsesRealTime = false;

            _startTime = 0;
            _lastUpdateTime = 0;

            _timeElapsedBeforeCancel = null;
            _timeElapsedBeforePause = null;
        }
    }
}