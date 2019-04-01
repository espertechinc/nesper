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
    public abstract class SubselectEvalStrategyNRBase : SubselectEvalStrategyNR
    {
        protected readonly ExprEvaluator ValueEval;
        protected readonly ExprEvaluator SelectEval;
        private readonly bool _resultWhenNoMatchingEvents;

        protected SubselectEvalStrategyNRBase(
            ExprEvaluator valueEval,
            ExprEvaluator selectEval,
            bool resultWhenNoMatchingEvents)
        {
            ValueEval = valueEval;
            SelectEval = selectEval;
            _resultWhenNoMatchingEvents = resultWhenNoMatchingEvents;
        }

        protected abstract Object EvaluateInternal(
            Object leftResult,
            EventBean[] events,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationService aggregationService);

        public Object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationService aggregationService)
        {
            if (matchingEvents == null || matchingEvents.Count == 0)
            {
                return _resultWhenNoMatchingEvents;
            }

            var leftResult = ValueEval.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            return EvaluateInternal(
                leftResult, events, isNewData, matchingEvents, exprEvaluatorContext, aggregationService);
        }
    }
} // end of namespace
