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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.common
{
    public abstract class HistoricalEventViewableForgeBase : HistoricalEventViewableForge
    {
        internal readonly EventType eventType;
        internal readonly int streamNum;
        internal readonly SortedSet<int> subordinateStreams = new SortedSet<int>();
        protected ExprForge[] inputParamEvaluators;
        protected int scheduleCallbackId = -1;

        public HistoricalEventViewableForgeBase(int streamNum, EventType eventType)
        {
            this.streamNum = streamNum;
            this.eventType = eventType;
        }

        public EventType EventType => eventType;

        public SortedSet<int> RequiredStreams => subordinateStreams;

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(TypeOfImplementation(), GetType(), classScope);
            var @ref = Ref("hist");
            method.Block.DeclareVar(TypeOfImplementation(), @ref.Ref, NewInstance(TypeOfImplementation()))
                .ExprDotMethod(@ref, "setStreamNumber", Constant(streamNum))
                .ExprDotMethod(
                    @ref, "setEventType", EventTypeUtility.ResolveTypeCodegen(eventType, symbols.GetAddInitSvc(method)))
                .ExprDotMethod(@ref, "setHasRequiredStreams", Constant(!subordinateStreams.IsEmpty()))
                .ExprDotMethod(@ref, "setScheduleCallbackId", Constant(scheduleCallbackId))
                .ExprDotMethod(
                    @ref, "setEvaluator",
                    ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                        inputParamEvaluators, null, method, GetType(), classScope));
            CodegenSetter(@ref, method, symbols, classScope);
            method.Block
                .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add("addReadyCallback", @ref))
                .MethodReturn(@ref);
            return LocalMethod(method);
        }

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }

        public abstract void Validate(
            StreamTypeService typeService, StatementBaseInfo @base, StatementCompileTimeServices services);

        public abstract Type TypeOfImplementation();

        public abstract void CodegenSetter(
            CodegenExpressionRef @ref, CodegenMethod method, SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);
    }
} // end of namespace