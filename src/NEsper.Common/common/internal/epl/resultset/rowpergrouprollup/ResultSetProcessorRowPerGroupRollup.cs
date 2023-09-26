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

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup
{
    public interface ResultSetProcessorRowPerGroupRollup : ResultSetProcessor,
        AggregationRowRemovedCallback
    {
        AggregationService AggregationService { get; }

        ExprEvaluatorContext GetExprEvaluatorContext();

        bool IsSelectRStream { get; }

        AggregationGroupByRollupDesc GroupByRollupDesc { get; }

        object GenerateGroupKeySingle(
            EventBean[] eventsPerStream,
            bool isNewData);

        void GenerateOutputBatchedMapUnsorted(
            bool join,
            object mk,
            AggregationGroupByRollupLevel level,
            EventBean[] eventsPerStream,
            bool isNewData,
            bool isSynthesize,
            IDictionary<object, EventBean> resultEvents);

        void GenerateOutputBatched(
            object mk,
            AggregationGroupByRollupLevel level,
            EventBean[] eventsPerStream,
            bool isNewData,
            bool isSynthesize,
            IList<EventBean> resultEvents,
            IList<object> optSortKeys);
    }
} // end of namespace