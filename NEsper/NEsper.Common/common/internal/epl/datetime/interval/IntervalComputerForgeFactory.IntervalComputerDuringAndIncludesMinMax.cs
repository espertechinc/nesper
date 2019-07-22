///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        public class IntervalComputerDuringAndIncludesMinMax : IntervalComputerForge
        {
            internal readonly bool during;
            internal readonly IntervalDeltaExprForge maxEval;
            internal readonly IntervalDeltaExprForge minEval;

            public IntervalComputerDuringAndIncludesMinMax(
                bool during,
                IntervalDeltaExprForge minEval,
                IntervalDeltaExprForge maxEval)
            {
                this.during = during;
                this.minEval = minEval;
                this.maxEval = maxEval;
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerDuringAndIncludesMinMaxEval(
                    during,
                    minEval.MakeEvaluator(),
                    maxEval.MakeEvaluator());
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
                return IntervalComputerDuringAndIncludesMinMaxEval.Codegen(
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