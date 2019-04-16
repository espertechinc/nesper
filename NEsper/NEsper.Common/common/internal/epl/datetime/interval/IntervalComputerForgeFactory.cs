///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.datetime.interval.deltaexpr;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public class IntervalComputerForgeFactory
    {
        public static IntervalComputerForge Make(
            DateTimeMethodEnum method,
            IList<ExprNode> expressions,
            TimeAbacus timeAbacus)
        {
            var parameters = GetParameters(expressions, timeAbacus);

            if (method == DateTimeMethodEnum.BEFORE) {
                if (parameters.Length == 0) {
                    return new IntervalComputerBeforeNoParamForge();
                }

                IntervalStartEndParameterPairForge pair =
                    IntervalStartEndParameterPairForge.FromParamsWithLongMaxEnd(parameters);
                if (pair.IsConstant) {
                    return new IntervalComputerConstantBefore(pair);
                }

                return new IntervalComputerBeforeWithDeltaExprForge(pair);
            }

            if (method == DateTimeMethodEnum.AFTER) {
                if (parameters.Length == 0) {
                    return new IntervalComputerAfterNoParam();
                }

                IntervalStartEndParameterPairForge pair =
                    IntervalStartEndParameterPairForge.FromParamsWithLongMaxEnd(parameters);
                if (pair.IsConstant) {
                    return new IntervalComputerConstantAfter(pair);
                }

                return new IntervalComputerAfterWithDeltaExprForge(pair);
            }

            if (method == DateTimeMethodEnum.COINCIDES) {
                if (parameters.Length == 0) {
                    return new IntervalComputerCoincidesNoParam();
                }

                IntervalStartEndParameterPairForge pair =
                    IntervalStartEndParameterPairForge.FromParamsWithSameEnd(parameters);
                if (pair.IsConstant) {
                    return new IntervalComputerConstantCoincides(pair);
                }

                return new IntervalComputerCoincidesWithDeltaExprForge(pair);
            }

            if (method == DateTimeMethodEnum.DURING || method == DateTimeMethodEnum.INCLUDES) {
                if (parameters.Length == 0) {
                    if (method == DateTimeMethodEnum.DURING) {
                        return new IntervalComputerDuringNoParam();
                    }

                    return new IntervalComputerIncludesNoParam();
                }

                IntervalStartEndParameterPairForge pair =
                    IntervalStartEndParameterPairForge.FromParamsWithSameEnd(parameters);
                if (parameters.Length == 1) {
                    return new IntervalComputerDuringAndIncludesThresholdForge(
                        method == DateTimeMethodEnum.DURING, pair.Start.Forge);
                }

                if (parameters.Length == 2) {
                    return new IntervalComputerDuringAndIncludesMinMax(
                        method == DateTimeMethodEnum.DURING, pair.Start.Forge, pair.End.Forge);
                }

                return new IntervalComputerDuringMinMaxStartEndForge(
                    method == DateTimeMethodEnum.DURING, GetEvaluators(expressions, timeAbacus));
            }

            if (method == DateTimeMethodEnum.FINISHES) {
                if (parameters.Length == 0) {
                    return new IntervalComputerFinishesNoParam();
                }

                ValidateConstantThreshold("finishes", parameters[0]);
                return new IntervalComputerFinishesThresholdForge(parameters[0].Forge);
            }

            if (method == DateTimeMethodEnum.FINISHEDBY) {
                if (parameters.Length == 0) {
                    return new IntervalComputerFinishedByNoParam();
                }

                ValidateConstantThreshold("finishedby", parameters[0]);
                return new IntervalComputerFinishedByThresholdForge(parameters[0].Forge);
            }

            if (method == DateTimeMethodEnum.MEETS) {
                if (parameters.Length == 0) {
                    return new IntervalComputerMeetsNoParam();
                }

                ValidateConstantThreshold("meets", parameters[0]);
                return new IntervalComputerMeetsThresholdForge(parameters[0].Forge);
            }

            if (method == DateTimeMethodEnum.METBY) {
                if (parameters.Length == 0) {
                    return new IntervalComputerMetByNoParam();
                }

                ValidateConstantThreshold("metBy", parameters[0]);
                return new IntervalComputerMetByThresholdForge(parameters[0].Forge);
            }

            if (method == DateTimeMethodEnum.OVERLAPS || method == DateTimeMethodEnum.OVERLAPPEDBY) {
                if (parameters.Length == 0) {
                    if (method == DateTimeMethodEnum.OVERLAPS) {
                        return new IntervalComputerOverlapsNoParam();
                    }

                    return new IntervalComputerOverlappedByNoParam();
                }

                if (parameters.Length == 1) {
                    return new IntervalComputerOverlapsAndByThreshold(
                        method == DateTimeMethodEnum.OVERLAPS, parameters[0].Forge);
                }

                return new IntervalComputerOverlapsAndByMinMaxForge(
                    method == DateTimeMethodEnum.OVERLAPS, parameters[0].Forge, parameters[1].Forge);
            }

            if (method == DateTimeMethodEnum.STARTS) {
                if (parameters.Length == 0) {
                    return new IntervalComputerStartsNoParam();
                }

                ValidateConstantThreshold("starts", parameters[0]);
                return new IntervalComputerStartsThresholdForge(parameters[0].Forge);
            }

            if (method == DateTimeMethodEnum.STARTEDBY) {
                if (parameters.Length == 0) {
                    return new IntervalComputerStartedByNoParam();
                }

                ValidateConstantThreshold("startedBy", parameters[0]);
                return new IntervalComputerStartedByThresholdForge(parameters[0].Forge);
            }

            throw new ArgumentException("Unknown datetime method '" + method + "'");
        }

        private static void ValidateConstantThreshold(
            string method,
            ExprOptionalConstantForge param)
        {
            if (param.OptionalConstant != null && param.OptionalConstant.AsLong() < 0) {
                throw new ExprValidationException(
                    "The " + method + " date-time method does not allow negative threshold value");
            }
        }

        private static ExprOptionalConstantForge[] GetParameters(
            IList<ExprNode> expressions,
            TimeAbacus timeAbacus)
        {
            var parameters = new ExprOptionalConstantForge[expressions.Count - 1];
            for (var i = 1; i < expressions.Count; i++) {
                parameters[i - 1] = GetExprOrConstant(expressions[i], timeAbacus);
            }

            return parameters;
        }

        private static IntervalDeltaExprForge[] GetEvaluators(
            IList<ExprNode> expressions,
            TimeAbacus timeAbacus)
        {
            var parameters = new IntervalDeltaExprForge[expressions.Count - 1];
            for (var i = 1; i < expressions.Count; i++) {
                parameters[i - 1] = GetExprOrConstant(expressions[i], timeAbacus).Forge;
            }

            return parameters;
        }

        private static ExprOptionalConstantForge GetExprOrConstant(
            ExprNode exprNode,
            TimeAbacus timeAbacus)
        {
            if (exprNode is ExprTimePeriod) {
                var timePeriod = (ExprTimePeriod) exprNode;
                if (!timePeriod.HasMonth && !timePeriod.HasYear) {
                    // no-month and constant
                    if (exprNode.Forge.ForgeConstantType.IsCompileTimeConstant()) {
                        double sec = timePeriod.EvaluateAsSeconds(null, true, null);
                        var l = timeAbacus.DeltaForSecondsDouble(sec);
                        return new ExprOptionalConstantForge(new IntervalDeltaExprMSecConstForge(l), l);
                    }

                    return new ExprOptionalConstantForge(
                        new IntervalDeltaExprTimePeriodNonConstForge(timePeriod, timeAbacus), null);
                }

                // has-month and not constant
                TimePeriodCompute timePeriodCompute = timePeriod.TimePeriodComputeForge.Evaluator;
                IntervalDeltaExprForge forge = new ProxyIntervalDeltaExprForge {
                    ProcMakeEvaluator = () => {
                        return new ProxyIntervalDeltaExprEvaluator {
                            ProcEvaluate = (
                                    reference,
                                    eventsPerStream,
                                    isNewData,
                                    context) =>
                                timePeriodCompute.DeltaAdd(
                                    reference, eventsPerStream, isNewData, context)
                        };
                    },

                    ProcCodegen = (
                        reference,
                        parent,
                        exprSymbol,
                        codegenClassScope) => {
                        CodegenExpressionField field = codegenClassScope.AddFieldUnshared(
                            true, typeof(TimePeriodCompute),
                            timePeriod.TimePeriodComputeForge.MakeEvaluator(
                                codegenClassScope.PackageScope.InitMethod, codegenClassScope));
                        return ExprDotMethod(
                            field, "deltaAdd", reference, exprSymbol.GetAddEPS(parent),
                            exprSymbol.GetAddIsNewData(parent), exprSymbol.GetAddExprEvalCtx(parent));
                    }
                };
                return new ExprOptionalConstantForge(forge, null);
            }

            if (ExprNodeUtilityQuery.IsConstant(exprNode)) {
                var constantNode = (ExprConstantNode) exprNode;
                long l = constantNode.ConstantValue.AsLong();
                return new ExprOptionalConstantForge(new IntervalDeltaExprMSecConstForge(l), l);
            }

            {
                var forge = exprNode.Forge;
                IntervalDeltaExprForge eval = new ProxyIntervalDeltaExprForge {
                    ProcMakeEvaluator = () => {
                        var evaluator = forge.ExprEvaluator;
                        return new ProxyIntervalDeltaExprEvaluator {
                            ProcEvaluate = (
                                    reference,
                                    eventsPerStream,
                                    isNewData,
                                    context) =>
                                evaluator.Evaluate(
                                    eventsPerStream, isNewData, context).AsLong()
                        };
                    },

                    ProcCodegen = (
                        reference,
                        codegenMethodScope,
                        exprSymbol,
                        codegenClassScope) => {
                        return SimpleNumberCoercerFactory.SimpleNumberCoercerLong.CodegenLong(
                            forge.EvaluateCodegen(
                                forge.EvaluationType, codegenMethodScope, exprSymbol, codegenClassScope),
                            forge.EvaluationType);
                    }
                };
                return new ExprOptionalConstantForge(eval, null);
            }
        }

        /// <summary>
        ///     After.
        /// </summary>
        public class IntervalComputerConstantAfter : IntervalComputerConstantBase,
            IntervalComputerForge,
            IntervalComputerEval
        {
            public IntervalComputerConstantAfter(IntervalStartEndParameterPairForge pair)
                : base(pair, true)
            {
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return StaticMethod(
                    typeof(IntervalComputerConstantAfter), "computeIntervalAfter", leftStart, rightEnd,
                    Constant(start),
                    Constant(end));
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return ComputeIntervalAfter(leftStart, rightEnd, Start, End);
            }

            public static bool ComputeIntervalAfter(
                long leftStart,
                long rightEnd,
                long start,
                long end)
            {
                var delta = leftStart - rightEnd;
                return start <= delta && delta <= end;
            }
        }

        public class IntervalComputerAfterWithDeltaExprForge : IntervalComputerForge
        {
            internal readonly IntervalDeltaExprForge finish;
            internal readonly IntervalDeltaExprForge start;

            public IntervalComputerAfterWithDeltaExprForge(IntervalStartEndParameterPairForge pair)
            {
                start = pair.Start.Forge;
                finish = pair.End.Forge;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerAfterWithDeltaExprEval(start.MakeEvaluator(), finish.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerAfterWithDeltaExprEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerAfterWithDeltaExprEval : IntervalComputerEval
        {
            private readonly IntervalDeltaExprEvaluator finish;
            private readonly IntervalDeltaExprEvaluator start;

            public IntervalComputerAfterWithDeltaExprEval(
                IntervalDeltaExprEvaluator start,
                IntervalDeltaExprEvaluator finish)
            {
                this.start = start;
                this.finish = finish;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                long rangeStartDelta = start.Evaluate(rightStart, eventsPerStream, newData, context);
                long rangeEndDelta = finish.Evaluate(rightStart, eventsPerStream, newData, context);
                if (rangeStartDelta > rangeEndDelta) {
                    return IntervalComputerConstantAfter.ComputeIntervalAfter(
                        leftStart, rightEnd, rangeEndDelta, rangeStartDelta);
                }

                return IntervalComputerConstantAfter.ComputeIntervalAfter(
                    leftStart, rightEnd, rangeStartDelta, rangeEndDelta);
            }

            public static CodegenExpression Codegen(
                IntervalComputerAfterWithDeltaExprForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(bool), typeof(IntervalComputerAfterWithDeltaExprEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar(
                        typeof(long), "rangeStartDelta",
                        forge.start.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTSTART, methodNode, exprSymbol, codegenClassScope))
                    .DeclareVar(
                        typeof(long), "rangeEndDelta",
                        forge.finish.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTSTART, methodNode, exprSymbol, codegenClassScope));
                block.IfCondition(Relational(Ref("rangeStartDelta"), GT, Ref("rangeEndDelta")))
                    .BlockReturn(
                        StaticMethod(
                            typeof(IntervalComputerConstantAfter), "computeIntervalAfter",
                            IntervalForgeCodegenNames.REF_LEFTSTART, IntervalForgeCodegenNames.REF_RIGHTEND,
                            Ref("rangeEndDelta"), Ref("rangeStartDelta")));
                block.MethodReturn(
                    StaticMethod(
                        typeof(IntervalComputerConstantAfter), "computeIntervalAfter",
                        IntervalForgeCodegenNames.REF_LEFTSTART, IntervalForgeCodegenNames.REF_RIGHTEND,
                        Ref("rangeStartDelta"), Ref("rangeEndDelta")));
                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }

        public class IntervalComputerAfterNoParam : IntervalComputerForge,
            IntervalComputerEval
        {
            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return leftStart > rightEnd;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return Relational(leftStart, GT, rightEnd);
            }
        }

        /// <summary>
        ///     Before.
        /// </summary>
        public class IntervalComputerConstantBefore : IntervalComputerConstantBase,
            IntervalComputerForge,
            IntervalComputerEval
        {
            public IntervalComputerConstantBefore(IntervalStartEndParameterPairForge pair)
                : base(pair, true)
            {
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return StaticMethod(
                    typeof(IntervalComputerConstantBefore), "computeIntervalBefore", leftEnd, rightStart,
                    Constant(start), Constant(end));
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return ComputeIntervalBefore(leftEnd, rightStart, start, end);
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            /// <param name="leftEnd">left end</param>
            /// <param name="right">right</param>
            /// <param name="start">start</param>
            /// <param name="end">end</param>
            /// <returns>flag</returns>
            public static bool ComputeIntervalBefore(
                long leftEnd,
                long right,
                long start,
                long end)
            {
                var delta = right - leftEnd;
                return start <= delta && delta <= end;
            }
        }

        public class IntervalComputerBeforeWithDeltaExprForge : IntervalComputerForge
        {
            internal readonly IntervalDeltaExprForge finish;
            internal readonly IntervalDeltaExprForge start;

            public IntervalComputerBeforeWithDeltaExprForge(IntervalStartEndParameterPairForge pair)
            {
                start = pair.Start.Forge;
                finish = pair.End.Forge;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerBeforeWithDeltaExprEval(start.MakeEvaluator(), finish.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerBeforeWithDeltaExprEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerBeforeWithDeltaExprEval : IntervalComputerEval
        {
            private readonly IntervalDeltaExprEvaluator finish;

            private readonly IntervalDeltaExprEvaluator start;

            public IntervalComputerBeforeWithDeltaExprEval(
                IntervalDeltaExprEvaluator start,
                IntervalDeltaExprEvaluator finish)
            {
                this.start = start;
                this.finish = finish;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                long rangeStartDelta = start.Evaluate(leftEnd, eventsPerStream, newData, context);
                long rangeEndDelta = finish.Evaluate(leftEnd, eventsPerStream, newData, context);
                if (rangeStartDelta > rangeEndDelta) {
                    return IntervalComputerConstantBefore.ComputeIntervalBefore(
                        leftEnd, rightStart, rangeEndDelta, rangeStartDelta);
                }

                return IntervalComputerConstantBefore.ComputeIntervalBefore(
                    leftEnd, rightStart, rangeStartDelta, rangeEndDelta);
            }

            public static CodegenExpression Codegen(
                IntervalComputerBeforeWithDeltaExprForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool), typeof(IntervalComputerBeforeWithDeltaExprEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar(
                        typeof(long), "rangeStartDelta",
                        forge.start.Codegen(
                            IntervalForgeCodegenNames.REF_LEFTEND, methodNode, exprSymbol, codegenClassScope))
                    .DeclareVar(
                        typeof(long), "rangeEndDelta",
                        forge.finish.Codegen(
                            IntervalForgeCodegenNames.REF_LEFTEND, methodNode, exprSymbol, codegenClassScope));
                block.IfCondition(Relational(Ref("rangeStartDelta"), GT, Ref("rangeEndDelta")))
                    .BlockReturn(
                        StaticMethod(
                            typeof(IntervalComputerConstantBefore), "computeIntervalBefore",
                            IntervalForgeCodegenNames.REF_LEFTEND, IntervalForgeCodegenNames.REF_RIGHTSTART,
                            Ref("rangeEndDelta"), Ref("rangeStartDelta")));
                block.MethodReturn(
                    StaticMethod(
                        typeof(IntervalComputerConstantBefore), "computeIntervalBefore",
                        IntervalForgeCodegenNames.REF_LEFTEND, IntervalForgeCodegenNames.REF_RIGHTSTART,
                        Ref("rangeStartDelta"), Ref("rangeEndDelta")));
                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }

        public class IntervalComputerBeforeNoParamForge : IntervalComputerForge,
            IntervalComputerEval
        {
            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return Relational(leftEnd, LT, rightStart);
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return leftEnd < rightStart;
            }
        }

        /// <summary>
        ///     Coincides.
        /// </summary>
        public class IntervalComputerConstantCoincides : IntervalComputerForge,
            IntervalComputerEval
        {
            internal readonly long end;
            internal readonly long start;

            public IntervalComputerConstantCoincides(IntervalStartEndParameterPairForge pair)
            {
                start = pair.Start.OptionalConstant.GetValueOrDefault();
                end = pair.End.OptionalConstant.GetValueOrDefault();
                if (start < 0 || end < 0) {
                    throw new ExprValidationException(
                        "The coincides date-time method does not allow negative start and end values");
                }
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return StaticMethod(
                    typeof(IntervalComputerConstantCoincides), "computeIntervalCoincides", leftStart, leftEnd,
                    rightStart, rightEnd, Constant(start), Constant(end));
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return ComputeIntervalCoincides(leftStart, leftEnd, rightStart, rightEnd, start, end);
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            /// <param name="left">left start</param>
            /// <param name="leftEnd">left end</param>
            /// <param name="right">right start</param>
            /// <param name="rightEnd">right end</param>
            /// <param name="startThreshold">start th</param>
            /// <param name="endThreshold">end th</param>
            /// <returns>flag</returns>
            public static bool ComputeIntervalCoincides(
                long left,
                long leftEnd,
                long right,
                long rightEnd,
                long startThreshold,
                long endThreshold)
            {
                return Math.Abs(left - right) <= startThreshold &&
                       Math.Abs(leftEnd - rightEnd) <= endThreshold;
            }
        }

        public class IntervalComputerCoincidesWithDeltaExprForge : IntervalComputerForge
        {
            internal readonly IntervalDeltaExprForge finish;
            internal readonly IntervalDeltaExprForge start;

            public IntervalComputerCoincidesWithDeltaExprForge(IntervalStartEndParameterPairForge pair)
            {
                start = pair.Start.Forge;
                finish = pair.End.Forge;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerCoincidesWithDeltaExprEval(start.MakeEvaluator(), finish.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerCoincidesWithDeltaExprEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerCoincidesWithDeltaExprEval : IntervalComputerEval
        {
            public const string METHOD_WARNCOINCIDESTARTENDLESSZERO = "warnCoincideStartEndLessZero";

            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            private readonly IntervalDeltaExprEvaluator finish;

            private readonly IntervalDeltaExprEvaluator start;

            public IntervalComputerCoincidesWithDeltaExprEval(
                IntervalDeltaExprEvaluator start,
                IntervalDeltaExprEvaluator finish)
            {
                this.start = start;
                this.finish = finish;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                long startValue = start.Evaluate(Math.Min(leftStart, rightStart), eventsPerStream, newData, context);
                long endValue = finish.Evaluate(Math.Min(leftEnd, rightEnd), eventsPerStream, newData, context);

                if (startValue < 0 || endValue < 0) {
                    Log.Warn("The coincides date-time method does not allow negative start and end values");
                    return null;
                }

                return IntervalComputerConstantCoincides.ComputeIntervalCoincides(
                    leftStart, leftEnd, rightStart, rightEnd, startValue, endValue);
            }

            public static CodegenExpression Codegen(
                IntervalComputerCoincidesWithDeltaExprForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool?), typeof(IntervalComputerCoincidesWithDeltaExprEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar(
                        typeof(long), "startValue",
                        forge.start.Codegen(
                            StaticMethod(
                                typeof(Math), "min", IntervalForgeCodegenNames.REF_LEFTSTART,
                                IntervalForgeCodegenNames.REF_RIGHTSTART), methodNode, exprSymbol, codegenClassScope))
                    .DeclareVar(
                        typeof(long), "endValue",
                        forge.finish.Codegen(
                            StaticMethod(
                                typeof(Math), "min", IntervalForgeCodegenNames.REF_LEFTEND,
                                IntervalForgeCodegenNames.REF_RIGHTEND), methodNode, exprSymbol, codegenClassScope));
                block.IfCondition(
                        Or(
                            Relational(Ref("startValue"), LT, Constant(0)),
                            Relational(Ref("endValue"), LT, Constant(0))))
                    .StaticMethod(
                        typeof(IntervalComputerCoincidesWithDeltaExprEval), METHOD_WARNCOINCIDESTARTENDLESSZERO)
                    .BlockReturn(ConstantNull());
                block.MethodReturn(
                    StaticMethod(
                        typeof(IntervalComputerConstantCoincides), "computeIntervalCoincides",
                        IntervalForgeCodegenNames.REF_LEFTSTART, IntervalForgeCodegenNames.REF_LEFTEND,
                        IntervalForgeCodegenNames.REF_RIGHTSTART, IntervalForgeCodegenNames.REF_RIGHTEND,
                        Ref("startValue"), Ref("endValue")));
                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            public static void WarnCoincideStartEndLessZero()
            {
                Log.Warn("The coincides date-time method does not allow negative start and end values");
            }
        }

        public class IntervalComputerCoincidesNoParam : IntervalComputerForge,
            IntervalComputerEval
        {
            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return And(EqualsIdentity(leftStart, rightStart), EqualsIdentity(leftEnd, rightEnd));
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return leftStart == rightStart && leftEnd == rightEnd;
            }
        }

        /// <summary>
        ///     During And Includes.
        /// </summary>
        public class IntervalComputerDuringNoParam : IntervalComputerForge,
            IntervalComputerEval
        {
            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return rightStart < leftStart && leftEnd < rightEnd;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return And(Relational(rightStart, LT, leftStart), Relational(leftEnd, LT, rightEnd));
            }
        }

        public class IntervalComputerIncludesNoParam : IntervalComputerForge,
            IntervalComputerEval
        {
            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return leftStart < rightStart && rightEnd < leftEnd;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return And(Relational(leftStart, LT, rightStart), Relational(rightEnd, LT, leftEnd));
            }
        }

        public class IntervalComputerDuringAndIncludesThresholdForge : IntervalComputerForge
        {
            internal readonly bool during;
            internal readonly IntervalDeltaExprForge threshold;

            public IntervalComputerDuringAndIncludesThresholdForge(
                bool during,
                IntervalDeltaExprForge threshold)
            {
                this.during = during;
                this.threshold = threshold;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerDuringAndIncludesThresholdEval(during, threshold.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerDuringAndIncludesThresholdEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerDuringAndIncludesThresholdEval : IntervalComputerEval
        {
            internal readonly bool during;
            internal readonly IntervalDeltaExprEvaluator threshold;

            public IntervalComputerDuringAndIncludesThresholdEval(
                bool during,
                IntervalDeltaExprEvaluator threshold)
            {
                this.during = during;
                this.threshold = threshold;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                long thresholdValue = threshold.Evaluate(leftStart, eventsPerStream, newData, context);

                if (during) {
                    var deltaStart = leftStart - rightStart;
                    if (deltaStart <= 0 || deltaStart > thresholdValue) {
                        return false;
                    }

                    var deltaEnd = rightEnd - leftEnd;
                    return !(deltaEnd <= 0 || deltaEnd > thresholdValue);
                }
                else {
                    var deltaStart = rightStart - leftStart;
                    if (deltaStart <= 0 || deltaStart > thresholdValue) {
                        return false;
                    }

                    var deltaEnd = leftEnd - rightEnd;
                    return !(deltaEnd <= 0 || deltaEnd > thresholdValue);
                }
            }

            public static CodegenExpression Codegen(
                IntervalComputerDuringAndIncludesThresholdForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool), typeof(IntervalComputerDuringAndIncludesThresholdEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar(
                        typeof(long), "thresholdValue",
                        forge.threshold.Codegen(
                            IntervalForgeCodegenNames.REF_LEFTSTART, methodNode, exprSymbol, codegenClassScope));

                if (forge.during) {
                    block.DeclareVar(
                            typeof(long), "deltaStart",
                            Op(IntervalForgeCodegenNames.REF_LEFTSTART, "-", IntervalForgeCodegenNames.REF_RIGHTSTART))
                        .IfConditionReturnConst(
                            Or(
                                Relational(Ref("deltaStart"), LE, Constant(0)),
                                Relational(Ref("deltaStart"), GT, Ref("thresholdValue"))), false)
                        .DeclareVar(
                            typeof(long), "deltaEnd",
                            Op(IntervalForgeCodegenNames.REF_RIGHTEND, "-", IntervalForgeCodegenNames.REF_LEFTEND))
                        .MethodReturn(
                            Not(
                                Or(
                                    Relational(Ref("deltaEnd"), LE, Constant(0)),
                                    Relational(Ref("deltaEnd"), GT, Ref("thresholdValue")))));
                }
                else {
                    block.DeclareVar(
                            typeof(long), "deltaStart",
                            Op(IntervalForgeCodegenNames.REF_RIGHTSTART, "-", IntervalForgeCodegenNames.REF_LEFTSTART))
                        .IfConditionReturnConst(
                            Or(
                                Relational(Ref("deltaStart"), LE, Constant(0)),
                                Relational(Ref("deltaStart"), GT, Ref("thresholdValue"))), false)
                        .DeclareVar(
                            typeof(long), "deltaEnd",
                            Op(IntervalForgeCodegenNames.REF_LEFTEND, "-", IntervalForgeCodegenNames.REF_RIGHTEND))
                        .MethodReturn(
                            Not(
                                Or(
                                    Relational(Ref("deltaEnd"), LE, Constant(0)),
                                    Relational(Ref("deltaEnd"), GT, Ref("thresholdValue")))));
                }

                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }

        public class IntervalComputerDuringAndIncludesMinMax : IntervalComputerForge
        {
            internal readonly bool during;
            internal readonly IntervalDeltaExprForge maxEval;
            internal readonly IntervalDeltaExprForge minEval;

            public IntervalComputerDuringAndIncludesMinMax(
                bool during,
                IntervalDeltaExprForge minEval,
                IntervalDeltaExprForge maxEval)
            {
                this.during = during;
                this.minEval = minEval;
                this.maxEval = maxEval;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerDuringAndIncludesMinMaxEval(
                    during, minEval.MakeEvaluator(), maxEval.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerDuringAndIncludesMinMaxEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerDuringAndIncludesMinMaxEval : IntervalComputerEval
        {
            internal readonly bool during;
            internal readonly IntervalDeltaExprEvaluator maxEval;
            internal readonly IntervalDeltaExprEvaluator minEval;

            public IntervalComputerDuringAndIncludesMinMaxEval(
                bool during,
                IntervalDeltaExprEvaluator minEval,
                IntervalDeltaExprEvaluator maxEval)
            {
                this.during = during;
                this.minEval = minEval;
                this.maxEval = maxEval;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                long min = minEval.Evaluate(leftStart, eventsPerStream, newData, context);
                long max = maxEval.Evaluate(rightEnd, eventsPerStream, newData, context);
                if (during) {
                    return ComputeIntervalDuring(leftStart, leftEnd, rightStart, rightEnd, min, max, min, max);
                }

                return ComputeIntervalIncludes(leftStart, leftEnd, rightStart, rightEnd, min, max, min, max);
            }

            public static CodegenExpression Codegen(
                IntervalComputerDuringAndIncludesMinMax forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool), typeof(IntervalComputerDuringAndIncludesMinMaxEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar(
                        typeof(long), "min",
                        forge.minEval.Codegen(
                            IntervalForgeCodegenNames.REF_LEFTSTART, methodNode, exprSymbol, codegenClassScope))
                    .DeclareVar(
                        typeof(long), "max",
                        forge.maxEval.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTEND, methodNode, exprSymbol, codegenClassScope));
                block.MethodReturn(
                    StaticMethod(
                        typeof(IntervalComputerDuringAndIncludesMinMaxEval),
                        forge.during ? "computeIntervalDuring" : "computeIntervalIncludes",
                        IntervalForgeCodegenNames.REF_LEFTSTART, IntervalForgeCodegenNames.REF_LEFTEND,
                        IntervalForgeCodegenNames.REF_RIGHTSTART, IntervalForgeCodegenNames.REF_RIGHTEND, Ref("min"),
                        Ref("max"), Ref("min"), Ref("max")));
                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }

            public static bool ComputeIntervalDuring(
                long left,
                long leftEnd,
                long right,
                long rightEnd,
                long startMin,
                long startMax,
                long endMin,
                long endMax)
            {
                if (startMin <= 0) {
                    startMin = 1;
                }

                var deltaStart = left - right;
                if (deltaStart < startMin || deltaStart > startMax) {
                    return false;
                }

                var deltaEnd = rightEnd - leftEnd;
                return !(deltaEnd < endMin || deltaEnd > endMax);
            }

            public static bool ComputeIntervalIncludes(
                long left,
                long leftEnd,
                long right,
                long rightEnd,
                long startMin,
                long startMax,
                long endMin,
                long endMax)
            {
                if (startMin <= 0) {
                    startMin = 1;
                }

                var deltaStart = right - left;
                if (deltaStart < startMin || deltaStart > startMax) {
                    return false;
                }

                var deltaEnd = leftEnd - rightEnd;
                return !(deltaEnd < endMin || deltaEnd > endMax);
            }
        }

        public class IntervalComputerDuringMinMaxStartEndForge : IntervalComputerForge
        {
            internal readonly bool during;
            internal readonly IntervalDeltaExprForge maxEndEval;
            internal readonly IntervalDeltaExprForge maxStartEval;
            internal readonly IntervalDeltaExprForge minEndEval;
            internal readonly IntervalDeltaExprForge minStartEval;

            public IntervalComputerDuringMinMaxStartEndForge(
                bool during,
                IntervalDeltaExprForge[] parameters)
            {
                this.during = during;
                minStartEval = parameters[0];
                maxStartEval = parameters[1];
                minEndEval = parameters[2];
                maxEndEval = parameters[3];
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerDuringMinMaxStartEndEval(
                    during, minStartEval.MakeEvaluator(), maxStartEval.MakeEvaluator(), minEndEval.MakeEvaluator(),
                    maxEndEval.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerDuringMinMaxStartEndEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerDuringMinMaxStartEndEval : IntervalComputerEval
        {
            internal readonly bool during;
            internal readonly IntervalDeltaExprEvaluator maxEndEval;
            internal readonly IntervalDeltaExprEvaluator maxStartEval;
            internal readonly IntervalDeltaExprEvaluator minEndEval;
            internal readonly IntervalDeltaExprEvaluator minStartEval;

            public IntervalComputerDuringMinMaxStartEndEval(
                bool during,
                IntervalDeltaExprEvaluator minStartEval,
                IntervalDeltaExprEvaluator maxStartEval,
                IntervalDeltaExprEvaluator minEndEval,
                IntervalDeltaExprEvaluator maxEndEval)
            {
                this.during = during;
                this.minStartEval = minStartEval;
                this.maxStartEval = maxStartEval;
                this.minEndEval = minEndEval;
                this.maxEndEval = maxEndEval;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                long minStart = minStartEval.Evaluate(rightStart, eventsPerStream, newData, context);
                long maxStart = maxStartEval.Evaluate(rightStart, eventsPerStream, newData, context);
                long minEnd = minEndEval.Evaluate(rightEnd, eventsPerStream, newData, context);
                long maxEnd = maxEndEval.Evaluate(rightEnd, eventsPerStream, newData, context);

                if (during) {
                    return IntervalComputerDuringAndIncludesMinMaxEval.ComputeIntervalDuring(
                        leftStart, leftEnd, rightStart, rightEnd, minStart, maxStart, minEnd, maxEnd);
                }

                return IntervalComputerDuringAndIncludesMinMaxEval.ComputeIntervalIncludes(
                    leftStart, leftEnd, rightStart, rightEnd, minStart, maxStart, minEnd, maxEnd);
            }

            public static CodegenExpression Codegen(
                IntervalComputerDuringMinMaxStartEndForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool), typeof(IntervalComputerDuringMinMaxStartEndEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar(
                        typeof(long), "minStart",
                        forge.minStartEval.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTSTART, methodNode, exprSymbol, codegenClassScope))
                    .DeclareVar(
                        typeof(long), "maxStart",
                        forge.maxStartEval.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTSTART, methodNode, exprSymbol, codegenClassScope))
                    .DeclareVar(
                        typeof(long), "minEnd",
                        forge.minEndEval.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTEND, methodNode, exprSymbol, codegenClassScope))
                    .DeclareVar(
                        typeof(long), "maxEnd",
                        forge.maxEndEval.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTEND, methodNode, exprSymbol, codegenClassScope));
                block.MethodReturn(
                    StaticMethod(
                        typeof(IntervalComputerDuringAndIncludesMinMaxEval),
                        forge.during ? "computeIntervalDuring" : "computeIntervalIncludes",
                        IntervalForgeCodegenNames.REF_LEFTSTART, IntervalForgeCodegenNames.REF_LEFTEND,
                        IntervalForgeCodegenNames.REF_RIGHTSTART, IntervalForgeCodegenNames.REF_RIGHTEND,
                        Ref("minStart"), Ref("maxStart"), Ref("minEnd"), Ref("maxEnd")));
                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }

        /// <summary>
        ///     Finishes.
        /// </summary>
        public class IntervalComputerFinishesNoParam : IntervalComputerForge,
            IntervalComputerEval
        {
            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return rightStart < leftStart && leftEnd == rightEnd;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return And(Relational(rightStart, LT, leftStart), EqualsIdentity(leftEnd, rightEnd));
            }
        }

        public class IntervalComputerFinishesThresholdForge : IntervalComputerForge
        {
            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            internal readonly IntervalDeltaExprForge thresholdExpr;

            public IntervalComputerFinishesThresholdForge(IntervalDeltaExprForge thresholdExpr)
            {
                this.thresholdExpr = thresholdExpr;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerFinishesThresholdEval(thresholdExpr.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerFinishesThresholdEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerFinishesThresholdEval : IntervalComputerEval
        {
            public const string METHOD_LOGWARNINGINTERVALFINISHTHRESHOLD = "logWarningIntervalFinishThreshold";

            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            internal readonly IntervalDeltaExprEvaluator thresholdExpr;

            public IntervalComputerFinishesThresholdEval(IntervalDeltaExprEvaluator thresholdExpr)
            {
                this.thresholdExpr = thresholdExpr;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                long threshold = thresholdExpr.Evaluate(Math.Min(leftEnd, rightEnd), eventsPerStream, newData, context);

                if (threshold < 0) {
                    LogWarningIntervalFinishThreshold();
                    return null;
                }

                if (rightStart >= leftStart) {
                    return false;
                }

                var delta = Math.Abs(leftEnd - rightEnd);
                return delta <= threshold;
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            public static void LogWarningIntervalFinishThreshold()
            {
                Log.Warn("The 'finishes' date-time method does not allow negative threshold");
            }

            public static CodegenExpression Codegen(
                IntervalComputerFinishesThresholdForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(bool?), typeof(IntervalComputerFinishesThresholdEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                methodNode.Block
                    .DeclareVar(
                        typeof(long), "threshold",
                        forge.thresholdExpr.Codegen(
                            StaticMethod(
                                typeof(Math), "min", IntervalForgeCodegenNames.REF_LEFTEND,
                                IntervalForgeCodegenNames.REF_RIGHTEND), methodNode, exprSymbol, codegenClassScope))
                    .IfCondition(Relational(Ref("threshold"), LT, Constant(0)))
                    .StaticMethod(
                        typeof(IntervalComputerFinishesThresholdEval), METHOD_LOGWARNINGINTERVALFINISHTHRESHOLD)
                    .BlockReturn(ConstantNull())
                    .IfConditionReturnConst(
                        Relational(
                            IntervalForgeCodegenNames.REF_RIGHTSTART, GE, IntervalForgeCodegenNames.REF_LEFTSTART),
                        false)
                    .DeclareVar(
                        typeof(long), "delta",
                        StaticMethod(
                            typeof(Math), "abs",
                            Op(IntervalForgeCodegenNames.REF_LEFTEND, "-", IntervalForgeCodegenNames.REF_RIGHTEND)))
                    .MethodReturn(Relational(Ref("delta"), LE, Ref("threshold")));
                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }

        /// <summary>
        ///     Finishes-By.
        /// </summary>
        public class IntervalComputerFinishedByNoParam : IntervalComputerForge,
            IntervalComputerEval
        {
            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return leftStart < rightStart && leftEnd == rightEnd;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return And(Relational(leftStart, LT, rightStart), EqualsIdentity(leftEnd, rightEnd));
            }
        }

        public class IntervalComputerFinishedByThresholdForge : IntervalComputerForge
        {
            internal readonly IntervalDeltaExprForge thresholdExpr;

            public IntervalComputerFinishedByThresholdForge(IntervalDeltaExprForge thresholdExpr)
            {
                this.thresholdExpr = thresholdExpr;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerFinishedByThresholdEval(thresholdExpr.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerFinishedByThresholdEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerFinishedByThresholdEval : IntervalComputerEval
        {
            public const string METHOD_LOGWARNINGINTERVALFINISHEDBYTHRESHOLD = "logWarningIntervalFinishedByThreshold";

            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            internal readonly IntervalDeltaExprEvaluator thresholdExpr;

            public IntervalComputerFinishedByThresholdEval(IntervalDeltaExprEvaluator thresholdExpr)
            {
                this.thresholdExpr = thresholdExpr;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                long threshold = thresholdExpr.Evaluate(Math.Min(rightEnd, leftEnd), eventsPerStream, newData, context);
                if (threshold < 0) {
                    LogWarningIntervalFinishedByThreshold();
                    return null;
                }

                if (leftStart >= rightStart) {
                    return false;
                }

                var delta = Math.Abs(leftEnd - rightEnd);
                return delta <= threshold;
            }

            public static void LogWarningIntervalFinishedByThreshold()
            {
                Log.Warn("The 'finishes' date-time method does not allow negative threshold");
            }

            public static CodegenExpression Codegen(
                IntervalComputerFinishedByThresholdForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool?), typeof(IntervalComputerFinishedByThresholdEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                methodNode.Block
                    .DeclareVar(
                        typeof(long), "threshold",
                        forge.thresholdExpr.Codegen(
                            StaticMethod(
                                typeof(Math), "min", IntervalForgeCodegenNames.REF_RIGHTEND,
                                IntervalForgeCodegenNames.REF_LEFTEND), methodNode, exprSymbol, codegenClassScope))
                    .IfCondition(Relational(Ref("threshold"), LT, Constant(0)))
                    .StaticMethod(
                        typeof(IntervalComputerFinishedByThresholdEval), METHOD_LOGWARNINGINTERVALFINISHEDBYTHRESHOLD)
                    .BlockReturn(ConstantNull())
                    .IfConditionReturnConst(
                        Relational(
                            IntervalForgeCodegenNames.REF_LEFTSTART, GE, IntervalForgeCodegenNames.REF_RIGHTSTART),
                        false)
                    .DeclareVar(
                        typeof(long), "delta",
                        StaticMethod(
                            typeof(Math), "abs",
                            Op(IntervalForgeCodegenNames.REF_LEFTEND, "-", IntervalForgeCodegenNames.REF_RIGHTEND)))
                    .MethodReturn(Relational(Ref("delta"), LE, Ref("threshold")));
                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }

        /// <summary>
        ///     Meets.
        /// </summary>
        public class IntervalComputerMeetsNoParam : IntervalComputerForge,
            IntervalComputerEval
        {
            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return leftEnd == rightStart;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return EqualsIdentity(leftEnd, rightStart);
            }
        }

        public class IntervalComputerMeetsThresholdForge : IntervalComputerForge
        {
            internal readonly IntervalDeltaExprForge thresholdExpr;

            public IntervalComputerMeetsThresholdForge(IntervalDeltaExprForge thresholdExpr)
            {
                this.thresholdExpr = thresholdExpr;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerMeetsThresholdEval(thresholdExpr.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerMeetsThresholdEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerMeetsThresholdEval : IntervalComputerEval
        {
            public const string METHOD_LOGWARNINGINTERVALMEETSTHRESHOLD = "logWarningIntervalMeetsThreshold";

            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            internal readonly IntervalDeltaExprEvaluator thresholdExpr;

            public IntervalComputerMeetsThresholdEval(IntervalDeltaExprEvaluator thresholdExpr)
            {
                this.thresholdExpr = thresholdExpr;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                long threshold = thresholdExpr.Evaluate(
                    Math.Min(leftEnd, rightStart), eventsPerStream, newData, context);
                if (threshold < 0) {
                    LogWarningIntervalMeetsThreshold();
                    return null;
                }

                var delta = Math.Abs(rightStart - leftEnd);
                return delta <= threshold;
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            public static void LogWarningIntervalMeetsThreshold()
            {
                Log.Warn("The 'meets' date-time method does not allow negative threshold");
            }

            public static CodegenExpression Codegen(
                IntervalComputerMeetsThresholdForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(bool?), typeof(IntervalComputerMeetsThresholdEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                methodNode.Block
                    .DeclareVar(
                        typeof(long), "threshold",
                        forge.thresholdExpr.Codegen(
                            StaticMethod(
                                typeof(Math), "min", IntervalForgeCodegenNames.REF_LEFTEND,
                                IntervalForgeCodegenNames.REF_RIGHTSTART), methodNode, exprSymbol, codegenClassScope))
                    .IfCondition(Relational(Ref("threshold"), LT, Constant(0)))
                    .StaticMethod(typeof(IntervalComputerMeetsThresholdEval), METHOD_LOGWARNINGINTERVALMEETSTHRESHOLD)
                    .BlockReturn(ConstantNull())
                    .DeclareVar(
                        typeof(long), "delta",
                        StaticMethod(
                            typeof(Math), "abs",
                            Op(IntervalForgeCodegenNames.REF_RIGHTSTART, "-", IntervalForgeCodegenNames.REF_LEFTEND)))
                    .MethodReturn(Relational(Ref("delta"), LE, Ref("threshold")));
                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }

        /// <summary>
        ///     Met-By.
        /// </summary>
        public class IntervalComputerMetByNoParam : IntervalComputerForge,
            IntervalComputerEval
        {
            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return rightEnd == leftStart;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return EqualsIdentity(rightEnd, leftStart);
            }
        }

        public class IntervalComputerMetByThresholdForge : IntervalComputerForge
        {
            internal readonly IntervalDeltaExprForge thresholdExpr;

            public IntervalComputerMetByThresholdForge(IntervalDeltaExprForge thresholdExpr)
            {
                this.thresholdExpr = thresholdExpr;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerMetByThresholdEval(thresholdExpr.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerMetByThresholdEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerMetByThresholdEval : IntervalComputerEval
        {
            public const string METHOD_LOGWARNINGINTERVALMETBYTHRESHOLD = "logWarningIntervalMetByThreshold";

            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            internal readonly IntervalDeltaExprEvaluator thresholdExpr;

            public IntervalComputerMetByThresholdEval(IntervalDeltaExprEvaluator thresholdExpr)
            {
                this.thresholdExpr = thresholdExpr;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                long threshold = thresholdExpr.Evaluate(
                    Math.Min(leftStart, rightEnd), eventsPerStream, newData, context);

                if (threshold < 0) {
                    LogWarningIntervalMetByThreshold();
                    return null;
                }

                var delta = Math.Abs(leftStart - rightEnd);
                return delta <= threshold;
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            public static void LogWarningIntervalMetByThreshold()
            {
                Log.Warn("The 'met-by' date-time method does not allow negative threshold");
            }

            public static CodegenExpression Codegen(
                IntervalComputerMetByThresholdForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(bool?), typeof(IntervalComputerMetByThresholdEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                methodNode.Block
                    .DeclareVar(
                        typeof(long), "threshold",
                        forge.thresholdExpr.Codegen(
                            StaticMethod(
                                typeof(Math), "min", IntervalForgeCodegenNames.REF_LEFTSTART,
                                IntervalForgeCodegenNames.REF_RIGHTEND), methodNode, exprSymbol, codegenClassScope))
                    .IfCondition(Relational(Ref("threshold"), LT, Constant(0)))
                    .StaticMethod(typeof(IntervalComputerMetByThresholdEval), METHOD_LOGWARNINGINTERVALMETBYTHRESHOLD)
                    .BlockReturn(ConstantNull())
                    .DeclareVar(
                        typeof(long), "delta",
                        StaticMethod(
                            typeof(Math), "abs",
                            Op(IntervalForgeCodegenNames.REF_LEFTSTART, "-", IntervalForgeCodegenNames.REF_RIGHTEND)))
                    .MethodReturn(Relational(Ref("delta"), LE, Ref("threshold")));
                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }

        /// <summary>
        ///     Overlaps.
        /// </summary>
        public class IntervalComputerOverlapsNoParam : IntervalComputerForge,
            IntervalComputerEval
        {
            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return leftStart < rightStart &&
                       rightStart < leftEnd &&
                       leftEnd < rightEnd;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return And(
                    Relational(leftStart, LT, rightStart),
                    Relational(rightStart, LT, leftEnd),
                    Relational(leftEnd, LT, rightEnd));
            }
        }

        public class IntervalComputerOverlapsAndByThreshold : IntervalComputerForge
        {
            internal readonly bool overlaps;
            internal readonly IntervalDeltaExprForge thresholdExpr;

            public IntervalComputerOverlapsAndByThreshold(
                bool overlaps,
                IntervalDeltaExprForge thresholdExpr)
            {
                this.overlaps = overlaps;
                this.thresholdExpr = thresholdExpr;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerOverlapsAndByThresholdEval(overlaps, thresholdExpr.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerOverlapsAndByThresholdEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerOverlapsAndByThresholdEval : IntervalComputerEval
        {
            internal readonly bool overlaps;
            internal readonly IntervalDeltaExprEvaluator thresholdExpr;

            public IntervalComputerOverlapsAndByThresholdEval(
                bool overlaps,
                IntervalDeltaExprEvaluator thresholdExpr)
            {
                this.overlaps = overlaps;
                this.thresholdExpr = thresholdExpr;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                if (overlaps) {
                    long threshold = thresholdExpr.Evaluate(leftStart, eventsPerStream, newData, context);
                    return ComputeIntervalOverlaps(leftStart, leftEnd, rightStart, rightEnd, 0, threshold);
                }
                else {
                    long threshold = thresholdExpr.Evaluate(rightStart, eventsPerStream, newData, context);
                    return ComputeIntervalOverlaps(rightStart, rightEnd, leftStart, leftEnd, 0, threshold);
                }
            }

            public static CodegenExpression Codegen(
                IntervalComputerOverlapsAndByThreshold forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool), typeof(IntervalComputerOverlapsAndByThresholdEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar(
                        typeof(long), "threshold",
                        forge.thresholdExpr.Codegen(
                            forge.overlaps
                                ? IntervalForgeCodegenNames.REF_LEFTSTART
                                : IntervalForgeCodegenNames.REF_RIGHTSTART, methodNode, exprSymbol, codegenClassScope));
                CodegenMethod method;
                if (forge.overlaps) {
                    block.MethodReturn(
                        StaticMethod(
                            typeof(IntervalComputerOverlapsAndByThresholdEval), "computeIntervalOverlaps",
                            IntervalForgeCodegenNames.REF_LEFTSTART, IntervalForgeCodegenNames.REF_LEFTEND,
                            IntervalForgeCodegenNames.REF_RIGHTSTART, IntervalForgeCodegenNames.REF_RIGHTEND,
                            Constant(0), Ref("threshold")));
                }
                else {
                    block.MethodReturn(
                        StaticMethod(
                            typeof(IntervalComputerOverlapsAndByThresholdEval), "computeIntervalOverlaps",
                            IntervalForgeCodegenNames.REF_RIGHTSTART, IntervalForgeCodegenNames.REF_RIGHTEND,
                            IntervalForgeCodegenNames.REF_LEFTSTART, IntervalForgeCodegenNames.REF_LEFTEND, Constant(0),
                            Ref("threshold")));
                }

                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            /// <param name="left">left start</param>
            /// <param name="leftEnd">left end</param>
            /// <param name="right">right start</param>
            /// <param name="rightEnd">right end</param>
            /// <param name="min">min</param>
            /// <param name="max">max</param>
            /// <returns>flag</returns>
            public static bool ComputeIntervalOverlaps(
                long left,
                long leftEnd,
                long right,
                long rightEnd,
                long min,
                long max)
            {
                var match = left < right &&
                            right < leftEnd &&
                            leftEnd < rightEnd;
                if (!match) {
                    return false;
                }

                var delta = leftEnd - right;
                return min <= delta && delta <= max;
            }
        }

        public class IntervalComputerOverlapsAndByMinMaxForge : IntervalComputerForge
        {
            internal readonly IntervalDeltaExprForge maxEval;
            internal readonly IntervalDeltaExprForge minEval;

            internal readonly bool overlaps;

            public IntervalComputerOverlapsAndByMinMaxForge(
                bool overlaps,
                IntervalDeltaExprForge minEval,
                IntervalDeltaExprForge maxEval)
            {
                this.overlaps = overlaps;
                this.minEval = minEval;
                this.maxEval = maxEval;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerOverlapsAndByMinMaxEval(
                    overlaps, minEval.MakeEvaluator(), maxEval.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerOverlapsAndByMinMaxEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerOverlapsAndByMinMaxEval : IntervalComputerEval
        {
            internal readonly IntervalDeltaExprEvaluator maxEval;
            internal readonly IntervalDeltaExprEvaluator minEval;

            internal readonly bool overlaps;

            public IntervalComputerOverlapsAndByMinMaxEval(
                bool overlaps,
                IntervalDeltaExprEvaluator minEval,
                IntervalDeltaExprEvaluator maxEval)
            {
                this.overlaps = overlaps;
                this.minEval = minEval;
                this.maxEval = maxEval;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                if (overlaps) {
                    long minThreshold = minEval.Evaluate(leftStart, eventsPerStream, newData, context);
                    long maxThreshold = maxEval.Evaluate(leftEnd, eventsPerStream, newData, context);
                    return IntervalComputerOverlapsAndByThresholdEval.ComputeIntervalOverlaps(
                        leftStart, leftEnd, rightStart, rightEnd, minThreshold, maxThreshold);
                }
                else {
                    long minThreshold = minEval.Evaluate(rightStart, eventsPerStream, newData, context);
                    long maxThreshold = maxEval.Evaluate(rightEnd, eventsPerStream, newData, context);
                    return IntervalComputerOverlapsAndByThresholdEval.ComputeIntervalOverlaps(
                        rightStart, rightEnd, leftStart, leftEnd, minThreshold, maxThreshold);
                }
            }

            public static CodegenExpression Codegen(
                IntervalComputerOverlapsAndByMinMaxForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool), typeof(IntervalComputerOverlapsAndByMinMaxEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar(
                        typeof(long), "minThreshold",
                        forge.minEval.Codegen(
                            forge.overlaps
                                ? IntervalForgeCodegenNames.REF_LEFTSTART
                                : IntervalForgeCodegenNames.REF_RIGHTSTART, methodNode, exprSymbol, codegenClassScope))
                    .DeclareVar(
                        typeof(long), "maxThreshold",
                        forge.maxEval.Codegen(
                            forge.overlaps
                                ? IntervalForgeCodegenNames.REF_LEFTEND
                                : IntervalForgeCodegenNames.REF_RIGHTEND, methodNode, exprSymbol, codegenClassScope));
                if (forge.overlaps) {
                    block.MethodReturn(
                        StaticMethod(
                            typeof(IntervalComputerOverlapsAndByThresholdEval), "computeIntervalOverlaps",
                            IntervalForgeCodegenNames.REF_LEFTSTART, IntervalForgeCodegenNames.REF_LEFTEND,
                            IntervalForgeCodegenNames.REF_RIGHTSTART, IntervalForgeCodegenNames.REF_RIGHTEND,
                            Ref("minThreshold"), Ref("maxThreshold")));
                }
                else {
                    block.MethodReturn(
                        StaticMethod(
                            typeof(IntervalComputerOverlapsAndByThresholdEval), "computeIntervalOverlaps",
                            IntervalForgeCodegenNames.REF_RIGHTSTART, IntervalForgeCodegenNames.REF_RIGHTEND,
                            IntervalForgeCodegenNames.REF_LEFTSTART, IntervalForgeCodegenNames.REF_LEFTEND,
                            Ref("minThreshold"), Ref("maxThreshold")));
                }

                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }

        /// <summary>
        ///     OverlappedBy.
        /// </summary>
        public class IntervalComputerOverlappedByNoParam : IntervalComputerForge,
            IntervalComputerEval
        {
            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return rightStart < leftStart &&
                       leftStart < rightEnd &&
                       rightEnd < leftEnd;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return And(
                    Relational(rightStart, LT, leftStart),
                    Relational(leftStart, LT, rightEnd),
                    Relational(rightEnd, LT, leftEnd));
            }
        }

        /// <summary>
        ///     Starts.
        /// </summary>
        public class IntervalComputerStartsNoParam : IntervalComputerForge,
            IntervalComputerEval
        {
            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return leftStart == rightStart && leftEnd < rightEnd;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return And(EqualsIdentity(leftStart, rightStart), Relational(leftEnd, LT, rightEnd));
            }
        }

        public class IntervalComputerStartsThresholdForge : IntervalComputerForge
        {
            internal readonly IntervalDeltaExprForge thresholdExpr;

            public IntervalComputerStartsThresholdForge(IntervalDeltaExprForge thresholdExpr)
            {
                this.thresholdExpr = thresholdExpr;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerStartsThresholdEval(thresholdExpr.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerStartsThresholdEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerStartsThresholdEval : IntervalComputerEval
        {
            public const string METHOD_LOGWARNINGINTERVALSTARTSTHRESHOLD = "logWarningIntervalStartsThreshold";

            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            internal readonly IntervalDeltaExprEvaluator thresholdExpr;

            public IntervalComputerStartsThresholdEval(IntervalDeltaExprEvaluator thresholdExpr)
            {
                this.thresholdExpr = thresholdExpr;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                long threshold = thresholdExpr.Evaluate(
                    Math.Min(leftStart, rightStart), eventsPerStream, newData, context);
                if (threshold < 0) {
                    LogWarningIntervalStartsThreshold();
                    return null;
                }

                var delta = Math.Abs(leftStart - rightStart);
                return delta <= threshold && leftEnd < rightEnd;
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            public static void LogWarningIntervalStartsThreshold()
            {
                Log.Warn("The 'starts' date-time method does not allow negative threshold");
            }

            public static CodegenExpression Codegen(
                IntervalComputerStartsThresholdForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(bool?), typeof(IntervalComputerStartsThresholdEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                methodNode.Block
                    .DeclareVar(
                        typeof(long), "threshold",
                        forge.thresholdExpr.Codegen(
                            StaticMethod(
                                typeof(Math), "min", IntervalForgeCodegenNames.REF_LEFTSTART,
                                IntervalForgeCodegenNames.REF_RIGHTSTART), methodNode, exprSymbol, codegenClassScope))
                    .IfCondition(Relational(Ref("threshold"), LT, Constant(0)))
                    .StaticMethod(typeof(IntervalComputerStartsThresholdEval), METHOD_LOGWARNINGINTERVALSTARTSTHRESHOLD)
                    .BlockReturn(ConstantNull())
                    .DeclareVar(
                        typeof(long), "delta",
                        StaticMethod(
                            typeof(Math), "abs",
                            Op(IntervalForgeCodegenNames.REF_LEFTSTART, "-", IntervalForgeCodegenNames.REF_RIGHTSTART)))
                    .MethodReturn(
                        And(
                            Relational(Ref("delta"), LE, Ref("threshold")),
                            Relational(
                                IntervalForgeCodegenNames.REF_LEFTEND, LT, IntervalForgeCodegenNames.REF_RIGHTEND)));
                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }

        /// <summary>
        ///     Started-by.
        /// </summary>
        public class IntervalComputerStartedByNoParam : IntervalComputerForge,
            IntervalComputerEval
        {
            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return leftStart == rightStart && leftEnd > rightEnd;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return And(EqualsIdentity(leftStart, rightStart), Relational(leftEnd, GT, rightEnd));
            }
        }

        public class IntervalComputerStartedByThresholdForge : IntervalComputerForge
        {
            internal readonly IntervalDeltaExprForge thresholdExpr;

            public IntervalComputerStartedByThresholdForge(IntervalDeltaExprForge thresholdExpr)
            {
                this.thresholdExpr = thresholdExpr;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerStartedByThresholdEval(thresholdExpr.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerStartedByThresholdEval.Codegen(
                    this, leftStart, leftEnd, rightStart, rightEnd, codegenMethodScope, exprSymbol, codegenClassScope);
            }
        }

        public class IntervalComputerStartedByThresholdEval : IntervalComputerEval
        {
            public const string METHOD_LOGWARNINGINTERVALSTARTEDBYTHRESHOLD = "logWarningIntervalStartedByThreshold";

            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            internal readonly IntervalDeltaExprEvaluator thresholdExpr;

            public IntervalComputerStartedByThresholdEval(IntervalDeltaExprEvaluator thresholdExpr)
            {
                this.thresholdExpr = thresholdExpr;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                long threshold = thresholdExpr.Evaluate(
                    Math.Min(leftStart, rightStart), eventsPerStream, newData, context);
                if (threshold < 0) {
                    LogWarningIntervalStartedByThreshold();
                    return null;
                }

                var delta = Math.Abs(leftStart - rightStart);
                return delta <= threshold && leftEnd > rightEnd;
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            public static void LogWarningIntervalStartedByThreshold()
            {
                Log.Warn("The 'started-by' date-time method does not allow negative threshold");
            }

            public static CodegenExpression Codegen(
                IntervalComputerStartedByThresholdForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool?), typeof(IntervalComputerStartedByThresholdEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                methodNode.Block
                    .DeclareVar(
                        typeof(long), "threshold",
                        forge.thresholdExpr.Codegen(
                            StaticMethod(
                                typeof(Math), "min", IntervalForgeCodegenNames.REF_LEFTSTART,
                                IntervalForgeCodegenNames.REF_RIGHTSTART), methodNode, exprSymbol, codegenClassScope))
                    .IfCondition(Relational(Ref("threshold"), LT, Constant(0)))
                    .StaticMethod(
                        typeof(IntervalComputerStartedByThresholdEval), METHOD_LOGWARNINGINTERVALSTARTEDBYTHRESHOLD)
                    .BlockReturn(ConstantNull())
                    .DeclareVar(
                        typeof(long), "delta",
                        StaticMethod(
                            typeof(Math), "abs",
                            Op(IntervalForgeCodegenNames.REF_LEFTSTART, "-", IntervalForgeCodegenNames.REF_RIGHTSTART)))
                    .MethodReturn(
                        And(
                            Relational(Ref("delta"), LE, Ref("threshold")),
                            Relational(
                                IntervalForgeCodegenNames.REF_LEFTEND, GT, IntervalForgeCodegenNames.REF_RIGHTEND)));
                return LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }
    }
} // end of namespace