namespace DEngine.Editor.ResourceTools
{
    public sealed partial class ResourceBuilderController
    {
        private sealed class AssetData
        {
            private readonly string m_Guid;
            private readonly string m_Name;
            private readonly int m_Length;
            private readonly int m_HashCode;
            private readonly string[] m_DependencyAssetNames;
            private readonly int m_MetaLength;
            private readonly int m_MetaHashCode;
            public AssetData(string guid, string name, int length, int hashCode, string[] dependencyAssetNames, int metaLength, int metaHashCode)
            {
                m_Guid = guid;
                m_Name = name;
                m_Length = length;
                m_HashCode = hashCode;
                m_DependencyAssetNames = dependencyAssetNames;
                m_MetaLength = metaLength;
                m_MetaHashCode = metaHashCode;
            }

            public string Guid
            {
                get
                {
                    return m_Guid;
                }
            }

            public string Name
            {
                get
                {
                    return m_Name;
                }
            }

            public int Length
            {
                get
                {
                    return m_Length;
                }
            }

            public int HashCode
            {
                get
                {
                    return m_HashCode;
                }
            }

            public int MetaLength
            {
                get
                {
                    return m_MetaLength;
                }
            }

            public int MetaHashCode
            {
                get
                {
                    return m_MetaHashCode;
                }
            }

            public string[] GetDependencyAssetNames()
            {
                return m_DependencyAssetNames;
            }
        }
    }
}
