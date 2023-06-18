//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;

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
