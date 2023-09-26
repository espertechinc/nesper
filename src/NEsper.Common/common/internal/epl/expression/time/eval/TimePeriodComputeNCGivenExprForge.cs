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
using com.espertech.esper.common.@internal.epl.expression.time.abacus;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
    public class TimePeriodComputeNCGivenExprForge : TimePeriodComputeForge
    {
        private readonly ExprForge _secondsEvaluator;
        private readonly TimeAbacus _timeAbacus;

        public TimePeriodComputeNCGivenExprForge(
            ExprForge secondsEvaluator,
            TimeAbacus timeAbacus)
        {
            _secondsEvaluator = secondsEvaluator;
            _timeAbacus = timeAbacus;
        }

        public TimePeriodCompute Evaluator =>
            new TimePeriodComputeNCGivenExprEval(_secondsEvaluator.ExprEvaluator, _timeAbacus);

        public CodegenExpression MakeEvaluator(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(TimePeriodComputeNCGivenExprEval), GetType(), classScope);
            method.Block
                .DeclareVarNewInstance<TimePeriodComputeNCGivenExprEval>("eval")
                .SetProperty(
                    Ref("eval"),
                    "SecondsEvaluator",
                    ExprNodeUtilityCodegen.CodegenEvaluator(_secondsEvaluator, method, GetType(), classScope))
                .SetProperty(
                    Ref("eval"),
                    "TimeAbacus",
                    classScope.AddOrGetDefaultFieldSharable(TimeAbacusField.INSTANCE))
                .MethodReturn(Ref("eval"));
            return LocalMethod(method);
        }
    }
} // end of namespace