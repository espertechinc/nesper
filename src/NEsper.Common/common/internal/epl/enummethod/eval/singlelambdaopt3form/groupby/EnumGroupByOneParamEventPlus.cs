///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.groupby {
    public class EnumGroupByOneParamEventPlus : ThreeFormEventPlus {
        public EnumGroupByOneParamEventPlus(
            ExprDotEvalParamLambda lambda,
            ObjectArrayEventType indexEventType,
            int numParameters) : base(lambda, indexEventType, numParameters)
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
                            return EmptyDictionary<object, ICollection<object>>.Instance;
                        }

                        var beans = enumcoll.Unwrap<EventBean>();
                        var indexEvent = new ObjectArrayEventBean(new object[2], FieldEventType);
                        var props = indexEvent.Properties;
                        props[1] = enumcoll.Count;
                        eventsLambda[StreamNumLambda + 1] = indexEvent;

                        var result = new HashMap<object, ICollection<object>>();

                        var count = -1;
                        foreach (var next in beans) {
                            count++;
                            props[0] = count;
                            eventsLambda[StreamNumLambda] = next;

                            var key = inner.Evaluate(eventsLambda, isNewData, context);

                            var value = result.Get(key);
                            if (value == null) {
                                value = new List<object>();
                                result.Put(key, value);
                            }

                            value.Add(next.Underlying);
                        }

                        return result;
                    }
                };
            }
        }

        public Type KeyType => InnerExpression.EvaluationType ?? typeof(object);


        public override Type ReturnTypeOfMethod(Type desiredReturnType)
        {
            var valType = GenericExtensions.GetDictionaryValueType(desiredReturnType);
            return typeof(IDictionary<,>).MakeGenericType(KeyType, valType);
        }

        public override CodegenExpression ReturnIfEmptyOptional(Type desiredReturnType)
        {
            var valType = GenericExtensions.GetDictionaryValueType(desiredReturnType);
            return EnumValue(typeof(EmptyDictionary<,>).MakeGenericType(KeyType, valType), "Instance");
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope,
            Type desiredReturnType)
        {
            var valType = GenericExtensions.GetDictionaryValueType(desiredReturnType);
            var dictType = typeof(IDictionary<,>).MakeGenericType(KeyType, valType);
            var nullType = typeof(NullableDictionary<,>).MakeGenericType(KeyType, valType);

            block.DeclareVar(dictType, "result", NewInstance(nullType));
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope,
            Type desiredReturnType)
        {
            var valType = GenericExtensions.GetDictionaryValueType(desiredReturnType);
            var itemType = GenericExtensions.GetComponentType(valType);
            var listType = typeof(List<>).MakeGenericType(itemType);

            block
                .DeclareVar(KeyType, "key",
                    InnerExpression.EvaluateCodegen(KeyType, methodNode, scope, codegenClassScope))
                .DeclareVar(valType, "value", ExprDotMethod(Ref("result"), "Get", Ref("key")))
                .IfRefNull("value")
                .AssignRef("value", NewInstance(listType))
                .Expression(ExprDotMethod(Ref("result"), "Put", Ref("key"), Ref("value")))
                .BlockEnd()
                .Expression(ExprDotMethod(Ref("value"), "Add", Cast(itemType, ExprDotUnderlying(Ref("next")))));
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace