///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createwindow
{
    public class StatementAgentInstanceFactoryCreateNwResult : StatementAgentInstanceFactoryResult
    {
        private readonly Viewable _eventStreamParentViewable;
        private readonly Viewable _topView;
        private readonly NamedWindowInstance _namedWindowInstance;
        private readonly ViewableActivationResult _viewableActivationResult;

        public StatementAgentInstanceFactoryCreateNwResult(
            Viewable finalView,
            AgentInstanceMgmtCallback stopCallback,
            AgentInstanceContext agentInstanceContext,
            Viewable eventStreamParentViewable,
            Viewable topView,
            NamedWindowInstance namedWindowInstance,
            ViewableActivationResult viewableActivationResult)
            : base(
                finalView,
                stopCallback,
                agentInstanceContext,
                null,
                EmptyDictionary<int, SubSelectFactoryResult>.Instance,
                null,
                null,
                null,
                EmptyDictionary<int, ExprTableEvalStrategy>.Instance,
                null,
                null)
        {
            _eventStreamParentViewable = eventStreamParentViewable;
            _topView = topView;
            _namedWindowInstance = namedWindowInstance;
            _viewableActivationResult = viewableActivationResult;
        }

        public Viewable EventStreamParentViewable => _eventStreamParentViewable;

        public Viewable TopView => _topView;

        public NamedWindowInstance NamedWindowInstance => _namedWindowInstance;

        public ViewableActivationResult ViewableActivationResult => _viewableActivationResult;
    }
} // end of namespace