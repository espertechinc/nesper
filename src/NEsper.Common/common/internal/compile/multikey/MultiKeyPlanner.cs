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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.serdeset.multikey;
using com.espertech.esper.common.@internal.util;
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
                return new MultiKeyPlan(EmptyList<StmtClassForgeableFactory>.Instance, MultiKeyClassRefEmpty.INSTANCE);
            }

            var propertyNames = eventType.PropertyNames;
            var props = new Type[propertyNames.Length];
            for (var i = 0; i < propertyNames.Length; i++) {
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
            return PlanMultiKey(
                ExprNodeUtilityQuery.GetExprResultTypes(criteriaExpressions),
                lenientEquals,
                raw,
                serdeResolver);
        }

        public static MultiKeyPlan PlanMultiKey(
            ExprForge[] criteriaExpressions,
            bool lenientEquals,
            StatementRawInfo raw,
            SerdeCompileTimeResolver serdeResolver)
        {
            return PlanMultiKey(
                ExprNodeUtilityQuery.GetExprResultTypes(criteriaExpressions),
                lenientEquals,
                raw,
                serdeResolver);
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

        public static DIOMultiKeyArraySerde GetMKSerdeClassForComponentType(Type componentType)
        {
            return DIOMultiKeyArraySerdeFactory
                .GetSerde(componentType.IsPrimitive ? componentType : typeof(object));
        }

        public static MultiKeyPlan PlanMultiKey(
            Type[] types,
            bool lenientEquals,
            StatementRawInfo raw,
            SerdeCompileTimeResolver serdeResolver)
        {
            if (types == null || types.Length == 0) {
                return new MultiKeyPlan(EmptyList<StmtClassForgeableFactory>.Instance, MultiKeyClassRefEmpty.INSTANCE);
            }

            if (types.Length == 1) {
                var paramType = types[0];
                if (paramType == null || !paramType.IsArray) {
                    var serdeForge = serdeResolver.SerdeForKeyNonArray(paramType, raw);
                    return new MultiKeyPlan(EmptyList<StmtClassForgeableFactory>.Instance, new MultiKeyClassRefWSerde(serdeForge, types));
                }

                var componentType = paramType.GetComponentType();
                var mkClass = GetMKClassForComponentType(componentType);
                var mkSerde = GetMKSerdeClassForComponentType(componentType);
                return new MultiKeyPlan(
                    EmptyList<StmtClassForgeableFactory>.Instance, 
                    new MultiKeyClassRefPredetermined(
                        mkClass,
                        types,
                        new DataInputOutputSerdeForgeSingleton(mkSerde.GetType()),
                        mkSerde));
            }

            var boxed = new Type[types.Length];
            for (var i = 0; i < boxed.Length; i++) {
                boxed[i] = types[i].GetBoxedType();
            }

            var forges = serdeResolver.SerdeForMultiKey(boxed, raw);

            var classNames = new MultiKeyClassRefUUIDBased(boxed, forges);
            StmtClassForgeableFactory factoryMK = new ProxyStmtClassForgeableFactory(
                (
                    namespaceScope,
                    classPostfix) => new StmtClassForgeableMultiKey(
                    classNames.GetClassNameMK(classPostfix),
                    namespaceScope,
                    types,
                    lenientEquals));
            StmtClassForgeableFactory factoryMKSerde = new ProxyStmtClassForgeableFactory(
                (
                    namespaceScope,
                    classPostfix) => new StmtClassForgeableMultiKeySerde(
                    classNames.GetClassNameMKSerde(classPostfix),
                    namespaceScope,
                    types,
                    classNames.GetClassNameMK(classPostfix),
                    forges));

            var forgeables = Arrays.AsList(factoryMK, factoryMKSerde);
            return new MultiKeyPlan(forgeables, classNames);
        }

        public static object ToMultiKey(object keyValue)
        {
            var componentType = keyValue.GetType().GetElementType();
            if (componentType == typeof(bool)) {
                return new MultiKeyArrayBoolean((bool[])keyValue);
            }
            else if (componentType == typeof(byte)) {
                return new MultiKeyArrayByte((byte[])keyValue);
            }
            else if (componentType == typeof(char)) {
                return new MultiKeyArrayChar((char[])keyValue);
            }
            else if (componentType == typeof(short)) {
                return new MultiKeyArrayShort((short[])keyValue);
            }
            else if (componentType == typeof(int)) {
                return new MultiKeyArrayInt((int[])keyValue);
            }
            else if (componentType == typeof(long)) {
                return new MultiKeyArrayLong((long[])keyValue);
            }
            else if (componentType == typeof(float)) {
                return new MultiKeyArrayFloat((float[])keyValue);
            }
            else if (componentType == typeof(double)) {
                return new MultiKeyArrayDouble((double[])keyValue);
            }

            return new MultiKeyArrayObject((object[])keyValue);
        }
    }
} // end of namespace