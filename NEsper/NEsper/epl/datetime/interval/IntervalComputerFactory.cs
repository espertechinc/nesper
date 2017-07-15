///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.epl.datetime.interval
{
    public class IntervalComputerFactory
    {
        public static IntervalComputer Make(DatetimeMethodEnum method, IList<ExprNode> expressions, TimeAbacus timeAbacus)
        {
            var parameters = GetParameters(expressions, timeAbacus);

            if (method == DatetimeMethodEnum.BEFORE)
            {
                if (parameters.Length == 0)
                {
                    return new IntervalComputerBeforeNoParam();
                }
                var pair = IntervalStartEndParameterPair.FromParamsWithLongMaxEnd(parameters);
                if (pair.IsConstant)
                {
                    return new IntervalComputerConstantBefore(pair);
                }
                return new IntervalComputerBeforeWithDeltaExpr(pair);
            }
            else if (method == DatetimeMethodEnum.AFTER)
            {
                if (parameters.Length == 0)
                {
                    return new IntervalComputerAfterNoParam();
                }
                var pair = IntervalStartEndParameterPair.FromParamsWithLongMaxEnd(parameters);
                if (pair.IsConstant)
                {
                    return new IntervalComputerConstantAfter(pair);
                }
                return new IntervalComputerAfterWithDeltaExpr(pair);
            }
            else if (method == DatetimeMethodEnum.COINCIDES)
            {
                if (parameters.Length == 0)
                {
                    return new IntervalComputerCoincidesNoParam();
                }
                var pair = IntervalStartEndParameterPair.FromParamsWithSameEnd(parameters);
                if (pair.IsConstant)
                {
                    return new IntervalComputerConstantCoincides(pair);
                }
                return new IntervalComputerCoincidesWithDeltaExpr(pair);
            }
            else if (method == DatetimeMethodEnum.DURING || method == DatetimeMethodEnum.INCLUDES)
            {
                if (parameters.Length == 0)
                {
                    if (method == DatetimeMethodEnum.DURING)
                    {
                        return new IntervalComputerDuringNoParam();
                    }
                    return new IntervalComputerIncludesNoParam();
                }
                var pair = IntervalStartEndParameterPair.FromParamsWithSameEnd(parameters);
                if (parameters.Length == 1)
                {
                    return new IntervalComputerDuringAndIncludesThreshold(
                        method == DatetimeMethodEnum.DURING, pair.Start.Evaluator);
                }
                if (parameters.Length == 2)
                {
                    return new IntervalComputerDuringAndIncludesMinMax(
                        method == DatetimeMethodEnum.DURING, pair.Start.Evaluator, pair.End.Evaluator);
                }
                return new IntervalComputerDuringMinMaxStartEnd(
                    method == DatetimeMethodEnum.DURING, GetEvaluators(expressions, timeAbacus));
            }
            else if (method == DatetimeMethodEnum.FINISHES)
            {
                if (parameters.Length == 0)
                {
                    return new IntervalComputerFinishesNoParam();
                }
                ValidateConstantThreshold("finishes", parameters[0]);
                return new IntervalComputerFinishesThreshold(parameters[0].Evaluator);
            }
            else if (method == DatetimeMethodEnum.FINISHEDBY)
            {
                if (parameters.Length == 0)
                {
                    return new IntervalComputerFinishedByNoParam();
                }
                ValidateConstantThreshold("finishedby", parameters[0]);
                return new IntervalComputerFinishedByThreshold(parameters[0].Evaluator);
            }
            else if (method == DatetimeMethodEnum.MEETS)
            {
                if (parameters.Length == 0)
                {
                    return new IntervalComputerMeetsNoParam();
                }
                ValidateConstantThreshold("meets", parameters[0]);
                return new IntervalComputerMeetsThreshold(parameters[0].Evaluator);
            }
            else if (method == DatetimeMethodEnum.METBY)
            {
                if (parameters.Length == 0)
                {
                    return new IntervalComputerMetByNoParam();
                }
                ValidateConstantThreshold("metBy", parameters[0]);
                return new IntervalComputerMetByThreshold(parameters[0].Evaluator);
            }
            else if (method == DatetimeMethodEnum.OVERLAPS || method == DatetimeMethodEnum.OVERLAPPEDBY)
            {
                if (parameters.Length == 0)
                {
                    if (method == DatetimeMethodEnum.OVERLAPS)
                    {
                        return new IntervalComputerOverlapsNoParam();
                    }
                    return new IntervalComputerOverlappedByNoParam();
                }
                if (parameters.Length == 1)
                {
                    return new IntervalComputerOverlapsAndByThreshold(
                        method == DatetimeMethodEnum.OVERLAPS, parameters[0].Evaluator);
                }
                return new IntervalComputerOverlapsAndByMinMax(
                    method == DatetimeMethodEnum.OVERLAPS, parameters[0].Evaluator, parameters[1].Evaluator);
            }
            else if (method == DatetimeMethodEnum.STARTS)
            {
                if (parameters.Length == 0)
                {
                    return new IntervalComputerStartsNoParam();
                }
                ValidateConstantThreshold("starts", parameters[0]);
                return new IntervalComputerStartsThreshold(parameters[0].Evaluator);
            }
            else if (method == DatetimeMethodEnum.STARTEDBY)
            {
                if (parameters.Length == 0)
                {
                    return new IntervalComputerStartedByNoParam();
                }
                ValidateConstantThreshold("startedBy", parameters[0]);
                return new IntervalComputerStartedByThreshold(parameters[0].Evaluator);
            }
            throw new ArgumentException("Unknown datetime method '" + method + "'");
        }

        private static void ValidateConstantThreshold(String method, ExprOptionalConstant param)
        {
            if (param.OptionalConstant != null && (param.OptionalConstant).AsLong() < 0)
            {
                throw new ExprValidationException(
                    "The " + method + " date-time method does not allow negative threshold value");
            }
        }

        private static ExprOptionalConstant[] GetParameters(IList<ExprNode> expressions, TimeAbacus timeAbacus)
        {
            var parameters = new ExprOptionalConstant[expressions.Count - 1];
            for (var i = 1; i < expressions.Count; i++)
            {
                parameters[i - 1] = GetExprOrConstant(expressions[i], timeAbacus);
            }
            return parameters;
        }

        private static IntervalDeltaExprEvaluator[] GetEvaluators(IList<ExprNode> expressions, TimeAbacus timeAbacus)
        {
            var parameters = new IntervalDeltaExprEvaluator[expressions.Count - 1];
            for (var i = 1; i < expressions.Count; i++)
            {
                parameters[i - 1] = GetExprOrConstant(expressions[i], timeAbacus).Evaluator;
            }
            return parameters;
        }

        private static ExprOptionalConstant GetExprOrConstant(ExprNode exprNode, TimeAbacus timeAbacus)
        {
            if (exprNode is ExprTimePeriod)
            {
                var timePeriod = (ExprTimePeriod)exprNode;
                if (!timePeriod.HasMonth && !timePeriod.HasYear)
                {
                    // no-month and constant
                    if (exprNode.IsConstantResult)
                    {
                        var sec = timePeriod.EvaluateAsSeconds(null, true, null);
                        var l = timeAbacus.DeltaForSecondsDouble(sec);
                        IntervalDeltaExprEvaluator eval = new ProxyIntervalDeltaExprEvaluator
                        {
                            ProcEvaluate = (reference, eventsPerStream, isNewData, context) => l,
                        };
                        return new ExprOptionalConstant(eval, l);
                    }
                    // no-month and not constant
                    else
                    {
                        IntervalDeltaExprEvaluator eval = new ProxyIntervalDeltaExprEvaluator
                        {
                            ProcEvaluate = (reference, eventsPerStream, isNewData, context) =>
                            {
                                double sec = timePeriod.EvaluateAsSeconds(eventsPerStream, isNewData, context);
                                return timeAbacus.DeltaForSecondsDouble(sec);
                            },
                        };
                        return new ExprOptionalConstant(eval, null);
                    }
                }
                // has-month
                else
                {
                    // has-month and constant
                    if (exprNode.IsConstantResult)
                    {
                        ExprTimePeriodEvalDeltaConst timerPeriodConst = timePeriod.ConstEvaluator(null);
                        IntervalDeltaExprEvaluator eval = new ProxyIntervalDeltaExprEvaluator
                        {
                            ProcEvaluate = (reference, eventsPerStream, isNewData, context) =>
                            {
                                return timerPeriodConst.DeltaAdd(reference);
                            },
                        };
                        return new ExprOptionalConstant(eval, null);
                    }
                    // has-month and not constant
                    else
                    {
                        ExprTimePeriodEvalDeltaNonConst timerPeriodNonConst = timePeriod.NonconstEvaluator();
                        IntervalDeltaExprEvaluator eval = new ProxyIntervalDeltaExprEvaluator
                        {
                            ProcEvaluate = (reference, eventsPerStream, isNewData, context) => timerPeriodNonConst.DeltaAdd(
                                reference, eventsPerStream, isNewData, context),
                        };
                        return new ExprOptionalConstant(eval, null);
                    }
                }
            }
            else if (ExprNodeUtility.IsConstantValueExpr(exprNode))
            {
                var constantNode = (ExprConstantNode)exprNode;
                long l = constantNode.GetConstantValue(null).AsLong();
                IntervalDeltaExprEvaluator eval = new ProxyIntervalDeltaExprEvaluator
                {
                    ProcEvaluate = (reference, eventsPerStream, isNewData, context) => l,
                };
                return new ExprOptionalConstant(eval, l);
            }
            else
            {
                var evaluator = exprNode.ExprEvaluator;
                IntervalDeltaExprEvaluator eval = new ProxyIntervalDeltaExprEvaluator
                {
                    ProcEvaluate = (reference, eventsPerStream, isNewData, context) => evaluator.Evaluate(
                        new EvaluateParams(eventsPerStream, isNewData, context)).AsLong(),
                };
                return new ExprOptionalConstant(eval, null);
            }
        }

        /// <summary>
        /// After.
        /// </summary>
        public class IntervalComputerConstantAfter
            : IntervalComputerConstantBase
            , IntervalComputer
        {
            public IntervalComputerConstantAfter(IntervalStartEndParameterPair pair)
                : base(pair, true)
            {
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
                return ComputeInternal(leftStart, leftEnd, rightStart, rightEnd, Start, End);
            }

            public static Boolean ComputeInternal(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                long start,
                long end)
            {
                var delta = leftStart - rightEnd;
                return start <= delta && delta <= end;
            }
        }

        public class IntervalComputerAfterWithDeltaExpr : IntervalComputer
        {
            private readonly IntervalDeltaExprEvaluator _start;
            private readonly IntervalDeltaExprEvaluator _finish;

            public IntervalComputerAfterWithDeltaExpr(IntervalStartEndParameterPair pair)
            {
                _start = pair.Start.Evaluator;
                _finish = pair.End.Evaluator;
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
                var rangeStartDelta = _start.Evaluate(rightStart, eventsPerStream, newData, context);
                var rangeEndDelta = _finish.Evaluate(rightStart, eventsPerStream, newData, context);
                if (rangeStartDelta > rangeEndDelta)
                {
                    return IntervalComputerConstantAfter.ComputeInternal(
                        leftStart, leftEnd, rightStart, rightEnd, rangeEndDelta, rangeStartDelta);
                }
                else
                {
                    return IntervalComputerConstantAfter.ComputeInternal(
                        leftStart, leftEnd, rightStart, rightEnd, rangeStartDelta, rangeEndDelta);
                }
            }
        }

        public class IntervalComputerAfterNoParam : IntervalComputer
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
        }

        /// <summary>
        /// Before.
        /// </summary>
        public class IntervalComputerConstantBefore
            : IntervalComputerConstantBase
            , IntervalComputer
        {
            public IntervalComputerConstantBefore(IntervalStartEndParameterPair pair)
                : base(pair, true)
            {
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
                return ComputeInternal(leftStart, leftEnd, rightStart, Start, End);
            }

            public static Boolean ComputeInternal(long left, long leftEnd, long right, long start, long end)
            {
                var delta = right - leftEnd;
                return start <= delta && delta <= end;
            }
        }

        public class IntervalComputerBeforeWithDeltaExpr : IntervalComputer
        {
            private readonly IntervalDeltaExprEvaluator _start;
            private readonly IntervalDeltaExprEvaluator _finish;

            public IntervalComputerBeforeWithDeltaExpr(IntervalStartEndParameterPair pair)
            {
                _start = pair.Start.Evaluator;
                _finish = pair.End.Evaluator;
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
                var rangeStartDelta = _start.Evaluate(leftEnd, eventsPerStream, newData, context);
                var rangeEndDelta = _finish.Evaluate(leftEnd, eventsPerStream, newData, context);
                if (rangeStartDelta > rangeEndDelta)
                {
                    return IntervalComputerConstantBefore.ComputeInternal(
                        leftStart, leftEnd, rightStart, rangeEndDelta, rangeStartDelta);
                }
                else
                {
                    return IntervalComputerConstantBefore.ComputeInternal(
                        leftStart, leftEnd, rightStart, rangeStartDelta, rangeEndDelta);
                }
            }
        }

        public class IntervalComputerBeforeNoParam : IntervalComputer
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
                return leftEnd < rightStart;
            }
        }

        /// <summary>
        /// Coincides.
        /// </summary>
        public class IntervalComputerConstantCoincides : IntervalComputer
        {
            protected readonly long Start;
            protected readonly long End;

            public IntervalComputerConstantCoincides(IntervalStartEndParameterPair pair)
            {
                Start = pair.Start.OptionalConstant.GetValueOrDefault();
                End = pair.End.OptionalConstant.GetValueOrDefault();
                if (Start < 0 || End < 0)
                {
                    throw new ExprValidationException(
                        "The coincides date-time method does not allow negative start and end values");
                }
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
                return ComputeInternal(leftStart, leftEnd, rightStart, rightEnd, Start, End);
            }

            public static Boolean ComputeInternal(
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

        public class IntervalComputerCoincidesWithDeltaExpr : IntervalComputer
        {
            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            private readonly IntervalDeltaExprEvaluator _start;
            private readonly IntervalDeltaExprEvaluator _finish;

            public IntervalComputerCoincidesWithDeltaExpr(IntervalStartEndParameterPair pair)
            {
                _start = pair.Start.Evaluator;
                _finish = pair.End.Evaluator;
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
                var startValue = _start.Evaluate(Math.Min(leftStart, rightStart), eventsPerStream, newData, context);
                var endValue = _finish.Evaluate(Math.Min(leftEnd, rightEnd), eventsPerStream, newData, context);

                if (startValue < 0 || endValue < 0)
                {
                    Log.Warn("The coincides date-time method does not allow negative start and end values");
                    return null;
                }

                return IntervalComputerConstantCoincides.ComputeInternal(
                    leftStart, leftEnd, rightStart, rightEnd, startValue, endValue);
            }
        }

        public class IntervalComputerCoincidesNoParam : IntervalComputer
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
                return leftStart == rightStart && leftEnd == rightEnd;
            }
        }

        /// <summary>
        /// During And Includes.
        /// </summary>
        public class IntervalComputerDuringNoParam : IntervalComputer
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
        }

        public class IntervalComputerIncludesNoParam : IntervalComputer
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
                return leftStart < rightStart && rightEnd < leftEnd;
            }
        }

        public class IntervalComputerDuringAndIncludesThreshold : IntervalComputer
        {
            private readonly bool _during;
            private readonly IntervalDeltaExprEvaluator _threshold;

            public IntervalComputerDuringAndIncludesThreshold(bool during, IntervalDeltaExprEvaluator threshold)
            {
                _during = during;
                _threshold = threshold;
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

                var thresholdValue = _threshold.Evaluate(leftStart, eventsPerStream, newData, context);

                if (_during)
                {
                    var deltaStart = leftStart - rightStart;
                    if (deltaStart <= 0 || deltaStart > thresholdValue)
                    {
                        return false;
                    }

                    var deltaEnd = rightEnd - leftEnd;
                    return !(deltaEnd <= 0 || deltaEnd > thresholdValue);
                }
                else
                {
                    var deltaStart = rightStart - leftStart;
                    if (deltaStart <= 0 || deltaStart > thresholdValue)
                    {
                        return false;
                    }

                    var deltaEnd = leftEnd - rightEnd;
                    return !(deltaEnd <= 0 || deltaEnd > thresholdValue);
                }
            }
        }

        public class IntervalComputerDuringAndIncludesMinMax : IntervalComputer
        {
            private readonly bool _during;
            private readonly IntervalDeltaExprEvaluator _minEval;
            private readonly IntervalDeltaExprEvaluator _maxEval;

            public IntervalComputerDuringAndIncludesMinMax(
                bool during,
                IntervalDeltaExprEvaluator minEval,
                IntervalDeltaExprEvaluator maxEval)
            {
                _during = during;
                _minEval = minEval;
                _maxEval = maxEval;
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
                var min = _minEval.Evaluate(leftStart, eventsPerStream, newData, context);
                var max = _maxEval.Evaluate(rightEnd, eventsPerStream, newData, context);
                if (_during)
                {
                    return ComputeInternalDuring(leftStart, leftEnd, rightStart, rightEnd, min, max, min, max);
                }
                else
                {
                    return ComputeInternalIncludes(leftStart, leftEnd, rightStart, rightEnd, min, max, min, max);
                }
            }

            public static bool ComputeInternalDuring(
                long left,
                long leftEnd,
                long right,
                long rightEnd,
                long startMin,
                long startMax,
                long endMin,
                long endMax)
            {
                if (startMin <= 0)
                {
                    startMin = 1;
                }
                var deltaStart = left - right;
                if (deltaStart < startMin || deltaStart > startMax)
                {
                    return false;
                }

                var deltaEnd = rightEnd - leftEnd;
                return !(deltaEnd < endMin || deltaEnd > endMax);
            }

            public static bool ComputeInternalIncludes(
                long left,
                long leftEnd,
                long right,
                long rightEnd,
                long startMin,
                long startMax,
                long endMin,
                long endMax)
            {
                if (startMin <= 0)
                {
                    startMin = 1;
                }
                var deltaStart = right - left;
                if (deltaStart < startMin || deltaStart > startMax)
                {
                    return false;
                }

                var deltaEnd = leftEnd - rightEnd;
                return !(deltaEnd < endMin || deltaEnd > endMax);
            }
        }

        public class IntervalComputerDuringMinMaxStartEnd : IntervalComputer
        {
            private readonly bool _during;
            private readonly IntervalDeltaExprEvaluator _minStartEval;
            private readonly IntervalDeltaExprEvaluator _maxStartEval;
            private readonly IntervalDeltaExprEvaluator _minEndEval;
            private readonly IntervalDeltaExprEvaluator _maxEndEval;

            public IntervalComputerDuringMinMaxStartEnd(bool during, IntervalDeltaExprEvaluator[] parameters)
            {
                _during = during;
                _minStartEval = parameters[0];
                _maxStartEval = parameters[1];
                _minEndEval = parameters[2];
                _maxEndEval = parameters[3];
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

                var minStart = _minStartEval.Evaluate(rightStart, eventsPerStream, newData, context);
                var maxStart = _maxStartEval.Evaluate(rightStart, eventsPerStream, newData, context);
                var minEnd = _minEndEval.Evaluate(rightEnd, eventsPerStream, newData, context);
                var maxEnd = _maxEndEval.Evaluate(rightEnd, eventsPerStream, newData, context);

                if (_during)
                {
                    return IntervalComputerDuringAndIncludesMinMax.ComputeInternalDuring(
                        leftStart, leftEnd, rightStart, rightEnd, minStart, maxStart, minEnd, maxEnd);
                }
                else
                {
                    return IntervalComputerDuringAndIncludesMinMax.ComputeInternalIncludes(
                        leftStart, leftEnd, rightStart, rightEnd, minStart, maxStart, minEnd, maxEnd);
                }
            }
        }

        /// <summary>
        /// Finishes.
        /// </summary>
        public class IntervalComputerFinishesNoParam : IntervalComputer
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
                return rightStart < leftStart && (leftEnd == rightEnd);
            }
        }

        public class IntervalComputerFinishesThreshold : IntervalComputer
        {
            private static readonly ILog Log =
                LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            private readonly IntervalDeltaExprEvaluator _thresholdExpr;

            public IntervalComputerFinishesThreshold(IntervalDeltaExprEvaluator thresholdExpr)
            {
                _thresholdExpr = thresholdExpr;
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

                var threshold = _thresholdExpr.Evaluate(Math.Min(leftEnd, rightEnd), eventsPerStream, newData, context);

                if (threshold < 0)
                {
                    Log.Warn("The 'finishes' date-time method does not allow negative threshold");
                    return null;
                }

                if (rightStart >= leftStart)
                {
                    return false;
                }
                var delta = Math.Abs(leftEnd - rightEnd);
                return delta <= threshold;
            }
        }

        /// <summary>
        /// Finishes-By.
        /// </summary>
        public class IntervalComputerFinishedByNoParam : IntervalComputer
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
                return leftStart < rightStart && (leftEnd == rightEnd);
            }
        }

        public class IntervalComputerFinishedByThreshold : IntervalComputer
        {
            private static readonly ILog Log =
                LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            private readonly IntervalDeltaExprEvaluator _thresholdExpr;

            public IntervalComputerFinishedByThreshold(IntervalDeltaExprEvaluator thresholdExpr)
            {
                _thresholdExpr = thresholdExpr;
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

                var threshold = _thresholdExpr.Evaluate(Math.Min(rightEnd, leftEnd), eventsPerStream, newData, context);
                if (threshold < 0)
                {
                    Log.Warn("The 'finishes' date-time method does not allow negative threshold");
                    return null;
                }

                if (leftStart >= rightStart)
                {
                    return false;
                }
                var delta = Math.Abs(leftEnd - rightEnd);
                return delta <= threshold;
            }
        }

        /// <summary>
        /// Meets.
        /// </summary>
        public class IntervalComputerMeetsNoParam : IntervalComputer
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
        }

        public class IntervalComputerMeetsThreshold : IntervalComputer
        {
            private static readonly ILog Log =
                LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            private readonly IntervalDeltaExprEvaluator _thresholdExpr;

            public IntervalComputerMeetsThreshold(IntervalDeltaExprEvaluator thresholdExpr)
            {
                _thresholdExpr = thresholdExpr;
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
                var threshold = _thresholdExpr.Evaluate(
                    Math.Min(leftEnd, rightStart), eventsPerStream, newData, context);
                if (threshold < 0)
                {
                    Log.Warn("The 'finishes' date-time method does not allow negative threshold");
                    return null;
                }

                var delta = Math.Abs(rightStart - leftEnd);
                return delta <= threshold;
            }
        }

        /// <summary>
        /// Met-By.
        /// </summary>
        public class IntervalComputerMetByNoParam : IntervalComputer
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
        }

        public class IntervalComputerMetByThreshold : IntervalComputer
        {
            private static readonly ILog Log =
                LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            private readonly IntervalDeltaExprEvaluator _thresholdExpr;

            public IntervalComputerMetByThreshold(IntervalDeltaExprEvaluator thresholdExpr)
            {
                _thresholdExpr = thresholdExpr;
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

                var threshold = _thresholdExpr.Evaluate(
                    Math.Min(leftStart, rightEnd), eventsPerStream, newData, context);

                if (threshold < 0)
                {
                    Log.Warn("The 'finishes' date-time method does not allow negative threshold");
                    return null;
                }

                var delta = Math.Abs(leftStart - rightEnd);
                return delta <= threshold;
            }
        }

        /// <summary>
        /// Overlaps.
        /// </summary>
        public class IntervalComputerOverlapsNoParam : IntervalComputer
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
                return (leftStart < rightStart) &&
                       (rightStart < leftEnd) &&
                       (leftEnd < rightEnd);
            }
        }

        public class IntervalComputerOverlapsAndByThreshold : IntervalComputer
        {
            private readonly bool _overlaps;
            private readonly IntervalDeltaExprEvaluator _thresholdExpr;

            public IntervalComputerOverlapsAndByThreshold(bool overlaps, IntervalDeltaExprEvaluator thresholdExpr)
            {
                _overlaps = overlaps;
                _thresholdExpr = thresholdExpr;
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

                if (_overlaps)
                {
                    var threshold = _thresholdExpr.Evaluate(leftStart, eventsPerStream, newData, context);
                    return ComputeInternalOverlaps(leftStart, leftEnd, rightStart, rightEnd, 0, threshold);
                }
                else
                {
                    var threshold = _thresholdExpr.Evaluate(rightStart, eventsPerStream, newData, context);
                    return ComputeInternalOverlaps(rightStart, rightEnd, leftStart, leftEnd, 0, threshold);
                }
            }

            public static bool ComputeInternalOverlaps(
                long left,
                long leftEnd,
                long right,
                long rightEnd,
                long min,
                long max)
            {
                var match = ((left < right) &&
                              (right < leftEnd) &&
                              (leftEnd < rightEnd));
                if (!match)
                {
                    return false;
                }
                var delta = leftEnd - right;
                return min <= delta && delta <= max;
            }
        }

        public class IntervalComputerOverlapsAndByMinMax : IntervalComputer
        {
            private readonly bool _overlaps;
            private readonly IntervalDeltaExprEvaluator _minEval;
            private readonly IntervalDeltaExprEvaluator _maxEval;

            public IntervalComputerOverlapsAndByMinMax(
                bool overlaps,
                IntervalDeltaExprEvaluator minEval,
                IntervalDeltaExprEvaluator maxEval)
            {
                _overlaps = overlaps;
                _minEval = minEval;
                _maxEval = maxEval;
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

                if (_overlaps)
                {
                    var minThreshold = _minEval.Evaluate(leftStart, eventsPerStream, newData, context);
                    var maxThreshold = _maxEval.Evaluate(leftEnd, eventsPerStream, newData, context);
                    return IntervalComputerOverlapsAndByThreshold.ComputeInternalOverlaps(
                        leftStart, leftEnd, rightStart, rightEnd, minThreshold, maxThreshold);
                }
                else
                {
                    var minThreshold = _minEval.Evaluate(rightStart, eventsPerStream, newData, context);
                    var maxThreshold = _maxEval.Evaluate(rightEnd, eventsPerStream, newData, context);
                    return IntervalComputerOverlapsAndByThreshold.ComputeInternalOverlaps(
                        rightStart, rightEnd, leftStart, leftEnd, minThreshold, maxThreshold);
                }
            }
        }

        /// <summary>
        /// OverlappedBy.
        /// </summary>
        public class IntervalComputerOverlappedByNoParam : IntervalComputer
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
                return (rightStart < leftStart) &&
                       (leftStart < rightEnd) &&
                       (rightEnd < leftEnd);
            }
        }

        /// <summary>
        /// Starts.
        /// </summary>
        public class IntervalComputerStartsNoParam : IntervalComputer
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
                return (leftStart == rightStart) && (leftEnd < rightEnd);
            }
        }

        public class IntervalComputerStartsThreshold : IntervalComputer
        {
            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            private readonly IntervalDeltaExprEvaluator _thresholdExpr;

            public IntervalComputerStartsThreshold(IntervalDeltaExprEvaluator thresholdExpr)
            {
                _thresholdExpr = thresholdExpr;
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

                var threshold = _thresholdExpr.Evaluate(
                    Math.Min(leftStart, rightStart), eventsPerStream, newData, context);
                if (threshold < 0)
                {
                    Log.Warn("The 'finishes' date-time method does not allow negative threshold");
                    return null;
                }

                var delta = Math.Abs(leftStart - rightStart);
                return delta <= threshold && (leftEnd < rightEnd);
            }
        }

        /// <summary>
        /// Started-by.
        /// </summary>
        public class IntervalComputerStartedByNoParam : IntervalComputer
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
                return (leftStart == rightStart) && (leftEnd > rightEnd);
            }
        }

        public class IntervalComputerStartedByThreshold : IntervalComputer
        {
            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            private readonly IntervalDeltaExprEvaluator _thresholdExpr;

            public IntervalComputerStartedByThreshold(IntervalDeltaExprEvaluator thresholdExpr)
            {
                _thresholdExpr = thresholdExpr;
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

                var threshold = _thresholdExpr.Evaluate(
                    Math.Min(leftStart, rightStart), eventsPerStream, newData, context);
                if (threshold < 0)
                {
                    Log.Warn("The 'finishes' date-time method does not allow negative threshold");
                    return null;
                }

                var delta = Math.Abs(leftStart - rightStart);
                return delta <= threshold && (leftEnd > rightEnd);
            }
        }
    }
}
