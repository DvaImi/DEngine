// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 01:20:01
// 版 本：1.0
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using UnityGameFramework.Runtime;

//自动生成于：2023/4/16 1:20:01
namespace GeminiLion.Hotfix
{

	public partial class EnitityPlaneLogic : HotfixEntityLogic
	{

		private EnitityPlaneData m_EnitityPlaneData;

		protected override void OnInit(object userdata)
		{
			base.OnInit(userdata);

		}

		protected override void OnShow(object userData)
		{
			base.OnShow(userData);
			m_EnitityPlaneData = (EnitityPlaneData)userData;
		}

		protected override void OnHide(bool isShutdown,object userData)
		{
			base.OnHide(isShutdown,userData);

			ReferencePool.Release(m_EnitityPlaneData);
		}

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (Input.GetKeyDown(KeyCode.S))
            {
                UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Restart);
            }
        }
    }
}
