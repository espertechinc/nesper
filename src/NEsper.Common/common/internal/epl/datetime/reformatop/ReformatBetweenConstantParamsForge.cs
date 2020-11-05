///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public class ReformatBetweenConstantParamsForge : ReformatForge,
        ReformatOp
    {
        private readonly long _first;
        private readonly long _second;

        public ReformatBetweenConstantParamsForge(IList<ExprNode> parameters)
        {
            var paramFirst = GetLongValue(parameters[0]);
            var paramSecond = GetLongValue(parameters[1]);

            if (paramFirst > paramSecond) {
                _second = paramFirst;
                _first = paramSecond;
            }
            else {
                _first = paramFirst;
                _second = paramSecond;
            }

            if (parameters.Count > 2) {
                if (!GetBooleanValue(parameters[2])) {
                    _first++;
                }

                if (!GetBooleanValue(parameters[3])) {
                    _second--;
                }
            }
        }

        public ReformatOp Op => this;

        public CodegenExpression CodegenDateTime(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CodegenLong(
                ExprDotMethod(inner, "UtcMillis"),
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CodegenLong(
                ExprDotMethod(inner, "UtcMillis"),
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CodegenLong(
                ExprDotMethod(inner, "UtcMillis"),
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression CodegenLong(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return And(Relational(Constant(_first), LE, inner), Relational(inner, LE, Constant(_second)));
        }

        public Type ReturnType => typeof(bool?);

        public FilterExprAnalyzerAffector GetFilterDesc(
            EventType[] typesPerStream,
            DatetimeMethodDesc currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            return null;
        }

        public object Evaluate(
            long ts,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(ts);
        }

        public object Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (dateTimeEx == null) {
                return null;
            }

            return EvaluateInternal(dateTimeEx.UtcMillis);
        }

        public object Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(DatetimeLongCoercerDateTimeOffset.CoerceToMillis(dateTimeOffset));
        }

        public object Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(DatetimeLongCoercerDateTime.CoerceToMillis(dateTime));
        }

        private long GetLongValue(ExprNode exprNode)
        {
            var value = exprNode.Forge.ExprEvaluator.Evaluate(null, true, null);
            if (value == null) {
                throw new ExprValidationException("Date-time method 'between' requires non-null parameter values");
            }

            return DatetimeLongCoercerFactory
                .GetCoercer(value.GetType())
                .Coerce(value);
        }

        private bool GetBooleanValue(ExprNode exprNode)
        {
            var value = exprNode.Forge.ExprEvaluator.Evaluate(null, true, null);
            if (value == null) {
                throw new ExprValidationException("Date-time method 'between' requires non-null parameter values");
            }

            return (bool) value;
        }

        public object EvaluateInternal(long ts)
        {
            return _first <= ts && ts <= _second;
        }
    }
} // end of namespace