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
    public abstract class EnumForgeBaseIndex : EnumForge
    {
        internal ObjectArrayEventType indexEventType;
        internal ExprForge innerExpression;
        internal int streamNumLambda;

        public EnumForgeBaseIndex(
            ExprForge innerExpression,
            int streamNumLambda,
            ObjectArrayEventType indexEventType)
        {
            this.innerExpression = innerExpression;
            this.streamNumLambda = streamNumLambda;
            this.indexEventType = indexEventType;
        }

        public int StreamNumSize => streamNumLambda + 2;

        public abstract EnumEval EnumEvaluator { get; }

        public abstract CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace