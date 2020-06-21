///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumAverageDecimalScalarLambdaForge : EnumForgeBase
    {
        internal readonly MathContext optionalMathContext;

        internal readonly ObjectArrayEventType resultEventType;

        public EnumAverageDecimalScalarLambdaForge(
            ExprForge innerExpression,
            int streamCountIncoming,
            ObjectArrayEventType resultEventType,
            MathContext optionalMathContext)
            : base(innerExpression, streamCountIncoming)
        {
            this.resultEventType = resultEventType;
            this.optionalMathContext = optionalMathContext;
        }

        public override EnumEval EnumEvaluator =>
            new EnumAverageDecimalScalarLambdaForgeEval(this, InnerExpression.ExprEvaluator);

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return EnumAverageDecimalScalarLambdaForgeEval.Codegen(
                this,
                premade,
                codegenMethodScope,
                codegenClassScope);
        }
    }
} // end of namespace