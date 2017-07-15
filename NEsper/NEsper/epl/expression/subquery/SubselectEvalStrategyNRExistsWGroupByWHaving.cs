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
using com.espertech.esper.events;

namespace com.espertech.esper.epl.expression.subquery
{
    public class SubselectEvalStrategyNRExistsWGroupByWHaving : SubselectEvalStrategyNR
    {
        private readonly ExprEvaluator _havingEval;

        public SubselectEvalStrategyNRExistsWGroupByWHaving(ExprEvaluator havingEval)
        {
            _havingEval = havingEval;
        }

        public Object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationService aggregationServiceAnyPartition)
        {
            if (matchingEvents == null || matchingEvents.Count == 0)
            {
                return false;
            }
            var aggregationService =
                aggregationServiceAnyPartition.GetContextPartitionAggregationService(
                    exprEvaluatorContext.AgentInstanceId);
            var groupKeys = aggregationService.GetGroupKeys(exprEvaluatorContext);
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var evaluateParams = new EvaluateParams(events, true, exprEvaluatorContext);

            foreach (var groupKey in groupKeys)
            {
                aggregationService.SetCurrentAccess(groupKey, exprEvaluatorContext.AgentInstanceId, null);

                var pass = _havingEval.Evaluate(evaluateParams);
                if ((pass == null) || (false.Equals(pass)))
                {
                    continue;
                }
                return true;
            }
            return false;
        }
    }
} // end of namespace
