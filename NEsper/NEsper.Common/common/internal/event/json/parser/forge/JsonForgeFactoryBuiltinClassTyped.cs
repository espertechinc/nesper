///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
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
		private readonly static IDictionary<Type, JsonEndValueForge> END_VALUE_FORGES = new Dictionary<Type, JsonEndValueForge>();
		private readonly static IDictionary<Type, Type> START_ARRAY_FORGES = new Dictionary<Type, Type>();
		private readonly static IDictionary<Type, Type> START_COLLECTION_FORGES = new Dictionary<Type, Type>();
		private readonly static IDictionary<Type, JsonWriteForge> WRITE_FORGES = new Dictionary<Type, JsonWriteForge>();
		private readonly static IDictionary<Type, JsonWriteForge> WRITE_ARRAY_FORGES = new Dictionary<Type, JsonWriteForge>();
		private readonly static IDictionary<Type, JsonWriteForge> WRITE_COLLECTION_FORGES = new Dictionary<Type, JsonWriteForge>();

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
			WRITE_FORGES.Put(typeof(byte?), JsonWriteForgeNumberWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(short?), JsonWriteForgeNumberWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(int?), JsonWriteForgeNumberWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(long?), JsonWriteForgeNumberWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(float?), JsonWriteForgeNumberWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(double?), JsonWriteForgeNumberWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(decimal?), JsonWriteForgeNumberWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(BigInteger?), JsonWriteForgeNumberWithToString.INSTANCE);

			WRITE_FORGES.Put(typeof(Guid), JsonWriteForgeStringWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(DateTimeEx), JsonWriteForgeStringWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(DateTimeOffset), JsonWriteForgeStringWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(DateTime), JsonWriteForgeStringWithToString.INSTANCE);
			WRITE_FORGES.Put(typeof(Uri), JsonWriteForgeStringWithToString.INSTANCE);

			START_ARRAY_FORGES.Put(typeof(string[]), typeof(JsonDelegateArrayString));
			START_ARRAY_FORGES.Put(typeof(char?[]), typeof(JsonDelegateArrayCharacter));
			START_ARRAY_FORGES.Put(typeof(bool?[]), typeof(JsonDelegateArrayBoolean));
			START_ARRAY_FORGES.Put(typeof(byte?[]), typeof(JsonDelegateArrayByte));
			START_ARRAY_FORGES.Put(typeof(short?[]), typeof(JsonDelegateArrayShort));
			START_ARRAY_FORGES.Put(typeof(int?[]), typeof(JsonDelegateArrayInteger));
			START_ARRAY_FORGES.Put(typeof(long?[]), typeof(JsonDelegateArrayLong));
			START_ARRAY_FORGES.Put(typeof(decimal?[]), typeof(JsonDelegateArrayDecimal));
			START_ARRAY_FORGES.Put(typeof(double?[]), typeof(JsonDelegateArrayDouble));
			START_ARRAY_FORGES.Put(typeof(float?[]), typeof(JsonDelegateArrayFloat));
			START_ARRAY_FORGES.Put(typeof(char[]), typeof(JsonDelegateArrayCharacterPrimitive));
			START_ARRAY_FORGES.Put(typeof(bool[]), typeof(JsonDelegateArrayBooleanPrimitive));
			START_ARRAY_FORGES.Put(typeof(byte[]), typeof(JsonDelegateArrayBytePrimitive));
			START_ARRAY_FORGES.Put(typeof(short[]), typeof(JsonDelegateArrayShortPrimitive));
			START_ARRAY_FORGES.Put(typeof(int[]), typeof(JsonDelegateArrayIntegerPrimitive));
			START_ARRAY_FORGES.Put(typeof(long[]), typeof(JsonDelegateArrayLongPrimitive));
			START_ARRAY_FORGES.Put(typeof(decimal[]), typeof(JsonDelegateArrayDecimalPrimitive));
			START_ARRAY_FORGES.Put(typeof(double[]), typeof(JsonDelegateArrayDoublePrimitive));
			START_ARRAY_FORGES.Put(typeof(float[]), typeof(JsonDelegateArrayFloatPrimitive));
			START_ARRAY_FORGES.Put(typeof(BigInteger[]), typeof(JsonDelegateArrayBigInteger));

			START_ARRAY_FORGES.Put(typeof(Guid[]), typeof(JsonDelegateArrayUUID));
			START_ARRAY_FORGES.Put(typeof(DateTimeEx[]), typeof(JsonDelegateArrayDateTimeEx));
			START_ARRAY_FORGES.Put(typeof(DateTimeOffset[]), typeof(JsonDelegateArrayDateTimeOffset));
			START_ARRAY_FORGES.Put(typeof(DateTime[]), typeof(JsonDelegateArrayDateTime));
			START_ARRAY_FORGES.Put(typeof(Uri[]), typeof(JsonDelegateArrayURI));

			START_ARRAY_FORGES.Put(typeof(string[][]), typeof(JsonDelegateArray2DimString));
		
			START_ARRAY_FORGES.Put(typeof(char?[][]), typeof(JsonDelegateArray2DimCharacter));
			START_ARRAY_FORGES.Put(typeof(bool?[][]), typeof(JsonDelegateArray2DimBoolean));
			START_ARRAY_FORGES.Put(typeof(byte?[][]), typeof(JsonDelegateArray2DimByte));
			START_ARRAY_FORGES.Put(typeof(short?[][]), typeof(JsonDelegateArray2DimShort));
			START_ARRAY_FORGES.Put(typeof(int?[][]), typeof(JsonDelegateArray2DimInteger));
			START_ARRAY_FORGES.Put(typeof(long?[][]), typeof(JsonDelegateArray2DimLong));
			START_ARRAY_FORGES.Put(typeof(float?[][]), typeof(JsonDelegateArray2DimFloat));
			START_ARRAY_FORGES.Put(typeof(double?[][]), typeof(JsonDelegateArray2DimDouble));
			START_ARRAY_FORGES.Put(typeof(decimal?[][]), typeof(JsonDelegateArray2DimDecimal));
			
			START_ARRAY_FORGES.Put(typeof(char[][]), typeof(JsonDelegateArray2DimCharacterPrimitive));
			START_ARRAY_FORGES.Put(typeof(bool[][]), typeof(JsonDelegateArray2DimBooleanPrimitive));
			START_ARRAY_FORGES.Put(typeof(byte[][]), typeof(JsonDelegateArray2DimBytePrimitive));
			START_ARRAY_FORGES.Put(typeof(short[][]), typeof(JsonDelegateArray2DimShortPrimitive));
			START_ARRAY_FORGES.Put(typeof(int[][]), typeof(JsonDelegateArray2DimIntegerPrimitive));
			START_ARRAY_FORGES.Put(typeof(long[][]), typeof(JsonDelegateArray2DimLongPrimitive));
			START_ARRAY_FORGES.Put(typeof(float[][]), typeof(JsonDelegateArray2DimFloatPrimitive));
			START_ARRAY_FORGES.Put(typeof(double[][]), typeof(JsonDelegateArray2DimDoublePrimitive));
			START_ARRAY_FORGES.Put(typeof(decimal[][]), typeof(JsonDelegateArray2DimDecimalPrimitive));
			
			START_ARRAY_FORGES.Put(typeof(BigInteger[][]), typeof(JsonDelegateArray2DimBigInteger));
			START_ARRAY_FORGES.Put(typeof(UUID[][]), typeof(JsonDelegateArray2DimUUID));
			START_ARRAY_FORGES.Put(typeof(OffsetDateTime[][]), typeof(JsonDelegateArray2DimOffsetDateTime));
			START_ARRAY_FORGES.Put(typeof(LocalDate[][]), typeof(JsonDelegateArray2DimLocalDate));
			START_ARRAY_FORGES.Put(typeof(LocalDateTime[][]), typeof(JsonDelegateArray2DimDateTime));
			START_ARRAY_FORGES.Put(typeof(ZonedDateTime[][]), typeof(JsonDelegateArray2DimZonedDateTime));
			START_ARRAY_FORGES.Put(typeof(URL[][]), typeof(JsonDelegateArray2DimURL));
			START_ARRAY_FORGES.Put(typeof(URI[][]), typeof(JsonDelegateArray2DimURI));

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

			foreach (Type clazz in new Type[] {
				typeof(UUID[]), typeof(OffsetDateTime[]), typeof(LocalDate[]), typeof(LocalDateTime[]), typeof(ZonedDateTime[]),
				typeof(URL[]), typeof(URI[])
			}) {
				WRITE_ARRAY_FORGES.Put(clazz, new JsonWriteForgeByMethod("WriteArrayObjectToString"));
			}

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
			foreach (Type clazz in new Type[] {
				typeof(UUID[][]), typeof(OffsetDateTime[][]), typeof(LocalDate[][]), typeof(LocalDateTime[][]), typeof(ZonedDateTime[][]),
				typeof(URL[][]), typeof(URI[][])
			}) {
				WRITE_ARRAY_FORGES.Put(clazz, new JsonWriteForgeByMethod("WriteArray2DimObjectToString"));
			}

			START_COLLECTION_FORGES.Put(typeof(string), typeof(JsonDelegateCollectionString));
			START_COLLECTION_FORGES.Put(typeof(char?), typeof(JsonDelegateCollectionCharacter));
			START_COLLECTION_FORGES.Put(typeof(bool?), typeof(JsonDelegateCollectionBoolean));
			START_COLLECTION_FORGES.Put(typeof(byte?), typeof(JsonDelegateCollectionByte));
			START_COLLECTION_FORGES.Put(typeof(short?), typeof(JsonDelegateCollectionShort));
			START_COLLECTION_FORGES.Put(typeof(int?), typeof(JsonDelegateCollectionInteger));
			START_COLLECTION_FORGES.Put(typeof(long?), typeof(JsonDelegateCollectionLong));
			START_COLLECTION_FORGES.Put(typeof(decimal?), typeof(JsonDelegateCollectionDecimal));
			START_COLLECTION_FORGES.Put(typeof(double?), typeof(JsonDelegateCollectionDouble));
			START_COLLECTION_FORGES.Put(typeof(float?), typeof(JsonDelegateCollectionFloat));
			START_COLLECTION_FORGES.Put(typeof(BigInteger), typeof(JsonDelegateCollectionBigInteger));
			START_COLLECTION_FORGES.Put(typeof(UUID), typeof(JsonDelegateCollectionUUID));
			START_COLLECTION_FORGES.Put(typeof(OffsetDateTime), typeof(JsonDelegateCollectionOffsetDateTime));
			START_COLLECTION_FORGES.Put(typeof(LocalDate), typeof(JsonDelegateCollectionLocalDate));
			START_COLLECTION_FORGES.Put(typeof(LocalDateTime), typeof(JsonDelegateCollectionLocalDateTime));
			START_COLLECTION_FORGES.Put(typeof(ZonedDateTime), typeof(JsonDelegateCollectionDateTime));
			START_COLLECTION_FORGES.Put(typeof(URL), typeof(JsonDelegateCollectionURL));
			START_COLLECTION_FORGES.Put(typeof(URI), typeof(JsonDelegateCollectionURI));

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
			WRITE_COLLECTION_FORGES.Put(typeof(BigDecimal), new JsonWriteForgeByMethod("WriteCollectionNumber"));
			WRITE_COLLECTION_FORGES.Put(typeof(BigInteger), new JsonWriteForgeByMethod("WriteCollectionNumber"));
			foreach (Type clazz in new Type[] {
				typeof(UUID), typeof(OffsetDateTime), typeof(LocalDate), typeof(LocalDateTime), typeof(ZonedDateTime),
				typeof(URL), typeof(URI)
			}) {
				WRITE_COLLECTION_FORGES.Put(clazz, new JsonWriteForgeByMethod("WriteCollectionWToString"));
			}
		}

		public static JsonForgeDesc Forge(
			Type type,
			string fieldName,
			FieldInfo optionalField,
			IDictionary<Type, JsonApplicationClassDelegateDesc> deepClasses,
			Attribute[] annotations,
			StatementCompileTimeServices services)
		{
			type = Boxing.GetBoxedType(type);
			JsonDelegateForge startObject = null;
			JsonDelegateForge startArray = null;
			JsonEndValueForge end = END_VALUE_FORGES.Get(type);
			JsonWriteForge write = WRITE_FORGES.Get(type);

			JsonSchemaField fieldAnnotation = FindFieldAnnotation(fieldName, annotations);

			if (fieldAnnotation != null && type != null) {
				Type clazz;
				try {
					clazz = services.ImportServiceCompileTime.ResolveClass(fieldAnnotation.Adapter(), true, ExtensionClassEmpty.INSTANCE);
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
				startObject = new JsonDelegateForgeByClass(typeof(JsonDelegateJsonGenericObject));
				startArray = new JsonDelegateForgeByClass(typeof(JsonDelegateJsonGenericArray));
				end = JsonEndValueForgeJsonValue.INSTANCE;
				write = new JsonWriteForgeByMethod("WriteJsonValue");
			}
			else if (type == typeof(object[])) {
				startArray = new JsonDelegateForgeByClass(typeof(JsonDelegateJsonGenericArray));
				end = new JsonEndValueForgeCast(type);
				write = new JsonWriteForgeByMethod("WriteJsonArray");
			}
			else if (type == typeof(IDictionary<string, object>)) {
				startObject = new JsonDelegateForgeByClass(typeof(JsonDelegateJsonGenericObject));
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
					startArray = new JsonDelegateForgeByClass(typeof(JsonDelegateArrayEnum), Constant(componentType));
					write = new JsonWriteForgeByMethod("WriteEnumArray");
				}
				else if (componentType.IsArray && componentType.GetElementType().IsEnum) {
					startArray = new JsonDelegateForgeByClass(typeof(JsonDelegateArray2DimEnum), Constant(componentType.GetElementType()));
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
						Type startArrayDelegateClass = START_ARRAY_FORGES.Get(type);
						if (startArrayDelegateClass == null) {
							throw GetUnsupported(type, fieldName);
						}

						startArray = new JsonDelegateForgeByClass(startArrayDelegateClass);
						write = WRITE_ARRAY_FORGES.Get(type);
					}
				}

				end = new JsonEndValueForgeCast(type);
			}
			else if (type == typeof(IList)) {
				if (optionalField != null) {
					Type genericType = TypeHelper.GetGenericFieldType(optionalField, true);
					if (genericType == null) {
						return null;
					}

					end = new JsonEndValueForgeCast(typeof(IList<object>)); // we are casting to list
					JsonApplicationClassDelegateDesc classNames = deepClasses.Get(genericType);
					if (classNames != null) {
						startArray = new JsonDelegateForgeWithDelegateFactoryCollection(classNames.DelegateFactoryClassName);
						write = new JsonWriteForgeAppClass(classNames.DelegateFactoryClassName, "WriteCollectionAppClass");
					}
					else {
						if (genericType.IsEnum) {
							startArray = new JsonDelegateForgeByClass(typeof(JsonDelegateCollectionEnum), Constant(genericType));
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

			foreach (Attribute annotation in annotations) {
				if (!(annotation is JsonSchemaFieldAttribute)) {
					continue;
				}

				var field = (JsonSchemaFieldAttribute) annotation;
				if (field.Name.Equals(fieldName)) {
					return field;
				}
			}

			return null;
		}

		private static UnsupportedOperationException GetUnsupported(
			Type type,
			string fieldName)
		{
			return new UnsupportedOperationException(
				"Unsupported type '" + type.Name + "' for property '" + fieldName + "' (use JsonSchemaField to declare additional information)");
		}
	}
} // end of namespace
