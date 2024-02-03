///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Factory for conversion/coercion and widening implementations for numbers.
    /// </summary>
    public partial class ArrayCoercerFactory
    {
        /// <summary>
        ///     Returns a coercer/widener/narrower for arrays.
        /// </summary>
        /// <param name="fromType">type to widen/narrow from</param>
        /// <param name="resultType">type to widen/narrow to</param>
        /// <returns>widener/narrower</returns>
        public static Coercer GetCoercer(
            Type fromType,
            Type resultType)
        {
            var fromElement = fromType.GetElementType();
            var toElement = resultType.GetElementType();

            if (fromElement == toElement) {
                return new CoercerNull(resultType);
            }

            if (fromElement.GetBoxedType() == toElement) {
                // Widening
                return new CoercerWiden(fromElement);
            }
            else if (fromElement == toElement.GetBoxedType()) {
                // Narrowing
                return new CoercerNarrow(toElement);
            }

            throw new ArgumentException(
                $"Cannot coerce array of type '{fromElement.CleanName()}' to array type '{resultType.CleanName()}'");
        }

        public static Array WidenArray(object source)
        {
            if (source == null) {
                return null;
            }

            if (source is Array sourceArray) {
                var sourceArrayLength = sourceArray.Length;
                var sourceArrayType = sourceArray.GetType().GetElementType();
                var targetArrayType = sourceArrayType.GetBoxedType().MakeArrayType();
                var targetArray = Arrays.CreateInstanceChecked(targetArrayType, sourceArrayLength);
                for (var ii = 0; ii < sourceArrayLength; ii++) {
                    targetArray.SetValue(sourceArray.GetValue(ii), ii);
                }

                return targetArray;
            }

            throw new EPException($"Invalid value presented for \"{nameof(source)}\" for array widening");
        }

        public static Array NarrowArray(object source)
        {
            if (source == null) {
                return null;
            }

            if (source is Array sourceArray) {
                var sourceArrayLength = sourceArray.Length;
                var sourceArrayType = sourceArray.GetType().GetElementType();
                var targetArrayType = sourceArrayType.GetUnboxedType().MakeArrayType();
                var targetArray = Arrays.CreateInstanceChecked(targetArrayType, sourceArrayLength);
                for (var ii = 0; ii < sourceArrayLength; ii++) {
                    var sourceValue = sourceArray.GetValue(ii);
                    if (sourceValue != null) {
                        targetArray.SetValue(sourceValue, ii);
                    }
                }

                return targetArray;
            }

            throw new EPException($"Invalid value presented for \"{nameof(source)}\" for array widening");
        }

        private class CoercerNull : Coercer
        {
            private readonly Type _returnType;

            public CoercerNull(Type returnType)
            {
                _returnType = returnType;
            }

            public object CoerceBoxed(object value)
            {
                return value;
            }

            public Type GetReturnType(Type valueType) => _returnType;

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
            {
                return value;
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return value;
            }
        }

        private class CoercerWiden : Coercer
        {
            private readonly Type _returnType;

            public CoercerWiden(Type returnType)
            {
                _returnType = returnType;
            }

            public object CoerceBoxed(object value)
            {
                return value;
            }

            public Type GetReturnType(Type valueType) => _returnType;

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
            {
                return StaticMethod(typeof(ArrayCoercerFactory), "WidenArray", value);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return StaticMethod(typeof(ArrayCoercerFactory), "WidenArray", value);
            }
        }

        private class CoercerNarrow : Coercer
        {
            private readonly Type _returnType;

            public CoercerNarrow(Type returnType)
            {
                _returnType = returnType;
            }

            public object CoerceBoxed(object value)
            {
                return value;
            }

            public Type GetReturnType(Type valueType) => _returnType;

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
            {
                return StaticMethod(typeof(ArrayCoercerFactory), "NarrowArray", value);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return StaticMethod(typeof(ArrayCoercerFactory), "NarrowArray", value);
            }
        }
    }
} // end of namespace