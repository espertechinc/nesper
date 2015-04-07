///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.dot;

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

        private readonly bool? _includeBoth;
        private readonly bool? _includeLow;
        private readonly bool? _includeHigh;
        private readonly ExprEvaluator _evalIncludeLow;
        private readonly ExprEvaluator _evalIncludeHigh;

        public ReformatOpBetweenNonConstantParams(IList<ExprNode> parameters)
        {
            _start = parameters[0];
            _startEval = _start.ExprEvaluator;
            _startCoercer = DatetimeLongCoercerFactory.GetCoercer(_startEval.ReturnType);
            _end = parameters[1];
            _endEval = _end.ExprEvaluator;
            _secondCoercer = DatetimeLongCoercerFactory.GetCoercer(_endEval.ReturnType);

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
                if (_includeLow.GetValueOrDefault() && _includeHigh.GetValueOrDefault())
                {
                    _includeBoth = true;
                }
            }
        }

        private static bool GetBooleanValue(ExprNode exprNode)
        {
            Object value = exprNode.ExprEvaluator.Evaluate(new EvaluateParams(null, true, null));
            if (value == null)
            {
                throw new ExprValidationException("Date-time method 'between' requires non-null parameter values");
            }
            return value.AsBoolean();
        }

        public Object Evaluate(long ts, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(ts, eventsPerStream, newData, exprEvaluatorContext);
        }

        public Object Evaluate(DateTime d, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(d.TimeInMillis(), eventsPerStream, newData, exprEvaluatorContext);
        }

        public Type ReturnType
        {
            get { return typeof(bool?); }
        }

        public Object EvaluateInternal(long ts, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            Object firstObj = _startEval.Evaluate(new EvaluateParams(eventsPerStream, newData, exprEvaluatorContext));
            if (firstObj == null)
            {
                return null;
            }
            Object secondObj = _endEval.Evaluate(new EvaluateParams(eventsPerStream, newData, exprEvaluatorContext));
            if (secondObj == null)
            {
                return null;
            }
            long first = _startCoercer.Coerce(firstObj);
            long second = _secondCoercer.Coerce(secondObj);
            if (_includeBoth.GetValueOrDefault())
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
                    includeLowEndpoint = _includeLow.GetValueOrDefault();
                }
                else
                {
                    Object value = _evalIncludeLow.Evaluate(new EvaluateParams(eventsPerStream, newData, exprEvaluatorContext));
                    if (value == null)
                    {
                        return null;
                    }
                    includeLowEndpoint = value.AsBoolean();

                }

                bool includeHighEndpoint;
                if (_includeHigh != null)
                {
                    includeHighEndpoint = _includeHigh.GetValueOrDefault();
                }
                else
                {
                    Object value = _evalIncludeHigh.Evaluate(new EvaluateParams(eventsPerStream, newData, exprEvaluatorContext));
                    if (value == null)
                    {
                        return null;
                    }
                    includeHighEndpoint = value.AsBoolean();

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

        public ExprDotNodeFilterAnalyzerDesc GetFilterDesc(EventType[] typesPerStream,
                                                           DatetimeMethodEnum currentMethod,
                                                           ICollection<ExprNode> currentParameters,
                                                           ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            if (_includeLow == null || _includeHigh == null)
            {
                return null;
            }

            int targetStreamNum;
            String targetProperty;
            if (inputDesc is ExprDotNodeFilterAnalyzerInputStream)
            {
                var targetStream = (ExprDotNodeFilterAnalyzerInputStream)inputDesc;
                targetStreamNum = targetStream.StreamNum;
                EventType targetType = typesPerStream[targetStreamNum];
                targetProperty = targetType.StartTimestampPropertyName;
            }
            else if (inputDesc is ExprDotNodeFilterAnalyzerInputProp)
            {
                var targetStream = (ExprDotNodeFilterAnalyzerInputProp)inputDesc;
                targetStreamNum = targetStream.StreamNum;
                targetProperty = targetStream.PropertyName;
            }
            else
            {
                return null;
            }

            return new ExprDotNodeFilterAnalyzerDTBetweenDesc(typesPerStream, targetStreamNum, targetProperty, _start,
                                                              _end, _includeLow.GetValueOrDefault(),
                                                              _includeHigh.GetValueOrDefault());
        }
    }
}
