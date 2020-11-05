///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryAggregationMultiPerm : AIRegistryAggregation
    {
        private readonly ArrayWrap<AggregationService> services;

        protected internal AIRegistryAggregationMultiPerm()
        {
            services = new ArrayWrap<AggregationService>(2);
        }

        public void AssignService(
            int serviceId,
            AggregationService aggregationService)
        {
            AIRegistryUtil.CheckExpand(serviceId, services);
            services.Array[serviceId] = aggregationService;
            InstanceCount++;
        }

        public void DeassignService(int serviceId)
        {
            if (serviceId >= services.Array.Length) {
                // possible since it may not have been assigned as there was nothing to assign
                return;
            }

            services.Array[serviceId] = null;
            InstanceCount--;
        }

        public int InstanceCount { get; private set; }

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            services.Array[exprEvaluatorContext.AgentInstanceId]
                .ApplyEnter(
                    eventsPerStream,
                    optionalGroupKeyPerRow,
                    exprEvaluatorContext);
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            services.Array[exprEvaluatorContext.AgentInstanceId]
                .ApplyLeave(
                    eventsPerStream,
                    optionalGroupKeyPerRow,
                    exprEvaluatorContext);
        }

        public void SetCurrentAccess(
            object groupKey,
            int agentInstanceId,
            AggregationGroupByRollupLevel rollupLevel)
        {
            services.Array[agentInstanceId].SetCurrentAccess(groupKey, agentInstanceId, null);
        }

        public AggregationService GetContextPartitionAggregationService(int agentInstanceId)
        {
            return services.Array[agentInstanceId];
        }

        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            services.Array[exprEvaluatorContext.AgentInstanceId].ClearResults(exprEvaluatorContext);
        }

        public object GetValue(
            int column,
            int agentInstanceId,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return services.Array[agentInstanceId]
                .GetValue(
                    column,
                    agentInstanceId,
                    eventsPerStream,
                    isNewData,
                    exprEvaluatorContext);
        }

        public ICollection<EventBean> GetCollectionOfEvents(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return services.Array[context.AgentInstanceId]
                .GetCollectionOfEvents(column, eventsPerStream, isNewData, context);
        }

        public ICollection<object> GetCollectionScalar(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return services.Array[context.AgentInstanceId]
                .GetCollectionScalar(column, eventsPerStream, isNewData, context);
        }

        public EventBean GetEventBean(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return services.Array[context.AgentInstanceId].GetEventBean(column, eventsPerStream, isNewData, context);
        }

        public AggregationRow GetAggregationRow(
            int agentInstanceId,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return services
                .Array[context.AgentInstanceId]
                .GetAggregationRow(agentInstanceId, eventsPerStream, isNewData, context);
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
            return services.Array[agentInstanceId].GetGroupKey(agentInstanceId);
        }

        public ICollection<object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return services.Array[exprEvaluatorContext.AgentInstanceId].GetGroupKeys(exprEvaluatorContext);
        }

        public void Stop()
        {
        }

        public bool IsGrouped {
            get { throw new UnsupportedOperationException("Not applicable"); }
        }
    }
} // end of namespace