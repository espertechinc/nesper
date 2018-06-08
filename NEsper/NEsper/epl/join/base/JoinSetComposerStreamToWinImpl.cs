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
    /// Implements the function to determine a join result for a unidirectional stream-to-window 
    /// joins, in which a single stream's events are ever only evaluated using a query strategy.
    /// </summary>
    public class JoinSetComposerStreamToWinImpl : JoinSetComposer
    {
        private readonly bool _allowInitIndex;
        private readonly EventTable[][] _repositories;
        private readonly int _streamNumber;
        private readonly QueryStrategy _queryStrategy;

        private readonly bool _isResetSelfJoinRepositories;
        private readonly bool[] _selfJoinRepositoryResets;

        private readonly ISet<MultiKey<EventBean>> _emptyResults = new LinkedHashSet<MultiKey<EventBean>>();
        private readonly ISet<MultiKey<EventBean>> _newResults = new LinkedHashSet<MultiKey<EventBean>>();

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="allowInitIndex">if set to <c>true</c> [allow initialize index].</param>
        /// <param name="repositories">for each stream an array of (indexed/unindexed) tables for lookup.</param>
        /// <param name="isPureSelfJoin">for self-joins</param>
        /// <param name="streamNumber">is the undirectional stream</param>
        /// <param name="queryStrategy">is the lookup query strategy for the stream</param>
        /// <param name="selfJoinRepositoryResets">indicators for any stream's table that reset after strategy executon</param>
        public JoinSetComposerStreamToWinImpl(
            bool allowInitIndex,
            IDictionary<TableLookupIndexReqKey, EventTable>[] repositories,
            bool isPureSelfJoin,
            int streamNumber,
            QueryStrategy queryStrategy,
            bool[] selfJoinRepositoryResets)
        {
            _allowInitIndex = allowInitIndex;
            _repositories = JoinSetComposerUtil.ToArray(repositories);
            _streamNumber = streamNumber;
            _queryStrategy = queryStrategy;

            _selfJoinRepositoryResets = selfJoinRepositoryResets;
            if (isPureSelfJoin)
            {
                _isResetSelfJoinRepositories = true;
                selfJoinRepositoryResets.Fill(true);
            }
            else
            {
                bool flag = false;
                foreach (bool selfJoinRepositoryReset in selfJoinRepositoryResets)
                {
                    flag |= selfJoinRepositoryReset;
                }
                _isResetSelfJoinRepositories = flag;
            }
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
            for (int i = 0; i < eventsPerStream.Length; i++)
            {
                if ((eventsPerStream[i] != null) && (i != _streamNumber))
                {
                    for (int j = 0; j < _repositories[i].Length; j++)
                    {
                        _repositories[i][j].Add(eventsPerStream[i], exprEvaluatorContext);
                    }
                }
            }
        }

        public void Destroy()
        {
            foreach (EventTable[] repository in _repositories)
            {
                if (repository != null)
                {
                    foreach (EventTable table in repository)
                    {
                        table.Destroy();
                    }
                }
            }
        }

        public UniformPair<ISet<MultiKey<EventBean>>> Join(EventBean[][] newDataPerStream, EventBean[][] oldDataPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QJoinCompositionStreamToWin(); }
            _newResults.Clear();

            // We add and remove data in one call to each index.
            // Most indexes will add first then remove as newdata and olddata may contain the same event.
            // Unique indexes may remove then add.
            for (int stream = 0; stream < newDataPerStream.Length; stream++)
            {
                if (stream != _streamNumber)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QJoinCompositionStepUpdIndex(stream, newDataPerStream[stream], oldDataPerStream[stream]); }
                    for (int j = 0; j < _repositories[stream].Length; j++)
                    {
                        _repositories[stream][j].AddRemove(newDataPerStream[stream], oldDataPerStream[stream], exprEvaluatorContext);
                    }
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AJoinCompositionStepUpdIndex(); }
                }
            }

            // join new data
            if (newDataPerStream[_streamNumber] != null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QJoinCompositionQueryStrategy(true, _streamNumber, newDataPerStream[_streamNumber]); }
                _queryStrategy.Lookup(newDataPerStream[_streamNumber], _newResults, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AJoinCompositionQueryStrategy(); }
            }

            // on self-joins there can be repositories which are temporary for join execution
            if (_isResetSelfJoinRepositories)
            {
                for (int i = 0; i < _selfJoinRepositoryResets.Length; i++)
                {
                    if (!_selfJoinRepositoryResets[i])
                    {
                        continue;
                    }
                    for (int j = 0; j < _repositories[i].Length; j++)
                    {
                        _repositories[i][j].Clear();
                    }
                }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AJoinCompositionStreamToWin(_newResults); }
            return new UniformPair<ISet<MultiKey<EventBean>>>(_newResults, _emptyResults);
        }

        public ISet<MultiKey<EventBean>> StaticJoin()
        {
            throw new UnsupportedOperationException("Iteration over a unidirectional join is not supported");
        }

        public void VisitIndexes(StatementAgentInstancePostLoadIndexVisitor visitor)
        {
            visitor.Visit(_repositories);
        }
    }
}
