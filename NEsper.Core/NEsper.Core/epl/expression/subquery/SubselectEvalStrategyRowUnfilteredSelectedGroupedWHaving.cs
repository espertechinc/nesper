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
using com.espertech.esper.events;

namespace com.espertech.esper.epl.expression.subquery
{
    /// <summary>Represents a subselect in an expression tree.</summary>
    public class SubselectEvalStrategyRowUnfilteredSelectedGroupedWHaving
        : SubselectEvalStrategyRowUnfilteredSelected
        , SubselectEvalStrategyRow
    {
        private readonly ExprEvaluator _havingClause;

        public SubselectEvalStrategyRowUnfilteredSelectedGroupedWHaving(ExprEvaluator havingClause)
        {
            _havingClause = havingClause;
        }

        public override Object Evaluate(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            var aggregationService = parent.SubselectAggregationService.GetContextPartitionAggregationService(
                    exprEvaluatorContext.AgentInstanceId);
            var groupKeys = aggregationService.GetGroupKeys(exprEvaluatorContext);
            if (groupKeys.IsEmpty())
            {
                return null;
            }

            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            bool haveResult = false;
            Object result = null;

            var evaluateParams = new EvaluateParams(events, newData, exprEvaluatorContext);
            foreach (object groupKey in groupKeys)
            {
                aggregationService.SetCurrentAccess(groupKey, exprEvaluatorContext.AgentInstanceId,null);

                var pass = _havingClause.Evaluate(evaluateParams);
                if (true.Equals(pass))
                {
                    if (haveResult)
                    {
                        return null;
                    }

                    result = base.Evaluate(eventsPerStream, newData, matchingEvents, exprEvaluatorContext, parent);
                    haveResult = true;
                }
            }

            return haveResult ? result : null;
        }


        public override ICollection<object> EvaluateGetCollScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext context,
            ExprSubselectRowNode parent)
        {
            return null;
        }

        public override EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext context,
            ExprSubselectRowNode parent)
        {
            return null;
        }

        public override ICollection<EventBean> EvaluateGetCollEvents(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext context,
            ExprSubselectRowNode parent)
        {
            AggregationService aggregationService =
                parent.SubselectAggregationService.GetContextPartitionAggregationService(context.AgentInstanceId);
            ICollection<object> groupKeys = aggregationService.GetGroupKeys(context);
            if (groupKeys.IsEmpty())
            {
                return null;
            }
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var evaluateParams = new EvaluateParams(events, newData, context);
            var result = new ArrayDeque<EventBean>(groupKeys.Count);
            foreach (object groupKey in groupKeys)
            {
                aggregationService.SetCurrentAccess(groupKey, context.AgentInstanceId, null);

                var pass = _havingClause.Evaluate(evaluateParams);
                if (true.Equals(pass))
                {
                    IDictionary<string, object> row = parent.EvaluateRow(events, true, context);
                    EventBean @event = parent.SubselectMultirowType.EventAdapterService.AdapterForTypedMap(
                        row, parent.SubselectMultirowType.EventType);
                    result.Add(@event);
                }
            }
            return result;
        }
    }
} // end of namespace