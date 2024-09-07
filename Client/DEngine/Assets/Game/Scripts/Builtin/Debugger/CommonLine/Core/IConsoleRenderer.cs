namespace Game.CommandLine
{
    public interface IConsoleRenderer
    {
        void Log(string msg);

        void Log(string[] msg);

        void LogError(string msg);

        void LogError(string[] msg);

        void Clear();
    }
}