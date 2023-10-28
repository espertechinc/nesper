///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.util;
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
        private readonly OutputStrategyPostProcessForge outputStrategyPostProcessForge;
        private readonly bool isDistinct;
        private readonly MultiKeyClassRef distinctMultiKey;
        private readonly ExprTimePeriod afterTimePeriodExpr;
        private readonly int? afterNumberOfEvents;
        private readonly OutputConditionFactoryForge _outputConditionFactoryForge;
        private readonly int _streamCount;
        private readonly ResultSetProcessorOutputConditionType _conditionType;
        private readonly bool terminable;
        private readonly bool hasAfter;
        private readonly bool unaggregatedUngrouped;
        private readonly SelectClauseStreamSelectorEnum selectClauseStreamSelector;
        private readonly EventType[] eventTypes;
        private readonly EventType resultEventType;
        private readonly StateMgmtSetting changeSetStateMgmtSettings;
        private readonly StateMgmtSetting outputFirstStateMgmtSettings;

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
            EventType resultEventType,
            StateMgmtSetting changeSetStateMgmtSettings,
            StateMgmtSetting outputFirstStateMgmtSettings)
        {
            this.outputStrategyPostProcessForge = outputStrategyPostProcessForge;
            this.isDistinct = isDistinct;
            this.distinctMultiKey = distinctMultiKey;
            this.afterTimePeriodExpr = afterTimePeriodExpr;
            this.afterNumberOfEvents = afterNumberOfEvents;
            this._outputConditionFactoryForge = outputConditionFactoryForge;
            this._streamCount = streamCount;
            this._conditionType = conditionType;
            this.terminable = terminable;
            this.hasAfter = hasAfter;
            this.unaggregatedUngrouped = unaggregatedUngrouped;
            this.selectClauseStreamSelector = selectClauseStreamSelector;
            this.eventTypes = eventTypes;
            this.resultEventType = resultEventType;
            this.changeSetStateMgmtSettings = changeSetStateMgmtSettings;
            this.outputFirstStateMgmtSettings = outputFirstStateMgmtSettings;
        }

        public bool IsDirectAndSimple => false;

        public bool IsCodeGenerated => false;

        public void ProvideCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var spec = Ref("spec");
            method.Block
                .DeclareVarNewInstance(typeof(OutputProcessViewConditionSpec), spec.Ref)
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
                    Constant(terminable))
                .SetProperty(
                    spec,
                    "SelectClauseStreamSelector",
                    EnumValue(typeof(SelectClauseStreamSelectorEnum), selectClauseStreamSelector.GetName()))
                .SetProperty(
                    spec,
                    "PostProcessFactory",
                    outputStrategyPostProcessForge == null
                        ? ConstantNull()
                        : outputStrategyPostProcessForge.Make(method, symbols, classScope))
                .SetProperty(
                    spec,
                    "HasAfter",
                    Constant(hasAfter))
                .SetProperty(
                    spec,
                    "IsDistinct",
                    Constant(isDistinct))
                .SetProperty(
                    spec,
                    "DistinctKeyGetter",
                    MultiKeyCodegen.CodegenGetterEventDistinct(
                        isDistinct,
                        resultEventType,
                        distinctMultiKey,
                        method,
                        classScope))
                .SetProperty(
                    spec,
                    "ResultEventType",
                    EventTypeUtility.ResolveTypeCodegen(resultEventType, symbols.GetAddInitSvc(method)))
                .SetProperty(
                    spec,
                    "AfterTimePeriod",
                    afterTimePeriodExpr == null
                        ? ConstantNull()
                        : afterTimePeriodExpr.TimePeriodComputeForge.MakeEvaluator(method, classScope))
                .SetProperty(
                    spec,
                    "AfterConditionNumberOfEvents",
                    Constant(afterNumberOfEvents))
                .SetProperty(
                    spec,
                    "IsUnaggregatedUngrouped",
                    Constant(unaggregatedUngrouped))
                .SetProperty(
                    spec,
                    "EventTypes",
                    EventTypeUtility.ResolveTypeArrayCodegen(eventTypes, EPStatementInitServicesConstants.REF))
                .SetProperty(
                    spec,
                    "ChangeSetStateMgmtSettings",
                    changeSetStateMgmtSettings.ToExpression())
                .SetProperty(
                    spec,
                    "OutputFirstStateMgmtSettings",
                    outputFirstStateMgmtSettings.ToExpression())
                .MethodReturn(NewInstance(typeof(OutputProcessViewConditionFactory), spec));
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

        public void CollectSchedules(IList<ScheduleHandleTracked> scheduleHandleCallbackProviders)
        {
            if (_outputConditionFactoryForge != null) {
                _outputConditionFactoryForge.CollectSchedules(
                    CallbackAttributionOutputRate.INSTANCE,
                    scheduleHandleCallbackProviders);
            }
        }
    }
} // end of namespace