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
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.metrics.instrumentation;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    /// <summary>
    ///     Represents the TYPEOF(a) function is an expression tree.
    /// </summary>
    public class ExprTypeofNodeForgeStreamEvent : ExprTypeofNodeForge,
        ExprEvaluator
    {
        private readonly ExprTypeofNode _parent;
        private readonly int _streamNum;

        public ExprTypeofNodeForgeStreamEvent(
            ExprTypeofNode parent,
            int streamNum)
        {
            this._parent = parent;
            this._streamNum = streamNum;
        }

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprEvaluator ExprEvaluator => this;

        public override ExprNodeRenderable ExprForgeRenderable => _parent;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var @event = eventsPerStream[_streamNum];
            if (@event == null) {
                return null;
            }

            if (@event is VariantEvent variantEvent) {
                return variantEvent.UnderlyingEventBean.EventType.Name;
            }

            return @event.EventType.Name;
        }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(string),
                typeof(ExprTypeofNodeForgeStreamEvent),
                codegenClassScope);

            var refEPS = exprSymbol.GetAddEps(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(_streamNum)))
                .IfRefNullReturnNull("@event")
                .IfCondition(InstanceOf(Ref("@event"), typeof(VariantEvent)))
                .BlockReturn(
                    ExprDotMethodChain(Cast(typeof(VariantEvent), Ref("@event")))
                        .Get("UnderlyingEventBean")
                        .Get("EventType")
                        .Get("Name"))
                .MethodReturn(
                    ExprDotMethodChain(Ref("@event"))
                        .Get("EventType")
                        .Get("Name"));
            return LocalMethod(methodNode);
        }

        public override CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(),
                this,
                "ExprTypeof",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }
    }
} // end of namespace