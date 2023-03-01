using System.Collections.Generic;
using System.Security.Cryptography;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.artifact
{
    public class InMemoryStore : IStore
    {
        private readonly IDictionary<string, byte[]> _bytesMap;
        
        public InMemoryStore()
        {
            _bytesMap = new Dictionary<string, byte[]>();
        }

        /// <summary>
        /// Retrieves the image.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public byte[] Load(string id)
        {
            _bytesMap.TryGetValue(id, out var value);
            return value;
        }

        /// <summary>
        /// Stores the image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public string Store(byte[] image)
        {
            using var hash = SHA256.Create();
            var id = hash.ComputeHash(image).ToHexString();
            _bytesMap[id] = image;
            return id;
        }
    }
}