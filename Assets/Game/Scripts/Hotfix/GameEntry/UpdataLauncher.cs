//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using HybridCLR;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game.Hotfix
{
    /// <summary>
    /// 更新启动器
    /// </summary>
    public class UpdataLauncher : MonoBehaviour
    {
        private int m_AotLength;
        private int m_LoadedAotLength;
        private void Start()
        {
            Log.Debug("启动器加载成功...");
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
