///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.deserializers.forge;
using com.espertech.esper.common.@internal.@event.json.forge;
using com.espertech.esper.common.@internal.@event.json.serializers.forge;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.util.IdentifierUtil; // getIdentifierMayStartNumeric
using static com.espertech.esper.common.@internal.@event.core.BaseNestableEventUtil;

using TypeExtensions = com.espertech.esper.compat.TypeExtensions; // resolvePropertyTypes

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
    public class JsonEventTypeUtility
    {
        public static JsonEventType MakeJsonTypeCompileTimeExistingType(
            EventTypeMetadata metadata,
            JsonEventType existingType,
            StatementCompileTimeServices services)
        {
            var getterFactoryJson = new EventTypeNestableGetterFactoryJson(existingType.Detail);
            return new JsonEventType(
                metadata,
                existingType.Types,
                null,
                EmptySet<EventType>.Instance,
                existingType.StartTimestampPropertyName,
                existingType.EndTimestampPropertyName,
                getterFactoryJson,
                services.BeanEventTypeFactoryPrivate,
                existingType.Detail,
                existingType.UnderlyingType,
                existingType.UnderlyingTypeIsTransient);
        }

        public static EventTypeForgeablesPair MakeJsonTypeCompileTimeNewType(
            EventTypeMetadata metadata,
            IDictionary<string, object> compiledTyping,
            Pair<EventType[], ISet<EventType>> superTypes,
            ConfigurationCommonEventTypeWithSupertype config,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            if (metadata.ApplicationType != EventTypeApplicationType.JSON) {
                throw new IllegalStateException("Expected Json application type");
            }

            // determine supertype
            var optionalSuperType = (JsonEventType)
                (superTypes == null
                    ? null
                    : superTypes.First == null || superTypes.First.Length == 0
                        ? null
                        : superTypes.First[0]);
            var numFieldsSuperType = optionalSuperType?.Detail.FieldDescriptors.Count ?? 0;

            // determine dynamic
            var jsonSchema = (JsonSchemaAttribute)AnnotationUtil.FindAnnotation(
                raw.Annotations,
                typeof(JsonSchemaAttribute));
            var dynamic = DetermineDynamic(jsonSchema, optionalSuperType, raw);

            // determine json underlying type class
            var optionalUnderlyingProvided = DetermineUnderlyingProvided(jsonSchema, services);

            // determine properties
            IDictionary<string, object> properties;
            IDictionary<string, string> fieldNames;
            IDictionary<Type, JsonApplicationClassSerializationDesc> deepClasses;
            IDictionary<string, FieldInfo> fields;
            if (optionalUnderlyingProvided == null) {
                properties = ResolvePropertyTypes(compiledTyping, services.EventTypeCompileTimeResolver);
                properties = RemoveEventBeanTypes(properties);
                fieldNames = ComputeFieldNames(properties);
                deepClasses = JsonEventTypeUtilityReflective.ComputeClassesDeep(
                    properties,
                    metadata.Name,
                    raw.Annotations,
                    services);
                fields = EmptyDictionary<string, FieldInfo>.Instance;
            }
            else {
                if (dynamic) {
                    throw new ExprValidationException(
                        "The dynamic flag is not supported when used with a provided JSON event class");
                }

                if (optionalSuperType != null) {
                    throw new ExprValidationException(
                        "Specifying a supertype is not supported with a provided JSON event class");
                }

                if (!optionalUnderlyingProvided.IsPublic && !optionalUnderlyingProvided.IsNestedPublic) {
                    throw new ExprValidationException("Provided JSON event class is not public");
                }

                if (!optionalUnderlyingProvided.HasDefaultConstructor()) {
                    throw new ExprValidationException(
                        "Provided JSON event class does not have a public default constructor or is a non-static inner class");
                }

                deepClasses = JsonEventTypeUtilityReflective.ComputeClassesDeep(
                    optionalUnderlyingProvided,
                    metadata.Name,
                    raw.Annotations,
                    services);
                fields = new LinkedHashMap<string, FieldInfo>();
                deepClasses.Get(optionalUnderlyingProvided).Fields.ForEach(field => fields.Put(field.Name, field));
                properties = ResolvePropertiesFromFields(fields);
                fieldNames = ComputeFieldNamesFromProperties(properties);
                compiledTyping = ResolvePropertyTypes(compiledTyping, services.EventTypeCompileTimeResolver);
                ValidateFieldTypes(optionalUnderlyingProvided, fields, compiledTyping);

                // use the rich-type definition for properties that may come from events
                foreach (var compiledTypingEntry in compiledTyping) {
                    if (compiledTypingEntry.Value is TypeBeanOrUnderlying ||
                        compiledTypingEntry.Value is TypeBeanOrUnderlying[]) {
                        properties.Put(compiledTypingEntry.Key, compiledTypingEntry.Value);
                    }
                }
            }

            var fieldDescriptors = ComputeFields(properties, fieldNames, optionalSuperType, fields);
            // Computes a forge for each property presented.
            var forgesByProperty = ComputeValueForges(properties, fields, deepClasses, raw.Annotations, services);
            // Determines a name for the internal class representation for this json event.
            var jsonClassNameSimple = DetermineJsonClassName(metadata, raw, optionalUnderlyingProvided);

            var forgeableDesc = new StmtClassForgeableJsonDesc(
                properties,
                fieldDescriptors,
                dynamic,
                numFieldsSuperType,
                optionalSuperType,
                forgesByProperty);

            var underlyingClassNameSimple = jsonClassNameSimple;
            var underlyingClassNameForReference = optionalUnderlyingProvided != null
                ? optionalUnderlyingProvided.Name
                : underlyingClassNameSimple;
            var underlyingClassNameFull = optionalUnderlyingProvided == null
                ? $"{services.Namespace}.{underlyingClassNameSimple}"
                : optionalUnderlyingProvided.FullName;

            var underlying = new ProxyStmtClassForgeableFactory() {
                ProcMake = (
                    namespaceScope,
                    classPostfix) => new StmtClassForgeableJsonUnderlying(
                    underlyingClassNameSimple,
                    underlyingClassNameFull,
                    namespaceScope,
                    forgeableDesc)
            };

            var delegateClassNameSimple = jsonClassNameSimple + "__Delegate";
            var @delegate = new ProxyStmtClassForgeableFactory() {
                ProcMake = (
                    namespaceScope,
                    classPostfix) => new StmtClassForgeableJsonDelegate(
                    CodegenClassType.JSONDELEGATE,
                    delegateClassNameSimple,
                    namespaceScope,
                    underlyingClassNameFull,
                    forgeableDesc)
            };

            var deserializerClassNameSimple = jsonClassNameSimple + "__Deserializer";
            var deserializer = new ProxyStmtClassForgeableFactory() {
                ProcMake = (
                    namespaceScope,
                    classPostfix) => new StmtClassForgeableJsonDeserializer(
                    CodegenClassType.JSONDESERIALIZER,
                    deserializerClassNameSimple,
                    namespaceScope,
                    underlyingClassNameFull,
                    forgeableDesc)
            };

            var serializerClassNameSimple = jsonClassNameSimple + "__Serializer";
            var serializer = new ProxyStmtClassForgeableFactory() {
                ProcMake = (
                    namespaceScope,
                    classPostfix) => new StmtClassForgeableJsonSerializer(
                    CodegenClassType.JSONSERIALIZER,
                    serializerClassNameSimple,
                    optionalUnderlyingProvided != null,
                    namespaceScope,
                    underlyingClassNameFull,
                    forgeableDesc)
            };

            var serializerClassNameFull = $"{services.Namespace}.{serializerClassNameSimple}";
            var deserializerClassNameFull = $"{services.Namespace}.{deserializerClassNameSimple}";
            var delegateClassNameFull = $"{services.Namespace}.{delegateClassNameSimple}";

            // include event type name as underlying-class may occur multiple times
            var serdeClassNameFull = $"{services.Namespace}.{jsonClassNameSimple}__{metadata.Name}__Serde";

            var detail = new JsonEventTypeDetail(
                underlyingClassNameFull,
                optionalUnderlyingProvided,
                delegateClassNameFull,
                deserializerClassNameFull,
                serializerClassNameFull,
                serdeClassNameFull,
                fieldDescriptors,
                dynamic,
                numFieldsSuperType);
            var getterFactoryJson = new EventTypeNestableGetterFactoryJson(detail);

            var isStandIn = optionalUnderlyingProvided == null;
            var standIn = isStandIn
                ? services.CompilerServices.CompileStandInClass(
                    CodegenClassType.JSONEVENT,
                    underlyingClassNameSimple,
                    services.Services)
                : optionalUnderlyingProvided;

            var eventType = new JsonEventType(
                metadata,
                properties,
                superTypes == null ? Array.Empty<EventType>() : superTypes.First,
                superTypes == null ? EmptySet<EventType>.Instance : superTypes.Second,
                config?.StartTimestampPropertyName,
                config?.EndTimestampPropertyName,
                getterFactoryJson,
                services.BeanEventTypeFactoryPrivate,
                detail,
                standIn,
                isStandIn);

            var additionalForgeables = new List<StmtClassForgeableFactory>();

            // generate serializer, deserializer, and delegate forgeables for application classes
            GenerateApplicationClassForgables(
                optionalUnderlyingProvided,
                deepClasses,
                additionalForgeables,
                raw.Annotations,
                services);

            if (optionalUnderlyingProvided == null) {
                additionalForgeables.Add(underlying);
            }

            additionalForgeables.Add(@delegate);
            additionalForgeables.Add(deserializer);
            additionalForgeables.Add(serializer);

            return new EventTypeForgeablesPair(eventType, additionalForgeables);
        }

        /// <summary>
        /// Determines a classname for the json event representation.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="raw"></param>
        /// <param name="optionalUnderlyingProvided"></param>
        /// <returns></returns>
        private static string DetermineJsonClassName(
            EventTypeMetadata metadata,
            StatementRawInfo raw,
            Type optionalUnderlyingProvided)
        {
            if (optionalUnderlyingProvided != null) {
                return optionalUnderlyingProvided.Name;
            }

            var jsonClassNameSimple = metadata.Name;
            if (metadata.AccessModifier.IsPrivateOrTransient()) {
                var uuid = CodeGenerationIDGenerator.GenerateClassNameUUID();
                jsonClassNameSimple = $"{jsonClassNameSimple}__{uuid}";
            }
            else if (raw.ModuleName != null) {
                jsonClassNameSimple = $"{jsonClassNameSimple}__module_{raw.ModuleName}";
            }

            return jsonClassNameSimple;
        }

        private static void ValidateFieldTypes(
            Type declaredClass,
            IDictionary<string, FieldInfo> targetFields,
            IDictionary<string, object> insertedFields)
        {
            foreach (var inserted in insertedFields) {
                var insertedName = inserted.Key;
                var insertedType = inserted.Value;
                var field = targetFields.Get(insertedName);

                if (field == null) {
                    throw new ExprValidationException(
                        "Failed to find public field '" + insertedName + "' on class '" + declaredClass.Name + "'");
                }

                var fieldClass = field.FieldType.GetBoxedType();
                if (insertedType is Type) {
                    var insertedClass = ((Type)insertedType).GetBoxedType();
                    if (!TypeHelper.IsSubclassOrImplementsInterface(insertedClass, fieldClass)) {
                        throw MakeInvalidField(insertedName, insertedClass, declaredClass, field);
                    }
                }
                else if (insertedType is TypeBeanOrUnderlying || insertedType is EventType) {
                    var eventType = insertedType is TypeBeanOrUnderlying
                        ? ((TypeBeanOrUnderlying)insertedType).EventType
                        : (EventType)insertedType;
                    if (!TypeHelper.IsSubclassOrImplementsInterface(eventType.UnderlyingType, fieldClass)) {
                        throw MakeInvalidField(insertedName, eventType.UnderlyingType, declaredClass, field);
                    }
                }
                else if (insertedType is TypeBeanOrUnderlying[] || insertedType is EventType[]) {
                    var eventType = insertedType is TypeBeanOrUnderlying[]
                        ? ((TypeBeanOrUnderlying[])insertedType)[0].EventType
                        : ((EventType[])insertedType)[0];
                    if (!fieldClass.IsArray ||
                        !TypeHelper.IsSubclassOrImplementsInterface(
                            eventType.UnderlyingType,
                            fieldClass.GetElementType())) {
                        throw MakeInvalidField(insertedName, eventType.UnderlyingType, declaredClass, field);
                    }
                }
                else {
                    throw new IllegalStateException("Unrecognized type '" + insertedType + "'");
                }
            }
        }

        private static ExprValidationException MakeInvalidField(
            string insertedName,
            Type insertedClass,
            Type declaredClass,
            FieldInfo field)
        {
            return new ExprValidationException(
                "Public field '" +
                insertedName +
                "' of class '" +
                declaredClass.CleanName() +
                "' declared as type " +
                "'" +
                field.FieldType.CleanName() +
                "' cannot receive a value of type '" +
                insertedClass.CleanName() +
                "'");
        }

        private static void GenerateApplicationClassForgables(
            Type optionalUnderlyingProvided,
            IDictionary<Type, JsonApplicationClassSerializationDesc> deepClasses,
            IList<StmtClassForgeableFactory> additionalForgeables,
            Attribute[] annotations,
            StatementCompileTimeServices services)
        {
            foreach (var entry in deepClasses) {
                if (entry.Key == optionalUnderlyingProvided) {
                    continue;
                }

                var fields = new LinkedHashMap<string, FieldInfo>();
                entry.Value.Fields.ForEach(field => fields.Put(field.Name, field));

                var properties = ResolvePropertiesFromFields(fields);
                var fieldNames = ComputeFieldNamesFromProperties(properties);
                var forges = ComputeValueForges(properties, fields, deepClasses, annotations, services);
                var fieldDescriptors = ComputeFields(properties, fieldNames, null, fields);
                var forgeableDesc = new StmtClassForgeableJsonDesc(
                    properties,
                    fieldDescriptors,
                    false,
                    0,
                    null,
                    forges);

                var deserializerClassNameSimple = entry.Value.DeserializerClassName;
                var deserializer = new ProxyStmtClassForgeableFactory() {
                    ProcMake = (
                        namespaceScope,
                        classPostfix) => new StmtClassForgeableJsonDeserializer(
                        CodegenClassType.JSONDESERIALIZER,
                        deserializerClassNameSimple,
                        namespaceScope,
                        entry.Key.FullName,
                        forgeableDesc)
                };

                var serializerClassNameSimple = entry.Value.SerializerClassName;
                var serializer = new ProxyStmtClassForgeableFactory() {
                    ProcMake = (
                        namespaceScope,
                        classPostfix) => new StmtClassForgeableJsonSerializer(
                        CodegenClassType.JSONSERIALIZER,
                        serializerClassNameSimple,
                        true,
                        namespaceScope,
                        entry.Key.FullName,
                        forgeableDesc)
                };

                additionalForgeables.Add(deserializer);
                additionalForgeables.Add(serializer);
            }
        }

        private static IDictionary<string, string> ComputeFieldNamesFromProperties(
            IDictionary<string, object> properties)
        {
            var fieldNames = new LinkedHashMap<string, string>();
            foreach (var key in properties.Keys) {
                fieldNames.Put(key, key);
            }

            return fieldNames;
        }

        private static Type DetermineUnderlyingProvided(
            JsonSchemaAttribute jsonSchema,
            StatementCompileTimeServices services)
        {
            if (jsonSchema != null && !string.IsNullOrWhiteSpace(jsonSchema.ClassName)) {
                try {
                    return services.ImportServiceCompileTime.ResolveType(
                        jsonSchema.ClassName,
                        true,
                        ExtensionClassEmpty.INSTANCE);
                }
                catch (ImportException e) {
                    throw new ExprValidationException(
                        "Failed to resolve JSON event class '" + jsonSchema.ClassName + "': " + e.Message,
                        e);
                }
            }

            return null;
        }

        private static bool DetermineDynamic(
            JsonSchemaAttribute jsonSchema,
            JsonEventType optionalSuperType,
            StatementRawInfo raw)
        {
            if (optionalSuperType != null && optionalSuperType.Detail.IsDynamic) {
                return true;
            }

            return jsonSchema != null && jsonSchema.Dynamic;
        }

        private static IDictionary<string, object> RemoveEventBeanTypes(IDictionary<string, object> properties)
        {
            var verified = new LinkedHashMap<string, object>();
            foreach (var prop in properties) {
                var propertyName = prop.Key;
                var propertyType = prop.Value;
                verified.Put(propertyName, propertyType);

                if (propertyType is EventType eventType) {
                    verified.Put(propertyName, new TypeBeanOrUnderlying(eventType));
                }
                else if (propertyType is EventType[] eventTypeArray) {
                    verified.Put(
                        propertyName,
                        new[] {
                            new TypeBeanOrUnderlying(eventTypeArray[0])
                        });
                }
            }

            return verified;
        }

        /// <summary>
        /// TODO: Understand what this method does...
        /// </summary>
        /// <param name="compiledTyping"></param>
        /// <param name="fields"></param>
        /// <param name="deepClasses"></param>
        /// <param name="annotations"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <exception cref="IllegalStateException"></exception>
        private static IDictionary<string, JsonForgeDesc> ComputeValueForges(
            IDictionary<string, object> compiledTyping,
            IDictionary<string, FieldInfo> fields,
            IDictionary<Type, JsonApplicationClassSerializationDesc> deepClasses,
            Attribute[] annotations,
            StatementCompileTimeServices services)
        {
            var valueForges = new Dictionary<string, JsonForgeDesc>();
            foreach (var entry in compiledTyping) {
                var type = entry.Value;
                var optionalField = fields.Get(entry.Key);

                JsonForgeDesc forgeDesc;

                if (type == null) {
                    forgeDesc = new JsonForgeDesc(
                        JsonDeserializerForgeNull.INSTANCE,
                        JsonSerializerForgeNull.INSTANCE);
                }
                else if (type is Type clazz) {
                    forgeDesc = JsonForgeFactoryBuiltinClassTyped.INSTANCE.Forge(
                        clazz,
                        entry.Key,
                        optionalField,
                        deepClasses,
                        annotations,
                        services);
                }
                else if (type is TypeBeanOrUnderlying typeBeanOrUnderlying) {
                    var eventType = typeBeanOrUnderlying.EventType;
                    ValidateJsonOrMapType(eventType);
                    if (eventType is JsonEventType jsonEventType) {
                        forgeDesc = JsonForgeFactoryEventTypeTyped.ForgeNonArray(
                            entry.Key,
                            jsonEventType);
                    }
                    else {
                        forgeDesc = JsonForgeFactoryBuiltinClassTyped.INSTANCE.Forge(
                            typeof(IDictionary<string, object>),
                            entry.Key,
                            optionalField,
                            deepClasses,
                            annotations,
                            services);
                    }
                }
                else if (type is TypeBeanOrUnderlying[] typeBeanOrUnderlyingArray) {
                    var eventType = typeBeanOrUnderlyingArray[0].EventType;
                    ValidateJsonOrMapType(eventType);
                    if (eventType is JsonEventType jsonEventType) {
                        forgeDesc = JsonForgeFactoryEventTypeTyped.ForgeArray(
                            entry.Key,
                            jsonEventType);
                    }
                    else {
                        forgeDesc = JsonForgeFactoryBuiltinClassTyped.INSTANCE.Forge(
                            typeof(IDictionary<string, object>[]),
                            entry.Key,
                            optionalField,
                            deepClasses,
                            annotations,
                            services);
                    }
                }
                else {
                    throw new IllegalStateException($"Unrecognized type {type}");
                }

                valueForges.Put(entry.Key, forgeDesc);
            }

            return valueForges;
        }

        private static void ValidateJsonOrMapType(EventType eventType)
        {
            if (!(eventType is JsonEventType) && !(eventType is MapEventType)) {
                throw new ExprValidationException(
                    "Failed to validate event type '" +
                    eventType.Metadata.Name +
                    "', expected a Json or Map event type");
            }
        }

        private static IDictionary<string, JsonUnderlyingField> ComputeFields(
            IDictionary<string, object> compiledTyping,
            IDictionary<string, string> fieldNames,
            JsonEventType optionalSuperType,
            IDictionary<string, FieldInfo> fields)
        {
            IDictionary<string, JsonUnderlyingField> allFieldsInclSupertype =
                new LinkedHashMap<string, JsonUnderlyingField>();

            var index = 0;
            if (optionalSuperType != null) {
                allFieldsInclSupertype.PutAll(optionalSuperType.Detail.FieldDescriptors);
                index = allFieldsInclSupertype.Count;
            }

            foreach (var entry in compiledTyping) {
                var fieldName = fieldNames.Get(entry.Key);

                var type = entry.Value;
                Type assignedType;
                if (type == null) {
                    assignedType = typeof(object);
                }
                else if (type is Type) {
                    assignedType = (Type)type;
                }
                else if (type is TypeBeanOrUnderlying) {
                    var other = ((TypeBeanOrUnderlying)type).EventType;
                    ValidateJsonOrMapType(other);
                    assignedType = GetAssignedType(other);
                }
                else if (type is TypeBeanOrUnderlying[]) {
                    var other = ((TypeBeanOrUnderlying[])type)[0].EventType;
                    ValidateJsonOrMapType(other);
                    assignedType = TypeHelper.GetArrayType(GetAssignedType(other));
                }
                else {
                    throw new IllegalStateException("Unrecognized type " + type);
                }

                allFieldsInclSupertype.Put(
                    entry.Key,
                    new JsonUnderlyingField(
                        fieldName,
                        index,
                        assignedType,
                        fields.Get(fieldName)));
                index++;
            }

            return allFieldsInclSupertype;
        }

        private static Type GetAssignedType(EventType type)
        {
            if (type is JsonEventType) {
                return type.UnderlyingType;
            }

            if (type is MapEventType) {
                return typeof(IDictionary<string, object>);
            }

            throw new ExprValidationException(
                "Incompatible type '" + type.Name + "' encountered, expected a Json or Map event type");
        }

        private static IDictionary<string, string> ComputeFieldNames(IDictionary<string, object> compiledTyping)
        {
            IDictionary<string, string> fields = new Dictionary<string, string>();
            ISet<string> assignedNames = new HashSet<string>();
            foreach (var name in compiledTyping.Keys) {
                var assigned = "_" + GetIdentifierMayStartNumeric(name.ToLowerInvariant());
                if (!assignedNames.Add(assigned)) {
                    var suffix = 0;
                    while (true) {
                        var withSuffix = assigned + "_" + suffix;
                        if (!assignedNames.Contains(withSuffix)) {
                            assigned = withSuffix;
                            assignedNames.Add(assigned);
                            break;
                        }

                        suffix++;
                    }
                }

                fields.Put(name, assigned);
            }

            return fields;
        }

        public static void AddJsonUnderlyingClass(
            IDictionary<string, EventType> moduleTypes,
            ParentTypeResolver typeResolver,
            string optionalDeploymentId)
        {
            foreach (var eventType in moduleTypes) {
                AddJsonUnderlyingClass(eventType.Value, typeResolver, optionalDeploymentId);
            }
        }

        public static void AddJsonUnderlyingClass(
            PathRegistry<string, EventType> pathEventTypes,
            ParentTypeResolver typeResolver)
        {
            pathEventTypes.Traverse(type => AddJsonUnderlyingClass(type, typeResolver, null));
        }

        public static void AddJsonUnderlyingClass(
            EventType eventType,
            ParentTypeResolver typeResolver,
            string optionalDeploymentId)
        {
            if (!(eventType is JsonEventType jsonEventType)) {
                return;
            }

            // for named-window the same underlying is used and we ignore duplicate add
            var allowDuplicate = eventType.Metadata.TypeClass == EventTypeTypeClass.NAMED_WINDOW;
            if (jsonEventType.Detail.OptionalUnderlyingProvided == null) {
                typeResolver.Add(
                    jsonEventType.Detail.UnderlyingClassName,
                    jsonEventType.UnderlyingType,
                    optionalDeploymentId,
                    allowDuplicate);
            }
            else {
                allowDuplicate = true;
            }

            typeResolver.Add(
                jsonEventType.Detail.DeserializerClassName,
                jsonEventType.DeserializerType,
                optionalDeploymentId,
                allowDuplicate);

            typeResolver.Add(
                jsonEventType.Detail.SerializerClassName,
                jsonEventType.SerializerType,
                optionalDeploymentId,
                allowDuplicate);
        }

        private static IDictionary<string, object> ResolvePropertiesFromFields(IDictionary<string, FieldInfo> fields)
        {
            var properties = new LinkedHashMap<string, object>();
            foreach (var field in fields) {
                properties.Put(field.Key, field.Value.FieldType);
            }

            return properties;
        }
    }
} // end of namespace