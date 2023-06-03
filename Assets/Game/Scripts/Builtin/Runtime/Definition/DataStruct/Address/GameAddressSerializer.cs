using System;
using System.Collections.Generic;
using GameFramework;

namespace Game
{
    public class GameAddressSerializer : GameFrameworkSerializer<Dictionary<string, string>>
    {
        private static readonly byte[] Header = new byte[] { (byte)'A', (byte)'S', (byte)'D' };

        public GameAddressSerializer()
        {

        }

        protected override byte[] GetHeader()
        {
            return Header;
        }
    }
}
