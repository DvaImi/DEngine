using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Fsm;
using DEngine.Procedure;
using Fantasy;
using Fantasy.Platform.Unity;
using Game.Network;
using Game.Update.Procedure;
using UnityEngine;

namespace Game.Update
{
    /// <summary>
    /// 更新启动器
    /// </summary>
    public class UpdateLauncher : MonoBehaviour
    {
        // ReSharper disable once Unity.IncorrectMethodSignature
        private async UniTaskVoid Start()
        {
            DEngine.Runtime.Log.Info("===============热更逻辑加载成功{0}==============", DateTime.Now);
            // 热更程序集加载后初始化
            AssemblyUtility.Initialize();
            if (GameDomain.Scene == null)
            {
                Log.Register(new NetworkLog());
                Entry.Initialize(AppDomain.CurrentDomain.GetAssemblies());
            }

            GameEntry.BuiltinData.DestroyDialog();
            await GameDomain.Initialize();
            await GameDomain.Scene.PublishAsync(new PreloadEventType());
            await UniTask.NextFrame();
            StartUpdateDomainProcedure();
            Destroy(gameObject);
        }

        /// <summary>
        /// 在此进入热更阈启动流程
        /// </summary>
        private void StartUpdateDomainProcedure()
        {
            // 重置流程组件，初始化热更新流程。
            GameEntry.Fsm.DestroyFsm<IProcedureManager>();

            //使用当前程序集获取流程Type
            var types = AssemblyUtility.GetTypes();

            var procedures = (from item in types where item.Assembly.GetName().FullName == GetType().Assembly.FullName && typeof(ProcedureBase).IsAssignableFrom(item) && !item.IsAbstract select (ProcedureBase)Activator.CreateInstance(item)).ToList();

            if (procedures.Count <= 0)
            {
                Log.Warning("procedures is invalid");
                return;
            }

            var procedureManager = DEngineEntry.GetModule<IProcedureManager>();
            procedureManager.Initialize(DEngineEntry.GetModule<IFsmManager>(), procedures.ToArray());
            procedureManager.StartProcedure<ProcedureEnterGameDomain>();
        }
    }
}