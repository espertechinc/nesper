///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Output process view that does not enforce any output policies and may simply
    /// hand over events to child views, but works with distinct and after-output policies
    /// </summary>
    public class OutputProcessViewDirectDistinctOrAfterFactory : OutputProcessViewDirectFactory
    {
        protected readonly ExprTimePeriod AfterTimePeriod;
        protected readonly int? AfterConditionNumberOfEvents;
        private readonly bool _isDistinct;
        private readonly EventBeanReader _eventBeanReader;

        public OutputProcessViewDirectDistinctOrAfterFactory(
            StatementContext statementContext,
            OutputStrategyPostProcessFactory postProcessFactory,
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory,
            bool distinct,
            ExprTimePeriod afterTimePeriod,
            int? afterConditionNumberOfEvents,
            EventType resultEventType)
            : base(statementContext, postProcessFactory, resultSetProcessorHelperFactory)
        {
            _isDistinct = distinct;
            AfterTimePeriod = afterTimePeriod;
            AfterConditionNumberOfEvents = afterConditionNumberOfEvents;

            if (_isDistinct)
            {
                if (resultEventType is EventTypeSPI)
                {
                    EventTypeSPI eventTypeSPI = (EventTypeSPI) resultEventType;
                    _eventBeanReader = eventTypeSPI.Reader;
                }
                if (_eventBeanReader == null)
                {
                    _eventBeanReader = new EventBeanReaderDefaultImpl(resultEventType);
                }
            }
        }

        public override OutputProcessViewBase MakeView(
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext)
        {
            bool isAfterConditionSatisfied = true;
            long? afterConditionTime = null;
            if (AfterConditionNumberOfEvents != null)
            {
                isAfterConditionSatisfied = false;
            }
            else if (AfterTimePeriod != null)
            {
                isAfterConditionSatisfied = false;
                long delta = AfterTimePeriod.NonconstEvaluator().DeltaUseEngineTime(null, agentInstanceContext);
                afterConditionTime = agentInstanceContext.StatementContext.TimeProvider.Time + delta;
            }

            if (base.PostProcessFactory == null)
            {
                return new OutputProcessViewDirectDistinctOrAfter(
                    ResultSetProcessorHelperFactory, agentInstanceContext, resultSetProcessor, afterConditionTime,
                    AfterConditionNumberOfEvents, isAfterConditionSatisfied, this);
            }
            OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
            return new OutputProcessViewDirectDistinctOrAfterPostProcess(
                ResultSetProcessorHelperFactory, agentInstanceContext, resultSetProcessor, afterConditionTime,
                AfterConditionNumberOfEvents, isAfterConditionSatisfied, this, postProcess);
        }

        public bool IsDistinct => _isDistinct;

        public EventBeanReader EventBeanReader => _eventBeanReader;
    }
} // end of namespace
