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
            InitCustomComponents();
        }

        public void Shutdown(ShutdownType shutdownType)
        {
            DEngine.Runtime.GameEntry.Shutdown(shutdownType);
        }
    }
}