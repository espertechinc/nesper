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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// Implements the function to determine a join result set using tables/indexes and query strategy instances for each stream.
    /// </summary>
    public class JoinSetComposerFAFImpl : JoinSetComposerImpl
    {
        private readonly bool _isOuterJoins;

        public JoinSetComposerFAFImpl(IDictionary<TableLookupIndexReqKey, EventTable>[] repositories, QueryStrategy[] queryStrategies, bool isPureSelfJoin, ExprEvaluatorContext exprEvaluatorContext, bool joinRemoveStream, bool outerJoins)
            : base(false, repositories, queryStrategies, isPureSelfJoin, exprEvaluatorContext, joinRemoveStream)
        {
            _isOuterJoins = outerJoins;
        }

        public override void Init(EventBean[][] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
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
            NewResults.Clear();

            // We add and remove data in one call to each index.
            // Most indexes will add first then remove as newdata and olddata may contain the same event.
            // Unique indexes may remove then add.
            for (int stream = 0; stream < newDataPerStream.Length; stream++)
            {
                var repositories = Repositories;
                for (int j = 0; j < repositories[stream].Length; j++)
                {
                    repositories[stream][j].AddRemove(newDataPerStream[stream], oldDataPerStream[stream], exprEvaluatorContext);
                }
            }

            // for outer joins, execute each query strategy
            if (_isOuterJoins)
            {
                for (int i = 0; i < newDataPerStream.Length; i++)
                {
                    if (newDataPerStream[i] != null)
                    {
                        QueryStrategies[i].Lookup(newDataPerStream[i], NewResults, exprEvaluatorContext);
                    }
                }
            }
            // handle all-inner joins by executing the smallest number of event's query strategy
            else
            {
                int minStream = -1;
                int minStreamCount = -1;
                for (int i = 0; i < newDataPerStream.Length; i++)
                {
                    if (newDataPerStream[i] != null)
                    {
                        if (newDataPerStream[i].Length == 0)
                        {
                            minStream = -1;
                            break;
                        }
                        if (newDataPerStream[i].Length > minStreamCount)
                        {
                            minStream = i;
                            minStreamCount = newDataPerStream[i].Length;
                        }
                    }
                }
                if (minStream != -1)
                {
                    QueryStrategies[minStream].Lookup(newDataPerStream[minStream], NewResults, exprEvaluatorContext);
                }
            }

            return new UniformPair<ISet<MultiKey<EventBean>>>(NewResults, OldResults);
        }

        public override ISet<MultiKey<EventBean>> StaticJoin()
        {
            // no action
            return null;
        }
    }
}
