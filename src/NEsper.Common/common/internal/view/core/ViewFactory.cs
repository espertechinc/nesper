///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;

namespace com.espertech.esper.common.@internal.view.core
{
    public interface ViewFactory
    {
        public static ViewFactory[] EMPTY_ARRAY { get; } = Array.Empty<ViewFactory>();
        public static ViewFactory[][] SINGLE_ELEMENT_ARRAY = new ViewFactory[][] {EMPTY_ARRAY};

        EventType EventType { get; set; }

        string ViewName { get; }

        void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services);

        View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext);
    }
} // end of namespace