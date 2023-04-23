// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-23 20:49:06
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GeminiLion
{
    /// <summary>
    /// 组件自动绑定工具
    /// </summary>
    public class ComponentAutoBindTool : MonoBehaviour
    {


#if UNITY_EDITOR
        [Serializable]
        public class BindData
        {
            public BindData()
            {
            }

            public BindData(string name, Component bindCom)
            {
                Name = name;
                BindCom = bindCom;
            }

            public string Name;
            public Component BindCom;
        }

        public List<BindData> BindDatas = new List<BindData>();
#endif

        [SerializeField]
        private List<Component> m_BindComs = new List<Component>();


        public T GetBindComponent<T>(int index) where T : Component
        {
            if (index >= m_BindComs.Count)
            {
                Debug.LogError("索引无效");
                return null;
            }

            T bindCom = m_BindComs[index] as T;

            if (bindCom == null)
            {
                Debug.LogError("类型无效");
                return null;
            }

            return bindCom;
        }


    }
}

