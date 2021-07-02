///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    public class TimerScheduleSpecComputeISOStringForge : TimerScheduleSpecComputeForge
    {
        private readonly ExprNode parameter;

        public TimerScheduleSpecComputeISOStringForge(ExprNode parameter)
        {
            this.parameter = parameter;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(
                typeof(TimerScheduleSpecComputeISOString),
                GetType(),
                classScope);
            method.Block.MethodReturn(
                NewInstance<TimerScheduleSpecComputeISOString>(
                    ExprNodeUtilityCodegen.CodegenEvaluator(parameter.Forge, method, GetType(), classScope)));
            return LocalMethod(method);
        }

        public void VerifyComputeAllConst(ExprValidationContext validationContext)
        {
            TimerScheduleSpecComputeISOString.Compute(parameter.Forge.ExprEvaluator, null, null);
        }
    }
} // end of namespace