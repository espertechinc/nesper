///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.deserializers.core;
using com.espertech.esper.common.@internal.@event.json.deserializers.forge;
using com.espertech.esper.common.@internal.@event.json.serializers.forge;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.json.forge
{
    public class JsonForgeFactoryBuiltinClassTyped
    {
        public static readonly JsonForgeFactoryBuiltinClassTyped INSTANCE = new JsonForgeFactoryBuiltinClassTyped();

        private readonly IDictionary<Type, JsonSerializationForgePair> _forgePairs =
            new Dictionary<Type, JsonSerializationForgePair>();

        private JsonForgeFactoryBuiltinClassTyped()
        {
            InitializeBuiltinForgePairs();
            InitializeCommonForgePairs();
        }

        private void InitializeBuiltinForgePairs()
        {
            _forgePairs[typeof(string)] = new JsonSerializationForgePair(
                JsonSerializerForgeString.INSTANCE,
                JsonDeserializerForgeString.INSTANCE);
            _forgePairs[typeof(char?)] = new JsonSerializationForgePair(
                JsonSerializerForgeStringWithToString.INSTANCE,
                JsonDeserializerForgeCharacter.INSTANCE);
            _forgePairs[typeof(bool?)] = new JsonSerializationForgePair(
                JsonSerializerForgeBoolean.INSTANCE,
                JsonDeserializerForgeBoolean.INSTANCE);
            _forgePairs[typeof(byte?)] = new JsonSerializationForgePair(
                JsonSerializerForgeNumber.INSTANCE,
                JsonDeserializerForgeByte.INSTANCE);
            _forgePairs[typeof(short?)] = new JsonSerializationForgePair(
                JsonSerializerForgeNumber.INSTANCE,
                JsonDeserializerForgeInt16.INSTANCE);
            _forgePairs[typeof(int?)] = new JsonSerializationForgePair(
                JsonSerializerForgeNumber.INSTANCE,
                JsonDeserializerForgeInt32.INSTANCE);
            _forgePairs[typeof(long?)] = new JsonSerializationForgePair(
                JsonSerializerForgeNumber.INSTANCE,
                JsonDeserializerForgeInt64.INSTANCE);
            _forgePairs[typeof(float?)] = new JsonSerializationForgePair(
                JsonSerializerForgeNumber.INSTANCE,
                JsonDeserializerForgeSingle.INSTANCE);
            _forgePairs[typeof(double?)] = new JsonSerializationForgePair(
                JsonSerializerForgeNumber.INSTANCE,
                JsonDeserializerForgeDouble.INSTANCE);
            _forgePairs[typeof(decimal?)] = new JsonSerializationForgePair(
                JsonSerializerForgeNumber.INSTANCE,
                JsonDeserializerForgeDecimal.INSTANCE);
            _forgePairs[typeof(BigInteger?)] = new JsonSerializationForgePair(
                JsonSerializerForgeNumber.INSTANCE,
                JsonDeserializerForgeBigInteger.INSTANCE);
            _forgePairs[typeof(Guid)] = new JsonSerializationForgePair(
                JsonSerializerForgeStringWithToString.INSTANCE,
                JsonDeserializerForgeUuid.INSTANCE);
            _forgePairs[typeof(DateTimeEx)] = new JsonSerializationForgePair(
                JsonSerializerForgeStringWithToString.INSTANCE,
                JsonDeserializerForgeDateTimeEx.INSTANCE);
            _forgePairs[typeof(DateTimeOffset)] = new JsonSerializationForgePair(
                JsonSerializerForgeStringWithToString.INSTANCE,
                JsonDeserializerForgeDateTimeOffset.INSTANCE);
            _forgePairs[typeof(DateTime)] = new JsonSerializationForgePair(
                JsonSerializerForgeStringWithToString.INSTANCE,
                JsonDeserializerForgeDateTime.INSTANCE);
            _forgePairs[typeof(Uri)] = new JsonSerializationForgePair(
                JsonSerializerForgeStringWithToString.INSTANCE,
                JsonDeserializerForgeUri.INSTANCE);
        }

        private void InitializeCommonForgePairs()
        {
            _forgePairs[typeof(IDictionary<string, object>)] = new JsonSerializationForgePair(
                new JsonSerializerForgeByMethod("WriteJsonMap"),
                new JsonDeserializerForgeByClass(typeof(JsonDeserializerGenericObject)));
            _forgePairs[typeof(object[])] = new JsonSerializationForgePair(
                new JsonSerializerForgeByMethod("WriteJsonArray"),
                new JsonDeserializerForgeByClass(typeof(JsonDeserializerGenericArray)));
            _forgePairs[typeof(object)] = new JsonSerializationForgePair(
                new JsonSerializerForgeByMethod("WriteJsonValue"),
                new JsonDeserializerForgeByClass(typeof(JsonDeserializerGenericObject)));
        }

        private JsonSerializationForgePair GetForgePair(
            Type type,
            IDictionary<Type, JsonApplicationClassSerializationDesc> classSerializationDescs)
        {
            type = type.GetBoxedType();

            // Search for a pre-built pair.  Please change this logic so that built-in types are
            // done in pairs.

            if (_forgePairs.TryGetValue(type, out var forgePair)) {
                return forgePair;
            }

            // The type was not found as a pre-built pair.  Examine the data type and attempt to create
            // the right pair of forges for this type.

            if (type.IsEnum) {
                return new JsonSerializationForgePair(
                    JsonSerializerForgeStringWithToString.INSTANCE,
                    new JsonDeserializerForgeEnum(type));
            }

            if (type.IsArray) {
                var componentType = type.GetElementType();
                var componentForgePair = GetForgePair(componentType, classSerializationDescs);
                return new JsonSerializationForgePair(
                    new JsonSerializerForgeArray(componentForgePair.SerializerForge, componentType),
                    new JsonDeserializerForgeArray(componentForgePair.DeserializerForge, componentType));
            }

            if (type.IsGenericStringDictionary()) {
                var valueType = type.GetDictionaryValueType();
                var valueTypeForgePair = GetForgePair(valueType, classSerializationDescs);

                return new JsonSerializationForgePair(
                    new JsonSerializerForgePropertyMap(
                        valueTypeForgePair.SerializerForge,
                        valueType),
                    new JsonDeserializerForgePropertyMap(
                        valueTypeForgePair.DeserializerForge,
                        valueType));
            }

            if (type.IsGenericList() || type.IsGenericEnumerable()) {
                var genericType = type.GetComponentType();
                if (genericType == null) {
                    return null;
                }

                var genericForgePair = GetForgePair(genericType, classSerializationDescs);
                return new JsonSerializationForgePair(
                    new JsonSerializerForgeArray(genericForgePair.SerializerForge, genericType),
                    new JsonDeserializerForgeArray(genericForgePair.DeserializerForge, genericType));
            }

            if (classSerializationDescs.TryGetValue(type, out var existingDesc)) {
                return new JsonSerializationForgePair(
                    new JsonSerializerForgeByClassName(existingDesc.SerializerClassName),
                    new JsonDeserializerForgeByClassName(existingDesc.DeserializerClassName));
            }

            //throw new ArgumentException($"unable to determine forge pair for type {type.CleanName()}");

            return null;
        }

        public JsonForgeDesc Forge(
            Type type,
            string fieldName,
            FieldInfo optionalField,
            IDictionary<Type, JsonApplicationClassSerializationDesc> deepClasses,
            Attribute[] annotations,
            StatementCompileTimeServices services)
        {
            type = type.GetBoxedType();

            // Determine the serializer and deserializer that should be used for this field.  The field annotations
            // can be used to specify custom serialization settings.

            var fieldAnnotation = FindFieldAnnotation(fieldName, annotations);
            if (fieldAnnotation != null && type != null) {
                var customForgePair = GetCustomSchemaAdapter(
                    services,
                    type,
                    fieldAnnotation);
                return new JsonForgeDesc(
                    customForgePair.DeserializerForge,
                    customForgePair.SerializerForge);
            }

            var forgePair = GetForgePair(type, deepClasses);
            if (forgePair == null) {
                throw GetUnsupported(type, fieldName);
            }

            return new JsonForgeDesc(
                forgePair.DeserializerForge,
                forgePair.SerializerForge);
            // throw new NotImplementedException("broken: db2166ea-3654-449e-8dfb-b01f921f5ea6");
        }

        /// <summary>
        /// Given a field and a set of annotations, determines if we should use a custom parse adapter.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="type"></param>
        /// <param name="fieldAnnotation"></param>
        /// <returns></returns>
        /// <exception cref="ExprValidationException"></exception>
        private JsonSerializationForgePair GetCustomSchemaAdapter(
            StatementCompileTimeServices services,
            Type type,
            JsonSchemaFieldAttribute fieldAnnotation)
        {
            Type clazz;
            try {
                clazz = services.ImportServiceCompileTime.ResolveType(
                    fieldAnnotation.Adapter,
                    true,
                    ExtensionClassEmpty.INSTANCE);
            }
            catch (ImportException e) {
                throw new ExprValidationException($"Failed to resolve Json schema field adapter class: {e.Message}", e);
            }

            if (clazz.FindGenericInterface(typeof(JsonFieldAdapterString<>)) == null) {
                throw new ExprValidationException(
                    $"Json schema field adapter class does not implement interface '{typeof(JsonFieldAdapterString<>).CleanName()}");
            }

            if (!clazz.HasDefaultConstructor()) {
                throw new ExprValidationException(
                    $"Json schema field adapter class '{clazz.CleanName()}' does not have a default constructor");
            }

            MethodInfo writeMethod;
            try {
                writeMethod = MethodResolver.ResolveMethod(
                    clazz,
                    "Parse",
                    new Type[] { typeof(string) },
                    true,
                    new bool[1],
                    new bool[1]);
            }
            catch (MethodResolverNoSuchMethodException e) {
                throw new ExprValidationException(
                    $"Failed to resolve write method of Json schema field adapter class: {e.Message}",
                    e);
            }

            if (!TypeHelper.IsSubclassOrImplementsInterface(type, writeMethod.ReturnType)) {
                throw new ExprValidationException(
                    $"Json schema field adapter class '{clazz.CleanName()}' mismatches the return type of the parse method, " +
                    $"expected '{type.CleanName()}' but found '{writeMethod.ReturnType.CleanName()}'");
            }

            return new JsonSerializationForgePair(
                new JsonSerializerForgeProvidedStringAdapter(clazz),
                new JsonDeserializerForgeProvidedAdapter(clazz));
        }

        private JsonSchemaFieldAttribute FindFieldAnnotation(
            string fieldName,
            Attribute[] annotations)
        {
            if (annotations == null || annotations.Length == 0) {
                return null;
            }

            return annotations
                .OfType<JsonSchemaFieldAttribute>()
                .FirstOrDefault(field => field.Name == fieldName);
        }

        private UnsupportedOperationException GetUnsupported(
            Type type,
            string fieldName)
        {
            return new UnsupportedOperationException(
                $"Unsupported type '{type.CleanName()}' for property '{fieldName}' (use JsonSchemaField to declare additional information)");
        }
    }


    internal class JsonSerializationForgePair
    {
        public JsonSerializerForge SerializerForge;
        public JsonDeserializerForge DeserializerForge;

        public JsonSerializationForgePair(
            JsonSerializerForge serializerForge,
            JsonDeserializerForge deserializerForge)
        {
            SerializerForge = serializerForge;
            DeserializerForge = deserializerForge;
        }
    }
} // end of namespace