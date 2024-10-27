namespace Game.CommandLine
{
    public delegate bool ParameterParser(string args, out object value);
}