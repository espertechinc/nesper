///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.subquery
{
    /// <summary>
    ///     Represents a subselect in an expression tree.
    /// </summary>
    public class ExprSubselectRowEvalStrategyUnfilteredSelectedGroupedAgg
        : ExprSubselectRowEvalStrategyUnfilteredSelected
        , ExprSubselectRowEvalStrategy
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override object Evaluate(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            ICollection<object> groupKeys = parent.SubselectAggregationService.GetGroupKeys(exprEvaluatorContext);
            if (groupKeys.IsEmpty() || groupKeys.Count > 1)
            {
                return null;
            }
            parent.SubselectAggregationService.SetCurrentAccess(
                groupKeys.FirstOrDefault(), exprEvaluatorContext.AgentInstanceId, null);

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
            ICollection<object> groupKeys = parent.SubselectAggregationService.GetGroupKeys(context);
            if (groupKeys.IsEmpty())
            {
                return null;
            }
            ICollection<EventBean> events = new ArrayDeque<EventBean>(groupKeys.Count);
            foreach (object groupKey in groupKeys)
            {
                parent.SubselectAggregationService.SetCurrentAccess(groupKey, context.AgentInstanceId, null);
                IDictionary<string, object> row = parent.EvaluateRow(null, true, context);
                EventBean @event = parent.subselectMultirowType.EventAdapterService.AdapterForTypedMap(
                    row, parent.subselectMultirowType.EventType);
                events.Add(@event);
            }
            return events;
        }
    }
}