///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalStreamNumUnd : ExprForge,
        ExprEvaluator,
        ExprNodeRenderable
    {
        private readonly int _streamNum;
        private readonly Type _returnType;

        public ExprEvalStreamNumUnd(
            int streamNum,
            Type returnType)
        {
            this._streamNum = streamNum;
            this._returnType = returnType;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return eventsPerStream[_streamNum].Underlying;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(codegenMethodScope);
            return FlexCast(_returnType, ExprDotUnderlying(ArrayAtIndex(refEPS, Constant(_streamNum))));
        }

        public ExprEvaluator ExprEvaluator {
            get => this;
        }

        public Type EvaluationType {
            get => _returnType;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => this;
        }

        public void ToEPL(TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(this.GetType().Name);
        }
    }
} // end of namespace