///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.view.keepall
{
    /// <summary>
    ///     Factory for keep-all-views.
    /// </summary>
    public class KeepAllViewFactory : DataWindowViewFactory,
        DataWindowViewWithPrevious
    {
        protected internal EventType eventType;

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var randomAccess =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprRandomAccess(agentInstanceViewFactoryContext);
            return new KeepAllView(agentInstanceViewFactoryContext, this, randomAccess);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public string ViewName => ViewEnum.KEEPALL_WINDOW.GetViewName();

        public PreviousGetterStrategy MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }
    }
} // end of namespace