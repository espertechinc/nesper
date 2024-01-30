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
        private readonly string _fragmentType;
        private readonly EventPropertyGetterSPI _getter;

        private readonly ExprTypeofNode _parent;
        private readonly int _streamId;

        public ExprTypeofNodeForgeFragmentType(
            ExprTypeofNode parent,
            int streamId,
            EventPropertyGetterSPI getter,
            string fragmentType)
        {
            _parent = parent;
            _streamId = streamId;
            _getter = getter;
            _fragmentType = fragmentType;
        }

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprEvaluator ExprEvaluator => this;

        public override ExprNodeRenderable ExprForgeRenderable => _parent;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var @event = eventsPerStream[_streamId];
            if (@event == null) {
                return null;
            }

            var fragment = _getter.GetFragment(@event);
            if (fragment == null) {
                return null;
            }

            if (fragment is EventBean bean) {
                return bean.EventType.Name;
            }

            if (fragment.GetType().IsArray) {
                var type = _fragmentType + "[]";
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

            var refEPS = exprSymbol.GetAddEps(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(_streamId)))
                .IfRefNullReturnNull("@event")
                .DeclareVar<object>(
                    "fragment",
                    _getter.EventBeanFragmentCodegen(Ref("@event"), methodNode, codegenClassScope))
                .IfRefNullReturnNull("fragment")
                .IfInstanceOf("fragment", typeof(EventBean))
                .BlockReturn(
                    ExprDotMethodChain(Cast(typeof(EventBean), Ref("fragment"))).Get("EventType").Get("Name"))
                .IfCondition(ExprDotMethodChain(Ref("fragment")).Add("GetType").Get("IsArray"))
                .BlockReturn(Constant(_fragmentType + "[]"))
                .MethodReturn(ConstantNull());
            return LocalMethod(methodNode);
        }
    }
} // end of namespace