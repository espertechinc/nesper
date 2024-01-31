///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.regressionlib.support.extend.view
{
    public class MyTrendSpotterViewFactory : ViewFactory
    {
        public ExprEvaluator Parameter { get; set; }

        public EventType EventType { get; set; }

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new MyTrendSpotterView(this, agentInstanceViewFactoryContext.AgentInstanceContext);
        }

        public string ViewName => "Trend-Spotter";
    }
} // end of namespace