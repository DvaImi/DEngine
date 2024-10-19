using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DEngine;

public class AsyncBarrier : IReference
{
    private int m_CurrentCount;
    private int m_TotalCount;
    private UniTaskCompletionSource m_CoreCompletionSource;

    public async UniTask SignalAndWaitAsync()
    {
        int count = Interlocked.Increment(ref m_CurrentCount);

        if (count == m_TotalCount)
        {
            Interlocked.Exchange(ref m_CurrentCount, 0);

            m_CoreCompletionSource.TrySetResult();

            m_CoreCompletionSource = new UniTaskCompletionSource();
        }

        await m_CoreCompletionSource.Task;
    }

    public static AsyncBarrier Creat(int participantCount)
    {
        AsyncBarrier barrier = ReferencePool.Acquire<AsyncBarrier>();
        if (participantCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(participantCount), "The number of participants must be greater than zero.");
        }
        barrier.m_TotalCount = participantCount;
        barrier.m_CurrentCount = 0;
        barrier.m_CoreCompletionSource = new UniTaskCompletionSource();
        return barrier;
    }

    public void Clear()
    {
        Interlocked.Exchange(ref m_TotalCount, 0);
        m_CoreCompletionSource.TrySetCanceled();
        m_CoreCompletionSource = new UniTaskCompletionSource();
    }
}