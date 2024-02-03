///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.resultset.agggrouped
{
    /// <summary>
    /// Iterator for group-by with aggregation.
    /// </summary>
    public class ResultSetProcessorAggregateGroupedIterator
    {
        /// <summary>
        /// Creates an aggregated group enumerator.
        /// </summary>
        /// <param name="sourceIterator">The source iterator.</param>
        /// <param name="resultSetProcessor">The result set processor.</param>
        /// <param name="aggregationService">The aggregation service.</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        /// <returns></returns>
        public static IEnumerator<EventBean> Create(
            IEnumerator<EventBean> sourceIterator,
            ResultSetProcessorAggregateGrouped resultSetProcessor,
            AggregationService aggregationService,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var eventsPerStream = new EventBean[1];

            while (sourceIterator.MoveNext()) {
                eventsPerStream[0] = sourceIterator.Current;

                var groupKey = resultSetProcessor.GenerateGroupKeySingle(eventsPerStream, true);
                aggregationService.SetCurrentAccess(groupKey, exprEvaluatorContext.AgentInstanceId, null);

                if (resultSetProcessor.HasHavingClause) {
                    var pass = resultSetProcessor.EvaluateHavingClause(eventsPerStream, true, exprEvaluatorContext);
                    if (!pass) {
                        continue;
                    }
                }

                yield return resultSetProcessor.SelectExprProcessor.Process(
                    eventsPerStream,
                    true,
                    true,
                    exprEvaluatorContext);
            }
        }
    }
} // end of namespace