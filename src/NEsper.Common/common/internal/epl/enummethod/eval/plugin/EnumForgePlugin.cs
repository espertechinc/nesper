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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.enummethod;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.enummethod.codegen.EnumForgeCodegenNames; //REF_ENUMCOLL

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.plugin
{
    public class EnumForgePlugin : EnumForge
    {
        private readonly IList<ExprDotEvalParam> _bodiesAndParameters;
        private readonly EnumMethodModeStaticMethod _mode;
        private readonly Type _expectedStateReturnType;
        private readonly int _numStreamsIncoming;
        private readonly EventType _inputEventType;

        public EnumForgePlugin(
            IList<ExprDotEvalParam> bodiesAndParameters,
            EnumMethodModeStaticMethod mode,
            Type expectedStateReturnType,
            int numStreamsIncoming,
            EventType inputEventType)
        {
            _bodiesAndParameters = bodiesAndParameters;
            _mode = mode;
            _expectedStateReturnType = expectedStateReturnType;
            _numStreamsIncoming = numStreamsIncoming;
            _inputEventType = inputEventType;
        }

        public EnumEval EnumEvaluator =>
            throw new UnsupportedOperationException("Enum-evaluator not available at compile-time");

        public int StreamNumSize {
            get {
                var countLambda = 0;
                foreach (var param in _bodiesAndParameters) {
                    if (param is ExprDotEvalParamLambda lambda) {
                        countLambda += lambda.GoesToNames.Count;
                    }
                }

                return _numStreamsIncoming + countLambda;
            }
        }

        public CodegenExpression Codegen(
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(_expectedStateReturnType, typeof(EnumForgePlugin), scope, codegenClassScope)
                .AddParam(PARAMS);
            methodNode.Block.DeclareVar(_mode.StateClass, "state", NewInstance(_mode.StateClass));

            // call set-parameter for each non-lambda expression
            var indexNonLambda = 0;
            foreach (var param in _bodiesAndParameters) {
                if (param is ExprDotEvalParamExpr) {
                    var expression = param.BodyForge.EvaluateCodegen(
                        typeof(object),
                        methodNode,
                        scope,
                        codegenClassScope);
                    methodNode.Block.ExprDotMethod(Ref("state"), "SetParameter", Constant(indexNonLambda), expression);
                    indexNonLambda++;
                }
            }

            // allocate event type and field for each lambda expression
            var indexParameter = 0;
            foreach (var param in _bodiesAndParameters) {
                if (param is ExprDotEvalParamLambda lambda) {
                    for (var i = 0; i < lambda.LambdaDesc.Types.Length; i++) {
                        var eventType = lambda.LambdaDesc.Types[i];
                        var lambdaParameterType =
                            _mode.LambdaParameters.Invoke(new EnumMethodLambdaParameterDescriptor(indexParameter, i));

                        if (eventType != _inputEventType) {
                            var type = codegenClassScope.AddDefaultFieldUnshared(
                                true,
                                typeof(ObjectArrayEventType),
                                Cast(
                                    typeof(ObjectArrayEventType),
                                    EventTypeUtility.ResolveTypeCodegen(
                                        lambda.LambdaDesc.Types[i],
                                        EPStatementInitServicesConstants.REF)));
                            var eventName = GetNameExt("resultEvent", indexParameter, i);
                            var propName = GetNameExt("props", indexParameter, i);
                            methodNode.Block
                                .DeclareVar<ObjectArrayEventBean>(
                                    eventName,
                                    NewInstance(
                                        typeof(ObjectArrayEventBean),
                                        NewArrayByLength(typeof(object), Constant(1)),
                                        type))
                                .AssignArrayElement(REF_EPS, Constant(lambda.StreamCountIncoming + i), Ref(eventName))
                                .DeclareVar<object[]>(propName, ExprDotName(Ref(eventName), "Properties"));

                            // initialize index-type lambda-parameters to zer0
                            if (lambdaParameterType is EnumMethodLambdaParameterTypeIndex) {
                                methodNode.Block
                                    .AssignArrayElement(propName, Constant(0), Constant(0));
                            }

                            if (lambdaParameterType is EnumMethodLambdaParameterTypeSize) {
                                methodNode.Block
                                    .AssignArrayElement(propName, Constant(0), ExprDotName(REF_ENUMCOLL, "Count"));
                            }

                            if (lambdaParameterType is EnumMethodLambdaParameterTypeStateGetter getter) {
                                methodNode.Block
                                    .AssignArrayElement(
                                        propName,
                                        Constant(0),
                                        ExprDotName(Ref("state"), getter.PropertyName));
                            }
                        }
                    }
                }

                indexParameter++;
            }

            var elementType = _inputEventType == null ? typeof(object) : typeof(EventBean);
            methodNode.Block.DeclareVar<int>("count", Constant(-1));
            var forEach = methodNode.Block.ForEach(elementType, "next", REF_ENUMCOLL);
            {
                forEach.IncrementRef("count");

                IList<CodegenExpression> paramsNext = new List<CodegenExpression>();
                paramsNext.Add(Ref("state"));
                paramsNext.Add(Ref("next"));

                indexParameter = 0;
                foreach (var param in _bodiesAndParameters) {
                    if (param is ExprDotEvalParamLambda lambda) {
                        var valueName = "value_" + indexParameter;
                        for (var i = 0; i < lambda.LambdaDesc.Types.Length; i++) {
                            var lambdaParameterType = _mode.LambdaParameters.Invoke(
                                new EnumMethodLambdaParameterDescriptor(indexParameter, i));

                            var propName = GetNameExt("props", indexParameter, i);
                            if (lambdaParameterType is EnumMethodLambdaParameterTypeValue) {
                                var eventType = lambda.LambdaDesc.Types[i];
                                if (eventType == _inputEventType) {
                                    forEach.AssignArrayElement(
                                        REF_EPS,
                                        Constant(lambda.StreamCountIncoming + i),
                                        Ref("next"));
                                }
                                else {
                                    forEach.AssignArrayElement(propName, Constant(0), Ref("next"));
                                }
                            }
                            else if (lambdaParameterType is EnumMethodLambdaParameterTypeIndex) {
                                forEach.AssignArrayElement(propName, Constant(0), Ref("count"));
                            }
                            else if (lambdaParameterType is EnumMethodLambdaParameterTypeStateGetter getter) {
                                forEach.AssignArrayElement(
                                    propName,
                                    Constant(0),
                                    ExprDotName(Ref("state"), getter.PropertyName));
                            }
                            else if (lambdaParameterType is EnumMethodLambdaParameterTypeSize) {
                                // no action needed
                            }
                            else {
                                throw new UnsupportedOperationException(
                                    "Unrecognized lambda parameter type " + lambdaParameterType);
                            }
                        }

                        var forge = lambda.BodyForge;
                        var evalType = forge.EvaluationType;
                        if (evalType == null) {
                            forEach.DeclareVar<object>(valueName, ConstantNull());
                        }
                        else {
                            forEach.DeclareVar(
                                evalType,
                                valueName,
                                forge.EvaluateCodegen(evalType, methodNode, scope, codegenClassScope));
                        }

                        paramsNext.Add(Ref(valueName));
                    }

                    indexParameter++;
                }

                forEach.Expression(StaticMethod(_mode.ServiceClass, _mode.MethodName, paramsNext.ToArray()));

                if (_mode.IsEarlyExit) {
                    forEach.IfCondition(ExprDotName(Ref("state"), "IsCompleted")).BreakLoop();
                }
            }

            methodNode.Block.MethodReturn(
                FlexCast(_expectedStateReturnType, ExprDotName(Ref("state"), "State")));

            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }

        private string GetNameExt(
            string prefix,
            int indexLambda,
            int number)
        {
            return prefix + "_" + indexLambda + "_" + number;
        }
    }
} // end of namespace