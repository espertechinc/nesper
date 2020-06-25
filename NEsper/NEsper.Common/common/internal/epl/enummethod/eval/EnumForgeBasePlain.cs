///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public abstract class EnumForgeBasePlain : EnumForge
    {
        public EnumForgeBasePlain(ExprDotEvalParamLambda lambda) 
            : this(lambda.BodyForge, lambda.StreamCountIncoming)
        {
        }

        public EnumForgeBasePlain(
            ExprForge innerExpression,
            int streamCountIncoming) : this(streamCountIncoming)
        {
            InnerExpression = innerExpression;
        }

        public EnumForgeBasePlain(int streamCountIncoming)
        {
            StreamNumLambda = streamCountIncoming;
        }

        public ExprForge InnerExpression { get; }

        public int StreamNumLambda { get; }

        public int StreamNumSize => StreamNumLambda + 1;

        public abstract EnumEval EnumEvaluator { get; }

        public abstract CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace