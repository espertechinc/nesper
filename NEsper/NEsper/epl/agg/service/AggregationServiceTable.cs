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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.agg.service
{
    public class AggregationServiceTable : AggregationService
    {
        private readonly TableStateInstance _tableState;

        public AggregationServiceTable(TableStateInstance tableState)
        {
            _tableState = tableState;
        }

        public TableStateInstance TableState
        {
            get { return _tableState; }
        }

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            Object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw HandleNotSupported();
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            Object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw HandleNotSupported();
        }

        public void SetCurrentAccess(Object groupKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            throw HandleNotSupported();
        }

        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            throw HandleNotSupported();
        }

        public void SetRemovedCallback(AggregationRowRemovedCallback callback)
        {
            throw HandleNotSupported();
        }

        public void Accept(AggregationServiceVisitor visitor)
        {
            // no action
        }

        public void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor)
        {
            // no action
        }

        public bool IsGrouped
        {
            get { return false; }
        }

        public object GetValue(int column, int agentInstanceId, EvaluateParams evaluateParams)
        {
            throw HandleNotSupported();
        }

        public ICollection<EventBean> GetCollectionOfEvents(int column, EvaluateParams evaluateParams)
        {
            throw HandleNotSupported();
        }

        public EventBean GetEventBean(int column, EvaluateParams evaluateParams)
        {
            throw HandleNotSupported();
        }

        public Object GetGroupKey(int agentInstanceId)
        {
            throw HandleNotSupported();
        }

        public ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            throw HandleNotSupported();
        }

        public ICollection<object> GetCollectionScalar(int column, EvaluateParams evaluateParams)
        {
            throw HandleNotSupported();
        }

        private UnsupportedOperationException HandleNotSupported()
        {
            return new UnsupportedOperationException("Operation not supported, aggregation server for reporting only");
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
