///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.variant;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen; //codegenExpressionMayCoerce;

namespace com.espertech.esper.common.@internal.compile.multikey
{
	public class MultiKeyCodegen
	{
		public static CodegenExpression CodegenEvaluatorReturnObjectOrArray(
			ExprForge[] forges,
			CodegenMethod method,
			Type generator,
			CodegenClassScope classScope)
		{
			return CodegenEvaluatorReturnObjectOrArrayWCoerce(forges, null, false, method, generator, classScope);
		}

		public static CodegenExpression CodegenEvaluatorReturnObjectOrArrayWCoerce(
			ExprForge[] forges,
			Type[] targetTypes,
			bool arrayMultikeyWhenSingleEvaluator,
			CodegenMethod method,
			Type generator,
			CodegenClassScope classScope)
		{
			var evaluate = new CodegenExpressionLambda(method.Block)
				.WithParams(PARAMS);
			var evaluator = NewInstance<ProxyExprEvaluator>(evaluate);

			// CodegenMethod evaluate = CodegenMethod
			// 	.MakeParentNode(typeof(object), generator, classScope)
			// 	.AddParam(ExprForgeCodegenNames.PARAMS);
			// evaluator.AddMethod("evaluate", evaluate);

			var exprSymbol = new ExprForgeCodegenSymbol(true, null);
			var exprMethod = method
				.MakeChildWithScope(typeof(object), typeof(CodegenLegoMethodExpression), exprSymbol, classScope)
				.AddParam(ExprForgeCodegenNames.PARAMS);

			var expressions = new CodegenExpression[forges.Length];
			for (var i = 0; i < forges.Length; i++) {
				expressions[i] = forges[i].EvaluateCodegen(forges[i].EvaluationType, exprMethod, exprSymbol, classScope);
			}

			exprSymbol.DerivedSymbolsCodegen(method, exprMethod.Block, classScope);

			if (forges.Length == 0) {
				exprMethod.Block.MethodReturn(ConstantNull());
			}
			else if (forges.Length == 1) {
				var evaluationType = forges[0].EvaluationType;
				CodegenExpression coerced;
				if (arrayMultikeyWhenSingleEvaluator && evaluationType.IsArray) {
					var clazz = MultiKeyPlanner.GetMKClassForComponentType(evaluationType.GetElementType());
					coerced = NewInstance(clazz, expressions[0]);
				}
				else {
					coerced = ExprNodeUtilityCodegen.CodegenCoerce(expressions[0], evaluationType, targetTypes == null ? null : targetTypes[0], false);
				}

				exprMethod.Block.MethodReturn(coerced);
			}
			else {
				exprMethod.Block.DeclareVar(typeof(object[]), "values", NewArrayByLength(typeof(object), Constant(forges.Length)));
				for (var i = 0; i < forges.Length; i++) {
					var coerced = ExprNodeUtilityCodegen.CodegenCoerce(
						expressions[i],
						forges[i].EvaluationType,
						targetTypes == null ? null : targetTypes[i],
						false);
					exprMethod.Block.AssignArrayElement("values", Constant(i), coerced);
				}

				exprMethod.Block.MethodReturn(Ref("values"));
			}

			evaluate.Block.MethodReturn(LocalMethod(exprMethod, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
			return evaluator;
		}

		public static CodegenExpression CodegenExprEvaluatorMayMultikey(
			ExprNode[] expressionNodes,
			Type[] optionalCoercionTypes,
			MultiKeyClassRef multiKeyClassRef,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			if (expressionNodes == null || expressionNodes.Length == 0) {
				return ConstantNull();
			}

			return CodegenExprEvaluatorMayMultikey(
				ExprNodeUtilityQuery.GetForges(expressionNodes),
				optionalCoercionTypes,
				multiKeyClassRef,
				method,
				classScope);
		}

		public static CodegenExpression CodegenExprEvaluatorMayMultikey(
			ExprForge[] forges,
			Type[] optionalCoercionTypes,
			MultiKeyClassRef multiKeyClassRef,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			if (forges == null || forges.Length == 0) {
				return ConstantNull();
			}

			if (multiKeyClassRef != null && multiKeyClassRef.ClassNameMK != null) {
				return CodegenMultiKeyExprEvaluator(forges, multiKeyClassRef, method, classScope);
			}

			return ExprNodeUtilityCodegen.CodegenEvaluatorWCoerce(
				forges[0],
				optionalCoercionTypes == null ? null : optionalCoercionTypes[0],
				method,
				typeof(MultiKeyCodegen),
				classScope);
		}

		public static CodegenMethod CodegenMethod(
			ExprNode[] expressionNodes,
			MultiKeyClassRef multiKeyClassRef,
			CodegenMethodScope parent,
			CodegenClassScope classScope)
		{
			var eventUnpackMethod = parent
				.MakeChildWithScope(typeof(object), typeof(CodegenLegoMethodExpression), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(ExprForgeCodegenNames.PARAMS);

			var exprSymbol = new ExprForgeCodegenSymbol(true, null);
			var exprMethod = eventUnpackMethod
				.MakeChildWithScope(typeof(object), typeof(CodegenLegoMethodExpression), exprSymbol, classScope)
				.AddParam(ExprForgeCodegenNames.PARAMS);

			var expressions = new CodegenExpression[expressionNodes.Length];
			for (var i = 0; i < expressionNodes.Length; i++) {
				var forge = expressionNodes[i].Forge;
				expressions[i] = CodegenExpressionMayCoerce(forge, multiKeyClassRef.MKTypes[i], exprMethod, exprSymbol, classScope);
			}

			exprSymbol.DerivedSymbolsCodegen(eventUnpackMethod, exprMethod.Block, classScope);
			exprMethod.Block.MethodReturn(NewInstance(multiKeyClassRef.ClassNameMK, expressions));

			eventUnpackMethod.Block.MethodReturn(LocalMethod(exprMethod, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
			return eventUnpackMethod;
		}

		public static CodegenExpression CodegenGetterMayMultiKey(
			EventType eventType,
			EventPropertyGetterSPI[] getters,
			Type[] getterResultTypes,
			Type[] optionalCoercionTypes,
			MultiKeyClassRef multiKeyClassRef,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			if (getters == null || getters.Length == 0) {
				return ConstantNull();
			}

			if (multiKeyClassRef != null && multiKeyClassRef.ClassNameMK != null) {
				return CodegenMultiKeyGetter(eventType, getters, getterResultTypes, multiKeyClassRef, method, classScope);
			}

			return EventTypeUtility.CodegenGetterWCoerce(
				getters[0],
				getterResultTypes[0],
				optionalCoercionTypes == null ? null : optionalCoercionTypes[0],
				method,
				typeof(MultiKeyCodegen),
				classScope);
		}

		public static CodegenExpression CodegenMultiKeyFromArrayTransform(
			MultiKeyClassRef optionalMultiKeyClasses,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			var fromLambda = new CodegenExpressionLambda(method.Block).WithParam<object[]>("keys");
			var fromClass = NewInstance<ProxyMultiKeyFromObjectArray>(fromLambda);

			// CodegenExpressionNewAnonymousClass fromClass = NewAnonymousClass(method.Block, typeof(MultiKeyFromObjectArray));
			// CodegenMethod from = CodegenMethod
			// 	.MakeParentNode(typeof(object), typeof(MultiKeyCodegen), classScope)
			// 	.AddParam(typeof(object[]), "keys");
			// fromClass.AddMethod("from", from);

			if (optionalMultiKeyClasses == null || optionalMultiKeyClasses.ClassNameMK == null) {
				fromLambda.Block.BlockReturn(ArrayAtIndex(Ref("keys"), Constant(0)));
			}
			else if (optionalMultiKeyClasses.MKTypes.Length == 1) {
				var paramType = optionalMultiKeyClasses.MKTypes[0];
				if (paramType == null || !paramType.IsArray) {
					fromLambda.Block.BlockReturn(ArrayAtIndex(Ref("keys"), Constant(0)));
				}
				else {
					var mktype = MultiKeyPlanner.GetMKClassForComponentType(paramType.GetElementType());
					fromLambda.Block.BlockReturn(NewInstance(mktype, Cast(paramType, ArrayAtIndex(Ref("keys"), Constant(0)))));
				}
			}
			else {
				var expressions = new CodegenExpression[optionalMultiKeyClasses.MKTypes.Length];
				for (var i = 0; i < expressions.Length; i++) {
					expressions[i] = Cast(optionalMultiKeyClasses.MKTypes[i], ArrayAtIndex(Ref("keys"), Constant(i)));
				}

				fromLambda.Block.BlockReturn(NewInstance(optionalMultiKeyClasses.ClassNameMK, expressions));
			}

			return fromClass;
		}

		public static CodegenExpression CodegenMultiKeyFromMultiKeyTransform(
			MultiKeyClassRef optionalMultiKeyClasses,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			var fromLambda = new CodegenExpressionLambda(method.Block).WithParam<object>("key");
			var fromInstance = NewInstance<ProxyMultiKeyFromMultiKey>(fromLambda);
			
			// CodegenExpressionNewAnonymousClass fromClass = NewAnonymousClass(method.Block, typeof(MultiKeyFromMultiKey));
			// var from = bytecodemodel.@base.CodegenMethod
			// 	.MakeParentNode(typeof(object), typeof(MultiKeyCodegen), classScope)
			// 	.AddParam(typeof(object), "key");
			// fromClass.AddMethod("from", from);

			if (optionalMultiKeyClasses == null || optionalMultiKeyClasses.ClassNameMK == null || optionalMultiKeyClasses.MKTypes.Length == 1) {
				fromLambda.Block.BlockReturn(Ref("key"));
			}
			else {
				var expressions = new CodegenExpression[optionalMultiKeyClasses.MKTypes.Length];
				fromLambda.Block.DeclareVar(typeof(MultiKeyArrayOfKeys<object>), "mk", Cast(typeof(MultiKeyArrayOfKeys<object>), Ref("key")));
				for (var i = 0; i < expressions.Length; i++) {
					expressions[i] = Cast(optionalMultiKeyClasses.MKTypes[i], ExprDotMethod(Ref("mk"), "GetKey", Constant(i)));
				}

				fromLambda.Block.BlockReturn(NewInstance(optionalMultiKeyClasses.ClassNameMK, expressions));
			}

			return fromInstance;
		}

		public static CodegenExpression CodegenGetterEventDistinct(
			bool isDistinct,
			EventType eventType,
			MultiKeyClassRef optionalDistinctMultiKey,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			if (!isDistinct) {
				return ConstantNull();
			}

			var propertyNames = eventType.PropertyNames;
			var spi = (EventTypeSPI) eventType;
			if (propertyNames.Length == 1) {
				var propertyName = propertyNames[0];
				var result = eventType.GetPropertyType(propertyName);
				var getter = spi.GetGetterSPI(propertyName);
				return EventTypeUtility.CodegenGetterWCoerceWArray(
					typeof(EventPropertyValueGetter),
					getter,
					result,
					null,
					method,
					typeof(MultiKeyCodegen),
					classScope);
			}

			var getters = new EventPropertyGetterSPI[propertyNames.Length];
			var getterResultTypes = new Type[propertyNames.Length];
			for (var i = 0; i < propertyNames.Length; i++) {
				getterResultTypes[i] = eventType.GetPropertyType(propertyNames[i]);
				getters[i] = spi.GetGetterSPI(propertyNames[i]);
			}

			if (eventType is VariantEventType) {
				return CodegenMultikeyGetterBeanGet(getters, getterResultTypes, optionalDistinctMultiKey, method, classScope);
			}

			return CodegenGetterMayMultiKey(eventType, getters, getterResultTypes, null, optionalDistinctMultiKey, method, classScope);
		}

		private static CodegenExpression CodegenMultiKeyExprEvaluator(
			ExprForge[] expressionNodes,
			MultiKeyClassRef multiKeyClassRef,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			var evaluate = new CodegenExpressionLambda(method.Block)
				.WithParams(PARAMS);
			var evaluator = NewInstance<ProxyExprEvaluator>(evaluate);
			
			// CodegenExpressionNewAnonymousClass evaluator = NewAnonymousClass(method.Block, typeof(ExprEvaluator));
			// CodegenMethod evaluate = CodegenMethod
			// 	.MakeParentNode(typeof(object), typeof(StmtClassForgeableMultiKey), classScope)
			// 	.AddParam(ExprForgeCodegenNames.PARAMS);
			//
			// evaluator.AddMethod("evaluate", evaluate);

			var exprSymbol = new ExprForgeCodegenSymbol(true, null);
			var exprMethod = method
				.MakeChildWithScope(typeof(object), typeof(CodegenLegoMethodExpression), exprSymbol, classScope)
				.AddParam(ExprForgeCodegenNames.PARAMS);

			var expressions = new CodegenExpression[expressionNodes.Length];
			for (var i = 0; i < expressionNodes.Length; i++) {
				expressions[i] = CodegenExpressionMayCoerce(expressionNodes[i], multiKeyClassRef.MKTypes[i], exprMethod, exprSymbol, classScope);
			}

			exprSymbol.DerivedSymbolsCodegen(method, exprMethod.Block, classScope);
			exprMethod.Block.MethodReturn(NewInstance(multiKeyClassRef.ClassNameMK, expressions));

			evaluate.Block.MethodReturn(LocalMethod(exprMethod, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
			return evaluator;
		}

		private static CodegenExpression CodegenMultiKeyGetter(
			EventType eventType,
			EventPropertyGetterSPI[] getters,
			Type[] getterResultTypes,
			MultiKeyClassRef multiKeyClassRef,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			var get = new CodegenExpressionLambda(method.Block)
				.WithParams(CodegenNamedParam.From(typeof(EventBean), "bean"));
			var anonymous = NewInstance<ProxyEventPropertyValueGetter>(get);

			var expressions = new CodegenExpression[getters.Length];
			for (var i = 0; i < getters.Length; i++) {
				expressions[i] = getters[i].UnderlyingGetCodegen(Ref("und"), method, classScope);
				var mkType = multiKeyClassRef.MKTypes[i];
				var getterType = getterResultTypes[i];
				expressions[i] = ExprNodeUtilityCodegen.CodegenCoerce(expressions[i], getterType, mkType, true);
			}

			get.Block
				.DeclareVar(eventType.UnderlyingType, "und", Cast(eventType.UnderlyingType, ExprDotUnderlying(Ref("bean"))))
				.BlockReturn(NewInstance(multiKeyClassRef.ClassNameMK, expressions));

			return anonymous;
		}

		private static CodegenExpression CodegenMultikeyGetterBeanGet(
			EventPropertyGetterSPI[] getters,
			Type[] getterResultTypes,
			MultiKeyClassRef multiKeyClassRef,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			var get = new CodegenExpressionLambda(method.Block)
				.WithParams(CodegenNamedParam.From(typeof(EventBean), "bean"));
			var lambda = NewInstance<ProxyEventPropertyValueGetter>(get);
			
			var expressions = new CodegenExpression[getters.Length];
			for (var i = 0; i < getters.Length; i++) {
				expressions[i] = getters[i].EventBeanGetCodegen(Ref("bean"), method, classScope);
				var mkType = multiKeyClassRef.MKTypes[i];
				var getterType = getterResultTypes[i];
				expressions[i] = ExprNodeUtilityCodegen.CodegenCoerce(expressions[i], getterType, mkType, true);
			}

			get.Block.BlockReturn(NewInstance(multiKeyClassRef.ClassNameMK, expressions));

			return lambda;
		}
	}
} // end of namespace
