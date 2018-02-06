///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.time
{
    public class ExprTimePeriodEvalDeltaNonConstDtxAdd : ExprTimePeriodEvalDeltaNonConst
    {
        private readonly DateTimeEx _dateTime;
        private readonly ExprTimePeriodImpl _parent;
        private readonly int _indexMicroseconds;

        public ExprTimePeriodEvalDeltaNonConstDtxAdd(TimeZoneInfo timeZone, ExprTimePeriodImpl parent)
        {
            _parent = parent;
            _dateTime = new DateTimeEx(DateTimeOffsetHelper.Now(timeZone), timeZone);
            _indexMicroseconds = ExprTimePeriodUtil.FindIndexMicroseconds(parent.Adders);
        }

        public long DeltaAdd(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            lock (this)
            {
                return AddSubtract(currentTime, 1, eventsPerStream, isNewData, context);
            }
        }
    
        public long DeltaSubtract(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            lock (this)
            {
                return AddSubtract(currentTime, -1, eventsPerStream, isNewData, context);
            }
        }
    
        public long DeltaUseEngineTime(EventBean[] eventsPerStream, AgentInstanceContext agentInstanceContext)
        {
            lock (this)
            {
                long currentTime = agentInstanceContext.StatementContext.SchedulingService.Time;
                return AddSubtract(currentTime, 1, eventsPerStream, true, agentInstanceContext);
            }
        }

        public ExprTimePeriodEvalDeltaResult DeltaAddWReference(long current, long reference, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            lock (this)
            {
                // find the next-nearest reference higher then the current time, compute delta, return reference one lower
                if (reference > current)
                {
                    while (reference > current)
                    {
                        reference = reference - DeltaSubtract(reference, eventsPerStream, isNewData, context);
                    }
                }

                long next = reference;
                long last;
                do
                {
                    last = next;
                    next = next + DeltaAdd(last, eventsPerStream, isNewData, context);
                } while (next <= current);
                return new ExprTimePeriodEvalDeltaResult(next - current, last);
            }
        }
    
        private long AddSubtract(long currentTime, int factor, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext context)
        {
            var remainder = _parent.TimeAbacus.CalendarSet(currentTime, _dateTime);
            var adders = _parent.Adders;
            var evaluators = _parent.Evaluators;
            var evaluateParams = new EvaluateParams(eventsPerStream, newData, context);
            var usec = 0;
            for (int i = 0; i < adders.Length; i++)
            {
                var value = evaluators[i].Evaluate(evaluateParams).AsInt();
                if (i == _indexMicroseconds)
                {
                    usec = value;
                }
                else
                {
                    adders[i].Add(_dateTime, factor * value);
                }
            }

            long result = _parent.TimeAbacus.CalendarGet(_dateTime, remainder);
            if (_indexMicroseconds != -1)
            {
                result += factor * usec;
            }
            return result - currentTime;
        }
    }
}
