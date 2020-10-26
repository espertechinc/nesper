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

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    ///     Output process view that does not enforce any output policies and may simply
    ///     hand over events to child views, but works with distinct and after-output policies
    /// </summary>
    public class OutputProcessViewDirectDistinctOrAfterFactory : OutputProcessViewDirectFactory
    {
        public OutputProcessViewDirectDistinctOrAfterFactory(
            OutputStrategyPostProcessFactory postProcessFactory,
            bool distinct,
            EventPropertyValueGetter distinctKeyGetter,
            TimePeriodCompute afterTimePeriod,
            int? afterConditionNumberOfEvents,
            EventType resultEventType)
            : base(postProcessFactory)
        {
            IsDistinct = distinct;
            DistinctKeyGetter = distinctKeyGetter;
            this.AfterTimePeriod = afterTimePeriod;
            this.AfterConditionNumberOfEvents = afterConditionNumberOfEvents;
        }
        public EventPropertyValueGetter DistinctKeyGetter { get; set; }
        
        public bool IsDistinct { get; }

        public int? AfterConditionNumberOfEvents { get; }

        public TimePeriodCompute AfterTimePeriod { get; }

        public override OutputProcessView MakeView(
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext)
        {
            var isAfterConditionSatisfied = true;
            long? afterConditionTime = null;
            if (AfterConditionNumberOfEvents != null) {
                isAfterConditionSatisfied = false;
            }
            else if (AfterTimePeriod != null) {
                isAfterConditionSatisfied = false;
                var time = agentInstanceContext.TimeProvider.Time;
                var delta = AfterTimePeriod.DeltaAdd(time, null, true, agentInstanceContext);
                afterConditionTime = time + delta;
            }

            if (postProcessFactory == null) {
                return new OutputProcessViewDirectDistinctOrAfter(
                    agentInstanceContext,
                    resultSetProcessor,
                    afterConditionTime,
                    AfterConditionNumberOfEvents,
                    isAfterConditionSatisfied,
                    this);
            }

            var postProcess = postProcessFactory.Make(agentInstanceContext);
            return new OutputProcessViewDirectDistinctOrAfterPostProcess(
                agentInstanceContext,
                resultSetProcessor,
                afterConditionTime,
                AfterConditionNumberOfEvents,
                isAfterConditionSatisfied,
                this,
                postProcess);
        }
    }
} // end of namespace