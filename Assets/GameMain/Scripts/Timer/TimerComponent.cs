// ========================================================
// 描述：
// 作者：JuvenileGemini 
// 创建时间：2023-01-01 00:29:08
// 版 本：1.0
// ========================================================
// ========================================================
// 描述：
// 作者：JuvenileGemini 
// 创建时间：2023-01-01 00:25:57
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Juvenile
{
    public class TimerComponent : MonoBehaviour
    {
        /// <summary>
        /// Manages updating all the <see cref="Timer"/>s that are running in the application.
        /// This will be instantiated the first time you create a timer -- you do not need to add it into the
        /// scene manually.
        /// </summary>

        private List<Timer> _timers = new List<Timer>();
        private List<Timer> _timersToAdd = new List<Timer>();

        /// <summary>
        /// Register a new timer that should fire an event after a certain amount of time
        /// has elapsed.
        ///
        /// Registered timers are destroyed when the scene changes.
        /// </summary>
        /// <param name="duration">The time to wait before the timer should fire, in seconds.</param>
        /// <param name="onComplete">An action to fire when the timer completes.</param>
        /// <param name="onUpdate">An action that should fire each time the timer is updated. Takes the amount
        /// of time passed in seconds since the start of the timer's current loop.</param>
        /// <param name="isLooped">Whether the timer should repeat after executing.</param>
        /// <param name="useRealTime">Whether the timer uses real-time(i.e. not affected by pauses,
        /// slow/fast motion) or game-time(will be affected by pauses and slow/fast-motion).</param>
        /// the timer will expire and not execute. This allows you to avoid annoying <see cref="NullReferenceException"/>s
        /// by preventing the timer from running and accessessing its parents' components
        /// after the parent has been destroyed.</param>
        /// <returns>A timer object that allows you to examine stats and stop/resume progress.</returns>
        public Timer Register(float duration, Action onComplete, Action<float> onUpdate = null, bool isLooped = false, bool useRealTime = false)
        {
            Timer timer = Timer.Creat(duration, onComplete, onUpdate, isLooped, useRealTime);
            RegisterTimer(timer);
            return timer;
        }

        public void CancelAllRegisteredTimers()
        {
            CancelAllTimers();
        }

        public void PauseAllRegisteredTimers()
        {
            PauseAllTimers();
        }

        public void ResumeAllRegisteredTimers()
        {
            ResumeAllTimers();
        }

        public void RegisterTimer(Timer timer)
        {
            _timersToAdd.Add(timer);
        }

        public void CancelAllTimers()
        {
            foreach (Timer timer in _timers)
            {
                timer.Cancel();
            }

            _timers = new List<Timer>();
            _timersToAdd = new List<Timer>();
        }

        public void PauseAllTimers()
        {
            foreach (Timer timer in _timers)
            {
                timer.Pause();
            }
        }

        public void ResumeAllTimers()
        {
            foreach (Timer timer in _timers)
            {
                timer.Resume();
            }
        }

        private void Update()
        {
            UpdateAllTimers();
        }

        private void UpdateAllTimers()
        {
            if (_timersToAdd.Count > 0)
            {
                _timers.AddRange(_timersToAdd);
                _timersToAdd.Clear();
            }

            foreach (Timer timer in _timers)
            {
                timer.Update();
            }

            _timers.RemoveAll(t => t.isDone);
        }
    }
}