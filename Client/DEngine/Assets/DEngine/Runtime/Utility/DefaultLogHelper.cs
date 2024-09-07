namespace DEngine.Runtime
{
    /// <summary>
    /// 默认游戏框架日志辅助器。
    /// </summary>
    public class DefaultLogHelper : DEngineLog.ILogHelper
    {
        public void Log(DEngineLogLevel level, object message)
        {
            switch (level)
            {
                case DEngineLogLevel.Debug:
                    Logger.Debug(message.ToString());
                    break;
                case DEngineLogLevel.Info:
                    Logger.Info(message.ToString());
                    break;
                case DEngineLogLevel.Warning:
                    Logger.Warning(message.ToString());
                    break;
                case DEngineLogLevel.Error:
                    Logger.Error(message.ToString());
                    break;
                case DEngineLogLevel.Fatal:
                    Logger.Fatal(message.ToString());
                    break;
                default:
                    throw new DEngineException(message.ToString());
            }
        }
    }
}
