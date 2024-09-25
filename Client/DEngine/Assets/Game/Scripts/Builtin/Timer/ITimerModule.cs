using System;
using Cysharp.Threading.Tasks;

namespace Game.Timer
{
    public interface ITimerModule
    {
        /// <summary>
        /// 取消计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        void CancelTimer(int id);

        /// <summary>
        /// 查询是否存在计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        bool IsExistTimer(int id);

        /// <summary>
        /// 暂停计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        void PauseTimer(int id);

        /// <summary>
        /// 恢复计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        void ResumeTimer(int id);

        /// <summary>
        /// 修改定时器时间
        /// </summary>
        /// <param name="id">定时器ID</param>
        /// <param name="time">修改时间</param>
        /// <param name="isChangeRepeat">是否修改如果是RepeatTimer每次运行时间</param>
        void ChangeTime(int id, long time, bool isChangeRepeat = false);

        /// <summary>
        /// 添加执行一次的定时器
        /// </summary>
        /// <param name="time">定时时间 1000/秒</param>
        /// <param name="callback">回调函数</param>
        /// <param name="updateCallBack">每帧回调函数</param>
        /// <returns></returns>
        int AddOnceTimer(long time, Action callback, Action<long> updateCallBack = null);

        /// <summary>
        /// 可等待执行一次的定时器
        /// </summary>
        /// <param name="time">定时时间 1000/秒</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        UniTask<bool> OnceTimerAsync(long time, CancellationToken cancellationToken = null);

        /// <summary>
        /// 可等待的帧定时器
        /// </summary>
        /// <returns>定时器 ID</returns>
        UniTask<bool> FrameAsync(CancellationToken cancellationToken = null);

        /// <summary>
        /// 添加执行多次的定时器
        /// </summary>
        /// <param name="time">定时时间 1000/秒</param>
        /// <param name="repeatCount">重复次数 (小于等于零 无限次调用） </param>
        /// <param name="callback">回调函数</param>
        /// <param name="updateCallback">每帧回调函数</param>
        /// <returns>定时器 ID</returns>
        /// <exception cref="Exception">定时时间太短 无意义</exception>
        int AddRepeatedTimer(long time, int repeatCount, Action callback, Action<long> updateCallback = null);

        /// <summary>
        /// 添加执行多次的定时器
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <param name="time">定时时间 1000/秒</param>
        /// <param name="repeatCount">重复次数 (小于等于零 无限次调用） </param>
        /// <param name="callback">回调函数</param>
        /// <param name="updateCallback">每帧回调函数</param>
        /// <returns>定时器 ID</returns>
        /// <exception cref="Exception">定时时间太短 无意义</exception>
        void AddRepeatedTimer(out int id, long time, int repeatCount, Action callback, Action<long> updateCallback = null);

        /// <summary>
        /// 添加帧定时器
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <returns>定时器 ID</returns>
        int AddFrameTimer(Action callback);
    }
}