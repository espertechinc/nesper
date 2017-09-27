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

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryAggregationSingle : AIRegistryAggregation, AggregationService {
        private AggregationService _service;
    
        public AIRegistryAggregationSingle() {
        }
    
        public void AssignService(int serviceId, AggregationService aggregationService) {
            _service = aggregationService;
        }
    
        public void DeassignService(int serviceId) {
            _service = null;
        }

        public int InstanceCount
        {
            get { return _service == null ? 0 : 1; }
        }

        public void ApplyEnter(EventBean[] eventsPerStream, Object optionalGroupKeyPerRow, ExprEvaluatorContext exprEvaluatorContext) {
            _service.ApplyEnter(eventsPerStream, optionalGroupKeyPerRow, exprEvaluatorContext);
        }
    
        public void ApplyLeave(EventBean[] eventsPerStream, Object optionalGroupKeyPerRow, ExprEvaluatorContext exprEvaluatorContext) {
            _service.ApplyLeave(eventsPerStream, optionalGroupKeyPerRow, exprEvaluatorContext);
        }
    
        public void SetCurrentAccess(Object groupKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel) {
            _service.SetCurrentAccess(groupKey, agentInstanceId, null);
        }
    
        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext) {
            _service.ClearResults(exprEvaluatorContext);
        }
    
        public object GetValue(int column, int agentInstanceId, EvaluateParams evaluateParams) {
            return _service.GetValue(column, agentInstanceId, evaluateParams);
        }
    
        public ICollection<EventBean> GetCollectionOfEvents(int column, EvaluateParams evaluateParams) {
            return _service.GetCollectionOfEvents(column, evaluateParams);
        }
    
        public ICollection<object> GetCollectionScalar(int column, EvaluateParams evaluateParams) {
            return _service.GetCollectionScalar(column, evaluateParams);
        }
    
        public EventBean GetEventBean(int column, EvaluateParams evaluateParams) {
            return _service.GetEventBean(column, evaluateParams);
        }
    
        public void SetRemovedCallback(AggregationRowRemovedCallback callback) {
            // not applicable
        }
    
        public void Accept(AggregationServiceVisitor visitor) {
            throw new UnsupportedOperationException("Not applicable");
        }
    
        public void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor) {
            throw new UnsupportedOperationException("Not applicable");
        }

        public bool IsGrouped
        {
            get { throw new UnsupportedOperationException("Not applicable"); }
        }

        public Object GetGroupKey(int agentInstanceId) {
            return _service.GetGroupKey(agentInstanceId);
        }
    
        public ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext) {
            return _service.GetGroupKeys(exprEvaluatorContext);
        }
    
        public AggregationService GetContextPartitionAggregationService(int agentInstanceId) {
            return _service;
        }
    
        public void Stop() {
        }
    }
} // end of namespace
