///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.expression.time.node;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    public sealed class OutputConditionPolledTimeFactoryForge : OutputConditionPolledFactoryForge
    {
        internal readonly ExprTimePeriod timePeriod;

        public OutputConditionPolledTimeFactoryForge(ExprTimePeriod timePeriod)
        {
            this.timePeriod = timePeriod;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(OutputConditionPolledFactory), GetType(), classScope);
            method.Block
                .DeclareVar<TimePeriodCompute>(
                    "delta",
                    timePeriod.TimePeriodComputeForge.MakeEvaluator(method, classScope))
                .MethodReturn(NewInstance<OutputConditionPolledTimeFactory>(Ref("delta")));
            return LocalMethod(method);
        }
    }
} // end of namespace