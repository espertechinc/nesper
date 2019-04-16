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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeUtilityCodegen
    {
        public static CodegenExpressionNewAnonymousClass CodegenEvaluator(
            ExprForge forge,
            CodegenMethod method,
            Type originator,
            CodegenClassScope classScope)
        {
            var anonymousClass = NewAnonymousClass(method.Block, typeof(ExprEvaluator));
            var evaluate = CodegenMethod.MakeParentNode(typeof(object), originator, classScope).AddParam(PARAMS);
            anonymousClass.AddMethod("evaluate", evaluate);
            if (forge.EvaluationType == null) {
                evaluate.Block.MethodReturn(ConstantNull());
            }
            else {
                var evalMethod = CodegenLegoMethodExpression.CodegenExpression(forge, method, classScope);
                evaluate.Block.MethodReturn(LocalMethod(evalMethod, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
            }

            return anonymousClass;
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
            IList<ExprForge> expressions,
            CodegenMethodScope parent,
            Type originator,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ExprEvaluator[]), originator, classScope);
            method.Block.DeclareVar(
                typeof(ExprEvaluator[]), "evals", NewArrayByLength(typeof(ExprEvaluator), Constant(expressions.Count)));
            for (var i = 0; i < expressions.Count; i++) {
                method.Block.AssignArrayElement(
                    "evals", Constant(i),
                    expressions[i] == null
                        ? ConstantNull()
                        : CodegenEvaluator(expressions[i], method, originator, classScope));
            }

            method.Block.MethodReturn(Ref("evals"));
            return LocalMethod(method);
        }

        public static CodegenExpressionNewAnonymousClass CodegenEvaluatorNoCoerce(
            ExprForge forge,
            CodegenMethod method,
            Type generator,
            CodegenClassScope classScope)
        {
            return CodegenEvaluatorWCoerce(forge, null, method, generator, classScope);
        }

        public static CodegenExpressionNewAnonymousClass CodegenEvaluatorWCoerce(
            ExprForge forge,
            Type optCoercionType,
            CodegenMethod method,
            Type generator,
            CodegenClassScope classScope)
        {
            var evaluator = NewAnonymousClass(method.Block, typeof(ExprEvaluator));
            var evaluate = CodegenMethod.MakeParentNode(typeof(object), generator, classScope).AddParam(PARAMS);
            evaluator.AddMethod("evaluate", evaluate);

            var result = ConstantNull();
            if (forge.EvaluationType != null) {
                var evalMethod = CodegenLegoMethodExpression.CodegenExpression(forge, method, classScope);
                result = LocalMethod(evalMethod, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT);

                if (optCoercionType != null && forge.EvaluationType.GetBoxedType() != optCoercionType.GetBoxedType()) {
                    var coercer = SimpleNumberCoercerFactory.GetCoercer(
                        forge.EvaluationType, optCoercionType.GetBoxedType());
                    evaluate.Block.DeclareVar(forge.EvaluationType, "result", result);
                    result = coercer.CoerceCodegen(Ref("result"), forge.EvaluationType);
                }
            }

            evaluate.Block.MethodReturn(result);
            return evaluator;
        }

        public static CodegenExpressionNewAnonymousClass CodegenEvaluatorMayMultiKeyWCoerce(
            IList<ExprForge> forges,
            IList<Type> optCoercionTypes,
            CodegenMethod method,
            Type generator,
            CodegenClassScope classScope)
        {
            if (forges.Count == 1) {
                return CodegenEvaluatorWCoerce(
                    forges[0], optCoercionTypes == null ? null : optCoercionTypes[0], method, generator, classScope);
            }

            var evaluator = NewAnonymousClass(method.Block, typeof(ExprEvaluator));
            var evaluate = CodegenMethod.MakeParentNode(typeof(object), generator, classScope).AddParam(PARAMS);
            evaluator.AddMethod("evaluate", evaluate);

            var exprSymbol = new ExprForgeCodegenSymbol(true, null);
            var exprMethod = evaluate.MakeChildWithScope(
                typeof(object), typeof(CodegenLegoMethodExpression), exprSymbol, classScope).AddParam(PARAMS);

            var expressions = new CodegenExpression[forges.Count];
            for (var i = 0; i < forges.Count; i++) {
                expressions[i] = forges[i].EvaluateCodegen(
                    forges[i].EvaluationType, exprMethod, exprSymbol, classScope);
            }

            exprSymbol.DerivedSymbolsCodegen(evaluate, exprMethod.Block, classScope);

            exprMethod.Block.DeclareVar(
                    typeof(object[]), "values", NewArrayByLength(typeof(object), Constant(forges.Count)))
                .DeclareVar(typeof(HashableMultiKey), "valuesMk", NewInstance(typeof(HashableMultiKey), Ref("values")));
            for (var i = 0; i < forges.Count; i++) {
                var result = expressions[i];
                if (optCoercionTypes != null &&
                    forges[i].EvaluationType.GetBoxedType() != optCoercionTypes[i].GetBoxedType()) {
                    var coercer = SimpleNumberCoercerFactory.GetCoercer(
                        forges[i].EvaluationType, optCoercionTypes[i].GetBoxedType());
                    var name = "result_" + i;
                    exprMethod.Block.DeclareVar(forges[i].EvaluationType, name, expressions[i]);
                    result = coercer.CoerceCodegen(Ref(name), forges[i].EvaluationType);
                }

                exprMethod.Block.AssignArrayElement("values", Constant(i), result);
            }

            exprMethod.Block.MethodReturn(Ref("valuesMk"));
            evaluate.Block.MethodReturn(LocalMethod(exprMethod, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));

            return evaluator;
        }

        public static CodegenExpressionNewAnonymousClass CodegenEvaluatorObjectArray(
            IList<ExprForge> forges,
            CodegenMethod method,
            Type generator,
            CodegenClassScope classScope)
        {
            var evaluator = NewAnonymousClass(method.Block, typeof(ExprEvaluator));
            var evaluate = CodegenMethod.MakeParentNode(typeof(object), generator, classScope).AddParam(PARAMS);
            evaluator.AddMethod("evaluate", evaluate);

            var exprSymbol = new ExprForgeCodegenSymbol(true, null);
            var exprMethod = evaluate.MakeChildWithScope(
                typeof(object), typeof(CodegenLegoMethodExpression), exprSymbol, classScope).AddParam(PARAMS);

            var expressions = new CodegenExpression[forges.Count];
            for (var i = 0; i < forges.Count; i++) {
                expressions[i] = forges[i].EvaluateCodegen(
                    forges[i].EvaluationType, exprMethod, exprSymbol, classScope);
            }

            exprSymbol.DerivedSymbolsCodegen(evaluate, exprMethod.Block, classScope);

            exprMethod.Block.DeclareVar(
                typeof(object[]), "values", NewArrayByLength(typeof(object), Constant(forges.Count)));
            for (var i = 0; i < forges.Count; i++) {
                var result = expressions[i];
                exprMethod.Block.AssignArrayElement("values", Constant(i), result);
            }

            exprMethod.Block.MethodReturn(Ref("values"));
            evaluate.Block.MethodReturn(LocalMethod(exprMethod, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));

            return evaluator;
        }

        public static CodegenExpression CodegenEvaluatorMayMultiKeyPropPerStream(
            IList<EventType> outerStreamTypesZeroIndexed,
            string[] propertyNames,
            Type[] optionalCoercionTypes,
            int[] keyStreamNums,
            CodegenMethod method,
            Type generator,
            CodegenClassScope classScope)
        {
            var forges = new ExprForge[propertyNames.Length];
            for (var i = 0; i < propertyNames.Length; i++) {
                var node = new ExprIdentNodeImpl(
                    outerStreamTypesZeroIndexed[keyStreamNums[i]], propertyNames[i], keyStreamNums[i]);
                forges[i] = node.Forge;
            }

            return CodegenEvaluatorMayMultiKeyWCoerce(forges, optionalCoercionTypes, method, generator, classScope);
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
                typeof(IDictionary<object, object>), generator, exprSymbol, classScope).AddParam(PARAMS);

            method.Block.DeclareVar(
                typeof(IDictionary<object, object>), "map",
                NewInstance(typeof(Dictionary<object, object>), Constant(selectAsNames.Length + 2)));
            var expressions = new CodegenExpression[selectAsNames.Length];
            for (var i = 0; i < selectClause.Count; i++) {
                expressions[i] = selectClause[i].Forge.EvaluateCodegen(typeof(object), method, exprSymbol, classScope);
            }

            exprSymbol.DerivedSymbolsCodegen(method, method.Block, classScope);

            for (var i = 0; i < selectClause.Count; i++) {
                method.Block.ExprDotMethod(Ref("map"), "put", Constant(selectAsNames[i]), expressions[i]);
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
            var evaluator = NewAnonymousClass(method.Block, typeof(ExprEnumerationGivenEvent));

            var enumSymbols = new ExprEnumerationGivenEventSymbol();
            var evaluateEventGetROCollectionEvents = CodegenMethod
                .MakeParentNode(typeof(ICollection<object>), generator, enumSymbols, classScope)
                .AddParam(typeof(EventBean), "event").AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            evaluator.AddMethod("evaluateEventGetROCollectionEvents", evaluateEventGetROCollectionEvents);
            evaluateEventGetROCollectionEvents.Block.MethodReturn(
                enumEval.EvaluateEventGetROCollectionEventsCodegen(
                    evaluateEventGetROCollectionEvents, enumSymbols, classScope));

            var evaluateEventGetROCollectionScalar = CodegenMethod
                .MakeParentNode(typeof(ICollection<object>), generator, enumSymbols, classScope)
                .AddParam(typeof(EventBean), "event").AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            evaluator.AddMethod("evaluateEventGetROCollectionScalar", evaluateEventGetROCollectionScalar);
            evaluateEventGetROCollectionScalar.Block.MethodReturn(
                enumEval.EvaluateEventGetROCollectionScalarCodegen(
                    evaluateEventGetROCollectionScalar, enumSymbols, classScope));

            var evaluateEventGetEventBean = CodegenMethod
                .MakeParentNode(typeof(EventBean), generator, enumSymbols, classScope)
                .AddParam(typeof(EventBean), "event").AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            evaluator.AddMethod("evaluateEventGetEventBean", evaluateEventGetEventBean);
            evaluateEventGetEventBean.Block.MethodReturn(
                enumEval.EvaluateEventGetEventBeanCodegen(evaluateEventGetEventBean, enumSymbols, classScope));

            return evaluator;
        }
    }
} // end of namespace