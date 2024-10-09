using System;
using System.Net;

namespace Game
{
    public static partial class GameUtility
    {
        public static class Web
        {
            public static string EscapeString(string stringToEscape)
            {
                return Uri.EscapeDataString(stringToEscape);
            }

            public static string UnescapeString(string stringToUnescape)
            {
                return Uri.UnescapeDataString(stringToUnescape);
            }
        }
    }
}