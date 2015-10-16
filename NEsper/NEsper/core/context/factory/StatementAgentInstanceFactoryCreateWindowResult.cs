///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.named;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactoryCreateWindowResult : StatementAgentInstanceFactoryResult
    {
        public StatementAgentInstanceFactoryCreateWindowResult(Viewable finalView, StopCallback stopCallback, AgentInstanceContext agentInstanceContext, Viewable eventStreamParentViewable, StatementAgentInstancePostLoad postLoad, Viewable topView, NamedWindowProcessorInstance processorInstance, ViewableActivationResult viewableActivationResult)
            : base(finalView, stopCallback, agentInstanceContext, null,
                   Collections.GetEmptyMap<ExprSubselectNode, SubSelectStrategyHolder>(),
                   Collections.GetEmptyMap<ExprPriorNode, ExprPriorEvalStrategy>(),
                   Collections.GetEmptyMap<ExprPreviousNode, ExprPreviousEvalStrategy>(),
                   null,
                   Collections.GetEmptyMap<ExprTableAccessNode, ExprTableAccessEvalStrategy>(),
                   Collections.GetEmptyList<StatementAgentInstancePreload>())
        {
            EventStreamParentViewable = eventStreamParentViewable;
            PostLoad = postLoad;
            TopView = topView;
            ProcessorInstance = processorInstance;
            ViewableActivationResult = viewableActivationResult;
        }

        public Viewable EventStreamParentViewable { get; private set; }

        public StatementAgentInstancePostLoad PostLoad { get; private set; }

        public Viewable TopView { get; private set; }

        public NamedWindowProcessorInstance ProcessorInstance { get; private set; }

        public ViewableActivationResult ViewableActivationResult { get; private set; }
    }
}
