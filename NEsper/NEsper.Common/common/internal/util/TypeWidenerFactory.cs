///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Factory for type widening.
    /// </summary>
    public class TypeWidenerFactory
    {
        private static readonly TypeWidenerSPI STRING_TO_CHAR_COERCER = new ProxyTypeWidenerSPI {
            ProcWiden = input => SimpleTypeCasterFactory.CharTypeCaster.Cast(input),
            ProcWidenCodegen = (
                    expression,
                    codegenMethodScope,
                    codegenClassScope) =>
                SimpleTypeCasterFactory.CharTypeCaster.Codegen(
                    expression,
                    typeof(object),
                    codegenMethodScope,
                    codegenClassScope)
        };

        public static readonly TypeWidenerObjectArrayToCollectionCoercer OBJECT_ARRAY_TO_COLLECTION_COERCER =
            new TypeWidenerObjectArrayToCollectionCoercer();

        private static readonly TypeWidenerByteArrayToCollectionCoercer BYTE_ARRAY_TO_COLLECTION_COERCER =
            new TypeWidenerByteArrayToCollectionCoercer();

        private static readonly TypeWidenerShortArrayToCollectionCoercer SHORT_ARRAY_TO_COLLECTION_COERCER =
            new TypeWidenerShortArrayToCollectionCoercer();

        private static readonly TypeWidenerIntArrayToCollectionCoercer INT_ARRAY_TO_COLLECTION_COERCER =
            new TypeWidenerIntArrayToCollectionCoercer();

        private static readonly TypeWidenerLongArrayToCollectionCoercer LONG_ARRAY_TO_COLLECTION_COERCER =
            new TypeWidenerLongArrayToCollectionCoercer();

        private static readonly TypeWidenerFloatArrayToCollectionCoercer FLOAT_ARRAY_TO_COLLECTION_COERCER =
            new TypeWidenerFloatArrayToCollectionCoercer();

        private static readonly TypeWidenerDoubleArrayToCollectionCoercer DOUBLE_ARRAY_TO_COLLECTION_COERCER =
            new TypeWidenerDoubleArrayToCollectionCoercer();

        private static readonly TypeWidenerBooleanArrayToCollectionCoercer BOOLEAN_ARRAY_TO_COLLECTION_COERCER =
            new TypeWidenerBooleanArrayToCollectionCoercer();

        private static readonly TypeWidenerCharArrayToCollectionCoercer CHAR_ARRAY_TO_COLLECTION_COERCER =
            new TypeWidenerCharArrayToCollectionCoercer();

        /// <summary>
        ///     Returns the widener.
        /// </summary>
        /// <param name="columnName">name of column</param>
        /// <param name="columnType">type of column</param>
        /// <param name="writeablePropertyType">property type</param>
        /// <param name="writeablePropertyName">propery name</param>
        /// <param name="allowObjectArrayToCollectionConversion">whether we widen object-array to collection</param>
        /// <param name="customizer">customization if any</param>
        /// <param name="statementName">statement name</param>
        /// <returns>type widender</returns>
        /// <throws>TypeWidenerException if type validation fails</throws>
        public static TypeWidenerSPI GetCheckPropertyAssignType(
            string columnName,
            Type columnType,
            Type writeablePropertyType,
            string writeablePropertyName,
            bool allowObjectArrayToCollectionConversion,
            TypeWidenerCustomizer customizer,
            string statementName)
        {
            var columnClassBoxed = columnType.GetBoxedType();
            var targetClassBoxed = writeablePropertyType.GetBoxedType();

            if (customizer != null) {
                var custom = customizer.WidenerFor(
                    columnName,
                    columnType,
                    writeablePropertyType,
                    writeablePropertyName,
                    statementName);
                if (custom != null) {
                    return custom;
                }
            }

            if (columnType == null) {
                if (writeablePropertyType.IsPrimitive) {
                    var message = "Invalid assignment of column '" +
                                  columnName +
                                  "' of null type to event property '" +
                                  writeablePropertyName +
                                  "' typed as '" +
                                  writeablePropertyType.Name +
                                  "', nullable type mismatch";
                    throw new TypeWidenerException(message);
                }
            }
            else if (columnClassBoxed != targetClassBoxed) {
                if (columnClassBoxed == typeof(string) && targetClassBoxed == typeof(char?)) {
                    return STRING_TO_CHAR_COERCER;
                }

                if (allowObjectArrayToCollectionConversion &&
                    columnClassBoxed.IsArray &&
                    !columnClassBoxed.GetElementType().IsPrimitive &&
                    targetClassBoxed.IsImplementsInterface(typeof(ICollection<object>))) {
                    return OBJECT_ARRAY_TO_COLLECTION_COERCER;
                }

                if (!columnClassBoxed.IsAssignmentCompatible(targetClassBoxed)) {
                    var writablePropName = writeablePropertyType.Name;
                    if (writeablePropertyType.IsArray) {
                        writablePropName = writeablePropertyType.GetElementType().Name + "[]";
                    }

                    var columnTypeName = columnType.Name;
                    if (columnType.IsArray) {
                        columnTypeName = columnType.GetElementType().Name + "[]";
                    }

                    var message = "Invalid assignment of column '" +
                                  columnName +
                                  "' of type '" +
                                  columnTypeName +
                                  "' to event property '" +
                                  writeablePropertyName +
                                  "' typed as '" +
                                  writablePropName +
                                  "', column and parameter types mismatch";
                    throw new TypeWidenerException(message);
                }

                if (writeablePropertyType.IsNumeric()) {
                    return new TypeWidenerBoxedNumeric(
                        SimpleNumberCoercerFactory.GetCoercer(columnClassBoxed, targetClassBoxed));
                }
            }

            return null;
        }

        public static TypeWidenerSPI GetArrayToCollectionCoercer(Type componentType)
        {
            if (!componentType.IsPrimitive) {
                return OBJECT_ARRAY_TO_COLLECTION_COERCER;
            }

            if (componentType == typeof(byte)) {
                return BYTE_ARRAY_TO_COLLECTION_COERCER;
            }

            if (componentType == typeof(short)) {
                return SHORT_ARRAY_TO_COLLECTION_COERCER;
            }

            if (componentType == typeof(int)) {
                return INT_ARRAY_TO_COLLECTION_COERCER;
            }

            if (componentType == typeof(long)) {
                return LONG_ARRAY_TO_COLLECTION_COERCER;
            }

            if (componentType == typeof(float)) {
                return FLOAT_ARRAY_TO_COLLECTION_COERCER;
            }

            if (componentType == typeof(double)) {
                return DOUBLE_ARRAY_TO_COLLECTION_COERCER;
            }

            if (componentType == typeof(bool)) {
                return BOOLEAN_ARRAY_TO_COLLECTION_COERCER;
            }

            if (componentType == typeof(char)) {
                return CHAR_ARRAY_TO_COLLECTION_COERCER;
            }

            throw new IllegalStateException("Unrecognized class " + componentType);
        }

        public static CodegenExpression CodegenWidener(
            TypeWidenerSPI widener,
            CodegenMethod method,
            Type originator,
            CodegenClassScope classScope)
        {
            var anonymousClass = NewAnonymousClass(method.Block, typeof(TypeWidener));
            var widen = CodegenMethod.MakeParentNode(typeof(object), originator, classScope)
                .AddParam(typeof(object), "input");
            anonymousClass.AddMethod("widen", widen);
            var widenResult = widener.WidenCodegen(Ref("input"), method, classScope);
            widen.Block.MethodReturn(widenResult);
            return anonymousClass;
        }

        protected internal static CodegenExpression CodegenWidenArrayAsListMayNull(
            CodegenExpression expression,
            Type arrayType,
            CodegenMethodScope codegenMethodScope,
            Type generator,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope.MakeChild(typeof(ICollection<object>), generator, codegenClassScope)
                .AddParam(typeof(object), "input")
                .Block
                .IfRefNullReturnNull("input")
                .MethodReturn(StaticMethod(typeof(CompatExtensions), "AsList", Cast(arrayType, Ref("input"))));
            return LocalMethodBuild(method).Pass(expression).Call();
        }

        internal class TypeWidenerByteArrayToCollectionCoercer : TypeWidenerSPI
        {
            public object Widen(object input)
            {
                return input == null ? null : Arrays.AsList((byte[]) input);
            }

            public CodegenExpression WidenCodegen(
                CodegenExpression expression,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenWidenArrayAsListMayNull(
                    expression,
                    typeof(byte[]),
                    codegenMethodScope,
                    typeof(TypeWidenerByteArrayToCollectionCoercer),
                    codegenClassScope);
            }
        }

        internal class TypeWidenerShortArrayToCollectionCoercer : TypeWidenerSPI
        {
            public object Widen(object input)
            {
                return input == null ? null : Arrays.AsList((short[]) input);
            }

            public CodegenExpression WidenCodegen(
                CodegenExpression expression,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenWidenArrayAsListMayNull(
                    expression,
                    typeof(short[]),
                    codegenMethodScope,
                    typeof(TypeWidenerShortArrayToCollectionCoercer),
                    codegenClassScope);
            }
        }

        internal class TypeWidenerIntArrayToCollectionCoercer : TypeWidenerSPI
        {
            public object Widen(object input)
            {
                return input == null ? null : Arrays.AsList((int[]) input);
            }

            public CodegenExpression WidenCodegen(
                CodegenExpression expression,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenWidenArrayAsListMayNull(
                    expression,
                    typeof(int[]),
                    codegenMethodScope,
                    typeof(TypeWidenerIntArrayToCollectionCoercer),
                    codegenClassScope);
            }
        }

        internal class TypeWidenerLongArrayToCollectionCoercer : TypeWidenerSPI
        {
            public object Widen(object input)
            {
                return input == null ? null : Arrays.AsList((long[]) input);
            }

            public CodegenExpression WidenCodegen(
                CodegenExpression expression,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenWidenArrayAsListMayNull(
                    expression,
                    typeof(long[]),
                    codegenMethodScope,
                    typeof(TypeWidenerLongArrayToCollectionCoercer),
                    codegenClassScope);
            }
        }

        internal class TypeWidenerFloatArrayToCollectionCoercer : TypeWidenerSPI
        {
            public object Widen(object input)
            {
                return input == null ? null : Arrays.AsList((float[]) input);
            }

            public CodegenExpression WidenCodegen(
                CodegenExpression expression,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenWidenArrayAsListMayNull(
                    expression,
                    typeof(float[]),
                    codegenMethodScope,
                    typeof(TypeWidenerFloatArrayToCollectionCoercer),
                    codegenClassScope);
            }
        }

        internal class TypeWidenerDoubleArrayToCollectionCoercer : TypeWidenerSPI
        {
            public object Widen(object input)
            {
                return input == null ? null : Arrays.AsList((double[]) input);
            }

            public CodegenExpression WidenCodegen(
                CodegenExpression expression,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenWidenArrayAsListMayNull(
                    expression,
                    typeof(double[]),
                    codegenMethodScope,
                    typeof(TypeWidenerDoubleArrayToCollectionCoercer),
                    codegenClassScope);
            }
        }

        internal class TypeWidenerBooleanArrayToCollectionCoercer : TypeWidenerSPI
        {
            public object Widen(object input)
            {
                return input == null ? null : Arrays.AsList((bool[]) input);
            }

            public CodegenExpression WidenCodegen(
                CodegenExpression expression,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenWidenArrayAsListMayNull(
                    expression,
                    typeof(bool[]),
                    codegenMethodScope,
                    typeof(TypeWidenerBooleanArrayToCollectionCoercer),
                    codegenClassScope);
            }
        }

        internal class TypeWidenerCharArrayToCollectionCoercer : TypeWidenerSPI
        {
            public object Widen(object input)
            {
                return input == null ? null : Arrays.AsList((char[]) input);
            }

            public CodegenExpression WidenCodegen(
                CodegenExpression expression,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenWidenArrayAsListMayNull(
                    expression,
                    typeof(char[]),
                    codegenMethodScope,
                    typeof(TypeWidenerCharArrayToCollectionCoercer),
                    codegenClassScope);
            }
        }
    }
} // end of namespace