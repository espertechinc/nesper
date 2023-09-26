///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;


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
        private readonly StateMgmtSetting changeSetStateMgmtSettings;
        private readonly StateMgmtSetting outputFirstStateMgmtSettings;

        public OutputProcessViewConditionFactory(OutputProcessViewConditionSpec spec) : base(
            spec.PostProcessFactory,
            spec.IsDistinct,
            spec.DistinctKeyGetter,
            spec.AfterTimePeriod,
            spec.AfterConditionNumberOfEvents)
        {
            outputConditionFactory = spec.OutputConditionFactory;
            streamCount = spec.StreamCount;
            conditionType = spec.ConditionType;
            terminable = spec.IsTerminable;
            hasAfter = spec.HasAfter;
            isUnaggregatedUngrouped = spec.IsUnaggregatedUngrouped;
            selectClauseStreamSelectorEnum = spec.SelectClauseStreamSelector;
            eventTypes = spec.EventTypes;
            changeSetStateMgmtSettings = spec.ChangeSetStateMgmtSettings;
            outputFirstStateMgmtSettings = spec.OutputFirstStateMgmtSettings;
        }

        public override OutputProcessView MakeView(
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext)
        {
            // determine after-stuff
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

            if (conditionType == ResultSetProcessorOutputConditionType.SNAPSHOT) {
                if (postProcessFactory == null) {
                    return new OutputProcessViewConditionSnapshot(
                        resultSetProcessor,
                        afterConditionTime,
                        AfterConditionNumberOfEvents,
                        isAfterConditionSatisfied,
                        this,
                        agentInstanceContext);
                }

                var postProcess = postProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionSnapshotPostProcess(
                    resultSetProcessor,
                    afterConditionTime,
                    AfterConditionNumberOfEvents,
                    isAfterConditionSatisfied,
                    this,
                    agentInstanceContext,
                    postProcess);
            }
            else if (conditionType == ResultSetProcessorOutputConditionType.POLICY_FIRST) {
                if (postProcessFactory == null) {
                    return new OutputProcessViewConditionFirst(
                        resultSetProcessor,
                        afterConditionTime,
                        AfterConditionNumberOfEvents,
                        isAfterConditionSatisfied,
                        this,
                        agentInstanceContext,
                        outputFirstStateMgmtSettings);
                }

                var postProcess = postProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionFirstPostProcess(
                    resultSetProcessor,
                    afterConditionTime,
                    AfterConditionNumberOfEvents,
                    isAfterConditionSatisfied,
                    this,
                    agentInstanceContext,
                    postProcess,
                    outputFirstStateMgmtSettings);
            }
            else if (conditionType == ResultSetProcessorOutputConditionType.POLICY_LASTALL_UNORDERED) {
                if (postProcessFactory == null) {
                    return new OutputProcessViewConditionLastAllUnord(
                        resultSetProcessor,
                        afterConditionTime,
                        AfterConditionNumberOfEvents,
                        isAfterConditionSatisfied,
                        this,
                        agentInstanceContext);
                }

                var postProcess = postProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionLastAllUnordPostProcessAll(
                    resultSetProcessor,
                    afterConditionTime,
                    AfterConditionNumberOfEvents,
                    isAfterConditionSatisfied,
                    this,
                    agentInstanceContext,
                    postProcess);
            }
            else {
                if (postProcessFactory == null) {
                    return new OutputProcessViewConditionDefault(
                        resultSetProcessor,
                        afterConditionTime,
                        AfterConditionNumberOfEvents,
                        isAfterConditionSatisfied,
                        this,
                        agentInstanceContext,
                        eventTypes,
                        changeSetStateMgmtSettings);
                }

                var postProcess = postProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionDefaultPostProcess(
                    resultSetProcessor,
                    afterConditionTime,
                    AfterConditionNumberOfEvents,
                    isAfterConditionSatisfied,
                    this,
                    agentInstanceContext,
                    postProcess,
                    StreamCount > 1,
                    eventTypes,
                    changeSetStateMgmtSettings);
            }
        }

        public OutputConditionFactory OutputConditionFactory => outputConditionFactory;

        public int StreamCount => streamCount;

        public bool IsTerminable => terminable;

        public bool HasAfter => hasAfter;

        public bool IsUnaggregatedUngrouped => isUnaggregatedUngrouped;

        public SelectClauseStreamSelectorEnum SelectClauseStreamSelectorEnum => selectClauseStreamSelectorEnum;
    }
} // end of namespace