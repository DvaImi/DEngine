// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 00:33:27
// 版 本：1.0
// ========================================================
using System;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

//自动生成于：2023/4/16 0:33:27
namespace GeminiLion.Hotfix
{

    public partial class WelcomeForm : HotfixUGuiForm
    {
        public Text m_Text;
        protected override void OnInit(object userdata)
        {
            base.OnInit(userdata);

        }


        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            m_Text.text += "\n";
            DRUIForm[] dataTables = GameEntry.DataTable.GetDataTable<DRUIForm>().GetAllDataRows();
            for (int i = 0; i < dataTables.Length; i++)
            {
                m_Text.text += dataTables[i].Id + "\n" + dataTables[i].AssetName + "\n" + dataTables[i].UIGroupName + "\n";
            }
            m_Text.text += GameEntry.Localization.GetString("CheckVersion.Tips");
        }
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }
    }
}
