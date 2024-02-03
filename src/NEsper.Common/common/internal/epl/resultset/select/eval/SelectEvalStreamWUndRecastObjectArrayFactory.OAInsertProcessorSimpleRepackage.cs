using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.arr;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public partial class SelectEvalStreamWUndRecastObjectArrayFactory
    {
        public class OAInsertProcessorSimpleRepackage : SelectExprProcessorForge
        {
            private readonly EventType resultType;
            private readonly SelectExprForgeContext selectExprForgeContext;
            private readonly int underlyingStreamNumber;

            internal OAInsertProcessorSimpleRepackage(
                SelectExprForgeContext selectExprForgeContext,
                int underlyingStreamNumber,
                EventType resultType)
            {
                this.selectExprForgeContext = selectExprForgeContext;
                this.underlyingStreamNumber = underlyingStreamNumber;
                this.resultType = resultType;
            }

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
                var value = ExprDotName(
                    Cast(typeof(ObjectArrayEventBean), ArrayAtIndex(refEPS, Constant(underlyingStreamNumber))),
                    "Properties");
                methodNode.Block.MethodReturn(
                    ExprDotMethod(
                        eventBeanFactory,
                        "AdapterForTypedObjectArray",
                        value,
                        resultEventType));
                return methodNode;
            }

            public EventType ResultEventType => resultType;
        }
    }
}