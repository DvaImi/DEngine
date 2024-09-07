using System;

namespace Game.CommandLine
{
    public class CommandAttribute : Attribute
    {
        public string Name { get; private set; }

        public string Tag { get; set; }

        public CommandAttribute(string name)
        {

            this.Name = name;
            this.Tag = null;
        }
        public CommandAttribute()
        {
            this.Name = null;
            this.Tag = null;
        }

        public CommandAttribute(string name, string tag) : this(name)
        {
            Tag = tag;
        }
    }
}