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
            DatetimeMethodDesc method,
            IList<ExprNode> expressions,
            TimeAbacus timeAbacus)
        {
            var parameters = GetParameters(expressions, timeAbacus);

            var dateTimeMethod = method.DatetimeMethod;
            switch (dateTimeMethod) {
                case DateTimeMethodEnum.BEFORE when parameters.Length == 0:
                    return new IntervalComputerBeforeNoParamForge();

                case DateTimeMethodEnum.BEFORE: {
                    var pair =
                        IntervalStartEndParameterPairForge.FromParamsWithLongMaxEnd(parameters);
                    if (pair.IsConstant) {
                        return new IntervalComputerConstantBefore(pair);
                    }

                    return new IntervalComputerBeforeWithDeltaExprForge(pair);
                }

                case DateTimeMethodEnum.AFTER when parameters.Length == 0:
                    return new IntervalComputerAfterNoParam();

                case DateTimeMethodEnum.AFTER: {
                    var pair =
                        IntervalStartEndParameterPairForge.FromParamsWithLongMaxEnd(parameters);
                    if (pair.IsConstant) {
                        return new IntervalComputerConstantAfter(pair);
                    }

                    return new IntervalComputerAfterWithDeltaExprForge(pair);
                }

                case DateTimeMethodEnum.COINCIDES when parameters.Length == 0:
                    return new IntervalComputerCoincidesNoParam();

                case DateTimeMethodEnum.COINCIDES: {
                    var pair =
                        IntervalStartEndParameterPairForge.FromParamsWithSameEnd(parameters);
                    if (pair.IsConstant) {
                        return new IntervalComputerConstantCoincides(pair);
                    }

                    return new IntervalComputerCoincidesWithDeltaExprForge(pair);
                }

                case DateTimeMethodEnum.DURING:
                case DateTimeMethodEnum.INCLUDES: {
                    if (parameters.Length == 0) {
                        if (dateTimeMethod == DateTimeMethodEnum.DURING) {
                            return new IntervalComputerDuringNoParam();
                        }

                        return new IntervalComputerIncludesNoParam();
                    }

                    var pair =
                        IntervalStartEndParameterPairForge.FromParamsWithSameEnd(parameters);
                    if (parameters.Length == 1) {
                        return new IntervalComputerDuringAndIncludesThresholdForge(
                            dateTimeMethod == DateTimeMethodEnum.DURING,
                            pair.Start.Forge);
                    }

                    if (parameters.Length == 2) {
                        return new IntervalComputerDuringAndIncludesMinMax(
                            dateTimeMethod == DateTimeMethodEnum.DURING,
                            pair.Start.Forge,
                            pair.End.Forge);
                    }

                    return new IntervalComputerDuringMinMaxStartEndForge(
                        dateTimeMethod == DateTimeMethodEnum.DURING,
                        GetEvaluators(expressions, timeAbacus));
                }

                case DateTimeMethodEnum.FINISHES when parameters.Length == 0:
                    return new IntervalComputerFinishesNoParam();

                case DateTimeMethodEnum.FINISHES:
                    ValidateConstantThreshold("finishes", parameters[0]);
                    return new IntervalComputerFinishesThresholdForge(parameters[0].Forge);

                case DateTimeMethodEnum.FINISHEDBY when parameters.Length == 0:
                    return new IntervalComputerFinishedByNoParam();

                case DateTimeMethodEnum.FINISHEDBY:
                    ValidateConstantThreshold("finishedby", parameters[0]);
                    return new IntervalComputerFinishedByThresholdForge(parameters[0].Forge);

                case DateTimeMethodEnum.MEETS when parameters.Length == 0:
                    return new IntervalComputerMeetsNoParam();

                case DateTimeMethodEnum.MEETS:
                    ValidateConstantThreshold("meets", parameters[0]);
                    return new IntervalComputerMeetsThresholdForge(parameters[0].Forge);

                case DateTimeMethodEnum.METBY when parameters.Length == 0:
                    return new IntervalComputerMetByNoParam();

                case DateTimeMethodEnum.METBY:
                    ValidateConstantThreshold("metBy", parameters[0]);
                    return new IntervalComputerMetByThresholdForge(parameters[0].Forge);

                case DateTimeMethodEnum.OVERLAPS:
                case DateTimeMethodEnum.OVERLAPPEDBY: {
                    if (parameters.Length == 0) {
                        if (dateTimeMethod == DateTimeMethodEnum.OVERLAPS) {
                            return new IntervalComputerOverlapsNoParam();
                        }

                        return new IntervalComputerOverlappedByNoParam();
                    }

                    if (parameters.Length == 1) {
                        return new IntervalComputerOverlapsAndByThreshold(
                            dateTimeMethod == DateTimeMethodEnum.OVERLAPS,
                            parameters[0].Forge);
                    }

                    return new IntervalComputerOverlapsAndByMinMaxForge(
                        dateTimeMethod == DateTimeMethodEnum.OVERLAPS,
                        parameters[0].Forge,
                        parameters[1].Forge);
                }

                case DateTimeMethodEnum.STARTS when parameters.Length == 0:
                    return new IntervalComputerStartsNoParam();

                case DateTimeMethodEnum.STARTS:
                    ValidateConstantThreshold("starts", parameters[0]);
                    return new IntervalComputerStartsThresholdForge(parameters[0].Forge);

                case DateTimeMethodEnum.STARTEDBY when parameters.Length == 0:
                    return new IntervalComputerStartedByNoParam();

                case DateTimeMethodEnum.STARTEDBY:
                    ValidateConstantThreshold("startedBy", parameters[0]);
                    return new IntervalComputerStartedByThresholdForge(parameters[0].Forge);

                default:
                    throw new ArgumentException("Unknown datetime method '" + method + "'");
            }
        }

        private static void ValidateConstantThreshold(
            string method,
            ExprOptionalConstantForge param)
        {
            if (param.OptionalConstant != null && param.OptionalConstant.AsInt64() < 0) {
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
                        new IntervalDeltaExprTimePeriodNonConstForge(timePeriod, timeAbacus),
                        null);
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
                                    reference,
                                    eventsPerStream,
                                    isNewData,
                                    context)
                        };
                    },

                    ProcCodegen = (
                        reference,
                        parent,
                        exprSymbol,
                        codegenClassScope) => {
                        var field = codegenClassScope.AddDefaultFieldUnshared(
                            true,
                            typeof(TimePeriodCompute),
                            timePeriod.TimePeriodComputeForge.MakeEvaluator(
                                codegenClassScope.NamespaceScope.InitMethod,
                                codegenClassScope));
                        return ExprDotMethod(
                            field,
                            "DeltaAdd",
                            reference,
                            exprSymbol.GetAddEPS(parent),
                            exprSymbol.GetAddIsNewData(parent),
                            exprSymbol.GetAddExprEvalCtx(parent));
                    }
                };
                return new ExprOptionalConstantForge(forge, null);
            }

            if (ExprNodeUtilityQuery.IsConstant(exprNode)) {
                var constantNode = (ExprConstantNode) exprNode;
                var l = constantNode.ConstantValue.AsInt64();
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
                                        eventsPerStream,
                                        isNewData,
                                        context)
                                    .AsInt64()
                        };
                    },

                    ProcCodegen = (
                        reference,
                        codegenMethodScope,
                        exprSymbol,
                        codegenClassScope) => SimpleNumberCoercerFactory.CoercerLong.CodegenLong(
                        forge.EvaluateCodegen(
                            forge.EvaluationType,
                            codegenMethodScope,
                            exprSymbol,
                            codegenClassScope),
                        forge.EvaluationType)
                };
                return new ExprOptionalConstantForge(eval, null);
            }
        }
    }
} // end of namespace