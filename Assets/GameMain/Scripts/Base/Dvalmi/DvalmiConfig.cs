using System.Linq;
using UnityEngine;

namespace Dvalmi
{
    public static class DvalmiConfig
    {
        public static readonly string NameSpace = "Dvalmi";
        public static readonly string HotfixNameSpace = "Dvalmi.Hotfix";
        public static string DataRowClassPrefixName = HotfixNameSpace + ".DR";

        public static GameFrameworkSettings GameFrameworkSettings { get; private set; }

        static DvalmiConfig()
        {
            if (GameFrameworkSettings == null)
            {
                GameFrameworkSettings = Resources.LoadAll<GameFrameworkSettings>("GameFrameworkSettings").FirstOrDefault();
            }
        }
    }
}