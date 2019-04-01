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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view;

namespace com.espertech.esper.supportregression.client
{
    public class MyFlushedSimpleViewFactory : ViewFactorySupport
    {
        private EventType _eventType;

        public override void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParameters)
        {
        }

        public override void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            _eventType = parentEventType;
        }

        public override View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new MyFlushedSimpleView(agentInstanceViewFactoryContext);
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override string ViewName
        {
            get { return GetType().Name; }
        }
    }
}
