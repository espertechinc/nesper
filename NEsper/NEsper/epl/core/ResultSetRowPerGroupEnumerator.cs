///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.core
{
    public class ResultSetRowPerGroupEnumerator
    {
        /// <summary>
        /// Creates a new enumeration wrapper.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="resultSetProcessor">The result set processor.</param>
        /// <param name="aggregationService">The aggregation service.</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        /// <returns></returns>
        public static IEnumerator<EventBean> New(
            IEnumerable<EventBean> source,
            ResultSetProcessorRowPerGroup resultSetProcessor,
            AggregationService aggregationService,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return New(
                source.GetEnumerator(),
                resultSetProcessor,
                aggregationService,
                exprEvaluatorContext);
        }

        /// <summary>
        /// Creates a new enumeration wrapper.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="resultSetProcessor">The result set processor.</param>
        /// <param name="aggregationService">The aggregation service.</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        /// <returns></returns>
        public static IEnumerator<EventBean> New(
            IEnumerator<EventBean> source,
            ResultSetProcessorRowPerGroup resultSetProcessor,
            AggregationService aggregationService,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var eventsPerStream = new EventBean[1];
            var priorSeenGroups = new HashSet<object>();
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            var optionHavingNode = resultSetProcessor.OptionalHavingNode;
            var selectExprProcessor = resultSetProcessor.SelectExprProcessor;

            while(source.MoveNext())
            {
                var candidate = source.Current;
                eventsPerStream[0] = candidate;
                var groupKey = resultSetProcessor.GenerateGroupKey(eventsPerStream, true);
                aggregationService.SetCurrentAccess(groupKey, exprEvaluatorContext.AgentInstanceId, null);

                if (optionHavingNode != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(candidate); }
                    var pass = resultSetProcessor.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(pass.AsBoxedBoolean()); }
                    if ((pass != null) && false.Equals(pass))
                    {
                        continue;
                    }
                }

                if (priorSeenGroups.Contains(groupKey))
                {
                    continue;
                }

                priorSeenGroups.Add(groupKey);

                yield return selectExprProcessor.Process(eventsPerStream, true, true, exprEvaluatorContext);
            }
        }
    }
}
