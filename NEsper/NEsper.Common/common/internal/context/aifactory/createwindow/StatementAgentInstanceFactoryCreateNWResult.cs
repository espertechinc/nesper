///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createwindow
{
    public class StatementAgentInstanceFactoryCreateNWResult : StatementAgentInstanceFactoryResult
    {

        private readonly Viewable eventStreamParentViewable;
        private readonly Viewable topView;
        private readonly NamedWindowInstance namedWindowInstance;
        private readonly ViewableActivationResult viewableActivationResult;

        public StatementAgentInstanceFactoryCreateNWResult(
            Viewable finalView,
            AgentInstanceStopCallback stopCallback,
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
                new EmptyDictionary<int, SubSelectFactoryResult>(), 
                null,
                null,
                null,
                new EmptyDictionary<int, ExprTableEvalStrategy>(), 
                null)
        {
            this.eventStreamParentViewable = eventStreamParentViewable;
            this.topView = topView;
            this.namedWindowInstance = namedWindowInstance;
            this.viewableActivationResult = viewableActivationResult;
        }

        public Viewable EventStreamParentViewable {
            get => eventStreamParentViewable;
        }

        public Viewable TopView {
            get => topView;
        }

        public NamedWindowInstance NamedWindowInstance {
            get => namedWindowInstance;
        }

        public ViewableActivationResult ViewableActivationResult {
            get => viewableActivationResult;
        }
    }
} // end of namespace