using System;
using System.Collections.Generic;
using DEngine;
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
            Type[] types = Utility.Assembly.GetTypes();

            List<ProcedureBase> procedures = new List<ProcedureBase>();
            foreach (var item in types)
            {
                if (item.Assembly.GetName().FullName == GetType().Assembly.FullName && typeof(ProcedureBase).IsAssignableFrom(item) && !item.IsAbstract)
                {
                    ProcedureBase procedure = (ProcedureBase)Activator.CreateInstance(item);
                    procedures.Add(procedure);
                }
            }

            var procedureManager = DEngineEntry.GetModule<IProcedureManager>();
            procedureManager.Initialize(DEngineEntry.GetModule<IFsmManager>(), procedures.ToArray());
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
