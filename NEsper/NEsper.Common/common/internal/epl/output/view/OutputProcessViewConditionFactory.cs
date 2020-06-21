///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    ///     A view that handles the "output snapshot" keyword in output rate stabilizing.
    /// </summary>
    public class OutputProcessViewConditionFactory : OutputProcessViewDirectDistinctOrAfterFactory
    {
        private readonly ResultSetProcessorOutputConditionType _conditionType;
        private readonly EventType[] _eventTypes;

        public OutputProcessViewConditionFactory(OutputProcessViewConditionSpec spec)
            : base(
                spec.PostProcessFactory,
                spec.IsDistinct,
                spec.DistinctKeyGetter,
                spec.AfterTimePeriod,
                spec.AfterConditionNumberOfEvents,
                spec.ResultEventType)
        {
            OutputConditionFactory = spec.OutputConditionFactory;
            StreamCount = spec.StreamCount;
            _conditionType = spec.ConditionType;
            IsTerminable = spec.IsTerminable;
            IsAfter = spec.HasAfter;
            IsUnaggregatedUngrouped = spec.IsUnaggregatedUngrouped;
            SelectClauseStreamSelectorEnum = spec.SelectClauseStreamSelector;
            _eventTypes = spec.EventTypes;
        }

        public OutputConditionFactory OutputConditionFactory { get; }

        public int StreamCount { get; }

        public bool IsTerminable { get; }

        public bool IsAfter { get; }

        public bool IsUnaggregatedUngrouped { get; }

        public SelectClauseStreamSelectorEnum SelectClauseStreamSelectorEnum { get; }

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

            if (_conditionType == ResultSetProcessorOutputConditionType.SNAPSHOT) {
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

            if (_conditionType == ResultSetProcessorOutputConditionType.POLICY_FIRST) {
                if (postProcessFactory == null) {
                    return new OutputProcessViewConditionFirst(
                        resultSetProcessor,
                        afterConditionTime,
                        AfterConditionNumberOfEvents,
                        isAfterConditionSatisfied,
                        this,
                        agentInstanceContext);
                }

                var postProcess = postProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionFirstPostProcess(
                    resultSetProcessor,
                    afterConditionTime,
                    AfterConditionNumberOfEvents,
                    isAfterConditionSatisfied,
                    this,
                    agentInstanceContext,
                    postProcess);
            }

            if (_conditionType == ResultSetProcessorOutputConditionType.POLICY_LASTALL_UNORDERED) {
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
                        StreamCount > 1,
                        _eventTypes);
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
                    _eventTypes);
            }
        }
    }
} // end of namespace