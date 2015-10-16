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
    public class ReformatOpBetweenConstantParams : ReformatOp
    {
        private readonly long _first;
        private readonly long _second;

        public ReformatOpBetweenConstantParams(IList<ExprNode> parameters)
        {
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

        private static long GetLongValue(ExprNode exprNode)
        {
            Object value = exprNode.ExprEvaluator.Evaluate(new EvaluateParams(null, true, null));
            if (value == null)
            {
                throw new ExprValidationException("Date-time method 'between' requires non-null parameter values");
            }
            return DatetimeLongCoercerFactory.GetCoercer(value.GetType()).Coerce(value);
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
            return EvaluateInternal(ts);
        }

        public object Evaluate(DateTimeOffset d, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(d.TimeInMillis());
        }

        public Object EvaluateInternal(long ts)
        {
            return _first <= ts && ts <= _second;
        }

        public Type ReturnType
        {
            get { return typeof(bool?); }
        }

        public ExprDotNodeFilterAnalyzerDesc GetFilterDesc(EventType[] typesPerStream,
                                                           DatetimeMethodEnum currentMethod,
                                                           ICollection<ExprNode> currentParameters,
                                                           ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            return null;
        }
    }
}
