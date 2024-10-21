using System;
using Cysharp.Threading.Tasks;
using DEngine;

namespace Game
{
    public class UniTaskParallel<T> : IReference, IDisposable
    {
        private readonly DEngineLinkedList<UniTask<T>> m_Tasks = null;

        public UniTaskParallel()
        {
            m_Tasks = new DEngineLinkedList<UniTask<T>>();
        }

        public void Push(UniTask<T> task)
        {
            m_Tasks.AddLast(task);
        }

        public async UniTask<T[]> WhenAll()
        {
            if (m_Tasks.Count <= 0)
            {
                return Array.Empty<T>();
            }

            return await UniTask.WhenAll(m_Tasks);
        }

        public static UniTaskParallel<T> Creat()
        {
            return ReferencePool.Acquire<UniTaskParallel<T>>();
        }

        public static UniTaskParallel<T> Creat(params UniTask<T>[] uniTasks)
        {
            var parallel = ReferencePool.Acquire<UniTaskParallel<T>>();
            foreach (var task in uniTasks)
            {
                parallel.Push(task);
            }

            return parallel;
        }

        public void Clear()
        {
            m_Tasks.Clear();
        }

        public void Dispose()
        {
            ReferencePool.Release(this);
        }
    }
}