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

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryAggregationSingle : AIRegistryAggregation,
        AggregationService
    {
        private AggregationService service;

        public void AssignService(
            int serviceId,
            AggregationService aggregationService)
        {
            service = aggregationService;
        }

        public void DeassignService(int serviceId)
        {
            service = null;
        }

        public int InstanceCount => service == null ? 0 : 1;

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            service.ApplyEnter(eventsPerStream, optionalGroupKeyPerRow, exprEvaluatorContext);
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            service.ApplyLeave(eventsPerStream, optionalGroupKeyPerRow, exprEvaluatorContext);
        }

        public void SetCurrentAccess(
            object groupKey,
            int agentInstanceId,
            AggregationGroupByRollupLevel rollupLevel)
        {
            service.SetCurrentAccess(groupKey, agentInstanceId, null);
        }

        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            service.ClearResults(exprEvaluatorContext);
        }

        public object GetValue(
            int column,
            int agentInstanceId,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return service.GetValue(column, agentInstanceId, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public ICollection<EventBean> GetCollectionOfEvents(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return service.GetCollectionOfEvents(column, eventsPerStream, isNewData, context);
        }

        public ICollection<object> GetCollectionScalar(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return service.GetCollectionScalar(column, eventsPerStream, isNewData, context);
        }

        public EventBean GetEventBean(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return service.GetEventBean(column, eventsPerStream, isNewData, context);
        }

        public AggregationRow GetAggregationRow(
            int agentInstanceId,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return service.GetAggregationRow(agentInstanceId, eventsPerStream, isNewData, context);
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

        public bool IsGrouped => throw new UnsupportedOperationException("Not applicable");

        public object GetGroupKey(int agentInstanceId)
        {
            return service.GetGroupKey(agentInstanceId);
        }

        public ICollection<object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return service.GetGroupKeys(exprEvaluatorContext);
        }

        public AggregationService GetContextPartitionAggregationService(int agentInstanceId)
        {
            return service;
        }

        public void Stop()
        {
        }
    }
} // end of namespace