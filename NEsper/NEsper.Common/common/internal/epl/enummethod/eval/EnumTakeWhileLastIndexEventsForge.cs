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
    public class EnumTakeWhileLastIndexEventsForge : EnumForgeBaseIndex
    {
        public EnumTakeWhileLastIndexEventsForge(
            ExprForge innerExpression, int streamNumLambda, ObjectArrayEventType indexEventType)
            : base(innerExpression, streamNumLambda, indexEventType)
        {
        }

        public override EnumEval EnumEvaluator =>
            new EnumTakeWhileLastIndexEventsForgeEval(this, innerExpression.ExprEvaluator);

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams premade, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return EnumTakeWhileLastIndexEventsForgeEval.Codegen(this, premade, codegenMethodScope, codegenClassScope);
        }
    }
} // end of namespace