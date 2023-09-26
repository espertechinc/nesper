///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Factory for type widening.
    /// </summary>
    public partial class TypeWidenerFactory
    {
        private static readonly TypeWidenerSPI STRING_TO_CHAR_COERCER = new ProxyTypeWidenerSPI {
            ProcWidenResultType = () => typeof(char),
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
        /// Returns the widener.
        /// </summary>
        /// <param name="columnName">name of column</param>
        /// <param name="columnType">type of column</param>
        /// <param name="writeablePropertyType">property type</param>
        /// <param name="writeablePropertyName">property name</param>
        /// <param name="allowObjectArrayToCollectionConversion">whether we widen object-array to collection</param>
        /// <param name="customizer">customization if any</param>
        /// <param name="statementName">statement name</param>
        /// <returns>type widener</returns>
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
            var columnTypeBoxed = columnType.GetBoxedType();
            var targetTypeBoxed = writeablePropertyType.GetBoxedType();

            var custom = customizer?.WidenerFor(
                columnName,
                columnType,
                writeablePropertyType,
                writeablePropertyName,
                statementName);
            if (custom != null) {
                return custom;
            }

            if (writeablePropertyType == null) {
                return null;
            }

            if (columnType == null) {
                if (writeablePropertyType.CanNotBeNull()) {
                    var message = "Invalid assignment of column '" +
                                  columnName +
                                  "' of null type to event property '" +
                                  writeablePropertyName +
                                  "' typed as '" +
                                  writeablePropertyType.CleanName() +
                                  "', nullable type mismatch";
                    throw new TypeWidenerException(message);
                }
            }
            else if (columnTypeBoxed != targetTypeBoxed) {
                if (columnTypeBoxed == typeof(string) && targetTypeBoxed == typeof(char?)) {
                    return STRING_TO_CHAR_COERCER;
                }

                if (allowObjectArrayToCollectionConversion &&
                    columnTypeBoxed.IsArray &&
                    !columnTypeBoxed.GetElementType().IsValueType &&
                    targetTypeBoxed.IsImplementsInterface(typeof(ICollection<object>))) {
                    return OBJECT_ARRAY_TO_COLLECTION_COERCER;
                }

                // Boxed types tend to be incompatible from an assignment perspective.  We have both
                // the boxed and unboxed values.  The problem is that the boxed values will always
                // be unboxed prior to assignment, so looking for assignment of boxed types is not
                // a winning approach.

                var columnTypeUnboxed = columnType.GetUnboxedType();
                var targetTypeUnboxed = targetTypeBoxed.GetUnboxedType();

                if (!columnType.IsAssignmentCompatible(writeablePropertyType) &&
                    !columnTypeUnboxed.IsAssignmentCompatible(targetTypeUnboxed)) {

                    // Arrays can be assigned to each other if the underlying target types
                    // can be assigned from one another.
                    if (columnType.IsArray &&
                        targetTypeBoxed.IsArray &&
                        columnType.GetArrayRank() == targetTypeBoxed.GetArrayRank()) {
                        var columnElementType = columnType.GetElementType();
                        var targetElementType = targetTypeBoxed.GetElementType();
                        if (columnElementType.IsAssignmentCompatible(targetElementType)) {
                            return new TypeWidenerCompatibleArrayCoercer(
                                columnElementType,
                                targetElementType);
                        }
                    }

                    var writablePropName = writeablePropertyType.CleanName();
                    if (writeablePropertyType.IsArray) {
                        writablePropName = writeablePropertyType.GetElementType().CleanName() + "[]";
                    }

                    var columnTypeName = columnType.CleanName();
                    if (columnType.IsArray) {
                        columnTypeName = columnType.GetElementType().CleanName() + "[]";
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

                if (writeablePropertyType.IsTypeNumeric()) {
                    return new TypeWidenerBoxedNumeric(
                        SimpleNumberCoercerFactory.GetCoercer(columnTypeBoxed, targetTypeBoxed));
                }
            }

            return null;
        }

        public static TypeWidenerSPI GetArrayToCollectionCoercer(Type componentType)
        {
            if (componentType.CanBeNull()) {
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
            var widen = new CodegenExpressionLambda(method.Block).WithParam<object>("input");
            var anonymousClass = NewInstance<ProxyTypeWidener>(widen);

            //var anonymousClass = NewAnonymousClass(method.Block, typeof(TypeWidener));
            //var widen = CodegenMethod.MakeParentNode(typeof(object), originator, classScope)
            //    .AddParam(typeof(object), "input");
            //anonymousClass.AddMethod("widen", widen);

            var widenResult = widener.WidenCodegen(Ref("input"), method, classScope);
            widen.Block.BlockReturn(widenResult);
            return anonymousClass;
        }

        internal static CodegenExpression CodegenWidenArrayAsListMayNull(
            CodegenExpression expression,
            Type arrayType,
            CodegenMethodScope codegenMethodScope,
            Type generator,
            CodegenClassScope codegenClassScope)
        {
            var elementType = arrayType.GetElementType();
            var collectionType = typeof(ICollection<>).MakeGenericType(elementType);
            var method = codegenMethodScope
                .MakeChild(collectionType, generator, codegenClassScope)
                .AddParam<object>("input")
                .Block
                .IfRefNullReturnNull("input")
                .MethodReturn(Unwrap(elementType, Ref("input")));
            
            return LocalMethodBuild(method).Pass(expression).Call();
        }
    }
} // end of namespace