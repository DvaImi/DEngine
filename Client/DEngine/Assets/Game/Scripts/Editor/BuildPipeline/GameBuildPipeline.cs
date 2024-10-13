using System;
using System.Linq;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        public static string[] PackagesNames { get; private set; }

        static GameBuildPipeline()
        {
            RefreshPackages();
        }
    }
}