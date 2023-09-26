using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    public partial class ExprDeclaredNodeImpl
    {
        private class DeclaredNodeEventPropertyGetterForge : ExprEventEvaluatorForge
        {
            private readonly ExprForge exprForge;

            public DeclaredNodeEventPropertyGetterForge(ExprForge exprForge)
            {
                this.exprForge = exprForge;
            }

            public CodegenExpression EventBeanWithCtxGet(
                CodegenExpression beanExpression,
                CodegenExpression ctxExpression,
                CodegenMethodScope parent,
                CodegenClassScope classScope)
            {
                if (exprForge.EvaluationType == null || exprForge.EvaluationType == null) {
                    return CodegenExpressionBuilder.ConstantNull();
                }

                var method = parent.MakeChild(exprForge.EvaluationType, GetType(), classScope)
                    .AddParam<EventBean>("bean");
                var exprMethod = CodegenLegoMethodExpression.CodegenExpression(exprForge, method, classScope);

                method.Block
                    .DeclareVar<EventBean[]>(
                        "events",
                        CodegenExpressionBuilder.NewArrayByLength(
                            typeof(EventBean),
                            CodegenExpressionBuilder.Constant(1)))
                    .AssignArrayElement(
                        CodegenExpressionBuilder.Ref("events"),
                        CodegenExpressionBuilder.Constant(0),
                        CodegenExpressionBuilder.Ref("bean"))
                    .MethodReturn(
                        CodegenExpressionBuilder.LocalMethod(
                            exprMethod,
                            CodegenExpressionBuilder.Ref("events"),
                            CodegenExpressionBuilder.ConstantTrue(),
                            CodegenExpressionBuilder.ConstantNull()));

                return CodegenExpressionBuilder.LocalMethod(method, beanExpression);
            }
        }
    }
}