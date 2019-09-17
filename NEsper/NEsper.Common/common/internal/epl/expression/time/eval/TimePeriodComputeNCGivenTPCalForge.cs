///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.adder;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.settings;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
    public class TimePeriodComputeNCGivenTPCalForge : TimePeriodComputeForge
    {
        private readonly int indexMicroseconds;
        private readonly ExprTimePeriodForge timePeriodForge;

        public TimePeriodComputeNCGivenTPCalForge(ExprTimePeriodForge timePeriodForge)
        {
            this.timePeriodForge = timePeriodForge;
            indexMicroseconds = ExprTimePeriodUtil.FindIndexMicroseconds(timePeriodForge.Adders);
        }

        public TimePeriodCompute Evaluator => new TimePeriodComputeNCGivenTPCalForgeEval(
            timePeriodForge.Evaluators,
            timePeriodForge.Adders,
            timePeriodForge.TimeAbacus,
            TimeZoneInfo.Utc,
            indexMicroseconds);

        public CodegenExpression MakeEvaluator(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(TimePeriodComputeNCGivenTPCalForgeEval), GetType(), classScope);
            method.Block
                .DeclareVar<TimePeriodComputeNCGivenTPCalForgeEval>(
                    "eval",
                    NewInstance(typeof(TimePeriodComputeNCGivenTPCalForgeEval)))
                .SetProperty(
                    Ref("eval"),
                    "Adders",
                    TimePeriodAdderUtil.MakeArray(timePeriodForge.Adders, parent, classScope))
                .SetProperty(
                    Ref("eval"),
                    "Evaluators",
                    ExprNodeUtilityCodegen.CodegenEvaluators(timePeriodForge.Forges, method, GetType(), classScope))
                .SetProperty(Ref("eval"), "TimeAbacus", classScope.AddOrGetFieldSharable(TimeAbacusField.INSTANCE))
                .SetProperty(
                    Ref("eval"),
                    "TimeZone",
                    classScope.AddOrGetFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE))
                .SetProperty(Ref("eval"), "IndexMicroseconds", Constant(indexMicroseconds))
                .MethodReturn(Ref("eval"));
            return LocalMethod(method);
        }
    }
} // end of namespace