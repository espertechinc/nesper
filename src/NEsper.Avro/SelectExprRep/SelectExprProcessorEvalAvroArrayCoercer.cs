///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace NEsper.Avro.SelectExprRep
{
    public class SelectExprProcessorEvalAvroArrayCoercer : ExprEvaluator,
        ExprForge,
        ExprNodeRenderable
    {
        private readonly ExprForge _forge;
        private readonly TypeWidenerSPI _widener;
        private ExprEvaluator _eval;

        public SelectExprProcessorEvalAvroArrayCoercer(
            ExprForge forge,
            TypeWidenerSPI widener,
            Type resultType)
        {
            _forge = forge;
            _widener = widener;
            EvaluationType = resultType;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            object result = _eval.Evaluate(eventsPerStream, isNewData, context);
            return _widener.Widen(result);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return _widener.WidenCodegen(
                _forge.EvaluateCodegen(requiredType, codegenMethodScope, exprSymbol, codegenClassScope),
                codegenMethodScope,
                codegenClassScope);
        }

        public ExprEvaluator ExprEvaluator {
            get {
                _eval = _forge.ExprEvaluator;
                return this;
            }
        }

        public Type EvaluationType { get; }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public ExprNodeRenderable ForgeRenderable {
            get => this;
        }

        public ExprNodeRenderable ExprForgeRenderable => ForgeRenderable;

        public void ToEPL(TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace