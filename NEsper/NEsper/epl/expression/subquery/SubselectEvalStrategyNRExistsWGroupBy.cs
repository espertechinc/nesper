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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.subquery
{
    public class SubselectEvalStrategyNRExistsWGroupBy : SubselectEvalStrategyNR
    {
        public static readonly SubselectEvalStrategyNRExistsWGroupBy INSTANCE =
            new SubselectEvalStrategyNRExistsWGroupBy();

        private SubselectEvalStrategyNRExistsWGroupBy()
        {
        }

        public Object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationService aggregationService)
        {
            if (matchingEvents == null || matchingEvents.Count == 0)
            {
                return false;
            }
            return !aggregationService.GetGroupKeys(exprEvaluatorContext).IsEmpty();
        }
    }
} // end of namespace
