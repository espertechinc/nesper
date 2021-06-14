///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    public class OutputProcessViewConditionForge : OutputProcessViewFactoryForge
    {
        private readonly int? _afterNumberOfEvents;
        private readonly ExprTimePeriod _afterTimePeriodExpr;
        private readonly ResultSetProcessorOutputConditionType _conditionType;
        private readonly EventType[] _eventTypes;
        private readonly bool _hasAfter;
        private readonly bool _isDistinct;
        private readonly MultiKeyClassRef _distinctMultiKey;
        private readonly OutputConditionFactoryForge _outputConditionFactoryForge;
        private readonly OutputStrategyPostProcessForge _outputStrategyPostProcessForge;
        private readonly EventType _resultEventType;
        private readonly SelectClauseStreamSelectorEnum _selectClauseStreamSelector;
        private readonly int _streamCount;
        private readonly bool _terminable;
        private readonly bool _unaggregatedUngrouped;

        public OutputProcessViewConditionForge(
            OutputStrategyPostProcessForge outputStrategyPostProcessForge,
            bool isDistinct,
            MultiKeyClassRef distinctMultiKey,
            ExprTimePeriod afterTimePeriodExpr,
            int? afterNumberOfEvents,
            OutputConditionFactoryForge outputConditionFactoryForge,
            int streamCount,
            ResultSetProcessorOutputConditionType conditionType,
            bool terminable,
            bool hasAfter,
            bool unaggregatedUngrouped,
            SelectClauseStreamSelectorEnum selectClauseStreamSelector,
            EventType[] eventTypes,
            EventType resultEventType)
        {
            _outputStrategyPostProcessForge = outputStrategyPostProcessForge;
            _isDistinct = isDistinct;
            _distinctMultiKey = distinctMultiKey;
            _afterTimePeriodExpr = afterTimePeriodExpr;
            _afterNumberOfEvents = afterNumberOfEvents;
            _outputConditionFactoryForge = outputConditionFactoryForge;
            _streamCount = streamCount;
            _conditionType = conditionType;
            _terminable = terminable;
            _hasAfter = hasAfter;
            _unaggregatedUngrouped = unaggregatedUngrouped;
            _selectClauseStreamSelector = selectClauseStreamSelector;
            _eventTypes = eventTypes;
            _resultEventType = resultEventType;
        }

        public bool IsCodeGenerated => false;

        public void ProvideCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var spec = Ref("spec");
            method.Block
                .DeclareVar<OutputProcessViewConditionSpec>(
                    spec.Ref,
                    NewInstance(typeof(OutputProcessViewConditionSpec)))
                .SetProperty(
                    spec,
                    "ConditionType",
                    EnumValue(typeof(ResultSetProcessorOutputConditionType), _conditionType.GetName()))
                .SetProperty(
                    spec,
                    "OutputConditionFactory",
                    _outputConditionFactoryForge.Make(method, symbols, classScope))
                .SetProperty(
                    spec,
                    "StreamCount",
                    Constant(_streamCount))
                .SetProperty(
                    spec,
                    "IsTerminable",
                    Constant(_terminable))
                .SetProperty(
                    spec,
                    "SelectClauseStreamSelector",
                    EnumValue(typeof(SelectClauseStreamSelectorEnum), _selectClauseStreamSelector.GetName()))
                .SetProperty(
                    spec,
                    "PostProcessFactory",
                    _outputStrategyPostProcessForge == null
                        ? ConstantNull()
                        : _outputStrategyPostProcessForge.Make(method, symbols, classScope))
                .SetProperty(
                    spec,
                    "HasAfter",
                    Constant(_hasAfter))
                .SetProperty(
                    spec,
                    "IsDistinct",
                    Constant(_isDistinct))
                .SetProperty(
                    spec,
                    "DistinctKeyGetter",
                    MultiKeyCodegen.CodegenGetterEventDistinct(
                        _isDistinct, _resultEventType, _distinctMultiKey, method, classScope))
                .SetProperty(
                    spec,
                    "ResultEventType",
                    EventTypeUtility.ResolveTypeCodegen(_resultEventType, symbols.GetAddInitSvc(method)))
                .SetProperty(
                    spec,
                    "AfterTimePeriod",
                    _afterTimePeriodExpr == null
                        ? ConstantNull()
                        : _afterTimePeriodExpr.TimePeriodComputeForge.MakeEvaluator(method, classScope))
                .SetProperty(
                    spec,
                    "AfterConditionNumberOfEvents",
                    Constant(_afterNumberOfEvents))
                .SetProperty(
                    spec,
                    "IsUnaggregatedUngrouped",
                    Constant(_unaggregatedUngrouped))
                .SetProperty(
                    spec,
                    "EventTypes",
                    EventTypeUtility.ResolveTypeArrayCodegen(_eventTypes, EPStatementInitServicesConstants.REF))
                .MethodReturn(NewInstance<OutputProcessViewConditionFactory>(spec));
        }

        public void UpdateCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
        }

        public void ProcessCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
        }

        public void EnumeratorCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
        }

        public void CollectSchedules(IList<ScheduleHandleCallbackProvider> scheduleHandleCallbackProviders)
        {
            _outputConditionFactoryForge?.CollectSchedules(scheduleHandleCallbackProviders);
        }
    }
} // end of namespace