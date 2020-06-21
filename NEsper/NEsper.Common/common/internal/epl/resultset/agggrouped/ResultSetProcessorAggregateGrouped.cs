///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;

namespace com.espertech.esper.common.@internal.epl.resultset.agggrouped
{
    public interface ResultSetProcessorAggregateGrouped : ResultSetProcessor,
        AggregationRowRemovedCallback
    {
        bool HasHavingClause { get; }

        SelectExprProcessor SelectExprProcessor { get; }

        AggregationService AggregationService { get; }

        ExprEvaluatorContext GetAgentInstanceContext();

        bool IsSelectRStream { get; }

        bool EvaluateHavingClause(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        object GenerateGroupKeySingle(
            EventBean[] eventsPerStream,
            bool isNewData);

        object[] GenerateGroupKeyArrayJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newData,
            bool isNewData);

        object[] GenerateGroupKeyArrayView(
            EventBean[] oldData,
            bool isNewData);

        EventBean GenerateOutputBatchedSingle(
            object key,
            EventBean[] @event,
            bool isNewData,
            bool isSynthesize);

        void GenerateOutputBatchedViewUnkeyed(
            EventBean[] outputEvents,
            object[] groupByKeys,
            bool isNewData,
            bool isSynthesize,
            ICollection<EventBean> resultEvents,
            IList<object> optSortKeys,
            EventBean[] eventsPerStream);

        void GenerateOutputBatchedJoinUnkeyed(
            ISet<MultiKeyArrayOfKeys<EventBean>> outputEvents,
            object[] groupByKeys,
            bool isNewData,
            bool isSynthesize,
            ICollection<EventBean> resultEvents,
            IList<object> optSortKeys);

        void GenerateOutputBatchedViewPerKey(
            EventBean[] oldData,
            object[] oldDataMultiKey,
            bool isNewData,
            bool isGenerateSynthetic,
            IDictionary<object, EventBean> outputLastUnordGroupOld,
            IDictionary<object, object> optSortKeys,
            EventBean[] eventsPerStream);

        void GenerateOutputBatchedJoinPerKey(
            ISet<MultiKeyArrayOfKeys<EventBean>> outputEvents,
            object[] groupByKeys,
            bool isNewData,
            bool isSynthesize,
            IDictionary<object, EventBean> resultEvents,
            IDictionary<object, object> optSortKeys);
    }
} // end of namespace