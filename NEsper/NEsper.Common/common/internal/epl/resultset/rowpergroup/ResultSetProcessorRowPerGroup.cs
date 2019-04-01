///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergroup
{
    public interface ResultSetProcessorRowPerGroup : ResultSetProcessor,
        AggregationRowRemovedCallback
    {
        AggregationService AggregationService { get; }

        ExprEvaluatorContext AgentInstanceContext { get; }

        SelectExprProcessor SelectExprProcessor { get; }
        object GenerateGroupKeySingle(EventBean[] eventsPerStream, bool isNewData);

        bool HasHavingClause { get; }

        bool EvaluateHavingClause(
            EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);

        bool IsSelectRStream { get; }

        EventBean GenerateOutputBatchedNoSortWMap(
            bool join, object mk, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize);

        void GenerateOutputBatchedArrFromIterator(
            bool join, IEnumerator<KeyValuePair<object, EventBean[]>> keysAndEvents, bool isNewData, bool isSynthesize,
            IList<EventBean> resultEvents, IList<object> optSortKeys);
    }
} // end of namespace