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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Iterator for the group-by case with a row per group.
    /// </summary>
    public class ResultSetRowPerGroupIterator : IEnumerator<EventBean>
    {
        private readonly IEnumerator<EventBean> sourceIterator;
        private readonly ResultSetProcessorRowPerGroup resultSetProcessor;
        private readonly AggregationService aggregationService;
        private EventBean nextResult;
        private readonly EventBean[] eventsPerStream;
        private readonly ISet<Object> priorSeenGroups;
        private readonly ExprEvaluatorContext exprEvaluatorContext;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="sourceIterator">is the parent view iterator</param>
        /// <param name="resultSetProcessor">for providing results</param>
        /// <param name="aggregationService">for pointing to the right aggregation row</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        public ResultSetRowPerGroupIterator(IEnumerator<EventBean> sourceIterator, ResultSetProcessorRowPerGroup resultSetProcessor, AggregationService aggregationService, ExprEvaluatorContext exprEvaluatorContext)
        {
            this.sourceIterator = sourceIterator;
            this.resultSetProcessor = resultSetProcessor;
            this.aggregationService = aggregationService;
            eventsPerStream = new EventBean[1];
            priorSeenGroups = new HashSet<Object>();
            this.exprEvaluatorContext = exprEvaluatorContext;
        }
    
        public bool HasNext()
        {
            if (nextResult != null)
            {
                return true;
            }
            FindNext();
            if (nextResult != null)
            {
                return true;
            }
            return false;
        }
    
        public EventBean Next()
        {
            if (nextResult != null)
            {
                EventBean result = nextResult;
                nextResult = null;
                return result;
            }
            FindNext();
            if (nextResult != null)
            {
                EventBean result = nextResult;
                nextResult = null;
                return result;
            }
            throw new NoSuchElementException();
        }
    
        private void FindNext()
        {
            while (sourceIterator.HasNext())
            {
                EventBean candidate = sourceIterator.Next();
                eventsPerStream[0] = candidate;
    
                Object groupKey = resultSetProcessor.GenerateGroupKey(eventsPerStream, true);
                aggregationService.SetCurrentAccess(groupKey, exprEvaluatorContext.AgentInstanceId, null);
    
                if (resultSetProcessor.OptionalHavingNode != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(candidate);}
                    Boolean pass = (Boolean) resultSetProcessor.OptionalHavingNode.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(pass);}
                    if (!pass)
                    {
                        continue;
                    }
                }
                if (priorSeenGroups.Contains(groupKey))
                {
                    continue;
                }
                priorSeenGroups.Add(groupKey);
    
                nextResult = resultSetProcessor.SelectExprProcessor.Process(eventsPerStream, true, true, exprEvaluatorContext);
    
                break;
            }
        }
    
        public void Remove()
        {
            throw new UnsupportedOperationException();
        }
    }
}
