///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.join.strategy;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    /// <summary>
    ///     Implements the function to determine a join result set using tables/indexes and query strategy
    ///     instances for each stream.
    /// </summary>
    public class JoinSetComposerImpl : JoinSetComposer
    {
        private readonly bool allowInitIndex;
        private readonly ExprEvaluatorContext exprEvaluatorContext;
        private readonly bool isPureSelfJoin;
        private readonly bool joinRemoveStream;
        internal readonly QueryStrategy[] queryStrategies;
        internal readonly EventTable[][] repositories;

        // Set semantic eliminates duplicates in result set, use Linked set to preserve order
        internal ISet<MultiKeyArrayOfKeys<EventBean>> oldResults = new LinkedHashSet<MultiKeyArrayOfKeys<EventBean>>();
        internal ISet<MultiKeyArrayOfKeys<EventBean>> newResults = new LinkedHashSet<MultiKeyArrayOfKeys<EventBean>>();

        public JoinSetComposerImpl(
            bool allowInitIndex,
            IDictionary<TableLookupIndexReqKey, EventTable>[] repositories,
            QueryStrategy[] queryStrategies,
            bool isPureSelfJoin,
            ExprEvaluatorContext exprEvaluatorContext,
            bool joinRemoveStream)
        {
            this.allowInitIndex = allowInitIndex;
            this.repositories = JoinSetComposerUtil.ToArray(repositories);
            this.queryStrategies = queryStrategies;
            this.isPureSelfJoin = isPureSelfJoin;
            this.exprEvaluatorContext = exprEvaluatorContext;
            this.joinRemoveStream = joinRemoveStream;
        }

        /// <summary>
        ///     Returns tables.
        /// </summary>
        /// <value>tables for stream.</value>
        protected EventTable[][] Tables => repositories;

        /// <summary>
        ///     Returns query strategies.
        /// </summary>
        /// <value>query strategies</value>
        protected QueryStrategy[] QueryStrategies => queryStrategies;

        public virtual bool AllowsInit()
        {
            return allowInitIndex;
        }

        public virtual void Init(
            EventBean[][] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (!allowInitIndex) {
                throw new IllegalStateException("Initialization by events not supported");
            }

            for (var i = 0; i < eventsPerStream.Length; i++) {
                if (eventsPerStream[i] != null) {
                    for (var j = 0; j < repositories[i].Length; j++) {
                        repositories[i][j].Add(eventsPerStream[i], exprEvaluatorContext);
                    }
                }
            }
        }

        public virtual void Destroy()
        {
            for (var i = 0; i < repositories.Length; i++) {
                if (repositories[i] != null) {
                    foreach (var table in repositories[i]) {
                        table.Destroy();
                    }
                }
            }
        }

        public virtual UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>> Join(
            EventBean[][] newDataPerStream,
            EventBean[][] oldDataPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
            instrumentationCommon.QJoinCompositionWinToWin();

            oldResults.Clear();
            newResults.Clear();

            // join old data
            if (joinRemoveStream) {
                for (var i = 0; i < oldDataPerStream.Length; i++) {
                    if (oldDataPerStream[i] != null) {
                        instrumentationCommon.QJoinCompositionQueryStrategy(false, i, oldDataPerStream[i]);
                        queryStrategies[i].Lookup(oldDataPerStream[i], oldResults, exprEvaluatorContext);
                        instrumentationCommon.AJoinCompositionQueryStrategy();
                    }
                }
            }

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

            // join new data
            for (var i = 0; i < newDataPerStream.Length; i++) {
                if (newDataPerStream[i] != null) {
                    instrumentationCommon.QJoinCompositionQueryStrategy(true, i, newDataPerStream[i]);
                    queryStrategies[i].Lookup(newDataPerStream[i], newResults, exprEvaluatorContext);
                    instrumentationCommon.AJoinCompositionQueryStrategy();
                }
            }

            // on self-joins there can be repositories which are temporary for join execution
            if (isPureSelfJoin) {
                foreach (var repository in repositories) {
                    foreach (var aRepository in repository) {
                        aRepository.Clear();
                    }
                }
            }

            instrumentationCommon.AJoinCompositionWinToWin(newResults, oldResults);
            return new UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>(newResults, oldResults);
        }

        public virtual ISet<MultiKeyArrayOfKeys<EventBean>> StaticJoin()
        {
            ISet<MultiKeyArrayOfKeys<EventBean>> result = new LinkedHashSet<MultiKeyArrayOfKeys<EventBean>>();
            var lookupEvents = new EventBean[1];

            // for each stream, perform query strategy
            for (var stream = 0; stream < queryStrategies.Length; stream++) {
                if (repositories[stream] == null) {
                    continue;
                }

                var streamEvents = repositories[stream][0].GetEnumerator();
                for (; streamEvents.MoveNext();) {
                    lookupEvents[0] = streamEvents.Current;
                    queryStrategies[stream].Lookup(lookupEvents, result, exprEvaluatorContext);
                }
            }

            return result;
        }

        public void VisitIndexes(EventTableVisitor visitor)
        {
            visitor.Visit(repositories);
        }
    }
} // end of namespace