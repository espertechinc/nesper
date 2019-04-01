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
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.@join.plan;

namespace com.espertech.esper.epl.datetime.reformatop
{
    using DateTimeExEval = Func<DateTimeEx, object>;

    public class ReformatOpEval<TValue> : ReformatOp
    {
        private readonly Func<DateTimeEx, TValue> _dtxEval;
        private readonly TimeZoneInfo _timeZone;
        private readonly TimeAbacus _timeAbacus;

        public ReformatOpEval(
            Func<DateTimeEx, TValue> dtxEval,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
        {
            _dtxEval = dtxEval;
            _timeZone = timeZone;
            _timeAbacus = timeAbacus;
        }

        public Object Evaluate(
            long ts,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            DateTimeEx dtx = DateTimeEx.GetInstance(_timeZone);
            _timeAbacus.CalendarSet(ts, dtx);
            return _dtxEval.Invoke(dtx);
        }

        public Object Evaluate(
            DateTime d,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            DateTimeEx dtx = DateTimeEx.GetInstance(_timeZone, d);
            return _dtxEval.Invoke(dtx);
        }

        public Object Evaluate(
            DateTimeOffset d,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            DateTimeEx dtx = DateTimeEx.GetInstance(_timeZone, d);
            return _dtxEval.Invoke(dtx);
        }

        public Object Evaluate(
            DateTimeEx dtx,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return _dtxEval.Invoke(dtx);
        }

        public Type ReturnType => typeof(TValue);

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
