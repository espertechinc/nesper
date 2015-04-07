///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implements an aggregation service for match recognize.
    /// </summary>
    public class AggregationServiceMatchRecognizeImpl : AggregationServiceMatchRecognize
    {
        private readonly ExprEvaluator[][] _evaluatorsEachStream;
        private readonly AggregationMethod[][] _aggregatorsEachStream;
        private readonly AggregationMethod[] _aggregatorsAll;

        public AggregationServiceMatchRecognizeImpl(
            ExprEvaluator[][] evaluatorsEachStream,
            AggregationMethod[][] aggregatorsEachStream,
            AggregationMethod[] aggregatorsAll)
        {
            _evaluatorsEachStream = evaluatorsEachStream;
            _aggregatorsEachStream = aggregatorsEachStream;
            _aggregatorsAll = aggregatorsAll;
        }

        public void ApplyEnter(EventBean[] eventsPerStream, int streamId, ExprEvaluatorContext exprEvaluatorContext)
        {

            var evaluatorsStream = _evaluatorsEachStream[streamId];
            if (evaluatorsStream == null)
            {
                return;
            }

            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            var aggregatorsStream = _aggregatorsEachStream[streamId];
            for (int j = 0; j < evaluatorsStream.Length; j++)
            {
                var columnResult = evaluatorsStream[j].Evaluate(evaluateParams);
                aggregatorsStream[j].Enter(columnResult);
            }
        }

        public object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _aggregatorsAll[column].Value;
        }

        public ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public void ClearResults()
        {
            foreach (AggregationMethod aggregator in _aggregatorsAll)
            {
                aggregator.Clear();
            }
        }

        public Object GetGroupKey(int agentInstanceId)
        {
            return null;
        }

        public ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }
    }
}
