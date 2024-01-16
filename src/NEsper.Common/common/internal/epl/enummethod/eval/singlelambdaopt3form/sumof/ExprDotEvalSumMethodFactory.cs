///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof {
    public interface ExprDotEvalSumMethodFactory {
        ExprDotEvalSumMethod SumAggregator { get; }

        Type ValueType { get; }

        void CodegenDeclare(CodegenBlock block);

        void CodegenEnterNumberTypedNonNull(
            CodegenBlock block,
            CodegenExpressionRef value);

        void CodegenEnterObjectTypedNonNull(
            CodegenBlock block,
            CodegenExpressionRef value);

        void CodegenReturn(CodegenBlock block);
    }
} // end of namespace