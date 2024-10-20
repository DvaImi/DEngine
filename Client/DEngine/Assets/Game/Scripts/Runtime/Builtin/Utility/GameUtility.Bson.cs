using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Game
{
    public static partial class GameUtility
    {
        public static class Bson
        {
            private static readonly JsonSerializer Serializer;

            static Bson()
            {
                Serializer = new JsonSerializer();
            }

            public static byte[] ToBson(object value)
            {
                using var ms = new MemoryStream();
                using (var writer = new BsonDataWriter(ms))
                {
                    Serializer.Serialize(writer, value);
                }

                return ms.ToArray();
            }

            public static T FormBson<T>(byte[] value)
            {
                using var ms = new MemoryStream(value);
                using var reader = new BsonDataReader(ms);
                return Serializer.Deserialize<T>(reader);
            }
        }
    }
}