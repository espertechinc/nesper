///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    ///     Output process view that does not enforce any output policies and may simply
    ///     hand over events to child views, but works with distinct and after-output policies
    /// </summary>
    public class OutputProcessViewDirectDistinctOrAfterFactory : OutputProcessViewDirectFactory
    {
        internal readonly int? afterConditionNumberOfEvents;

        internal readonly TimePeriodCompute afterTimePeriod;

        public OutputProcessViewDirectDistinctOrAfterFactory(
            OutputStrategyPostProcessFactory postProcessFactory,
            bool distinct,
            TimePeriodCompute afterTimePeriod,
            int? afterConditionNumberOfEvents,
            EventType resultEventType)
            : base(postProcessFactory)
        {
            IsDistinct = distinct;
            this.afterTimePeriod = afterTimePeriod;
            this.afterConditionNumberOfEvents = afterConditionNumberOfEvents;

            if (IsDistinct) {
                if (resultEventType is EventTypeSPI) {
                    var eventTypeSPI = (EventTypeSPI) resultEventType;
                    EventBeanReader = eventTypeSPI.Reader;
                }

                if (EventBeanReader == null) {
                    EventBeanReader = new EventBeanReaderDefaultImpl(resultEventType);
                }
            }
        }

        public bool IsDistinct { get; }

        public EventBeanReader EventBeanReader { get; }

        public override OutputProcessView MakeView(
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext)
        {
            var isAfterConditionSatisfied = true;
            long? afterConditionTime = null;
            if (afterConditionNumberOfEvents != null) {
                isAfterConditionSatisfied = false;
            }
            else if (afterTimePeriod != null) {
                isAfterConditionSatisfied = false;
                var time = agentInstanceContext.TimeProvider.Time;
                var delta = afterTimePeriod.DeltaAdd(time, null, true, agentInstanceContext);
                afterConditionTime = time + delta;
            }

            if (postProcessFactory == null) {
                return new OutputProcessViewDirectDistinctOrAfter(
                    agentInstanceContext,
                    resultSetProcessor,
                    afterConditionTime,
                    afterConditionNumberOfEvents,
                    isAfterConditionSatisfied,
                    this);
            }

            var postProcess = postProcessFactory.Make(agentInstanceContext);
            return new OutputProcessViewDirectDistinctOrAfterPostProcess(
                agentInstanceContext,
                resultSetProcessor,
                afterConditionTime,
                afterConditionNumberOfEvents,
                isAfterConditionSatisfied,
                this,
                postProcess);
        }
    }
} // end of namespace