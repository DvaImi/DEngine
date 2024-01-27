using System;
using System.Text;
using DEngine.Debugger;
using Game.CommandLine;
using UnityEngine;

namespace Game.Debugger
{
    public class CommonLineDebuggerWindow : IDebuggerWindow, IConsoleRenderer
    {
        private Vector2 m_ScrollPosition = Vector2.zero;
        private string m_InputCommonline;
        private StringBuilder m_Commonline;
        public void Initialize(params object[] args)
        {
            m_Commonline = new StringBuilder();
            CommandSystem.Initialize(this);
        }

        public void Shutdown()
        {

        }

        public void OnEnter()
        {
        }

        public void OnLeave()
        {
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            
        }

        public void OnDraw()
        {
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
            {
                DrawCommonLine();
            }
            GUILayout.EndScrollView();
        }

        private void DrawCommonLine()
        {
            GUILayout.Label("<b>CommonLine</b>");
            GUILayout.BeginVertical("box", GUILayout.MinHeight(300));
            {
                GUILayout.Label(m_Commonline.ToString());
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                {
                    m_InputCommonline = GUILayout.TextField(m_InputCommonline);
                    if (GUILayout.Button("Submit", GUILayout.Width(100)))
                    {
                        CommandSystem.Execute(m_InputCommonline);
                        m_InputCommonline = null;
                    }
                    if (GUILayout.Button("Clear", GUILayout.Width(100)))
                    {
                        CommandSystem.Clear();
                        m_InputCommonline = null;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        public void Log(string msg)
        {
            m_Commonline.Append("[");
            m_Commonline.Append(DateTime.Now.ToString("G"));
            m_Commonline.Append("]");
            m_Commonline.Append(" ");
            m_Commonline.Append(msg);
            m_Commonline.Append("\n");
        }

        public void Log(string[] msgs)
        {
            for (int i = 0; i < msgs.Length; i++)
            {
                Log(msgs[i]);
            }
        }

        public void LogError(string msg)
        {
            Log($"<color=#FF4500>{msg}</color>");
        }

        public void LogError(string[] msgs)
        {
            string msg = string.Empty;
            foreach (string line in msgs)
            {
                msg += $"<color=#FF4500>{msg}</color>";
            }

            Log(msg);
        }

        public void Clear()
        {
            m_Commonline.Clear();
        }

        [Command]
        static void TestCommon()
        {
            Debug.Log("common");
        }
    }
}