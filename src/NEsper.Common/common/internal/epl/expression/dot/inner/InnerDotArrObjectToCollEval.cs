///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.inner
{
    public class InnerDotArrObjectToCollEval : ExprDotEvalRootChildInnerEval
    {
        private readonly ExprEvaluator _rootEvaluator;

        public InnerDotArrObjectToCollEval(ExprEvaluator rootEvaluator)
        {
            _rootEvaluator = rootEvaluator;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var array = _rootEvaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (array == null) {
                return null;
            }

            return Arrays.AsList((object[])array);
        }

        public static CodegenExpression CodegenEvaluate(
            InnerDotArrObjectToCollForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var evalType = forge.rootForge.EvaluationType;
            var elementType = evalType.GetComponentType();

            var methodNode = codegenMethodScope.MakeChild(
                typeof(ICollection<>).MakeGenericType(elementType),
                typeof(InnerDotArrObjectToCollEval),
                codegenClassScope);

            methodNode.Block
                .DeclareVar(
                    evalType,
                    "array",
                    forge.rootForge.EvaluateCodegen(evalType, methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("array")
                .MethodReturn(Unwrap(elementType, Ref("array")));
            return LocalMethod(methodNode);
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }
    }
} // end of namespace