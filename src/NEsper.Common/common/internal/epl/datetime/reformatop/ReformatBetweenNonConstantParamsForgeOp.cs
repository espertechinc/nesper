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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public class ReformatBetweenNonConstantParamsForgeOp : ReformatOp
    {
        private readonly ExprEvaluator _endEval;
        private readonly ExprEvaluator _evalIncludeHigh;
        private readonly ExprEvaluator _evalIncludeLow;
        private readonly ReformatBetweenNonConstantParamsForge _forge;
        private readonly ExprEvaluator _startEval;

        public ReformatBetweenNonConstantParamsForgeOp(
            ReformatBetweenNonConstantParamsForge forge,
            ExprEvaluator startEval,
            ExprEvaluator endEval,
            ExprEvaluator evalIncludeLow,
            ExprEvaluator evalIncludeHigh)
        {
            _forge = forge;
            _startEval = startEval;
            _endEval = endEval;
            _evalIncludeLow = evalIncludeLow;
            _evalIncludeHigh = evalIncludeHigh;
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
                dateTimeEx.UtcMillis,
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
                        ExprDotMethod(Ref("d"), "UtcMillis"),
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
                        ExprDotMethod(Ref("d"), "UtcMillis"),
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
                        ExprDotName(Ref("dateTime"), "UtcMillis"),
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
            var firstObj = _startEval.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
            if (firstObj == null) {
                return null;
            }

            var secondObj = _endEval.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
            if (secondObj == null) {
                return null;
            }

            var first = _forge.StartCoercer.Coerce(firstObj);
            var second = _forge.SecondCoercer.Coerce(secondObj);
            if (_forge.IncludeBoth) {
                if (first <= second) {
                    return first <= ts && ts <= second;
                }

                return second <= ts && ts <= first;
            }

            bool includeLowEndpoint;
            if (_forge.IncludeLow != null) {
                includeLowEndpoint = _forge.IncludeLow.Value;
            }
            else {
                var value = _evalIncludeLow.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
                if (value == null) {
                    return null;
                }

                includeLowEndpoint = (bool) value;
            }

            bool includeHighEndpoint;
            if (_forge.IncludeHigh != null) {
                includeHighEndpoint = _forge.IncludeHigh.Value;
            }
            else {
                var value = _evalIncludeHigh.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
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
                forge.Start,
                forge.StartCoercer,
                methodNode,
                exprSymbol,
                codegenClassScope);

            CodegenLongCoercion(
                block,
                "second",
                forge.End,
                forge.SecondCoercer,
                methodNode,
                exprSymbol,
                codegenClassScope);

            CodegenExpression first = Ref("first");
            CodegenExpression second = Ref("second");
            CodegenExpression ts = Ref("ts");

            if (forge.IncludeBoth) {
                block.IfCondition(Relational(first, LE, second))
                    .BlockReturn(And(Relational(first, LE, ts), Relational(ts, LE, second)))
                    .MethodReturn(And(Relational(second, LE, ts), Relational(ts, LE, first)));
            }
            else if (forge.IncludeLow != null && forge.IncludeHigh != null) {
                block.IfCondition(Relational(ts, forge.IncludeLow.Value ? LT : LE, first))
                    .BlockReturn(ConstantFalse())
                    .IfCondition(Relational(ts, forge.IncludeHigh.Value ? GT : GE, second))
                    .BlockReturn(ConstantFalse())
                    .MethodReturn(ConstantTrue());
            }
            else {
                CodegenBooleanEval(
                    block,
                    "includeLowEndpoint",
                    forge.IncludeLow,
                    forge.ForgeIncludeLow,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);

                CodegenBooleanEval(
                    block,
                    "includeLowHighpoint",
                    forge.IncludeHigh,
                    forge.ForgeIncludeHigh,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);

                block.MethodReturn(
                    StaticMethod(
                        typeof(ReformatBetweenNonConstantParamsForgeOp),
                        "CompareTimestamps",
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
                block.DeclareVar<bool?>(variable, Constant(preset));
                return;
            }

            if (forge.EvaluationType == typeof(bool)) {
                block.DeclareVar<bool?>(
                    variable,
                    forge.EvaluateCodegen(typeof(bool), codegenMethodScope, exprSymbol, codegenClassScope));
                return;
            }

            var refname = variable + "Obj";
            block
                .DeclareVar<bool?>(refname, forge.EvaluateCodegen(typeof(bool), codegenMethodScope, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull(refname)
                .DeclareVar<bool>(variable, Unbox<bool?>(Ref(refname)));
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
            if (evaluationType.CanBeNull()) {
                block.IfRefNullReturnNull(refname);
            }

            block.DeclareVar<long>(variable, coercer.Codegen(Ref(refname), evaluationType, codegenClassScope));

            //block.Debug("CodegenLongCoercion: {0} => {1}", Ref(refname), Ref(variable));
        }
    }
} // end of namespace