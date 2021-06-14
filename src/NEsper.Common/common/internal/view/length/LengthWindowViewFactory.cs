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
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.view.length
{
    public class LengthWindowViewFactory : ViewFactory,
        DataWindowViewWithPrevious
    {
        protected internal EventType eventType;

        protected internal ExprEvaluator size;

        public ExprEvaluator SizeEvaluator {
            set => size = value;
        }

        public PreviousGetterStrategy MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var sizeValue = ViewFactoryUtil.EvaluateSizeParam(
                ViewName,
                size,
                agentInstanceViewFactoryContext.AgentInstanceContext);
            var randomAccess =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprRandomAccess(agentInstanceViewFactoryContext);
            if (agentInstanceViewFactoryContext.IsRemoveStream) {
                return new LengthWindowViewRStream(agentInstanceViewFactoryContext, this, sizeValue);
            }

            return new LengthWindowView(agentInstanceViewFactoryContext, this, sizeValue, randomAccess);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public string ViewName => ViewEnum.LENGTH_WINDOW.GetViewName();
    }
} // end of namespace