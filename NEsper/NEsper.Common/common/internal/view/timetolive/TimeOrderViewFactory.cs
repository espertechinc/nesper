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

namespace com.espertech.esper.common.@internal.view.timetolive
{
    /// <summary>
    ///     Factory for views for time-ordering events.
    /// </summary>
    public class TimeOrderViewFactory : DataWindowViewFactory,
        DataWindowViewWithPrevious
    {
        internal EventType eventType;
        internal bool isTimeToLive;
        internal int scheduleCallbackId;
        internal TimePeriodCompute timePeriodCompute;
        internal ExprEvaluator timestampEval;

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public ExprEvaluator TimestampEval {
            get => timestampEval;
            set => timestampEval = value;
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;
            set => scheduleCallbackId = value;
        }

        public TimePeriodCompute TimePeriodCompute {
            set => timePeriodCompute = value;
        }

        public bool TimeToLive {
            set => isTimeToLive = value;
        }

        public string ViewName => isTimeToLive ? ViewEnum.TIMETOLIVE.Name : ViewEnum.TIME_ORDER.Name;

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
        }

        PreviousGetterStrategy DataWindowViewWithPrevious.MakePreviousGetter()
        {
            return MakePreviousGetter();
        }

        public RandomAccessByIndexGetter MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            TimePeriodProvide timePeriodProvide =
                timePeriodCompute.GetNonVariableProvide(agentInstanceViewFactoryContext.AgentInstanceContext);
            IStreamSortRankRandomAccess sortedRandomAccess =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprSortedRankedAccess(agentInstanceViewFactoryContext);
            return new TimeOrderView(agentInstanceViewFactoryContext, this, sortedRandomAccess, timePeriodProvide);
        }
    }
} // end of namespace