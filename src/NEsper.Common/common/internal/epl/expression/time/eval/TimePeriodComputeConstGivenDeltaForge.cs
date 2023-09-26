///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
    public class TimePeriodComputeConstGivenDeltaForge : TimePeriodComputeForge
    {
        private readonly long timeDelta;

        public TimePeriodComputeConstGivenDeltaForge(long timeDelta)
        {
            this.timeDelta = timeDelta;
        }

        public TimePeriodCompute Evaluator => new TimePeriodComputeConstGivenDeltaEval(timeDelta);

        public CodegenExpression MakeEvaluator(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return NewInstance<TimePeriodComputeConstGivenDeltaEval>(Constant(timeDelta));
        }
    }
} // end of namespace