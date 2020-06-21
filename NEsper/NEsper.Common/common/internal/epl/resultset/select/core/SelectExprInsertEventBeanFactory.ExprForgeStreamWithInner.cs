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
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public partial class SelectExprInsertEventBeanFactory
    {
        public class ExprForgeStreamWithInner : ExprForge,
            ExprEvaluator,
            ExprNodeRenderable
        {
            private readonly Type componentReturnType;

            private readonly ExprForge inner;

            public ExprForgeStreamWithInner(
                ExprForge inner,
                Type componentReturnType)
            {
                this.inner = inner;
                this.componentReturnType = componentReturnType;
            }

            public object Evaluate(
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
            }

            public ExprEvaluator ExprEvaluator => this;

            public CodegenExpression EvaluateCodegen(
                Type requiredType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var arrayType = TypeHelper.GetArrayType(componentReturnType);
                var methodNode = codegenMethodScope.MakeChild(arrayType, GetType(), codegenClassScope);

                methodNode.Block
                    .DeclareVar<EventBean[]>(
                        "events",
                        CodegenExpressionBuilder.Cast(
                            typeof(EventBean[]),
                            inner.EvaluateCodegen(requiredType, methodNode, exprSymbol, codegenClassScope)))
                    .IfRefNullReturnNull("events")
                    .DeclareVar(
                        arrayType,
                        "values",
                        CodegenExpressionBuilder.NewArrayByLength(
                            componentReturnType,
                            CodegenExpressionBuilder.ArrayLength(CodegenExpressionBuilder.Ref("events"))))
                    .ForLoopIntSimple("i", CodegenExpressionBuilder.ArrayLength(CodegenExpressionBuilder.Ref("events")))
                    .AssignArrayElement(
                        "values",
                        CodegenExpressionBuilder.Ref("i"),
                        CodegenExpressionBuilder.Cast(
                            componentReturnType,
                            CodegenExpressionBuilder.ExprDotUnderlying(
                                CodegenExpressionBuilder.ArrayAtIndex(
                                    CodegenExpressionBuilder.Ref("events"),
                                    CodegenExpressionBuilder.Ref("i")))))
                    .BlockEnd()
                    .MethodReturn(CodegenExpressionBuilder.Ref("values"));
                return CodegenExpressionBuilder.LocalMethod(methodNode);
            }

            public Type EvaluationType => TypeHelper.GetArrayType(componentReturnType);

            public ExprNodeRenderable ExprForgeRenderable => this;

            public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

            public void ToEPL(TextWriter writer,
                ExprPrecedenceEnum parentPrecedence,
                ExprNodeRenderableFlags flags)
            {
                writer.Write(typeof(ExprForgeStreamWithInner).GetSimpleName());
            }
        }
    }
}