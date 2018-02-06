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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.@join.plan;

namespace com.espertech.esper.epl.datetime.reformatop
{
    public class ReformatOpToDateTime : ReformatOp
    {
    private readonly TimeZoneInfo _timeZone;
        private readonly TimeAbacus _timeAbacus;
    
        public ReformatOpToDateTime(TimeZoneInfo timeZone, TimeAbacus timeAbacus)
        {
            _timeZone = timeZone;
            _timeAbacus = timeAbacus;
        }
    
        public Object Evaluate(long ts, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext) {
            return _timeAbacus.ToDate(ts);
        }
    
        public Object Evaluate(DateTime d, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext) {
            return d;
        }

        public Object Evaluate(DateTimeOffset d, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return d.DateTime;
        }

        public Object Evaluate(DateTimeEx dtx, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return dtx.DateTime.DateTime;
        }

        public Type ReturnType
        {
            get { return typeof (DateTime); }
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
