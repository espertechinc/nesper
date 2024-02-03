using System;
using System.IO;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.util.serde
{
    public class ProxySerializer : Serializer
    {
        public Func<object, byte[]> ProcSerialize { get; set; }
        public Func<byte[], object> ProcDeserialize { get; set; }

        /// <summary>
        /// Accepts the specified c.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        public bool Accepts(Type c)
        {
            throw new UnsupportedOperationException("Not supported for serializer");
        }

        /// <summary>
        /// Serializes any.
        /// </summary>
        /// <param name="value">The value.</param>
        public byte[] SerializeAny(object value)
        {
            return ProcSerialize.Invoke(value);
        }

        /// <summary>
        /// Deserializes any.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <returns></returns>
        public object DeserializeAny(byte[] data)
        {
            return ProcDeserialize.Invoke(data);
        }

        public static readonly ProxySerializer NULL_SERIALIZER = new ProxySerializer {
            ProcSerialize = value => Array.Empty<byte>(),
            ProcDeserialize = stream => null
        };
    }
}