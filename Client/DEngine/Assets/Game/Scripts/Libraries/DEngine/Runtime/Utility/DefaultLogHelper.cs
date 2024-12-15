using UnityEngine;

namespace DEngine.Runtime
{
    /// <summary>
    /// 默认游戏框架日志辅助器。
    /// </summary>
    public class DefaultLogHelper : DEngineLog.ILogHelper
    {
        private const string NameColor = "#1E90FF";
        private const string Header = "DEngine";
        private const string Gray = "#BEBEBE";
        private const string White = "#FFFFFF";
        private const string Yellow = "#F8F80D";
        private const string Orangered = "#FF4500";
        private const string Violet = "#EE82EE";

        public void Log(DEngineLogLevel level, object message)
        {
            switch (level)
            {
                case DEngineLogLevel.Debug:
                    Debug.Log(Utility.Text.Format("<color={0}><b>[{1}] ▶ </b></color><color={2}><b>[DEBUG] ▶ </b> {3}</color>", NameColor, Header, Gray, message));
                    break;
                case DEngineLogLevel.Info:
                    Debug.Log(Utility.Text.Format("<color={0}><b>[{1}] ▶ </b></color><color={2}><b>[Info] ▶ </b> {3}</color>", NameColor, Header, White, message));
                    break;  
                case DEngineLogLevel.Warning:
                    Debug.LogWarning(Utility.Text.Format("<color={0}><b>[{1}] ▶ </b></color><color={2}><b>[Warning] ▶ </b> {3}</color>", NameColor, Header, Yellow, message));
                    break;
                case DEngineLogLevel.Error:
                    Debug.LogError(Utility.Text.Format("<color={0}><b>[{1}] ▶ </b></color><color={2}><b>[Error] ▶ </b> {3}</color>", NameColor, Header, Orangered, message));
                    break;
                case DEngineLogLevel.Fatal:
                    Debug.Log(Utility.Text.Format("<color={0}><b>[{1}] ▶ </b></color><color={2}><b>[Fatal] ▶ </b> {3}</color>", NameColor, Header, Violet, message));
                    break;
                default:
                    throw new DEngineException(message.ToString());
            }
        }
    }
}