using System;
using System.Collections.Generic;
namespace Game.CommandLine
{

    public static class CommandParameterHandle
    {
        static readonly Dictionary<Type, ParameterParser> DefaultParseFunctions;
        static readonly Dictionary<Type, ParameterParser> CustomParseFunctions;

        static CommandParameterHandle()
        {

            DefaultParseFunctions = new Dictionary<Type, ParameterParser>();
            CustomParseFunctions = new Dictionary<Type, ParameterParser>();

            DefaultParseFunctions.Add(typeof(int), ParseInt);
            DefaultParseFunctions.Add(typeof(string), ParseString);
            DefaultParseFunctions.Add(typeof(float), ParseFloat);
            DefaultParseFunctions.Add(typeof(double), ParseDouble);
            DefaultParseFunctions.Add(typeof(bool), ParseBool);
            DefaultParseFunctions.Add(typeof(char), ParseChar);
            DefaultParseFunctions.Add(typeof(byte), ParseByte);
            DefaultParseFunctions.Add(typeof(short), ParseShort);
            DefaultParseFunctions.Add(typeof(long), ParseLong);
            DefaultParseFunctions.Add(typeof(ushort), ParseUShort);
            DefaultParseFunctions.Add(typeof(uint), ParseUInt);
            DefaultParseFunctions.Add(typeof(ulong), ParseULong);
            DefaultParseFunctions.Add(typeof(decimal), ParseDecimal);
            DefaultParseFunctions.Add(typeof(sbyte), ParseSByte);
        }

        static bool ParseInt(string args, out object value)
        {
            if (int.TryParse(args, out int result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        static bool ParseString(string args, out object value)
        {
            value = args ?? string.Empty;
            return true;
        }

        static bool ParseFloat(string args, out object value)
        {
            if (float.TryParse(args, out float result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        static bool ParseDouble(string args, out object value)
        {
            if (double.TryParse(args, out double result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        static bool ParseBool(string args, out object value)
        {
            if (bool.TryParse(args, out bool result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        static bool ParseChar(string args, out object value)
        {
            if (char.TryParse(args, out char result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        static bool ParseByte(string args, out object value)
        {
            if (byte.TryParse(args, out byte result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        static bool ParseShort(string args, out object value)
        {
            if (short.TryParse(args, out short result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        static bool ParseLong(string args, out object value)
        {
            if (long.TryParse(args, out long result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        static bool ParseUShort(string args, out object value)
        {
            if (ushort.TryParse(args, out ushort result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        static bool ParseUInt(string args, out object value)
        {
            if (uint.TryParse(args, out uint result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        static bool ParseULong(string args, out object value)
        {
            if (ulong.TryParse(args, out ulong result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        static bool ParseDecimal(string args, out object value)
        {
            if (decimal.TryParse(args, out decimal result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        static bool ParseSByte(string args, out object value)
        {
            if (sbyte.TryParse(args, out sbyte result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }


        public static bool RegisterParser(Type type, ParameterParser parser)
        {
            if (type == null || parser == null) return false;
            if (DefaultParseFunctions.ContainsKey(type)) return false;
            if (CustomParseFunctions.ContainsKey(type))
            {
                CustomParseFunctions[type] = parser;
            }
            else
            {
                CustomParseFunctions.Add(type, parser);
            }
            return true;
        }

        public static bool ContainsParser(Type type)
        {
            return type != null && (DefaultParseFunctions.ContainsKey(type) || CustomParseFunctions.ContainsKey(type));
        }

        public static bool ParseParameter(string args, Type type, out object value)
        {
            value = null;
            if (DefaultParseFunctions.TryGetValue(type, out var parser))
            {
                return parser(args, out value);
            }
            if (!CustomParseFunctions.TryGetValue(type, out parser)) return false;
            try
            {
                return parser(args, out value);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

}