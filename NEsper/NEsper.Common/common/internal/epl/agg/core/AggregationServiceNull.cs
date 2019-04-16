///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    /// <summary>
    ///     A null object implementation of the AggregationService
    ///     interface.
    /// </summary>
    public class AggregationServiceNull : AggregationService
    {
        public static readonly AggregationServiceNull INSTANCE = new AggregationServiceNull();

        private AggregationServiceNull()
        {
        }

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public void SetCurrentAccess(
            object groupKey,
            int agentInstanceId,
            AggregationGroupByRollupLevel rollupLevel)
        {
        }

        public object GetValue(
            int column,
            int agentInstanceId,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public ICollection<EventBean> GetCollectionOfEvents(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<object> GetCollectionScalar(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public EventBean GetEventBean(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            // no state to clear
        }

        public void SetRemovedCallback(AggregationRowRemovedCallback callback)
        {
            // not applicable
        }

        public void Accept(AggregationServiceVisitor visitor)
        {
        }

        public void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor)
        {
        }

        public bool IsGrouped => false;

        public object GetGroupKey(int agentInstanceId)
        {
            return null;
        }

        public ICollection<object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public void Stop()
        {
        }

        public AggregationService GetContextPartitionAggregationService(int agentInstanceId)
        {
            return this;
        }
    }
} // end of namespace