///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.arrayOf {
    public class EnumArrayOfScalar : ThreeFormScalar {
        private readonly Type arrayComponentType;

        public EnumArrayOfScalar(
            ExprDotEvalParamLambda lambda,
            ObjectArrayEventType fieldEventType,
            int numParameters,
            Type arrayComponentType)
            : base(lambda, fieldEventType, numParameters)
        {
            this.arrayComponentType = arrayComponentType;
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
                        var length = enumcoll.Count;
                        var array = Arrays.CreateInstanceChecked(arrayComponentType, length);
                        if (enumcoll.IsEmpty()) {
                            return array;
                        }

                        var evalEvent = new ObjectArrayEventBean(new object[3], fieldEventType);
                        eventsLambda[StreamNumLambda] = evalEvent;
                        var evalProps = evalEvent.Properties;
                        evalProps[2] = enumcoll.Count;
                        var count = -1;

                        foreach (var next in enumcoll) {
                            count++;
                            evalProps[0] = next;
                            evalProps[1] = count;
                            var item = inner.Evaluate(eventsLambda, isNewData, context);
                            array.SetValue(item, count);
                        }

                        return array;
                    });
            }
        }

        public override Type ReturnTypeOfMethod(Type desiredReturnType)
        {
            return TypeHelper.GetArrayType(arrayComponentType);
        }

        public override CodegenExpression ReturnIfEmptyOptional(Type desiredReturnType)
        {
            return NewArrayByLength(arrayComponentType, Constant(0));
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope,
            Type desiredReturnType)
        {
            var arrayType = ReturnTypeOfMethod(typeof(ICollection<object>));
            block
                .DeclareVar(
                    arrayType,
                    "result",
                    NewArrayByLength(arrayComponentType, ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count")))
                .DeclareVar<int>("index", Constant(0));
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope, Type desiredReturnType)
        {
            block
                .DeclareVar<object>(
                    "item",
                    InnerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
                .AssignArrayElement(Ref("result"), Ref("index"), Cast(arrayComponentType, Ref("item")))
                .IncrementRef("index");
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace