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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    /// Factory for <seealso cref="SizeView" /> instances.
    /// </summary>
    public class SizeViewFactory : ViewFactory
    {
        protected StatViewAdditionalPropsEval additionalProps;
        protected EventType eventType;

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            if (eventType == null) {
                throw new IllegalStateException("Event type not provided");
            }
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new SizeView(this, agentInstanceViewFactoryContext.AgentInstanceContext, eventType, additionalProps);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public StatViewAdditionalPropsEval AdditionalProps {
            get => additionalProps;
            set => additionalProps = value;
        }

        public string ViewName => ViewEnum.SIZE.GetViewName();
    }
} // end of namespace