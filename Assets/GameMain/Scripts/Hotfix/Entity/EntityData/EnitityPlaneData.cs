// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 01:20:01
// 版 本：1.0
// ========================================================
using UnityEngine;

//自动生成于：2023/4/16 1:20:01
namespace GeminiLion.Hotfix
{

	public class EnitityPlaneData : HotfixEntityData
	{

		public EnitityPlaneData()
		{
		}

		public EnitityPlaneData Fill(int typeId)
		{
			Fill(GameEntry.Entity.GenerateSerialId(),typeId);
			return this;
		}

		public override void Clear()
		{
			base.Clear();
		}

	}
}
