using DEngine;

namespace Game
{
    public class VariableContainer : IReference
    {
        private readonly DEngineMultiDictionary<string, Variable> m_Variables = new();

        public object Owner { get; private set; }

        public bool HasVariable(string name)
        {
            return m_Variables.Contains(name);
        }

        public bool TryGetVariable<T>(string name, out T value) where T : Variable
        {
            value = null;
            if (m_Variables.TryGetValue(name, out var range))
            {
                foreach (var variable in range)
                {
                    value = variable as T;
                    if (value != null) return true;
                }
            }

            return false;
        }

        public T GetVariable<T>(string name) where T : Variable
        {
            if (m_Variables.TryGetValue(name, out var range))
            {
                foreach (var variable in range)
                {
                    if (variable is T typedVariable)
                    {
                        return typedVariable;
                    }
                }
            }

            return default;
        }

        public void SetVariable<T>(string name, T value) where T : Variable
        {
            m_Variables.Add(name, value);
        }

        public bool RemoveVariable<T>(string name, T value) where T : Variable
        {
            if (m_Variables.Contains(name))
            {
                return m_Variables.Remove(name, value);
            }

            return false;
        }

        public bool RemoveAllVariables(string name)
        {
            return m_Variables.RemoveAll(name);
        }

        public void Clear()
        {
            foreach (var range in m_Variables)
            {
                foreach (var variable in range.Value)
                {
                    ReferencePool.Release(variable);
                }
            }

            m_Variables.Clear();
        }

        public static VariableContainer Create(object owner)
        {
            var container = ReferencePool.Acquire<VariableContainer>();
            container.Owner = owner;
            return container;
        }
    }
}