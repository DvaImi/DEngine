using DEngine;
using DEngine.Runtime;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry : MonoBehaviour
    {
        private void Start()
        {
            InitBuiltinComponents();
        }

        private void Update()
        {
            Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            ShutdownModule();
        }

        public void Shutdown(ShutdownType shutdownType)
        {
            DEngine.Runtime.GameEntry.Shutdown(shutdownType);
        }
    }
}