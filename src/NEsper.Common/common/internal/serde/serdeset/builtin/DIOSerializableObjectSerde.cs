///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text.Json;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.util.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
    /// <summary>
    /// Serde that serializes and de-serializes.
    /// </summary>
    public class DIOSerializableObjectSerde : DataInputOutputSerde
    {
        /// <summary>
        /// Instance.
        /// </summary>
        public static readonly DIOSerializableObjectSerde INSTANCE = new DIOSerializableObjectSerde();

        // JsonSerializerOptions caches type metadata per instance; must be reused across calls
        private static readonly JsonSerializerOptions OPTIONS = new JsonSerializerOptions();

        private DIOSerializableObjectSerde()
        {
        }

        public void Write(
            object @object,
            DataOutput output,
            byte[] pageFullKey,
            EventBeanCollatedWriter writer)
        {
            SerializeTo(@object, output);
        }

        public object Read(
            DataInput input,
            byte[] resourceKey)
        {
            var size = input.ReadInt();
            var buf = new byte[size];
            input.ReadFully(buf);
            return ByteArrToObject(buf);
        }

        /// <summary>
        /// Serialize object to byte array.
        /// </summary>
        /// <param name="underlying">to serialize</param>
        /// <returns>byte array</returns>
        public static byte[] ObjectToByteArr(object underlying)
        {
            using var buffer = new PooledByteBufferWriter();
            SerializeToBuffer(underlying, buffer);
            return buffer.ToArray();
        }

        private static void SerializeToBuffer(
            object underlying,
            PooledByteBufferWriter buffer)
        {
            var writer = Utf8JsonWriterPool.Rent(buffer);
            try {
                if (underlying == null) {
                    writer.WriteNullValue();
                }
                else {
                    writer.WriteStartObject();
                    writer.WriteString("__type", underlying.GetType().FullName);
                    writer.WritePropertyName("__data");
                    JsonSerializer.Serialize(writer, underlying, OPTIONS);
                    writer.WriteEndObject();
                }

                writer.Flush();
            }
            finally {
                Utf8JsonWriterPool.Return(writer);
            }
        }

        /// <summary>
        /// Deserialize byte array to object.
        /// </summary>
        /// <param name="bytes">to read</param>
        /// <returns>object</returns>
        public static object ByteArrToObject(byte[] bytes)
        {
            var memory = new ReadOnlyMemory<byte>(bytes);
            var options = new JsonDocumentOptions();
            var document = JsonDocument.Parse(memory, options);

            var rootElement = document.RootElement;
            if (rootElement.ValueKind == JsonValueKind.Null) {
                return null;
            }

            var typeElement = rootElement.GetProperty("__type");
            var typeName = typeElement.GetString();
            if (string.IsNullOrWhiteSpace(typeName)) {
                throw new InvalidDataException();
            }

            var type = TypeHelper.ResolveType(typeName);
            var dataElement = document.RootElement.GetProperty("__data");
            return dataElement.Deserialize(type, OPTIONS);
        }

        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="value">value to serialize</param>
        /// <param name="output">output stream</param>
        /// <throws>IOException when a problem occurs</throws>
        public static void SerializeTo(
            object value,
            DataOutput output)
        {
            using var buffer = new PooledByteBufferWriter();
            SerializeToBuffer(value, buffer);
            output.WriteInt(buffer.WrittenCount);
            buffer.WriteTo(output);
        }

        /// <summary>
        /// Deserialize object
        /// </summary>
        /// <param name="input">input stream</param>
        /// <returns>value</returns>
        /// <throws>IOException when a problem occurs</throws>
        public static object DeserializeFrom(DataInput input)
        {
            var size = input.ReadInt();
            var buf = new byte[size];
            input.ReadFully(buf);

            return ByteArrToObject(buf);
        }
    }
} // end of namespace