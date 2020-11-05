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
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.view.timebatch
{
    /// <summary>
    ///     Factory for <seealso cref="TimeBatchView" />.
    /// </summary>
    public class TimeBatchViewFactory : DataWindowViewFactory,
        DataWindowViewWithPrevious
    {
        protected internal EventType eventType;
        protected internal bool isForceUpdate;
        protected internal bool isStartEager;

        /// <summary>
        ///     The reference point, or null if none supplied.
        /// </summary>
        protected internal long? optionalReferencePoint;

        protected internal int scheduleCallbackId;
        protected internal TimePeriodCompute timePeriodCompute;

        public TimePeriodCompute TimePeriodCompute {
            get => timePeriodCompute;
            set => timePeriodCompute = value;
        }

        public long? OptionalReferencePoint {
            get => optionalReferencePoint;
            set => optionalReferencePoint = value;
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;
            set => scheduleCallbackId = value;
        }

        public bool IsForceUpdate {
            get => isForceUpdate;
            set => isForceUpdate = value;
        }

        public bool IsStartEager {
            get => isStartEager;
            set => isStartEager = value;
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
            var viewUpdatedCollection =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprRelativeAccess(agentInstanceViewFactoryContext);
            if (agentInstanceViewFactoryContext.IsRemoveStream) {
                return new TimeBatchViewRStream(this, agentInstanceViewFactoryContext, timePeriodProvide);
            }

            return new TimeBatchView(this, agentInstanceViewFactoryContext, viewUpdatedCollection, timePeriodProvide);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public string ViewName => ViewEnum.TIME_BATCH.GetViewName();

        public PreviousGetterStrategy MakePreviousGetter()
        {
            return new RelativeAccessByEventNIndexGetterImpl();
        }
    }
} // end of namespace