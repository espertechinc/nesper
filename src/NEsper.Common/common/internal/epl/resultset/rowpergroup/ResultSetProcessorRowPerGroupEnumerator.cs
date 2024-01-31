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

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergroup
{
    /// <summary>
    ///     Iterator for the group-by case with a row per group.
    /// </summary>
    public class ResultSetProcessorRowPerGroupEnumerator
    {
        public static IEnumerator<EventBean> For(
            IEnumerator<EventBean> sourceIterator,
            ResultSetProcessorRowPerGroup resultSetProcessor,
            AggregationService aggregationService,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var eventsPerStream = new EventBean[1];
            var priorSeenGroups = new HashSet<object>();
            var hasHavingClause = resultSetProcessor.HasHavingClause;

            while (sourceIterator.MoveNext()) {
                eventsPerStream[0] = sourceIterator.Current;
                var groupKey = resultSetProcessor.GenerateGroupKeySingle(eventsPerStream, true);
                aggregationService.SetCurrentAccess(groupKey, exprEvaluatorContext.AgentInstanceId, null);

                if (hasHavingClause) {
                    var pass = resultSetProcessor.EvaluateHavingClause(eventsPerStream, true, exprEvaluatorContext);
                    if (!pass) {
                        continue;
                    }
                }

                if (priorSeenGroups.Contains(groupKey)) {
                    continue;
                }

                priorSeenGroups.Add(groupKey);
                yield return resultSetProcessor.SelectExprProcessor.Process(
                    eventsPerStream,
                    true,
                    true,
                    exprEvaluatorContext);
            }
        }
    }
} // end of namespace