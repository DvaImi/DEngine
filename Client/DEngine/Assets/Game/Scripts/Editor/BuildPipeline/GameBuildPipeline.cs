using System;
using System.Linq;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        private static Platform s_OriginalPlatform;
        public static string[] ResourceMode { get; private set; }
        public static string[] PlatformNames { get; private set; }
        public static string[] PackagesNames { get; private set; }

        static GameBuildPipeline()
        {
            ResourceMode = Enum.GetNames(typeof(ResourceMode)).Skip(1).ToArray();
            PlatformNames = Enum.GetNames(typeof(Platform)).Skip(1).ToArray();
            RefreshPackages();
        }
    }
}