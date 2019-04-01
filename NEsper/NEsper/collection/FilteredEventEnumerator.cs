///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.collection
{
    public class FilteredEventEnumerator
    {
        public static IEnumerable<EventBean> New(
            ExprEvaluator[] filters,
            IEnumerable<EventBean> parent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if ((filters == null) || (filters.Length == 0))
            {
                foreach (var eventBean in parent)
                {
                    yield return eventBean;
                }
            }
            else
            {
                var evaluatorContext = exprEvaluatorContext;
                var eventArray = new EventBean[1];
                foreach (var eventBean in parent)
                {
                    var isFiltered = true;
                    eventArray[0] = eventBean;
                    foreach (var filter in filters)
                    {
                        var result = filter.Evaluate(new EvaluateParams(eventArray, true, evaluatorContext));
                        if (result == null || false.Equals(result))
                        {
                            // Event was filtered; end processing so that we can proceed
                            // to the next eventBean.
                            isFiltered = false;
                            break;
                        }
                    }
                    // Event was not filtered
                    if (isFiltered)
                    {
                        yield return eventBean;
                    }
                }
            }
        }

        public static IEnumerator<EventBean> Enumerate(
            ExprEvaluator[] filters,
            IEnumerable<EventBean> parent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return New(filters, parent, exprEvaluatorContext).GetEnumerator();
        }
    }
}
