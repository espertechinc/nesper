///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.epl.db;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// Implements the function to determine a join result set using tables/indexes and
    /// query strategy instances for each stream.
    /// </summary>
    public class JoinSetComposerHistoricalImpl : JoinSetComposer
    {
        private readonly bool _allowInitIndex;
        private readonly EventTable[][] _repositories;
        private readonly QueryStrategy[] _queryStrategies;

        // Set semantic eliminates duplicates in result set, use Linked set to preserve order
        private readonly ISet<MultiKey<EventBean>> _oldResults = new LinkedHashSet<MultiKey<EventBean>>();
        private readonly ISet<MultiKey<EventBean>> _newResults = new LinkedHashSet<MultiKey<EventBean>>();
        private readonly EventTable[][] _tables = new EventTable[0][];
        private readonly Viewable[] _streamViews;
        private readonly ExprEvaluatorContext _staticEvalExprEvaluatorContext;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="allowInitIndex">if set to <c>true</c> [allow initialize index].</param>
        /// <param name="repositories">indexes for non-historical streams</param>
        /// <param name="queryStrategies">for each stream a strategy to execute the join</param>
        /// <param name="streamViews">the viewable representing each stream</param>
        /// <param name="staticEvalExprEvaluatorContext">expression evaluation context for static (not runtime) evaluation</param>
        public JoinSetComposerHistoricalImpl(
            bool allowInitIndex,
            IDictionary<TableLookupIndexReqKey, EventTable>[] repositories,
            QueryStrategy[] queryStrategies,
            Viewable[] streamViews,
            ExprEvaluatorContext staticEvalExprEvaluatorContext)
        {
            _allowInitIndex = allowInitIndex;
            _repositories = JoinSetComposerUtil.ToArray(repositories, streamViews.Length);
            _queryStrategies = queryStrategies;
            _streamViews = streamViews;
            _staticEvalExprEvaluatorContext = staticEvalExprEvaluatorContext;
        }

        public bool AllowsInit
        {
            get { return _allowInitIndex; }
        }

        public void Init(EventBean[][] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (!_allowInitIndex)
            {
                throw new IllegalStateException("Initialization by events not supported");
            }

            if (_repositories == null)
            {
                return;
            }

            for (var i = 0; i < eventsPerStream.Length; i++)
            {
                if ((eventsPerStream[i] != null) && (_repositories[i] != null))
                {
                    for (var j = 0; j < _repositories[i].Length; j++)
                    {
                        _repositories[i][j].Add(eventsPerStream[i], exprEvaluatorContext);
                    }
                }
            }
        }

        public void Destroy()
        {
            if (_repositories == null)
            {
                return;
            }

            for (var i = 0; i < _repositories.Length; i++)
            {
                if (_repositories[i] != null)
                {
                    foreach (var table in _repositories[i])
                    {
                        table.Destroy();
                    }
                }
            }
        }

        public UniformPair<ISet<MultiKey<EventBean>>> Join(EventBean[][] newDataPerStream, EventBean[][] oldDataPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QJoinCompositionHistorical(); }

            _oldResults.Clear();
            _newResults.Clear();

            // join old data
            for (var i = 0; i < oldDataPerStream.Length; i++)
            {
                if (oldDataPerStream[i] != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QJoinCompositionQueryStrategy(false, i, oldDataPerStream[i]); }
                    _queryStrategies[i].Lookup(oldDataPerStream[i], _oldResults, exprEvaluatorContext);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AJoinCompositionQueryStrategy(); }
                }
            }

            if (_repositories != null)
            {
                // We add and remove data in one call to each index.
                // Most indexes will add first then remove as newdata and olddata may contain the same event.
                // Unique indexes may remove then add.
                for (var stream = 0; stream < newDataPerStream.Length; stream++)
                {
                    for (var j = 0; j < _repositories[stream].Length; j++)
                    {
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QJoinCompositionStepUpdIndex(stream, newDataPerStream[stream], oldDataPerStream[stream]); }
                        _repositories[stream][j].AddRemove(newDataPerStream[stream], oldDataPerStream[stream], exprEvaluatorContext);
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AJoinCompositionStepUpdIndex(); }
                    }
                }
            }

            // join new data
            for (var i = 0; i < newDataPerStream.Length; i++)
            {
                if (newDataPerStream[i] != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QJoinCompositionQueryStrategy(true, i, newDataPerStream[i]); }
                    _queryStrategies[i].Lookup(newDataPerStream[i], _newResults, exprEvaluatorContext);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AJoinCompositionQueryStrategy(); }
                }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AJoinCompositionHistorical(_newResults, _oldResults); }
            return new UniformPair<ISet<MultiKey<EventBean>>>(_newResults, _oldResults);
        }

        /// <summary>Returns tables. </summary>
        /// <value>tables for stream.</value>
        protected EventTable[][] Tables
        {
            get { return _tables; }
        }

        /// <summary>Returns query strategies. </summary>
        /// <value>query strategies</value>
        protected QueryStrategy[] QueryStrategies
        {
            get { return _queryStrategies; }
        }

        public ISet<MultiKey<EventBean>> StaticJoin()
        {
            var result = new LinkedHashSet<MultiKey<EventBean>>();
            var lookupEvents = new EventBean[1];

            // Assign a local cache for the thread's evaluation of the join
            // This ensures that if a SQL/method generates a row for a result set based on an input parameter, the event instance is the same
            // in the join, and thus the same row does not appear twice.
            var caches = new DataCacheClearableMap[_queryStrategies.Length];
            AssignThreadLocalCache(_streamViews, caches);

            // perform join
            try
            {
                // for each stream, perform query strategy
                for (var stream = 0; stream < _queryStrategies.Length; stream++)
                {
                    if (_streamViews[stream] is HistoricalEventViewable)
                    {
                        var historicalViewable = (HistoricalEventViewable)_streamViews[stream];
                        if (historicalViewable.HasRequiredStreams)
                        {
                            continue;
                        }

                        // there may not be a query strategy since only a full outer join may need to consider all rows
                        if (_queryStrategies[stream] != null)
                        {
                            var streamEvents = historicalViewable.GetEnumerator();
                            for (; streamEvents.MoveNext(); )
                            {
                                lookupEvents[0] = streamEvents.Current;
                                _queryStrategies[stream].Lookup(lookupEvents, result, _staticEvalExprEvaluatorContext);
                            }
                        }
                    }
                    else
                    {
                        var streamEvents = _streamViews[stream].GetEnumerator();
                        for (; streamEvents.MoveNext(); )
                        {
                            lookupEvents[0] = streamEvents.Current;
                            _queryStrategies[stream].Lookup(lookupEvents, result, _staticEvalExprEvaluatorContext);
                        }
                    }
                }
            }
            finally
            {
                DeassignThreadLocalCache(_streamViews, caches);
            }

            return result;
        }

        public void VisitIndexes(StatementAgentInstancePostLoadIndexVisitor visitor)
        {
            visitor.Visit(_repositories);
        }

        private void AssignThreadLocalCache(Viewable[] streamViews, DataCacheClearableMap[] caches)
        {
            for (var stream = 0; stream < streamViews.Length; stream++)
            {
                if (streamViews[stream] is HistoricalEventViewable)
                {
                    var historicalViewable = (HistoricalEventViewable)streamViews[stream];
                    caches[stream] = new DataCacheClearableMap();
                    historicalViewable.DataCacheThreadLocal.Value = caches[stream];
                }
            }
        }

        private void DeassignThreadLocalCache(Viewable[] streamViews, DataCacheClearableMap[] caches)
        {
            for (var stream = 0; stream < streamViews.Length; stream++)
            {
                if (streamViews[stream] is HistoricalEventViewable)
                {
                    var historicalViewable = (HistoricalEventViewable)streamViews[stream];
                    historicalViewable.DataCacheThreadLocal.Value = null;
                    caches[stream].Clear();
                }
            }
        }
    }
}
