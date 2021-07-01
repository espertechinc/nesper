///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class ContextSpecConditionCrontab : ContextSpecCondition,
        ScheduleHandleCallbackProvider
    {
        private ExprForge[][] forges;

        public ContextSpecConditionCrontab(
            IList<IList<ExprNode>> crontabs,
            bool immediate)
        {
            Crontabs = crontabs;
            IsImmediate = immediate;
        }

        public IList<IList<ExprNode>> Crontabs { get; }

        public int ScheduleCallbackId { get; set; } = -1;

        public bool IsImmediate { get; }

        public ExprForge[][] ForgesPerCrontab {
            set => forges = value;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (ScheduleCallbackId == -1) {
                throw new IllegalStateException("Unassigned schedule callback id");
            }

            var method = parent
                .MakeChild(typeof(ContextConditionDescriptorCrontab), GetType(), classScope);
            method.Block
                .DeclareVar<ContextConditionDescriptorCrontab>(
                    "condition",
                    NewInstance(typeof(ContextConditionDescriptorCrontab)))
                .SetProperty(
                    Ref("condition"),
                    "EvaluatorsPerCrontab",
                    ExprNodeUtilityCodegen.CodegenEvaluators(forges, method, GetType(), classScope))
                .SetProperty(
                    Ref("condition"),
                    "ScheduleCallbackId",
                    Constant(ScheduleCallbackId))
                .SetProperty(
                    Ref("condition"),
                    "IsImmediate",
                    Constant(IsImmediate))
                .MethodReturn(
                    Ref("condition"));
            return LocalMethod(method);
        }
    }
} // end of namespace