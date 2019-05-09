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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalStreamInsertNamedWindow : ExprForge,
        ExprEvaluator,
        ExprNodeRenderable
    {
        private readonly int streamNum;
        private readonly EventType namedWindowAsType;
        private readonly Type returnType;

        public ExprEvalStreamInsertNamedWindow(
            int streamNum,
            EventType namedWindowAsType,
            Type returnType)
        {
            this.streamNum = streamNum;
            this.namedWindowAsType = namedWindowAsType;
            this.returnType = returnType;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException("Cannot evaluate at runtime");
        }

        public int StreamNum {
            get => streamNum;
        }

        public ExprEvaluator ExprEvaluator {
            get => this;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionField eventSvc = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            CodegenExpressionField namedWindowType = codegenClassScope.AddFieldUnshared(
                true, typeof(EventType), EventTypeUtility.ResolveTypeCodegen(namedWindowAsType, EPStatementInitServicesConstants.REF));
            CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(EventBean), typeof(ExprEvalStreamInsertNamedWindow), codegenClassScope);

            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);

            string method = EventTypeUtility.GetAdapterForMethodName(namedWindowAsType);
            methodNode.Block
                .DeclareVar(typeof(EventBean), "event", ArrayAtIndex(refEPS, Constant(streamNum)))
                .IfRefNullReturnNull("event")
                .MethodReturn(
                    ExprDotMethod(eventSvc, method, Cast(namedWindowAsType.UnderlyingType, ExprDotUnderlying(@Ref("event"))), namedWindowType));
            return LocalMethod(methodNode);
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public Type EvaluationType {
            get => returnType;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => this;
        }

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(this.GetType().Name);
        }
    }
} // end of namespace