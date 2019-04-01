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
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.expression.subquery
{
    public class SubselectEvalStrategyNRExistsDefault : SubselectEvalStrategyNR
    {
        private readonly ExprEvaluator _filterEval;
        private readonly ExprEvaluator _havingEval;

        public SubselectEvalStrategyNRExistsDefault(ExprEvaluator filterEval, ExprEvaluator havingEval)
        {
            _filterEval = filterEval;
            _havingEval = havingEval;
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
            if (_filterEval == null && _havingEval == null)
            {
                return true;
            }

            EventBean[] events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var evaluateParams = new EvaluateParams(events, true, exprEvaluatorContext);

            if (_havingEval != null)
            {
                var pass = _havingEval.Evaluate(evaluateParams);
                return (pass != null) && true.Equals(pass);
            }
            else if (_filterEval != null)
            {
                foreach (EventBean subselectEvent in matchingEvents)
                {
                    // Prepare filter expression event list
                    events[0] = subselectEvent;

                    var pass = _filterEval.Evaluate(evaluateParams);
                    if ((pass != null) && true.Equals(pass))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                throw new IllegalStateException("Both filter and having clause encountered");
            }
        }
    }
} // end of namespace
