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
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.time
{
    public class ExprTimePeriodEvalDeltaNonConstCalAdd : ExprTimePeriodEvalDeltaNonConst {
        private readonly Calendar cal;
        private readonly ExprTimePeriodImpl parent;
        private readonly int indexMicroseconds;
    
        public ExprTimePeriodEvalDeltaNonConstCalAdd(TimeZone timeZone, ExprTimePeriodImpl parent) {
            this.parent = parent;
            this.cal = Calendar.GetInstance(timeZone);
            this.indexMicroseconds = ExprTimePeriodUtil.FindIndexMicroseconds(parent.Adders);
        }
    
        public synchronized long DeltaAdd(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return AddSubtract(currentTime, 1, eventsPerStream, isNewData, context);
        }
    
        public synchronized long DeltaSubtract(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return AddSubtract(currentTime, -1, eventsPerStream, isNewData, context);
        }
    
        public synchronized long DeltaUseEngineTime(EventBean[] eventsPerStream, AgentInstanceContext agentInstanceContext) {
            long currentTime = agentInstanceContext.StatementContext.SchedulingService.Time;
            return AddSubtract(currentTime, 1, eventsPerStream, true, agentInstanceContext);
        }
    
        public synchronized ExprTimePeriodEvalDeltaResult DeltaAddWReference(long current, long reference, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            // find the next-nearest reference higher then the current time, compute delta, return reference one lower
            if (reference > current) {
                while (reference > current) {
                    reference = reference - DeltaSubtract(reference, eventsPerStream, isNewData, context);
                }
            }
    
            long next = reference;
            long last;
            do {
                last = next;
                next = next + DeltaAdd(last, eventsPerStream, isNewData, context);
            } while (next <= current);
            return new ExprTimePeriodEvalDeltaResult(next - current, last);
        }
    
        private long AddSubtract(long currentTime, int factor, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext context) {
            long remainder = parent.TimeAbacus.CalendarSet(currentTime, cal);
    
            ExprTimePeriodImpl.TimePeriodAdder[] adders = parent.Adders;
            ExprEvaluator[] evaluators = parent.Evaluators;
            int usec = 0;
            for (int i = 0; i < adders.Length; i++) {
                int value = ((Number) evaluators[i].Evaluate(eventsPerStream, newData, context)).IntValue();
                if (i == indexMicroseconds) {
                    usec = value;
                } else {
                    adders[i].Add(cal, factor * value);
                }
            }
    
            long result = parent.TimeAbacus.CalendarGet(cal, remainder);
            if (indexMicroseconds != -1) {
                result += factor * usec;
            }
            return result - currentTime;
        }
    }
} // end of namespace
