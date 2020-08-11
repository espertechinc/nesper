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
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.parser.delegates.array;
using com.espertech.esper.common.@internal.@event.json.parser.delegates.array2dim;
using com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue;
using com.espertech.esper.common.@internal.@event.json.write;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // constant

namespace com.espertech.esper.common.@internal.@event.json.parser.forge
{
	public class JsonForgeFactoryBuiltinClassTyped
	{
		private static readonly IDictionary<Type, JsonEndValueForge> END_VALUE_FORGES = new Dictionary<Type, JsonEndValueForge>();
		private static readonly IDictionary<Type, Type> START_ARRAY_FORGES = new Dictionary<Type, Type>();
		private static readonly IDictionary<Type, Type> START_COLLECTION_FORGES = new Dictionary<Type, Type>();
		private static readonly IDictionary<Type, JsonWriteForge> WRITE_FORGES = new Dictionary<Type, JsonWriteForge>();
		private static readonly IDictionary<Type, JsonWriteForge> WRITE_ARRAY_FORGES = new Dictionary<Type, JsonWriteForge>();
		private static readonly IDictionary<Type, JsonWriteForge> WRITE_COLLECTION_FORGES = new Dictionary<Type, JsonWriteForge>();

		static JsonForgeFactoryBuiltinClassTyped()
		{
			END_VALUE_FORGES.Put(typeof(string), JsonEndValueForgeString.INSTANCE);
			END_VALUE_FORGES.Put(typeof(char?), JsonEndValueForgeCharacter.INSTANCE);
			END_VALUE_FORGES.Put(typeof(bool?), JsonEndValueForgeBoolean.INSTANCE);
			END_VALUE_FORGES.Put(typeof(byte?), JsonEndValueForgeByte.INSTANCE);
			END_VALUE_FORGES.Put(typeof(short?), JsonEndValueForgeShort.INSTANCE);
			END_VALUE_FORGES.Put(typeof(int?), JsonEndValueForgeInteger.INSTANCE);
			END_VALUE_FORGES.Put(typeof(long?), JsonEndValueForgeLong.INSTANCE);
			END_VALUE_FORGES.Put(typeof(float?), JsonEndValueForgeFloat.INSTANCE);
			END_VALUE_FORGES.Put(typeof(double?), JsonEndValueForgeDouble.INSTANCE);
			END_VALUE_FORGES.Put(typeof(decimal?), JsonEndValueForgeDecimal.INSTANCE);
			END_VALUE_FORGES.Put(typeof(BigInteger?), JsonEndValueForgeBigInteger.INSTANCE);

			END_VALUE_FORGES.Put(typeof(Guid?), JsonEndValueForgeUUID.INSTANCE);
			END_VALUE_FORGES.Put(typeof(DateTime?), JsonEndValueForgeDateTime.INSTANCE);
			END_VALUE_FORGES.Put(typeof(DateTimeOffset?), JsonEndValueForgeDateTimeOffset.INSTANCE);
			END_VALUE_FORGES.Put(typeof(DateTimeEx), JsonEndValueForgeDateTimeEx.INSTANCE);
			END_VALUE_FORGES.Put(typeof(Uri), JsonEndValueForgeURI.INSTANCE);

			WRITE_FORGES.Put(typeof(string), JsonWriteForgeString.INSTANCE);
			WRITE_FORGES.Put(typeof(char?), JsonWriteForgeStringWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(bool?), JsonWriteForgeBoolean.INSTANCE);
			WRITE_FORGES.Put(typeof(byte?), JsonWriteForgeNumber.INSTANCE);
			WRITE_FORGES.Put(typeof(short?), JsonWriteForgeNumber.INSTANCE);
			WRITE_FORGES.Put(typeof(int?), JsonWriteForgeNumber.INSTANCE);
			WRITE_FORGES.Put(typeof(long?), JsonWriteForgeNumber.INSTANCE);
			WRITE_FORGES.Put(typeof(float?), JsonWriteForgeNumber.INSTANCE);
			WRITE_FORGES.Put(typeof(double?), JsonWriteForgeNumber.INSTANCE);
			WRITE_FORGES.Put(typeof(decimal?), JsonWriteForgeNumber.INSTANCE);

			WRITE_FORGES.Put(typeof(BigInteger?), JsonWriteForgeNumber.INSTANCE);
			WRITE_FORGES.Put(typeof(BigInteger), JsonWriteForgeNumber.INSTANCE);
			WRITE_FORGES.Put(typeof(Guid), JsonWriteForgeStringWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(DateTimeEx), JsonWriteForgeStringWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(DateTimeOffset), JsonWriteForgeStringWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(DateTime), JsonWriteForgeStringWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(Uri), JsonWriteForgeStringWithToString.INSTANCE);

			START_ARRAY_FORGES.Put(typeof(string[]), typeof(JsonDeserializerArrayString));
			START_ARRAY_FORGES.Put(typeof(char?[]), typeof(JsonDeserializerArrayCharacter));
			START_ARRAY_FORGES.Put(typeof(bool?[]), typeof(JsonDeserializerArrayBoolean));
			START_ARRAY_FORGES.Put(typeof(byte?[]), typeof(JsonDeserializerArrayByte));
			START_ARRAY_FORGES.Put(typeof(short?[]), typeof(JsonDeserializerArrayShort));
			START_ARRAY_FORGES.Put(typeof(int?[]), typeof(JsonDeserializerArrayInteger));
			START_ARRAY_FORGES.Put(typeof(long?[]), typeof(JsonDeserializerArrayLong));
			START_ARRAY_FORGES.Put(typeof(decimal?[]), typeof(JsonDeserializerArrayDecimal));
			START_ARRAY_FORGES.Put(typeof(double?[]), typeof(JsonDeserializerArrayDouble));
			START_ARRAY_FORGES.Put(typeof(float?[]), typeof(JsonDeserializerArrayFloat));
			START_ARRAY_FORGES.Put(typeof(char[]), typeof(JsonDeserializerArrayCharacterPrimitive));
			START_ARRAY_FORGES.Put(typeof(bool[]), typeof(JsonDeserializerArrayBooleanPrimitive));
			START_ARRAY_FORGES.Put(typeof(byte[]), typeof(JsonDeserializerArrayBytePrimitive));
			START_ARRAY_FORGES.Put(typeof(short[]), typeof(JsonDeserializerArrayShortPrimitive));
			START_ARRAY_FORGES.Put(typeof(int[]), typeof(JsonDeserializerArrayIntegerPrimitive));
			START_ARRAY_FORGES.Put(typeof(long[]), typeof(JsonDeserializerArrayLongPrimitive));
			START_ARRAY_FORGES.Put(typeof(decimal[]), typeof(JsonDeserializerArrayDecimalPrimitive));
			START_ARRAY_FORGES.Put(typeof(double[]), typeof(JsonDeserializerArrayDoublePrimitive));
			START_ARRAY_FORGES.Put(typeof(float[]), typeof(JsonDeserializerArrayFloatPrimitive));
			
			START_ARRAY_FORGES.Put(typeof(BigInteger[]), typeof(JsonDeserializerArrayBigInteger));
			START_ARRAY_FORGES.Put(typeof(Guid[]), typeof(JsonDeserializerArrayUuid));
			START_ARRAY_FORGES.Put(typeof(DateTimeEx[]), typeof(JsonDeserializerArrayDateTimeEx));
			START_ARRAY_FORGES.Put(typeof(DateTimeOffset[]), typeof(JsonDeserializerArrayDateTimeOffset));
			START_ARRAY_FORGES.Put(typeof(DateTime[]), typeof(JsonDeserializerArrayDateTime));
			START_ARRAY_FORGES.Put(typeof(Uri[]), typeof(JsonDeserializerArrayUri));

			START_ARRAY_FORGES.Put(typeof(string[][]), typeof(JsonDeserializerArray2DimString));
			START_ARRAY_FORGES.Put(typeof(char?[][]), typeof(JsonDeserializerArray2DimCharacter));
			START_ARRAY_FORGES.Put(typeof(bool?[][]), typeof(JsonDeserializerArray2DimBoolean));
			START_ARRAY_FORGES.Put(typeof(byte?[][]), typeof(JsonDeserializerArray2DimByte));
			START_ARRAY_FORGES.Put(typeof(short?[][]), typeof(JsonDeserializerArray2DimShort));
			START_ARRAY_FORGES.Put(typeof(int?[][]), typeof(JsonDeserializerArray2DimInteger));
			START_ARRAY_FORGES.Put(typeof(long?[][]), typeof(JsonDeserializerArray2DimLong));
			START_ARRAY_FORGES.Put(typeof(float?[][]), typeof(JsonDeserializerArray2DimFloat));
			START_ARRAY_FORGES.Put(typeof(double?[][]), typeof(JsonDeserializerArray2DimDouble));
			START_ARRAY_FORGES.Put(typeof(decimal?[][]), typeof(JsonDeserializerArray2DimDecimal));
			START_ARRAY_FORGES.Put(typeof(char[][]), typeof(JsonDeserializerArray2DimCharacterPrimitive));
			START_ARRAY_FORGES.Put(typeof(bool[][]), typeof(JsonDeserializerArray2DimBooleanPrimitive));
			START_ARRAY_FORGES.Put(typeof(byte[][]), typeof(JsonDeserializerArray2DimBytePrimitive));
			START_ARRAY_FORGES.Put(typeof(short[][]), typeof(JsonDeserializerArray2DimShortPrimitive));
			START_ARRAY_FORGES.Put(typeof(int[][]), typeof(JsonDeserializerArray2DimIntegerPrimitive));
			START_ARRAY_FORGES.Put(typeof(long[][]), typeof(JsonDeserializerArray2DimLongPrimitive));
			START_ARRAY_FORGES.Put(typeof(float[][]), typeof(JsonDeserializerArray2DimFloatPrimitive));
			START_ARRAY_FORGES.Put(typeof(double[][]), typeof(JsonDeserializerArray2DimDoublePrimitive));
			START_ARRAY_FORGES.Put(typeof(decimal[][]), typeof(JsonDeserializerArray2DimDecimalPrimitive));
			
			START_ARRAY_FORGES.Put(typeof(BigInteger[][]), typeof(JsonDeserializerArray2DimBigInteger));
			START_ARRAY_FORGES.Put(typeof(Guid[][]), typeof(JsonDeserializerArray2DimUuid));
			START_ARRAY_FORGES.Put(typeof(DateTimeEx[][]), typeof(JsonDeserializerArray2DimDateTimeEx));
			START_ARRAY_FORGES.Put(typeof(DateTimeOffset[][]), typeof(JsonDeserializerArray2DimDateTimeOffset));
			START_ARRAY_FORGES.Put(typeof(DateTime[][]), typeof(JsonDeserializerArray2DimDateTime));
			START_ARRAY_FORGES.Put(typeof(Uri[][]), typeof(JsonDeserializerArray2DimUri));

			WRITE_ARRAY_FORGES.Put(typeof(string[]), new JsonWriteForgeByMethod("WriteArrayString"));
			WRITE_ARRAY_FORGES.Put(typeof(char?[]), new JsonWriteForgeByMethod("WriteArrayCharacter"));
			WRITE_ARRAY_FORGES.Put(typeof(bool?[]), new JsonWriteForgeByMethod("WriteArrayBoolean"));
			WRITE_ARRAY_FORGES.Put(typeof(byte?[]), new JsonWriteForgeByMethod("WriteArrayByte"));
			WRITE_ARRAY_FORGES.Put(typeof(short?[]), new JsonWriteForgeByMethod("WriteArrayShort"));
			WRITE_ARRAY_FORGES.Put(typeof(int?[]), new JsonWriteForgeByMethod("WriteArrayInteger"));
			WRITE_ARRAY_FORGES.Put(typeof(long?[]), new JsonWriteForgeByMethod("WriteArrayLong"));
			WRITE_ARRAY_FORGES.Put(typeof(decimal?[]), new JsonWriteForgeByMethod("WriteArrayDecimal"));
			WRITE_ARRAY_FORGES.Put(typeof(double?[]), new JsonWriteForgeByMethod("WriteArrayDouble"));
			WRITE_ARRAY_FORGES.Put(typeof(float?[]), new JsonWriteForgeByMethod("WriteArrayFloat"));
			WRITE_ARRAY_FORGES.Put(typeof(char[]), new JsonWriteForgeByMethod("WriteArrayCharPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(bool[]), new JsonWriteForgeByMethod("WriteArrayBooleanPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(byte[]), new JsonWriteForgeByMethod("WriteArrayBytePrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(short[]), new JsonWriteForgeByMethod("WriteArrayShortPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(int[]), new JsonWriteForgeByMethod("WriteArrayIntPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(long[]), new JsonWriteForgeByMethod("WriteArrayLongPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(decimal[]), new JsonWriteForgeByMethod("WriteArrayDecimalPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(double[]), new JsonWriteForgeByMethod("WriteArrayDoublePrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(float[]), new JsonWriteForgeByMethod("WriteArrayFloatPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(BigInteger[]), new JsonWriteForgeByMethod("WriteArrayBigInteger"));

			WRITE_ARRAY_FORGES.Put(typeof(Guid[]), new JsonWriteForgeByMethod("WriteArrayObjectToString"));
			WRITE_ARRAY_FORGES.Put(typeof(DateTimeEx[]), new JsonWriteForgeByMethod("WriteArrayObjectToString"));
			WRITE_ARRAY_FORGES.Put(typeof(DateTimeOffset[]), new JsonWriteForgeByMethod("WriteArrayObjectToString"));
			WRITE_ARRAY_FORGES.Put(typeof(DateTime[]), new JsonWriteForgeByMethod("WriteArrayObjectToString"));
			WRITE_ARRAY_FORGES.Put(typeof(Uri[]), new JsonWriteForgeByMethod("WriteArrayObjectToString"));

			WRITE_ARRAY_FORGES.Put(typeof(string[][]), new JsonWriteForgeByMethod("WriteArray2DimString"));
			WRITE_ARRAY_FORGES.Put(typeof(char?[][]), new JsonWriteForgeByMethod("WriteArray2DimCharacter"));
			WRITE_ARRAY_FORGES.Put(typeof(bool?[][]), new JsonWriteForgeByMethod("WriteArray2DimBoolean"));
			WRITE_ARRAY_FORGES.Put(typeof(byte?[][]), new JsonWriteForgeByMethod("WriteArray2DimByte"));
			WRITE_ARRAY_FORGES.Put(typeof(short?[][]), new JsonWriteForgeByMethod("WriteArray2DimShort"));
			WRITE_ARRAY_FORGES.Put(typeof(int?[][]), new JsonWriteForgeByMethod("WriteArray2DimInteger"));
			WRITE_ARRAY_FORGES.Put(typeof(long?[][]), new JsonWriteForgeByMethod("WriteArray2DimLong"));
			WRITE_ARRAY_FORGES.Put(typeof(decimal?[][]), new JsonWriteForgeByMethod("WriteArray2DimDecimal"));
			WRITE_ARRAY_FORGES.Put(typeof(double?[][]), new JsonWriteForgeByMethod("WriteArray2DimDouble"));
			WRITE_ARRAY_FORGES.Put(typeof(float?[][]), new JsonWriteForgeByMethod("WriteArray2DimFloat"));
			WRITE_ARRAY_FORGES.Put(typeof(char[][]), new JsonWriteForgeByMethod("WriteArray2DimCharPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(bool[][]), new JsonWriteForgeByMethod("WriteArray2DimBooleanPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(byte[][]), new JsonWriteForgeByMethod("WriteArray2DimBytePrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(short[][]), new JsonWriteForgeByMethod("WriteArray2DimShortPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(int[][]), new JsonWriteForgeByMethod("WriteArray2DimIntPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(long[][]), new JsonWriteForgeByMethod("WriteArray2DimLongPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(decimal[][]), new JsonWriteForgeByMethod("WriteArray2DimDecimalPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(double[][]), new JsonWriteForgeByMethod("WriteArray2DimDoublePrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(float[][]), new JsonWriteForgeByMethod("WriteArray2DimFloatPrimitive"));
			WRITE_ARRAY_FORGES.Put(typeof(BigInteger[][]), new JsonWriteForgeByMethod("WriteArray2DimBigInteger"));

			WRITE_ARRAY_FORGES.Put(typeof(Guid[][]), new JsonWriteForgeByMethod("WriteArray2DimObjectToString"));
			WRITE_ARRAY_FORGES.Put(typeof(DateTimeEx[][]), new JsonWriteForgeByMethod("WriteArray2DimObjectToString"));
			WRITE_ARRAY_FORGES.Put(typeof(DateTimeOffset[][]), new JsonWriteForgeByMethod("WriteArray2DimObjectToString"));
			WRITE_ARRAY_FORGES.Put(typeof(DateTime[][]), new JsonWriteForgeByMethod("WriteArray2DimObjectToString"));
			WRITE_ARRAY_FORGES.Put(typeof(Uri[][]), new JsonWriteForgeByMethod("WriteArray2DimObjectToString"));

			START_COLLECTION_FORGES.Put(typeof(string), typeof(JsonDeserializerCollectionString));
			START_COLLECTION_FORGES.Put(typeof(char?), typeof(JsonDeserializerCollectionCharacter));
			START_COLLECTION_FORGES.Put(typeof(bool?), typeof(JsonDeserializerCollectionBoolean));
			START_COLLECTION_FORGES.Put(typeof(byte?), typeof(JsonDeserializerCollectionByte));
			START_COLLECTION_FORGES.Put(typeof(short?), typeof(JsonDeserializerCollectionShort));
			START_COLLECTION_FORGES.Put(typeof(int?), typeof(JsonDeserializerCollectionInteger));
			START_COLLECTION_FORGES.Put(typeof(long?), typeof(JsonDeserializerCollectionLong));
			START_COLLECTION_FORGES.Put(typeof(decimal?), typeof(JsonDeserializerCollectionDecimal));
			START_COLLECTION_FORGES.Put(typeof(double?), typeof(JsonDeserializerCollectionDouble));
			START_COLLECTION_FORGES.Put(typeof(float?), typeof(JsonDeserializerCollectionFloat));
			START_COLLECTION_FORGES.Put(typeof(BigInteger), typeof(JsonDeserializerCollectionBigInteger));
			
			START_COLLECTION_FORGES.Put(typeof(Guid), typeof(JsonDeserializerCollectionUuid));
			START_COLLECTION_FORGES.Put(typeof(DateTimeEx), typeof(JsonDeserializerCollectionDateTimeEx));
			START_COLLECTION_FORGES.Put(typeof(DateTimeOffset), typeof(JsonDeserializerCollectionDateTimeOffset));
			START_COLLECTION_FORGES.Put(typeof(DateTime), typeof(JsonDeserializerCollectionDateTime));
			START_COLLECTION_FORGES.Put(typeof(Uri), typeof(JsonDeserializerCollectionUri));

			WRITE_COLLECTION_FORGES.Put(typeof(string), new JsonWriteForgeByMethod("WriteCollectionString"));
			WRITE_COLLECTION_FORGES.Put(typeof(char?), new JsonWriteForgeByMethod("WriteCollectionWToString"));
			WRITE_COLLECTION_FORGES.Put(typeof(bool?), new JsonWriteForgeByMethod("WriteCollectionBoolean"));
			WRITE_COLLECTION_FORGES.Put(typeof(byte?), new JsonWriteForgeByMethod("WriteCollectionNumber"));
			WRITE_COLLECTION_FORGES.Put(typeof(short?), new JsonWriteForgeByMethod("WriteCollectionNumber"));
			WRITE_COLLECTION_FORGES.Put(typeof(int?), new JsonWriteForgeByMethod("WriteCollectionNumber"));
			WRITE_COLLECTION_FORGES.Put(typeof(long?), new JsonWriteForgeByMethod("WriteCollectionNumber"));
			WRITE_COLLECTION_FORGES.Put(typeof(decimal?), new JsonWriteForgeByMethod("WriteCollectionNumber"));
			WRITE_COLLECTION_FORGES.Put(typeof(double?), new JsonWriteForgeByMethod("WriteCollectionNumber"));
			WRITE_COLLECTION_FORGES.Put(typeof(float?), new JsonWriteForgeByMethod("WriteCollectionNumber"));
			WRITE_COLLECTION_FORGES.Put(typeof(BigInteger), new JsonWriteForgeByMethod("WriteCollectionNumber"));
			
			WRITE_COLLECTION_FORGES.Put(typeof(Guid), new JsonWriteForgeByMethod("WriteCollectionWToString"));
			WRITE_COLLECTION_FORGES.Put(typeof(DateTimeEx), new JsonWriteForgeByMethod("WriteCollectionWToString"));
			WRITE_COLLECTION_FORGES.Put(typeof(DateTimeOffset), new JsonWriteForgeByMethod("WriteCollectionWToString"));
			WRITE_COLLECTION_FORGES.Put(typeof(DateTime), new JsonWriteForgeByMethod("WriteCollectionWToString"));
			WRITE_COLLECTION_FORGES.Put(typeof(Uri), new JsonWriteForgeByMethod("WriteCollectionWToString"));
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
			JsonDelegateForge startObject = null;
			JsonDelegateForge startArray = null;
			JsonEndValueForge end = END_VALUE_FORGES.Get(type);
			JsonWriteForge write = WRITE_FORGES.Get(type);

			JsonSchemaFieldAttribute fieldAnnotation = FindFieldAnnotation(fieldName, annotations);

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

				end = new JsonEndValueForgeProvidedStringAdapter(clazz);
				write = new JsonWriteForgeProvidedStringAdapter(clazz);
			}
			else if (type == typeof(object)) {
				startObject = new JsonDelegateForgeByClass(typeof(JsonDeserializerGenericObject));
				startArray = new JsonDelegateForgeByClass(typeof(JsonDeserializerGenericArray));
				end = JsonEndValueForgeJsonValue.INSTANCE;
				write = new JsonWriteForgeByMethod("WriteJsonValue");
			}
			else if (type == typeof(object[])) {
				startArray = new JsonDelegateForgeByClass(typeof(JsonDeserializerGenericArray));
				end = new JsonEndValueForgeCast(type);
				write = new JsonWriteForgeByMethod("WriteJsonArray");
			}
			else if (type == typeof(IDictionary<string, object>)) {
				startObject = new JsonDelegateForgeByClass(typeof(JsonDeserializerGenericObject));
				end = new JsonEndValueForgeCast(type);
				write = new JsonWriteForgeByMethod("WriteJsonMap");
			}
			else if (type.IsEnum) {
				end = new JsonEndValueForgeEnum(type);
				write = JsonWriteForgeStringWithToString.INSTANCE;
			}
			else if (type.IsArray) {
				var componentType = type.GetElementType();
				if (componentType.IsEnum) {
					startArray = new JsonDelegateForgeByClass(typeof(JsonDeserializerArrayEnum), Constant(componentType));
					write = new JsonWriteForgeByMethod("WriteEnumArray");
				}
				else if (componentType.IsArray && componentType.GetElementType().IsEnum) {
					startArray = new JsonDelegateForgeByClass(typeof(JsonDeserializerArray2DimEnum), Constant(componentType.GetElementType()));
					write = new JsonWriteForgeByMethod("WriteEnumArray2Dim");
				}
				else {
					Type arrayType = TypeHelper.GetArrayComponentTypeInnermost(type);
					JsonApplicationClassDelegateDesc classNames = deepClasses.Get(arrayType);
					if (classNames != null && TypeHelper.GetArrayDimensions(arrayType) <= 2) {
						if (componentType.IsArray) {
							startArray = new JsonDelegateForgeWithDelegateFactoryArray2Dim(classNames.DelegateFactoryClassName, componentType);
							write = new JsonWriteForgeAppClass(classNames.DelegateFactoryClassName, "WriteArray2DimAppClass");
						}
						else {
							startArray = new JsonDelegateForgeWithDelegateFactoryArray(classNames.DelegateFactoryClassName, arrayType);
							write = new JsonWriteForgeAppClass(classNames.DelegateFactoryClassName, "WriteArrayAppClass");
						}
					}
					else {
						var startArrayDelegateClass = START_ARRAY_FORGES.Get(type);
						if (startArrayDelegateClass == null) {
							throw GetUnsupported(type, fieldName);
						}

						startArray = new JsonDelegateForgeByClass(startArrayDelegateClass);
						write = WRITE_ARRAY_FORGES.Get(type);
					}
				}

				end = new JsonEndValueForgeCast(type);
			}
			else if (type == typeof(IList<object>)) {
				if (optionalField != null) {
					var genericType = TypeHelper.GetGenericFieldType(optionalField, true);
					if (genericType == null) {
						return null;
					}

					end = new JsonEndValueForgeCast(typeof(IList<object>)); // we are casting to list
					
					var classNames = deepClasses.Get(genericType);
					if (classNames != null) {
						startArray = new JsonDelegateForgeWithDelegateFactoryCollection(classNames.DelegateFactoryClassName);
						write = new JsonWriteForgeAppClass(classNames.DelegateFactoryClassName, "WriteCollectionAppClass");
					}
					else {
						if (genericType.IsEnum) {
							startArray = new JsonDelegateForgeByClass(typeof(JsonDeserializerCollectionEnum), Constant(genericType));
							write = new JsonWriteForgeByMethod("WriteEnumCollection");
						}
						else {
							Type startArrayDelegateClass = START_COLLECTION_FORGES.Get(genericType);
							if (startArrayDelegateClass == null) {
								throw GetUnsupported(genericType, fieldName);
							}

							startArray = new JsonDelegateForgeByClass(startArrayDelegateClass);
							write = WRITE_COLLECTION_FORGES.Get(genericType);
						}
					}
				}
			}

			if (end == null) {
				JsonApplicationClassDelegateDesc delegateDesc = deepClasses.Get(type);
				if (delegateDesc == null) {
					throw GetUnsupported(type, fieldName);
				}

				end = new JsonEndValueForgeCast(type);
				write = new JsonWriteForgeDelegate(delegateDesc.DelegateFactoryClassName);
				if (optionalField != null && optionalField.DeclaringType == optionalField.FieldType) {
					startObject = new JsonDelegateForgeWithDelegateFactorySelf(delegateDesc.DelegateClassName, optionalField.FieldType);
				}
				else {
					startObject = new JsonDelegateForgeWithDelegateFactory(delegateDesc.DelegateFactoryClassName);
				}
			}

			if (write == null) {
				throw GetUnsupported(type, fieldName);
			}

			return new JsonForgeDesc(fieldName, startObject, startArray, end, write);
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
