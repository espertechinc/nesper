///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.@join.plan;

namespace com.espertech.esper.epl.datetime.reformatop
{
    public class ReformatOpBetweenNonConstantParams : ReformatOp
    {
        private readonly ExprNode _start;
        private readonly ExprEvaluator _startEval;
        private readonly DatetimeLongCoercer _startCoercer;
        private readonly ExprNode _end;
        private readonly ExprEvaluator _endEval;
        private readonly DatetimeLongCoercer _secondCoercer;
        private readonly TimeZoneInfo _timeZone;

        private readonly bool _includeBoth;
        private readonly bool? _includeLow;
        private readonly bool? _includeHigh;
        private readonly ExprEvaluator _evalIncludeLow;
        private readonly ExprEvaluator _evalIncludeHigh;

        public ReformatOpBetweenNonConstantParams(IList<ExprNode> parameters, TimeZoneInfo timeZone)
        {
            _timeZone = timeZone;
            _start = parameters[0];
            _startEval = _start.ExprEvaluator;
            _startCoercer = DatetimeLongCoercerFactory.GetCoercer(_startEval.ReturnType, timeZone);
            _end = parameters[1];
            _endEval = _end.ExprEvaluator;
            _secondCoercer = DatetimeLongCoercerFactory.GetCoercer(_endEval.ReturnType, timeZone);

            if (parameters.Count == 2)
            {
                _includeBoth = true;
                _includeLow = true;
                _includeHigh = true;
            }
            else
            {
                if (parameters[2].IsConstantResult)
                {
                    _includeLow = GetBooleanValue(parameters[2]);
                }
                else
                {
                    _evalIncludeLow = parameters[2].ExprEvaluator;
                }
                if (parameters[3].IsConstantResult)
                {
                    _includeHigh = GetBooleanValue(parameters[3]);
                }
                else
                {
                    _evalIncludeHigh = parameters[3].ExprEvaluator;
                }
                if (_includeLow != null && _includeHigh != null && _includeLow.Value && _includeHigh.Value)
                {
                    _includeBoth = true;
                }
            }
        }

        private bool GetBooleanValue(ExprNode exprNode)
        {
            var value = exprNode.ExprEvaluator.Evaluate(new EvaluateParams(null, true, null));
            if (value == null)
            {
                throw new ExprValidationException("Date-time method 'between' requires non-null parameter values");
            }
            return (bool) value;
        }

        public Object Evaluate(
            long ts,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(ts, eventsPerStream, newData, exprEvaluatorContext);
        }

        public Object Evaluate(
            DateTime d,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(d.UtcMillis(), eventsPerStream, newData, exprEvaluatorContext);
        }

        public Object Evaluate(
            DateTimeOffset d,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(d.TimeInMillis(), eventsPerStream, newData, exprEvaluatorContext);
        }

        public Object Evaluate(
            DateTimeEx dtx,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (dtx == null)
            {
                return null;
            }
            return EvaluateInternal(dtx.TimeInMillis, eventsPerStream, newData, exprEvaluatorContext);
        }

        public Type ReturnType
        {
            get { return typeof (bool?); }
        }

        public Object EvaluateInternal(
            long ts,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, newData, exprEvaluatorContext);
            var firstObj = _startEval.Evaluate(evaluateParams);
            if (firstObj == null)
            {
                return null;
            }
            var secondObj = _endEval.Evaluate(evaluateParams);
            if (secondObj == null)
            {
                return null;
            }
            var first = _startCoercer.Coerce(firstObj);
            var second = _secondCoercer.Coerce(secondObj);
            if (_includeBoth)
            {
                if (first <= second)
                {
                    return first <= ts && ts <= second;
                }
                else
                {
                    return second <= ts && ts <= first;
                }
            }
            else
            {

                bool includeLowEndpoint;
                if (_includeLow != null)
                {
                    includeLowEndpoint = _includeLow.Value;
                }
                else
                {
                    var value = _evalIncludeLow.Evaluate(evaluateParams);
                    if (value == null)
                    {
                        return null;
                    }
                    includeLowEndpoint = (bool) value;

                }

                bool includeHighEndpoint;
                if (_includeHigh != null)
                {
                    includeHighEndpoint = _includeHigh.Value;
                }
                else
                {
                    var value = _evalIncludeHigh.Evaluate(evaluateParams);
                    if (value == null)
                    {
                        return null;
                    }
                    includeHighEndpoint = (bool) value;

                }

                if (includeLowEndpoint)
                {
                    if (ts < first)
                    {
                        return false;
                    }
                }
                else
                {
                    if (ts <= first)
                    {
                        return false;
                    }
                }

                if (includeHighEndpoint)
                {
                    if (ts > second)
                    {
                        return false;
                    }
                }
                else
                {
                    if (ts >= second)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public FilterExprAnalyzerAffector GetFilterDesc(
            EventType[] typesPerStream,
            DatetimeMethodEnum currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            if (_includeLow == null || _includeHigh == null)
            {
                return null;
            }

            int targetStreamNum;
            string targetProperty;
            if (inputDesc is ExprDotNodeFilterAnalyzerInputStream)
            {
                var targetStream = (ExprDotNodeFilterAnalyzerInputStream) inputDesc;
                targetStreamNum = targetStream.StreamNum;
                var targetType = typesPerStream[targetStreamNum];
                targetProperty = targetType.StartTimestampPropertyName;
            }
            else if (inputDesc is ExprDotNodeFilterAnalyzerInputProp)
            {
                var targetStream = (ExprDotNodeFilterAnalyzerInputProp) inputDesc;
                targetStreamNum = targetStream.StreamNum;
                targetProperty = targetStream.PropertyName;
            }
            else
            {
                return null;
            }

            return new FilterExprAnalyzerDTBetweenAffector(
                typesPerStream, targetStreamNum, targetProperty, _start, _end, _includeLow.Value, _includeHigh.Value);
        }
    }
} // end of namespace
