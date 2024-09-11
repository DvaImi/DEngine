using System;
using System.Collections.Generic;
using DEngine;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        private static readonly DEngineLinkedList<GameModule> m_GameModules = new DEngineLinkedList<GameModule>();

        private void UpdateModule(float elapseSeconds, float realElapseSeconds)
        {
            foreach (GameModule module in m_GameModules)
            {
                module.Update(elapseSeconds, realElapseSeconds);
            }
        }

        public static void ShutdownModule()
        {
            for (LinkedListNode<GameModule> current = m_GameModules.Last; current != null; current = current.Previous)
            {
                current.Value.Shutdown();
            }

            m_GameModules.Clear();
        }

        public static T GetModule<T>() where T : class
        {
            Type interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
            {
                throw new DEngineException(Utility.Text.Format("You must get module by interface, but '{0}' is not.", interfaceType.FullName));
            }

            if (interfaceType.FullName != null && !interfaceType.FullName.StartsWith("Game.", StringComparison.Ordinal))
            {
                throw new DEngineException(Utility.Text.Format("You must get a Game module, but '{0}' is not.", interfaceType.FullName));
            }

            string moduleName = Utility.Text.Format("{0}.{1}", interfaceType.Namespace, interfaceType.Name[1..]);
            Type moduleType = Type.GetType(moduleName);
            if (moduleType == null)
            {
                throw new DEngineException(Utility.Text.Format("Can not find Game module type '{0}'.", moduleName));
            }

            return GetModule(moduleType) as T;
        }

        private static GameModule GetModule(Type moduleType)
        {
            foreach (GameModule module in m_GameModules)
            {
                if (module.GetType() == moduleType)
                {
                    return module;
                }
            }

            return CreateModule(moduleType);
        }

        private static GameModule CreateModule(Type moduleType)
        {
            GameModule module = (GameModule)Activator.CreateInstance(moduleType) ?? throw new DEngineException(Utility.Text.Format("Can not create module '{0}'.", moduleType.FullName));
            LinkedListNode<GameModule> current = m_GameModules.First;
            while (current != null)
            {
                if (module.Priority > current.Value.Priority)
                {
                    break;
                }

                current = current.Next;
            }

            if (current != null)
            {
                m_GameModules.AddBefore(current, module);
            }
            else
            {
                m_GameModules.AddLast(module);
            }

            return module;
        }
    }
}
