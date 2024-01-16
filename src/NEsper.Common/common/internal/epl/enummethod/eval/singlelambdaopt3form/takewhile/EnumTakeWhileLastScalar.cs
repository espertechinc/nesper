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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational; // GE
using static
    com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.takewhile.
    EnumTakeWhileHelper; // takeWhileLastScalarToArray

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.takewhile {
    public class EnumTakeWhileLastScalar : ThreeFormScalar {
        private CodegenExpression innerValue;

        public EnumTakeWhileLastScalar(
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
                                return EmptyList<object>.Instance;
                            }

                            return Collections.List<object>(item);
                        }

                        var all = TakeWhileLastScalarToArray(enumcoll);
                        var result = new ArrayDeque<object>();
                        var count = -1;

                        for (var i = all.Length - 1; i >= 0; i--) {
                            props[0] = all[i];
                            count++;
                            props[1] = count;

                            var pass = inner.Evaluate(eventsLambda, isNewData, context);
                            if (pass == null || false.Equals(pass)) {
                                break;
                            }

                            result.AddFirst(all[i]);
                        }

                        return result;
                    });
            }
        }

        public override Type ReturnTypeOfMethod(Type inputCollectionType)
        {
            return inputCollectionType;
        }

        public override CodegenExpression ReturnIfEmptyOptional(Type inputCollectionType)
        {
            //return EnumForgeCodegenNames.REF_ENUMCOLL;
            var componentType = inputCollectionType.GetComponentType();
            return EnumValue(typeof(EmptyList<>).MakeGenericType(componentType), "Instance");
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope,
            Type inputCollectionType)
        {
            var itemType = inputCollectionType.GetComponentType();

            innerValue = InnerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope);

            InitBlockSizeOneScalar(
                numParameters,
                block,
                innerValue,
                InnerExpression.EvaluationType,
                inputCollectionType.GetComponentType());

            block.DeclareVar(
                itemType.MakeArrayType(1),
                "all",
                StaticMethod(
                    //typeof(EnumTakeWhileHelper), "TakeWhileLastScalarToArray",
                    typeof(Enumerable), "ToArray",
                    EnumForgeCodegenNames.REF_ENUMCOLL));

            var forEach = block.ForLoop(
                    typeof(int),
                    "i",
                    Op(ArrayLength(Ref("all")), "-", Constant(1)),
                    Relational(Ref("i"), GE, Constant(0)),
                    DecrementRef("i"))
                .AssignArrayElement("props", Constant(0), ArrayAtIndex(Ref("all"), Ref("i")));
            if (numParameters >= 2) {
                forEach.IncrementRef("count")
                    .AssignArrayElement("props", Constant(1), Ref("count"));
            }

            CodegenLegoBooleanExpression.CodegenBreakIfNotNullAndNotPass(
                forEach,
                InnerExpression.EvaluationType,
                innerValue);
            forEach.Expression(ExprDotMethod(Ref("result"), "AddFirst", ArrayAtIndex(Ref("all"), Ref("i"))));
        }

        public override bool HasForEachLoop()
        {
            return false;
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope, Type inputCollectionType)
        {
            throw new IllegalStateException();
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace