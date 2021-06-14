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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.schedule;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    ///     Output process view that does not enforce any output policies and may simply
    ///     hand over events to child views, but works with distinct and after-output policies
    /// </summary>
    public class OutputProcessViewDirectDistinctOrAfterFactoryForge : OutputProcessViewFactoryForge
    {
        private readonly bool _isDistinct;
        private readonly OutputStrategyPostProcessForge _outputStrategyPostProcessForge;

        public OutputProcessViewDirectDistinctOrAfterFactoryForge(
            OutputStrategyPostProcessForge outputStrategyPostProcessForge,
            bool isDistinct,
            MultiKeyClassRef distinctMultiKey,
            ExprTimePeriod afterTimePeriod,
            int? afterConditionNumberOfEvents,
            EventType resultEventType)
        {
            _outputStrategyPostProcessForge = outputStrategyPostProcessForge;
            _isDistinct = isDistinct;
            DistinctMultiKey = distinctMultiKey;
            AfterTimePeriod = afterTimePeriod;
            AfterConditionNumberOfEvents = afterConditionNumberOfEvents;
            ResultEventType = resultEventType;
        }

        public bool IsCodeGenerated => false;
        
        public MultiKeyClassRef DistinctMultiKey { get; }

        public int? AfterConditionNumberOfEvents { get; }

        public ExprTimePeriod AfterTimePeriod { get; }

        public EventType ResultEventType { get; }

        public void ProvideCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(
                NewInstance<OutputProcessViewDirectDistinctOrAfterFactory>(
                    _outputStrategyPostProcessForge == null
                        ? ConstantNull()
                        : _outputStrategyPostProcessForge.Make(method, symbols, classScope),
                    Constant(_isDistinct),
                    MultiKeyCodegen.CodegenGetterEventDistinct(
                        _isDistinct, ResultEventType, DistinctMultiKey, method, classScope),
                    AfterTimePeriod == null
                        ? ConstantNull()
                        : AfterTimePeriod.TimePeriodComputeForge.MakeEvaluator(method, classScope),
                    Constant(AfterConditionNumberOfEvents),
                    EventTypeUtility.ResolveTypeCodegen(ResultEventType, symbols.GetAddInitSvc(method))));
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
        }
    }
} // end of namespace