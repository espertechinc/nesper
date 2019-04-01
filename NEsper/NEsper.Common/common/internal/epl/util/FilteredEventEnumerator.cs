///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.util
{
    /// <summary>
    ///     An enumerator that filters events supplied by another enumerator,
    ///     using a list of one or more filter expressions as filter.
    /// </summary>
    public class FilteredEventEnumerator
    {
        private static IEnumerator<EventBean> ForImpl(
            ExprEvaluator filter,
            IEnumerator<EventBean> parent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var eventsPerStream = new EventBean[1];

            while (parent.MoveNext()) {
                var eventBean = parent.Current;
                eventsPerStream[0] = eventBean;
                var result = filter.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                if (result != null && true.Equals(result)) {
                    yield return eventBean;
                }
            }
        }

        public static IEnumerator<EventBean> For(
            ExprEvaluator filter,
            IEnumerator<EventBean> parent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (filter == null) {
                throw new ArgumentException("Null filter");
            }

            return ForImpl(filter, parent, exprEvaluatorContext);
        }
    }
} // end of namespace