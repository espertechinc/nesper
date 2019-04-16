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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumAggregateScalarForge : EnumAggregateForge
    {
        internal readonly ObjectArrayEventType evalEventType;

        public EnumAggregateScalarForge(
            ExprForge initialization,
            ExprForge innerExpression,
            int streamNumLambda,
            ObjectArrayEventType resultEventType,
            ObjectArrayEventType evalEventType)
            : base(initialization, innerExpression, streamNumLambda, resultEventType)

        {
            this.evalEventType = evalEventType;
        }

        public override EnumEval EnumEvaluator => new EnumAggregateScalarForgeEval(
            this, initialization.ExprEvaluator, innerExpression.ExprEvaluator);

        public ObjectArrayEventType EvalEventType => evalEventType;

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return EnumAggregateScalarForgeEval.Codegen(this, premade, codegenMethodScope, codegenClassScope);
        }
    }
} // end of namespace