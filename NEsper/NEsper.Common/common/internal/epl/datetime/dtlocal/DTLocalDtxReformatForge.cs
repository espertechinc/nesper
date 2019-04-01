///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.reformatop;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtxReformatForge : DTLocalReformatForgeBase
    {
        public DTLocalDtxReformatForge(ReformatForge reformatForge)
            : base(reformatForge)
        {
        }

        public override DTLocalEvaluator DTEvaluator => new DTLocalDtxReformatEval(reformatForge.Op);

        public override CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return reformatForge.CodegenDateTimeEx(inner, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        private class DTLocalDtxReformatEval : DTLocalReformatEvalBase
        {
            internal DTLocalDtxReformatEval(ReformatOp reformatOp)
                : base(reformatOp)
            {
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return reformatOp.Evaluate((DateTimeEx) target, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }
    }
} // end of namespace