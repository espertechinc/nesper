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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryAggregationMultiPerm : AIRegistryAggregation
    {
        private readonly ArrayWrap<AggregationService> _services;
        private int _count;

        public AIRegistryAggregationMultiPerm()
        {
            _services = new ArrayWrap<AggregationService>(2);
        }

        public void AssignService(int serviceId, AggregationService aggregationService)
        {
            AIRegistryUtil.CheckExpand(serviceId, _services);
            _services.Array[serviceId] = aggregationService;
            _count++;
        }

        public void DeassignService(int serviceId)
        {
            if (serviceId >= _services.Array.Length)
            {
                // possible since it may not have been assigned as there was nothing to assign
                return;
            }
            _services.Array[serviceId] = null;
            _count--;
        }

        public int InstanceCount
        {
            get { return _count; }
        }

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            Object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            _services.Array[exprEvaluatorContext.AgentInstanceId].ApplyEnter(
                eventsPerStream, optionalGroupKeyPerRow, exprEvaluatorContext);
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            Object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            _services.Array[exprEvaluatorContext.AgentInstanceId].ApplyLeave(
                eventsPerStream, optionalGroupKeyPerRow, exprEvaluatorContext);
        }

        public void SetCurrentAccess(Object groupKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            _services.Array[agentInstanceId].SetCurrentAccess(groupKey, agentInstanceId, null);
        }

        public AggregationService GetContextPartitionAggregationService(int agentInstanceId)
        {
            return _services.Array[agentInstanceId];
        }

        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            _services.Array[exprEvaluatorContext.AgentInstanceId].ClearResults(exprEvaluatorContext);
        }

        public object GetValue(int column, int agentInstanceId, EvaluateParams evaluateParams)
        {
            return _services.Array[agentInstanceId].GetValue(
                column, agentInstanceId, evaluateParams);
        }

        public ICollection<EventBean> GetCollectionOfEvents(int column, EvaluateParams evaluateParams)
        {
            return _services.Array[evaluateParams.ExprEvaluatorContext.AgentInstanceId].GetCollectionOfEvents(column, evaluateParams);
        }

        public ICollection<object> GetCollectionScalar(int column, EvaluateParams evaluateParams)
        {
            return _services.Array[evaluateParams.ExprEvaluatorContext.AgentInstanceId].GetCollectionScalar(column, evaluateParams);
        }

        public EventBean GetEventBean(int column, EvaluateParams evaluateParams)
        {
            return _services.Array[evaluateParams.ExprEvaluatorContext.AgentInstanceId].GetEventBean(column, evaluateParams);
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

        public bool IsGrouped
        {
            get { throw new UnsupportedOperationException("Not applicable"); }
        }

        public Object GetGroupKey(int agentInstanceId)
        {
            return _services.Array[agentInstanceId].GetGroupKey(agentInstanceId);
        }

        public ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return _services.Array[exprEvaluatorContext.AgentInstanceId].GetGroupKeys(exprEvaluatorContext);
        }

        public void Stop()
        {
        }
    }
} // end of namespace
