using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DEngine;

namespace Game
{
    public class UniTaskParallel<T1, T2> : IReference, IDisposable
    {
        private readonly DEngineLinkedList<UniTask<T1>> m_Tasks1 = null;
        private readonly DEngineLinkedList<UniTask<T2>> m_Tasks2 = null;

        public UniTaskParallel()
        {
            m_Tasks1 = new DEngineLinkedList<UniTask<T1>>();
            m_Tasks2 = new DEngineLinkedList<UniTask<T2>>();
        }

        public void Push(UniTask<T1> task1)
        {
            m_Tasks1.AddLast(task1);
        }

        public void Push(UniTask<T2> task2)
        {
            m_Tasks2.AddLast(task2);
        }

        public void Push(UniTask<T1> task1, UniTask<T2> task2)
        {
            m_Tasks1.AddLast(task1);
            m_Tasks2.AddLast(task2);
        }

        public async UniTask<(T1, T2)> FirstOfEach()
        {
            if (m_Tasks1.Count == 0 && m_Tasks2.Count == 0)
            {
                return (default, default);
            }

            var task1Result = UniTask.WhenAll(m_Tasks1);
            var task2Result = UniTask.WhenAll(m_Tasks2);

            return ((await task1Result).FirstOrDefault(), (await task2Result).FirstOrDefault());
        }

        public async UniTask<(T1[], T2)> AllResultsFirstOfT2()
        {
            if (m_Tasks1.Count == 0 && m_Tasks2.Count == 0)
            {
                return (Array.Empty<T1>(), default);
            }

            var task1Result = UniTask.WhenAll(m_Tasks1);
            var task2Result = UniTask.WhenAll(m_Tasks2);

            return (await task1Result, (await task2Result).FirstOrDefault());
        }

        public async UniTask<(T1, T2[])> FirstOfT1AllOfT2()
        {
            if (m_Tasks1.Count == 0 && m_Tasks2.Count == 0)
            {
                return (default, Array.Empty<T2>());
            }

            var task1Result = UniTask.WhenAll(m_Tasks1);
            var task2Result = UniTask.WhenAll(m_Tasks2);

            return ((await task1Result).FirstOrDefault(), await task2Result);
        }


        public async UniTask<(T1[], T2[])> WhenAll()
        {
            if (m_Tasks1.Count <= 0 && m_Tasks2.Count <= 0)
            {
                return (Array.Empty<T1>(), Array.Empty<T2>());
            }

            var task1Result = UniTask.WhenAll(m_Tasks1);
            var task2Result = UniTask.WhenAll(m_Tasks2);

            return (await task1Result, await task2Result);
        }


        public static UniTaskParallel<T1, T2> Creat()
        {
            return ReferencePool.Acquire<UniTaskParallel<T1, T2>>();
        }

        public static UniTaskParallel<T1, T2> Creat(UniTask<T1>[] uniTasks1, UniTask<T2>[] uniTasks2)
        {
            var parallel = ReferencePool.Acquire<UniTaskParallel<T1, T2>>();
            foreach (var task1 in uniTasks1)
            {
                parallel.m_Tasks1.AddLast(task1);
            }

            foreach (var task2 in uniTasks2)
            {
                parallel.m_Tasks2.AddLast(task2);
            }

            return parallel;
        }

        public void Clear()
        {
            m_Tasks1.Clear();
            m_Tasks2.Clear();
        }

        public void Dispose()
        {
            ReferencePool.Release(this);
        }
    }
}