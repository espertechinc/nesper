///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Numerics;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Factory for conversion/coercion and widening implementations for numbers.
    /// </summary>
    public partial class SimpleNumberCoercerFactory
    {
        /// <summary>
        ///     Returns a coercer/widener to BigInteger for a given type.
        /// </summary>
        /// <param name="fromType">to widen</param>
        /// <returns>widener</returns>
        public static BigIntegerCoercer GetCoercerBigInteger(Type fromType)
        {
            if (fromType == typeof(BigInteger)) {
                return CoercerBigIntNull.INSTANCE;
            }

            return CoercerBigInt.INSTANCE;
        }

        /// <summary>
        ///     Returns a coercer/widener/narrower to a result number type from a given type.
        /// </summary>
        /// <param name="fromType">to widen/narrow, can be null to indicate that no shortcut-coercer is used</param>
        /// <param name="resultBoxedType">type to widen/narrow to</param>
        /// <returns>widener/narrower</returns>
        public static SimpleNumberCoercer GetCoercer(
            Type fromType,
            Type resultBoxedType)
        {
            var trueBoxedType = resultBoxedType.GetBoxedType();
            if (trueBoxedType != resultBoxedType) {
                Debug.Assert(trueBoxedType != resultBoxedType);
            }

            if (fromType == resultBoxedType) {
                return CoercerNull.INSTANCE;
            }

            if (resultBoxedType == typeof(decimal?)) {
                return CoercerDecimal.INSTANCE;
            }

            if (resultBoxedType == typeof(double?)) {
                return CoercerDouble.INSTANCE;
            }

            if (resultBoxedType == typeof(long?)) {
                return CoercerLong.INSTANCE;
            }

            if (resultBoxedType == typeof(float?)) {
                return CoercerFloat.INSTANCE;
            }

            if (resultBoxedType == typeof(int?)) {
                return CoercerInt.INSTANCE;
            }

            if (resultBoxedType == typeof(short?)) {
                return CoercerShort.INSTANCE;
            }

            if (resultBoxedType == typeof(byte?)) {
                return CoercerByte.INSTANCE;
            }

            if (resultBoxedType == typeof(BigInteger)) {
                return CoercerBigInt.INSTANCE;
            }

            if (resultBoxedType == typeof(object)) {
                return CoercerNull.INSTANCE;
            }

            throw new ArgumentException("Cannot coerce to number subtype " + resultBoxedType.Name);
        }

        private static CodegenExpression CodegenCoerceNonNull(
            Type primitive,
            Type boxed,
            string numberValueMethodName,
            CodegenExpression param,
            Type type)
        {
            if (type == primitive) {
                return param;
            }
            if (type == boxed) {
                return ExprDotName(param, "Value");
            }
            if (type.IsPrimitive) {
                return Cast(primitive, param);
            }

            return ExprDotMethod(param, numberValueMethodName);
        }

        private static CodegenExpression CodegenCoerceMayNull(
            Type primitive,
            Type boxed,
            string numberValueMethodName,
            CodegenExpression param,
            Type type,
            CodegenMethodScope codegenMethodScope,
            Type generator,
            CodegenClassScope codegenClassScope)
        {
            if (type == primitive || type == boxed) {
                return param;
            }

            if (type == null) {
                return ConstantNull();
            }

            if (type.IsPrimitive) {
                return Cast(primitive, param);
            }

            var method = codegenMethodScope
                .MakeChild(boxed, generator, codegenClassScope)
                .AddParam(type, "value")
                .Block
                .IfRefNullReturnNull("value")
                .MethodReturn(ExprDotMethod(Ref("value"), numberValueMethodName));
            return LocalMethod(method, param);
        }
    }
} // end of namespace