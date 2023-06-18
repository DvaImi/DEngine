//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://DEngine.cn/
// Feedback: mailto:ellan@DEngine.cn
//------------------------------------------------------------

namespace Game
{
    public abstract class CSPacketBase : PacketBase
    {
        public override PacketType PacketType
        {
            get
            {
                return PacketType.ClientToServer;
            }
        }
    }
}
