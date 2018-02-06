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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// Implements the function to determine a join result set using tables/indexes and 
    /// query strategy instances for each stream.
    /// </summary>
    public class JoinSetComposerImpl : JoinSetComposer
    {
        private readonly bool _allowInitIndex;
        private readonly EventTable[][] _repositories;
        private readonly QueryStrategy[] _queryStrategies;
        private readonly bool _isPureSelfJoin;
        private readonly ExprEvaluatorContext _exprEvaluatorContext;
        private readonly bool _joinRemoveStream;

        // Set semantic eliminates duplicates in result set, use Linked set to preserve order
        protected ISet<MultiKey<EventBean>> OldResults = new LinkedHashSet<MultiKey<EventBean>>();
        protected ISet<MultiKey<EventBean>> NewResults = new LinkedHashSet<MultiKey<EventBean>>();

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="allowInitIndex">if set to <c>true</c> [allow initialize index].</param>
        /// <param name="repositories">for each stream an array of (indexed/unindexed) tables for lookup.</param>
        /// <param name="queryStrategies">for each stream a strategy to execute the join</param>
        /// <param name="isPureSelfJoin">for self-join only</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <param name="joinRemoveStream">if set to <c>true</c> [join remove stream].</param>
        public JoinSetComposerImpl(
            bool allowInitIndex,
            IDictionary<TableLookupIndexReqKey, EventTable>[] repositories,
            QueryStrategy[] queryStrategies,
            bool isPureSelfJoin,
            ExprEvaluatorContext exprEvaluatorContext,
            bool joinRemoveStream)
        {
            _allowInitIndex = allowInitIndex;
            _repositories = JoinSetComposerUtil.ToArray(repositories);
            _queryStrategies = queryStrategies;
            _isPureSelfJoin = isPureSelfJoin;
            _exprEvaluatorContext = exprEvaluatorContext;
            _joinRemoveStream = joinRemoveStream;
        }

        public bool AllowsInit
        {
            get { return _allowInitIndex; }
        }

        public virtual void Init(EventBean[][] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (!_allowInitIndex)
            {
                throw new IllegalStateException("Initialization by events not supported");
            }

            for (var i = 0; i < eventsPerStream.Length; i++)
            {
                if (eventsPerStream[i] != null)
                {
                    for (var j = 0; j < _repositories[i].Length; j++)
                    {
                        _repositories[i][j].Add(eventsPerStream[i], exprEvaluatorContext);
                    }
                }
            }
        }

        public virtual void Destroy()
        {
            unchecked
            {
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
        }

        public virtual UniformPair<ISet<MultiKey<EventBean>>> Join(
            EventBean[][] newDataPerStream,
            EventBean[][] oldDataPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QJoinCompositionWinToWin(); }

            OldResults.Clear();
            NewResults.Clear();

            // join old data
            if (_joinRemoveStream)
            {
                for (var i = 0; i < oldDataPerStream.Length; i++)
                {
                    if (oldDataPerStream[i] != null)
                    {
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QJoinCompositionQueryStrategy(false, i, oldDataPerStream[i]); }
                        _queryStrategies[i].Lookup(oldDataPerStream[i], OldResults, exprEvaluatorContext);
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AJoinCompositionQueryStrategy(); }
                    }
                }
            }

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

            // join new data
            for (var i = 0; i < newDataPerStream.Length; i++)
            {
                if (newDataPerStream[i] != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QJoinCompositionQueryStrategy(true, i, newDataPerStream[i]); }
                    _queryStrategies[i].Lookup(newDataPerStream[i], NewResults, exprEvaluatorContext);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AJoinCompositionQueryStrategy(); }
                }
            }

            // on self-joins there can be repositories which are temporary for join execution
            if (_isPureSelfJoin)
            {
                foreach (var repository in _repositories)
                {
                    foreach (var aRepository in repository)
                    {
                        aRepository.Clear();
                    }
                }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AJoinCompositionWinToWin(NewResults, OldResults); }
            return new UniformPair<ISet<MultiKey<EventBean>>>(NewResults, OldResults);
        }

        /// <summary>
        /// Gets the repositories.
        /// </summary>
        /// <value>The repositories.</value>
        public EventTable[][] Repositories
        {
            get { return _repositories; }
        }

        /// <summary>Returns tables. </summary>
        /// <value>tables for stream.</value>
        internal EventTable[][] Tables
        {
            get { return _repositories; }
        }

        /// <summary>Returns query strategies. </summary>
        /// <value>query strategies</value>
        internal QueryStrategy[] QueryStrategies
        {
            get { return _queryStrategies; }
        }

        public virtual ISet<MultiKey<EventBean>> StaticJoin()
        {
            var result = new LinkedHashSet<MultiKey<EventBean>>();
            var lookupEvents = new EventBean[1];

            // for each stream, perform query strategy
            for (var stream = 0; stream < _queryStrategies.Length; stream++)
            {
                if (_repositories[stream] == null)
                {
                    continue;
                }

                IEnumerator<EventBean> streamEvents = _repositories[stream][0].GetEnumerator();
                while (streamEvents.MoveNext())
                {
                    lookupEvents[0] = streamEvents.Current;
                    _queryStrategies[stream].Lookup(lookupEvents, result, _exprEvaluatorContext);
                }
            }

            return result;
        }

        public void VisitIndexes(StatementAgentInstancePostLoadIndexVisitor visitor)
        {
            visitor.Visit(_repositories);
        }
    }
}