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

namespace com.espertech.esper.common.@internal.view.exttimedbatch
{
    public class ExternallyTimedBatchViewFactory : DataWindowViewFactory,
        DataWindowViewWithPrevious
    {
        protected internal EventType eventType;
        protected internal long? optionalReferencePoint;
        protected internal TimePeriodCompute timePeriodCompute;
        protected internal ExprEvaluator timestampEval;

        public long? OptionalReferencePoint {
            get => optionalReferencePoint;
            set => optionalReferencePoint = value;
        }

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
            var viewUpdatedCollection =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprRelativeAccess(agentInstanceViewFactoryContext);
            return new ExternallyTimedBatchView(
                this,
                viewUpdatedCollection,
                agentInstanceViewFactoryContext,
                timePeriodProvide);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public string ViewName => ViewEnum.EXT_TIMED_BATCH.GetViewName();

        PreviousGetterStrategy DataWindowViewWithPrevious.MakePreviousGetter()
        {
            return MakePreviousGetter();
        }

        public RelativeAccessByEventNIndexGetterImpl MakePreviousGetter()
        {
            return new RelativeAccessByEventNIndexGetterImpl();
        }
    }
} // end of namespace