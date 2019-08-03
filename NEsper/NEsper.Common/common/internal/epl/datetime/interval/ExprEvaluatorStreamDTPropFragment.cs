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
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public class ExprEvaluatorStreamDTPropFragment : ExprForge,
        ExprEvaluator,
        ExprNodeRenderable
    {
        private readonly EventPropertyGetterSPI getterFragment;
        private readonly EventPropertyGetterSPI getterTimestamp;

        private readonly int streamId;

        public ExprEvaluatorStreamDTPropFragment(
            int streamId,
            EventPropertyGetterSPI getterFragment,
            EventPropertyGetterSPI getterTimestamp)
        {
            this.streamId = streamId;
            this.getterFragment = getterFragment;
            this.getterTimestamp = getterTimestamp;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var theEvent = eventsPerStream[streamId];
            if (theEvent == null) {
                return null;
            }

            var @event = getterFragment.GetFragment(theEvent);
            if (!(@event is EventBean)) {
                return null;
            }

            return getterTimestamp.Get((EventBean) @event);
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprEvaluator ExprEvaluator => this;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(long),
                typeof(ExprEvaluatorStreamDTPropFragment),
                codegenClassScope);
            var refEPS = exprSymbol.GetAddEPS(methodNode);

            methodNode.Block
                .DeclareVar<EventBean>("theEvent", ArrayAtIndex(refEPS, Constant(streamId)))
                .IfRefNullReturnNull("theEvent")
                .DeclareVar<object>(
                    "@event",
                    getterFragment.EventBeanFragmentCodegen(Ref("theEvent"), methodNode, codegenClassScope))
                .IfCondition(Not(InstanceOf(Ref("@event"), typeof(EventBean))))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        typeof(long),
                        getterTimestamp.EventBeanGetCodegen(
                            Cast(typeof(EventBean), Ref("@event")),
                            methodNode,
                            codegenClassScope)));
            return LocalMethod(methodNode);
        }

        public Type EvaluationType => typeof(long);

        public ExprNodeRenderable ExprForgeRenderable => this;

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace