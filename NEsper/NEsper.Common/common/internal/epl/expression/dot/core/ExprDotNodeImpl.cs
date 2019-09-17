///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree;
using com.espertech.esper.common.@internal.epl.@join.analyze;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    /// <summary>
    ///     Represents an Dot-operator expression, for use when "(expression).method(...).method(...)"
    /// </summary>
    [Serializable]
    public class ExprDotNodeImpl : ExprNodeBase,
        ExprDotNode,
        ExprStreamRefNode,
        ExprNodeInnerNodeProvider
    {
        private readonly bool isDuckTyping;
        private readonly bool isUDFCache;

        [NonSerialized] private ExprDotNodeForge forge;

        public ExprDotNodeImpl(
            IList<ExprChainedSpec> chainSpec,
            bool isDuckTyping,
            bool isUDFCache)
        {
            ChainSpec = new List<ExprChainedSpec>(chainSpec); // for safety, copy the list
            this.isDuckTyping = isDuckTyping;
            this.isUDFCache = isUDFCache;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public bool IsConstantResult {
            get {
                CheckValidated(forge);
                return forge.IsReturnsConstantResult;
            }
        }

        public IDictionary<string, object> EventType => null;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // check for plannable methods: these are validated according to different rules
            var appDotMethod = GetAppDotMethod(validationContext.IsFilterExpression);
            if (appDotMethod != null) {
                return appDotMethod;
            }

            // validate all parameters
            ExprNodeUtilityValidate.Validate(ExprNodeOrigin.DOTNODEPARAMETER, ChainSpec, validationContext);

            // determine if there are enumeration method expressions in the chain
            var hasEnumerationMethod = false;
            foreach (var chain in ChainSpec) {
                if (EnumMethodEnumExtensions.IsEnumerationMethod(chain.Name)) {
                    hasEnumerationMethod = true;
                    break;
                }
            }

            // determine if there is an implied binding, replace first chain element with evaluation node if there is
            if (validationContext.StreamTypeService.HasTableTypes &&
                validationContext.TableCompileTimeResolver != null &&
                ChainSpec.Count > 1 &&
                ChainSpec[0].IsProperty) {
                var tableNode = TableCompileTimeUtil.GetTableNodeChainable(
                    validationContext.StreamTypeService,
                    ChainSpec,
                    validationContext.ImportService,
                    validationContext.TableCompileTimeResolver);
                if (tableNode != null) {
                    var node = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.DOTNODE,
                        tableNode.First,
                        validationContext);
                    if (tableNode.Second.IsEmpty()) {
                        return node;
                    }

                    ChainSpec.Clear();
                    ChainSpec.AddAll(tableNode.Second);
                    AddChildNode(node);
                }
            }

            // The root node expression may provide the input value:
            //   Such as "window(*).doIt(...)" or "(select * from Window).doIt()" or "prevwindow(sb).doIt(...)", in which case the expression to act on is a child expression
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
                    typeInfoX = EPTypeHelper.SingleValue(
                        rootNode.Forge.EvaluationType); // not a collection type, treat as scalar
                }
                else {
                    typeInfoX = enumSrc.ReturnType;
                }

                var evalsX = ExprDotNodeUtility.GetChainEvaluators(
                    enumSrc.StreamOfProviderIfApplicable,
                    typeInfoX,
                    ChainSpec,
                    validationContext,
                    isDuckTyping,
                    new ExprDotNodeFilterAnalyzerInputExpr());
                forge = new ExprDotNodeForgeRootChild(
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
            if (ChainSpec.Count == 1) {
                var spec = ChainSpec[0];
                if (spec.Parameters.IsEmpty()) {
                    throw HandleNotFound(spec.Name);
                }

                // single-parameter can resolve to a property
                Pair<PropertyResolutionDescriptor, string> propertyInfoPairX = null;
                try {
                    propertyInfoPairX = ExprIdentNodeUtil.GetTypeFromStream(
                        streamTypeService,
                        spec.Name,
                        streamTypeService.HasPropertyAgnosticType,
                        false,
                        validationContext.TableCompileTimeResolver);
                }
                catch (ExprValidationPropertyException) {
                    // fine
                }

                // if not a property then try built-in single-row non-grammar functions
                if (propertyInfoPairX == null &&
                    spec.Name.ToLowerInvariant()
                        .Equals(ImportServiceCompileTime.EXT_SINGLEROW_FUNCTION_TRANSPOSE)) {
                    if (spec.Parameters.Count != 1) {
                        throw new ExprValidationException(
                            "The " +
                            ImportServiceCompileTime.EXT_SINGLEROW_FUNCTION_TRANSPOSE +
                            " function requires a single parameter expression");
                    }

                    forge = new ExprDotNodeForgeTransposeAsStream(this, ChainSpec[0].Parameters[0].Forge);
                }
                else if (spec.Parameters.Count != 1) {
                    throw HandleNotFound(spec.Name);
                }
                else {
                    if (propertyInfoPairX == null) {
                        throw new ExprValidationException(
                            "Unknown single-row function, aggregation function or mapped or indexed property named '" +
                            spec.Name +
                            "' could not be resolved");
                    }

                    forge = GetPropertyPairEvaluator(spec.Parameters[0].Forge, propertyInfoPairX, validationContext);
                }

                return null;
            }

            // handle the case where the first chain spec element is a stream name.
            ExprValidationException prefixedStreamNumException = null;
            var prefixedStreamNumber = PrefixedStreamName(ChainSpec, validationContext.StreamTypeService);
            if (prefixedStreamNumber != -1) {
                var specAfterStreamName = ChainSpec[1];

                // Attempt to resolve as property
                Pair<PropertyResolutionDescriptor, string> propertyInfoPairX = null;
                try {
                    var propName = ChainSpec[0].Name + "." + specAfterStreamName.Name;
                    propertyInfoPairX = ExprIdentNodeUtil.GetTypeFromStream(
                        streamTypeService,
                        propName,
                        streamTypeService.HasPropertyAgnosticType,
                        false,
                        validationContext.TableCompileTimeResolver);
                }
                catch (ExprValidationPropertyException) {
                    // fine
                }

                if (propertyInfoPairX != null) {
                    if (specAfterStreamName.Parameters.Count != 1) {
                        throw HandleNotFound(specAfterStreamName.Name);
                    }

                    forge = GetPropertyPairEvaluator(
                        specAfterStreamName.Parameters[0].Forge,
                        propertyInfoPairX,
                        validationContext);
                    return null;
                }

                // Attempt to resolve as event-underlying object instance method
                var eventType = validationContext.StreamTypeService.EventTypes[prefixedStreamNumber];
                var type = eventType.UnderlyingType;

                IList<ExprChainedSpec> remainderChain = new List<ExprChainedSpec>(ChainSpec);
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
                    forge = new ExprDotNodeForgeStream(
                        this,
                        filterExprAnalyzerAffector,
                        prefixedStreamNumber,
                        eventType,
                        underlyingMethodChain,
                        true);
                }
                else if (eventTypeMethodChain != null) {
                    forge = new ExprDotNodeForgeStream(
                        this,
                        filterExprAnalyzerAffector,
                        prefixedStreamNumber,
                        eventType,
                        eventTypeMethodChain,
                        false);
                }

                if (forge != null) {
                    return null;
                }

                if (ExprDotNodeUtility.IsDatetimeOrEnumMethod(remainderChain[0].Name)) {
                    prefixedStreamNumException = enumDatetimeEx;
                }
                else {
                    prefixedStreamNumException = new ExprValidationException(
                        "Failed to solve '" +
                        remainderChain[0].Name +
                        "' to either an date-time or enumeration method, an event property or a method on the event underlying object: " +
                        methodEx.Message,
                        methodEx);
                }
            }

            // There no root node, in this case the classname or property name is provided as part of the chain.
            // Such as "MyClass.myStaticLib(...)" or "mycollectionproperty.doIt(...)"
            //
            IList<ExprChainedSpec> modifiedChain = new List<ExprChainedSpec>(ChainSpec);
            var firstItem = modifiedChain.DeleteAt(0);

            Pair<PropertyResolutionDescriptor, string> propertyInfoPair = null;
            try {
                propertyInfoPair = ExprIdentNodeUtil.GetTypeFromStream(
                    streamTypeService,
                    firstItem.Name,
                    streamTypeService.HasPropertyAgnosticType,
                    true,
                    validationContext.TableCompileTimeResolver);
            }
            catch (ExprValidationPropertyException) {
                // not a property
            }

            // If property then treat it as such
            if (propertyInfoPair != null) {
                var propertyName = propertyInfoPair.First.PropertyName;
                var streamId = propertyInfoPair.First.StreamNum;
                var streamType = streamTypeService.EventTypes[streamId];
                EPType typeInfoX;
                ExprEnumerationForge enumerationForge = null;
                EPType inputType;
                ExprForge rootNodeForge = null;
                EventPropertyGetterSPI getter;

                if (firstItem.Parameters.IsEmpty()) {
                    getter = ((EventTypeSPI) streamType).GetGetterSPI(propertyInfoPair.First.PropertyName);

                    var propertyEval =
                        ExprDotNodeUtility.GetPropertyEnumerationSource(
                            propertyInfoPair.First.PropertyName,
                            streamId,
                            streamType,
                            hasEnumerationMethod,
                            validationContext.IsDisablePropertyExpressionEventCollCache);
                    typeInfoX = propertyEval.ReturnType;
                    enumerationForge = propertyEval.Enumeration;
                    inputType = propertyEval.ReturnType;
                    rootNodeForge = new PropertyDotNonLambdaForge(
                        streamId,
                        getter,
                        propertyInfoPair.First.PropertyType.GetBoxedType());
                }
                else {
                    // property with parameter - mapped or indexed property
                    var desc = EventTypeUtility.GetNestablePropertyDescriptor(
                        streamTypeService.EventTypes[propertyInfoPair.First.StreamNum],
                        firstItem.Name);
                    if (firstItem.Parameters.Count > 1) {
                        throw new ExprValidationException(
                            "Property '" + firstItem.Name + "' may not be accessed passing 2 or more parameters");
                    }

                    var paramEval = firstItem.Parameters[0].Forge;
                    typeInfoX = EPTypeHelper.SingleValue(desc.PropertyComponentType);
                    inputType = typeInfoX;
                    getter = null;
                    if (desc.IsMapped) {
                        if (paramEval.EvaluationType != typeof(string)) {
                            throw new ExprValidationException(
                                "Parameter expression to mapped property '" +
                                propertyName +
                                "' is expected to return a string-type value but returns " +
                                paramEval.EvaluationType.CleanName());
                        }

                        var mappedGetter =
                            ((EventTypeSPI) propertyInfoPair.First.StreamEventType).GetGetterMappedSPI(
                                propertyInfoPair.First.PropertyName);
                        if (mappedGetter == null) {
                            throw new ExprValidationException(
                                "Mapped property named '" + propertyName + "' failed to obtain getter-object");
                        }

                        rootNodeForge = new PropertyDotNonLambdaMappedForge(
                            streamId,
                            mappedGetter,
                            paramEval,
                            desc.PropertyComponentType);
                    }

                    if (desc.IsIndexed) {
                        if (paramEval.EvaluationType.GetBoxedType() != typeof(int?)) {
                            throw new ExprValidationException(
                                "Parameter expression to mapped property '" +
                                propertyName +
                                "' is expected to return a Integer-type value but returns " +
                                paramEval.EvaluationType.CleanName());
                        }

                        var indexedGetter =
                            ((EventTypeSPI) propertyInfoPair.First.StreamEventType).GetGetterIndexedSPI(
                                propertyInfoPair.First.PropertyName);
                        if (indexedGetter == null) {
                            throw new ExprValidationException(
                                "Mapped property named '" + propertyName + "' failed to obtain getter-object");
                        }

                        rootNodeForge = new PropertyDotNonLambdaIndexedForge(
                            streamId,
                            indexedGetter,
                            paramEval,
                            desc.PropertyComponentType);
                    }
                }

                if (typeInfoX == null) {
                    throw new ExprValidationException(
                        "Property '" + propertyName + "' is not a mapped or indexed property");
                }

                // try to build chain based on the input (non-fragment)
                ExprDotNodeRealizedChain evalsX;
                var filterAnalyzerInputProp = new ExprDotNodeFilterAnalyzerInputProp(
                    propertyInfoPair.First.StreamNum,
                    propertyInfoPair.First.PropertyName);
                var rootIsEventBean = false;
                try {
                    evalsX = ExprDotNodeUtility.GetChainEvaluators(
                        streamId,
                        inputType,
                        modifiedChain,
                        validationContext,
                        isDuckTyping,
                        filterAnalyzerInputProp);
                }
                catch (ExprValidationException) {
                    // try building the chain based on the fragment event type (i.e. A.after(B) based on A-configured start time where A is a fragment)
                    var fragment = propertyInfoPair.First.FragmentEventType;
                    if (fragment == null) {
                        throw;
                    }

                    EPType fragmentTypeInfo;
                    if (fragment.IsIndexed) {
                        fragmentTypeInfo = EPTypeHelper.CollectionOfEvents(fragment.FragmentType);
                    }
                    else {
                        fragmentTypeInfo = EPTypeHelper.SingleEvent(fragment.FragmentType);
                    }

                    rootIsEventBean = true;
                    evalsX = ExprDotNodeUtility.GetChainEvaluators(
                        propertyInfoPair.First.StreamNum,
                        fragmentTypeInfo,
                        modifiedChain,
                        validationContext,
                        isDuckTyping,
                        filterAnalyzerInputProp);
                    rootNodeForge = new PropertyDotNonLambdaFragmentForge(streamId, getter);
                }

                var filterExprAnalyzerAffector = evalsX.FilterAnalyzerDesc;
                var streamNumReferenced = propertyInfoPair.First.StreamNum;
                var rootPropertyName = propertyInfoPair.First.PropertyName;
                forge = new ExprDotNodeForgeRootChild(
                    this,
                    filterExprAnalyzerAffector,
                    streamNumReferenced,
                    rootPropertyName,
                    hasEnumerationMethod,
                    rootNodeForge,
                    enumerationForge,
                    inputType,
                    evalsX.Chain,
                    evalsX.ChainWithUnpack,
                    !rootIsEventBean);
                return null;
            }

            // If variable then resolve as such
            var variable = validationContext.VariableCompileTimeResolver.Resolve(firstItem.Name);
            if (variable != null) {
                if (variable.OptionalContextName != null) {
                    throw new ExprValidationException(
                        "Method invocation on context-specific variable is not supported");
                }

                EPType typeInfoX;
                ExprDotStaticMethodWrap wrap;
                if (variable.Type.IsArray) {
                    typeInfoX = EPTypeHelper.CollectionOfSingleValue(variable.Type.GetElementType());
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
                forge = new ExprDotNodeForgeVariable(this, variable, wrap, evalsX.ChainWithUnpack);
                return null;
            }

            // try resolve as enumeration class with value
            var enumconstant = ImportCompileTimeUtil.ResolveIdentAsEnumConst(
                firstItem.Name,
                validationContext.ImportService,
                false);
            if (enumconstant != null) {
                // try resolve method
                var methodSpec = modifiedChain[0];
                var enumvalue = firstItem.Name;
                ExprNodeUtilResolveExceptionHandler handler = new ProxyExprNodeUtilResolveExceptionHandler {
                    ProcHandle = ex => {
                        return new ExprValidationException(
                            "Failed to resolve method '" +
                            methodSpec.Name +
                            "' on enumeration value '" +
                            enumvalue +
                            "': " +
                            ex.Message);
                    }
                };
                var wildcardType = validationContext.StreamTypeService.EventTypes.Length != 1
                    ? null
                    : validationContext.StreamTypeService.EventTypes[0];
                var methodDesc = ExprNodeUtilityResolve.ResolveMethodAllowWildcardAndStream(
                    enumconstant.GetType().Name,
                    enumconstant.GetType(),
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
                var typeInfoX = optionalLambdaWrapX != null
                    ? optionalLambdaWrapX.TypeInfo
                    : EPTypeHelper.SingleValue(methodDesc.ReflectionMethod.ReturnType);

                var evalsX = ExprDotNodeUtility.GetChainEvaluators(
                    null,
                    typeInfoX,
                    modifiedChain,
                    validationContext,
                    false,
                    new ExprDotNodeFilterAnalyzerInputStatic());
                forge = new ExprDotNodeForgeStaticMethod(
                    this,
                    false,
                    firstItem.Name,
                    methodDesc.ReflectionMethod,
                    methodDesc.ChildForges,
                    false,
                    evalsX.ChainWithUnpack,
                    optionalLambdaWrapX,
                    false,
                    enumconstant,
                    validationContext.StatementName);
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

            var method = ExprNodeUtilityResolve.ResolveMethodAllowWildcardAndStream(
                firstItem.Name,
                null,
                secondItem.Name,
                secondItem.Parameters,
                allowWildcard,
                streamZeroType,
                new ExprNodeUtilResolveExceptionHandlerDefault(firstItem.Name + "." + secondItem.Name, false),
                secondItem.Name,
                validationContext.StatementRawInfo,
                validationContext.StatementCompileTimeService);

            var isConstantParameters = method.IsAllConstants && isUDFCache;
            var isReturnsConstantResult = isConstantParameters && modifiedChain.IsEmpty();

            // this may return a pair of null if there is no lambda or the result cannot be wrapped for lambda-function use
            var optionalLambdaWrap = ExprDotStaticMethodWrapFactory.Make(
                method.ReflectionMethod,
                modifiedChain,
                null,
                validationContext);
            var typeInfo = optionalLambdaWrap != null
                ? optionalLambdaWrap.TypeInfo
                : EPTypeHelper.SingleValue(method.ReflectionMethod.ReturnType);

            var evals = ExprDotNodeUtility.GetChainEvaluators(
                null,
                typeInfo,
                modifiedChain,
                validationContext,
                false,
                new ExprDotNodeFilterAnalyzerInputStatic());
            forge = new ExprDotNodeForgeStaticMethod(
                this,
                isReturnsConstantResult,
                firstItem.Name,
                method.ReflectionMethod,
                method.ChildForges,
                isConstantParameters,
                evals.ChainWithUnpack,
                optionalLambdaWrap,
                false,
                null,
                validationContext.StatementName);

            return null;
        }

        public FilterExprAnalyzerAffector GetAffector(bool isOuterJoin)
        {
            CheckValidated(forge);
            return isOuterJoin ? null : forge.FilterExprAnalyzerAffector;
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            base.Accept(visitor);
            ExprNodeUtilityQuery.AcceptChain(visitor, ChainSpec);
        }

        public override void Accept(ExprNodeVisitorWithParent visitor)
        {
            base.Accept(visitor);
            ExprNodeUtilityQuery.AcceptChain(visitor, ChainSpec);
        }

        public override void AcceptChildnodes(
            ExprNodeVisitorWithParent visitor,
            ExprNode parent)
        {
            base.AcceptChildnodes(visitor, parent);
            ExprNodeUtilityQuery.AcceptChain(visitor, ChainSpec, this);
        }

        public override void ReplaceUnlistedChildNode(
            ExprNode nodeToReplace,
            ExprNode newNode)
        {
            ExprNodeUtilityModify.ReplaceChainChildNode(nodeToReplace, newNode, ChainSpec);
        }

        public IList<ExprChainedSpec> ChainSpec { get; }

        public override ExprForge Forge {
            get {
                CheckValidated(forge);
                return forge;
            }
        }

        public int? StreamReferencedIfAny {
            get {
                CheckValidated(forge);
                return forge.StreamNumReferenced;
            }
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.MINIMUM;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprDotNodeImpl)) {
                return false;
            }

            var other = (ExprDotNodeImpl) node;
            if (other.ChainSpec.Count != ChainSpec.Count) {
                return false;
            }

            for (var i = 0; i < ChainSpec.Count; i++) {
                if (!ChainSpec[i].Equals(other.ChainSpec[i])) {
                    return false;
                }
            }

            return true;
        }

        public VariableMetaData IsVariableOpGetName(VariableCompileTimeResolver variableCompileTimeResolver)
        {
            if (ChainSpec.Count > 0 && ChainSpec[0].IsProperty) {
                return variableCompileTimeResolver.Resolve(ChainSpec[0].Name);
            }

            return null;
        }

        public IList<ExprNode> AdditionalNodes => ExprNodeUtilityQuery.CollectChainParameters(ChainSpec);

        public string RootPropertyNameIfAny {
            get {
                CheckValidated(forge);
                return forge.RootPropertyName;
            }
        }

        private ExprDotNodeForge GetPropertyPairEvaluator(
            ExprForge parameterForge,
            Pair<PropertyResolutionDescriptor, string> propertyInfoPair,
            ExprValidationContext validationContext)
        {
            var propertyName = propertyInfoPair.First.PropertyName;
            var propertyDesc = EventTypeUtility.GetNestablePropertyDescriptor(
                propertyInfoPair.First.StreamEventType,
                propertyName);
            if (propertyDesc == null || !propertyDesc.IsMapped && !propertyDesc.IsIndexed) {
                throw new ExprValidationException(
                    "Unknown single-row function, aggregation function or mapped or indexed property named '" +
                    propertyName +
                    "' could not be resolved");
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

                mappedGetter =
                    ((EventTypeSPI) propertyInfoPair.First.StreamEventType).GetGetterMappedSPI(
                        propertyInfoPair.First.PropertyName);
                if (mappedGetter == null) {
                    throw new ExprValidationException(
                        "Mapped property named '" + propertyName + "' failed to obtain getter-object");
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

                indexedGetter =
                    ((EventTypeSPI) propertyInfoPair.First.StreamEventType).GetGetterIndexedSPI(
                        propertyInfoPair.First.PropertyName);
                if (indexedGetter == null) {
                    throw new ExprValidationException(
                        "Indexed property named '" + propertyName + "' failed to obtain getter-object");
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
            IList<ExprChainedSpec> chainSpec,
            StreamTypeService streamTypeService)
        {
            if (chainSpec.Count < 1) {
                return -1;
            }

            var spec = chainSpec[0];
            if (spec.Parameters.Count > 0 && !spec.IsProperty) {
                return -1;
            }

            return streamTypeService.GetStreamNumForStreamName(spec.Name);
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (ChildNodes.Length != 0) {
                writer.Write(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(ChildNodes[0]));
            }

            ExprNodeUtilityPrint.ToExpressionString(ChainSpec, writer, ChildNodes.Length != 0, null);
        }

        private ExprValidationException HandleNotFound(string name)
        {
            var appDotMethodDidYouMean = GetAppDotMethodDidYouMean();
            var message =
                "Unknown single-row function, expression declaration, script or aggregation function named '" +
                name +
                "' could not be resolved";
            if (appDotMethodDidYouMean != null) {
                message += " (did you mean '" + appDotMethodDidYouMean + "')";
            }

            return new ExprValidationException(message);
        }

        private string GetAppDotMethodDidYouMean()
        {
            var lhsName = ChainSpec[0].Name.ToLowerInvariant();
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
            if (ChainSpec.Count < 2) {
                return null;
            }

            var lhsName = ChainSpec[0].Name.ToLowerInvariant();
            var operationName = ChainSpec[1].Name.ToLowerInvariant();
            var pointInside = lhsName.Equals("point") && operationName.Equals("inside");
            var rectangleIntersects = lhsName.Equals("rectangle") && operationName.Equals("intersects");
            if (!pointInside && !rectangleIntersects) {
                return null;
            }

            if (ChainSpec[1].Parameters.Count != 1) {
                throw GetAppDocMethodException(lhsName, operationName);
            }

            var param = ChainSpec[1].Parameters[0];
            if (!(param is ExprDotNode)) {
                throw GetAppDocMethodException(lhsName, operationName);
            }

            var compared = (ExprDotNode) ChainSpec[1].Parameters[0];
            if (compared.ChainSpec.Count != 1) {
                throw GetAppDocMethodException(lhsName, operationName);
            }

            var rhsName = compared.ChainSpec[0].Name.ToLowerInvariant();
            var pointInsideRectangle = pointInside && rhsName.Equals("rectangle");
            var rectangleIntersectsRectangle = rectangleIntersects && rhsName.Equals("rectangle");
            if (!pointInsideRectangle && !rectangleIntersectsRectangle) {
                throw GetAppDocMethodException(lhsName, operationName);
            }

            var lhsExpressions = ChainSpec[0].Parameters;
            ExprNode[] indexNamedParameter = null;
            IList<ExprNode> lhsExpressionsValues = new List<ExprNode>();
            foreach (var lhsExpression in lhsExpressions) {
                if (lhsExpression is ExprNamedParameterNode) {
                    var named = (ExprNamedParameterNode) lhsExpression;
                    if (named.ParameterName.ToLowerInvariant() == ExprDotNodeConstants.FILTERINDEX_NAMED_PARAMETER) {
                        if (!filterExpression) {
                            throw new ExprValidationException(
                                "The '" +
                                named.ParameterName +
                                "' named parameter can only be used in in filter expressions");
                        }

                        indexNamedParameter = named.ChildNodes.ToArray();
                    }
                    else {
                        throw new ExprValidationException(
                            lhsName + " does not accept '" + named.ParameterName + "' as a named parameter");
                    }
                }
                else {
                    lhsExpressionsValues.Add(lhsExpression);
                }
            }

            var lhs = ExprNodeUtilityQuery.ToArray(lhsExpressionsValues);
            var rhs = ExprNodeUtilityQuery.ToArray(compared.ChainSpec[0].Parameters);

            SettingsApplicationDotMethod predefined;
            if (pointInsideRectangle) {
                predefined = new SettingsApplicationDotMethodPointInsideRectangle(
                    this,
                    lhsName,
                    lhs,
                    operationName,
                    rhsName,
                    rhs,
                    indexNamedParameter);
            }
            else {
                predefined = new SettingsApplicationDotMethodRectangeIntersectsRectangle(
                    this,
                    lhsName,
                    lhs,
                    operationName,
                    rhsName,
                    rhs,
                    indexNamedParameter);
            }

            return new ExprAppDotMethodImpl(predefined);
        }

        private ExprValidationException GetAppDocMethodException(
            string lhsName,
            string operationName)
        {
            return new ExprValidationException(
                lhsName + "." + operationName + " requires a single rectangle as parameter");
        }
    }
} // end of namespace