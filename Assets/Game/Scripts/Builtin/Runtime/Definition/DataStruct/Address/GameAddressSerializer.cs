using System;
using System.Collections.Generic;
using GameFramework;

namespace Game
{
    public class GameAddressSerializer : GameFrameworkSerializer<Dictionary<string, Dictionary<Type, string>>>
    {
        private static readonly byte[] m_Header = new byte[] { (byte)'A', (byte)'S', (byte)'D' };

        public GameAddressSerializer()
        {

        }

        protected override byte[] GetHeader()
        {
            return m_Header;
        }
    }
}
