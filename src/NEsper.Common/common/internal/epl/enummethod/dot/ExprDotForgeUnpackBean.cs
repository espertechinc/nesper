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
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.rettype;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotForgeUnpackBean : ExprDotForge,
        ExprDotEval
    {
        private readonly EPChainableType returnType;

        public ExprDotForgeUnpackBean(EventType lambdaType)
        {
            returnType = EPChainableTypeHelper.SingleValueNonNull(lambdaType.UnderlyingType);
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = (EventBean) target;
            return theEvent?.Underlying;
        }

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            Type resultType = EPChainableTypeHelper.GetCodegenReturnType(returnType);
            CodegenMethod methodNode = parent
                .MakeChild(resultType, typeof(ExprDotForgeUnpackBean), classScope)
                .AddParam(innerType, "target");

            methodNode.Block
                .IfRefNullReturnNull("target")
                .MethodReturn(FlexCast(resultType, ExprDotUnderlying(Cast(typeof(EventBean), Ref("target")))));
            return LocalMethod(methodNode, inner);
        }

        public EPChainableType TypeInfo {
            get => returnType;
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitUnderlyingEvent();
        }

        public ExprDotEval DotEvaluator {
            get => this;
        }

        public ExprDotForge DotForge {
            get => this;
        }
    }
} // end of namespace