using System.Diagnostics;
using UnityEngine.Profiling;

namespace Game
{
    /// <summary>
    /// 游戏框架Profiler分析器类。
    /// </summary>
    public class GameProfiler
    {
        /// <summary>
        /// 开始使用自定义采样分析一段代码。
        /// </summary>
        /// <param name="name">用于在Profiler窗口中标识样本的字符串。</param>
        [Conditional("ENABLE_PROFILER")]
        public static void BeginSample(string name)
        {
            Profiler.BeginSample(name);
        }

        /// <summary>
        /// 结束本次自定义采样分析。
        /// </summary>
        [Conditional("ENABLE_PROFILER")]
        public static void EndSample()
        {
            Profiler.EndSample();
        }
    }
}