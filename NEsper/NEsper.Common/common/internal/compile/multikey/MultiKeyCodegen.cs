///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen; //codegenExpressionMayCoerce;

namespace com.espertech.esper.common.@internal.compile.multikey
{
	public class MultiKeyCodegen
	{

		public static CodegenExpressionNewAnonymousClass CodegenEvaluatorReturnObjectOrArray(
			ExprForge[] forges,
			CodegenMethod method,
			Type generator,
			CodegenClassScope classScope)
		{
			return CodegenEvaluatorReturnObjectOrArrayWCoerce(forges, null, false, method, generator, classScope);
		}

		public static CodegenExpressionNewAnonymousClass CodegenEvaluatorReturnObjectOrArrayWCoerce(
			ExprForge[] forges,
			Type[] targetTypes,
			bool arrayMultikeyWhenSingleEvaluator,
			CodegenMethod method,
			Type generator,
			CodegenClassScope classScope)
		{
			CodegenExpressionNewAnonymousClass evaluator = NewAnonymousClass(method.Block, typeof(ExprEvaluator));
			CodegenMethod evaluate = CodegenMethod
				.MakeParentNode(typeof(object), generator, classScope)
				.AddParam(ExprForgeCodegenNames.PARAMS);
			evaluator.AddMethod("evaluate", evaluate);

			ExprForgeCodegenSymbol exprSymbol = new ExprForgeCodegenSymbol(true, null);
			CodegenMethod exprMethod = evaluate
				.MakeChildWithScope(typeof(object), typeof(CodegenLegoMethodExpression), exprSymbol, classScope)
				.AddParam(ExprForgeCodegenNames.PARAMS);

			CodegenExpression[] expressions = new CodegenExpression[forges.Length];
			for (int i = 0; i < forges.Length; i++) {
				expressions[i] = forges[i].EvaluateCodegen(forges[i].EvaluationType, exprMethod, exprSymbol, classScope);
			}

			exprSymbol.DerivedSymbolsCodegen(evaluate, exprMethod.Block, classScope);

			if (forges.Length == 0) {
				exprMethod.Block.MethodReturn(ConstantNull());
			}
			else if (forges.Length == 1) {
				Type evaluationType = forges[0].EvaluationType;
				CodegenExpression coerced;
				if (arrayMultikeyWhenSingleEvaluator && evaluationType.IsArray) {
					Type clazz = MultiKeyPlanner.GetMKClassForComponentType(evaluationType.ComponentType);
					coerced = NewInstance(clazz, expressions[0]);
				}
				else {
					coerced = ExprNodeUtilityCodegen.CodegenCoerce(expressions[0], evaluationType, targetTypes == null ? null : targetTypes[0], false);
				}

				exprMethod.Block.MethodReturn(coerced);
			}
			else {
				exprMethod.Block.DeclareVar(typeof(object[]), "values", NewArrayByLength(typeof(object), Constant(forges.Length)));
				for (int i = 0; i < forges.Length; i++) {
					CodegenExpression coerced = ExprNodeUtilityCodegen.CodegenCoerce(
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
			CodegenMethod eventUnpackMethod = parent
				.MakeChildWithScope(typeof(object), typeof(CodegenLegoMethodExpression), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(ExprForgeCodegenNames.PARAMS);

			ExprForgeCodegenSymbol exprSymbol = new ExprForgeCodegenSymbol(true, null);
			CodegenMethod exprMethod = eventUnpackMethod
				.MakeChildWithScope(typeof(object), typeof(CodegenLegoMethodExpression), exprSymbol, classScope)
				.AddParam(ExprForgeCodegenNames.PARAMS);

			CodegenExpression[] expressions = new CodegenExpression[expressionNodes.Length];
			for (int i = 0; i < expressionNodes.Length; i++) {
				ExprForge forge = expressionNodes[i].Forge;
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
			CodegenExpressionNewAnonymousClass fromClass = NewAnonymousClass(method.Block, typeof(MultiKeyFromObjectArray));
			CodegenMethod from = CodegenMethod
				.MakeParentNode(typeof(object), typeof(MultiKeyCodegen), classScope)
				.AddParam(typeof(object[]), "keys");
			fromClass.AddMethod("from", from);

			if (optionalMultiKeyClasses == null || optionalMultiKeyClasses.ClassNameMK == null) {
				from.Block.MethodReturn(ArrayAtIndex(Ref("keys"), Constant(0)));
			}
			else if (optionalMultiKeyClasses.MKTypes.Length == 1) {
				Type paramType = optionalMultiKeyClasses.MKTypes[0];
				if (paramType == null || !paramType.IsArray) {
					from.Block.MethodReturn(ArrayAtIndex(Ref("keys"), Constant(0)));
				}
				else {
					Type mktype = MultiKeyPlanner.GetMKClassForComponentType(paramType.GetElementType());
					from.Block.MethodReturn(NewInstance(mktype, Cast(paramType, ArrayAtIndex(Ref("keys"), Constant(0)))));
				}
			}
			else {
				CodegenExpression[] expressions = new CodegenExpression[optionalMultiKeyClasses.MKTypes.Length];
				for (int i = 0; i < expressions.Length; i++) {
					expressions[i] = Cast(optionalMultiKeyClasses.MKTypes[i], ArrayAtIndex(Ref("keys"), Constant(i)));
				}

				from.Block.MethodReturn(NewInstance(optionalMultiKeyClasses.ClassNameMK, expressions));
			}

			return fromClass;
		}

		public static CodegenExpression CodegenMultiKeyFromMultiKeyTransform(
			MultiKeyClassRef optionalMultiKeyClasses,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			CodegenExpressionNewAnonymousClass fromClass = NewAnonymousClass(method.Block, typeof(MultiKeyFromMultiKey));
			CodegenMethod from = CodegenMethod.MakeParentNode(typeof(object), typeof(MultiKeyCodegen), classScope)
				.AddParam(typeof(object), "key");
			fromClass.AddMethod("from", from);

			if (optionalMultiKeyClasses == null || optionalMultiKeyClasses.ClassNameMK == null || optionalMultiKeyClasses.MKTypes.Length == 1) {
				from.Block.MethodReturn(Ref("key"));
			}
			else {
				CodegenExpression[] expressions = new CodegenExpression[optionalMultiKeyClasses.MKTypes.Length];
				from.Block.DeclareVar(typeof(MultiKeyArrayOfKeys<>), "mk", Cast(typeof(MultiKeyArrayOfKeys<>), Ref("key")));
				for (int i = 0; i < expressions.Length; i++) {
					expressions[i] = Cast(optionalMultiKeyClasses.MKTypes[i], ExprDotMethod(Ref("mk"), "getKey", Constant(i)));
				}

				from.Block.MethodReturn(NewInstance(optionalMultiKeyClasses.ClassNameMK, expressions));
			}

			return fromClass;
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

			string[] propertyNames = eventType.PropertyNames;
			EventTypeSPI spi = (EventTypeSPI) eventType;
			if (propertyNames.Length == 1) {
				string propertyName = propertyNames[0];
				Type result = eventType.GetPropertyType(propertyName);
				EventPropertyGetterSPI getter = spi.GetGetterSPI(propertyName);
				return EventTypeUtility.CodegenGetterWCoerceWArray(
					typeof(EventPropertyValueGetter),
					getter,
					result,
					null,
					method,
					typeof(MultiKeyCodegen),
					classScope);
			}

			EventPropertyGetterSPI[] getters = new EventPropertyGetterSPI[propertyNames.Length];
			Type[] getterResultTypes = new Type[propertyNames.Length];
			for (int i = 0; i < propertyNames.Length; i++) {
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
			CodegenExpressionNewAnonymousClass evaluator = NewAnonymousClass(method.Block, typeof(ExprEvaluator));
			CodegenMethod evaluate = CodegenMethod
				.MakeParentNode(typeof(object), typeof(StmtClassForgeableMultiKey), classScope)
				.AddParam(ExprForgeCodegenNames.PARAMS);
			evaluator.AddMethod("evaluate", evaluate);

			ExprForgeCodegenSymbol exprSymbol = new ExprForgeCodegenSymbol(true, null);
			CodegenMethod exprMethod = evaluate.MakeChildWithScope(typeof(object), typeof(CodegenLegoMethodExpression), exprSymbol, classScope)
				.AddParam(ExprForgeCodegenNames.PARAMS);

			CodegenExpression[] expressions = new CodegenExpression[expressionNodes.Length];
			for (int i = 0; i < expressionNodes.Length; i++) {
				expressions[i] = CodegenExpressionMayCoerce(expressionNodes[i], multiKeyClassRef.MKTypes[i], exprMethod, exprSymbol, classScope);
			}

			exprSymbol.DerivedSymbolsCodegen(evaluate, exprMethod.Block, classScope);
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
			CodegenMethod get = CodegenMethod.MakeParentNode(typeof(object), typeof(StmtClassForgeableMultiKey), classScope)
				.AddParam(typeof(EventBean), "bean");
			CodegenExpressionNewAnonymousClass getter = NewAnonymousClass(method.Block, typeof(EventPropertyValueGetter));
			getter.AddMethod("get", get);

			CodegenExpression[] expressions = new CodegenExpression[getters.Length];
			for (int i = 0; i < getters.Length; i++) {
				expressions[i] = getters[i].UnderlyingGetCodegen(Ref("und"), get, classScope);
				Type mkType = multiKeyClassRef.MKTypes[i];
				Type getterType = getterResultTypes[i];
				expressions[i] = ExprNodeUtilityCodegen.CodegenCoerce(expressions[i], getterType, mkType, true);
			}

			get.Block
				.DeclareVar(eventType.UnderlyingType, "und", Cast(eventType.UnderlyingType, ExprDotUnderlying(Ref("bean"))))
				.MethodReturn(NewInstance(multiKeyClassRef.ClassNameMK, expressions));

			return getter;
		}

		private static CodegenExpression CodegenMultikeyGetterBeanGet(
			EventPropertyGetterSPI[] getters,
			Type[] getterResultTypes,
			MultiKeyClassRef multiKeyClassRef,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			CodegenMethod get = CodegenMethod
				.MakeParentNode(typeof(object), typeof(StmtClassForgeableMultiKey), classScope)
				.AddParam(typeof(EventBean), "bean");
			CodegenExpressionNewAnonymousClass getter = NewAnonymousClass(method.Block, typeof(EventPropertyValueGetter));
			getter.AddMethod("get", get);

			CodegenExpression[] expressions = new CodegenExpression[getters.Length];
			for (int i = 0; i < getters.Length; i++) {
				expressions[i] = getters[i].EventBeanGetCodegen(Ref("bean"), get, classScope);
				Type mkType = multiKeyClassRef.MKTypes[i];
				Type getterType = getterResultTypes[i];
				expressions[i] = ExprNodeUtilityCodegen.CodegenCoerce(expressions[i], getterType, mkType, true);
			}

			get.Block
				.MethodReturn(NewInstance(multiKeyClassRef.ClassNameMK, expressions));

			return getter;
		}
	}
} // end of namespace
