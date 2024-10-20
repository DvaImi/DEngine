using DEngine;
using Fantasy;

namespace Game.Network
{
    public class NetworkLog : ILog
    {
        private const string NameColor = "#1E90FF";
        private const string Header = "Fantasy";
        private const string Gray = "#BEBEBE";
        private const string White = "#FFFFFF";
        private const string Yellow = "#F8F80D";
        private const string Orangered = "#FF4500";
        private const string Violet = "#EE82EE";

        private static void InternalLog(DEngineLogLevel level, object message)
        {
            switch (level)
            {
                case DEngineLogLevel.Debug:
                    UnityEngine.Debug.Log(Utility.Text.Format("<color={0}><b>[{1}] ► </b></color><color={2}><b>[DEBUG] ► </b> {3}</color>", NameColor, Header, Gray, message));
                    break;
                case DEngineLogLevel.Info:
                    UnityEngine.Debug.Log(Utility.Text.Format("<color={0}><b>[{1}] ► </b></color><color={2}><b>[Info] ► </b> {3}</color>", NameColor, Header, White, message));
                    break;
                case DEngineLogLevel.Warning:
                    UnityEngine.Debug.LogWarning(Utility.Text.Format("<color={0}><b>[{1}] ► </b></color><color={2}><b>[Warning] ► </b> {3}</color>", NameColor, Header, Yellow, message));
                    break;
                case DEngineLogLevel.Error:
                    UnityEngine.Debug.LogError(Utility.Text.Format("<color={0}><b>[{1}] ► </b></color><color={2}><b>[Error] ► </b> {3}</color>", NameColor, Header, Orangered, message));
                    break;
                case DEngineLogLevel.Fatal:
                    UnityEngine.Debug.Log(Utility.Text.Format("<color={0}><b>[{1}] ► </b></color><color={2}><b>[Fatal] ► </b> {3}</color>", NameColor, Header, Violet, message));
                    break;
                default:
                    throw new DEngineException(message.ToString());
            }
        }

        public void Trace(string message)
        {
            InternalLog(DEngineLogLevel.Debug, message);
        }

        public void Warning(string message)
        {
            InternalLog(DEngineLogLevel.Warning, message);
        }

        public void Info(string message)
        {
            InternalLog(DEngineLogLevel.Info, message);
        }

        public void Debug(string message)
        {
            InternalLog(DEngineLogLevel.Debug, message);
        }

        public void Error(string message)
        {
            InternalLog(DEngineLogLevel.Error, message);
        }

        public void Trace(string message, params object[] args)
        {
            InternalLog(DEngineLogLevel.Debug, Utility.Text.Format(message, args));
        }

        public void Warning(string message, params object[] args)
        {
            InternalLog(DEngineLogLevel.Warning, Utility.Text.Format(message, args));
        }

        public void Info(string message, params object[] args)
        {
            InternalLog(DEngineLogLevel.Info, Utility.Text.Format(message, args));
        }

        public void Debug(string message, params object[] args)
        {
            InternalLog(DEngineLogLevel.Debug, Utility.Text.Format(message, args));
        }

        public void Error(string message, params object[] args)
        {
            InternalLog(DEngineLogLevel.Error, Utility.Text.Format(message, args));
        }
    }
}