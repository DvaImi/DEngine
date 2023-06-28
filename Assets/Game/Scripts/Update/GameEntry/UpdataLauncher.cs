using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;
using UnityEngine;

namespace Game.Update
{
    /// <summary>
    /// 更新启动器
    /// </summary>
    public class UpdataLauncher : MonoBehaviour
    {
        private void Start()
        {
            Log.Info("UpdataLauncher...");
            StartHotfixProcedure();
        }

        private void StartHotfixProcedure()
        {
            GameEntry.BuiltinData.DestroyDialog();
            // 重置流程组件，初始化热更新流程。
            GameEntry.Fsm.DestroyFsm<IProcedureManager>();
            var procedureManager = DEngine.DEngineEntry.GetModule<IProcedureManager>();
            ProcedureBase[] procedures =
            {
                new ProcedureHotfixLaunch(),
                new ProcedurePreload(),
                new ProcedureChangeScene(),
                new ProcedureMenu(),
            };
            procedureManager.Initialize(DEngine.DEngineEntry.GetModule<IFsmManager>(), procedures);

            //在此进入热更新启动流程
            procedureManager.StartProcedure<ProcedureHotfixLaunch>();
            UnLoadLauncher();
        }

        /// <summary>
        /// 启动热更新流程后卸载热更新启动器
        /// </summary>
        private void UnLoadLauncher()
        {
            Destroy(gameObject);
        }
    }
}
