﻿using DEngine.Debugger;
using UnityEngine;

namespace Game.Debugger
{
    public abstract class ScrollableDebuggerWindowBase : IDebuggerWindow
    {
        private Vector2 m_ScrollPosition = Vector2.zero;

        public virtual void Initialize(params object[] args)
        {
        }

        public virtual void Shutdown()
        {
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnLeave()
        {
        }

        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void OnDraw()
        {
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
            {
                OnDrawScrollableWindow();
            }
            GUILayout.EndScrollView();
        }

        protected abstract void OnDrawScrollableWindow();
    }
}