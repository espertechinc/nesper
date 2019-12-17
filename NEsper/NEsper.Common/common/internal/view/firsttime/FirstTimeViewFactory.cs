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
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.firsttime
{
    /// <summary>
    /// Factory for <seealso cref="FirstTimeView" />.
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
            TimePeriodProvide timePeriodProvide =
                timePeriodCompute.GetNonVariableProvide(agentInstanceViewFactoryContext.AgentInstanceContext);
            return new FirstTimeView(this, agentInstanceViewFactoryContext, timePeriodProvide);
        }

        public EventType EventType {
            get => eventType;
            set { this.eventType = value; }
        }

        private string GetViewParamMessage()
        {
            return ViewName + " view requires a single numeric or time period parameter";
        }

        public TimePeriodCompute TimePeriodCompute {
            set { this.timePeriodCompute = value; }
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;
            set { this.scheduleCallbackId = value; }
        }

        public string ViewName {
            get => ViewEnum.FIRST_TIME_WINDOW.GetViewName();
        }
    }
} // end of namespace