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
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// Implements the function to determine a join result for a all-unidirectional full-outer-join (all streams),
    /// in which a single stream's events are ever only evaluated and repositories don't exist.
    /// </summary>
    public class JoinSetComposerAllUnidirectionalOuter : JoinSetComposer
    {
        private readonly QueryStrategy[] _queryStrategies;

        private readonly ISet<MultiKey<EventBean>> _emptyResults = new LinkedHashSet<MultiKey<EventBean>>();
        private readonly ISet<MultiKey<EventBean>> _newResults = new LinkedHashSet<MultiKey<EventBean>>();

        public JoinSetComposerAllUnidirectionalOuter(QueryStrategy[] queryStrategies)
        {
            _queryStrategies = queryStrategies;
        }

        public bool AllowsInit
        {
            get { return false; }
        }

        public void Init(EventBean[][] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
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
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QJoinCompositionStreamToWin();
            }
            _newResults.Clear();

            for (int i = 0; i < _queryStrategies.Length; i++)
            {
                if (newDataPerStream[i] != null)
                {
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().QJoinCompositionQueryStrategy(true, i, newDataPerStream[i]);
                    }
                    _queryStrategies[i].Lookup(newDataPerStream[i], _newResults, exprEvaluatorContext);
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().AJoinCompositionQueryStrategy();
                    }
                }
            }

            return new UniformPair<ISet<MultiKey<EventBean>>>(_newResults, _emptyResults);
        }

        public ISet<MultiKey<EventBean>> StaticJoin()
        {
            throw new UnsupportedOperationException("Iteration over a unidirectional join is not supported");
        }

        public void VisitIndexes(StatementAgentInstancePostLoadIndexVisitor visitor)
        {
        }
    }
} // end of namespace
