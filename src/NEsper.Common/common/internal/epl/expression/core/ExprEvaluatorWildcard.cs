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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprEvaluatorWildcard : ExprEvaluator
    {
        public static readonly ExprEvaluatorWildcard INSTANCE = new ExprEvaluatorWildcard();

        private ExprEvaluatorWildcard()
        {
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var @event = eventsPerStream[0];

            return @event?.Underlying;
        }

        public static CodegenExpression Codegen(
            Type requiredType,
            Type underlyingType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                requiredType == typeof(object) ? typeof(object) : underlyingType,
                typeof(ExprEvaluatorWildcard),
                codegenClassScope);
            var refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(0)))
                .IfRefNullReturnNull("@event");
            if (requiredType == typeof(object)) {
                methodNode.Block.MethodReturn(ExprDotName(Ref("@event"), "Underlying"));
            }
            else {
                methodNode.Block.MethodReturn(Cast(underlyingType, ExprDotName(Ref("@event"), "Underlying")));
            }

            return LocalMethod(methodNode);
        }
    }
} // end of namespace