using System;
using System.Collections.Generic;
using System.Linq;
using DEngine;

namespace Game.CommandLine
{
    public static class CommandSystem
    {
        private static readonly Dictionary<string, Command> m_Commands = new();
        private static readonly QueryBuffer m_QueryBuffer = new(20);
        private static readonly CommandParser m_Parser = new();
        private static IConsoleRenderer m_Renderer;

        static CommandSystem()
        {
            foreach (var command in CommandCreator.CollectCommands<CommandAttribute>())
            {
                if (m_Commands.ContainsKey(command.name))
                {
                    m_Commands[command.name] = command;
                    continue;
                }
                m_Commands.Add(command.name, command);
            }

        }

        public static void Initialize(IConsoleRenderer renderer)
        {
            m_Renderer = renderer ?? throw new DEngineException("renderer is invalid");
        }

        public static IEnumerable<Command> TotalCommands
        {
            get
            {
                return m_Commands.Values;
            }
        }

        public static bool Execute(string input)
        {
            if (!m_Parser.Parse(input, out string[] result))
            {
                m_Renderer.LogError("invalid command: " + input);
                return false;
            }
            string commandName = result[0];
            if (!m_Commands.TryGetValue(commandName, out Command command))
            {
                m_Renderer.LogError("unknown command :" + commandName);
                return false;
            }
            Exception e = null;
            try
            {
                m_Renderer.Log(result);
                e = command.Execute(result);
            }
            catch (Exception)
            {
                if (e != null)
                {
                    m_Renderer.LogError(e.Message);
                    m_Renderer.LogError(e.StackTrace);
                }
                return false;
            }

            return true;
        }

        public static void Clear()
        {
            m_Renderer.Clear();
        }

        public static Command[] QueryCommands(string query, int count, Func<string, string, int> scoreFunc, string tag = null)
        {

            if (query == null || query.Length == 0)
            {
                return new Command[0];
            }

            var queryDetailInfo = query + ":" + tag ?? string.Empty;
            if (m_QueryBuffer.GetCache(queryDetailInfo, out Command[] result))
            {
                return result;
            }

            count = Math.Max(1, count);
            var bestChoices = m_Commands
            .Where(s => s.Value.CompareTag(tag))
            .Select(s => new { value = s, score = scoreFunc(query, s.Key) })
            .Where(s => s.score > 0)
            .OrderByDescending(s => s.score)
            .Take(count)
            .ToArray();
            result = bestChoices.Select(s => s.value.Value).ToArray();
            m_QueryBuffer.Cache(queryDetailInfo, result);
            return result;
        }
    }
}