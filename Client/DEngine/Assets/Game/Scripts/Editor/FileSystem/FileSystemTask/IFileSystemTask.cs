namespace Game.Editor.FileSystem
{
    public interface IFileSystemTask
    {
        void Run(FileSystemTaskRunner runner);
    }
}