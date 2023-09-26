///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.firsttime
{
    /// <summary>
    /// Factory for <seealso cref = "FirstTimeView"/>.
    /// </summary>
    public class FirstTimeViewFactory : ViewFactory
    {
        protected EventType eventType;
        protected TimePeriodCompute timePeriodCompute;
        protected int scheduleCallbackId;

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var timePeriodProvide =
                timePeriodCompute.GetNonVariableProvide(agentInstanceViewFactoryContext.AgentInstanceContext);
            return new FirstTimeView(this, agentInstanceViewFactoryContext, timePeriodProvide);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public TimePeriodCompute TimePeriodCompute {
            set => timePeriodCompute = value;
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;
            set => scheduleCallbackId = value;
        }

        public string ViewName => ViewEnum.FIRST_TIME_WINDOW.GetViewName();

        public string ViewParamMessage => ViewName + " view requires a single numeric or time period parameter";
    }
} // end of namespace