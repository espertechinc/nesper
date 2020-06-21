///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.serdeset.multikey;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.multikey
{
	public class MultiKeyPlanner
	{
		public static bool RequiresDeepEquals(Type arrayComponentType)
		{
			return arrayComponentType == typeof(object) || arrayComponentType.IsArray;
		}

		public static MultiKeyPlan PlanMultiKeyDistinct(
			bool isDistinct,
			EventType eventType,
			StatementRawInfo raw,
			SerdeCompileTimeResolver serdeResolver)
		{
			if (!isDistinct) {
				return new MultiKeyPlan(
					EmptyList<StmtClassForgeableFactory>.Instance,
					MultiKeyClassRefEmpty.INSTANCE);
			}

			string[] propertyNames = eventType.PropertyNames;
			Type[] props = new Type[propertyNames.Length];
			for (int i = 0; i < propertyNames.Length; i++) {
				props[i] = eventType.GetPropertyType(propertyNames[i]);
			}

			return PlanMultiKey(props, false, raw, serdeResolver);
		}

		public static MultiKeyPlan PlanMultiKey(
			ExprNode[] criteriaExpressions,
			bool lenientEquals,
			StatementRawInfo raw,
			SerdeCompileTimeResolver serdeResolver)
		{
			return PlanMultiKey(ExprNodeUtilityQuery.GetExprResultTypes(criteriaExpressions), lenientEquals, raw, serdeResolver);
		}

		public static MultiKeyPlan PlanMultiKey(
			ExprForge[] criteriaExpressions,
			bool lenientEquals,
			StatementRawInfo raw,
			SerdeCompileTimeResolver serdeResolver)
		{
			return PlanMultiKey(ExprNodeUtilityQuery.GetExprResultTypes(criteriaExpressions), lenientEquals, raw, serdeResolver);
		}

		public static Type GetMKClassForComponentType(Type componentType)
		{
			if (componentType == typeof(bool)) {
				return typeof(MultiKeyArrayBoolean);
			}
			else if (componentType == typeof(byte)) {
				return typeof(MultiKeyArrayByte);
			}
			else if (componentType == typeof(char)) {
				return typeof(MultiKeyArrayChar);
			}
			else if (componentType == typeof(short)) {
				return typeof(MultiKeyArrayShort);
			}
			else if (componentType == typeof(int)) {
				return typeof(MultiKeyArrayInt);
			}
			else if (componentType == typeof(long)) {
				return typeof(MultiKeyArrayLong);
			}
			else if (componentType == typeof(float)) {
				return typeof(MultiKeyArrayFloat);
			}
			else if (componentType == typeof(double)) {
				return typeof(MultiKeyArrayDouble);
			}

			return typeof(MultiKeyArrayObject);
		}

		public static DataInputOutputSerde GetMKSerdeClassForComponentType(Type componentType)
		{
			if (componentType == typeof(bool)) {
				return DIOMultiKeyArrayBooleanSerde.INSTANCE;
			}
			else if (componentType == typeof(byte)) {
				return DIOMultiKeyArrayByteSerde.INSTANCE;
			}
			else if (componentType == typeof(char)) {
				return DIOMultiKeyArrayCharSerde.INSTANCE;
			}
			else if (componentType == typeof(short)) {
				return DIOMultiKeyArrayShortSerde.INSTANCE;
			}
			else if (componentType == typeof(int)) {
				return DIOMultiKeyArrayIntSerde.INSTANCE;
			}
			else if (componentType == typeof(long)) {
				return DIOMultiKeyArrayLongSerde.INSTANCE;
			}
			else if (componentType == typeof(float)) {
				return DIOMultiKeyArrayFloatSerde.INSTANCE;
			}
			else if (componentType == typeof(double)) {
				return DIOMultiKeyArrayDoubleSerde.INSTANCE;
			}

			return DIOMultiKeyArrayObjectSerde.INSTANCE;
		}

		public static MultiKeyPlan PlanMultiKey(
			Type[] types,
			bool lenientEquals,
			StatementRawInfo raw,
			SerdeCompileTimeResolver serdeResolver)
		{
			if (types == null || types.Length == 0) {
				return new MultiKeyPlan(
					EmptyList<StmtClassForgeableFactory>.Instance,
					MultiKeyClassRefEmpty.INSTANCE);
			}

			if (types.Length == 1) {
				Type paramType = types[0];
				if (paramType == null || !paramType.IsArray) {
					DataInputOutputSerdeForge serdeForge = serdeResolver.SerdeForKeyNonArray(paramType, raw);
					return new MultiKeyPlan(
						EmptyList<StmtClassForgeableFactory>.Instance, 
						new MultiKeyClassRefWSerde(serdeForge, types));
				}

				Type mkClass = GetMKClassForComponentType(paramType.GetElementType());
				DataInputOutputSerde mkSerde = GetMKSerdeClassForComponentType(paramType.GetElementType());
				return new MultiKeyPlan(
					EmptyList<StmtClassForgeableFactory>.Instance,
					new MultiKeyClassRefPredetermined(
						mkClass,
						types,
						new DataInputOutputSerdeForgeSingleton(mkSerde.GetType())));
			}

			Type[] boxed = new Type[types.Length];
			for (int i = 0; i < boxed.Length; i++) {
				boxed[i] = Boxing.GetBoxedType(types[i]);
			}

			MultiKeyClassRefUUIDBased classNames = new MultiKeyClassRefUUIDBased(boxed);
			StmtClassForgeableFactory factoryMK = new ProxyStmtClassForgeableFactory(
				(
					namespaceScope,
					classPostfix) => {
					return new StmtClassForgeableMultiKey(classNames.GetClassNameMK(classPostfix), namespaceScope, types, lenientEquals);
				});

			DataInputOutputSerdeForge[] forges = serdeResolver.SerdeForMultiKey(types, raw);
			StmtClassForgeableFactory factoryMKSerde = new ProxyStmtClassForgeableFactory(
				(
					namespaceScope,
					classPostfix) => {
					return new StmtClassForgeableMultiKeySerde(
						classNames.GetClassNameMKSerde(classPostfix),
						namespaceScope,
						types,
						classNames.GetClassNameMK(classPostfix),
						forges);
				});

			IList<StmtClassForgeableFactory> forgeables = Arrays.AsList(factoryMK, factoryMKSerde);
			return new MultiKeyPlan(forgeables, classNames);
		}

		public static object ToMultiKey(object keyValue)
		{
			Type componentType = keyValue.GetType().GetElementType();
			if (componentType == typeof(bool)) {
				return new MultiKeyArrayBoolean((bool[]) keyValue);
			}
			else if (componentType == typeof(byte)) {
				return new MultiKeyArrayByte((byte[]) keyValue);
			}
			else if (componentType == typeof(char)) {
				return new MultiKeyArrayChar((char[]) keyValue);
			}
			else if (componentType == typeof(short)) {
				return new MultiKeyArrayShort((short[]) keyValue);
			}
			else if (componentType == typeof(int)) {
				return new MultiKeyArrayInt((int[]) keyValue);
			}
			else if (componentType == typeof(long)) {
				return new MultiKeyArrayLong((long[]) keyValue);
			}
			else if (componentType == typeof(float)) {
				return new MultiKeyArrayFloat((float[]) keyValue);
			}
			else if (componentType == typeof(double)) {
				return new MultiKeyArrayDouble((double[]) keyValue);
			}

			return new MultiKeyArrayObject((object[]) keyValue);
		}
	}
} // end of namespace
