using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.resultset.@select.eval
{
    public partial class SelectEvalStreamWUndRecastMapFactory
    {
        internal class MapInsertProcessorSimpleRepackage : SelectExprProcessorForge
        {
            private readonly SelectExprForgeContext selectExprForgeContext;
            private readonly int underlyingStreamNumber;

            internal MapInsertProcessorSimpleRepackage(
                SelectExprForgeContext selectExprForgeContext,
                int underlyingStreamNumber,
                EventType resultType)
            {
                this.selectExprForgeContext = selectExprForgeContext;
                this.underlyingStreamNumber = underlyingStreamNumber;
                ResultEventType = resultType;
            }

            public EventType ResultEventType { get; }

            public CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType,
                CodegenExpression eventBeanFactory,
                CodegenMethodScope codegenMethodScope,
                SelectExprProcessorCodegenSymbol selectSymbol,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                var refEPS = exprSymbol.GetAddEps(methodNode);
                var value = CodegenExpressionBuilder.ExprDotName(
                    CodegenExpressionBuilder.Cast(typeof(MappedEventBean), CodegenExpressionBuilder.ArrayAtIndex(refEPS, CodegenExpressionBuilder.Constant(underlyingStreamNumber))),
                    "Properties");
                methodNode.Block.MethodReturn(
                    CodegenExpressionBuilder.ExprDotMethod(eventBeanFactory, "AdapterForTypedMap", value, resultEventType));
                return methodNode;
            }
        }
    }
}