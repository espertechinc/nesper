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
        public class IntervalComputerDuringMinMaxStartEndForge : IntervalComputerForge
        {
            internal readonly bool during;
            internal readonly IntervalDeltaExprForge maxEndEval;
            internal readonly IntervalDeltaExprForge maxStartEval;
            internal readonly IntervalDeltaExprForge minEndEval;
            internal readonly IntervalDeltaExprForge minStartEval;

            public IntervalComputerDuringMinMaxStartEndForge(
                bool during,
                IntervalDeltaExprForge[] parameters)
            {
                this.during = during;
                minStartEval = parameters[0];
                maxStartEval = parameters[1];
                minEndEval = parameters[2];
                maxEndEval = parameters[3];
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return new IntervalComputerDuringMinMaxStartEndEval(
                    during,
                    minStartEval.MakeEvaluator(),
                    maxStartEval.MakeEvaluator(),
                    minEndEval.MakeEvaluator(),
                    maxEndEval.MakeEvaluator());
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
                return IntervalComputerDuringMinMaxStartEndEval.Codegen(
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