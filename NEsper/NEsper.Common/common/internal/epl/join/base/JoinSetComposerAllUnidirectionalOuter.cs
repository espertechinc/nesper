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
using com.espertech.esper.common.@internal.epl.join.strategy;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    /// <summary>
    ///     Implements the function to determine a join result for a all-unidirectional full-outer-join (all streams),
    ///     in which a single stream's events are ever only evaluated and repositories don't exist.
    /// </summary>
    public class JoinSetComposerAllUnidirectionalOuter : JoinSetComposer
    {
        private readonly ISet<MultiKey<EventBean>> emptyResults = new LinkedHashSet<MultiKey<EventBean>>();
        private readonly ISet<MultiKey<EventBean>> newResults = new LinkedHashSet<MultiKey<EventBean>>();
        private readonly QueryStrategy[] queryStrategies;

        public JoinSetComposerAllUnidirectionalOuter(QueryStrategy[] queryStrategies)
        {
            this.queryStrategies = queryStrategies;
        }

        public bool AllowsInit()
        {
            return false;
        }

        public void Init(
            EventBean[][] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public void Destroy()
        {
        }

        public UniformPair<ISet<MultiKey<EventBean>>> Join(
            EventBean[][] newDataPerStream,
            EventBean[][] oldDataPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
            instrumentationCommon.QJoinCompositionStreamToWin();

            newResults.Clear();

            for (var i = 0; i < queryStrategies.Length; i++) {
                if (newDataPerStream[i] != null) {
                    instrumentationCommon.QJoinCompositionQueryStrategy(true, i, newDataPerStream[i]);
                    queryStrategies[i].Lookup(newDataPerStream[i], newResults, exprEvaluatorContext);
                    instrumentationCommon.AJoinCompositionQueryStrategy();
                }
            }

            instrumentationCommon.AJoinCompositionStreamToWin(newResults);
            return new UniformPair<ISet<MultiKey<EventBean>>>(newResults, emptyResults);
        }

        public ISet<MultiKey<EventBean>> StaticJoin()
        {
            throw new UnsupportedOperationException("Iteration over a unidirectional join is not supported");
        }

        public void VisitIndexes(EventTableVisitor visitor)
        {
        }
    }
} // end of namespace