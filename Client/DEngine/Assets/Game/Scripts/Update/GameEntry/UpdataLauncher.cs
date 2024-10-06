using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;
using Game.Update.Procedure;
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
            StartHotfixProcedure();
        }

        private async void StartHotfixProcedure()
        {
            Log.Info("StartHotfixProcedure...");

            GameEntry.BuiltinData.DestroyDialog();
            // 重置流程组件，初始化热更新流程。
            GameEntry.Fsm.DestroyFsm<IProcedureManager>();

            //使用当前程序集获取流程Type
            Type[] types = AssemblyUtility.GetTypes();

            List<ProcedureBase> procedures = new List<ProcedureBase>();
            foreach (var item in types)
            {
                if (item.Assembly.GetName().FullName == GetType().Assembly.FullName && typeof(ProcedureBase).IsAssignableFrom(item) && !item.IsAbstract)
                {
                    ProcedureBase procedure = (ProcedureBase)Activator.CreateInstance(item);
                    procedures.Add(procedure);
                    Log.Info("自动注册流程: " + procedure.GetType().Name);
                }
            }
            if (procedures.Count <= 0)
            {
                Log.Warning("procedures is invalid");
                return;
            }
            IProcedureManager procedureManager = DEngineEntry.GetModule<IProcedureManager>();
            IFsmManager manager = DEngineEntry.GetModule<IFsmManager>();
            procedureManager.Initialize(manager, procedures.ToArray());

            await UniTask.NextFrame();

            //在此进入热更新启动流程
            procedureManager.StartProcedure<ProcedureLanuchUpdate>();

            UnLoadLauncher();
        }

        /// <summary>
        /// 启动热更新流程后卸载热更新启动器
        /// </summary>
        private void UnLoadLauncher()
        {
            Log.Info("UnLoadLauncher...");
            Destroy(gameObject);
        }
    }
}
