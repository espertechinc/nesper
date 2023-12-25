///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.firstoflastof
{
    public class EnumFirstOfScalar : ThreeFormScalar
    {
        private readonly EPChainableType _columnType;

        public EnumFirstOfScalar(
            ExprDotEvalParamLambda lambda,
            ObjectArrayEventType fieldEventType,
            int numParameters,
            EPChainableType columnType) : base(lambda, fieldEventType, numParameters)
        {
            _columnType = columnType;
        }

        public override EnumEval EnumEvaluator {
            get {
                var inner = InnerExpression.ExprEvaluator;
                return new ProxyEnumEval() {
                    ProcEvaluateEnumMethod = (
                        eventsLambda,
                        enumcoll,
                        isNewData,
                        context) => {
                        var evalEvent = new ObjectArrayEventBean(new object[3], fieldEventType);
                        eventsLambda[StreamNumLambda] = evalEvent;
                        var props = evalEvent.Properties;
                        props[2] = enumcoll.Count;
                        var count = -1;

                        foreach (var next in enumcoll) {
                            count++;
                            props[0] = next;
                            props[1] = count;
                            var pass = inner.Evaluate(eventsLambda, isNewData, context);
                            if (pass == null || false.Equals(pass)) {
                                continue;
                            }

                            return next;
                        }

                        return null;
                    }
                };
            }
        }

        public override Type ReturnTypeOfMethod(Type inputCollectionType)
        {
            return _columnType.GetCodegenReturnType().GetBoxedType();
        }

        public override CodegenExpression ReturnIfEmptyOptional()
        {
            return ConstantNull();
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope,
            Type inputCollectionType)
        {
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope)
        {
            CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
                block,
                InnerExpression.EvaluationType,
                InnerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope));
            block.BlockReturn(Ref("next"));
            //block.BlockReturn(Cast(ReturnTypeOfMethod(TODO), Ref("next")));
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(ConstantNull());
        }
    }
} // end of namespace