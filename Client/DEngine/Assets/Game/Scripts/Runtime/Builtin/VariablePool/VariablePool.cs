using DEngine;

namespace Game
{
    public static class VariablePool
    {
        private static readonly DEngineMultiDictionary<string, Variable> Variables = new();

        public static bool HasVariable(string name)
        {
            return Variables.Contains(name);
        }

        public static bool TryGetVariable<T>(string name, out T value) where T : Variable
        {
            value = null;
            if (Variables.TryGetValue(name, out var range))
            {
                foreach (var variable in range)
                {
                    value = variable as T;
                    if (value != null) return true;
                }
            }

            return false;
        }

        public static T GetVariable<T>(string name) where T : Variable
        {
            if (Variables.TryGetValue(name, out var range))
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

        public static void SetVariable<T>(string name, T value) where T : Variable
        {
            Variables.Add(name, value);
        }

        public static bool RemoveVariable<T>(string name, T value) where T : Variable
        {
            if (Variables.Contains(name))
            {
                return Variables.Remove(name, value);
            }

            return false;
        }

        public static bool RemoveAllVariables(string name)
        {
            return Variables.RemoveAll(name);
        }

        public static void Clear()
        {
            foreach (var range in Variables)
            {
                foreach (var variable in range.Value)
                {
                    ReferencePool.Release(variable);
                }
            }

            Variables.Clear();
        }
    }
}