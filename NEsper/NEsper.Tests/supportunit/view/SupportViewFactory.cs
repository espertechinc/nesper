///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view;

namespace com.espertech.esper.supportunit.view
{
    public abstract class SupportViewFactory : ViewFactory
    {
        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParameters)
        {
        }

        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return null;
        }

        public EventType EventType
        {
            get { return null; }
        }

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            return false;
        }

        abstract public string ViewName { get; }
    }
}
