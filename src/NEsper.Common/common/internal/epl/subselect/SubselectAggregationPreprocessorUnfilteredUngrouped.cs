///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubselectAggregationPreprocessorUnfilteredUngrouped : SubselectAggregationPreprocessorBase
    {
        public SubselectAggregationPreprocessorUnfilteredUngrouped(
            AggregationService aggregationService,
            ExprEvaluator filterEval,
            ExprEvaluator groupKeys)
            : base(
                aggregationService,
                filterEval,
                groupKeys)
        {
        }

        public override void Evaluate(
            EventBean[] eventsPerStream,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            aggregationService.ClearResults(exprEvaluatorContext);
            if (matchingEvents == null) {
                return;
            }

            var events = new EventBean[eventsPerStream.Length + 1];
            Array.Copy(eventsPerStream, 0, events, 1, eventsPerStream.Length);

            foreach (var subselectEvent in matchingEvents) {
                events[0] = subselectEvent;
                aggregationService.ApplyEnter(events, null, exprEvaluatorContext);
            }
        }
    }
} // end of namespace