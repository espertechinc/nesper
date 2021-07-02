///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.inner
{
    public class InnerDotScalarUnpackEventEval : ExprDotEvalRootChildInnerEval
    {
        private readonly ExprEvaluator _rootEvaluator;

        public InnerDotScalarUnpackEventEval(ExprEvaluator rootEvaluator)
        {
            this._rootEvaluator = rootEvaluator;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var target = _rootEvaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (target is EventBean eventBean) {
                return eventBean.Underlying;
            }

            return target;
        }

        public static CodegenExpression CodegenEvaluate(
            InnerDotScalarUnpackEventForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var type = forge.RootForge.EvaluationType;
            if (type.IsNullType()) {
                return ConstantNull();
            }
            
            var methodNode = codegenMethodScope.MakeChild(type, typeof(InnerDotScalarUnpackEventEval), codegenClassScope);

            methodNode.Block
                .DeclareVar<object>(
                    "target",
                    forge.RootForge.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                .IfInstanceOf("target", typeof(EventBean))
                .BlockReturn(CodegenLegoCast.CastSafeFromObjectType(type, ExprDotName(Cast(typeof(EventBean), Ref("target")), "Underlying")))
                .MethodReturn(CodegenLegoCast.CastSafeFromObjectType(type, Ref("target")));
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