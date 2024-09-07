using System;
using System.Collections.Generic;
using System.Reflection;

namespace Game.CommandLine
{
    public static class CommandCreator
    {
        public static IEnumerable<Command> CollectCommands<T>() where T : CommandAttribute
        {

            Type[] totalTypes = Assembly.GetExecutingAssembly().GetTypes();
            RegisterParameterParsers(totalTypes);
            foreach (Type type in totalTypes)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (methods.Length == 0) continue;
                foreach (MethodInfo method in methods)
                {
                    var attr = method.GetCustomAttribute<T>();
                    if (attr == null) continue;
                    if (CreateCommand(method, attr, out Command command))
                    {
                        yield return command;
                    }
                }
            }
        }

        public static void RegisterParameterParsers(Type[] totalTypes)
        {

            foreach (Type type in totalTypes)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (methods.Length == 0) continue;
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<CommandParameterParserAttribute>();
                    if (attr == null || attr.type == null) continue;
                    if (TryConvertDelegate<ParameterParser>(method, out var pp))
                    {
                        CommandParameterHandle.RegisterParser(attr.type, pp);
                    }
                }
            }
        }

        public static bool TryConvertDelegate<T>(MethodInfo methodInfo, out T result) where T : Delegate
        {
            // Check if the return types match
            result = null;
            MethodInfo delegateMethodInfo = typeof(T).GetMethod("Invoke");
            if (methodInfo.ReturnType != delegateMethodInfo.ReturnType) return false;

            // Check if the parameter types match
            var methodParams = methodInfo.GetParameters();
            var delegateParams = delegateMethodInfo.GetParameters();

            if (methodParams.Length != delegateParams.Length) return false;

            for (int i = 0; i < methodParams.Length; i++)
            {
                if (methodParams[i].ParameterType != delegateParams[i].ParameterType)
                    return false;
            }

            result = Delegate.CreateDelegate(typeof(T), null, methodInfo) as T;
            return true;
        }

        public static bool CreateCommand(MethodInfo methodInfo, CommandAttribute attr, out Command command)
        {

            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length > 0)
            {
                foreach (ParameterInfo parameter in parameters)
                {
                    if (CommandParameterHandle.ContainsParser(parameter.ParameterType)) continue;
                    command = default;
                    return false;
                }
            }

            string commandName = attr.Name ?? methodInfo.Name;
            command = new Command(commandName, methodInfo);
            return true;
        }
    }
}