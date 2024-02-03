///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotForgeGetCollectionEval : ExprDotEval
    {
        private readonly ExprDotForgeGetCollection _forge;
        private readonly ExprEvaluator _indexExpression;

        public ExprDotForgeGetCollectionEval(
            ExprDotForgeGetCollection forge,
            ExprEvaluator indexExpression)
        {
            _forge = forge;
            _indexExpression = indexExpression;
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

            var index = _indexExpression.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (index is int indexNum) {
                return CollectionElementAt(target.AsObjectCollection(), indexNum);
            }

            return null;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="target">collection</param>
        /// <returns>frequence params</returns>
        public static T CollectionElementAt<T>(
            ICollection<T> target,
            int indexNum)
        {
            var collection = target;
            if (collection.Count <= indexNum) {
                return default;
            }

            if (collection is IList<T> list) {
                return list[indexNum];
            }

            return collection.Skip(indexNum).FirstOrDefault();
        }

        public EPChainableType TypeInfo => _forge.TypeInfo;

        public ExprDotForge DotForge => _forge;

        public static CodegenExpression Codegen(
            ExprDotForgeGetCollection forge,
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                    forge.TypeInfo.GetNormalizedType(),
                    typeof(ExprDotForgeGetCollectionEval),
                    codegenClassScope)
                .AddParam(innerType, "target");

            var block = methodNode.Block;
            if (!innerType.IsPrimitive) {
                block.IfRefNullReturnNull("target");
            }

            var targetType = forge.TypeInfo.GetCodegenReturnType();
            block.DeclareVar<int>(
                    "index",
                    forge.IndexExpression.EvaluateCodegen(typeof(int), methodNode, exprSymbol, codegenClassScope))
                .MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        targetType,
                        StaticMethod(
                            typeof(ExprDotForgeGetCollectionEval),
                            "CollectionElementAt",
                            Ref("target"),
                            Ref("index"))));
            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace