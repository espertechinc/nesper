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
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.view.exttimedwin
{
    /// <summary>
    ///     Factory for <seealso cref="ExternallyTimedWindowView" />.
    /// </summary>
    public class ExternallyTimedWindowViewFactory : DataWindowViewFactory,
        DataWindowViewWithPrevious
    {
        protected internal EventType eventType;
        protected internal TimePeriodCompute timePeriodCompute;
        protected internal ExprEvaluator timestampEval;

        public ExprEvaluator TimestampEval {
            get => timestampEval;
            set => timestampEval = value;
        }

        public TimePeriodCompute TimePeriodCompute {
            set => timePeriodCompute = value;
        }

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var timePeriodProvide =
                timePeriodCompute.GetNonVariableProvide(agentInstanceViewFactoryContext.AgentInstanceContext);
            var randomAccess =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprRandomAccess(agentInstanceViewFactoryContext);
            return new ExternallyTimedWindowView(
                this,
                randomAccess,
                agentInstanceViewFactoryContext,
                timePeriodProvide);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public string ViewName => ViewEnum.EXT_TIMED_WINDOW.GetViewName();

        PreviousGetterStrategy DataWindowViewWithPrevious.MakePreviousGetter()
        {
            return MakePreviousGetter();
        }

        public RandomAccessByIndexGetter MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }
    }
} // end of namespace