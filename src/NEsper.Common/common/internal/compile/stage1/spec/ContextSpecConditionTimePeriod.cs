///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class ContextSpecConditionTimePeriod : ContextSpecCondition,
        ScheduleHandleCallbackProvider
    {
        public ContextSpecConditionTimePeriod(
            ExprTimePeriod timePeriod,
            bool immediate)
        {
            TimePeriod = timePeriod;
            IsImmediate = immediate;
        }

        public ExprTimePeriod TimePeriod { get; set; }

        public bool IsImmediate { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (ScheduleCallbackId == -1) {
                throw new IllegalStateException("Unassigned schedule callback id");
            }

            var method = parent.MakeChild(typeof(ContextConditionDescriptorTimePeriod), GetType(), classScope);
            method.Block
                .DeclareVarNewInstance<ContextConditionDescriptorTimePeriod>("condition")
                .DeclareVar<TimePeriodCompute>(
                    "eval",
                    TimePeriod.TimePeriodComputeForge.MakeEvaluator(method, classScope))
                .SetProperty(Ref("condition"), "TimePeriodCompute", Ref("eval"))
                .SetProperty(Ref("condition"), "ScheduleCallbackId", Constant(ScheduleCallbackId))
                .SetProperty(Ref("condition"), "IsImmediate", Constant(IsImmediate))
                .MethodReturn(Ref("condition"));
            return LocalMethod(method);
        }

        public int ScheduleCallbackId { get; set; } = -1;

        public T Accept<T>(ContextSpecConditionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
} // end of namespace