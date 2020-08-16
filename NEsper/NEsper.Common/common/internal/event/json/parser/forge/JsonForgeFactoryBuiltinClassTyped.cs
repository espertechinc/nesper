///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.parser.deserializers.forge;
using com.espertech.esper.common.@internal.@event.json.serializers;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.json.parser.forge
{
	public class JsonForgeFactoryBuiltinClassTyped
	{
		// Don't like all of this "static" behavior...
		
		private static readonly IDictionary<Type, JsonDeserializerForge> DESERIALIZER_FORGES = new Dictionary<Type, JsonDeserializerForge>();
		private static readonly IDictionary<Type, JsonSerializerForge> SERIALIZER_FORGES = new Dictionary<Type, JsonSerializerForge>();

		static JsonForgeFactoryBuiltinClassTyped()
		{
			SERIALIZER_FORGES.Put(typeof(string), JsonSerializerForgeString.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(char?), JsonSerializerForgeStringWithToString.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(bool?), JsonSerializerForgeBoolean.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(byte?), JsonSerializerForgeNumber.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(short?), JsonSerializerForgeNumber.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(int?), JsonSerializerForgeNumber.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(long?), JsonSerializerForgeNumber.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(float?), JsonSerializerForgeNumber.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(double?), JsonSerializerForgeNumber.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(decimal?), JsonSerializerForgeNumber.INSTANCE);

			SERIALIZER_FORGES.Put(typeof(BigInteger?), JsonSerializerForgeNumber.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(BigInteger), JsonSerializerForgeNumber.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(Guid), JsonSerializerForgeStringWithToString.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(DateTimeEx), JsonSerializerForgeStringWithToString.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(DateTimeOffset), JsonSerializerForgeStringWithToString.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(DateTime), JsonSerializerForgeStringWithToString.INSTANCE);
			SERIALIZER_FORGES.Put(typeof(Uri), JsonSerializerForgeStringWithToString.INSTANCE);

			SERIALIZER_FORGES.Put(typeof(string[]), new JsonSerializerForgeByMethod("WriteArrayString"));
			
			SERIALIZER_FORGES.Put(typeof(char?[]), new JsonSerializerForgeByMethod("WriteArrayCharacter"));
			SERIALIZER_FORGES.Put(typeof(bool?[]), new JsonSerializerForgeByMethod("WriteArrayBoolean"));
			SERIALIZER_FORGES.Put(typeof(byte?[]), new JsonSerializerForgeByMethod("WriteArrayByte"));
			SERIALIZER_FORGES.Put(typeof(short?[]), new JsonSerializerForgeByMethod("WriteArrayShort"));
			SERIALIZER_FORGES.Put(typeof(int?[]), new JsonSerializerForgeByMethod("WriteArrayInteger"));
			SERIALIZER_FORGES.Put(typeof(long?[]), new JsonSerializerForgeByMethod("WriteArrayLong"));
			SERIALIZER_FORGES.Put(typeof(decimal?[]), new JsonSerializerForgeByMethod("WriteArrayDecimal"));
			SERIALIZER_FORGES.Put(typeof(double?[]), new JsonSerializerForgeByMethod("WriteArrayDouble"));
			SERIALIZER_FORGES.Put(typeof(float?[]), new JsonSerializerForgeByMethod("WriteArrayFloat"));
			
			SERIALIZER_FORGES.Put(typeof(char[]), new JsonSerializerForgeByMethod("WriteArrayCharPrimitive"));
			SERIALIZER_FORGES.Put(typeof(bool[]), new JsonSerializerForgeByMethod("WriteArrayBooleanPrimitive"));
			SERIALIZER_FORGES.Put(typeof(byte[]), new JsonSerializerForgeByMethod("WriteArrayBytePrimitive"));
			SERIALIZER_FORGES.Put(typeof(short[]), new JsonSerializerForgeByMethod("WriteArrayShortPrimitive"));
			SERIALIZER_FORGES.Put(typeof(int[]), new JsonSerializerForgeByMethod("WriteArrayIntPrimitive"));
			SERIALIZER_FORGES.Put(typeof(long[]), new JsonSerializerForgeByMethod("WriteArrayLongPrimitive"));
			SERIALIZER_FORGES.Put(typeof(decimal[]), new JsonSerializerForgeByMethod("WriteArrayDecimalPrimitive"));
			SERIALIZER_FORGES.Put(typeof(double[]), new JsonSerializerForgeByMethod("WriteArrayDoublePrimitive"));
			SERIALIZER_FORGES.Put(typeof(float[]), new JsonSerializerForgeByMethod("WriteArrayFloatPrimitive"));
			SERIALIZER_FORGES.Put(typeof(BigInteger[]), new JsonSerializerForgeByMethod("WriteArrayBigInteger"));

			SERIALIZER_FORGES.Put(typeof(Guid[]), new JsonSerializerForgeByMethod("WriteArrayObjectToString"));
			SERIALIZER_FORGES.Put(typeof(DateTimeEx[]), new JsonSerializerForgeByMethod("WriteArrayObjectToString"));
			SERIALIZER_FORGES.Put(typeof(DateTimeOffset[]), new JsonSerializerForgeByMethod("WriteArrayObjectToString"));
			SERIALIZER_FORGES.Put(typeof(DateTime[]), new JsonSerializerForgeByMethod("WriteArrayObjectToString"));
			SERIALIZER_FORGES.Put(typeof(Uri[]), new JsonSerializerForgeByMethod("WriteArrayObjectToString"));

			SERIALIZER_FORGES.Put(typeof(string[][]), new JsonSerializerForgeByMethod("WriteArray2DimString"));
			SERIALIZER_FORGES.Put(typeof(char?[][]), new JsonSerializerForgeByMethod("WriteArray2DimCharacter"));
			SERIALIZER_FORGES.Put(typeof(bool?[][]), new JsonSerializerForgeByMethod("WriteArray2DimBoolean"));
			SERIALIZER_FORGES.Put(typeof(byte?[][]), new JsonSerializerForgeByMethod("WriteArray2DimByte"));
			SERIALIZER_FORGES.Put(typeof(short?[][]), new JsonSerializerForgeByMethod("WriteArray2DimShort"));
			SERIALIZER_FORGES.Put(typeof(int?[][]), new JsonSerializerForgeByMethod("WriteArray2DimInteger"));
			SERIALIZER_FORGES.Put(typeof(long?[][]), new JsonSerializerForgeByMethod("WriteArray2DimLong"));
			SERIALIZER_FORGES.Put(typeof(decimal?[][]), new JsonSerializerForgeByMethod("WriteArray2DimDecimal"));
			SERIALIZER_FORGES.Put(typeof(double?[][]), new JsonSerializerForgeByMethod("WriteArray2DimDouble"));
			SERIALIZER_FORGES.Put(typeof(float?[][]), new JsonSerializerForgeByMethod("WriteArray2DimFloat"));
			SERIALIZER_FORGES.Put(typeof(char[][]), new JsonSerializerForgeByMethod("WriteArray2DimCharPrimitive"));
			SERIALIZER_FORGES.Put(typeof(bool[][]), new JsonSerializerForgeByMethod("WriteArray2DimBooleanPrimitive"));
			SERIALIZER_FORGES.Put(typeof(byte[][]), new JsonSerializerForgeByMethod("WriteArray2DimBytePrimitive"));
			SERIALIZER_FORGES.Put(typeof(short[][]), new JsonSerializerForgeByMethod("WriteArray2DimShortPrimitive"));
			SERIALIZER_FORGES.Put(typeof(int[][]), new JsonSerializerForgeByMethod("WriteArray2DimIntPrimitive"));
			SERIALIZER_FORGES.Put(typeof(long[][]), new JsonSerializerForgeByMethod("WriteArray2DimLongPrimitive"));
			SERIALIZER_FORGES.Put(typeof(decimal[][]), new JsonSerializerForgeByMethod("WriteArray2DimDecimalPrimitive"));
			SERIALIZER_FORGES.Put(typeof(double[][]), new JsonSerializerForgeByMethod("WriteArray2DimDoublePrimitive"));
			SERIALIZER_FORGES.Put(typeof(float[][]), new JsonSerializerForgeByMethod("WriteArray2DimFloatPrimitive"));
			SERIALIZER_FORGES.Put(typeof(BigInteger[][]), new JsonSerializerForgeByMethod("WriteArray2DimBigInteger"));

			SERIALIZER_FORGES.Put(typeof(Guid[][]), new JsonSerializerForgeByMethod("WriteArray2DimObjectToString"));
			SERIALIZER_FORGES.Put(typeof(DateTimeEx[][]), new JsonSerializerForgeByMethod("WriteArray2DimObjectToString"));
			SERIALIZER_FORGES.Put(typeof(DateTimeOffset[][]), new JsonSerializerForgeByMethod("WriteArray2DimObjectToString"));
			SERIALIZER_FORGES.Put(typeof(DateTime[][]), new JsonSerializerForgeByMethod("WriteArray2DimObjectToString"));
			SERIALIZER_FORGES.Put(typeof(Uri[][]), new JsonSerializerForgeByMethod("WriteArray2DimObjectToString"));

			// --------------------------------------------------------------------------------
			
			DESERIALIZER_FORGES.Put(typeof(string), JsonDeserializerForgeString.INSTANCE);
			DESERIALIZER_FORGES.Put(typeof(char?), JsonDeserializerForgeCharacter.INSTANCE);
			DESERIALIZER_FORGES.Put(typeof(bool?), JsonDeserializerForgeBoolean.INSTANCE);
			DESERIALIZER_FORGES.Put(typeof(byte?), JsonDeserializerForgeByte.INSTANCE);
		}

		public static JsonForgeDesc Forge(
			Type type,
			string fieldName,
			FieldInfo optionalField,
			IDictionary<Type, JsonApplicationClassDelegateDesc> deepClasses,
			Attribute[] annotations,
			StatementCompileTimeServices services)
		{
			type = type.GetBoxedType();

			var serializerForge = SERIALIZER_FORGES.Get(type);
			var deserializerForge = DESERIALIZER_FORGES.Get(type);

			throw new NotImplementedException("broken");
#if BRAINDEAD_BROKEN
			var fieldAnnotation = FindFieldAnnotation(fieldName, annotations);
			if (fieldAnnotation != null && type != null) {
				Type clazz;
				try {
					clazz = services.ImportServiceCompileTime.ResolveClass(fieldAnnotation.Adapter, true, ExtensionClassEmpty.INSTANCE);
				}
				catch (ImportException e) {
					throw new ExprValidationException("Failed to resolve Json schema field adapter class: " + e.Message, e);
				}

				if (!TypeHelper.IsImplementsInterface(clazz, typeof(JsonFieldAdapterString))) {
					throw new ExprValidationException("Json schema field adapter class does not implement interface '" + typeof(JsonFieldAdapterString).Name);
				}

				if (!ConstructorHelper.HasDefaultConstructor(clazz)) {
					throw new ExprValidationException("Json schema field adapter class '" + clazz.Name + "' does not have a default constructor");
				}

				MethodInfo writeMethod;
				try {
					writeMethod = MethodResolver.ResolveMethod(clazz, "parse", new Type[] {typeof(string)}, true, new bool[1], new bool[1]);
				}
				catch (MethodResolverNoSuchMethodException e) {
					throw new ExprValidationException("Failed to resolve write method of Json schema field adapter class: " + e.Message, e);
				}

				if (!TypeHelper.IsSubclassOrImplementsInterface(type, writeMethod.ReturnType)) {
					throw new ExprValidationException(
						"Json schema field adapter class '" +
						clazz.Name +
						"' mismatches the return type of the parse method, expected '" +
						type.Name +
						"' but found '" +
						writeMethod.ReturnType.Name +
						"'");
				}

				end = new JsonDeserializerForgeProvidedStringAdapter(clazz);
				serializerForge = new JsonSerializerForgeProvidedStringAdapter(clazz);
			}
			else if (type == typeof(object)) {
				deserializerForge = new JsonDeserializerForgeByClass(typeof(JsonDeserializerGenericObject));
				serializerForge = new JsonSerializerForgeByMethod("WriteJsonValue");
			}
			else if (type == typeof(object[])) {
				deserializerForge = new JsonDeserializerForgeByClass(typeof(JsonDeserializerGenericArray));
				serializerForge = new JsonSerializerForgeByMethod("WriteJsonArray");
			}
			else if (type == typeof(IDictionary<string, object>)) {
				deserializerForge = new JsonDeserializerForgeByClass(typeof(JsonDeserializerGenericObject));
				serializerForge = new JsonSerializerForgeByMethod("WriteJsonMap");
			}
			else if (type.IsEnum) {
				deserializerForge = new JsonDeserializerForgeEnum(type);
				serializerForge = JsonSerializerForgeStringWithToString.INSTANCE;
			}
			else if (type.IsArray) {
				var componentType = type.GetElementType();
				if (componentType.IsEnum) {
					deserializerForge = new JsonDeserializerForgeByClass(typeof(JsonDeserializerArrayEnum), Constant(componentType));
					serializerForge = new JsonSerializerForgeByMethod("WriteEnumArray");
				}
				else if (componentType.IsArray && componentType.GetElementType().IsEnum) {
					deserializerForge = new JsonDeserializerForgeByClass(typeof(JsonDeserializerArray2DimEnum), Constant(componentType.GetElementType()));
					serializerForge = new JsonSerializerForgeByMethod("WriteEnumArray2Dim");
				}
				else {
					Type arrayType = TypeHelper.GetArrayComponentTypeInnermost(type);
					JsonApplicationClassDelegateDesc classNames = deepClasses.Get(arrayType);
					if (classNames != null && TypeHelper.GetArrayDimensions(arrayType) <= 2) {
						if (componentType.IsArray) {
							startArray = new JsonAllocatorForgeWithAllocatorFactoryArray2Dim(classNames.DelegateFactoryClassName, componentType);
							serializerForge = new JsonSerializerForgeAppClass(classNames.DelegateFactoryClassName, "WriteArray2DimAppClass");
						}
						else {
							startArray = new JsonAllocatorForgeWithAllocatorFactoryArray(classNames.DelegateFactoryClassName, arrayType);
							serializerForge = new JsonSerializerForgeAppClass(classNames.DelegateFactoryClassName, "WriteArrayAppClass");
						}
					}
					else {
						var startArrayDelegateClass = START_ARRAY_FORGES.Get(type);
						if (startArrayDelegateClass == null) {
							throw GetUnsupported(type, fieldName);
						}

						startArray = new JsonDeserializerForgeByClass(startArrayDelegateClass);
						serializerForge = SERIALIZER_FORGES.Get(type);
					}
				}

				end = new JsonDeserializerForgeCast(type);
			}
			else if (type.IsGenericList()) {
				if (optionalField != null) {
					var genericType = TypeHelper.GetGenericFieldType(optionalField, true);
					if (genericType == null) {
						return null;
					}

					end = new JsonDeserializerForgeCast(typeof(IList<object>)); // we are casting to list
					
					var classNames = deepClasses.Get(genericType);
					if (classNames != null) {
						startArray = new JsonAllocatorForgeWithAllocatorFactoryCollection(classNames.DelegateFactoryClassName);
						serializerForge = new JsonSerializerForgeAppClass(classNames.DelegateFactoryClassName, "WriteCollectionAppClass");
					}
					else {
						if (genericType.IsEnum) {
							startArray = new JsonDeserializerForgeByClass(typeof(JsonDeserializerCollectionEnum), Constant(genericType));
							serializerForge = new JsonSerializerForgeByMethod("WriteEnumCollection");
						}
						else {
							Type startArrayDelegateClass = DESERIALIZER_FORGES.Get(genericType);
							if (startArrayDelegateClass == null) {
								throw GetUnsupported(genericType, fieldName);
							}

							startArray = new JsonDeserializerForgeByClass(startArrayDelegateClass);
							serializerForge = WRITE_COLLECTION_FORGES.Get(genericType);
						}
					}
				}
			}

			if (serializerForge == null) {
				throw GetUnsupported(type, fieldName);
			}

			return new JsonForgeDesc(fieldName, deserializerForge, serializerForge);
#endif
		}

		private static JsonSchemaFieldAttribute FindFieldAnnotation(
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

		private static UnsupportedOperationException GetUnsupported(
			Type type,
			string fieldName)
		{
			return new UnsupportedOperationException(
				$"Unsupported type '{type.Name}' for property '{fieldName}' (use JsonSchemaField to declare additional information)");
		}
	}
} // end of namespace
