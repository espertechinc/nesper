///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.subquery
{
    /// <summary>Represents a subselect in an expression tree.</summary>
    public class SubselectEvalStrategyRowUnfilteredSelectedGroupedNoHaving
        : SubselectEvalStrategyRowUnfilteredSelected
        , SubselectEvalStrategyRow
    {
        public override Object Evaluate(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            ICollection<Object> groupKeys = parent.SubselectAggregationService.GetGroupKeys(exprEvaluatorContext);
            if (groupKeys.IsEmpty() || groupKeys.Count > 1)
            {
                return null;
            }
            parent.SubselectAggregationService.SetCurrentAccess(
                groupKeys.First(), exprEvaluatorContext.AgentInstanceId, null);

            return base.Evaluate(eventsPerStream, newData, matchingEvents, exprEvaluatorContext, parent);
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
            var aggregationService = parent.SubselectAggregationService.GetContextPartitionAggregationService(context.AgentInstanceId);
            var groupKeys = aggregationService.GetGroupKeys(context);
            if (groupKeys.IsEmpty())
            {
                return null;
            }
            var events = new ArrayDeque<EventBean>(groupKeys.Count);
            foreach (Object groupKey in groupKeys)
            {
                aggregationService.SetCurrentAccess(groupKey, context.AgentInstanceId, null);
                IDictionary<string, Object> row = parent.EvaluateRow(null, true, context);
                EventBean @event = parent.SubselectMultirowType.EventAdapterService.AdapterForTypedMap(
                    row, parent.SubselectMultirowType.EventType);
                events.Add(@event);
            }
            return events;
        }
    }
} // end of namespace