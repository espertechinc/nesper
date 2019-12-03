///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumMinMaxScalarLambdaForge : EnumForgeBase
    {
        internal readonly bool max;
        internal readonly ObjectArrayEventType resultEventType;

        public EnumMinMaxScalarLambdaForge(
            ExprForge innerExpression,
            int streamCountIncoming,
            bool max,
            ObjectArrayEventType resultEventType)
            : base(innerExpression, streamCountIncoming)

        {
            this.max = max;
            this.resultEventType = resultEventType;
        }

        public override EnumEval EnumEvaluator {
            get => new EnumMinMaxScalarLambdaForgeEval(this, InnerExpression.ExprEvaluator);
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return EnumMinMaxScalarLambdaForgeEval.Codegen(this, premade, codegenMethodScope, codegenClassScope);
        }
    }
} // end of namespace