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
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    ///     A view that handles the "output snapshot" keyword in output rate stabilizing.
    /// </summary>
    public class OutputProcessViewConditionFactory : OutputProcessViewDirectDistinctOrAfterFactory
    {
        public enum ConditionType
        {
            SNAPSHOT,
            POLICY_FIRST,
            POLICY_LASTALL_UNORDERED,
            POLICY_NONFIRST
        }

        private readonly ConditionType _conditionType;
        private readonly bool _hasAfter;
        private readonly bool _isUnaggregatedUngrouped;
        private readonly OutputConditionFactory _outputConditionFactory;
        private readonly OutputLimitLimitType _outputLimitLimitType;
        private readonly ResultSetProcessorHelperFactory _resultSetProcessorHelperFactory;
        private readonly SelectClauseStreamSelectorEnum _selectClauseStreamSelectorEnum;
        private readonly int _streamCount;
        private readonly bool _terminable;

        public OutputProcessViewConditionFactory(
            StatementContext statementContext,
            OutputStrategyPostProcessFactory postProcessFactory,
            bool distinct,
            ExprTimePeriod afterTimePeriod,
            int? afterConditionNumberOfEvents,
            EventType resultEventType,
            OutputConditionFactory outputConditionFactory,
            int streamCount,
            ConditionType conditionType,
            OutputLimitLimitType outputLimitLimitType,
            bool terminable,
            bool hasAfter,
            bool isUnaggregatedUngrouped,
            SelectClauseStreamSelectorEnum selectClauseStreamSelectorEnum,
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory)
            : base(
                statementContext, postProcessFactory, resultSetProcessorHelperFactory, distinct, afterTimePeriod,
                afterConditionNumberOfEvents, resultEventType)
        {
            _outputConditionFactory = outputConditionFactory;
            _streamCount = streamCount;
            _conditionType = conditionType;
            _outputLimitLimitType = outputLimitLimitType;
            _terminable = terminable;
            _hasAfter = hasAfter;
            _isUnaggregatedUngrouped = isUnaggregatedUngrouped;
            _selectClauseStreamSelectorEnum = selectClauseStreamSelectorEnum;
            _resultSetProcessorHelperFactory = resultSetProcessorHelperFactory;
        }

        public OutputConditionFactory OutputConditionFactory => _outputConditionFactory;

        public int StreamCount => _streamCount;

        public OutputLimitLimitType OutputLimitLimitType => _outputLimitLimitType;

        public bool IsTerminable => _terminable;

        public bool HasAfter => _hasAfter;

        public bool IsUnaggregatedUngrouped => _isUnaggregatedUngrouped;

        public SelectClauseStreamSelectorEnum SelectClauseStreamSelectorEnum => _selectClauseStreamSelectorEnum;

        public override OutputProcessViewBase MakeView(
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext)
        {
            // determine after-stuff
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

            if (_conditionType == ConditionType.SNAPSHOT)
            {
                if (base.PostProcessFactory == null)
                {
                    return new OutputProcessViewConditionSnapshot(
                        _resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime,
                        AfterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext);
                }
                OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionSnapshotPostProcess(
                    _resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime,
                    AfterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext, postProcess);
            }
            else if (_conditionType == ConditionType.POLICY_FIRST)
            {
                if (base.PostProcessFactory == null)
                {
                    return new OutputProcessViewConditionFirst(
                        _resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime,
                        AfterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext);
                }
                OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionFirstPostProcess(
                    _resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime,
                    AfterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext, postProcess);
            }
            else if (_conditionType == ConditionType.POLICY_LASTALL_UNORDERED)
            {
                if (base.PostProcessFactory == null)
                {
                    return new OutputProcessViewConditionLastAllUnord(
                        _resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime,
                        AfterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext);
                }
                OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionLastAllUnordPostProcessAll(
                    _resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime,
                    AfterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext, postProcess);
            }
            else
            {
                if (base.PostProcessFactory == null)
                {
                    return new OutputProcessViewConditionDefault(
                        _resultSetProcessorHelperFactory, resultSetProcessor, afterConditionTime,
                        AfterConditionNumberOfEvents, isAfterConditionSatisfied, this, agentInstanceContext,
                        _streamCount > 1);
                }
                OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
                return new OutputProcessViewConditionDefaultPostProcess(
                    resultSetProcessor, afterConditionTime, AfterConditionNumberOfEvents, isAfterConditionSatisfied,
                    this, agentInstanceContext, postProcess, _streamCount > 1, _resultSetProcessorHelperFactory);
            }
        }
    }
} // end of namespace