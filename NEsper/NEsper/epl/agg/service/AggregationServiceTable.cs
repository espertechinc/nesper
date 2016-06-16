///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO.Ports;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.agg.service
{
    public class AggregationServiceTable : AggregationService
    {
        private readonly TableStateInstance _tableState;
    
        public AggregationServiceTable(TableStateInstance tableState) {
            this._tableState = tableState;
        }

        public TableStateInstance TableState
        {
            get { return _tableState; }
        }

        public void ApplyEnter(EventBean[] eventsPerStream, object optionalGroupKeyPerRow, ExprEvaluatorContext exprEvaluatorContext) {
            throw HandleNotSupported();
        }
    
        public void ApplyLeave(EventBean[] eventsPerStream, object optionalGroupKeyPerRow, ExprEvaluatorContext exprEvaluatorContext) {
            throw HandleNotSupported();
        }
    
        public void SetCurrentAccess(object groupKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel) {
            throw HandleNotSupported();
        }
    
        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext) {
            throw HandleNotSupported();
        }
    
        public void SetRemovedCallback(AggregationRowRemovedCallback callback) {
            throw HandleNotSupported();
        }
    
        public void Accept(AggregationServiceVisitor visitor) {
            // no action
        }
    
        public void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor) {
            // no action
        }

        public bool IsGrouped
        {
            get { return false; }
        }

        public object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            throw HandleNotSupported();
        }
    
        public ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            throw HandleNotSupported();
        }
    
        public EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            throw HandleNotSupported();
        }
    
        public object GetGroupKey(int agentInstanceId) {
            throw HandleNotSupported();
        }
    
        public ICollection<object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext) {
            throw HandleNotSupported();
        }
    
        public ICollection<object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            throw HandleNotSupported();
        }
    
        private UnsupportedOperationException HandleNotSupported() {
            return new UnsupportedOperationException("Operation not supported, aggregation server for reporting only");
        }

        public void Stop() {
        }
    }
}
