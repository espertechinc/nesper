///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Enumerator for aggregation results that aggregate all rows.
    /// </summary>

    public class ResultSetAggregateAllEnumerator
    {
        public static IEnumerator<EventBean> New(
            IEnumerable<EventBean> source,
            ResultSetProcessorAggregateAll resultSetProcessor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var eventsPerStream = new EventBean[1];
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            var optionHavingNode = resultSetProcessor.OptionalHavingNode;
            var selectExprProcessor = resultSetProcessor.SelectExprProcessor;

            foreach (EventBean eventBean in source)
            {
                eventsPerStream[0] = eventBean;

                if (optionHavingNode != null)
                {
                    var pass = optionHavingNode.Evaluate(
                        evaluateParams);
                    if (false.Equals(pass))
                        continue;
                }

                yield return selectExprProcessor.Process(
                    eventsPerStream, true, true, exprEvaluatorContext);
            }
        }
    }
}