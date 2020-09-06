///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.@event.propertyparser;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
	/// <summary>
	/// Represents an Dot-operator expression, for use when "(expression).method(...).method(...)"
	/// </summary>
	public class ExprDotNodeImpl : ExprNodeBase,
		ExprDotNode,
		ExprStreamRefNode,
		ExprNodeInnerNodeProvider
	{
		private IList<Chainable> _chainSpec;
		private readonly bool _isDuckTyping;
		private readonly bool _isUdfCache;

		[NonSerialized] private ExprDotNodeForge _forge;

		public ExprDotNodeImpl(
			IList<Chainable> chainSpec,
			bool isDuckTyping,
			bool isUDFCache)
		{
			_chainSpec = chainSpec.AsReadOnlyList(); // for safety, make it unmodifiable the list
			_isDuckTyping = isDuckTyping;
			_isUdfCache = isUDFCache;
		}

		public override ExprNode Validate(ExprValidationContext validationContext)
		{
			// check for plannable methods: these are validated according to different rules
			var appDotMethod = GetAppDotMethod(validationContext.IsFilterExpression);
			if (appDotMethod != null) {
				return appDotMethod;
			}

			// determine if there is an implied binding, replace first chain element with evaluation node if there is
			if (validationContext.StreamTypeService.HasTableTypes &&
			    validationContext.TableCompileTimeResolver != null &&
			    _chainSpec.Count > 1 &&
			    _chainSpec[0] is ChainableName) {
				var tableNode = TableCompileTimeUtil.GetTableNodeChainable(
					validationContext.StreamTypeService,
					_chainSpec,
					validationContext.IsAllowTableAggReset,
					validationContext.TableCompileTimeResolver);
				if (tableNode != null) {
					var node = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.DOTNODE, tableNode.First, validationContext);
					if (tableNode.Second.IsEmpty()) {
						return node;
					}

					IList<Chainable> modifiedChainX = new List<Chainable>(tableNode.Second);
					ChainSpec = modifiedChainX;
					AddChildNode(node);
				}
			}

			// handle aggregation methods: method on aggregation state coming from certain aggregations or from table column (both table-access or table-in-from-clause)
			// this is done here as a walker does not have the information that the validated child node has
			var aggregationMethodNode = HandleAggregationMethod(validationContext);
			if (aggregationMethodNode != null) {
				if (aggregationMethodNode.Second.IsEmpty()) {
					return aggregationMethodNode.First;
				}

				IList<Chainable> modifiedChainX = new List<Chainable>(aggregationMethodNode.Second);
				ChainSpec = modifiedChainX;
				ChildNodes[0] = aggregationMethodNode.First;
			}

			// validate all parameters
			ExprNodeUtilityValidate.Validate(ExprNodeOrigin.DOTNODEPARAMETER, _chainSpec, validationContext);

			// determine if there are enumeration method expressions in the chain
			var hasEnumerationMethod = false;
			foreach (var chain in _chainSpec) {
				if (!(chain is ChainableCall)) {
					continue;
				}

				var call = (ChainableCall) chain;
				if (EnumMethodResolver.IsEnumerationMethod(call.Name, validationContext.ImportService)) {
					hasEnumerationMethod = true;
					break;
				}
			}

			// The root node expression may provide the input value:
			//   Such as "window(*).DoIt(...)" or "(select * from Window).DoIt()" or "prevwindow(sb).DoIt(...)",
			//   in which case the expression to act on is a child expression
			//
			var streamTypeService = validationContext.StreamTypeService;
			if (ChildNodes.Length != 0) {
				// the root expression is the first child node
				var rootNode = ChildNodes[0];

				// the root expression may also provide a lambda-function input (Iterator<EventBean>)
				// Determine collection-type and evaluator if any for root node
				var enumSrc = ExprDotNodeUtility.GetEnumerationSource(
					rootNode,
					validationContext.StreamTypeService,
					hasEnumerationMethod,
					validationContext.IsDisablePropertyExpressionEventCollCache,
					validationContext.StatementRawInfo,
					validationContext.StatementCompileTimeService);

				EPType typeInfoX;
				if (enumSrc.ReturnType == null) {
					typeInfoX = EPTypeHelper.SingleValue(rootNode.Forge.EvaluationType); // not a collection type, treat as scalar
				}
				else {
					typeInfoX = enumSrc.ReturnType;
				}

				var evalsX = ExprDotNodeUtility.GetChainEvaluators(
					enumSrc.StreamOfProviderIfApplicable,
					typeInfoX,
					_chainSpec,
					validationContext,
					_isDuckTyping,
					new ExprDotNodeFilterAnalyzerInputExpr());
				_forge = new ExprDotNodeForgeRootChild(
					this,
					null,
					null,
					null,
					hasEnumerationMethod,
					rootNode.Forge,
					enumSrc.Enumeration,
					typeInfoX,
					evalsX.Chain,
					evalsX.ChainWithUnpack,
					false);
				return null;
			}

			// No root node, and this is a 1-element chain i.e. "something(param,...)".
			// Plug-in single-row methods are not handled here.
			// Plug-in aggregation methods are not handled here.
			if (_chainSpec.Count == 1) {
				var chainable = _chainSpec[0];
				if (!(chainable is ChainableCall)) {
					throw new IllegalStateException("Unexpected chainable : " + chainable);
				}

				var call = (ChainableCall) chainable;
				if (call.Parameters.IsEmpty()) {
					throw HandleNotFound(call.Name);
				}

				// single-parameter can resolve to a property
				Pair<PropertyResolutionDescriptor, string> propertyInfoPairX = null;
				try {
					propertyInfoPairX = ExprIdentNodeUtil.GetTypeFromStream(
						streamTypeService,
						call.Name,
						streamTypeService.HasPropertyAgnosticType,
						false,
						validationContext.TableCompileTimeResolver);
				}
				catch (ExprValidationPropertyException) {
					// fine
				}

				// if not a property then try built-in single-row non-grammar functions
				if (propertyInfoPairX == null && 
				    call.Name.Equals(ImportServiceCompileTimeConstants.EXT_SINGLEROW_FUNCTION_TRANSPOSE, StringComparison.InvariantCultureIgnoreCase)) {
					if (call.Parameters.Count != 1) {
						throw new ExprValidationException(
							"The " + ImportServiceCompileTimeConstants.EXT_SINGLEROW_FUNCTION_TRANSPOSE + " function requires a single parameter expression");
					}

					_forge = new ExprDotNodeForgeTransposeAsStream(this, call.Parameters[0].Forge);
				}
				else if (call.Parameters.Count != 1) {
					throw HandleNotFound(call.Name);
				}
				else {
					if (propertyInfoPairX == null) {
						throw new ExprValidationException(
							"Unknown single-row function, aggregation function or mapped or indexed property named '" + call.Name + "' could not be resolved");
					}

					_forge = GetPropertyPairEvaluator(call.Parameters[0].Forge, propertyInfoPairX, validationContext);
				}

				return null;
			}

			// handle the case where the first chain spec element is a stream name.
			ExprValidationException prefixedStreamNumException = null;
			var prefixedStreamNumber = PrefixedStreamName(_chainSpec, validationContext.StreamTypeService);
			if (prefixedStreamNumber != -1) {

				var first = (ChainableName) _chainSpec[0];
				var specAfterStreamName = _chainSpec[1];

				// Attempt to resolve as property
				Pair<PropertyResolutionDescriptor, string> propertyInfoPairX = null;
				try {
					var propName = first.Name + "." + specAfterStreamName.GetRootNameOrEmptyString();
					propertyInfoPairX = ExprIdentNodeUtil.GetTypeFromStream(
						streamTypeService,
						propName,
						streamTypeService.HasPropertyAgnosticType,
						true,
						validationContext.TableCompileTimeResolver);
				}
				catch (ExprValidationPropertyException) {
					// fine
				}

				if (propertyInfoPairX != null) {
					IList<Chainable> chain = new List<Chainable>(_chainSpec);
					// handle "property[x]" and "property(x)"
					if (chain.Count == 2 && specAfterStreamName.GetParametersOrEmpty().Count == 1) {
						_forge = GetPropertyPairEvaluator(specAfterStreamName.GetParametersOrEmpty()[0].Forge, propertyInfoPairX, validationContext);
						return null;
					}

					chain.RemoveAt(0);
					chain.RemoveAt(0);
					var desc = HandlePropertyInfoPair(
						true,
						specAfterStreamName,
						chain,
						propertyInfoPairX,
						hasEnumerationMethod,
						validationContext,
						this);
					desc.Apply(this);
					return null;
				}

				// Attempt to resolve as event-underlying object instance method
				var eventType = validationContext.StreamTypeService.EventTypes[prefixedStreamNumber];
				var type = eventType.UnderlyingType;

				IList<Chainable> remainderChain = new List<Chainable>(_chainSpec);
				remainderChain.RemoveAt(0);

				ExprValidationException methodEx = null;
				ExprDotForge[] underlyingMethodChain = null;
				try {
					var typeInfoX = EPTypeHelper.SingleValue(type);
					if (validationContext.TableCompileTimeResolver.ResolveTableFromEventType(eventType) != null) {
						typeInfoX = new ClassEPType(typeof(object[]));
					}

					underlyingMethodChain = ExprDotNodeUtility.GetChainEvaluators(
							prefixedStreamNumber,
							typeInfoX,
							remainderChain,
							validationContext,
							false,
							new ExprDotNodeFilterAnalyzerInputStream(prefixedStreamNumber))
						.ChainWithUnpack;
				}
				catch (ExprValidationException ex) {
					methodEx = ex;
					// expected - may not be able to find the methods on the underlying
				}

				ExprDotForge[] eventTypeMethodChain = null;
				ExprValidationException enumDatetimeEx = null;
				FilterExprAnalyzerAffector filterExprAnalyzerAffector = null;
				try {
					var typeInfoX = EPTypeHelper.SingleEvent(eventType);
					var chain = ExprDotNodeUtility.GetChainEvaluators(
						prefixedStreamNumber,
						typeInfoX,
						remainderChain,
						validationContext,
						false,
						new ExprDotNodeFilterAnalyzerInputStream(prefixedStreamNumber));
					eventTypeMethodChain = chain.ChainWithUnpack;
					filterExprAnalyzerAffector = chain.FilterAnalyzerDesc;
				}
				catch (ExprValidationException ex) {
					enumDatetimeEx = ex;
					// expected - may not be able to find the methods on the underlying
				}

				if (underlyingMethodChain != null) {
					_forge = new ExprDotNodeForgeStream(
						this, filterExprAnalyzerAffector, prefixedStreamNumber, eventType, underlyingMethodChain, true);
				}
				else if (eventTypeMethodChain != null) {
					_forge = new ExprDotNodeForgeStream(
						this, filterExprAnalyzerAffector, prefixedStreamNumber, eventType, eventTypeMethodChain, false);
				}

				if (_forge != null) {
					return null;
				}
				else {
					var remainerName = remainderChain[0].GetRootNameOrEmptyString();
					if (ExprDotNodeUtility.IsDatetimeOrEnumMethod(remainerName, validationContext.ImportService)) {
						prefixedStreamNumException = enumDatetimeEx;
					}
					else {
						prefixedStreamNumException = new ExprValidationException(
							"Failed to solve '" +
							remainerName +
							"' to either an date-time or enumeration method, an event property or a method on the event underlying object: " +
							methodEx.Message,
							methodEx);
					}
				}
			}

			// There no root node, in this case the classname or property name is provided as part of the chain.
			// Such as "MyClass.myStaticLib(...)" or "mycollectionproperty.DoIt(...)"
			//
			IList<Chainable> modifiedChain = new List<Chainable>(_chainSpec);
			var firstItem = modifiedChain.DeleteAt(0);
			var firstItemName = firstItem.GetRootNameOrEmptyString();

			Pair<PropertyResolutionDescriptor, string> propertyInfoPair = null;
			try {
				propertyInfoPair = ExprIdentNodeUtil.GetTypeFromStream(
					streamTypeService,
					firstItemName,
					streamTypeService.HasPropertyAgnosticType,
					true,
					validationContext.TableCompileTimeResolver);
			}
			catch (ExprValidationPropertyException) {
				// not a property
			}

			// If property then treat it as such
			if (propertyInfoPair != null) {
				var desc = HandlePropertyInfoPair(
					false,
					firstItem,
					modifiedChain,
					propertyInfoPair,
					hasEnumerationMethod,
					validationContext,
					this);
				desc.Apply(this);
				return null;
			}

			// If variable then resolve as such
			var variable = validationContext.VariableCompileTimeResolver.Resolve(firstItemName);
			if (variable != null) {
				if (variable.OptionalContextName != null) {
					throw new ExprValidationException("Method invocation on context-specific variable is not supported");
				}

				EPType typeInfoX;
				ExprDotStaticMethodWrap wrap;
				if (variable.Type.IsArray) {
					typeInfoX = EPTypeHelper.CollectionOfSingleValue(
						variable.Type.GetElementType(),
						variable.Type);
					wrap = new ExprDotStaticMethodWrapArrayScalar(variable.VariableName, variable.Type);
				}
				else if (variable.EventType != null) {
					typeInfoX = EPTypeHelper.SingleEvent(variable.EventType);
					wrap = null;
				}
				else {
					typeInfoX = EPTypeHelper.SingleValue(variable.Type);
					wrap = null;
				}

				var evalsX = ExprDotNodeUtility.GetChainEvaluators(
					null,
					typeInfoX,
					modifiedChain,
					validationContext,
					false,
					new ExprDotNodeFilterAnalyzerInputStatic());
				_forge = new ExprDotNodeForgeVariable(this, variable, wrap, evalsX.ChainWithUnpack);
				return null;
			}

			// try resolve as enumeration class with value
			var enumconstantDesc = ImportCompileTimeUtil.ResolveIdentAsEnumConst(
				firstItemName,
				validationContext.ImportService,
				validationContext.ClassProvidedExtension,
				false);
			if (enumconstantDesc != null && modifiedChain[0] is ChainableCall) {

				// try resolve method
				var methodSpec = (ChainableCall) modifiedChain[0];
				var enumvalue = firstItemName;
				ExprNodeUtilResolveExceptionHandler handler = new ProxyExprNodeUtilResolveExceptionHandler() {
					ProcHandle = (ex) => {
						return new ExprValidationException(
							"Failed to resolve method '" + methodSpec.Name + "' on enumeration value '" + enumvalue + "': " + ex.Message);
					},
				};
				var wildcardType = validationContext.StreamTypeService.EventTypes.Length != 1 ? null : validationContext.StreamTypeService.EventTypes[0];
				var methodDesc = ExprNodeUtilityResolve.ResolveMethodAllowWildcardAndStream(
					enumconstantDesc.Value.GetType().Name,
					enumconstantDesc.Value.GetType(),
					methodSpec.Name,
					methodSpec.Parameters,
					wildcardType != null,
					wildcardType,
					handler,
					methodSpec.Name,
					validationContext.StatementRawInfo,
					validationContext.StatementCompileTimeService);

				// method resolved, hook up
				modifiedChain.RemoveAt(0); // we identified this piece
				var optionalLambdaWrapX = ExprDotStaticMethodWrapFactory.Make(
					methodDesc.ReflectionMethod,
					modifiedChain,
					null,
					validationContext);
				var typeInfoX = optionalLambdaWrapX != null ? optionalLambdaWrapX.TypeInfo : EPTypeHelper.SingleValue(methodDesc.ReflectionMethod.ReturnType);

				var evalsX = ExprDotNodeUtility.GetChainEvaluators(
					null,
					typeInfoX,
					modifiedChain,
					validationContext,
					false,
					new ExprDotNodeFilterAnalyzerInputStatic());
				_forge = new ExprDotNodeForgeStaticMethod(
					this,
					false,
					firstItemName,
					methodDesc.ReflectionMethod,
					methodDesc.ChildForges,
					false,
					evalsX.ChainWithUnpack,
					optionalLambdaWrapX,
					false,
					enumconstantDesc,
					validationContext.StatementName,
					methodDesc.IsLocalInlinedClass);
				return null;
			}

			// if prefixed by a stream name, we are giving up
			if (prefixedStreamNumException != null) {
				throw prefixedStreamNumException;
			}

			// If class then resolve as class
			var secondItem = modifiedChain.DeleteAt(0);

			var allowWildcard = validationContext.StreamTypeService.EventTypes.Length == 1;
			EventType streamZeroType = null;
			if (validationContext.StreamTypeService.EventTypes.Length > 0) {
				streamZeroType = validationContext.StreamTypeService.EventTypes[0];
			}

			var secondItemName = secondItem.GetRootNameOrEmptyString();
			var separator = string.IsNullOrWhiteSpace(secondItemName) ? "" : ".";
			var msgHandler = new ExprNodeUtilResolveExceptionHandlerDefault(firstItemName + separator + secondItemName, false);
			var method = ExprNodeUtilityResolve.ResolveMethodAllowWildcardAndStream(
				firstItemName,
				null,
				secondItem.GetRootNameOrEmptyString(),
				secondItem.GetParametersOrEmpty(),
				allowWildcard,
				streamZeroType,
				msgHandler,
				secondItem.GetRootNameOrEmptyString(),
				validationContext.StatementRawInfo,
				validationContext.StatementCompileTimeService);

			var isConstantParameters = method.IsAllConstants && _isUdfCache;
			var isReturnsConstantResult = isConstantParameters && modifiedChain.IsEmpty();

			// this may return a pair of null if there is no lambda or the result cannot be wrapped for lambda-function use
			var optionalLambdaWrap = ExprDotStaticMethodWrapFactory.Make(method.ReflectionMethod, modifiedChain, null, validationContext);
			var typeInfo = optionalLambdaWrap != null ? optionalLambdaWrap.TypeInfo : EPTypeHelper.SingleValue(method.ReflectionMethod.ReturnType);

			var evals = ExprDotNodeUtility.GetChainEvaluators(
				null,
				typeInfo,
				modifiedChain,
				validationContext,
				false,
				new ExprDotNodeFilterAnalyzerInputStatic());
			_forge = new ExprDotNodeForgeStaticMethod(
				this,
				isReturnsConstantResult,
				firstItemName,
				method.ReflectionMethod,
				method.ChildForges,
				isConstantParameters,
				evals.ChainWithUnpack,
				optionalLambdaWrap,
				false,
				null,
				validationContext.StatementName,
				method.IsLocalInlinedClass);

			return null;
		}

		private static PropertyInfoPairDesc HandlePropertyInfoPair(
			bool nestedComplexProperty,
			Chainable firstItem,
			IList<Chainable> chain,
			Pair<PropertyResolutionDescriptor, string> propertyInfoPair,
			bool hasEnumerationMethod,
			ExprValidationContext validationContext,
			ExprDotNodeImpl myself)
		{
			var streamTypeService = validationContext.StreamTypeService;
			var propertyName = propertyInfoPair.First.PropertyName;
			var streamId = propertyInfoPair.First.StreamNum;
			var streamType = (EventTypeSPI) streamTypeService.EventTypes[streamId];
			ExprEnumerationForge enumerationForge = null;
			EPType inputType;
			ExprForge rootNodeForge = null;
			EventPropertyGetterSPI getter;
			var rootIsEventBean = false;

			if (firstItem is ChainableName) {
				getter = streamType.GetGetterSPI(propertyName);
				// Handle first-chainable not an array
				if (!(chain[0] is ChainableArray)) {
					var allowEnum = nestedComplexProperty || hasEnumerationMethod;
					var propertyEval = ExprDotNodeUtility.GetPropertyEnumerationSource(
						propertyName,
						streamId,
						streamType,
						allowEnum,
						validationContext.IsDisablePropertyExpressionEventCollCache);
					enumerationForge = propertyEval.Enumeration;
					inputType = propertyEval.ReturnType;
					rootNodeForge = new PropertyDotNonLambdaForge(streamId, getter, propertyInfoPair.First.PropertyType.GetBoxedType());
				}
				else {
					// first-chainable is an array, use array-of-fragments or array-of-type
					var array = (ChainableArray) chain[0];
					var indexExpression = ChainableArray.ValidateSingleIndexExpr(array.Indexes, () => "property '" + propertyName + "'");
					var propertyType = streamType.GetPropertyType(propertyName);
					var fragmentEventType = streamType.GetFragmentType(propertyName);
					if (fragmentEventType != null && fragmentEventType.IsIndexed) {
						// handle array-of-fragment by including the array operation in the root
						inputType = EPTypeHelper.SingleEvent(fragmentEventType.FragmentType);
						chain = chain.SubList(1, chain.Count); // we remove the array operation from the chain as its handled by the forge
						rootNodeForge = new PropertyDotNonLambdaFragmentIndexedForge(streamId, getter, indexExpression, propertyName);
						rootIsEventBean = true;
					}
					else if (propertyType.IsArray) {
						// handle array-of-type by simple property and array operation as part of chain
						inputType = EPTypeHelper.SingleValue(propertyType);
						rootNodeForge = new PropertyDotNonLambdaForge(streamId, getter, propertyInfoPair.First.PropertyType.GetBoxedType());
					}
					else {
						throw new ExprValidationException("Invalid array operation for property '" + propertyName + "'");
					}
				}
			}
			else {
				// property with parameter - mapped or indexed property
				getter = null;
				var call = (ChainableCall) firstItem;
				var desc = EventTypeUtility.GetNestablePropertyDescriptor(
					streamTypeService.EventTypes[propertyInfoPair.First.StreamNum],
					call.Name);
				if (call.Parameters.Count > 1) {
					throw new ExprValidationException("Property '" + call.Name + "' may not be accessed passing 2 or more parameters");
				}

				var paramEval = call.Parameters[0].Forge;
				inputType = EPTypeHelper.SingleValue(desc.PropertyComponentType);
				if (desc.IsMapped) {
					if (paramEval.EvaluationType != typeof(string)) {
						throw new ExprValidationException(
							"Parameter expression to mapped property '" +
							propertyName +
							"' is expected to return a string-type value but returns " +
							paramEval.EvaluationType.CleanName());
					}

					var mappedGetter = ((EventTypeSPI) propertyInfoPair.First.StreamEventType).GetGetterMappedSPI(propertyName);
					if (mappedGetter == null) {
						throw new ExprValidationException("Mapped property named '" + propertyName + "' failed to obtain getter-object");
					}

					rootNodeForge = new PropertyDotNonLambdaMappedForge(streamId, mappedGetter, paramEval, desc.PropertyComponentType);
				}

				if (desc.IsIndexed) {
					if (paramEval.EvaluationType.GetBoxedType() != typeof(int?)) {
						throw new ExprValidationException(
							"Parameter expression to mapped property '" +
							propertyName +
							"' is expected to return a Integer-type value but returns " +
							paramEval.EvaluationType.CleanName());
					}

					var indexedGetter = ((EventTypeSPI) propertyInfoPair.First.StreamEventType).GetGetterIndexedSPI(propertyName);
					if (indexedGetter == null) {
						throw new ExprValidationException("Mapped property named '" + propertyName + "' failed to obtain getter-object");
					}

					rootNodeForge = new PropertyDotNonLambdaIndexedForge(streamId, indexedGetter, paramEval, desc.PropertyComponentType);
				}
			}

			// try to build chain based on the input (non-fragment)
			ExprDotNodeRealizedChain evals;
			var filterAnalyzerInputProp = new ExprDotNodeFilterAnalyzerInputProp(propertyInfoPair.First.StreamNum, propertyName);
			try {
				evals = ExprDotNodeUtility.GetChainEvaluators(streamId, inputType, chain, validationContext, myself._isDuckTyping, filterAnalyzerInputProp);
			}
			catch (ExprValidationException) {
				if (inputType is EventEPType || inputType is EventMultiValuedEPType) {
					throw;
				}

				// try building the chain based on the fragment event type (i.e. A.after(B) based on A-configured start time where A is a fragment)
				var fragment = propertyInfoPair.First.FragmentEventType;
				if (fragment == null) {
					throw;
				}

				rootIsEventBean = true;
				EPType fragmentTypeInfo;
				if (!fragment.IsIndexed) {
					if (chain[0] is ChainableArray) {
						throw new ExprValidationException("Cannot perform array operation as property '" + propertyName + "' does not return an array");
					}

					fragmentTypeInfo = EPTypeHelper.SingleEvent(fragment.FragmentType);
				}
				else {
					fragmentTypeInfo = EPTypeHelper.ArrayOfEvents(fragment.FragmentType);
				}

				inputType = fragmentTypeInfo;
				rootNodeForge = new PropertyDotNonLambdaFragmentForge(streamId, getter, fragment.IsIndexed);
				evals = ExprDotNodeUtility.GetChainEvaluators(
					propertyInfoPair.First.StreamNum,
					fragmentTypeInfo,
					chain,
					validationContext,
					myself._isDuckTyping,
					filterAnalyzerInputProp);
			}

			var filterExprAnalyzerAffector = evals.FilterAnalyzerDesc;
			var streamNumReferenced = propertyInfoPair.First.StreamNum;
			var forge = new ExprDotNodeForgeRootChild(
				myself,
				filterExprAnalyzerAffector,
				streamNumReferenced,
				propertyName,
				hasEnumerationMethod,
				rootNodeForge,
				enumerationForge,
				inputType,
				evals.Chain,
				evals.ChainWithUnpack,
				!rootIsEventBean);
			return new PropertyInfoPairDesc(forge);
		}

		private Pair<ExprDotNodeAggregationMethodRootNode, IList<Chainable>> HandleAggregationMethod(ExprValidationContext validationContext)
		{
			if (_chainSpec.IsEmpty() || ChildNodes.Length == 0) {
				return null;
			}

			var chainFirst = _chainSpec[0];
			if (chainFirst is ChainableArray) {
				return null;
			}

			var rootNode = ChildNodes[0];
			var aggMethodParams = chainFirst.GetParametersOrEmpty().ToArray();
			var aggMethodName = chainFirst.GetRootNameOrEmptyString();

			// handle property, such as "sortedcolumn.floorKey('a')" since "floorKey" can also be a property
			if (chainFirst is ChainableName) {
				var prop = PropertyParserNoDep.ParseAndWalkLaxToSimple(chainFirst.GetRootNameOrEmptyString(), false);
				if (prop is MappedProperty) {
					var mappedProperty = (MappedProperty) prop;
					aggMethodName = mappedProperty.PropertyNameAtomic;
					aggMethodParams = new ExprNode[] {new ExprConstantNodeImpl(mappedProperty.Key)};
				}
			}

			if (!(rootNode is ExprTableAccessNodeSubprop) && 
			    !(rootNode is ExprAggMultiFunctionNode) &&
			    !(rootNode is ExprTableIdentNode)) {
				return null;
			}

			ExprDotNodeAggregationMethodForge aggregationMethodForge;
			if (rootNode is ExprAggMultiFunctionNode) {
				// handle local aggregation
				var mf = (ExprAggMultiFunctionNode) rootNode;
				if (!mf.AggregationForgeFactory.AggregationPortableValidation.IsAggregationMethod(aggMethodName, aggMethodParams, validationContext)) {
					return null;
				}

				aggregationMethodForge = new ExprDotNodeAggregationMethodForgeLocal(
					this,
					aggMethodName,
					aggMethodParams,
					mf.AggregationForgeFactory.AggregationPortableValidation,
					mf);
			}
			else if (rootNode is ExprTableIdentNode) {
				// handle table-column via from-clause
				var tableSubprop = (ExprTableIdentNode) rootNode;
				var column = tableSubprop.TableMetadata.Columns.Get(tableSubprop.ColumnName);
				if (!(column is TableMetadataColumnAggregation)) {
					return null;
				}

				var columnAggregation = (TableMetadataColumnAggregation) column;
				if (aggMethodName.ToLowerInvariant().Equals("reset")) {
					if (!validationContext.IsAllowTableAggReset) {
						throw new ExprValidationException(AggregationPortableValidationBase.INVALID_TABLE_AGG_RESET);
					}

					aggregationMethodForge = new ExprDotNodeAggregationMethodForgeTableReset(
						this,
						aggMethodName,
						aggMethodParams,
						columnAggregation.AggregationPortableValidation,
						tableSubprop,
						columnAggregation);
				}
				else {
					if (columnAggregation.IsMethodAgg ||
					    !columnAggregation.AggregationPortableValidation.IsAggregationMethod(aggMethodName, aggMethodParams, validationContext)) {
						return null;
					}

					aggregationMethodForge = new ExprDotNodeAggregationMethodForgeTableIdent(
						this,
						aggMethodName,
						aggMethodParams,
						columnAggregation.AggregationPortableValidation,
						tableSubprop,
						columnAggregation);
				}
			}
			else if (rootNode is ExprTableAccessNodeSubprop) {
				// handle table-column via table-access
				var tableSubprop = (ExprTableAccessNodeSubprop) rootNode;
				var column = tableSubprop.TableMeta.Columns.Get(tableSubprop.SubpropName);
				if (!(column is TableMetadataColumnAggregation)) {
					return null;
				}

				var columnAggregation = (TableMetadataColumnAggregation) column;
				if (columnAggregation.IsMethodAgg ||
				    !columnAggregation.AggregationPortableValidation.IsAggregationMethod(aggMethodName, aggMethodParams, validationContext)) {
					return null;
				}

				aggregationMethodForge = new ExprDotNodeAggregationMethodForgeTableAccess(
					this,
					aggMethodName,
					aggMethodParams,
					columnAggregation.AggregationPortableValidation,
					tableSubprop,
					columnAggregation);
			}
			else {
				throw new IllegalStateException("Unhandled aggregation method root node");
			}

			// validate
			aggregationMethodForge.Validate(validationContext);

			var newChain = _chainSpec.Count == 1 
				? (IList<Chainable>) EmptyList<Chainable>.Instance
				: new List<Chainable>(_chainSpec.SubList(1, _chainSpec.Count));
			
			var root = new ExprDotNodeAggregationMethodRootNode(aggregationMethodForge);
			root.AddChildNode(rootNode);
			return new Pair<ExprDotNodeAggregationMethodRootNode, IList<Chainable>>(root, newChain);
		}

		public FilterExprAnalyzerAffector GetAffector(bool isOuterJoin)
		{
			CheckValidated(_forge);
			return isOuterJoin ? null : _forge.FilterExprAnalyzerAffector;
		}

		private ExprDotNodeForge GetPropertyPairEvaluator(
			ExprForge parameterForge,
			Pair<PropertyResolutionDescriptor, string> propertyInfoPair,
			ExprValidationContext validationContext)
		{
			var propertyName = propertyInfoPair.First.PropertyName;
			var propertyDesc = EventTypeUtility.GetNestablePropertyDescriptor(propertyInfoPair.First.StreamEventType, propertyName);
			if (propertyDesc == null || (!propertyDesc.IsMapped && !propertyDesc.IsIndexed)) {
				throw new ExprValidationException(
					"Unknown single-row function, aggregation function or mapped or indexed property named '" + propertyName + "' could not be resolved");
			}

			var streamNum = propertyInfoPair.First.StreamNum;
			EventPropertyGetterMappedSPI mappedGetter = null;
			EventPropertyGetterIndexedSPI indexedGetter = null;

			var propertyType = typeof(object);
			if (propertyDesc.IsMapped) {
				if (parameterForge.EvaluationType != typeof(string)) {
					throw new ExprValidationException(
						"Parameter expression to mapped property '" +
						propertyDesc.PropertyName +
						"' is expected to return a string-type value but returns " +
						parameterForge.EvaluationType.CleanName());
				}

				mappedGetter = ((EventTypeSPI) propertyInfoPair.First.StreamEventType).GetGetterMappedSPI(propertyInfoPair.First.PropertyName);
				if (mappedGetter == null) {
					throw new ExprValidationException("Mapped property named '" + propertyName + "' failed to obtain getter-object");
				}
			}
			else {
				if (parameterForge.EvaluationType.GetBoxedType() != typeof(int?)) {
					throw new ExprValidationException(
						"Parameter expression to indexed property '" +
						propertyDesc.PropertyName +
						"' is expected to return a Integer-type value but returns " +
						parameterForge.EvaluationType.CleanName());
				}

				indexedGetter = ((EventTypeSPI) propertyInfoPair.First.StreamEventType).GetGetterIndexedSPI(propertyInfoPair.First.PropertyName);
				if (indexedGetter == null) {
					throw new ExprValidationException("Indexed property named '" + propertyName + "' failed to obtain getter-object");
				}
			}

			if (propertyDesc.PropertyComponentType != null) {
				propertyType = propertyDesc.PropertyComponentType.GetBoxedType();
			}

			return new ExprDotNodeForgePropertyExpr(
				this,
				validationContext.StatementName,
				propertyDesc.PropertyName,
				streamNum,
				parameterForge,
				propertyType,
				indexedGetter,
				mappedGetter);
		}

		private int PrefixedStreamName(
			IList<Chainable> chainSpec,
			StreamTypeService streamTypeService)
		{
			if (chainSpec.Count < 1) {
				return -1;
			}

			var spec = chainSpec[0];
			if (!(spec is ChainableName)) {
				return -1;
			}

			var name = (ChainableName) spec;
			return streamTypeService.GetStreamNumForStreamName(name.Name);
		}

		public override void Accept(ExprNodeVisitor visitor)
		{
			base.Accept(visitor);
			ExprNodeUtilityQuery.AcceptChain(visitor, _chainSpec);
		}

		public override void Accept(ExprNodeVisitorWithParent visitor)
		{
			base.Accept(visitor);
			ExprNodeUtilityQuery.AcceptChain(visitor, _chainSpec);
		}

		public override void AcceptChildnodes(
			ExprNodeVisitorWithParent visitor,
			ExprNode parent)
		{
			base.AcceptChildnodes(visitor, parent);
			ExprNodeUtilityQuery.AcceptChain(visitor, _chainSpec, this);
		}

		public override void ReplaceUnlistedChildNode(
			ExprNode nodeToReplace,
			ExprNode newNode)
		{
			ExprNodeUtilityModify.ReplaceChainChildNode(nodeToReplace, newNode, _chainSpec);
		}

		public IList<Chainable> ChainSpec {
			get => _chainSpec;
			set => _chainSpec = value.AsReadOnlyList();
		}

		public ExprEvaluator ExprEvaluator {
			get {
				CheckValidated(_forge);
				return _forge.ExprEvaluator;
			}
		}

		public bool IsConstantResult {
			get {
				CheckValidated(_forge);
				return _forge.IsReturnsConstantResult;
			}
		}

		public override ExprForge Forge {
			get {
				CheckValidated(_forge);
				return _forge;
			}
		}

		public int? StreamReferencedIfAny {
			get {
				CheckValidated(_forge);
				return _forge.StreamNumReferenced;
			}
		}

		public string RootPropertyNameIfAny {
			get {
				CheckValidated(_forge);
				return _forge.RootPropertyName;
			}
		}

		public override void ToPrecedenceFreeEPL(
			TextWriter writer,
			ExprNodeRenderableFlags flags)
		{
			if (ChildNodes.Length != 0) {
				writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(ChildNodes[0]));
			}

			ExprNodeUtilityPrint.ToExpressionString(_chainSpec, writer, ChildNodes.Length != 0, null);
		}

		public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

		public IDictionary<string, object> EventType => null;

		public override bool EqualsNode(
			ExprNode node,
			bool ignoreStreamPrefix)
		{
			if (!(node is ExprDotNodeImpl)) {
				return false;
			}

			var other = (ExprDotNodeImpl) node;
			if (other._chainSpec.Count != _chainSpec.Count) {
				return false;
			}

			for (var i = 0; i < _chainSpec.Count; i++) {
				if (!_chainSpec[i].Equals(other._chainSpec[i])) {
					return false;
				}
			}

			return true;
		}

		public IList<ExprNode> AdditionalNodes => ExprNodeUtilityQuery.CollectChainParameters(_chainSpec);

		public VariableMetaData IsVariableOpGetName(VariableCompileTimeResolver variableCompileTimeResolver)
		{
			if (_chainSpec.Count > 0 && _chainSpec[0] is ChainableName) {
				return variableCompileTimeResolver.Resolve(((ChainableName) _chainSpec[0]).Name);
			}

			return null;
		}

		private ExprValidationException HandleNotFound(string name)
		{
			var appDotMethodDidYouMean = GetAppDotMethodDidYouMean();
			var message = "Unknown single-row function, expression declaration, script or aggregation function named '" + name + "' could not be resolved";
			if (appDotMethodDidYouMean != null) {
				message += " (did you mean '" + appDotMethodDidYouMean + "')";
			}

			return new ExprValidationException(message);
		}

		private string GetAppDotMethodDidYouMean()
		{
			var lhsName = _chainSpec[0].GetRootNameOrEmptyString().ToLowerInvariant();
			if (lhsName.Equals("rectangle")) {
				return "rectangle.intersects";
			}

			if (lhsName.Equals("point")) {
				return "point.inside";
			}

			return null;
		}

		private ExprAppDotMethodImpl GetAppDotMethod(bool filterExpression)
		{
			if (_chainSpec.Count < 2) {
				return null;
			}

			if (!(_chainSpec[1] is ChainableCall)) {
				return null;
			}

			var call = (ChainableCall) _chainSpec[1];
			var lhsName = _chainSpec[0].GetRootNameOrEmptyString();
			var operationName = call.Name.ToLowerInvariant();
			var pointInside = lhsName.Equals("point") && operationName.Equals("inside");
			var rectangleIntersects = lhsName.Equals("rectangle") && operationName.Equals("intersects");
			if (!pointInside && !rectangleIntersects) {
				return null;
			}

			if (call.Parameters.Count != 1) {
				throw GetAppDocMethodException(lhsName, operationName);
			}

			var param = call.Parameters[0];
			if (!(param is ExprDotNode)) {
				throw GetAppDocMethodException(lhsName, operationName);
			}

			var compared = (ExprDotNode) call.Parameters[0];
			if (compared.ChainSpec.Count != 1) {
				throw GetAppDocMethodException(lhsName, operationName);
			}

			var rhsName = compared.ChainSpec[0].GetRootNameOrEmptyString().ToLowerInvariant();
			var pointInsideRectangle = pointInside && rhsName.Equals("rectangle");
			var rectangleIntersectsRectangle = rectangleIntersects && rhsName.Equals("rectangle");
			if (!pointInsideRectangle && !rectangleIntersectsRectangle) {
				throw GetAppDocMethodException(lhsName, operationName);
			}

			var lhsExpressions = _chainSpec[0].GetParametersOrEmpty();
			ExprNode[] indexNamedParameter = null;
			IList<ExprNode> lhsExpressionsValues = new List<ExprNode>();
			foreach (var lhsExpression in lhsExpressions) {
				if (lhsExpression is ExprNamedParameterNode) {
					var named = (ExprNamedParameterNode) lhsExpression;
					if (named.ParameterName.Equals(ExprDotNodeConstants.FILTERINDEX_NAMED_PARAMETER, StringComparison.InvariantCultureIgnoreCase)) {
						if (!filterExpression) {
							throw new ExprValidationException("The '" + named.ParameterName + "' named parameter can only be used in in filter expressions");
						}

						indexNamedParameter = named.ChildNodes;
					}
					else {
						throw new ExprValidationException(lhsName + " does not accept '" + named.ParameterName + "' as a named parameter");
					}
				}
				else {
					lhsExpressionsValues.Add(lhsExpression);
				}
			}

			var lhs = ExprNodeUtilityQuery.ToArray(lhsExpressionsValues);
			var rhs = ExprNodeUtilityQuery.ToArray(compared.ChainSpec[0].GetParametersOrEmpty());

			SettingsApplicationDotMethod predefined;
			if (pointInsideRectangle) {
				predefined = new SettingsApplicationDotMethodPointInsideRectangle(this, lhsName, lhs, operationName, rhsName, rhs, indexNamedParameter);
			}
			else {
				predefined = new SettingsApplicationDotMethodRectangeIntersectsRectangle(this, lhsName, lhs, operationName, rhsName, rhs, indexNamedParameter);
			}

			return new ExprAppDotMethodImpl(predefined);
		}

		public bool IsLocalInlinedClass => _forge.IsLocalInlinedClass;

		private ExprValidationException GetAppDocMethodException(
			string lhsName,
			string operationName)
		{
			return new ExprValidationException(lhsName + "." + operationName + " requires a single rectangle as parameter");
		}

		private class PropertyInfoPairDesc
		{
			public PropertyInfoPairDesc(ExprDotNodeForgeRootChild forge)
			{
				Forge = forge;
			}

			public ExprDotNodeForgeRootChild Forge { get; }

			public void Apply(ExprDotNodeImpl node)
			{
				node._forge = Forge;
			}
		}
	}
} // end of namespace
