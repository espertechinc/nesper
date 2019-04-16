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
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.@join.strategy;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    /// <summary>
    ///     Implements the function to determine a join result for a unidirectional stream-to-window joins,
    ///     in which a single stream's events are ever only evaluated using a query strategy.
    /// </summary>
    public class JoinSetComposerStreamToWinImpl : JoinSetComposer
    {
        private readonly bool allowInitIndex;

        private readonly bool isResetSelfJoinRepositories;
        private readonly QueryStrategy queryStrategy;
        private readonly EventTable[][] repositories;
        private readonly bool[] selfJoinRepositoryResets;
        private readonly int streamNumber;

        private readonly ISet<MultiKey<EventBean>> emptyResults = new LinkedHashSet<MultiKey<EventBean>>();
        private readonly ISet<MultiKey<EventBean>> newResults = new LinkedHashSet<MultiKey<EventBean>>();

        public JoinSetComposerStreamToWinImpl(
            bool allowInitIndex,
            IDictionary<TableLookupIndexReqKey, EventTable>[] repositories,
            bool isPureSelfJoin,
            int streamNumber,
            QueryStrategy queryStrategy,
            bool[] selfJoinRepositoryResets)
        {
            this.allowInitIndex = allowInitIndex;
            this.repositories = JoinSetComposerUtil.ToArray();
            this.streamNumber = streamNumber;
            this.queryStrategy = queryStrategy;

            this.selfJoinRepositoryResets = selfJoinRepositoryResets;
            if (isPureSelfJoin) {
                isResetSelfJoinRepositories = true;
                CompatExtensions.Fill(selfJoinRepositoryResets, true);
            }
            else {
                var flag = false;
                foreach (var selfJoinRepositoryReset in selfJoinRepositoryResets) {
                    flag |= selfJoinRepositoryReset;
                }

                isResetSelfJoinRepositories = flag;
            }
        }

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

            for (var i = 0; i < eventsPerStream.Length; i++) {
                if (eventsPerStream[i] != null && i != streamNumber) {
                    for (var j = 0; j < repositories[i].Length; j++) {
                        repositories[i][j].Add(eventsPerStream[i], exprEvaluatorContext);
                    }
                }
            }
        }

        public void Destroy()
        {
            foreach (var repository in repositories) {
                if (repository != null) {
                    foreach (var table in repository) {
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
            instrumentationCommon.QJoinCompositionStreamToWin();

            newResults.Clear();

            // We add and remove data in one call to each index.
            // Most indexes will add first then remove as newdata and olddata may contain the same event.
            // Unique indexes may remove then add.
            for (var stream = 0; stream < newDataPerStream.Length; stream++) {
                if (stream != streamNumber) {
                    instrumentationCommon.QJoinCompositionStepUpdIndex(
                        stream, newDataPerStream[stream], oldDataPerStream[stream]);
                    for (var j = 0; j < repositories[stream].Length; j++) {
                        repositories[stream][j].AddRemove(
                            newDataPerStream[stream], oldDataPerStream[stream], exprEvaluatorContext);
                    }

                    instrumentationCommon.AJoinCompositionStepUpdIndex();
                }
            }

            // join new data
            if (newDataPerStream[streamNumber] != null) {
                instrumentationCommon.QJoinCompositionQueryStrategy(true, streamNumber, newDataPerStream[streamNumber]);
                queryStrategy.Lookup(newDataPerStream[streamNumber], newResults, exprEvaluatorContext);
                instrumentationCommon.AJoinCompositionQueryStrategy();
            }

            // on self-joins there can be repositories which are temporary for join execution
            if (isResetSelfJoinRepositories) {
                for (var i = 0; i < selfJoinRepositoryResets.Length; i++) {
                    if (!selfJoinRepositoryResets[i]) {
                        continue;
                    }

                    for (var j = 0; j < repositories[i].Length; j++) {
                        repositories[i][j].Clear();
                    }
                }
            }

            exprEvaluatorContext.InstrumentationProvider.AJoinCompositionStreamToWin(newResults);

            return new UniformPair<ISet<MultiKey<EventBean>>>(newResults, emptyResults);
        }

        public ISet<MultiKey<EventBean>> StaticJoin()
        {
            throw new UnsupportedOperationException("Iteration over a unidirectional join is not supported");
        }

        public void VisitIndexes(EventTableVisitor visitor)
        {
            visitor.Visit(repositories);
        }
    }
} // end of namespace