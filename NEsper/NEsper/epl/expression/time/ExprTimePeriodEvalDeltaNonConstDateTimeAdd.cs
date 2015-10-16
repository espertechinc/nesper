///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class ExprTimePeriodEvalDeltaNonConstDateTimeAdd : ExprTimePeriodEvalDeltaNonConst
    {
        private DateTimeEx _dateTime;
        private readonly ExprTimePeriodImpl _parent;

        public ExprTimePeriodEvalDeltaNonConstDateTimeAdd(TimeZoneInfo timeZone, ExprTimePeriodImpl parent)
        {
            _parent = parent;
            _dateTime = new DateTimeEx(DateTimeOffsetHelper.Now(timeZone), timeZone);
        }
    
        public long DeltaMillisecondsAdd(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            lock (this)
            {
                _dateTime.SetUtcMillis(currentTime);
                AddSubtract(_parent, _dateTime, 1, eventsPerStream, isNewData, context);
                return _dateTime.TimeInMillis - currentTime;
            }
        }
    
        public long DeltaMillisecondsSubtract(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            lock (this)
            {
                _dateTime.SetUtcMillis(currentTime);
                AddSubtract(_parent, _dateTime, -1, eventsPerStream, isNewData, context);
                return _dateTime.TimeInMillis - currentTime;
            }
        }
    
        public long DeltaMillisecondsUseEngineTime(EventBean[] eventsPerStream, AgentInstanceContext agentInstanceContext)
        {
            lock (this)
            {
                long currentTime = agentInstanceContext.StatementContext.SchedulingService.Time;
                _dateTime.SetUtcMillis(currentTime);
                AddSubtract(_parent, _dateTime, 1, eventsPerStream, true, agentInstanceContext);
                return _dateTime.TimeInMillis - currentTime;
            }
        }

        public ExprTimePeriodEvalDeltaResult DeltaMillisecondsAddWReference(long current, long reference, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            lock (this)
            {
                // find the next-nearest reference higher then the current time, compute delta, return reference one lower
                if (reference > current)
                {
                    while (reference > current)
                    {
                        reference = reference -
                                    DeltaMillisecondsSubtract(reference, eventsPerStream, isNewData, context);
                    }
                }

                long next = reference;
                long last;
                do
                {
                    last = next;
                    next = next + DeltaMillisecondsAdd(last, eventsPerStream, isNewData, context);
                } while (next <= current);
                return new ExprTimePeriodEvalDeltaResult(next - current, last);
            }
        }
    
        private void AddSubtract(ExprTimePeriodImpl parent, DateTimeEx dateTime, int factor, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext context)
        {
            var adders = parent.Adders;
            var evaluators = parent.Evaluators;
            var evaluateParams = new EvaluateParams(eventsPerStream, newData, context);
            for (int i = 0; i < adders.Length; i++)
            {
                var value = evaluators[i].Evaluate(evaluateParams).AsInt();
                adders[i].Add(dateTime, factor * value);
            }
        }
    }
}
