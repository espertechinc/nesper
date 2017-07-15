///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>Implements an aggregation service for match recognize.</summary>
    public class AggregationServiceMatchRecognizeFactoryImpl : AggregationServiceMatchRecognizeFactory {
        private ExprEvaluator[][] evaluatorsEachStream;
        private AggregationMethodFactory[][] factoryEachStream;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="countStreams">number of streams/variables</param>
        /// <param name="aggregatorsPerStream">aggregation methods per stream</param>
        /// <param name="evaluatorsPerStream">evaluation functions per stream</param>
        public AggregationServiceMatchRecognizeFactoryImpl(int countStreams, LinkedHashMap<int?, AggregationMethodFactory[]> aggregatorsPerStream, IDictionary<int?, ExprEvaluator[]> evaluatorsPerStream) {
            evaluatorsEachStream = new ExprEvaluator[countStreams][];
            factoryEachStream = new AggregationMethodFactory[countStreams][];
    
            foreach (var agg in aggregatorsPerStream) {
                factoryEachStream[agg.Key] = agg.Value;
            }
    
            foreach (var eval in evaluatorsPerStream) {
                evaluatorsEachStream[eval.Key] = eval.Value;
            }
        }
    
        public AggregationServiceMatchRecognize MakeService(AgentInstanceContext agentInstanceContext) {
    
            var aggregatorsEachStream = new AggregationMethod[factoryEachStream.Length][];
    
            int count = 0;
            for (int stream = 0; stream < factoryEachStream.Length; stream++) {
                AggregationMethodFactory[] thatStream = factoryEachStream[stream];
                if (thatStream != null) {
                    aggregatorsEachStream[stream] = new AggregationMethod[thatStream.Length];
                    for (int aggId = 0; aggId < thatStream.Length; aggId++) {
                        aggregatorsEachStream[stream][aggId] = factoryEachStream[stream][aggId].Make();
                        count++;
                    }
                }
            }
    
            var aggregatorsAll = new AggregationMethod[count];
            count = 0;
            for (int stream = 0; stream < factoryEachStream.Length; stream++) {
                if (factoryEachStream[stream] != null) {
                    for (int aggId = 0; aggId < factoryEachStream[stream].Length; aggId++) {
                        aggregatorsAll[count] = aggregatorsEachStream[stream][aggId];
                        count++;
                    }
                }
            }
    
            return new AggregationServiceMatchRecognizeImpl(evaluatorsEachStream, aggregatorsEachStream, aggregatorsAll);
        }
    }
} // end of namespace
