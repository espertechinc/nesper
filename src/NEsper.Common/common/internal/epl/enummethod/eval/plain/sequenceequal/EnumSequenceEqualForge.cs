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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumSequenceEqualForge : EnumForgeBasePlain
    {
        private readonly bool _scalar;

        public EnumSequenceEqualForge(
            ExprForge innerExpression,
            int streamCountIncoming,
            bool scalar)
            : base(innerExpression, streamCountIncoming)
        {
            _scalar = scalar;
        }

        public bool Scalar => _scalar;

        public override EnumEval EnumEvaluator {
            get => new EnumSequenceEqualForgeEval(InnerExpression.ExprEvaluator);
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return EnumSequenceEqualForgeEval.Codegen(this, premade, codegenMethodScope, codegenClassScope);
        }
    }
} // end of namespace