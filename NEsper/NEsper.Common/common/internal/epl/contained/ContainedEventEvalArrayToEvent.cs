///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.contained
{
    public class ContainedEventEvalArrayToEvent : ContainedEventEval
    {
        private readonly ExprEvaluator evaluator;
        private readonly EventBeanManufacturer manufacturer;

        public ContainedEventEvalArrayToEvent(
            ExprEvaluator evaluator,
            EventBeanManufacturer manufacturer)
        {
            this.evaluator = evaluator;
            this.manufacturer = manufacturer;
        }

        public object GetFragment(
            EventBean eventBean,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            Array result = evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext) as Array;
            if (result == null) {
                return null;
            }

            EventBean[] events = new EventBean[result.Length];
            for (int i = 0; i < events.Length; i++) {
                object column = result.GetValue(i);
                if (column != null) {
                    events[i] = manufacturer.Make(new object[] {column});
                }
            }

            return events;
        }
    }
} // end of namespace