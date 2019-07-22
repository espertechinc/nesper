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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.historical.datacache;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.@join.strategy;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    /// <summary>
    ///     Implements the function to determine a join result set using tables/indexes and query strategy
    ///     instances for each stream.
    /// </summary>
    public class JoinSetComposerHistoricalImpl : JoinSetComposer
    {
        private readonly bool allowInitIndex;
        private readonly EventTable[][] repositories;
        private readonly ISet<MultiKey<EventBean>> newResults = new LinkedHashSet<MultiKey<EventBean>>();

        // Set semantic eliminates duplicates in result set, use Linked set to preserve order
        private readonly ISet<MultiKey<EventBean>> oldResults = new LinkedHashSet<MultiKey<EventBean>>();
        private readonly ExprEvaluatorContext staticEvalExprEvaluatorContext;
        private readonly Viewable[] streamViews;

        public JoinSetComposerHistoricalImpl(
            bool allowInitIndex,
            IDictionary<TableLookupIndexReqKey, EventTable>[] repositories,
            QueryStrategy[] queryStrategies,
            Viewable[] streamViews,
            ExprEvaluatorContext staticEvalExprEvaluatorContext)
        {
            this.allowInitIndex = allowInitIndex;
            this.repositories = JoinSetComposerUtil.ToArray(repositories);
            QueryStrategies = queryStrategies;
            this.streamViews = streamViews;
            this.staticEvalExprEvaluatorContext = staticEvalExprEvaluatorContext;
        }

        /// <summary>
        ///     Returns tables.
        /// </summary>
        /// <value>tables for stream.</value>
        protected EventTable[][] Tables { get; } = new EventTable[0][];

        /// <summary>
        ///     Returns query strategies.
        /// </summary>
        /// <value>query strategies</value>
        protected QueryStrategy[] QueryStrategies { get; }

        public bool AllowsInit()
        {
            return allowInitIndex;
        }

        public void Init(
            EventBean[][] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (!allowInitIndex) {
                throw new IllegalStateException("Initialization by events not supported");
            }

            if (repositories == null) {
                return;
            }

            for (var i = 0; i < eventsPerStream.Length; i++) {
                if (eventsPerStream[i] != null && repositories[i] != null) {
                    for (var j = 0; j < repositories[i].Length; j++) {
                        repositories[i][j].Add(eventsPerStream[i], exprEvaluatorContext);
                    }
                }
            }
        }

        public void Destroy()
        {
            if (repositories == null) {
                return;
            }

            for (var i = 0; i < repositories.Length; i++) {
                if (repositories[i] != null) {
                    foreach (var table in repositories[i]) {
                        table.Destroy();
                    }
                }
            }
        }

        public UniformPair<ISet<MultiKey<EventBean>>> Join(
            EventBean[][] newDataPerStream,
            EventBean[][] oldDataPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
            instrumentationCommon.QJoinCompositionHistorical();

            oldResults.Clear();
            newResults.Clear();

            // join old data
            for (var i = 0; i < oldDataPerStream.Length; i++) {
                if (oldDataPerStream[i] != null) {
                    instrumentationCommon.QJoinCompositionQueryStrategy(false, i, oldDataPerStream[i]);
                    QueryStrategies[i].Lookup(oldDataPerStream[i], oldResults, exprEvaluatorContext);
                    instrumentationCommon.AJoinCompositionQueryStrategy();
                }
            }

            if (repositories != null) {
                // We add and remove data in one call to each index.
                // Most indexes will add first then remove as newdata and olddata may contain the same event.
                // Unique indexes may remove then add.
                for (var stream = 0; stream < newDataPerStream.Length; stream++) {
                    instrumentationCommon.QJoinCompositionStepUpdIndex(
                        stream,
                        newDataPerStream[stream],
                        oldDataPerStream[stream]);
                    for (var j = 0; j < repositories[stream].Length; j++) {
                        repositories[stream][j]
                            .AddRemove(
                                newDataPerStream[stream],
                                oldDataPerStream[stream],
                                exprEvaluatorContext);
                    }

                    instrumentationCommon.AJoinCompositionStepUpdIndex();
                }
            }

            // join new data
            for (var i = 0; i < newDataPerStream.Length; i++) {
                if (newDataPerStream[i] != null) {
                    instrumentationCommon.QJoinCompositionQueryStrategy(true, i, newDataPerStream[i]);
                    QueryStrategies[i].Lookup(newDataPerStream[i], newResults, exprEvaluatorContext);
                    instrumentationCommon.AJoinCompositionQueryStrategy();
                }
            }

            instrumentationCommon.AJoinCompositionHistorical(newResults, oldResults);
            return new UniformPair<ISet<MultiKey<EventBean>>>(newResults, oldResults);
        }

        public ISet<MultiKey<EventBean>> StaticJoin()
        {
            ISet<MultiKey<EventBean>> result = new LinkedHashSet<MultiKey<EventBean>>();
            var lookupEvents = new EventBean[1];

            // Assign a local cache for the thread's evaluation of the join
            // This ensures that if a SQL/method generates a row for a result set based on an input parameter, the event instance is the same
            // in the join, and thus the same row does not appear twice.
            var caches = new HistoricalDataCacheClearableMap[QueryStrategies.Length];
            AssignThreadLocalCache(streamViews, caches);

            // perform join
            try {
                // for each stream, perform query strategy
                for (var stream = 0; stream < QueryStrategies.Length; stream++) {
                    if (streamViews[stream] is HistoricalEventViewable) {
                        var historicalViewable = (HistoricalEventViewable) streamViews[stream];
                        if (historicalViewable.HasRequiredStreams) {
                            continue;
                        }

                        // there may not be a query strategy since only a full outer join may need to consider all rows
                        if (QueryStrategies[stream] != null) {
                            IEnumerator<EventBean> streamEvents = historicalViewable.GetEnumerator();
                            for (; streamEvents.MoveNext();) {
                                lookupEvents[0] = streamEvents.Current;
                                QueryStrategies[stream].Lookup(lookupEvents, result, staticEvalExprEvaluatorContext);
                            }
                        }
                    }
                    else {
                        IEnumerator<EventBean> streamEvents = streamViews[stream].GetEnumerator();
                        for (; streamEvents.MoveNext();) {
                            lookupEvents[0] = streamEvents.Current;
                            QueryStrategies[stream].Lookup(lookupEvents, result, staticEvalExprEvaluatorContext);
                        }
                    }
                }
            }
            finally {
                DeassignThreadLocalCache(streamViews, caches);
            }

            return result;
        }

        public void VisitIndexes(EventTableVisitor visitor)
        {
            visitor.Visit(repositories);
        }

        private void AssignThreadLocalCache(
            Viewable[] streamViews,
            HistoricalDataCacheClearableMap[] caches)
        {
            for (var stream = 0; stream < streamViews.Length; stream++) {
                if (streamViews[stream] is HistoricalEventViewable) {
                    var historicalViewable = (HistoricalEventViewable) streamViews[stream];
                    caches[stream] = new HistoricalDataCacheClearableMap();
                    historicalViewable.DataCacheThreadLocal.Value = caches[stream];
                }
            }
        }

        private void DeassignThreadLocalCache(
            Viewable[] streamViews,
            HistoricalDataCacheClearableMap[] caches)
        {
            for (var stream = 0; stream < streamViews.Length; stream++) {
                if (streamViews[stream] is HistoricalEventViewable) {
                    var historicalViewable = (HistoricalEventViewable) streamViews[stream];
                    historicalViewable.DataCacheThreadLocal.Value = null;
                    caches[stream].Clear();
                }
            }
        }
    }
} // end of namespace