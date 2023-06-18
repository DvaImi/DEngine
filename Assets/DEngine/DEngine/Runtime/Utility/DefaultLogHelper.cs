//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using DEngine;
using UnityEngine;

namespace DEngine.Runtime
{
    /// <summary>
    /// 默认游戏框架日志辅助器。
    /// </summary>
    public class DefaultLogHelper : DEngineLog.ILogHelper
    {
        /// <summary>
        /// 记录日志。
        /// </summary>
        /// <param name="level">日志等级。</param>
        /// <param name="message">日志内容。</param>
        public void Log(DEngineLogLevel level, object message)
        {
            switch (level)
            {
                case DEngineLogLevel.Debug:
                    Debug.Log(Utility.Text.Format("<color=#888888>{0}</color>", message));
                    break;

                case DEngineLogLevel.Info:
                    Debug.Log(message.ToString());
                    break;

                case DEngineLogLevel.Warning:
                    Debug.LogWarning(message.ToString());
                    break;

                case DEngineLogLevel.Error:
                    Debug.LogError(message.ToString());
                    break;

                default:
                    throw new DEngineException(message.ToString());
            }
        }
    }
}
