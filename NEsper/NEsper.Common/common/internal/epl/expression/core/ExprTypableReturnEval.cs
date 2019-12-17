///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    /// Interface for evaluators that select possible multi-valued results in a single select column,
    /// such as subqueries and "new" and case+new combined.
    /// <para />When returning non-null results from {#getRowProperties},
    /// the {@link ExprEvaluator#evaluate(com.espertech.esper.common.client.EventBean[], boolean, ExprEvaluatorContext)}
    /// must return an instance of Map&lt;String, Object&gt; (HashMap is fine).
    /// <para />When returning non-null results, the
    /// the evaluator must also return either Object[] results or Object[][],
    /// each object-array following the same exact order as provided by the map,
    /// matching the multi-row flag.
    /// </summary>
    public interface ExprTypableReturnEval : ExprEvaluator
    {
        object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        object[][] EvaluateTypableMulti(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);
    }
} // end of namespace