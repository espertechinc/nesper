///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    /// A view that handles the "output snapshot" keyword in output rate stabilizing.
    /// </summary>
    public class OutputProcessViewConditionFactory : OutputProcessViewDirectDistinctOrAfterFactory
    {
        private readonly OutputConditionFactory outputConditionFactory;
        private readonly int streamCount;
        private readonly ResultSetProcessorOutputConditionType conditionType;
        private readonly bool terminable;
        private readonly bool hasAfter;
        private readonly bool isUnaggregatedUngrouped;
        private readonly SelectClauseStreamSelectorEnum selectClauseStreamSelectorEnum;
        private readonly EventType[] eventTypes;

        public OutputProcessViewConditionFactory(OutputProcessViewConditionSpec spec)
            : base(spec.PostProcessFactory, spec.IsDistinct, spec.AfterTimePeriod, spec.AfterConditionNumberOfEvents, spec.ResultEventType)
        {
            this.outputConditionFactory = spec.OutputConditionFactory;
            this.streamCount = spec.StreamCount;
            this.conditionType = spec.ConditionType;
            this.terminable = spec.IsTerminable;
            this.hasAfter = spec.IsAfter;
            this.isUnaggregatedUngrouped = spec.IsUnaggregatedUngrouped;
            this.selectClauseStreamSelectorEnum = spec.SelectClauseStreamSelector;
            this.eventTypes = spec.EventTypes;
        }

        public override OutputProcessView MakeView(
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext)
        {
            // determine after-stuff
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

            if (conditionType == ResultSetProcessorOutputConditionType.SNAPSHOT) {
                if (base.postProcessFactory == null) {
                    return new OutputProcessViewConditionSnapshot(
                        resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext);
                }

                OutputStrategyPostProcess postProcess = postProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionSnapshotPostProcess(
                    resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext,
                    postProcess);
            }
            else if (conditionType == ResultSetProcessorOutputConditionType.POLICY_FIRST) {
                if (base.postProcessFactory == null) {
                    return new OutputProcessViewConditionFirst(
                        resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext);
                }

                OutputStrategyPostProcess postProcess = postProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionFirstPostProcess(
                    resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext,
                    postProcess);
            }
            else if (conditionType == ResultSetProcessorOutputConditionType.POLICY_LASTALL_UNORDERED) {
                if (base.postProcessFactory == null) {
                    return new OutputProcessViewConditionLastAllUnord(
                        resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext);
                }

                OutputStrategyPostProcess postProcess = postProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionLastAllUnordPostProcessAll(
                    resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext,
                    postProcess);
            }
            else {
                if (base.postProcessFactory == null) {
                    return new OutputProcessViewConditionDefault(
                        resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext,
                        streamCount > 1, eventTypes);
                }

                OutputStrategyPostProcess postProcess = postProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionDefaultPostProcess(
                    resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext,
                    postProcess, streamCount > 1, eventTypes);
            }
        }

        public OutputConditionFactory OutputConditionFactory {
            get => outputConditionFactory;
        }

        public int StreamCount {
            get => streamCount;
        }

        public bool IsTerminable {
            get => terminable;
        }

        public bool IsAfter {
            get => hasAfter;
        }

        public bool IsUnaggregatedUngrouped {
            get => isUnaggregatedUngrouped;
        }

        public SelectClauseStreamSelectorEnum SelectClauseStreamSelectorEnum {
            get => selectClauseStreamSelectorEnum;
        }
    }
} // end of namespace