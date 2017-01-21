///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implements an aggregation service for match recognize.
    /// </summary>
    public class AggregationServiceMatchRecognizeFactoryImpl : AggregationServiceMatchRecognizeFactory
    {
        private readonly ExprEvaluator[][] _evaluatorsEachStream;
        private readonly AggregationMethodFactory[][] _factoryEachStream;
    
        /// <summary>Ctor. </summary>
        /// <param name="countStreams">number of streams/variables</param>
        /// <param name="aggregatorsPerStream">aggregation methods per stream</param>
        /// <param name="evaluatorsPerStream">evaluation functions per stream</param>
        public AggregationServiceMatchRecognizeFactoryImpl(int countStreams, IDictionary<int, AggregationMethodFactory[]> aggregatorsPerStream, IDictionary<int, ExprEvaluator[]> evaluatorsPerStream)
        {
            _evaluatorsEachStream = new ExprEvaluator[countStreams][];
            _factoryEachStream = new AggregationMethodFactory[countStreams][];
    
            foreach (var agg in aggregatorsPerStream)
            {
                _factoryEachStream[agg.Key] = agg.Value;
            }
    
            foreach (var eval in evaluatorsPerStream)
            {
                _evaluatorsEachStream[eval.Key] = eval.Value;
            }
        }
    
        public AggregationServiceMatchRecognize MakeService(AgentInstanceContext agentInstanceContext)
        {
            var aggregatorsEachStream = new AggregationMethod[_factoryEachStream.Length][];
    
            int count = 0;
            for (int stream = 0; stream < _factoryEachStream.Length; stream++)
            {
                AggregationMethodFactory[] thatStream = _factoryEachStream[stream];
                if (thatStream != null) {
                    aggregatorsEachStream[stream] = new AggregationMethod[thatStream.Length];
                    for (int aggId = 0; aggId < thatStream.Length; aggId++)
                    {
                        aggregatorsEachStream[stream][aggId] =
                            _factoryEachStream[stream][aggId].Make();
                        count++;
                    }
                }
            }
    
            var aggregatorsAll = new AggregationMethod[count];
            count = 0;
            for (int stream = 0; stream < _factoryEachStream.Length; stream++) {
                if (_factoryEachStream[stream] != null) {
                    for (int aggId = 0; aggId < _factoryEachStream[stream].Length; aggId++) {
                        aggregatorsAll[count] = aggregatorsEachStream[stream][aggId];
                        count++;
                    }
                }
            }
    
            return new AggregationServiceMatchRecognizeImpl(_evaluatorsEachStream, aggregatorsEachStream, aggregatorsAll);
        }
    }
}
