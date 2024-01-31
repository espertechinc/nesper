///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util.serde;
using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Utility class for copying serializable objects via object input and output streams.
    /// </summary>
    public class SerializableObjectCopier : IObjectCopier
    {
        private readonly IContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableObjectCopier"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public SerializableObjectCopier(IContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Determines whether the copying the specified type is supported.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// <c>true</c> if the specified type is supported; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSupported(Type type)
        {
            // previously, we only allowed objects marked with the "Serializable" attribute to
            // be serialized, but now we basically allow anything.
            // return type.IsSerializable || type.IsInterface;
            
            return true;
        }

        /// <summary>
        /// Deep copies the input object.
        /// </summary>
        /// <param name="orig">is the object to be copied, must be serializable</param>
        /// <returns>copied object</returns>
        /// <throws>IOException if the streams returned an exception</throws>
        /// <throws>TypeLoadException if the de-serialize fails</throws>
        public T Copy<T>(T orig)
        {
#if NET6_0_OR_GREATER
            TypeResolver typeResolver;

            if (_container.Has<TypeResolver>()) {
                typeResolver = _container.Resolve<TypeResolver>();
            } else if (_container.Has<TypeResolverProvider>()) {
                typeResolver = _container.Resolve<TypeResolverProvider>().TypeResolver;
            } else {
                typeResolver = TypeResolverDefault.INSTANCE;
            }

            lock (_container) {
                if (!_container.Has<ObjectSerializer>()) {
                    _container.Register<ObjectSerializer>(ic => new ObjectSerializer(typeResolver), Lifespan.Singleton);
                }
            }

            var serializer = _container.Resolve<ObjectSerializer>();
            //var serializer = new ObjectSerializer(typeResolver);
            var serialized = serializer.SerializeAny(orig);
            var deserialized = serializer.DeserializeAny(serialized);
            return (T)deserialized;
#else
            // Create the formatter
            var formatter = new BinaryFormatter();
            formatter.FilterLevel = TypeFilterLevel.Full;
            formatter.AssemblyFormat = FormatterAssemblyStyle.Full;
            formatter.TypeFormat = FormatterTypeStyle.TypesAlways;
            formatter.Context = new StreamingContext(StreamingContextStates.Clone, container);
            formatter.Binder = new TypeSerializationBinder();
            formatter.SurrogateSelector = new TypeSurrogateSelector();

            using (var stream = new MemoryStream()) {
                // Serialize the object graph to the stream
                formatter.Serialize(stream, orig);
                // Rewind the stream
                stream.Seek(0, SeekOrigin.Begin);
                // Deserialize the object graph from the stream
                return (T)formatter.Deserialize(stream);
            }
#endif
        }

        public static IObjectCopier GetInstance(IContainer container)
        {
            return container.ResolveSingleton<IObjectCopier>(
                () => new SerializableObjectCopier(container));
        }

        public static T CopyMayFail<T>(
            IContainer container,
            T input)
        {
            return GetInstance(container).Copy<T>(input);
        }

        /// <summary>
        /// TypeBinder is used during deserialization to determine which "type" to use for a given assembly name and
        /// type name.  When presented with System.RuntimeType, we return our RuntimeTypeSurrogate.  This type is then
        /// presented to the TypeSurrogateSelector which in-turn uses our RuntimeTypeSurrogate.
        /// </summary>
        public class TypeSerializationBinder : SerializationBinder
        {
            public override Type BindToType(
                string assemblyName,
                string typeName)
            {
                var simpleResolve = Type.GetType($"{typeName}, {assemblyName}");
                if (simpleResolve != null) {
                    return simpleResolve;
                }

                var assembly = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(_ => _.FullName == assemblyName);
                if (assembly == null) {
                    return null;
                }

                var typeFromAssembly = assembly.GetType(typeName);
                if (typeFromAssembly != null) {
                    return typeFromAssembly;
                }

                var typeResolve = Type.GetType(typeName);
                if (typeResolve != null) {
                    return typeResolve;
                }

                return null;
            }
        }

        /// <summary>
        /// Surrogate for handling RuntimeType.
        /// </summary>
        public class RuntimeTypeSerializationSurrogate : ISerializationSurrogate
        {
            private const string ASSEMBLY_QUALIFIED_NAME = nameof(Type.AssemblyQualifiedName);

            public void GetObjectData(
                object obj,
                SerializationInfo info,
                StreamingContext context)
            {
                var objAsType = obj as Type;
                info.AddValue(ASSEMBLY_QUALIFIED_NAME, objAsType?.AssemblyQualifiedName);
            }

            public object SetObjectData(
                object obj,
                SerializationInfo info,
                StreamingContext context,
                ISurrogateSelector selector)
            {
                var assemblyQualifiedName = info.GetString(ASSEMBLY_QUALIFIED_NAME);
                return Type.GetType(assemblyQualifiedName);
            }
        }

        /// <summary>
        /// Determines which surrogate to use for a given type.  Here, we are only interested in types
        /// derived from System.Type (e.g. System.RuntimeType) as these are not serializable under .NET Core.
        /// </summary>
        public class TypeSurrogateSelector : ISurrogateSelector
        {
            public virtual void ChainSelector(ISurrogateSelector selector)
            {
                throw new NotSupportedException();
            }

            public virtual ISurrogateSelector GetNextSelector()
            {
                throw new NotSupportedException();
            }

            public virtual ISerializationSurrogate GetSurrogate(
                Type type,
                StreamingContext context,
                out ISurrogateSelector selector)
            {
                if (typeof(Type).IsAssignableFrom(type)) {
                    selector = this;
                    return new RuntimeTypeSerializationSurrogate();
                }

                selector = null;
                return null;
            }
        }
    }
}