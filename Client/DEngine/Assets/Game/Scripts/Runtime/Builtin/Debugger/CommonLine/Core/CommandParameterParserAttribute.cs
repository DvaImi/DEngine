using System;

namespace Game.CommandLine
{
    public class CommandParameterParserAttribute : Attribute
    {
        public readonly Type type;
        public CommandParameterParserAttribute(Type type)
        {
            this.type = type;
        }
    }
}
