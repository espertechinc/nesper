using System;
using System.IO;

namespace com.espertech.esper.common.@internal.util.serde
{
    public class BinarySerializer<T> : Serializer
    {
        public Action<T, BinaryWriter> ProcSerialize { get; set; }
        public Func<BinaryReader, T> ProcDeserialize { get; set; }
        
        public bool Accepts(Type c)
        {
            return c.IsAssignableFrom(typeof(T));
        }

        public byte[] SerializeAny(object value)
        {
            using var stream = new MemoryStream();
            SerializeAny(value, stream);
            return stream.ToArray();
        }

        public void SerializeAny(
            object value,
            Stream stream)
        {
            if (value is T value1) {
                Serialize(value1, new BinaryWriter(stream));
            }
        }

        public void Serialize(
            T value,
            BinaryWriter writer)
        {
            ProcSerialize.Invoke(value, writer);
        }

        public object DeserializeAny(byte[] data)
        {
            return DeserializeAny(new MemoryStream(data));
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

    public class BinarySerializer
    {
        public static readonly BinarySerializer<bool> BOOLEAN =
            new BinarySerializer<bool> {
                ProcSerialize = (
                    obj,
                    writer) => writer.Write(obj),
                ProcDeserialize = reader => reader.ReadBoolean()
            };

        public static readonly BinarySerializer<char> CHARACTER =
            new BinarySerializer<char> {
                ProcSerialize = (
                    obj,
                    writer) => writer.Write(obj),
                ProcDeserialize = reader => reader.ReadChar()
            };

        public static readonly BinarySerializer<byte> BYTE =
            new BinarySerializer<byte> {
                ProcSerialize = (
                    obj,
                    writer) => writer.Write(obj),
                ProcDeserialize = reader => reader.ReadByte()
            };

        public static readonly BinarySerializer<short> INT16 =
            new BinarySerializer<short> {
                ProcSerialize = (
                    obj,
                    writer) => writer.Write(obj),
                ProcDeserialize = reader => reader.ReadInt16()
            };

        public static readonly BinarySerializer<int> INT32 =
            new BinarySerializer<int> {
                ProcSerialize = (
                    obj,
                    writer) => writer.Write(obj),
                ProcDeserialize = reader => reader.ReadInt32()
            };

        public static readonly BinarySerializer<long> INT64 =
            new BinarySerializer<long> {
                ProcSerialize = (
                    obj,
                    writer) => writer.Write(obj),
                ProcDeserialize = reader => reader.ReadInt64()
            };

        public static readonly BinarySerializer<float> SINGLE =
            new BinarySerializer<float> {
                ProcSerialize = (
                    obj,
                    writer) => writer.Write(obj),
                ProcDeserialize = reader => reader.ReadSingle()
            };

        public static readonly BinarySerializer<double> DOUBLE =
            new BinarySerializer<double> {
                ProcSerialize = (
                    obj,
                    writer) => writer.Write(obj),
                ProcDeserialize = reader => reader.ReadDouble()
            };

        public static readonly BinarySerializer<decimal> DECIMAL =
            new BinarySerializer<decimal> {
                ProcSerialize = (
                    obj,
                    writer) => writer.Write(obj),
                ProcDeserialize = reader => reader.ReadDecimal()
            };

        public static readonly BinarySerializer<string> STRING =
            new BinarySerializer<string> {
                ProcSerialize = (
                    obj,
                    writer) => writer.Write(obj),
                ProcDeserialize = reader => reader.ReadString()
            };
    }
}