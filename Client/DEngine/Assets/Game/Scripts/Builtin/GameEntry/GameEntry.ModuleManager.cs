using System;
using System.Collections.Generic;
using System.Reflection;
using DEngine;
using DEngine.Runtime;

namespace Game
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        private static readonly DEngineLinkedList<IGameModule> m_GameModules = new DEngineLinkedList<IGameModule>();

        private void UpdateModule(float elapseSeconds, float realElapseSeconds)
        {
            foreach (IGameModule module in m_GameModules)
            {
                module.Update(elapseSeconds, realElapseSeconds);
            }
        }

        public static void ShutdownModule()
        {
            for (LinkedListNode<IGameModule> current = m_GameModules.Last; current != null; current = current.Previous)
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
            Type moduleType = Utility.Assembly.GetType(moduleName);
            if (moduleType == null)
            {
                throw new DEngineException(Utility.Text.Format("Can not find Game module type '{0}'.", moduleName));
            }

            return GetModule(moduleType) as T;
        }

        public static T GetModule<T>(Assembly assembly) where T : class
        {
            if (assembly == null)
            {
                throw new DEngineException("Can not find assembly .");
            }

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
            Type moduleType = assembly.GetType(moduleName);
            if (moduleType == null)
            {
                throw new DEngineException(Utility.Text.Format("Can not find Game module type '{0}'.", moduleName));
            }

            return GetModule(moduleType) as T;
        }

        public static IGameModule GetModule(Type moduleType)
        {
            foreach (IGameModule module in m_GameModules)
            {
                if (module.GetType() == moduleType)
                {
                    return module;
                }
            }

            return CreateModule(moduleType);
        }

        private static IGameModule CreateModule(Type moduleType)
        {
            Log.Info("create module '{0}'", moduleType.FullName);
            IGameModule module = (IGameModule)Activator.CreateInstance(moduleType) ?? throw new DEngineException(Utility.Text.Format("Can not create module '{0}'.", moduleType.FullName));
            LinkedListNode<IGameModule> current = m_GameModules.First;
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