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
    public class ReformatOpToDateTimeOffset : ReformatOp
    {
        private readonly TimeZoneInfo _timeZone;
        private readonly TimeAbacus _timeAbacus;

        public ReformatOpToDateTimeOffset(TimeZoneInfo timeZone, TimeAbacus timeAbacus)
        {
            _timeZone = timeZone;
            _timeAbacus = timeAbacus;
        }

        public Object Evaluate(long ts, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _timeAbacus.ToDate(ts)
                .DateTime
                .TranslateTo(_timeZone);
        }

        public object Evaluate(DateTime d, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return d.TranslateTo(_timeZone);
        }

        public object Evaluate(DateTimeOffset d, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return d.TranslateTo(_timeZone);
        }

        public object Evaluate(DateTimeEx d, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return d.DateTime.TranslateTo(_timeZone);
        }

        public Type ReturnType
        {
            get { return typeof(DateTimeOffset); }
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
}
