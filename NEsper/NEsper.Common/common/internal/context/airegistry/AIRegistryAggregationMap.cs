///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryAggregationMap : AIRegistryAggregation
    {
        private readonly IDictionary<int, AggregationService> services;

        protected internal AIRegistryAggregationMap()
        {
            services = new Dictionary<int, AggregationService>();
        }

        public void AssignService(
            int serviceId,
            AggregationService aggregationService)
        {
            services.Put(serviceId, aggregationService);
        }

        public void DeassignService(int serviceId)
        {
            services.Remove(serviceId);
        }

        public int InstanceCount => services.Count;

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            services.Get(exprEvaluatorContext.AgentInstanceId).ApplyEnter(
                eventsPerStream, optionalGroupKeyPerRow, exprEvaluatorContext);
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            services.Get(exprEvaluatorContext.AgentInstanceId).ApplyLeave(
                eventsPerStream, optionalGroupKeyPerRow, exprEvaluatorContext);
        }

        public void SetCurrentAccess(
            object groupKey,
            int agentInstanceId,
            AggregationGroupByRollupLevel rollupLevel)
        {
            services.Get(agentInstanceId).SetCurrentAccess(groupKey, agentInstanceId, null);
        }

        public AggregationService GetContextPartitionAggregationService(int agentInstanceId)
        {
            return services.Get(agentInstanceId);
        }

        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            services.Get(exprEvaluatorContext.AgentInstanceId).ClearResults(exprEvaluatorContext);
        }

        public object GetValue(
            int column,
            int agentInstanceId,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return services.Get(agentInstanceId).GetValue(
                column, agentInstanceId, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public ICollection<EventBean> GetCollectionOfEvents(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return services.Get(context.AgentInstanceId)
                .GetCollectionOfEvents(column, eventsPerStream, isNewData, context);
        }

        public ICollection<object> GetCollectionScalar(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return services.Get(context.AgentInstanceId)
                .GetCollectionScalar(column, eventsPerStream, isNewData, context);
        }

        public EventBean GetEventBean(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return services.Get(context.AgentInstanceId).GetEventBean(column, eventsPerStream, isNewData, context);
        }

        public void SetRemovedCallback(AggregationRowRemovedCallback callback)
        {
            // not applicable
        }

        public void Accept(AggregationServiceVisitor visitor)
        {
            throw new UnsupportedOperationException("Not applicable");
        }

        public void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor)
        {
            throw new UnsupportedOperationException("Not applicable");
        }

        public object GetGroupKey(int agentInstanceId)
        {
            return services.Get(agentInstanceId).GetGroupKey(agentInstanceId);
        }

        public ICollection<object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return services.Get(exprEvaluatorContext.AgentInstanceId).GetGroupKeys(exprEvaluatorContext);
        }

        public void Stop()
        {
        }

        public bool IsGrouped => throw new UnsupportedOperationException("Not applicable");
    }
} // end of namespace