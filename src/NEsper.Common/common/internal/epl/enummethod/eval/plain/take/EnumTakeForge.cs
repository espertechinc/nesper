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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.plain.take
{
    public class EnumTakeForge : EnumForge
    {
        public EnumTakeForge(
            ExprForge sizeEval,
            int numStreams,
            bool scalar)
        {
            SizeEval = sizeEval;
            StreamNumSize = numStreams;
            Scalar = scalar;
        }

        public bool Scalar { get; }

        public ExprForge SizeEval { get; }

        public int StreamNumSize { get; }

        public virtual EnumEval EnumEvaluator => new EnumTakeForgeEval(SizeEval.ExprEvaluator);

        public virtual CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return EnumTakeForgeEval.Codegen(
                this,
                premade,
                codegenMethodScope,
                codegenClassScope);
        }
    }
} // end of namespace