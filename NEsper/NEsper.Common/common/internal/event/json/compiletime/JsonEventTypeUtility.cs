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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue;
using com.espertech.esper.common.@internal.@event.json.parser.forge;
using com.espertech.esper.common.@internal.@event.json.write;
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
				existingType.UnderlyingType);
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
			var optionalSuperType =
				(JsonEventType) (superTypes == null ? null : (superTypes.First == null || superTypes.First.Length == 0 ? null : superTypes.First[0]));
			var numFieldsSuperType = optionalSuperType == null ? 0 : optionalSuperType.Detail.FieldDescriptors.Count;

			// determine dynamic
			var jsonSchema = (JsonSchemaAttribute) AnnotationUtil.FindAnnotation(raw.Annotations, typeof(JsonSchemaAttribute));
			var dynamic = DetermineDynamic(jsonSchema, optionalSuperType, raw);

			// determine json underlying type class
			var optionalUnderlyingProvided = DetermineUnderlyingProvided(jsonSchema, services);

			// determine properties
			IDictionary<string, object> properties;
			IDictionary<string, string> fieldNames;
			IDictionary<Type, JsonApplicationClassDelegateDesc> deepClasses;
			IDictionary<string, FieldInfo> fields;
			if (optionalUnderlyingProvided == null) {
				properties = ResolvePropertyTypes(compiledTyping, services.EventTypeCompileTimeResolver);
				properties = RemoveEventBeanTypes(properties);
				fieldNames = ComputeFieldNames(properties);
				deepClasses = JsonEventTypeUtilityReflective.ComputeClassesDeep(properties, metadata.Name, raw.Annotations, services);
				fields = EmptyDictionary<string, FieldInfo>.Instance;
			}
			else {
				if (dynamic) {
					throw new ExprValidationException("The dynamic flag is not supported when used with a provided JSON event class");
				}

				if (optionalSuperType != null) {
					throw new ExprValidationException("Specifying a supertype is not supported with a provided JSON event class");
				}

				if (!optionalUnderlyingProvided.IsPublic) {
					throw new ExprValidationException("Provided JSON event class is not public");
				}

				if (!optionalUnderlyingProvided.HasDefaultConstructor()) {
					throw new ExprValidationException("Provided JSON event class does not have a public default constructor or is a non-static inner class");
				}

				deepClasses = JsonEventTypeUtilityReflective.ComputeClassesDeep(optionalUnderlyingProvided, metadata.Name, raw.Annotations, services);
				fields = new LinkedHashMap<string,FieldInfo>();
				deepClasses.Get(optionalUnderlyingProvided).Fields.ForEach(field => fields.Put(field.Name, field));
				properties = ResolvePropertiesFromFields(fields);
				fieldNames = ComputeFieldNamesFromProperties(properties);
				compiledTyping = ResolvePropertyTypes(compiledTyping, services.EventTypeCompileTimeResolver);
				ValidateFieldTypes(optionalUnderlyingProvided, fields, compiledTyping);

				// use the rich-type definition for properties that may come from events
				foreach (var compiledTypingEntry in compiledTyping) {
					if (compiledTypingEntry.Value is TypeBeanOrUnderlying || compiledTypingEntry.Value is TypeBeanOrUnderlying[]) {
						properties.Put(compiledTypingEntry.Key, compiledTypingEntry.Value);
					}
				}
			}

			var fieldDescriptors = ComputeFields(properties, fieldNames, optionalSuperType, fields);
			var forges = ComputeValueForges(properties, fields, deepClasses, raw.Annotations, services);

			string jsonClassNameSimple;
			if (optionalUnderlyingProvided != null) {
				jsonClassNameSimple = optionalUnderlyingProvided.Name;
			}
			else {
				jsonClassNameSimple = metadata.Name;
				if (metadata.AccessModifier.IsPrivateOrTransient()) {
					var uuid = CodeGenerationIDGenerator.GenerateClassNameUUID();
					jsonClassNameSimple = jsonClassNameSimple + "__" + uuid;
				}
				else if (raw.ModuleName != null) {
					jsonClassNameSimple = jsonClassNameSimple + "__" + "module" + "_" + raw.ModuleName;
				}
			}

			var forgeableDesc = new StmtClassForgeableJsonDesc(properties, fieldDescriptors, dynamic, numFieldsSuperType, optionalSuperType, forges);

			var underlyingClassNameSimple = jsonClassNameSimple;
			var underlyingClassNameForReference = optionalUnderlyingProvided != null ? optionalUnderlyingProvided.Name : underlyingClassNameSimple;
			StmtClassForgeableFactory underlying = new ProxyStmtClassForgeableFactory() {
				ProcMake = (
					namespaceScope,
					classPostfix) => {
					return new StmtClassForgeableJsonUnderlying(underlyingClassNameSimple, namespaceScope, forgeableDesc);
				},
			};

			var delegateClassNameSimple = jsonClassNameSimple + "__Delegate";
			StmtClassForgeableFactory @delegate = new ProxyStmtClassForgeableFactory() {
				ProcMake = (
					namespaceScope,
					classPostfix) => {
					return new StmtClassForgeableJsonDelegate(
						CodegenClassType.JSONDELEGATE,
						delegateClassNameSimple,
						namespaceScope,
						underlyingClassNameForReference,
						forgeableDesc);
				},
			};

			var delegateFactoryClassNameSimple = jsonClassNameSimple + "__Factory";
			StmtClassForgeableFactory delegateFactory = new ProxyStmtClassForgeableFactory() {
				ProcMake = (
					namespaceScope,
					classPostfix) => {
					return new StmtClassForgeableJsonDelegateFactory(
						CodegenClassType.JSONDELEGATEFACTORY,
						delegateFactoryClassNameSimple,
						optionalUnderlyingProvided != null,
						namespaceScope,
						delegateClassNameSimple,
						underlyingClassNameForReference,
						forgeableDesc);
				},
			};

			var underlyingClassNameFull = optionalUnderlyingProvided == null
				? services.Namespace + "." + underlyingClassNameSimple
				: optionalUnderlyingProvided.Name;
			var delegateClassNameFull = services.Namespace + "." + delegateClassNameSimple;
			var delegateFactoryClassNameFull = services.Namespace + "." + delegateFactoryClassNameSimple;
			var serdeClassNameFull =
				services.Namespace +
				"." +
				jsonClassNameSimple +
				"__" +
				metadata.Name +
				"__Serde"; // include event type name as underlying-class may occur multiple times

			var detail = new JsonEventTypeDetail(
				underlyingClassNameFull,
				optionalUnderlyingProvided,
				delegateClassNameFull,
				delegateFactoryClassNameFull,
				serdeClassNameFull,
				fieldDescriptors,
				dynamic,
				numFieldsSuperType);
			var getterFactoryJson = new EventTypeNestableGetterFactoryJson(detail);

			var standIn = optionalUnderlyingProvided == null
				? services.CompilerServices.CompileStandInClass(CodegenClassType.JSONEVENT, underlyingClassNameSimple, services.Services)
				: optionalUnderlyingProvided;

			var eventType = new JsonEventType(
				metadata,
				properties,
				superTypes == null ? new EventType[0] : superTypes.First,
				superTypes == null ? EmptySet<EventType>.Instance : superTypes.Second,
				config == null ? null : config.StartTimestampPropertyName,
				config == null ? null : config.EndTimestampPropertyName,
				getterFactoryJson,
				services.BeanEventTypeFactoryPrivate,
				detail,
				standIn);

			IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>(3);

			// generate delegate and factory forgables for application classes
			GenerateApplicationClassForgables(optionalUnderlyingProvided, deepClasses, additionalForgeables, raw.Annotations, services);

			if (optionalUnderlyingProvided == null) {
				additionalForgeables.Add(underlying);
			}

			additionalForgeables.Add(@delegate);
			additionalForgeables.Add(delegateFactory);

			return new EventTypeForgeablesPair(eventType, additionalForgeables);
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
					throw new ExprValidationException("Failed to find public field '" + insertedName + "' on class '" + declaredClass.Name + "'");
				}

				var fieldClass = Boxing.GetBoxedType(field.FieldType);
				if (insertedType is Type) {
					var insertedClass = ((Type) insertedType).GetBoxedType();
					if (!TypeHelper.IsSubclassOrImplementsInterface(insertedClass, fieldClass)) {
						throw MakeInvalidField(insertedName, insertedClass, declaredClass, field);
					}
				}
				else if (insertedType is TypeBeanOrUnderlying || insertedType is EventType) {
					var eventType = (insertedType is TypeBeanOrUnderlying) ? ((TypeBeanOrUnderlying) insertedType).EventType : (EventType) insertedType;
					if (!TypeHelper.IsSubclassOrImplementsInterface(eventType.UnderlyingType, fieldClass)) {
						throw MakeInvalidField(insertedName, eventType.UnderlyingType, declaredClass, field);
					}
				}
				else if (insertedType is TypeBeanOrUnderlying[] || insertedType is EventType[]) {
					var eventType = (insertedType is TypeBeanOrUnderlying[])
						? ((TypeBeanOrUnderlying[]) insertedType)[0].EventType
						: ((EventType[]) insertedType)[0];
					if (!fieldClass.IsArray || !TypeHelper.IsSubclassOrImplementsInterface(eventType.UnderlyingType, fieldClass.GetElementType())) {
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
				TypeExtensions.CleanName(field.FieldType) +
				"' cannot receive a value of type '" +
				insertedClass.CleanName() +
				"'");
		}

		private static void GenerateApplicationClassForgables(
			Type optionalUnderlyingProvided,
			IDictionary<Type, JsonApplicationClassDelegateDesc> deepClasses,
			IList<StmtClassForgeableFactory> additionalForgeables,
			Attribute[] annotations,
			StatementCompileTimeServices services)
		{
			foreach (var entry in deepClasses) {
				if (entry.Key == optionalUnderlyingProvided) {
					continue;
				}

				LinkedHashMap<string, FieldInfo> fields = new LinkedHashMap<string, FieldInfo>();
				entry.Value.Fields.ForEach(field => fields.Put(field.Name, field));

				var properties = ResolvePropertiesFromFields(fields);
				var fieldNames = ComputeFieldNamesFromProperties(properties);
				var forges = ComputeValueForges(properties, fields, deepClasses, annotations, services);
				var fieldDescriptors = ComputeFields(properties, fieldNames, null, fields);

				var delegateClassNameSimple = entry.Value.DelegateClassName;
				var forgeableDesc = new StmtClassForgeableJsonDesc(properties, fieldDescriptors, false, 0, null, forges);
				StmtClassForgeableFactory @delegate = new ProxyStmtClassForgeableFactory() {
					ProcMake = (
						namespaceScope,
						classPostfix) => new StmtClassForgeableJsonDelegate(
						CodegenClassType.JSONNESTEDCLASSDELEGATEANDFACTORY,
						delegateClassNameSimple,
						namespaceScope,
						entry.Key.Name,
						forgeableDesc),
				};

				var delegateFactoryClassNameSimple = entry.Value.DelegateFactoryClassName;
				StmtClassForgeableFactory delegateFactory = new ProxyStmtClassForgeableFactory() {
					ProcMake = (
						namespaceScope,
						classPostfix) => new StmtClassForgeableJsonDelegateFactory(
						CodegenClassType.JSONNESTEDCLASSDELEGATEANDFACTORY,
						delegateFactoryClassNameSimple,
						true,
						namespaceScope,
						delegateClassNameSimple,
						entry.Key.Name,
						forgeableDesc),
				};

				additionalForgeables.Add(@delegate);
				additionalForgeables.Add(delegateFactory);
			}
		}

		private static IDictionary<string, string> ComputeFieldNamesFromProperties(IDictionary<string, object> properties)
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
					return services.ImportServiceCompileTime.ResolveClass(jsonSchema.ClassName, true, ExtensionClassEmpty.INSTANCE);
				}
				catch (ImportException e) {
					throw new ExprValidationException("Failed to resolve JSON event class '" + jsonSchema.ClassName + "': " + e.Message, e);
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

				if (propertyType is EventType) {
					var eventType = (EventType) propertyType;
					verified.Put(propertyName, new TypeBeanOrUnderlying(eventType));
				}
				else if (propertyType is EventType[]) {
					var eventType = ((EventType[]) propertyType)[0];
					verified.Put(propertyName, new TypeBeanOrUnderlying[] {new TypeBeanOrUnderlying(eventType)});
				}
			}

			return verified;
		}

		private static IDictionary<string, JsonForgeDesc> ComputeValueForges(
			IDictionary<string, object> compiledTyping,
			IDictionary<string, FieldInfo> fields,
			IDictionary<Type, JsonApplicationClassDelegateDesc> deepClasses,
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
						entry.Key,
						null,
						null,
						JsonEndValueForgeNull.INSTANCE,
						JsonWriteForgeNull.INSTANCE);
				}
				else if (type is Type clazz) {
					forgeDesc = JsonForgeFactoryBuiltinClassTyped.Forge(
						clazz,
						entry.Key,
						optionalField,
						deepClasses,
						annotations,
						services);
				}
				else if (type is TypeBeanOrUnderlying) {
					var eventType = ((TypeBeanOrUnderlying) type).EventType;
					ValidateJsonOrMapType(eventType);
					if (eventType is JsonEventType jsonEventType) {
						forgeDesc = JsonForgeFactoryEventTypeTyped.ForgeNonArray(
							entry.Key,
							jsonEventType);
					}
					else {
						forgeDesc = JsonForgeFactoryBuiltinClassTyped.Forge(
							typeof(IDictionary<string, object>),
							entry.Key,
							optionalField,
							deepClasses,
							annotations,
							services);
					}
				}
				else if (type is TypeBeanOrUnderlying[]) {
					var eventType = ((TypeBeanOrUnderlying[]) type)[0].EventType;
					ValidateJsonOrMapType(eventType);
					if (eventType is JsonEventType jsonEventType) {
						forgeDesc = JsonForgeFactoryEventTypeTyped.ForgeArray(
							entry.Key,
							jsonEventType);
					}
					else {
						forgeDesc = JsonForgeFactoryBuiltinClassTyped.Forge(
							typeof(IDictionary<string, object>[]),
							entry.Key,
							optionalField,
							deepClasses,
							annotations,
							services);
					}
				}
				else {
					throw new IllegalStateException("Unrecognized type " + type);
				}

				valueForges.Put(entry.Key, forgeDesc);
			}

			return valueForges;
		}

		private static void ValidateJsonOrMapType(EventType eventType)
		{
			if (!(eventType is JsonEventType) && !(eventType is MapEventType)) {
				throw new ExprValidationException("Failed to validate event type '" + eventType.Metadata.Name + "', expected a Json or Map event type");
			}
		}

		private static IDictionary<string, JsonUnderlyingField> ComputeFields(
			IDictionary<string, object> compiledTyping,
			IDictionary<string, string> fieldNames,
			JsonEventType optionalSuperType,
			IDictionary<string, FieldInfo> fields)
		{
			IDictionary<string, JsonUnderlyingField> allFieldsInclSupertype = new LinkedHashMap<string, JsonUnderlyingField>();

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
					assignedType = (Type) type;
				}
				else if (type is TypeBeanOrUnderlying) {
					var other = ((TypeBeanOrUnderlying) type).EventType;
					ValidateJsonOrMapType(other);
					assignedType = GetAssignedType(other);
				}
				else if (type is TypeBeanOrUnderlying[]) {
					var other = ((TypeBeanOrUnderlying[]) type)[0].EventType;
					ValidateJsonOrMapType(other);
					assignedType = TypeHelper.GetArrayType(GetAssignedType(other));
				}
				else {
					throw new IllegalStateException("Unrecognized type " + type);
				}

				allFieldsInclSupertype.Put(entry.Key, new JsonUnderlyingField(fieldName, index, assignedType, fields.Get(fieldName)));
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

			throw new ExprValidationException("Incompatible type '" + type.Name + "' encountered, expected a Json or Map event type");
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
			ParentClassLoader classLoader,
			string optionalDeploymentId)
		{
			foreach (var eventType in moduleTypes) {
				AddJsonUnderlyingClassInternal(eventType.Value, classLoader, optionalDeploymentId);
			}
		}

		public static void AddJsonUnderlyingClass(
			PathRegistry<string, EventType> pathEventTypes,
			ParentClassLoader classLoader)
		{
			pathEventTypes.Traverse(type => AddJsonUnderlyingClassInternal(type, classLoader, null));
		}

		private static void AddJsonUnderlyingClassInternal(
			EventType eventType,
			ParentClassLoader classLoader,
			string optionalDeploymentId)
		{
			if (!(eventType is JsonEventType)) {
				return;
			}

			var jsonEventType = (JsonEventType) eventType;
			// for named-window the same underlying is used and we ignore duplicate add
			var allowDuplicate = eventType.Metadata.TypeClass == EventTypeTypeClass.NAMED_WINDOW;
			if (jsonEventType.Detail.OptionalUnderlyingProvided == null) {
				classLoader.Add(jsonEventType.Detail.UnderlyingClassName, jsonEventType.UnderlyingType, optionalDeploymentId, allowDuplicate);
			}
			else {
				allowDuplicate = true;
			}

			classLoader.Add(jsonEventType.Detail.DelegateClassName, jsonEventType.DelegateType, optionalDeploymentId, allowDuplicate);
			classLoader.Add(jsonEventType.Detail.DelegateFactoryClassName, jsonEventType.DelegateFactory.GetType(), optionalDeploymentId, allowDuplicate);
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
