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
using com.espertech.esper.common.@internal.rettype;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotForgeArrayGetEval : ExprDotEval
    {
        private readonly ExprDotForgeArrayGet forge;
        private readonly ExprEvaluator indexExpression;

        public ExprDotForgeArrayGetEval(
            ExprDotForgeArrayGet forge,
            ExprEvaluator indexExpression)
        {
            this.forge = forge;
            this.indexExpression = indexExpression;
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target == null) {
                return null;
            }

            var index = indexExpression.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (index == null) {
                return null;
            }

            if (!(index is int)) {
                return null;
            }

            var indexNum = (int) index;
            var targetArray = (Array) target;
            if (targetArray.Length <= indexNum) {
                return null;
            }

            return targetArray.GetValue(indexNum);
        }

        public EPType TypeInfo {
            get => forge.TypeInfo;
        }

        public ExprDotForge DotForge {
            get => forge;
        }

        public static CodegenExpression Codegen(
            ExprDotForgeArrayGet forge,
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                EPTypeHelper.GetNormalizedClass(forge.TypeInfo), typeof(ExprDotForgeArrayGetEval), codegenClassScope).AddParam(innerType, "target");

            CodegenBlock block = methodNode.Block;
            if (!innerType.IsPrimitive) {
                block.IfRefNullReturnNull("target");
            }

            block.DeclareVar(typeof(int), "index", forge.IndexExpression.EvaluateCodegen(typeof(int), methodNode, exprSymbol, codegenClassScope));
            block.IfCondition(Relational(ArrayLength(@Ref("target")), LE, @Ref("index")))
                .BlockReturn(ConstantNull())
                .MethodReturn(ArrayAtIndex(@Ref("target"), @Ref("index")));
            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace