///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    /// Output process view that does not enforce any output policies and may simply
    /// hand over events to child views, but works with distinct and after-output policies
    /// </summary>
    public class OutputProcessViewDirectDistinctOrAfterFactory : OutputProcessViewDirectFactory
    {
        private readonly bool isDistinct;
        internal readonly TimePeriodCompute afterTimePeriod;
        internal readonly int? afterConditionNumberOfEvents;

        private EventBeanReader eventBeanReader;

        public OutputProcessViewDirectDistinctOrAfterFactory(
            OutputStrategyPostProcessFactory postProcessFactory,
            bool distinct,
            TimePeriodCompute afterTimePeriod,
            int? afterConditionNumberOfEvents,
            EventType resultEventType)
            : base(postProcessFactory)
        {
            isDistinct = distinct;
            this.afterTimePeriod = afterTimePeriod;
            this.afterConditionNumberOfEvents = afterConditionNumberOfEvents;

            if (isDistinct) {
                if (resultEventType is EventTypeSPI) {
                    EventTypeSPI eventTypeSPI = (EventTypeSPI) resultEventType;
                    eventBeanReader = eventTypeSPI.Reader;
                }

                if (eventBeanReader == null) {
                    eventBeanReader = new EventBeanReaderDefaultImpl(resultEventType);
                }
            }
        }

        public override OutputProcessView MakeView(
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext)
        {
            bool isAfterConditionSatisfied = true;
            long? afterConditionTime = null;
            if (afterConditionNumberOfEvents != null) {
                isAfterConditionSatisfied = false;
            }
            else if (afterTimePeriod != null) {
                isAfterConditionSatisfied = false;
                long time = agentInstanceContext.TimeProvider.Time;
                long delta = afterTimePeriod.DeltaAdd(time, null, true, agentInstanceContext);
                afterConditionTime = time + delta;
            }

            if (base.postProcessFactory == null) {
                return new OutputProcessViewDirectDistinctOrAfter(
                    agentInstanceContext, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this);
            }

            OutputStrategyPostProcess postProcess = postProcessFactory.Make(agentInstanceContext);
            return new OutputProcessViewDirectDistinctOrAfterPostProcess(
                agentInstanceContext, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this,
                postProcess);
        }

        public bool IsDistinct {
            get => isDistinct;
        }

        public EventBeanReader EventBeanReader {
            get => eventBeanReader;
        }
    }
} // end of namespace