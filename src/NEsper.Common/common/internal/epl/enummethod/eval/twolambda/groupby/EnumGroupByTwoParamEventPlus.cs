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
using com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.groupby
{
    public class EnumGroupByTwoParamEventPlus : TwoLambdaThreeFormEventPlus
    {
        public EnumGroupByTwoParamEventPlus(
            ExprForge innerExpression,
            int streamNumLambda,
            ObjectArrayEventType indexEventType,
            ExprForge secondExpression,
            int numParameters) : base(innerExpression, streamNumLambda, indexEventType, secondExpression, numParameters)
        {
        }

        public override EnumEval EnumEvaluator {
            get {
                var first = InnerExpression.ExprEvaluator;
                var second = SecondExpression.ExprEvaluator;
                return new ProxyEnumEval(
                    (
                        eventsLambda,
                        enumcoll,
                        isNewData,
                        context) => {
                        if (enumcoll.IsEmpty()) {
                            return EmptyDictionary<object, object>.Instance;
                        }

                        IDictionary<object, object> result = new LinkedHashMap<object, object>();
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

                            var key = first.Evaluate(eventsLambda, isNewData, context);
                            var entry = second.Evaluate(eventsLambda, isNewData, context);

                            var value = (ICollection<object>)result.Get(key);
                            if (value == null) {
                                value = new List<object>();
                                result.Put(key, value);
                            }

                            value.Add(entry);
                        }

                        return result;
                    });
            }
        }

        public override Type ReturnType()
        {
            return typeof(IDictionary<object, object>);
        }

        public override CodegenExpression ReturnIfEmptyOptional()
        {
            return EnumValue(typeof(EmptyDictionary<object, object>), "Instance");
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope)
        {
            block.DeclareVar<IDictionary<object, object>>(
                "result",
                NewInstance(typeof(NullableDictionary<object, object>)));
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope)
        {
            block.DeclareVar<object>(
                    "key",
                    InnerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
                .DeclareVar<object>(
                    "entry",
                    SecondExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
                .DeclareVar<ICollection<object>>(
                    "value",
                    Cast(typeof(ICollection<object>), ExprDotMethod(Ref("result"), "Get", Ref("key"))))
                .IfRefNull("value")
                .AssignRef("value", NewInstance(typeof(List<object>)))
                .Expression(ExprDotMethod(Ref("result"), "Put", Ref("key"), Ref("value")))
                .BlockEnd()
                .Expression(ExprDotMethod(Ref("value"), "Add", Ref("entry")));
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace