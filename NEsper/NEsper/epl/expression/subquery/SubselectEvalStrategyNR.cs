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

namespace com.espertech.esper.epl.expression.subquery
{
    /// <summary>Strategy for evaluation of a subselect.</summary>
    public interface SubselectEvalStrategyNR
    {
        /// <summary>
        /// Evaluate.
        /// </summary>
        /// <param name="eventsPerStream">events per stream</param>
        /// <param name="isNewData">true for new data</param>
        /// <param name="matchingEvents">prefiltered events</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <param name="aggregationService">aggregation service or null if none</param>
        /// <returns>eval result</returns>
        Object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationService aggregationService);
    }
} // end of namespace
