///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.codegen
{
    public class CodegenLegoCast
    {
        public static CodegenExpression CastSafeFromObjectType(
            Type targetType,
            CodegenExpression value)
        {
            if (targetType == null) {
                return ConstantNull();
            }

            if (targetType == typeof(object)) {
                return value;
            }

            if (targetType == typeof(void)) {
                throw new ArgumentException("Invalid void target type for cast");
            }

            if (targetType == typeof(int)) {
                return ExprDotMethod(value, "AsInt");
            }
            else if (targetType == typeof(int?)) {
                return ExprDotMethod(value, "AsBoxedInt");
            }
            else if (targetType == typeof(long)) {
                return ExprDotMethod(value, "AsLong");
            }
            else if (targetType == typeof(long?)) {
                return ExprDotMethod(value, "AsBoxedLong");
            }
            else if (targetType == typeof(short)) {
                return ExprDotMethod(value, "AsShort");
            }
            else if (targetType == typeof(short?)) {
                return ExprDotMethod(value, "AsBoxedShort");
            }
            else if (targetType == typeof(byte)) {
                return ExprDotMethod(value, "AsByte");
            }
            else if (targetType == typeof(byte?)) {
                return ExprDotMethod(value, "AsBoxedByte");
            }
            else if (targetType == typeof(decimal)) {
                return ExprDotMethod(value, "AsDecimal");
            }
            else if (targetType == typeof(decimal?)) {
                return ExprDotMethod(value, "AsBoxedDecimal");
            }
            else if (targetType == typeof(double)) {
                return ExprDotMethod(value, "AsDouble");
            }
            else if (targetType == typeof(double?)) {
                return ExprDotMethod(value, "AsBoxedDouble");
            }
            else if (targetType == typeof(float)) {
                return ExprDotMethod(value, "AsFloat");
            }
            else if (targetType == typeof(float?)) {
                return ExprDotMethod(value, "AsBoxedFloat");
            }
            else if (targetType == typeof(bool)) {
                return ExprDotMethod(value, "AsBoolean");
            }
            else if (targetType == typeof(bool?)) {
                return ExprDotMethod(value, "AsBoxedBoolean");
            }
            else if (targetType == typeof(DateTime)) {
                return ExprDotMethod(value, "AsDateTime");
            }
            else if (targetType == typeof(DateTime?)) {
                return ExprDotMethod(value, "AsBoxedDateTime");
            }
            else if (targetType == typeof(DateTimeOffset)) {
                return ExprDotMethod(value, "AsDateTimeOffset");
            }
            else if (targetType == typeof(DateTimeOffset?)) {
                return ExprDotMethod(value, "AsBoxedDateTimeOffset");
            }

            return Cast(targetType, value);
        }

        public static void AsDoubleNullReturnNull(
            CodegenBlock block,
            string variable,
            ExprForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var type = forge.EvaluationType;
            if (type == typeof(double)) {
                block.DeclareVar(
                    type,
                    variable,
                    forge.EvaluateCodegen(type, codegenMethodScope, exprSymbol, codegenClassScope));
                return;
            }

            string holder = variable + "_";
            block.DeclareVar(
                type,
                holder,
                forge.EvaluateCodegen(type, codegenMethodScope, exprSymbol, codegenClassScope));
            if (!type.IsPrimitive) {
                block.IfRefNullReturnNull(holder);
            }

            block.DeclareVar<double>(
                variable,
                SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(@Ref(holder), type));
        }
    }
} // end of namespace