///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.lastevent
{
    /// <summary>
    ///     Factory for <seealso cref="LastEventView" /> instances.
    /// </summary>
    public class LastEventViewFactory : DataWindowViewFactory
    {
        protected internal EventType eventType;

        public void Init(ViewFactoryContext viewFactoryContext, EPStatementInitServices services)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new LastEventView(this, agentInstanceViewFactoryContext.AgentInstanceContext);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public string ViewName => ViewEnum.LAST_EVENT.Name;
    }
} // end of namespace