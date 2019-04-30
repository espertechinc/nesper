///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.datetime.interval.deltaexpr;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalComputerForgeFactory
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

                var pair =
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

                var pair =
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

                var pair =
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

                var pair =
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
                    if (exprNode.Forge.ForgeConstantType.IsCompileTimeConstant) {
                        var sec = timePeriod.EvaluateAsSeconds(null, true, null);
                        var l = timeAbacus.DeltaForSecondsDouble(sec);
                        return new ExprOptionalConstantForge(new IntervalDeltaExprMSecConstForge(l), l);
                    }

                    return new ExprOptionalConstantForge(
                        new IntervalDeltaExprTimePeriodNonConstForge(timePeriod, timeAbacus), null);
                }

                // has-month and not constant
                var timePeriodCompute = timePeriod.TimePeriodComputeForge.Evaluator;
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
                        var field = codegenClassScope.AddFieldUnshared(
                            true, typeof(TimePeriodCompute),
                            timePeriod.TimePeriodComputeForge.MakeEvaluator(
                                codegenClassScope.NamespaceScope.InitMethod, codegenClassScope));
                        return ExprDotMethod(
                            field, "deltaAdd", reference, exprSymbol.GetAddEPS(parent),
                            exprSymbol.GetAddIsNewData(parent), exprSymbol.GetAddExprEvalCtx(parent));
                    }
                };
                return new ExprOptionalConstantForge(forge, null);
            }

            if (ExprNodeUtilityQuery.IsConstant(exprNode)) {
                var constantNode = (ExprConstantNode) exprNode;
                var l = constantNode.ConstantValue.AsLong();
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
                        codegenClassScope) => SimpleNumberCoercerFactory.CoercerLong.CodegenLong(
                        forge.EvaluateCodegen(
                            forge.EvaluationType, codegenMethodScope, exprSymbol, codegenClassScope),
                        forge.EvaluationType)
                };
                return new ExprOptionalConstantForge(eval, null);
            }
        }
    }
} // end of namespace