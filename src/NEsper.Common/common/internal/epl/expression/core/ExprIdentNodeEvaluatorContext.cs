///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprIdentNodeEvaluatorContext : ExprIdentNodeEvaluator
    {
        private readonly int _streamNum;
        private readonly Type _resultType;
        private readonly EventPropertyGetterSPI _getter;
        private readonly EventTypeSPI _eventType;

        public ExprIdentNodeEvaluatorContext(
            int streamNum,
            Type resultType,
            EventPropertyGetterSPI getter,
            EventTypeSPI eventType)
        {
            _streamNum = streamNum;
            _resultType = resultType;
            _getter = getter;
            _eventType = eventType;
        }

        public bool EvaluatePropertyExists(
            EventBean[] eventsPerStream,
            bool isNewData)
        {
            return true;
        }

        public int StreamNum => _streamNum;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (context.ContextProperties != null) {
                return _getter.Get(context.ContextProperties);
            }

            return null;
        }

        public CodegenExpression Codegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (_resultType == null) {
                return ConstantNull();
            }

            var methodNode = codegenMethodScope.MakeChild(_resultType, GetType(), codegenClassScope);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);

            methodNode.Block
                .IfCondition(NotEqualsNull(refExprEvalCtx))
                .BlockReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        _resultType,
                        _getter.EventBeanGetCodegen(
                            ExprDotName(refExprEvalCtx, "ContextProperties"),
                            methodNode,
                            codegenClassScope)))
                .MethodReturn(ConstantNull());
            return LocalMethod(methodNode);
        }

        public Type EvaluationType => _resultType;

        public EventPropertyGetterSPI Getter => _getter;

        public bool IsContextEvaluated => true;

        public bool OptionalEvent {
            get => throw new NotImplementedException();
            set { }
        }

        public EventTypeSPI EventType => _eventType;
    }
} // end of namespace