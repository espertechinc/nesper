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
using com.espertech.esper.common.@internal.epl.expression.time.node;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    public class TimerScheduleSpecComputeFromExprForge : TimerScheduleSpecComputeForge
    {
        private readonly ExprNode dateNode;
        private readonly ExprNode repetitionsNode;
        private readonly ExprTimePeriod periodNode;

        public TimerScheduleSpecComputeFromExprForge(
            ExprNode dateNode,
            ExprNode repetitionsNode,
            ExprTimePeriod periodNode)
        {
            this.dateNode = dateNode;
            this.repetitionsNode = repetitionsNode;
            this.periodNode = periodNode;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(
                typeof(TimerScheduleSpecComputeFromExpr),
                GetType(),
                classScope);
            method.Block
                .DeclareVar<TimerScheduleSpecComputeFromExpr>(
                    "compute",
                    NewInstance(typeof(TimerScheduleSpecComputeFromExpr)))
                .SetProperty(
                    Ref("compute"),
                    "Date",
                    dateNode == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(dateNode.Forge, method, GetType(), classScope))
                .SetProperty(
                    Ref("compute"),
                    "Repetitions",
                    repetitionsNode == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            repetitionsNode.Forge,
                            method,
                            GetType(),
                            classScope));
            if (periodNode != null) {
                method.Block.SetProperty(
                    Ref("compute"),
                    "TimePeriod",
                    periodNode.MakeTimePeriodAnonymous(method, classScope));
            }

            method.Block.MethodReturn(Ref("compute"));
            return LocalMethod(method);
        }

        public void VerifyComputeAllConst(ExprValidationContext validationContext)
        {
            TimerScheduleSpecComputeFromExpr.Compute(
                dateNode == null ? null : dateNode.Forge.ExprEvaluator,
                repetitionsNode == null ? null : repetitionsNode.Forge.ExprEvaluator,
                periodNode == null ? null : periodNode.TimePeriodEval,
                null,
                null,
                null,
                validationContext.ImportService.TimeAbacus);
        }
    }
} // end of namespace