namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        public static string[] PackagesNames { get; private set; }
        public static string[] VariantNames { get; private set; }

        static GameBuildPipeline()
        {
            RefreshPackages();
        }
    }
}