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
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeUtilityCodegen
    {
        public static CodegenExpression CodegenExpressionMayCoerce(
            ExprForge forge, 
            Type targetType,
            CodegenMethod exprMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope classScope)
        {
            if (targetType.IsNullType()) {
                return ConstantNull();
            }
            
            var expr = forge.EvaluateCodegen(forge.EvaluationType, exprMethod, exprSymbol, classScope);
            return CodegenCoerce(expr, forge.EvaluationType, targetType, false);
        }

        public static CodegenExpression CodegenCoerce(
            CodegenExpression expression,
            Type exprType,
            Type targetType,
            bool alwaysCast)
        {
            if (targetType.IsNullTypeSafe() || exprType.IsNullTypeSafe()) {
                return expression;
            }

            var exprTypeBoxed = exprType.GetBoxedType();
            var targetTypeBoxed = targetType.GetBoxedType();
            
            if (exprTypeBoxed == targetTypeBoxed) {
                return alwaysCast ? Cast(targetType, expression) : expression;
            }

            var coercer = SimpleNumberCoercerFactory.GetCoercer(exprType, targetTypeBoxed);
            if (exprType.IsPrimitive || alwaysCast) {
                expression = Cast(exprTypeBoxed, expression);
            }

            return coercer.CoerceCodegen(expression, exprType);
        }

        public static CodegenExpression CodegenEvaluator(
            ExprForge forge,
            CodegenMethod method,
            Type originator,
            CodegenClassScope classScope)
        {
            var lambda = new CodegenExpressionLambda(method.Block)
                .WithParams(LAMBDA_PARAMS);

            var forgeEvaluationType = forge.EvaluationType;
            if (forgeEvaluationType.IsNullTypeSafe()) {
                lambda.Block.BlockReturn(ConstantNull());
                return NewInstance<ProxyExprEvaluator>(lambda);
            }
            else if (forgeEvaluationType.IsVoid()) {
                var evalMethod = CodegenLegoMethodExpression.CodegenExpression(forge, method, classScope);
                lambda.Block
                    .LocalMethod(
                        evalMethod,
                        LAMBDA_REF_EPS,
                        LAMBDA_REF_ISNEWDATA,
                        LAMBDA_REF_EXPREVALCONTEXT)
                    .BlockReturn(ConstantNull());
            }
            else {
                var evalMethod = CodegenLegoMethodExpression.CodegenExpression(forge, method, classScope);
                lambda.Block.BlockReturn(
                    LocalMethod(
                        evalMethod,
                        LAMBDA_REF_EPS,
                        LAMBDA_REF_ISNEWDATA,
                        LAMBDA_REF_EXPREVALCONTEXT));
            }

            return NewInstance<ProxyExprEvaluator>(lambda);
        }

        public static CodegenExpression CodegenEvaluators(
            IList<ExprNode> expressions,
            CodegenMethodScope parent,
            Type originator,
            CodegenClassScope classScope)
        {
            return CodegenEvaluators(ExprNodeUtilityQuery.GetForges(expressions), parent, originator, classScope);
        }

        public static CodegenExpression CodegenEvaluators(
            ExprForge[][] expressions,
            CodegenMethodScope parent,
            Type originator,
            CodegenClassScope classScope)
        {
            var init = new CodegenExpression[expressions.Length];
            for (int i = 0; i < init.Length; i++) {
                init[i] = CodegenEvaluators(expressions[i], parent, originator, classScope);
            }
            return NewArrayWithInit(typeof(ExprEvaluator[]), init);
        }
        
        public static CodegenExpression CodegenEvaluators(
            IList<ExprForge> expressions,
            CodegenMethodScope parent,
            Type originator,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ExprEvaluator[]), originator, classScope);
            method.Block.DeclareVar<ExprEvaluator[]>(
                "evals",
                NewArrayByLength(typeof(ExprEvaluator), Constant(expressions.Count)));
            for (var i = 0; i < expressions.Count; i++) {
                method.Block.AssignArrayElement(
                    "evals",
                    Constant(i),
                    expressions[i] == null
                        ? ConstantNull()
                        : CodegenEvaluator(expressions[i], method, originator, classScope));
            }

            method.Block.MethodReturn(Ref("evals"));
            return LocalMethod(method);
        }

        public static CodegenExpression CodegenEvaluatorNoCoerce(
            ExprForge forge,
            CodegenMethod method,
            Type generator,
            CodegenClassScope classScope)
        {
            return CodegenEvaluatorWCoerce(forge, null, method, generator, classScope);
        }

        public static CodegenExpression CodegenEvaluatorWCoerce(
            ExprForge forge,
            Type optCoercionType,
            CodegenMethod method,
            Type generator,
            CodegenClassScope classScope)
        {
            var evaluate = new CodegenExpressionLambda(method.Block)
                .WithParams(PARAMS);
            var evaluator = NewInstance<ProxyExprEvaluator>(evaluate);
            
            //var evaluator = NewAnonymousClass(method.Block, typeof(ExprEvaluator));
            //var evaluate = CodegenMethod.MakeParentNode(typeof(object), generator, classScope)
			//	.AddParam(PARAMS);
            //evaluator.AddMethod("Evaluate", evaluate);

            var result = ConstantNull();
            if (forge.EvaluationType != null) {
                var evalMethod = CodegenLegoMethodExpression.CodegenExpression(forge, method, classScope);
                result = LocalMethod(evalMethod, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT);

                var forgeEvaluationType = forge.EvaluationType.GetBoxedType();
                if ((optCoercionType != null) && (forgeEvaluationType != optCoercionType)) {
                    var coercer = SimpleNumberCoercerFactory.GetCoercer(forgeEvaluationType, optCoercionType.GetBoxedType());
                    evaluate.Block.DeclareVar(forgeEvaluationType, "result", result);
                    result = coercer.CoerceCodegen(Ref("result"), forgeEvaluationType);
                }
            }

            evaluate.Block.BlockReturn(result);
            return evaluator;
        }

        public static CodegenExpression CodegenEvaluatorObjectArray(
            IList<ExprForge> forges,
            CodegenMethod method,
            Type generator,
            CodegenClassScope classScope)
        {
            var exprSymbol = new ExprForgeCodegenSymbol(true, null);

            //var evaluator = NewAnonymousClass(method.Block, typeof(ExprEvaluator));
            //var evaluate = CodegenMethod.MakeParentNode<object>(generator, classScope)
			//	.AddParam(PARAMS);
            //evaluator.AddMethod("Evaluate", evaluate);

            var exprMethod = method
                .MakeChildWithScope(
                    typeof(object),
                    typeof(CodegenLegoMethodExpression),
                    exprSymbol,
                    classScope)
                .AddParam(PARAMS);

            var expressions = new CodegenExpression[forges.Count];
            for (var i = 0; i < forges.Count; i++) {
                var evaluationType = forges[i].EvaluationType;
                if (evaluationType.IsNullTypeSafe()) {
                    expressions[i] = ConstantNull();
                }
                else {
                    expressions[i] = forges[i]
                        .EvaluateCodegen(
                            evaluationType,
                            exprMethod,
                            exprSymbol,
                            classScope);
                }
            }

            exprSymbol.DerivedSymbolsCodegen(exprMethod, exprMethod.Block, classScope);

            exprMethod.Block.DeclareVar<object[]>(
                "values",
                NewArrayByLength(typeof(object), Constant(forges.Count)));
            for (var i = 0; i < forges.Count; i++) {
                var result = expressions[i];
                exprMethod.Block.AssignArrayElement("values", Constant(i), result);
            }

            exprMethod.Block.MethodReturn(Ref("values"));

            var evaluate = new CodegenExpressionLambda(method.Block)
                .WithParams(PARAMS)
                .WithBody(
                    block => block
                        .BlockReturn(
                            LocalMethod(
                                exprMethod,
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT)));

            return NewInstance<ProxyExprEvaluator>(evaluate);
        }


        public static CodegenMethod CodegenMapSelect(
            IList<ExprNode> selectClause,
            string[] selectAsNames,
            Type generator,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var exprSymbol = new ExprForgeCodegenSymbol(true, null);
            var method = parent.MakeChildWithScope(
                    typeof(IDictionary<string, object>),
                    generator,
                    exprSymbol,
                    classScope)
                .AddParam(PARAMS);

            method.Block.DeclareVar<IDictionary<string, object>>(
                "map",
                NewInstance(typeof(HashMap<string, object>), Constant(selectAsNames.Length + 2)));
            var expressions = new CodegenExpression[selectAsNames.Length];
            for (var i = 0; i < selectClause.Count; i++) {
                expressions[i] = selectClause[i].Forge.EvaluateCodegen(typeof(object), method, exprSymbol, classScope);
            }

            exprSymbol.DerivedSymbolsCodegen(method, method.Block, classScope);

            for (var i = 0; i < selectClause.Count; i++) {
                method.Block.ExprDotMethod(Ref("map"), "Put", Constant(selectAsNames[i]), expressions[i]);
            }

            method.Block.MethodReturn(Ref("map"));
            return method;
        }

        public static CodegenExpression CodegenExprEnumEval(
            ExprEnumerationGivenEventForge enumEval,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope,
            Type generator)
        {
            //var evaluator = NewAnonymousClass(method.Block, typeof(ExprEnumerationGivenEvent));

            var enumSymbols = new ExprEnumerationGivenEventSymbol();

            var evaluateEventGetROCollectionEventsMethod = method
                .MakeChildWithScope(typeof(FlexCollection), generator, enumSymbols, classScope)
                .AddParam(typeof(EventBean), "@event")
                .AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            evaluateEventGetROCollectionEventsMethod.Block.MethodReturn(
                enumEval.EvaluateEventGetROCollectionEventsCodegen(
                    evaluateEventGetROCollectionEventsMethod,
                    enumSymbols,
                    classScope));

            var evaluateEventGetROCollectionEvents = new CodegenExpressionLambda(method.Block)
                .WithParam(typeof(EventBean), "@event")
                .WithParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT)
                .WithBody(
                    block => {
                        block.DebugStack();
                        block.BlockReturn(
                            LocalMethod(
                                evaluateEventGetROCollectionEventsMethod,
                                Ref("@event"),
                                Ref(NAME_EXPREVALCONTEXT)));
                    });
                
            //var evaluateEventGetROCollectionEvents = CodegenMethod
            //    .MakeParentNode(typeof(ICollection<object>), generator, enumSymbols, classScope)
            //    .AddParam(typeof(EventBean), "@event")
            //    .AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            //evaluator.AddMethod("EvaluateEventGetROCollectionEvents", evaluateEventGetROCollectionEvents);
            //
            //evaluateEventGetROCollectionEvents.Block.MethodReturn(
            //    enumEval.EvaluateEventGetROCollectionEventsCodegen(
            //        evaluateEventGetROCollectionEvents,
            //        enumSymbols,
            //        classScope));

            var evaluateEventGetROCollectionScalarMethod = method
                .MakeChildWithScope(typeof(FlexCollection), generator, enumSymbols, classScope)
                .AddParam(typeof(EventBean), "@event")
                .AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            evaluateEventGetROCollectionScalarMethod.Block.MethodReturn(
                    enumEval.EvaluateEventGetROCollectionScalarCodegen(
                        evaluateEventGetROCollectionScalarMethod,
                        enumSymbols,
                        classScope));

            var evaluateEventGetROCollectionScalar = new CodegenExpressionLambda(method.Block)
                .WithParam(typeof(EventBean), "@event")
                .WithParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT)
                .WithBody(
                    block => {
                        block.DebugStack();
                        block.BlockReturn(
                            LocalMethod(
                                evaluateEventGetROCollectionScalarMethod,
                                Ref("@event"),
                                Ref(NAME_EXPREVALCONTEXT)));
                    });

            //var evaluateEventGetROCollectionScalar = CodegenMethod
            //    .MakeParentNode(typeof(ICollection<object>), generator, enumSymbols, classScope)
            //    .AddParam(typeof(EventBean), "@event")
            //    .AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            //evaluator.AddMethod("EvaluateEventGetROCollectionScalar", evaluateEventGetROCollectionScalar);
            //
            //evaluateEventGetROCollectionScalar.Block.MethodReturn(
            //    enumEval.EvaluateEventGetROCollectionScalarCodegen(
            //        evaluateEventGetROCollectionScalar,
            //        enumSymbols,
            //        classScope));

            var evaluateEventGetEventBeanMethod = method
                .MakeChildWithScope(typeof(EventBean), generator, enumSymbols, classScope)
                .AddParam(typeof(EventBean), "@event")
                .AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            evaluateEventGetEventBeanMethod.Block.MethodReturn(
                enumEval.EvaluateEventGetEventBeanCodegen(
                    evaluateEventGetEventBeanMethod, 
                    enumSymbols, 
                    classScope));
            
            var evaluateEventGetEventBean = new CodegenExpressionLambda(method.Block)
                .WithParam(typeof(EventBean), "@event")
                .WithParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT)
                .WithBody(
                    block => {
                        block.DebugStack();
                        block.BlockReturn(
                            LocalMethod(
                                evaluateEventGetEventBeanMethod,
                                Ref("@event"),
                                Ref(NAME_EXPREVALCONTEXT)));
                    });

            //var evaluateEventGetEventBean = CodegenMethod
            //    .MakeParentNode(typeof(EventBean), generator, enumSymbols, classScope)
            //    .AddParam(typeof(EventBean), "@event")
            //    .AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            //evaluator.AddMethod("EvaluateEventGetEventBean", evaluateEventGetEventBean);
            //
            //evaluateEventGetEventBean.Block.MethodReturn(
            //    enumEval.EvaluateEventGetEventBeanCodegen(
            //        evaluateEventGetEventBean, 
            //        enumSymbols, 
            //        classScope));

            var evaluator = NewInstance<ProxyExprEnumerationGivenEvent>(
                evaluateEventGetROCollectionEvents,
                evaluateEventGetROCollectionScalar,
                evaluateEventGetEventBean);

            return evaluator;
        }
    }
} // end of namespace