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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumAggregateScalarForgeEval : EnumEval
    {
        private readonly EnumAggregateScalarForge _forge;
        private readonly ExprEvaluator _initialization;
        private readonly ExprEvaluator _innerExpression;

        public EnumAggregateScalarForgeEval(
            EnumAggregateScalarForge forge,
            ExprEvaluator initialization,
            ExprEvaluator innerExpression)
        {
            _forge = forge;
            _initialization = initialization;
            _innerExpression = innerExpression;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var value = _initialization.Evaluate(eventsLambda, isNewData, context);

            if (enumcoll.IsEmpty()) {
                return value;
            }

            var resultEvent = new ObjectArrayEventBean(new object[1], _forge.ResultEventType);
            var evalEvent = new ObjectArrayEventBean(new object[1], _forge.EvalEventType);
            eventsLambda[_forge.StreamNumLambda] = resultEvent;
            eventsLambda[_forge.StreamNumLambda + 1] = evalEvent;
            var resultProps = resultEvent.Properties;
            var evalProps = evalEvent.Properties;

            foreach (var next in enumcoll) {
                resultProps[0] = value;
                evalProps[0] = next;
                value = _innerExpression.Evaluate(eventsLambda, isNewData, context);
            }

            return value;
        }

        public static CodegenExpression Codegen(
            EnumAggregateScalarForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var resultTypeMember = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(forge.ResultEventType, EPStatementInitServicesConstants.REF)));
            var evalTypeMember = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(forge.EvalEventType, EPStatementInitServicesConstants.REF)));

            var paramTypes = EnumForgeCodegenNames.PARAMS;

            var initializationEvalType = forge.Initialization.EvaluationType.GetBoxedType();
            var innerEvalType = forge.Initialization.EvaluationType;

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    initializationEvalType,
                    typeof(EnumAggregateScalarForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(paramTypes);

            var block = methodNode.Block;
            
            block.DeclareVar(
                    initializationEvalType,
                    "value",
                    forge.Initialization.EvaluateCodegen(
                        initializationEvalType,
                        methodNode,
                        scope,
                        codegenClassScope))
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(Ref("value"));
            
            block.DeclareVar<ObjectArrayEventBean>(
                    "resultEvent",
                    NewInstance<ObjectArrayEventBean>(
                        NewArrayByLength(typeof(object), Constant(1)),
                        resultTypeMember))
                .DeclareVar<ObjectArrayEventBean>(
                    "evalEvent",
                    NewInstance<ObjectArrayEventBean>(
                        NewArrayByLength(typeof(object), Constant(1)),
                        evalTypeMember))
                .AssignArrayElement(
                    EnumForgeCodegenNames.REF_EPS,
                    Constant(forge.StreamNumLambda),
                    Ref("resultEvent"))
                .AssignArrayElement(
                    EnumForgeCodegenNames.REF_EPS,
                    Constant(forge.StreamNumLambda + 1),
                    Ref("evalEvent"))
                .DeclareVar<object[]>(
                    "resultProps",
                    ExprDotName(Ref("resultEvent"), "Properties"))
                .DeclareVar<object[]>(
                    "evalProps",
                    ExprDotName(Ref("evalEvent"), "Properties"));

            block.ForEach(
                    typeof(object),
                    "next",
                    EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(
                    "resultProps",
                    Constant(0),
                    Ref("value"))
                .AssignArrayElement(
                    "evalProps",
                    Constant(0),
                    Ref("next"))
                .AssignRef(
                    "value",
                    forge.InnerExpression.EvaluateCodegen(
                        innerEvalType,
                        methodNode,
                        scope,
                        codegenClassScope))
                .BlockEnd();
            
            block.MethodReturn(Ref("value"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace