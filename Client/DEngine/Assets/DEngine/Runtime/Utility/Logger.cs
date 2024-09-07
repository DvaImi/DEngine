using System;
using System.Diagnostics;
using System.Text;
using DEngine.Runtime;
using UnityEngine;
namespace DEngine
{

    /// <summary>
    /// 日志工具
    /// </summary>
    public static class Logger
    {
        private static readonly StringBuilder m_StringBuilder = new StringBuilder();
        private static readonly string m_NameColor = "#1E90FF";
        private static readonly string m_Header = "DEngine";
        /// <summary>
        /// 打印信息日志
        /// </summary>
        /// <param name="color">日志颜色</param>
        /// <param name="message">日志内容</param>
        public static void ColorInfo(Color color, string message)
        {
            string colorString = UnityEngine.ColorUtility.ToHtmlStringRGBA(color);
            string[] lines = message.Split('\n');
            string traceback = message.Replace(lines[0], "");
            Log.Info($"<color=#{colorString}>color : '{color}'   {lines[0]}</color>{traceback}");
        }

        /// <summary>
        /// 打印信息日志
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void ColorInfo(ColorType colorType, string message)
        {
            string[] lines = message.Split('\n');
            string traceback = message.Replace(lines[0], "");
            Log.Info($"<color=#{GetColor(colorType)}> {lines[0]}</color>{traceback}");
        }

        /// <summary>
        /// 打印Proto信息日志
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void ProtoColorInfo(ColorType colorType, int protoId, string message)
        {
            Log.Info($"<color=#{GetColor(colorType)}> protoId:'{protoId}'</color>  {message}");
        }

        /// <summary>
        /// 打印直线
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="startZ"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        /// <param name="endZ"></param>
        /// <param name="color"></param>
        public static void DrawLine(float startX, float startY, float startZ, float endX, float endY, float endZ, Color color)
        {
            UnityEngine.Debug.DrawLine(new Vector3(startX, startY, startZ), new Vector3(endX, endY, endZ), color);
        }

        /// <summary>
        /// 打印射线
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="startZ"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        /// <param name="endZ"></param>
        /// <param name="color"></param>
        public static void DrawRay(float startX, float startY, float startZ, float endX, float endY, float endZ, Color color)
        {
            UnityEngine.Debug.DrawRay(new Vector3(startX, startY, startZ), new Vector3(endX, endY, endZ), color);
        }

        public static void Debug(string message)
        {
            OutLog(LogLevel.DEBUG, message);
        }

        public static void Debug<T>(string message)
        {
            message = $"{typeof(T).Name}:{message}";
            OutLog(LogLevel.DEBUG, message);
        }

        public static void Info(string message)
        {
            OutLog(LogLevel.INFO, message);
        }

        public static void Info<T>(string message)
        {
            message = $"{typeof(T).Name}:{message}";
            OutLog(LogLevel.INFO, message);
        }

        public static void Assert(string message)
        {
            OutLog(LogLevel.ASSERT, message);
        }

        public static void Assert<T>(string message)
        {
            message = $"{typeof(T).Name}:{message}";
            OutLog(LogLevel.ASSERT, message);
        }

        public static void Warning(string message)
        {
            OutLog(LogLevel.WARNING, message);
        }

        public static void Warning<T>(string message)
        {
            message = $"{typeof(T).Name}:{message}";
            OutLog(LogLevel.WARNING, message);
        }

        public static void Error(string message)
        {
            OutLog(LogLevel.ERROR, message);
        }

        public static void Error<T>(string message)
        {
            message = $"{typeof(T).Name}:{message}";

            OutLog(LogLevel.ERROR, message);
        }

        public static void Fatal(string message)
        {
            OutLog(LogLevel.FATAL, message);
        }

        public static void Fatal<T>(string message)
        {
            message = $"{typeof(T).Name}:{message}";
            OutLog(LogLevel.FATAL, message);
        }

        private static void OutLog(LogLevel logLevel, string message)
        {
            StringBuilder infoBuilder = GetStringByColor(logLevel, message);
            //获取C#堆栈,Warning以上级别日志才获取堆栈
            if (logLevel == LogLevel.ERROR || logLevel == LogLevel.WARNING || logLevel == LogLevel.FATAL)
            {
                StackFrame[] stackFrames = new StackTrace().GetFrames();
                if (stackFrames != null)
                {
                    infoBuilder.Append("\n");
                    foreach (var t in stackFrames)
                    {
                        StackFrame frame = t;
                        var declaringType = frame.GetMethod().DeclaringType;
                        if (declaringType != null)
                        {
                            string declaringTypeName = declaringType.FullName;
                            string methodName = t.GetMethod().Name;
                            infoBuilder.Append($"[{declaringTypeName}::{methodName}\n");
                        }
                    }
                }
            }
            message = infoBuilder.ToString();

            if (logLevel == LogLevel.INFO || logLevel == LogLevel.DEBUG)
            {
                UnityEngine.Debug.Log(message);
            }
            else if (logLevel == LogLevel.WARNING)
            {
                UnityEngine.Debug.LogWarning(message);
            }
            else if (logLevel == LogLevel.ASSERT)
            {
                UnityEngine.Debug.LogAssertion(message);
            }
            else if (logLevel == LogLevel.ERROR)
            {
                UnityEngine.Debug.LogError(message);
            }
            else if (logLevel == LogLevel.FATAL)
            {
                UnityEngine.Debug.LogError(message);
            }
        }

        private static StringBuilder GetStringByColor(LogLevel logLevel, string logString)
        {
            m_StringBuilder.Clear();
            string[] logStrings = logString.Split('\n', 2);
            logString = logStrings[0].Trim();
            switch (logLevel)
            {
                case LogLevel.DEBUG:
                    m_StringBuilder.Append($"<color={m_NameColor}><b>[{m_Header}] ► </b></color><color=#{GetColor(ColorType.gray)}><b>[DEBUG] ► </b> {logString}</color>");
                    break;
                case LogLevel.INFO:
                    m_StringBuilder.Append($"<color={m_NameColor}><b>[{m_Header}] ► </b></color><color=#{GetColor(ColorType.white)}><b>[INFO] ► </b> {logString}</color>");
                    break;
                case LogLevel.ASSERT:
                    m_StringBuilder.Append($"<color={m_NameColor}><b>[{m_Header}] ► </b></color><color=#{GetColor(ColorType.green)}><b>[ASSERT] ► </b> {logString}</color>");
                    break;
                case LogLevel.WARNING:
                    m_StringBuilder.Append($"<color={m_NameColor}><b>[{m_Header}] ► </b></color><color=#{GetColor(ColorType.yellow)}><b>[WARNING] ► </b> {logString}</color>");
                    break;
                case LogLevel.ERROR:
                    m_StringBuilder.Append($"<color={m_NameColor}><b>[{m_Header}] ► </b></color><color=#{GetColor(ColorType.orangered)}><b>[ERROR] ► </b> {logString}</color>");
                    break;
                case LogLevel.FATAL:
                    m_StringBuilder.Append($"<color={m_NameColor}><b>[{m_Header}] ► </b></color><color=#{GetColor(ColorType.violet)}><b>[FATAL] ► </b> {logString}</color>");
                    break;
            }
            return m_StringBuilder;
        }

        public static string GetColor(ColorType colorType)
        {
            int color = (int)colorType;
            string colorString = Convert.ToString(color, 16);
            while (colorString.Length < 6)
            {
                colorString = "0" + colorString;
            }
            return colorString;
        }
    }
}