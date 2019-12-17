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
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.join.strategy;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    /// <summary>
    ///     Implements the function to determine a join result set using tables/indexes and query strategy
    ///     instances for each stream.
    /// </summary>
    public class JoinSetComposerFAFImpl : JoinSetComposerImpl
    {
        private readonly bool isOuterJoins;

        public JoinSetComposerFAFImpl(
            IDictionary<TableLookupIndexReqKey, EventTable>[] repositories,
            QueryStrategy[] queryStrategies,
            bool isPureSelfJoin,
            ExprEvaluatorContext exprEvaluatorContext,
            bool joinRemoveStream,
            bool outerJoins)
            : base(false, repositories, queryStrategies, isPureSelfJoin, exprEvaluatorContext, joinRemoveStream)
        {
            isOuterJoins = outerJoins;
        }

        public override void Init(
            EventBean[][] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // no action
        }

        public override void Destroy()
        {
            // no action
        }

        public override UniformPair<ISet<MultiKey<EventBean>>> Join(
            EventBean[][] newDataPerStream,
            EventBean[][] oldDataPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            newResults.Clear();

            // We add and remove data in one call to each index.
            // Most indexes will add first then remove as newdata and olddata may contain the same event.
            // Unique indexes may remove then add.
            for (var stream = 0; stream < newDataPerStream.Length; stream++) {
                for (var j = 0; j < repositories[stream].Length; j++) {
                    repositories[stream][j]
                        .AddRemove(
                            newDataPerStream[stream],
                            oldDataPerStream[stream],
                            exprEvaluatorContext);
                }
            }

            // for outer joins, execute each query strategy
            if (isOuterJoins) {
                for (var i = 0; i < newDataPerStream.Length; i++) {
                    if (newDataPerStream[i] != null) {
                        queryStrategies[i].Lookup(newDataPerStream[i], newResults, exprEvaluatorContext);
                    }
                }
            }
            else {
                // handle all-inner joins by executing the smallest number of event's query strategy
                var minStream = -1;
                var minStreamCount = -1;
                for (var i = 0; i < newDataPerStream.Length; i++) {
                    if (newDataPerStream[i] != null) {
                        if (newDataPerStream[i].Length == 0) {
                            minStream = -1;
                            break;
                        }

                        if (newDataPerStream[i].Length > minStreamCount) {
                            minStream = i;
                            minStreamCount = newDataPerStream[i].Length;
                        }
                    }
                }

                if (minStream != -1) {
                    queryStrategies[minStream].Lookup(newDataPerStream[minStream], newResults, exprEvaluatorContext);
                }
            }

            return new UniformPair<ISet<MultiKey<EventBean>>>(newResults, oldResults);
        }

        public override ISet<MultiKey<EventBean>> StaticJoin()
        {
            // no action
            return null;
        }
    }
} // end of namespace