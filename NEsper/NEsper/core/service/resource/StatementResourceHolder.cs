///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.named;
using com.espertech.esper.pattern;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service.resource
{
    public class StatementResourceHolder
    {
        public StatementResourceHolder(AgentInstanceContext agentInstanceContext)
        {
            AgentInstanceContext = agentInstanceContext;
            SubselectStrategies = Collections.GetEmptyMap<ExprSubselectNode, SubSelectStrategyHolder>();
        }

        public AgentInstanceContext AgentInstanceContext { get; internal set; }

        public Viewable[] TopViewables { get; internal set; }

        public Viewable[] EventStreamViewables { get; internal set; }

        public EvalRootState[] PatternRoots { get; internal set; }

        public AggregationService AggregationService { get; internal set; }

        public IDictionary<ExprSubselectNode, SubSelectStrategyHolder> SubselectStrategies { get; internal set; }

        public StatementAgentInstancePostLoad PostLoad { get; internal set; }

        public NamedWindowProcessorInstance NamedWindowProcessorInstance { get; internal set; }

        public StatementResourceExtension StatementResourceExtension { get; internal set; }
    }
}
