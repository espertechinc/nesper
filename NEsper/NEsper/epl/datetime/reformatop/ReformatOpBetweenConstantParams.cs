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
    public class ReformatOpBetweenConstantParams : ReformatOp
    {
        private readonly long _first;
        private readonly long _second;
        private readonly TimeZoneInfo _timeZone;

        public ReformatOpBetweenConstantParams(IList<ExprNode> parameters, TimeZoneInfo timeZone)
        {
            this._timeZone = timeZone;

            long paramFirst = GetLongValue(parameters[0]);
            long paramSecond = GetLongValue(parameters[1]);

            if (paramFirst > paramSecond)
            {
                _second = paramFirst;
                _first = paramSecond;
            }
            else
            {
                _first = paramFirst;
                _second = paramSecond;
            }
            if (parameters.Count > 2)
            {
                if (!GetBooleanValue(parameters[2]))
                {
                    _first = _first + 1;
                }
                if (!GetBooleanValue(parameters[3]))
                {
                    _second = _second - 1;
                }
            }
        }

        private long GetLongValue(ExprNode exprNode)
        {
            var value = exprNode.ExprEvaluator.Evaluate(new EvaluateParams(null, true, null));
            if (value == null)
            {
                throw new ExprValidationException("Date-time method 'between' requires non-null parameter values");
            }
            return DatetimeLongCoercerFactory.GetCoercer(value.GetType(), _timeZone).Coerce(value);
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
            return EvaluateInternal(ts);
        }

        public Object Evaluate(
            DateTime d,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(d.UtcMillis());
        }

        public Object Evaluate(
            DateTimeOffset d,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(d.TimeInMillis());
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
            return EvaluateInternal(dtx.TimeInMillis);
        }

        public Object EvaluateInternal(long ts)
        {
            return _first <= ts && ts <= _second;
        }

        public Type ReturnType
        {
            get { return typeof (bool?); }
        }

        public FilterExprAnalyzerAffector GetFilterDesc(
            EventType[] typesPerStream,
            DatetimeMethodEnum currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            return null;
        }
    }
} // end of namespace
