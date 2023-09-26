///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.view.timelengthbatch
{
    /// <summary>
    ///     Factory for <seealso cref="TimeLengthBatchView" />.
    /// </summary>
    public class TimeLengthBatchViewFactory : DataWindowViewFactory,
        DataWindowViewWithPrevious
    {
        internal EventType eventType;
        internal bool isForceUpdate;
        internal bool isStartEager;
        internal int scheduleCallbackId;

        /// <summary>
        ///     Number of events to collect before batch fires.
        /// </summary>
        internal ExprEvaluator sizeEvaluator;

        internal TimePeriodCompute timePeriodCompute;

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public TimePeriodCompute TimePeriodCompute {
            set => timePeriodCompute = value;
        }

        public bool ForceUpdate {
            set => isForceUpdate = value;
        }

        public bool StartEager {
            set => isStartEager = value;
        }

        public ExprEvaluator SizeEvaluator {
            get => sizeEvaluator;
            set => sizeEvaluator = value;
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;
            set => scheduleCallbackId = value;
        }

        public bool IsForceUpdate => isForceUpdate;

        public bool IsStartEager => isStartEager;

        public string ViewName => ViewEnum.TIME_LENGTH_BATCH.GetViewName();

        public PreviousGetterStrategy MakePreviousGetter()
        {
            return new RelativeAccessByEventNIndexGetterImpl();
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
            var sizeValue = ViewFactoryUtil.EvaluateSizeParam(
                ViewName,
                sizeEvaluator,
                agentInstanceViewFactoryContext.AgentInstanceContext);
            var viewUpdatedCollection =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprRelativeAccess(agentInstanceViewFactoryContext);
            return new TimeLengthBatchView(
                this,
                sizeValue,
                agentInstanceViewFactoryContext,
                viewUpdatedCollection,
                timePeriodProvide);
        }
    }
} // end of namespace