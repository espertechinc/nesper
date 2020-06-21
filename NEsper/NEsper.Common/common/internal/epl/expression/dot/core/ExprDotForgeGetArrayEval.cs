///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class ExprDotForgeGetArrayEval : ExprDotEval
    {
        private readonly ExprDotForgeGetArray forge;
        private readonly ExprEvaluator indexExpression;

        public ExprDotForgeGetArrayEval(
            ExprDotForgeGetArray forge,
            ExprEvaluator indexExpression)
        {
            this.forge = forge;
            this.indexExpression = indexExpression;
        }

        public EPType TypeInfo => forge.TypeInfo;

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var array = target as Array;
            if (array == null) {
                return null;
            }

            var index = indexExpression.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (index == null) {
                return null;
            }

            if (!(index is int indexNum)) {
                return null;
            }

            if (array.Length <= indexNum) {
                return null;
            }

            return array.GetValue(indexNum);
        }

        public ExprDotForge DotForge => forge;

        public static CodegenExpression Codegen(
            ExprDotForgeGetArray forge,
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(forge.TypeInfo.GetNormalizedClass(), typeof(ExprDotForgeGetArrayEval), codegenClassScope)
                .AddParam(innerType, "target");

            var block = methodNode.Block;
            if (!innerType.IsPrimitive) {
                block.IfRefNullReturnNull("target");
            }

            var targetType = forge.TypeInfo.GetCodegenReturnType();
            block.DeclareVar(typeof(int), "index", forge.IndexExpression.EvaluateCodegen(typeof(int), methodNode, exprSymbol, codegenClassScope))
                .IfCondition(Relational(ArrayLength(Ref("target")), LE, Ref("index")))
                .BlockReturn(ConstantNull())
                .MethodReturn(CodegenLegoCast.CastSafeFromObjectType(targetType, ArrayAtIndex(Ref("target"), Ref("index"))));
            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace