///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.resultset.rowforall
{
    public interface ResultSetProcessorRowForAll : ResultSetProcessor
    {
        AggregationService AggregationService { get; }

        //ExprEvaluatorContext ExprEvaluatorContext { get; }

        bool EvaluateHavingClause(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        EventBean[] GetSelectListEventsAsArray(
            bool isNewData,
            bool isSynthesize,
            bool join);

        bool IsSelectRStream { get; }
    }
} // end of namespace