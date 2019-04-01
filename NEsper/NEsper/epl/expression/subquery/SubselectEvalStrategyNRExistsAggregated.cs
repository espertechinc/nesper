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
    public class SubselectEvalStrategyNRExistsAggregated : SubselectEvalStrategyNR
    {
        private readonly ExprEvaluator _havingEval;
    
        public SubselectEvalStrategyNRExistsAggregated(ExprEvaluator havingEval)
        {
            _havingEval = havingEval;
        }
    
        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, AggregationService aggregationService)
        {
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var pass = _havingEval.Evaluate(new EvaluateParams(events, true, exprEvaluatorContext));
            return pass != null && true.Equals(pass);
        }
    }
} // end of namespace
