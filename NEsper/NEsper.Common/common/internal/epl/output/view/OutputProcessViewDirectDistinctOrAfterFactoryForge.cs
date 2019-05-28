///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    /// Output process view that does not enforce any output policies and may simply
    /// hand over events to child views, but works with distinct and after-output policies
    /// </summary>
    public class OutputProcessViewDirectDistinctOrAfterFactoryForge : OutputProcessViewFactoryForge
    {
        private readonly OutputStrategyPostProcessForge outputStrategyPostProcessForge;
        private readonly bool isDistinct;
        internal readonly ExprTimePeriod afterTimePeriod;
        internal readonly int? afterConditionNumberOfEvents;
        internal readonly EventType resultEventType;

        public OutputProcessViewDirectDistinctOrAfterFactoryForge(
            OutputStrategyPostProcessForge outputStrategyPostProcessForge,
            bool isDistinct,
            ExprTimePeriod afterTimePeriod,
            int? afterConditionNumberOfEvents,
            EventType resultEventType)
        {
            this.outputStrategyPostProcessForge = outputStrategyPostProcessForge;
            this.isDistinct = isDistinct;
            this.afterTimePeriod = afterTimePeriod;
            this.afterConditionNumberOfEvents = afterConditionNumberOfEvents;
            this.resultEventType = resultEventType;
        }

        public bool IsCodeGenerated {
            get { return false; }
        }

        public void ProvideCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(
                NewInstance<OutputProcessViewDirectDistinctOrAfterFactory>(
                    outputStrategyPostProcessForge == null ? ConstantNull() : outputStrategyPostProcessForge.Make(method, symbols, classScope),
                    Constant(isDistinct),
                    afterTimePeriod == null ? ConstantNull() : afterTimePeriod.TimePeriodComputeForge.MakeEvaluator(method, classScope),
                    Constant(afterConditionNumberOfEvents),
                    EventTypeUtility.ResolveTypeCodegen(resultEventType, symbols.GetAddInitSvc(method))));
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

        public void IteratorCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
        }

        public void CollectSchedules(IList<ScheduleHandleCallbackProvider> scheduleHandleCallbackProviders)
        {
        }
    }
} // end of namespace