///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.resultset.rowperevent
{
    /// <summary>
    ///     Enumerator for aggregation results that aggregate all rows.
    /// </summary>
    public class ResultSetProcessorRowPerEventEnumerator
    {
        public static IEnumerator<EventBean> For(
            IEnumerator<EventBean> sourceIterator,
            ResultSetProcessorRowPerEvent resultSetProcessor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var eventsPerStream = new EventBean[1];
            var hasHavingClause = resultSetProcessor.HasHavingClause();
            var selectExprProcessor = resultSetProcessor.SelectExprProcessor;

            if (!hasHavingClause) {
                while (sourceIterator.MoveNext()) {
                    eventsPerStream[0] = sourceIterator.Current;
                    yield return selectExprProcessor.Process(eventsPerStream, true, true, exprEvaluatorContext);
                }
            }
            else {
                while (sourceIterator.MoveNext()) {
                    eventsPerStream[0] = sourceIterator.Current;
                    if (!resultSetProcessor.EvaluateHavingClause(eventsPerStream, true, exprEvaluatorContext)) {
                        continue;
                    }

                    yield return selectExprProcessor.Process(eventsPerStream, true, true, exprEvaluatorContext);
                }
            }
        }
    }
} // end of namespace