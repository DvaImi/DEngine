using System;
using System.Collections.Generic;
using System.Linq;
using DEngine;
using DEngine.Runtime;

namespace Game
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        private static readonly DEngineLinkedList<IGameModule> GameModules = new();
        private static readonly DEngineLinkedList<IGameUpdateModule> GameUpdateModules = new();

        private static void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var module in GameUpdateModules)
            {
                module.Update(elapseSeconds, realElapseSeconds);
            }
        }

        private static void ShutdownModule()
        {
            for (LinkedListNode<IGameModule> current = GameModules.Last; current != null; current = current.Previous)
            {
                current.Value.Shutdown();
            }

            GameModules.Clear();
        }

        /// <summary>
        /// 获取模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetModule<T>() where T : class, IGameModule
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

            var moduleTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(t => typeof(T).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).ToList();
            if (!moduleTypes.Any())
            {
                throw new InvalidOperationException($"No implementation of {typeof(T).Name} found.");
            }

            if (moduleTypes.Count > 1)
            {
                throw new InvalidOperationException($"Multiple implementations of {typeof(T).Name} found: {string.Join(", ", moduleTypes.Select(t => t.Name))}");
            }

            return GetModule(moduleTypes.First()) as T;
        }

        private static IGameModule GetModule(Type moduleType)
        {
            foreach (var module in GameModules.Where(module => module.GetType() == moduleType))
            {
                return module;
            }

            return CreateModule(moduleType);
        }

        private static IGameModule CreateModule(Type moduleType)
        {
            Log.Info("create module '{0}'", moduleType.FullName);
            IGameModule module = (IGameModule)Activator.CreateInstance(moduleType) ?? throw new DEngineException(Utility.Text.Format("Can not create module '{0}'.", moduleType.FullName));
            LinkedListNode<IGameModule> current = GameModules.First;
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
                GameModules.AddBefore(current, module);
            }
            else
            {
                GameModules.AddLast(module);
            }

            if (module is IGameUpdateModule updateModule)
            {
                LinkedListNode<IGameUpdateModule> updateCurrent = GameUpdateModules.First;
                while (updateCurrent != null)
                {
                    if (module.Priority > updateCurrent.Value.Priority)
                    {
                        break;
                    }

                    updateCurrent = updateCurrent.Next;
                }

                if (updateCurrent != null)
                {
                    GameUpdateModules.AddBefore(updateCurrent, updateModule);
                }
                else
                {
                    GameUpdateModules.AddLast(updateModule);
                }
            }

            return module;
        }
    }
}