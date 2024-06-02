using Game.Archive;
using UnityEngine;

namespace Game.Archive
{
    public record ArchivePlayer : IArchiveData
    {
        private readonly string m_Identifier;
        private readonly string m_UniqueId;

        public ArchivePlayer(string identifier, string uniqueId)
        {
            m_Identifier = identifier;
            m_UniqueId = uniqueId;
        }

        public string Identifier
        {
            get => m_Identifier;
        }

        public string UniqueId
        {
            get => m_UniqueId;
        }

        public int Hp { get; set; }

        public string Name { get; set; }

        public Vector2 Pos { get; set; }

        public Quaternion Quaternion { get; set; }
    }

    public record TestData2 : IArchiveData
    {
        private readonly string m_Identifier;
        private readonly string m_UniqueId;

        public TestData2(string identifier, string uniqueId)
        {
            m_Identifier = identifier;
            m_UniqueId = uniqueId;
        }

        public string Identifier
        {
            get => m_Identifier;
        }

        public string UniqueId
        {
            get => m_UniqueId;
        }

        public float Speed { get; set; }

        public float Frequency { get; set; }
    }
}