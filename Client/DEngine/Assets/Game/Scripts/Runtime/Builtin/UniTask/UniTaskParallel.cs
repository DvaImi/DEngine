using Cysharp.Threading.Tasks;
using DEngine;

namespace Game
{
    public class UniTaskParallel : IReference
    {
        private readonly DEngineLinkedList<UniTask> m_Tasks = null;

        public UniTaskParallel()
        {
            m_Tasks = new DEngineLinkedList<UniTask>();
        }

        public void Push(UniTask task)
        {
            m_Tasks.AddLast(task);
        }

        public async UniTask WhenAll()
        {
            if (m_Tasks.Count <= 0)
            {
                return;
            }

            try
            {
                await UniTask.WhenAll(m_Tasks);
            }
            finally
            {
                ReferencePool.Release(this);
            }
        }

        public static UniTaskParallel Creat()
        {
            return ReferencePool.Acquire<UniTaskParallel>();
        }

        
        public static UniTaskParallel Creat(params UniTask[] uniTasks)
        {
            var parallel = ReferencePool.Acquire<UniTaskParallel>();
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
    }
}