///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.where
{
    public class EnumWhereEventPlus : ThreeFormEventPlus
    {
        public EnumWhereEventPlus(
            ExprDotEvalParamLambda lambda,
            ObjectArrayEventType indexEventType,
            int numParameters) : base(lambda, indexEventType, numParameters)
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
                            //return enumcoll;
                            return EmptyList<EventBean>.Instance;
                        }

                        var beans = (ICollection<EventBean>)enumcoll;
                        var result = new ArrayDeque<EventBean>();
                        var indexEvent = new ObjectArrayEventBean(new object[2], FieldEventType);
                        eventsLambda[StreamNumLambda + 1] = indexEvent;
                        var props = indexEvent.Properties;
                        props[1] = enumcoll.Count;

                        var count = -1;
                        foreach (var next in beans) {
                            count++;
                            props[0] = count;
                            eventsLambda[StreamNumLambda] = next;

                            var pass = inner.Evaluate(eventsLambda, isNewData, context);
                            if (pass == null || false.Equals(pass)) {
                                continue;
                            }

                            result.Add(next);
                        }

                        return result;
                    });
            }
        }

        public override Type ReturnTypeOfMethod()
        {
            return typeof(ICollection<EventBean>);
        }

        public override CodegenExpression ReturnIfEmptyOptional()
        {
            //return EnumForgeCodegenNames.REF_ENUMCOLL;
            return EnumValue(typeof(EmptyList<EventBean>), "Instance");
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope)
        {
            block.DeclareVar<ArrayDeque<EventBean>>("result", NewInstance(typeof(ArrayDeque<EventBean>)));
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
            block.Expression(ExprDotMethod(Ref("result"), "Add", Ref("next")));
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace