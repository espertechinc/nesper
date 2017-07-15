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
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.subquery
{
    public class SubselectAggregationPreprocessorUnfilteredGrouped : SubselectAggregationPreprocessorBase
    {
        public SubselectAggregationPreprocessorUnfilteredGrouped(
            AggregationService aggregationService,
            ExprEvaluator filterEval,
            ExprEvaluator[] groupKeys)
            : base(aggregationService, filterEval, groupKeys)
        {
        }

        public override void Evaluate(
            EventBean[] eventsPerStream,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            aggregationService.ClearResults(exprEvaluatorContext);
            if (matchingEvents == null)
            {
                return;
            }
            var events = new EventBean[eventsPerStream.Length + 1];
            Array.Copy(eventsPerStream, 0, events, 1, eventsPerStream.Length);

            foreach (EventBean subselectEvent in matchingEvents)
            {
                events[0] = subselectEvent;
                Object groupKey = GenerateGroupKey(events, true, exprEvaluatorContext);
                aggregationService.ApplyEnter(events, groupKey, exprEvaluatorContext);
            }
        }
    }
} // end of namespace