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
        public static SerializerFactory Instance = new SerializerFactory();
        
        private readonly List<Serializer> _serializers;
        private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();
        
        public readonly Serializer NULL_SERIALIZER;
        public readonly Serializer OBJECT_SERIALIZER;

        public SerializerFactory()
        {
            NULL_SERIALIZER = new StreamSerializer {
                ProcSerialize = (
                    obj,
                    writer) => {
                },
                ProcDeserialize = stream => null
            };
            
            OBJECT_SERIALIZER = new StreamSerializer {
                ProcSerialize = (
                    obj,
                    stream) => _binaryFormatter.Serialize(stream, obj),
                ProcDeserialize = stream => {
                    try {
                        return _binaryFormatter.Deserialize(stream);
                    }
                    catch (TypeLoadException e) {
                        throw new IOException("unable to deserialize object", e);
                    }
                }
            };
            
            _serializers = new List<Serializer>();
            _serializers.Add(
                new SmartSerializer<bool> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadBoolean()
                });
            _serializers.Add(
                new SmartSerializer<char> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadChar()
                });
            _serializers.Add(
                new SmartSerializer<byte> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadByte()
                });
            _serializers.Add(
                new SmartSerializer<short> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadInt16()
                });
            _serializers.Add(
                new SmartSerializer<int> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadInt32()
                });
            _serializers.Add(
                new SmartSerializer<long> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadInt64()
                });
            _serializers.Add(
                new SmartSerializer<float> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadSingle()
                });
            _serializers.Add(
                new SmartSerializer<double> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadDouble()
                });
            _serializers.Add(
                new SmartSerializer<decimal> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadDecimal()
                });
            _serializers.Add(
                new SmartSerializer<string> {
                    ProcSerialize = (
                        obj,
                        writer) => writer.Write(obj),
                    ProcDeserialize = reader => reader.ReadString()
                });
        }

        public Serializer[] GetDefaultSerializers()
        {
            List<Serializer> serializer = new List<Serializer>();
            serializer.Add(NULL_SERIALIZER);
            serializer.AddRange(_serializers);
            serializer.Add(OBJECT_SERIALIZER);
            return serializer.ToArray();
        }

        public Serializer[] GetSerializers(IEnumerable<Type> types)
        {
            return types.Select(GetSerializer).ToArray();
        }

        public Serializer GetSerializer(Type type)
        {
            if (type == null) {
                return NULL_SERIALIZER;
            }

            type = type.GetBoxedType();
            foreach (var serializer in _serializers.Where(serializer => serializer.Accepts(type))) {
                return serializer;
            }

            return OBJECT_SERIALIZER;
        }

        public byte[] Serialize(
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

        public byte[] Serialize(
            Serializer serializer,
            object @object)
        {
            using (var binaryStream = new MemoryStream()) {
                serializer.SerializeAny(@object, binaryStream);
                return binaryStream.ToArray();
            }
        }

        public object[] Deserialize(
            int numObjects,
            byte[] bytes,
            Serializer[] serializers)
        {
            Debug.Assert(serializers.Length == numObjects);

            using (var binaryStream = new MemoryStream(bytes)) {
                var result = serializers.Select(
                        serializer =>
                            serializer.DeserializeAny(binaryStream))
                    .ToArray();
                return result;
            }
        }
    }
}