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
            InitCustomsModules();
        }

        private void Update()
        {
            UpdateModule(Time.deltaTime, Time.unscaledDeltaTime);
        }
    }
}