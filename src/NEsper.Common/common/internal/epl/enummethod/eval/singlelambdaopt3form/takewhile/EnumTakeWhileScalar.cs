///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.takewhile
{
    public class EnumTakeWhileScalar : ThreeFormScalar
    {
        private CodegenExpression _innerValue;

        public EnumTakeWhileScalar(
            ExprDotEvalParamLambda lambda,
            ObjectArrayEventType fieldEventType,
            int numParameters) : base(lambda, fieldEventType, numParameters)
        {
        }

        public override EnumEval EnumEvaluator {
            get {
                var inner = InnerExpression.ExprEvaluator;

                return new ProxyEnumEval(
                    (
                        eventsLambda,
                        enumcoll,
                        isNewData,
                        context) => {
                        if (enumcoll.IsEmpty()) {
                            return enumcoll;
                        }

                        var evalEvent = new ObjectArrayEventBean(new object[3], fieldEventType);
                        eventsLambda[StreamNumLambda] = evalEvent;
                        var props = evalEvent.Properties;
                        props[2] = enumcoll.Count;

                        if (enumcoll.Count == 1) {
                            var item = enumcoll.First();
                            props[0] = item;
                            props[1] = 0;
                            var pass = inner.Evaluate(eventsLambda, isNewData, context);
                            if (pass == null || false.Equals(pass)) {
                                return FlexCollection.Empty;
                            }

                            return FlexCollection.OfObject(item);
                        }

                        var result = new ArrayDeque<object>();
                        var count = -1;

                        foreach (var next in enumcoll) {
                            count++;
                            props[0] = next;
                            props[1] = count;
                            var pass = inner.Evaluate(eventsLambda, isNewData, context);
                            if (pass == null || false.Equals(pass)) {
                                break;
                            }

                            result.Add(next);
                        }

                        return FlexCollection.Of(result);
                    });
            }
        }

        public override Type ReturnTypeOfMethod()
        {
            return typeof(ICollection<object>);
        }

        public override CodegenExpression ReturnIfEmptyOptional()
        {
            //return EnumForgeCodegenNames.REF_ENUMCOLL;
            return EnumValue(typeof(EmptyList<object>), "Instance");
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope)
        {
            _innerValue = InnerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope);
            EnumTakeWhileHelper.InitBlockSizeOneScalar(
                numParameters,
                block,
                _innerValue,
                InnerExpression.EvaluationType);
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope)
        {
            CodegenLegoBooleanExpression.CodegenBreakIfNotNullAndNotPass(
                block,
                InnerExpression.EvaluationType,
                _innerValue);
            block.Expression(ExprDotMethod(Ref("result"), "Add", Ref("next")));
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace