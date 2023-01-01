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
        private List<Timer> _timers = new List<Timer>();
        private List<Timer> _timersToAdd = new List<Timer>();

        public Timer Register(float duration, Action onComplete, Action<float> onUpdate = null, bool isLooped = false, bool useRealTime = false)
        {
            Timer timer = Timer.Creat(duration, onComplete, onUpdate, isLooped, useRealTime);
            RegisterTimer(timer);
            return timer;
        }

        public void RegisterTimer(Timer timer)
        {
            _timersToAdd.Add(timer);
        }

        public void Cancel(Timer timer)
        {
            timer?.Cancel();
        }

        public void Pause(Timer timer)
        {
            timer?.Pause();
        }

        public void Resume(Timer timer)
        {
            timer?.Resume();
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

            _timers.RemoveAll(t => t.IsDone);
        }
    }
}