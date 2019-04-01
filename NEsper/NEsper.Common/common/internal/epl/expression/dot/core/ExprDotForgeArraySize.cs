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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotForgeArraySize : ExprDotForge,
        ExprDotEval
    {
        public EPType TypeInfo => EPTypeHelper.SingleValue(typeof(int?));

        public ExprDotEval DotEvaluator => this;

        public ExprDotForge DotForge => this;

        public object Evaluate(
            object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target == null) {
                return null;
            }

            return ((Array) target).Length;
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitArrayLength();
        }

        public CodegenExpression Codegen(
            CodegenExpression inner, Type innerType, CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope.MakeChild(typeof(int?), typeof(ExprDotForgeArraySize), codegenClassScope)
                .AddParam(innerType, "target").Block
                .IfRefNullReturnNull("target")
                .MethodReturn(ArrayLength(Ref("target")));
            return LocalMethodBuild(method).Pass(inner).Call();
        }
    }
} // end of namespace