///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalComputerForgeFactory
    {
        public class IntervalComputerDuringAndIncludesThresholdForge : IntervalComputerForge
        {
            internal readonly bool during;
            internal readonly IntervalDeltaExprForge threshold;

            public IntervalComputerDuringAndIncludesThresholdForge(
                bool during,
                IntervalDeltaExprForge threshold)
            {
                this.during = during;
                this.threshold = threshold;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerDuringAndIncludesThresholdEval(during, threshold.MakeEvaluator());
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalComputerDuringAndIncludesThresholdEval.Codegen(
                    this,
                    leftStart,
                    leftEnd,
                    rightStart,
                    rightEnd,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }
        }
    }
}