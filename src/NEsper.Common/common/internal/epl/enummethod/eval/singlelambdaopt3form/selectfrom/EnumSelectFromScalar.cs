///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.enummethod.codegen.EnumForgeCodegenNames; // REF_ENUMCOLL

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.selectfrom {
    public class EnumSelectFromScalar : ThreeFormScalar {
        public EnumSelectFromScalar(
            ExprDotEvalParamLambda lambda,
            ObjectArrayEventType resultEventType,
            int numParameters) : base(lambda, resultEventType, numParameters)
        {
        }

        public override EnumEval EnumEvaluator {
            get {
                var inner = InnerExpression.ExprEvaluator;
                return new ProxyEnumEval()
                {
                    ProcEvaluateEnumMethod = (
                        eventsLambda,
                        enumcoll,
                        isNewData,
                        context) => {
                        if (enumcoll.IsEmpty()) {
                            return enumcoll;
                        }

                        var result = new ArrayDeque<object>();
                        var evalEvent = new ObjectArrayEventBean(new object[3], fieldEventType);
                        eventsLambda[StreamNumLambda] = evalEvent;
                        var evalProps = evalEvent.Properties;

                        var count = -1;
                        evalProps[2] = enumcoll.Count;

                        foreach (var next in enumcoll) {
                            count++;
                            evalProps[0] = next;
                            evalProps[1] = count;

                            var value = inner.Evaluate(eventsLambda, isNewData, context);
                            if (value != null) {
                                result.Add(value);
                            }
                        }

                        return result;
                    }
                };
            }
        }

        public override Type ReturnTypeOfMethod(Type desiredReturnType)
        {
            return desiredReturnType;
        }

        public override CodegenExpression ReturnIfEmptyOptional(Type desiredReturnType)
        {
            var componentType = desiredReturnType.GetComponentType();
            return EnumValue(typeof(EmptyList<>).MakeGenericType(componentType), "Instance");
            //return REF_ENUMCOLL;
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope,
            Type desiredReturnType)
        {
            var itemType = desiredReturnType.GetComponentType();
            var arrayType = typeof(ArrayDeque<>).MakeGenericType(itemType);

            block.DeclareVar(arrayType, "result", NewInstance(arrayType, ExprDotName(REF_ENUMCOLL, "Count")));
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope,
            Type desiredReturnType)
        {
            var itemType = desiredReturnType.GetComponentType();

            block
                .DeclareVar(itemType, "item",
                    InnerExpression.EvaluateCodegen(itemType, methodNode, scope, codegenClassScope))
                .IfCondition(NotEqualsNull(Ref("item")))
                .Expression(ExprDotMethod(Ref("result"), "Add", Ref("item")));
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace