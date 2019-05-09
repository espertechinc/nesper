///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.util
{
    public class SerializerFactory
    {
        private static readonly List<Serializer> Serializers;
        private static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();

        public static readonly Serializer NULL_SERIALIZER = new StreamSerializer {
            ProcSerialize = (
                obj,
                writer) => {
            },
            ProcDeserialize = stream => null
        };

        public static readonly Serializer OBJECT_SERIALIZER = new StreamSerializer {
            ProcSerialize = (
                obj,
                stream) => BinaryFormatter.Serialize(stream, obj),
            ProcDeserialize = stream => {
                try {
                    return BinaryFormatter.Deserialize(stream);
                }
                catch (TypeLoadException e) {
                    throw new IOException("unable to deserialize object", e);
                }
            }
        };

        static SerializerFactory()
        {
            Serializers = new List<Serializer>();
            Serializers.Add(
                new SmartSerializer<bool> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadBoolean()
                });
            Serializers.Add(
                new SmartSerializer<char> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadChar()
                });
            Serializers.Add(
                new SmartSerializer<byte> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadByte()
                });
            Serializers.Add(
                new SmartSerializer<short> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadInt16()
                });
            Serializers.Add(
                new SmartSerializer<int> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadInt32()
                });
            Serializers.Add(
                new SmartSerializer<long> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadInt64()
                });
            Serializers.Add(
                new SmartSerializer<float> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadSingle()
                });
            Serializers.Add(
                new SmartSerializer<double> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadDouble()
                });
            Serializers.Add(
                new SmartSerializer<decimal> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadDecimal()
                });
            Serializers.Add(
                new SmartSerializer<string> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadString()
                });
        }

        public static Serializer[] GetDefaultSerializers()
        {
            List<Serializer> serializer = new List<Serializer>();
            serializer.Add(NULL_SERIALIZER);
            serializer.AddRange(Serializers);
            serializer.Add(OBJECT_SERIALIZER);
            return serializer.ToArray();
        }

        public static Serializer[] GetSerializers(IEnumerable<Type> types)
        {
            return types.Select(GetSerializer).ToArray();
        }

        public static Serializer GetSerializer(Type type)
        {
            if (type == null) {
                return NULL_SERIALIZER;
            }

            foreach (var serializer in Serializers.Where(serializer => serializer.Accepts(type.GetBoxedType()))) {
                return serializer;
            }

            return OBJECT_SERIALIZER;
        }

        public static byte[] Serialize(
            Serializer[] serializers,
            object[] objects)
        {
            Debug.Assert(serializers.Length == objects.Length);

            using (var binaryStream = new MemoryStream()) {
                for (int ii = 0; ii < objects.Length; ii++) {
                    serializers[ii].SerializeAny(objects[ii], binaryStream);
                }

                return binaryStream.ToArray();
            }
        }

        public static byte[] Serialize(
            Serializer serializer,
            object @object)
        {
            using (var binaryStream = new MemoryStream()) {
                serializer.SerializeAny(@object, binaryStream);
                return binaryStream.ToArray();
            }
        }

        public static object[] Deserialize(
            int numObjects,
            byte[] bytes,
            Serializer[] serializers)
        {
            Debug.Assert(serializers.Length == numObjects);

            using (var binaryStream = new MemoryStream(bytes)) {
                var result = serializers.Select(
                    serializer =>
                        serializer.DeserializeAny(binaryStream)).ToArray();
                return result;
            }
        }
    }
}