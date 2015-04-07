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
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.subquery
{
    public class SubselectAggregationPreprocessorUnfilteredGrouped : SubselectAggregationPreprocessorBase
    {
        public SubselectAggregationPreprocessorUnfilteredGrouped(
            AggregationService aggregationService,
            ExprEvaluator filterExpr,
            ExprEvaluator[] groupKeys)
            : base(aggregationService, filterExpr, groupKeys)
        {
        }

        public override void Evaluate(
            EventBean[] eventsPerStream,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            AggregationService.ClearResults(exprEvaluatorContext);
            if (matchingEvents == null)
            {
                return;
            }

            var events = new EventBean[eventsPerStream.Length + 1];
            Array.Copy(eventsPerStream, 0, events, 1, eventsPerStream.Length);

            foreach (EventBean subselectEvent in matchingEvents)
            {
                events[0] = subselectEvent;
                var groupKey = GenerateGroupKey(events, true, exprEvaluatorContext);
                AggregationService.ApplyEnter(events, groupKey, exprEvaluatorContext);
            }
        }
    }
}