///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.view.time_accum
{
    public class TimeAccumViewFactory
        : DataWindowViewFactory,
            DataWindowViewWithPrevious
    {
        internal EventType eventType;
        internal int scheduleCallbackId;
        internal TimePeriodCompute timePeriodCompute;

        public TimePeriodCompute TimePeriodCompute {
            set => timePeriodCompute = value;
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;
            set => scheduleCallbackId = value;
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
            var randomAccess = agentInstanceViewFactoryContext
                .StatementContext
                .ViewServicePreviousFactory
                .GetOptPreviousExprRandomAccess(agentInstanceViewFactoryContext);
            if (agentInstanceViewFactoryContext.IsRemoveStream) {
                return new TimeAccumViewRStream(this, agentInstanceViewFactoryContext, timePeriodProvide);
            }

            return new TimeAccumView(this, agentInstanceViewFactoryContext, randomAccess, timePeriodProvide);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public string ViewName => ViewEnum.TIME_ACCUM.GetViewName();

        public PreviousGetterStrategy MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }
    }
} // end of namespace