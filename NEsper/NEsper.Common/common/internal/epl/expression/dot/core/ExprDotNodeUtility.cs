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
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.analyze;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode; //instblock

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
	public class ExprDotNodeUtility
	{
		public static ObjectArrayEventType MakeTransientOAType(
			string enumMethod,
			string propertyName,
			Type type,
			StatementRawInfo statementRawInfo,
			StatementCompileTimeServices services)
		{
			IDictionary<string, object> propsResult = new Dictionary<string, object>();
			propsResult.Put(propertyName, type.GetBoxedType());
			return MakeTransientOATypeInternal(
				enumMethod,
				propsResult,
				propertyName,
				statementRawInfo,
				services);
		}

		public static ObjectArrayEventType MakeTransientOAType(
			string enumMethod,
			IDictionary<string, object> boxedPropertyTypes,
			StatementRawInfo statementRawInfo,
			StatementCompileTimeServices services)
		{
			return MakeTransientOATypeInternal(
				enumMethod,
				boxedPropertyTypes,
				CodeGenerationIDGenerator.GenerateClassNameUUID(),
				statementRawInfo,
				services);
		}

		private static ObjectArrayEventType MakeTransientOATypeInternal(
			string enumMethod,
			IDictionary<string, object> boxedPropertyTypes,
			string eventTypeNameUUid,
			StatementRawInfo statementRawInfo,
			StatementCompileTimeServices services)
		{
			var eventTypeName = 
				services.EventTypeNameGeneratorStatement.GetAnonymousTypeNameEnumMethod(enumMethod, eventTypeNameUUid);
			var metadata = new EventTypeMetadata(
				eventTypeName,
				statementRawInfo.ModuleName,
				EventTypeTypeClass.ENUMDERIVED,
				EventTypeApplicationType.OBJECTARR,
				NameAccessModifier.TRANSIENT,
				EventTypeBusModifier.NONBUS,
				false,
				EventTypeIdPair.Unassigned());
			var oatype = BaseNestableEventUtil.MakeOATypeCompileTime(
				metadata,
				boxedPropertyTypes,
				null,
				null,
				null,
				null,
				services.BeanEventTypeFactoryPrivate,
				services.EventTypeCompileTimeResolver);
			services.EventTypeCompileTimeRegistry.NewType(oatype);
			return oatype;
		}

		public static bool IsDatetimeOrEnumMethod(
			string name,
			ImportServiceCompileTime importService)
		{
			return EnumMethodResolver.IsEnumerationMethod(name, importService) ||
			       DatetimeMethodResolver.IsDateTimeMethod(name, importService);
		}

		public static ExprDotEnumerationSourceForge GetEnumerationSource(
			ExprNode inputExpression,
			StreamTypeService streamTypeService,
			bool hasEnumerationMethod,
			bool disablePropertyExpressionEventCollCache,
			StatementRawInfo statementRawInfo,
			StatementCompileTimeServices compileTimeServices)
		{
			var rootNodeForge = inputExpression.Forge;
			ExprEnumerationForge rootLambdaForge = null;
			EPType info = null;

			if (rootNodeForge is ExprEnumerationForge) {
				rootLambdaForge = (ExprEnumerationForge) rootNodeForge;
				var eventTypeCollection = rootLambdaForge.GetEventTypeCollection(statementRawInfo, compileTimeServices);
				if (eventTypeCollection != null) {
					info = EPTypeHelper.CollectionOfEvents(eventTypeCollection);
				}

				if (info == null) {
					var eventTypeSingle = rootLambdaForge.GetEventTypeSingle(statementRawInfo, compileTimeServices);
					if (eventTypeSingle != null) {
						info = EPTypeHelper.SingleEvent(eventTypeSingle);
					}
				}

				if (info == null) {
					var componentType = rootLambdaForge.ComponentTypeCollection;
					if (componentType != null) {
						info = EPTypeHelper.CollectionOfSingleValue(
							rootLambdaForge.ComponentTypeCollection,
							typeof(ICollection<>).MakeGenericType(componentType));
					}
				}

				if (info == null) {
					rootLambdaForge = null; // not a lambda evaluator
				}
			}
			else if (inputExpression is ExprIdentNode) {
				var identNode = (ExprIdentNode) inputExpression;
				var streamId = identNode.StreamId;
				var streamType = streamTypeService.EventTypes[streamId];
				return GetPropertyEnumerationSource(
					identNode.ResolvedPropertyName,
					streamId,
					streamType,
					hasEnumerationMethod,
					disablePropertyExpressionEventCollCache);
			}

			return new ExprDotEnumerationSourceForge(info, null, rootLambdaForge);
		}

		public static ExprDotEnumerationSourceForgeForProps GetPropertyEnumerationSource(
			string propertyName,
			int streamId,
			EventType streamType,
			bool allowEnumType,
			bool disablePropertyExpressionEventCollCache)
		{

			var propertyType = streamType.GetPropertyType(propertyName);
			var typeInfo = EPTypeHelper.SingleValue(propertyType); // assume scalar for now

			// no enumeration methods, no need to expose as an enumeration
			if (!allowEnumType) {
				return new ExprDotEnumerationSourceForgeForProps(null, typeInfo, streamId, null);
			}

			var fragmentEventType = streamType.GetFragmentType(propertyName);
			var getter = ((EventTypeSPI) streamType).GetGetterSPI(propertyName);

			ExprEnumerationForge enumEvaluator = null;
			if (getter != null && fragmentEventType != null) {
				if (fragmentEventType.IsIndexed) {
					enumEvaluator = new PropertyDotEventCollectionForge(
						propertyName,
						streamId,
						fragmentEventType.FragmentType,
						getter,
						disablePropertyExpressionEventCollCache);
					typeInfo = EPTypeHelper.CollectionOfEvents(fragmentEventType.FragmentType);
				}
				else { // we don't want native to require an eventbean instance
					enumEvaluator = new PropertyDotEventSingleForge(streamId, fragmentEventType.FragmentType, getter);
					typeInfo = EPTypeHelper.SingleEvent(fragmentEventType.FragmentType);
				}
			}
			else {
				var desc = EventTypeUtility.GetNestablePropertyDescriptor(streamType, propertyName);
				if (desc != null && desc.IsIndexed && !desc.IsRequiresIndex && desc.PropertyComponentType != null) {
					if (propertyType == typeof(string)) {
						enumEvaluator = new PropertyDotScalarStringForge(
							propertyName,
							streamId,
							getter);
					}
					else if (propertyType.IsArray) {
						enumEvaluator = new PropertyDotScalarArrayForge(
							propertyName,
							streamId,
							getter,
							desc.PropertyComponentType,
							desc.PropertyType);
					}
					else if (propertyType.IsGenericCollection()) {
						enumEvaluator = new PropertyDotScalarCollection(
							propertyName,
							streamId,
							getter,
							desc.PropertyComponentType);
					}
					else if (propertyType.IsGenericEnumerable()) {
						enumEvaluator = new PropertyDotScalarIterable(
							propertyName,
							streamId,
							getter,
							desc.PropertyComponentType,
							propertyType);
					}
					else {
						throw new IllegalStateException(
							"Property indicated indexed-type but failed to find proper collection adapter for use with enumeration methods");
					}

					typeInfo = EPTypeHelper.CollectionOfSingleValue(desc.PropertyComponentType);
				}
			}

			return new ExprDotEnumerationSourceForgeForProps(enumEvaluator, typeInfo, streamId, (ExprEnumerationGivenEventForge) enumEvaluator);
		}

		public static ExprDotEval[] GetEvaluators(ExprDotForge[] forges)
		{
			var evals = new ExprDotEval[forges.Length];
			for (var i = 0; i < forges.Length; i++) {
				evals[i] = forges[i].DotEvaluator;
			}

			return evals;
		}

		public static object EvaluateChain(
			ExprDotForge[] forges,
			ExprDotEval[] evaluators,
			object inner,
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext context)
		{
			foreach (var methodEval in evaluators) {
				inner = methodEval.Evaluate(inner, eventsPerStream, isNewData, context);
				if (inner == null) {
					break;
				}
			}

			return inner;
		}

		public static ExprDotNodeRealizedChain GetChainEvaluators(
			int? streamOfProviderIfApplicable,
			EPType inputType,
			IList<Chainable> chainSpec,
			ExprValidationContext validationContext,
			bool isDuckTyping,
			ExprDotNodeFilterAnalyzerInput inputDesc)
		{
			var methodForges = new List<ExprDotForge>();
			var currentInputType = inputType;
			EnumMethodDesc lastLambdaFunc = null;
			var lastElement = chainSpec.IsEmpty() ? null : chainSpec[chainSpec.Count - 1];
			FilterExprAnalyzerAffector filterAnalyzerDesc = null;

			Deque<Chainable> chainSpecStack = new ArrayDeque<Chainable>(chainSpec);
			while (!chainSpecStack.IsEmpty()) {
				var chainElement = chainSpecStack.RemoveFirst();
				var parameters = chainElement.GetParametersOrEmpty();
				var chainElementName = chainElement.GetRootNameOrEmptyString();
				var last = chainSpecStack.IsEmpty();
				lastLambdaFunc = null; // reset

				// compile parameters for chain element
				var paramForges = new ExprForge[parameters.Count];
				var paramTypes = new Type[parameters.Count];
				for (var i = 0; i < parameters.Count; i++) {
					paramForges[i] = parameters[i].Forge;
					paramTypes[i] = paramForges[i].EvaluationType;
				}

				// check if special 'size' method
				if (currentInputType is ClassMultiValuedEPType) {
					var type = (ClassMultiValuedEPType) currentInputType;
					if (string.Equals(chainElementName, "size", StringComparison.InvariantCultureIgnoreCase) &&
					    paramTypes.Length == 0 &&
					    lastElement == chainElement) {
						ExprDotForge size;
						var containerType = ((ClassMultiValuedEPType) currentInputType).Container;
						if (containerType.IsArray) {
							size = new ExprDotForgeSizeArray();
						}
						else {
							size = new ExprDotForgeSizeCollection();
						}

						methodForges.Add(size);
						currentInputType = size.TypeInfo;
						continue;
					}

					if (string.Equals(chainElementName, "get", StringComparison.InvariantCultureIgnoreCase) &&
					    paramTypes.Length == 1 &&
					    paramTypes[0].GetBoxedType() == typeof(int?)) {
						ExprDotForge get;
						var componentType = type.Component.GetBoxedType();
						if (type.Container.IsArray) {
							get = new ExprDotForgeGetArray(paramForges[0], componentType);
						}
						else {
							get = new ExprDotForgeGetCollection(paramForges[0], componentType);
						}

						methodForges.Add(get);
						currentInputType = get.TypeInfo;
						continue;
					}

					if (chainElement is ChainableArray && type.Container.IsArray) {
						var array = (ChainableArray) chainElement;
						var typeInfo = currentInputType;
						var indexExpr = ChainableArray.ValidateSingleIndexExpr(
							array.Indexes,
							() => "operation on type " + typeInfo.ToTypeDescriptive());
						var componentType = type.Component.GetBoxedType();
						ExprDotForgeGetArray get = new ExprDotForgeGetArray(indexExpr.Forge, componentType);
						methodForges.Add(get);
						currentInputType = get.TypeInfo;
						continue;
					}
				}

				// determine if there is a matching method
				var matchingMethod = false;
				var methodTarget = GetMethodTarget(currentInputType);
				if (methodTarget != null && (!(chainElement is ChainableArray))) {
					try {
						GetValidateMethodDescriptor(methodTarget, chainElementName, parameters, validationContext);
						matchingMethod = true;
					}
					catch (ExprValidationException) {
						// expected
					}
				}

				if (EnumMethodResolver.IsEnumerationMethod(chainElementName, validationContext.ImportService) &&
				    (!matchingMethod || methodTarget.IsArray || methodTarget.IsGenericCollection())) {
					var enumerationMethod = EnumMethodResolver.FromName(chainElementName, validationContext.ImportService);
					ExprDotForgeEnumMethod eval = enumerationMethod.Factory.Invoke(chainElement.GetParametersOrEmpty().Count);
					if (currentInputType is ClassEPType classEpType && classEpType.Clazz.IsGenericCollection()) {
						currentInputType = EPTypeHelper.CollectionOfSingleValue(typeof(object));
					}

					eval.Init(streamOfProviderIfApplicable, enumerationMethod, chainElementName, currentInputType, parameters, validationContext);
					currentInputType = eval.TypeInfo;
					if (currentInputType == null) {
						throw new IllegalStateException("Enumeration method '" + chainElementName + "' has not returned type information");
					}

					methodForges.Add(eval);
					lastLambdaFunc = enumerationMethod;
					continue;
				}

				// resolve datetime
				if (DatetimeMethodResolver.IsDateTimeMethod(chainElementName, validationContext.ImportService) &&
				    (!matchingMethod || 
				     methodTarget == typeof(DateTimeEx) ||
				     methodTarget == typeof(DateTimeOffset) ||
				     methodTarget == typeof(DateTime))) {
					DatetimeMethodDesc datetimeMethod = DatetimeMethodResolver.FromName(chainElementName, validationContext.ImportService);
					var datetimeImpl = ExprDotDTFactory.ValidateMake(
						validationContext.StreamTypeService,
						chainSpecStack,
						datetimeMethod,
						chainElementName,
						currentInputType,
						parameters,
						inputDesc,
						validationContext.ImportService.TimeAbacus,
						validationContext.TableCompileTimeResolver,
						validationContext.ImportService,
						validationContext.StatementRawInfo);
					currentInputType = datetimeImpl.ReturnType;
					if (currentInputType == null) {
						throw new IllegalStateException(
							"Date-time method '" + chainElementName + "' has not returned type information");
					}

					methodForges.Add(datetimeImpl.Forge);
					filterAnalyzerDesc = datetimeImpl.IntervalFilterDesc;
					continue;
				}

				// try to resolve as property if the last method returned a type
				if (currentInputType is EventEPType) {
					if (chainElement is ChainableArray) {
						throw new ExprValidationException("Could not perform array operation on type " + currentInputType.ToTypeDescriptive());
					}

					var inputEventType = (EventTypeSPI) ((EventEPType) currentInputType).EventType;
					var type = inputEventType.GetPropertyType(chainElementName);
					var getter = inputEventType.GetGetterSPI(chainElementName);
					var fragmentType = inputEventType.GetFragmentType(chainElementName);
					ExprDotForge forge;
					if (type != null && getter != null) {
						if (fragmentType == null || last) {
							forge = new ExprDotForgeProperty(getter, EPTypeHelper.SingleValue(type.GetBoxedType()));
							currentInputType = forge.TypeInfo;
						}
						else {
							if (!fragmentType.IsIndexed) {
								currentInputType = EPTypeHelper.SingleEvent(fragmentType.FragmentType);
							}
							else {
								currentInputType = EPTypeHelper.ArrayOfEvents(fragmentType.FragmentType);
							}

							forge = new ExprDotForgePropertyFragment(getter, currentInputType);
						}

						methodForges.Add(forge);
						continue;
					}
				}

				if (currentInputType is EventMultiValuedEPType eventMultiValuedEpType && chainElement is ChainableArray chainableArray) {
					var inputEventType = (EventTypeSPI) eventMultiValuedEpType.Component;
					var typeInfo = currentInputType;
					var indexExpr = ChainableArray.ValidateSingleIndexExpr(
						chainableArray.Indexes,
						() => "operation on type " + typeInfo.ToTypeDescriptive());
					currentInputType = EPTypeHelper.SingleEvent(inputEventType);
					ExprDotForgeEventArrayAtIndex forge = new ExprDotForgeEventArrayAtIndex(currentInputType, indexExpr);
					methodForges.Add(forge);
					continue;
				}

				// Finally try to resolve the method
				if (methodTarget != null && !(chainElement is ChainableArray)) {
					try {
						// find descriptor again, allow for duck typing
						var desc = GetValidateMethodDescriptor(methodTarget, chainElementName, parameters, validationContext);
						paramForges = desc.ChildForges;
						ExprDotForge forge;
						if (currentInputType is ClassEPType) {
							// if followed by an enumeration method, convert array to collection
							if (desc.ReflectionMethod.ReturnType.IsArray &&
							    !chainSpecStack.IsEmpty() &&
							    EnumMethodResolver.IsEnumerationMethod(chainSpecStack.First.GetRootNameOrEmptyString(), validationContext.ImportService)) {
								forge = new ExprDotMethodForgeNoDuck(
									validationContext.StatementName,
									desc.ReflectionMethod,
									paramForges,
									ExprDotMethodForgeNoDuck.DuckType.WRAPARRAY);
							}
							else {
								forge = new ExprDotMethodForgeNoDuck(
									validationContext.StatementName,
									desc.ReflectionMethod,
									paramForges,
									ExprDotMethodForgeNoDuck.DuckType.PLAIN);
							}
						}
						else {
							forge = new ExprDotMethodForgeNoDuck(
								validationContext.StatementName,
								desc.ReflectionMethod,
								paramForges,
								ExprDotMethodForgeNoDuck.DuckType.UNDERLYING);
						}

						methodForges.Add(forge);
						currentInputType = forge.TypeInfo;
					}
					catch (Exception e) {
						if (!isDuckTyping) {
							throw new ExprValidationException(e.Message, e);
						}

						var duck = new ExprDotMethodForgeDuck(
							validationContext.StatementName,
							validationContext.ImportService,
							chainElementName,
							paramTypes,
							paramForges);
						methodForges.Add(duck);
						currentInputType = duck.TypeInfo;
					}

					continue;
				}

				string message;
				if (!(chainElement is ChainableArray)) {
					message = "Could not find event property or method named '" + chainElementName + "' in " + currentInputType.ToTypeDescriptive();
				}
				else {
					message = "Could not perform array operation on type " + currentInputType.ToTypeDescriptive();
				}

				throw new ExprValidationException(message);
			}

			var intermediateEvals = methodForges.ToArray();

			if (lastLambdaFunc != null) {
				ExprDotForge finalEval = null;
				if (currentInputType is EventMultiValuedEPType  mvType) {
					var tableMetadata = validationContext.TableCompileTimeResolver.ResolveTableFromEventType(mvType.Component);
					if (tableMetadata != null) {
						finalEval = new ExprDotForgeUnpackCollEventBeanTable(mvType.Component, tableMetadata);
					}
					else {
						finalEval = new ExprDotForgeUnpackCollEventBean(mvType.Component);
					}
				}
				else if (currentInputType is EventEPType epType) {
					var tableMetadata = validationContext.TableCompileTimeResolver.ResolveTableFromEventType(epType.EventType);
					if (tableMetadata != null) {
						finalEval = new ExprDotForgeUnpackBeanTable(epType.EventType, tableMetadata);
					}
					else {
						finalEval = new ExprDotForgeUnpackBean(epType.EventType);
					}
				}

				if (finalEval != null) {
					methodForges.Add(finalEval);
				}
			}

			var unpackingForges = methodForges.ToArray();
			return new ExprDotNodeRealizedChain(intermediateEvals, unpackingForges, filterAnalyzerDesc);
		}

		private static Type GetMethodTarget(EPType currentInputType)
		{
			if (currentInputType is ClassEPType classEpType) {
				return classEpType.Clazz;
			}
			else if (currentInputType is EventEPType eventEpType) {
				return eventEpType.EventType.UnderlyingType;
			}

			return null;
		}

		public static object EvaluateChainWithWrap(
			ExprDotStaticMethodWrap resultWrapLambda,
			object result,
			EventType optionalResultSingleEventType,
			Type resultType,
			ExprDotEval[] chainEval,
			ExprDotForge[] chainForges,
			EventBean[] eventsPerStream,
			bool newData,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			if (result == null) {
				return null;
			}

			if (resultWrapLambda != null) {
				result = resultWrapLambda.ConvertNonNull(result);
			}

			foreach (var aChainEval in chainEval) {
				result = aChainEval.Evaluate(result, eventsPerStream, newData, exprEvaluatorContext);
				if (result == null) {
					return null;
				}
			}

			return result;
		}

		public static CodegenExpression EvaluateChainCodegen(
			CodegenMethod parent,
			ExprForgeCodegenSymbol exprSymbol,
			CodegenClassScope codegenClassScope,
			CodegenExpression inner,
			Type innerType,
			ExprDotForge[] forges,
			ExprDotStaticMethodWrap optionalResultWrapLambda)
		{
			if (forges.Length == 0) {
				return inner;
			}

			var last = forges[forges.Length - 1];
			var lastType = last.TypeInfo.GetCodegenReturnType().GetBoxedType();
			var methodNode = parent.MakeChild(lastType, typeof(ExprDotNodeUtility), codegenClassScope).AddParam(innerType, "inner");

			var block = methodNode.Block.DebugStack();
			var currentTarget = "wrapped";
			Type currentTargetType;
			if (optionalResultWrapLambda != null) {
				currentTargetType = optionalResultWrapLambda.TypeInfo.GetCodegenReturnType();

				var wrapped = optionalResultWrapLambda.CodegenConvertNonNull(Ref("inner"), methodNode, codegenClassScope);
				if (currentTargetType == typeof(FlexCollection)) {
					wrapped = FlexWrap(wrapped);
				}
				
				if (lastType != typeof(void)) {
					block.IfRefNullReturnNull("inner");
				}

				block.DeclareVar(currentTargetType, "wrapped", wrapped);
			}
			else if (innerType == typeof(FlexCollection)) {
				block.DeclareVar(innerType, "wrapped", FlexWrap(Ref("inner")));
				currentTargetType = innerType;
			}
			else {
				block.DeclareVar(innerType, "wrapped", Ref("inner"));
				currentTargetType = innerType;
			}

			string refname = null;
			var instrumentationName = new ExprDotEvalVisitorImpl();
			for (var i = 0; i < forges.Length; i++) {
				refname = "r" + i;
				forges[i].Visit(instrumentationName);
				block.Apply(
					Instblock(
						codegenClassScope,
						"qExprDotChainElement",
						Constant(i),
						Constant(instrumentationName.MethodType),
						Constant(instrumentationName.MethodName)));

				var typeInformation = ConstantNull();
				if (codegenClassScope.IsInstrumented) {
					typeInformation = codegenClassScope.AddOrGetDefaultFieldSharable(new EPTypeCodegenSharable(forges[i].TypeInfo, codegenClassScope));
				}

				var reftype = forges[i].TypeInfo.GetCodegenReturnType().GetBoxedType();
				var invocation = forges[i]
					.Codegen(
						Ref(currentTarget),
						currentTargetType,
						methodNode,
						exprSymbol,
						codegenClassScope);
				
				// We can't assume anything about the return type.  Unfortunately, we do not know the resultant type
				// of the forge... I must fix this. - TBD
				invocation = CodegenLegoCast.CastSafeFromObjectType(reftype, invocation);
				
				if (reftype == typeof(void)) {
					block
						.Expression(invocation)
						.Apply(Instblock(codegenClassScope, "aExprDotChainElement", typeInformation, ConstantNull()));
				}
				else {
					block.DeclareVar(reftype, refname, invocation);
					currentTarget = refname;
					currentTargetType = reftype;
					if (reftype.CanBeNull()) {
						var ifBlock = block
							.IfRefNull(refname)
							.Apply(Instblock(codegenClassScope, "aExprDotChainElement", typeInformation, ConstantNull()));
						if (lastType != typeof(void)) {
							ifBlock.BlockReturn(ConstantNull());
						}
						else {
							ifBlock.BlockEnd();
						}
					}

					block.Apply(Instblock(codegenClassScope, "aExprDotChainElement", typeInformation, Ref(refname)));
				}
			}

			if (lastType == typeof(void)) {
				block.MethodEnd();
			}
			else {
				block.MethodReturn(Ref(refname));
			}

			return LocalMethod(methodNode, inner);
		}

		private static ExprNodeUtilMethodDesc GetValidateMethodDescriptor(
			Type methodTarget,
			string methodName,
			IList<ExprNode> parameters,
			ExprValidationContext validationContext)
		{
			ExprNodeUtilResolveExceptionHandler exceptionHandler = new ProxyExprNodeUtilResolveExceptionHandler(
				e => new ExprValidationException("Failed to resolve method '" + methodName + "': " + e.Message, e));

			var wildcardType = validationContext.StreamTypeService.EventTypes.Length != 1 ? null : validationContext.StreamTypeService.EventTypes[0];
			return ExprNodeUtilityResolve.ResolveMethodAllowWildcardAndStream(
				methodTarget.FullName,
				methodTarget,
				methodName,
				parameters,
				wildcardType != null,
				wildcardType,
				exceptionHandler,
				methodName,
				validationContext.StatementRawInfo,
				validationContext.StatementCompileTimeService);
		}
	}
} // end of namespace
