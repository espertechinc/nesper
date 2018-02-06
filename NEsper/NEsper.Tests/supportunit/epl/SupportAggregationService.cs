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
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.supportunit.epl
{
    public class SupportAggregationService : AggregationService
    {
        private readonly List<Pair<EventBean[], Object>> _enterList = new List<Pair<EventBean[], Object>>();
        private readonly List<Pair<EventBean[], Object>> _leaveList = new List<Pair<EventBean[], Object>>();

        public List<Pair<EventBean[], object>> LeaveList
        {
            get { return _leaveList; }
        }

        public List<Pair<EventBean[], object>> EnterList
        {
            get { return _enterList; }
        }

        #region AggregationService Members

        public void ApplyEnter(EventBean[] eventsPerStream,
                               Object optionalGroupKeyPerRow,
                               ExprEvaluatorContext exprEvaluatorContext)
        {
            _enterList.Add(
                new Pair<EventBean[], Object>(eventsPerStream,
                                              optionalGroupKeyPerRow));
        }

        public void ApplyLeave(EventBean[] eventsPerStream,
                               Object optionalGroupKeyPerRow,
                               ExprEvaluatorContext exprEvaluatorContext)
        {
            _leaveList.Add(
                new Pair<EventBean[], Object>(eventsPerStream,
                                              optionalGroupKeyPerRow));
        }

        public void SetCurrentAccess(object groupKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
        }

        public object GetValue(int column, int agentInstanceId, EvaluateParams evaluateParams)
        {
            return null;
        }

        public ICollection<EventBean> GetCollectionOfEvents(int column, EvaluateParams evaluateParams)
        {
            return null;
        }

        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public EventBean GetEventBean(int column, EvaluateParams evaluateParams)
        {
            return null;
        }

        public void SetRemovedCallback(AggregationRowRemovedCallback callback)
        {
            // not applicable
        }

        public void Accept(AggregationServiceVisitor visitor)
        {
            throw new UnsupportedOperationException("not applicable");
        }

        public void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor)
        {
            throw new UnsupportedOperationException("not applicable");
        }

        public bool IsGrouped
        {
            get { throw new UnsupportedOperationException("not applicable"); }
        }

        public object GetGroupKey(int agentInstanceId)
        {
            return null;
        }

        public ICollection<object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public ICollection<object> GetCollectionScalar(int column, EvaluateParams evaluateParams)
        {
            return null;
        }

        public void Stop()
        {
        }

        #endregion

        public AggregationService GetContextPartitionAggregationService(int agentInstanceId)
        {
            return this;
        }
    }
}
