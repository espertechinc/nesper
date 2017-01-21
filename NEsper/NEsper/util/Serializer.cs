///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using com.espertech.esper.compat;

namespace com.espertech.esper.util
{
    public interface Serializer
    {
        bool Accepts(Type c);
        void SerializeAny(Object value, Stream stream);
        Object DeserializeAny(Stream stream);
    }

    public class StreamSerializer : Serializer
    {
        public Action<Object, Stream> ProcSerialize { get; set; }
        public Func<Stream, Object> ProcDeserialize { get; set; }

        /// <summary>
        /// Acceptses the specified c.
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
        /// <param name="stream">The output stream.</param>
        public void SerializeAny(object value, Stream stream)
        {
            ProcSerialize.Invoke(value, stream);
        }

        /// <summary>
        /// Deserializes any.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <returns></returns>
        public object DeserializeAny(Stream stream)
        {
            return ProcDeserialize.Invoke(stream);
        }
    }

    public class SmartSerializer<T> : Serializer
    {
        public Action<T, BinaryWriter> ProcSerialize { get; set; }
        public Func<BinaryReader, T> ProcDeserialize { get; set; }

        public bool Accepts(Type c)
        {
            return c.IsAssignableFrom(typeof (T));
        }

        public void SerializeAny(object value, Stream stream)
        {
            if (value is T)
            {
                Serialize((T) value, new BinaryWriter(stream));
            }
        }

        public void Serialize(T value, BinaryWriter writer)
        {
            ProcSerialize.Invoke(value, writer);
        }

        public object DeserializeAny(Stream stream)
        {
            return Deserialize(new BinaryReader(stream));
        }

        public T Deserialize(BinaryReader reader)
        {
            return ProcDeserialize.Invoke(reader);
        }
    }
}
