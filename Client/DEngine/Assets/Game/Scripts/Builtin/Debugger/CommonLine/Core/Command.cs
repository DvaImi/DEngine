using System;
using System.Reflection;
using DEngine;

namespace Game.CommandLine
{
    public readonly struct Command
    {
        public readonly string name;
        public readonly string tag;
        public readonly MethodInfo method;
        public readonly ParameterInfo[] parameters;
        public int ParameterCount
        {
            get
            {
                return parameters.Length;
            }
        }

        public string ParameterInfo
        {
            get
            {
                if (parameters.Length == 0)
                {
                    return "()";
                }
                string result = string.Empty;
                foreach (var parameter in parameters)
                {
                    result += $"{parameter.ParameterType.Name} {parameter.Name}, ";
                }
                return $"({result[..^2]})";
            }
        }

        public Command(string name, MethodInfo method, string tag = null)
        {

            this.name = name;
            this.method = method;
            this.tag = tag ?? string.Empty;
            parameters = method.GetParameters();
        }

        public Exception Execute(string[] args)
        {
            try
            {
                object[] loadedParams = new object[parameters.Length];
                for (int i = 0; i < loadedParams.Length; i++)
                {
                    if (i >= args.Length)
                    {
                        if (parameters[i].HasDefaultValue)
                        {
                            loadedParams[i] = parameters[i].DefaultValue;
                            continue;
                        }
                        throw new DEngineException($"parameter {parameters[i].Name} is missing");
                    }

                    if (CommandParameterHandle.ParseParameter(args[i], parameters[i].ParameterType, out loadedParams[i]))
                    {
                        continue;
                    }
                    throw new DEngineException($"parameter <{parameters[i].Name}>('{args[i]}') is invalid");
                }

                method.Invoke(null, loadedParams);
                return null;
            }
            catch (TargetInvocationException ex)
            {
                return ex.InnerException ?? ex;
            }
            catch (Exception ex)
            {

                return ex.InnerException ?? ex;
            }
        }
        public bool CompareTag(string tag)
        {
            return tag == null || tag.Length == 0 || this.tag == tag;
        }

        public override string ToString()
        {
            return name;
        }
    }

}