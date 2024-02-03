///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.view.filter
{
    /// <summary>
    /// Iterator for reading and filtering a source event iterator.
    /// </summary>
    public class FilterExprViewIterator
    {
        public static IEnumerator<EventBean> For(
            IEnumerator<EventBean> sourceIterator,
            ExprEvaluator filter,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var evalEventArr = new EventBean[1];
            while (sourceIterator.MoveNext()) {
                var candidate = sourceIterator.Current;
                evalEventArr[0] = candidate;

                var pass = filter.Evaluate(evalEventArr, true, exprEvaluatorContext);
                if (pass != null && true.Equals(pass)) {
                    yield return candidate;
                }
            }
        }
    }
} // end of namespace