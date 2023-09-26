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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.arrayOf
{
    public class EnumArrayOfEventPlus : ThreeFormEventPlus
    {
        private readonly Type _arrayComponentType;

        public EnumArrayOfEventPlus(
            ExprDotEvalParamLambda lambda,
            ObjectArrayEventType indexEventType,
            int numParameters,
            Type arrayComponentType)
            : base(lambda, indexEventType, numParameters)
        {
            _arrayComponentType = arrayComponentType;
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
                        var array = Arrays.CreateInstanceChecked(_arrayComponentType, enumcoll.Count);
                        if (enumcoll.IsEmpty()) {
                            return array;
                        }

                        var indexEvent = new ObjectArrayEventBean(new object[2], FieldEventType);
                        var props = indexEvent.Properties;
                        props[1] = enumcoll.Count;
                        eventsLambda[StreamNumLambda + 1] = indexEvent;
                        var beans = (ICollection<EventBean>)enumcoll;
                        var count = -1;

                        foreach (var next in beans) {
                            count++;
                            props[0] = count;
                            eventsLambda[StreamNumLambda] = next;
                            var item = inner.Evaluate(eventsLambda, isNewData, context);
                            array.SetValue(item, count);
                        }

                        return array;
                    });
            }
        }

        public override Type ReturnTypeOfMethod()
        {
            return TypeHelper.GetArrayType(_arrayComponentType);
        }

        public override CodegenExpression ReturnIfEmptyOptional()
        {
            return NewArrayByLength(_arrayComponentType, Constant(0));
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope)
        {
            var arrayType = ReturnTypeOfMethod();
            block.DeclareVar(
                arrayType,
                "result",
                NewArrayByLength(_arrayComponentType, ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count")));
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope)
        {
            block
                .DeclareVar<object>(
                    "item",
                    InnerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
                .AssignArrayElement(Ref("result"), Ref("count"), FlexCast(_arrayComponentType, Ref("item")));
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace