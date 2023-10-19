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

using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.util.serde
{
    public class SerializerFactory
    {
        private readonly IList<Serializer> _serializers;
        private readonly Serializer _defaultSerializer;

        public static Serializer CreateDefaultSerializer(IContainer container)
        {
            var typeResolver = container.TypeResolver() ??
                               container.TypeResolverProvider()?.TypeResolver;

            return new ObjectSerializer(typeResolver);
        }

        public IList<Serializer> Serializers => _serializers;

        public Serializer DefaultSerializer => _defaultSerializer;

        public SerializerFactory(Serializer defaultSerializer)
        {
            _defaultSerializer = defaultSerializer;
            _serializers = new List<Serializer>();
            _serializers.Add(BinarySerializer.BOOLEAN);
            _serializers.Add(BinarySerializer.CHARACTER);
            _serializers.Add(BinarySerializer.BYTE);
            _serializers.Add(BinarySerializer.INT16);
            _serializers.Add(BinarySerializer.INT32);
            _serializers.Add(BinarySerializer.INT64);
            _serializers.Add(BinarySerializer.SINGLE);
            _serializers.Add(BinarySerializer.DOUBLE);
            _serializers.Add(BinarySerializer.DECIMAL);
            _serializers.Add(BinarySerializer.STRING);
        }

        public SerializerFactory(IContainer container) :
            this(CreateDefaultSerializer(container))
        {
        }

        public Serializer[] GetSerializers(IEnumerable<Type> types)
        {
            return types.Select(GetSerializer).ToArray();
        }

        public Serializer GetSerializer(Type type)
        {
            if (type == null) {
                return ProxySerializer.NULL_SERIALIZER;
            }

            type = type.GetBoxedType();
            foreach (var serializer in _serializers.Where(serializer => serializer.Accepts(type))) {
                return serializer;
            }

            return _defaultSerializer;
        }

        public byte[] SerializeAndFlatten(
            Serializer[] serializers,
            object[] objects)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            foreach (var datum in Serialize(serializers, objects)) {
                writer.Write(datum.Length);
                writer.Write(datum);
            }

            writer.Flush();
            stream.Flush();

            return stream.ToArray();
        }

        public byte[][] Serialize(
            Serializer[] serializers,
            object[] objects)
        {
            Debug.Assert(serializers.Length == objects.Length);
            return serializers
                .Zip(objects)
                .Select(_ => _.First.SerializeAny(_.Second))
                .ToArray();
        }

        public byte[] Serialize(
            Serializer serializer,
            object @object)
        {
            return serializer.SerializeAny(@object);
        }

        public object[] Deserialize(
            int numObjects,
            byte[][] bytes,
            Serializer[] serializers)
        {
            Debug.Assert(serializers.Length == numObjects);
            return serializers
                .Zip(bytes)
                .Select(_ => _.First.DeserializeAny(_.Second))
                .ToArray();
        }
        
        public object Deserialize(
            byte[] bytes,
            Serializer serializer)
        {
            return serializer.DeserializeAny(bytes);
        }
    }
}