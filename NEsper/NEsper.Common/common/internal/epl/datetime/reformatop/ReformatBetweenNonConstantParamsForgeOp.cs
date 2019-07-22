///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public class ReformatBetweenNonConstantParamsForgeOp : ReformatOp
    {
        private readonly ExprEvaluator endEval;
        private readonly ExprEvaluator evalIncludeHigh;
        private readonly ExprEvaluator evalIncludeLow;
        private readonly ReformatBetweenNonConstantParamsForge forge;
        private readonly ExprEvaluator startEval;

        public ReformatBetweenNonConstantParamsForgeOp(
            ReformatBetweenNonConstantParamsForge forge,
            ExprEvaluator startEval,
            ExprEvaluator endEval,
            ExprEvaluator evalIncludeLow,
            ExprEvaluator evalIncludeHigh)
        {
            this.forge = forge;
            this.startEval = startEval;
            this.endEval = endEval;
            this.evalIncludeLow = evalIncludeLow;
            this.evalIncludeHigh = evalIncludeHigh;
        }

        public object Evaluate(
            long ts,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(ts, eventsPerStream, newData, exprEvaluatorContext);
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

            return EvaluateInternal(
                dateTimeEx.TimeInMillis,
                eventsPerStream,
                newData,
                exprEvaluatorContext);
        }

        public object Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(
                DatetimeLongCoercerDateTimeOffset.CoerceToMillis(dateTimeOffset),
                eventsPerStream,
                newData,
                exprEvaluatorContext);
        }

        public object Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(
                DatetimeLongCoercerDateTime.CoerceToMillis(dateTime),
                eventsPerStream,
                newData,
                exprEvaluatorContext);
        }

        public static CodegenExpression CodegenLong(
            ReformatBetweenNonConstantParamsForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CodegenLongInternal(forge, inner, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public static CodegenExpression CodegenDateTime(
            ReformatBetweenNonConstantParamsForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                    typeof(bool?),
                    typeof(ReformatBetweenNonConstantParamsForgeOp),
                    codegenClassScope)
                .AddParam(typeof(DateTime), "d");

            methodNode.Block
                .IfRefNullReturnNull("d")
                .MethodReturn(
                    CodegenLongInternal(
                        forge,
                        ExprDotMethod(Ref("d"), "getTime"),
                        methodNode,
                        exprSymbol,
                        codegenClassScope));
            return LocalMethod(methodNode, inner);
        }

        public static CodegenExpression CodegenDateTimeOffset(
            ReformatBetweenNonConstantParamsForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                    typeof(bool?),
                    typeof(ReformatBetweenNonConstantParamsForgeOp),
                    codegenClassScope)
                .AddParam(typeof(DateTimeOffset), "d");

            methodNode.Block
                .IfRefNullReturnNull("d")
                .MethodReturn(
                    CodegenLongInternal(
                        forge,
                        ExprDotMethod(Ref("d"), "getTime"),
                        methodNode,
                        exprSymbol,
                        codegenClassScope));
            return LocalMethod(methodNode, inner);
        }

        public static CodegenExpression CodegenDateTimeEx(
            ReformatBetweenNonConstantParamsForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                    typeof(bool?),
                    typeof(ReformatBetweenNonConstantParamsForgeOp),
                    codegenClassScope)
                .AddParam(typeof(DateTimeEx), "dateTime");

            methodNode.Block
                .IfRefNullReturnNull("dateTime")
                .MethodReturn(
                    CodegenLongInternal(
                        forge,
                        ExprDotMethod(Ref("dateTime"), "getTimeInMillis"),
                        methodNode,
                        exprSymbol,
                        codegenClassScope));
            return LocalMethod(methodNode, inner);
        }

        public object EvaluateInternal(
            long ts,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var firstObj = startEval.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
            if (firstObj == null) {
                return null;
            }

            var secondObj = endEval.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
            if (secondObj == null) {
                return null;
            }

            var first = forge.startCoercer.Coerce(firstObj);
            var second = forge.secondCoercer.Coerce(secondObj);
            if (forge.includeBoth) {
                if (first <= second) {
                    return first <= ts && ts <= second;
                }

                return second <= ts && ts <= first;
            }

            bool includeLowEndpoint;
            if (forge.includeLow != null) {
                includeLowEndpoint = forge.includeLow.Value;
            }
            else {
                var value = evalIncludeLow.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
                if (value == null) {
                    return null;
                }

                includeLowEndpoint = (bool) value;
            }

            bool includeHighEndpoint;
            if (forge.includeHigh != null) {
                includeHighEndpoint = forge.includeHigh.Value;
            }
            else {
                var value = evalIncludeHigh.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
                if (value == null) {
                    return null;
                }

                includeHighEndpoint = (bool) value;
            }

            return CompareTimestamps(first, ts, second, includeLowEndpoint, includeHighEndpoint);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="first">first</param>
        /// <param name="ts">ts</param>
        /// <param name="second">second</param>
        /// <param name="includeLowEndpoint">flag</param>
        /// <param name="includeHighEndpoint">flag</param>
        /// <returns>result</returns>
        public static bool CompareTimestamps(
            long first,
            long ts,
            long second,
            bool includeLowEndpoint,
            bool includeHighEndpoint)
        {
            if (includeLowEndpoint) {
                if (ts < first) {
                    return false;
                }
            }
            else {
                if (ts <= first) {
                    return false;
                }
            }

            if (includeHighEndpoint) {
                if (ts > second) {
                    return false;
                }
            }
            else {
                if (ts >= second) {
                    return false;
                }
            }

            return true;
        }

        private static CodegenExpression CodegenLongInternal(
            ReformatBetweenNonConstantParamsForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                    typeof(bool?),
                    typeof(ReformatBetweenNonConstantParamsForgeOp),
                    codegenClassScope)
                .AddParam(typeof(long), "ts");

            var block = methodNode.Block;
            CodegenLongCoercion(
                block,
                "first",
                forge.start,
                forge.startCoercer,
                methodNode,
                exprSymbol,
                codegenClassScope);
            CodegenLongCoercion(
                block,
                "second",
                forge.end,
                forge.secondCoercer,
                methodNode,
                exprSymbol,
                codegenClassScope);
            CodegenExpression first = Ref("first");
            CodegenExpression second = Ref("second");
            CodegenExpression ts = Ref("ts");
            if (forge.includeBoth) {
                block.IfCondition(Relational(first, LE, second))
                    .BlockReturn(And(Relational(first, LE, ts), Relational(ts, LE, second)))
                    .MethodReturn(And(Relational(second, LE, ts), Relational(ts, LE, first)));
            }
            else if (forge.includeLow != null && forge.includeHigh != null) {
                block.IfCondition(Relational(ts, forge.includeLow.Value ? LT : LE, first))
                    .BlockReturn(ConstantFalse())
                    .IfCondition(Relational(ts, forge.includeHigh.Value ? GT : GE, second))
                    .BlockReturn(ConstantFalse())
                    .MethodReturn(ConstantTrue());
            }
            else {
                CodegenBooleanEval(
                    block,
                    "includeLowEndpoint",
                    forge.includeLow.Value,
                    forge.forgeIncludeLow,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                CodegenBooleanEval(
                    block,
                    "includeLowHighpoint",
                    forge.includeHigh.Value,
                    forge.forgeIncludeHigh,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                block.MethodReturn(
                    StaticMethod(
                        typeof(ReformatBetweenNonConstantParamsForgeOp),
                        "compareTimestamps",
                        first,
                        ts,
                        second,
                        Ref("includeLowEndpoint"),
                        Ref("includeLowHighpoint")));
            }

            return LocalMethod(methodNode, inner);
        }

        private static void CodegenBooleanEval(
            CodegenBlock block,
            string variable,
            bool? preset,
            ExprForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (preset != null) {
                block.DeclareVar<bool>(variable, Constant(preset));
                return;
            }

            if (forge.EvaluationType == typeof(bool)) {
                block.DeclareVar<bool>(
                    variable,
                    forge.EvaluateCodegen(typeof(bool), codegenMethodScope, exprSymbol, codegenClassScope));
                return;
            }

            var refname = variable + "Obj";
            block.DeclareVar<bool?>(
                    refname,
                    forge.EvaluateCodegen(typeof(bool?), codegenMethodScope, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull(refname)
                .DeclareVar<bool>(variable, Ref(refname));
        }

        private static void CodegenLongCoercion(
            CodegenBlock block,
            string variable,
            ExprNode assignment,
            DatetimeLongCoercer coercer,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var evaluationType = assignment.Forge.EvaluationType;
            if (evaluationType == typeof(long)) {
                block.DeclareVar<long>(
                    variable,
                    assignment.Forge.EvaluateCodegen(typeof(long), codegenMethodScope, exprSymbol, codegenClassScope));
                return;
            }

            var refname = variable + "Obj";
            block.DeclareVar(
                evaluationType,
                refname,
                assignment.Forge.EvaluateCodegen(evaluationType, codegenMethodScope, exprSymbol, codegenClassScope));
            if (!evaluationType.IsPrimitive) {
                block.IfRefNullReturnNull(refname);
            }

            block.DeclareVar<long>(variable, coercer.Codegen(Ref(refname), evaluationType, codegenClassScope));
        }
    }
} // end of namespace