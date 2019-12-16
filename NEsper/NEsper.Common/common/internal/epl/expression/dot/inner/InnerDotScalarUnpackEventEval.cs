///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.inner
{
    public class InnerDotScalarUnpackEventEval : ExprDotEvalRootChildInnerEval
    {
        private ExprEvaluator rootEvaluator;

        public InnerDotScalarUnpackEventEval(ExprEvaluator rootEvaluator)
        {
            this.rootEvaluator = rootEvaluator;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            object target = rootEvaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (target is EventBean) {
                return ((EventBean) target).Underlying;
            }

            return target;
        }

        public static CodegenExpression CodegenEvaluate(
            InnerDotScalarUnpackEventForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                forge.RootForge.EvaluationType,
                typeof(InnerDotScalarUnpackEventEval),
                codegenClassScope);

            methodNode.Block
                .DeclareVar<object>(
                    "target",
                    forge.RootForge.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                .IfInstanceOf("target", typeof(EventBean))
                .BlockReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        forge.RootForge.EvaluationType,
                        ExprDotName(Cast(typeof(EventBean), Ref("target")), "Underlying")))
                .MethodReturn(CodegenLegoCast.CastSafeFromObjectType(forge.RootForge.EvaluationType, Ref("target")));
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