///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.firstlength
{
    /// <summary>
    /// Factory for <seealso cref="FirstLengthWindowView" />.
    /// </summary>
    public class FirstLengthWindowViewFactory : DataWindowViewFactory
    {
        protected ExprEvaluator size;
        protected EventType eventType;

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var size = ViewFactoryUtil.EvaluateSizeParam(
                ViewName,
                this.size,
                agentInstanceViewFactoryContext.AgentInstanceContext);
            return new FirstLengthWindowView(agentInstanceViewFactoryContext, this, size);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public ExprEvaluator SizeEvaluator {
            set => size = value;
        }

        public string ViewName => ViewEnum.FIRST_LENGTH_WINDOW.GetViewName();
    }
} // end of namespace