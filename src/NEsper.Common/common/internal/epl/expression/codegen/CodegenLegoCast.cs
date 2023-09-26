///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
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

            if (value is CodegenExpressionConstantNull) {
                return value;
            }

            if (value is CodegenExpressionConstant codegenExpressionConstant &&
                codegenExpressionConstant.IsNull) {
                return value;
            }

            if (targetType.IsTypeVoid()) {
                throw new ArgumentException("Invalid void target type for cast");
            }

            if (targetType == typeof(int)) {
                return ExprDotMethod(value, "AsInt32");
            }
            else if (targetType == typeof(int?)) {
                return ExprDotMethod(value, "AsBoxedInt32");
            }
            else if (targetType == typeof(long)) {
                return ExprDotMethod(value, "AsInt64");
            }
            else if (targetType == typeof(long?)) {
                return ExprDotMethod(value, "AsBoxedInt64");
            }
            else if (targetType == typeof(short)) {
                return ExprDotMethod(value, "AsInt16");
            }
            else if (targetType == typeof(short?)) {
                return ExprDotMethod(value, "AsBoxedInt16");
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
            else if (targetType == typeof(FlexCollection)) {
                return StaticMethod(typeof(FlexCollection), "Of", value);
            }
            else if (targetType == typeof(IDictionary<string, object>)) {
                return StaticMethod(typeof(CompatExtensions), "AsStringDictionary", value);
            }
            else if (targetType == typeof(IDictionary<object, object>)) {
                return StaticMethod(typeof(CompatExtensions), "AsObjectDictionary", value);
            }
            else if (targetType.IsGenericDictionary()) {
                return Cast(targetType, value);
            }
            else if (targetType.IsArray()) {
                var elementType = targetType.GetElementType();
                return StaticMethod(typeof(CompatExtensions), "UnwrapIntoArray", new[] { elementType }, value);
            }
            else if (targetType.IsGenericList()) {
                var elementType = targetType.GetCollectionItemType();
                return StaticMethod(typeof(CompatExtensions), "UnwrapIntoList", new[] { elementType }, value);
            }
            else if (targetType.IsGenericSet()) {
                var elementType = targetType.GetCollectionItemType();
                return StaticMethod(typeof(CompatExtensions), "UnwrapIntoSet", new[] { elementType }, value);
            }
            else if (targetType == typeof(ICollection<object>)) {
                return StaticMethod(typeof(TypeHelper), "AsObjectCollection", value);
            }
            else if (targetType.IsGenericCollection()) {
                var elementType = targetType.GetCollectionItemType();
                return Unwrap(elementType, value);
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
            if (type == null) {
                block.DeclareVar<double>(variable, Constant(0.0d));
                return;
            }

            if (type == typeof(double)) {
                block.DeclareVar(
                    type,
                    variable,
                    forge.EvaluateCodegen(type, codegenMethodScope, exprSymbol, codegenClassScope));
                return;
            }

            var holder = variable + "_";
            block.DeclareVar(
                type,
                holder,
                forge.EvaluateCodegen(type, codegenMethodScope, exprSymbol, codegenClassScope));
            if (type.CanBeNull()) {
                block.IfRefNullReturnNull(holder);
            }

            block.DeclareVar<double>(
                variable,
                SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(Ref(holder), type));
        }
    }
} // end of namespace