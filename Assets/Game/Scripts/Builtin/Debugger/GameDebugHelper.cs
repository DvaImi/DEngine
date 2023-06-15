// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-22 11:31:19
// 版 本：1.0
// ========================================================
using GameFramework;

namespace Game.Helper
{
    public class GameDebugHelper : GameFrameworkLog.ILogHelper
    {
        public void Log(GameFrameworkLogLevel level, object message)
        {
            switch (level)
            {
                case GameFrameworkLogLevel.Debug:
                    Logger.Debug(message.ToString(), true);
                    break;
                case GameFrameworkLogLevel.Info:
                    Logger.Info(message.ToString(), true);
                    break;
                case GameFrameworkLogLevel.Warning:
                    Logger.Warning(message.ToString(), true);
                    break;
                case GameFrameworkLogLevel.Error:
                    Logger.Error(message.ToString(), true);
                    break;
                case GameFrameworkLogLevel.Fatal:
                    Logger.Fatal(message.ToString(), true);
                    break;
                default:
                    throw new GameFrameworkException(message.ToString());
            }
        }
    }
}
