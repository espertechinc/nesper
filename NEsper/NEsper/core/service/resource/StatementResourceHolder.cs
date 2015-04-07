///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.pattern;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service.resource
{
    public class StatementResourceHolder
    {
        public StatementResourceHolder(IReaderWriterLock agentInstanceLock, Viewable[] topViewables, Viewable[] eventStreamViewables, EvalRootState[] patternRoots, AggregationService aggegationService, IDictionary<ExprSubselectNode, SubSelectStrategyHolder> subselectStrategies, StatementAgentInstancePostLoad postLoad)
        {
            AgentInstanceLock = agentInstanceLock;
            TopViewables = topViewables;
            EventStreamViewables = eventStreamViewables;
            PatternRoots = patternRoots;
            AggegationService = aggegationService;
            SubselectStrategies = subselectStrategies;
            PostLoad = postLoad;
        }

        public IReaderWriterLock AgentInstanceLock { get; private set; }

        public Viewable[] TopViewables { get; private set; }

        public Viewable[] EventStreamViewables { get; private set; }

        public EvalRootState[] PatternRoots { get; private set; }

        public AggregationService AggegationService { get; private set; }

        public IDictionary<ExprSubselectNode, SubSelectStrategyHolder> SubselectStrategies { get; private set; }

        public StatementAgentInstancePostLoad PostLoad { get; private set; }
    }
}
