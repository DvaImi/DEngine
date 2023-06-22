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
                    Logger.Debug(message.ToString(), true);
                    break;
                case DEngineLogLevel.Info:
                    Logger.Info(message.ToString(), true);
                    break;
                case DEngineLogLevel.Warning:
                    Logger.Warning(message.ToString(), true);
                    break;
                case DEngineLogLevel.Error:
                    Logger.Error(message.ToString(), true);
                    break;
                case DEngineLogLevel.Fatal:
                    Logger.Fatal(message.ToString(), true);
                    break;
                default:
                    throw new DEngineException(message.ToString());
            }
        }
    }
}
