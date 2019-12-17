///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumMinMaxScalarLambdaForgeEval : EnumEval
    {
        private readonly EnumMinMaxScalarLambdaForge _forge;
        private readonly ExprEvaluator _innerExpression;

        public EnumMinMaxScalarLambdaForgeEval(
            EnumMinMaxScalarLambdaForge forge,
            ExprEvaluator innerExpression)
        {
            _forge = forge;
            _innerExpression = innerExpression;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            IComparable minKey = null;

            var resultEvent = new ObjectArrayEventBean(new object[1], _forge.resultEventType);
            eventsLambda[_forge.StreamNumLambda] = resultEvent;
            var props = resultEvent.Properties;

            var coll = (ICollection<object>) enumcoll;
            foreach (var next in coll) {
                props[0] = next;

                var comparable = _innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (comparable == null) {
                    continue;
                }

                if (minKey == null) {
                    minKey = (IComparable) comparable;
                }
                else {
                    if (_forge.max) {
                        if (minKey.CompareTo(comparable) < 0) {
                            minKey = (IComparable) comparable;
                        }
                    }
                    else {
                        if (minKey.CompareTo(comparable) > 0) {
                            minKey = (IComparable) comparable;
                        }
                    }
                }
            }

            return minKey;
        }

        public static CodegenExpression Codegen(
            EnumMinMaxScalarLambdaForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var resultTypeMember = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(forge.resultEventType, EPStatementInitServicesConstants.REF)));
            var innerType = forge.InnerExpression.EvaluationType;
            var innerTypeBoxed = Boxing.GetBoxedType(innerType);

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(innerTypeBoxed, typeof(EnumMinMaxScalarLambdaForgeEval), scope, codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            var block = methodNode.Block
                .DeclareVar(innerTypeBoxed, "minKey", ConstantNull())
                .DeclareVar<ObjectArrayEventBean>(
                    "resultEvent",
                    NewInstance<ObjectArrayEventBean>(NewArrayByLength(typeof(object), Constant(1)), resultTypeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.StreamNumLambda), Ref("resultEvent"))
                .DeclareVar<object[]>("props", ExprDotName(Ref("resultEvent"), "Properties"));

            var forEach = block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), Ref("next"))
                .DeclareVar(
                    innerTypeBoxed,
                    "value",
                    forge.InnerExpression.EvaluateCodegen(innerTypeBoxed, methodNode, scope, codegenClassScope));
            if (innerType.CanBeNull()) {
                forEach.IfRefNull("value").BlockContinue();
            }

            forEach.IfCondition(EqualsNull(Ref("minKey")))
                .AssignRef("minKey", Ref("value"))
                .IfElse()
                .IfCondition(
                    Relational(
                        ExprDotMethod(Unbox(Ref("minKey"), innerTypeBoxed), "CompareTo", Ref("value")),
                        forge.max ? LT : GT,
                        Constant(0)))
                .AssignRef("minKey", Ref("value"));

            block.MethodReturn(Ref("minKey"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace