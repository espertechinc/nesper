///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.json.forge;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
    public class JsonEventTypeUtilityReflective
    {
        public static IDictionary<Type, JsonApplicationClassSerializationDesc> ComputeClassesDeep(
            Type clazz,
            string eventTypeName,
            Attribute[] annotations,
            StatementCompileTimeServices services)
        {
            var deepClassesWFields = new LinkedHashMap<Type, IList<FieldInfo>>();
            ComputeClassesDeep(clazz, deepClassesWFields, new ArrayDeque<Type>(), annotations, services);
            return AssignDelegateClassNames(eventTypeName, deepClassesWFields);
        }

        public static IDictionary<Type, JsonApplicationClassSerializationDesc> ComputeClassesDeep(
            IDictionary<string, object> fields,
            string eventTypeName,
            Attribute[] annotations,
            StatementCompileTimeServices services)
        {
            var deepClassesWFields = new LinkedHashMap<Type, IList<FieldInfo>>();
            foreach (var entry in fields) {
                if (entry.Value is Type clazz) {
                    if (IsDeepClassEligibleType(clazz, entry.Key, null, annotations, services)) {
                        ComputeClassesDeep(clazz, deepClassesWFields, new ArrayDeque<Type>(), annotations, services);
                    }
                }
            }

            return AssignDelegateClassNames(eventTypeName, deepClassesWFields);
        }

        private static IDictionary<Type, JsonApplicationClassSerializationDesc> AssignDelegateClassNames(
            string eventTypeName,
            IDictionary<Type, IList<FieldInfo>> deepClassesWFields)
        {
            var classes = new LinkedHashMap<Type, JsonApplicationClassSerializationDesc>();
            foreach (var classEntry in deepClassesWFields) {
                var replaced = classEntry.Key.Name
                    .RegexReplaceAll("\\.", "_")
                    .RegexReplaceAll("\\$", "_");

				var serializerClassName = eventTypeName + "_Serializer_" + replaced;
				var deserializerClassName = eventTypeName + "_Deserializer_" + replaced;
                classes.Put(
                    classEntry.Key,
                    new JsonApplicationClassSerializationDesc(
						serializerClassName,
						deserializerClassName,
                        classEntry.Value));
            }

            return classes;
        }

        private static void ComputeClassesDeep(
            Type clazz,
            IDictionary<Type, IList<FieldInfo>> deepClasses,
            Deque<Type> stack,
            Attribute[] annotations,
            StatementCompileTimeServices services)
        {
            if (deepClasses.ContainsKey(clazz)) {
                return;
            }

            var fields = ResolveFields(clazz);

            // we go deep first
            foreach (var field in fields) {
                var fieldType = field.FieldType;
                if (fieldType.IsGenericCollection()) {
                    var genericType = TypeHelper.GetGenericFieldType(field, true);
                    if (genericType != null &&
                        !stack.Contains(genericType) &&
                        IsDeepClassEligibleType(genericType, field.Name, field, annotations, services) &&
                        genericType != typeof(object)) {
                        stack.Add(genericType);
                        ComputeClassesDeep(genericType, deepClasses, stack, annotations, services);
                        stack.RemoveLast();
                    }

                    continue;
                }

                if (fieldType.IsArray) {
                    var arrayType = TypeHelper.GetArrayComponentTypeInnermost(fieldType);
                    if (!stack.Contains(arrayType) &&
                        IsDeepClassEligibleType(arrayType, field.Name, field, annotations, services) &&
                        arrayType != typeof(object)) {
                        stack.Add(arrayType);
                        ComputeClassesDeep(arrayType, deepClasses, stack, annotations, services);
                        stack.RemoveLast();
                    }

                    continue;
                }

                if (!stack.Contains(fieldType) &&
                    IsDeepClassEligibleType(fieldType, field.Name, field, annotations, services)) {
                    stack.Add(fieldType);
                    ComputeClassesDeep(fieldType, deepClasses, stack, annotations, services);
                    stack.RemoveLast();
                }
            }

            deepClasses.Put(clazz, fields);
        }

        private static bool IsDeepClassEligibleType(
            Type genericType,
            string fieldName,
            FieldInfo optionalField,
            Attribute[] annotations,
            StatementCompileTimeServices services)
        {
            if (!genericType.HasDefaultConstructor()) {
                return false;
            }

            try {
                JsonForgeFactoryBuiltinClassTyped.INSTANCE.Forge(
                    genericType,
                    fieldName,
                    optionalField,
					EmptyDictionary<Type, JsonApplicationClassSerializationDesc>.Instance,
                    annotations,
                    services);
                return false;
            }
            catch (UnsupportedOperationException) {
                return true;
            }
        }

        private static IList<FieldInfo> ResolveFields(Type clazz)
        {
            var propertyListBuilder = new PropertyListBuilderPublic(new ConfigurationCommonEventTypeBean());
            var properties = propertyListBuilder.AssessProperties(clazz);
            var props = new List<FieldInfo>();
            foreach (var stem in properties) {
                var field = stem.AccessorField;
                if (field == null) {
                    continue;
                }

				if (field.IsPublic && !field.IsStatic) {
                    props.Add(field);
                }
            }

            return props;
        }
    }
} // end of namespace