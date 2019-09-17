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
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.adder;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.settings;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
    public class TimePeriodComputeConstGivenCalAddForge : TimePeriodComputeForge
    {
        private readonly int[] added;
        private readonly TimePeriodAdder[] adders;
        private readonly int indexMicroseconds;
        private readonly TimeAbacus timeAbacus;

        public TimePeriodComputeConstGivenCalAddForge(
            TimePeriodAdder[] adders,
            int[] added,
            TimeAbacus timeAbacus)
        {
            this.adders = adders;
            this.added = added;
            this.timeAbacus = timeAbacus;
            indexMicroseconds = ExprTimePeriodUtil.FindIndexMicroseconds(adders);
        }

        public TimePeriodCompute Evaluator =>
            new TimePeriodComputeConstGivenCalAddEval(adders, added, timeAbacus, indexMicroseconds, TimeZoneInfo.Utc);

        public CodegenExpression MakeEvaluator(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(TimePeriodComputeConstGivenCalAddEval), GetType(), classScope);
            method.Block
                .DeclareVar<TimePeriodComputeConstGivenCalAddEval>(
                    "eval",
                    NewInstance(typeof(TimePeriodComputeConstGivenCalAddEval)))
                .SetProperty(Ref("eval"), "Adders", TimePeriodAdderUtil.MakeArray(adders, parent, classScope))
                .SetProperty(Ref("eval"), "Added", Constant(added))
                .SetProperty(Ref("eval"), "TimeAbacus", classScope.AddOrGetFieldSharable(TimeAbacusField.INSTANCE))
                .SetProperty(Ref("eval"), "IndexMicroseconds", Constant(indexMicroseconds))
                .SetProperty(
                    Ref("eval"),
                    "TimeZone",
                    classScope.AddOrGetFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE))
                .MethodReturn(Ref("eval"));
            return LocalMethod(method);
        }
    }
} // end of namespace