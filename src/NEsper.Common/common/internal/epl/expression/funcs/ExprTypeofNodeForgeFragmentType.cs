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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprTypeofNodeForgeFragmentType : ExprTypeofNodeForge,
        ExprEvaluator
    {
        private readonly string fragmentType;
        private readonly EventPropertyGetterSPI getter;

        private readonly ExprTypeofNode parent;
        private readonly int streamId;

        public ExprTypeofNodeForgeFragmentType(
            ExprTypeofNode parent,
            int streamId,
            EventPropertyGetterSPI getter,
            string fragmentType)
        {
            this.parent = parent;
            this.streamId = streamId;
            this.getter = getter;
            this.fragmentType = fragmentType;
        }

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprEvaluator ExprEvaluator => this;

        public override ExprNodeRenderable ExprForgeRenderable => parent;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var @event = eventsPerStream[streamId];
            if (@event == null) {
                return null;
            }

            var fragment = getter.GetFragment(@event);
            if (fragment == null) {
                return null;
            }

            if (fragment is EventBean bean) {
                return bean.EventType.Name;
            }

            if (fragment.GetType().IsArray) {
                var type = fragmentType + "[]";
                return type;
            }

            return null;
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

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(string),
                typeof(ExprTypeofNodeForgeFragmentType),
                codegenClassScope);

            var refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(streamId)))
                .IfRefNullReturnNull("@event")
                .DeclareVar<object>(
                    "fragment",
                    getter.EventBeanFragmentCodegen(Ref("@event"), methodNode, codegenClassScope))
                .IfRefNullReturnNull("fragment")
                .IfInstanceOf("fragment", typeof(EventBean))
                .BlockReturn(
                    ExprDotMethodChain(Cast(typeof(EventBean), Ref("fragment"))).Get("EventType").Get("Name"))
                .IfCondition(ExprDotMethodChain(Ref("fragment")).Add("GetType").Get("IsArray"))
                .BlockReturn(Constant(fragmentType + "[]"))
                .MethodReturn(ConstantNull());
            return LocalMethod(methodNode);
        }
    }
} // end of namespace