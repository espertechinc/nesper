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
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumLastOfPredicateScalarForge : EnumForgeBaseScalar
    {
        internal readonly EPType resultType;

        public EnumLastOfPredicateScalarForge(
            ExprForge innerExpression,
            int streamCountIncoming,
            ObjectArrayEventType type,
            EPType resultType)
            : base(innerExpression, streamCountIncoming, type)
        {
            this.resultType = resultType;
        }

        public override EnumEval EnumEvaluator {
            get => new EnumLastOfPredicateScalarForgeEval(this, innerExpression.ExprEvaluator);
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return EnumLastOfPredicateScalarForgeEval.Codegen(this, premade, codegenMethodScope, codegenClassScope);
        }
    }
} // end of namespace