//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;

namespace Game.Hotfix
{
    /// <summary>
    /// 热更新启动器
    /// </summary>
    public class HotfixLauncher : MonoBehaviour
    {

        private void Start()
        {
            StartHotfixProcedure();
        }

        private void StartHotfixProcedure()
        {
            GameEntry.BuiltinData.DestroyDialog();
            // 重置流程组件，初始化热更新流程。
            GameEntry.Fsm.DestroyFsm<IProcedureManager>();
            var procedureManager = GameFrameworkEntry.GetModule<IProcedureManager>();
            ProcedureBase[] procedures =
            {
                new ProcedureHotfixLaunch(),
                new ProcedurePreload(),
                new ProcedureChangeScene(),
                new ProcedureMenu(),
            };
            procedureManager.Initialize(GameFrameworkEntry.GetModule<IFsmManager>(), procedures);

            //在此进入热更新启动流程
            procedureManager.StartProcedure<ProcedureHotfixLaunch>();
            UnLoadLauncher().Forget();
        }

        /// <summary>
        /// 启动热更新流程后卸载热更新启动器
        /// </summary>
        private async UniTask UnLoadLauncher()
        {
            await UniTask.NextFrame();
            Destroy(gameObject);
        }
    }
}
