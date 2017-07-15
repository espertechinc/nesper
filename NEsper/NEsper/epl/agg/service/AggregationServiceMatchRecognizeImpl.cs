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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>Implements an aggregation service for match recognize.</summary>
    public class AggregationServiceMatchRecognizeImpl : AggregationServiceMatchRecognize {
        private ExprEvaluator[][] evaluatorsEachStream;
        private AggregationMethod[][] aggregatorsEachStream;
        private AggregationMethod[] aggregatorsAll;
    
        public AggregationServiceMatchRecognizeImpl(ExprEvaluator[][] evaluatorsEachStream, AggregationMethod[][] aggregatorsEachStream, AggregationMethod[] aggregatorsAll) {
            this.evaluatorsEachStream = evaluatorsEachStream;
            this.aggregatorsEachStream = aggregatorsEachStream;
            this.aggregatorsAll = aggregatorsAll;
        }
    
        public void ApplyEnter(EventBean[] eventsPerStream, int streamId, ExprEvaluatorContext exprEvaluatorContext) {
    
            ExprEvaluator[] evaluatorsStream = evaluatorsEachStream[streamId];
            if (evaluatorsStream == null) {
                return;
            }
    
            AggregationMethod[] aggregatorsStream = aggregatorsEachStream[streamId];
            for (int j = 0; j < evaluatorsStream.Length; j++) {
                Object columnResult = evaluatorsStream[j].Evaluate(eventsPerStream, true, exprEvaluatorContext);
                aggregatorsStream[j].Enter(columnResult);
            }
        }
    
        public Object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            return aggregatorsAll[column].Value;
        }
    
        public ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return null;
        }
    
        public ICollection<Object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return null;
        }
    
        public EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return null;
        }
    
        public void ClearResults() {
            foreach (AggregationMethod aggregator in aggregatorsAll) {
                aggregator.Clear();
            }
        }
    
        public Object GetGroupKey(int agentInstanceId) {
            return null;
        }
    
        public ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext) {
            return null;
        }
    }
} // end of namespace
