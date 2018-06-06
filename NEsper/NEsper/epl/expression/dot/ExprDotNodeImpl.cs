///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.index.quadtree;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.dot
{
    /// <summary>
    /// Represents an Dot-operator expression, for use when "(expression).Method(...).Method(...)"
    /// </summary>
    [Serializable]
    public class ExprDotNodeImpl
        : ExprNodeBase
            , ExprDotNode
            , ExprNodeInnerNodeProvider
            , ExprStreamRefNode
    {
        private readonly IList<ExprChainedSpec> _chainSpec;
        private readonly bool _isDuckTyping;
        private readonly bool _isUdfCache;

        [NonSerialized] private ExprEvaluator _exprEvaluator;
        private bool _isReturnsConstantResult;

        [NonSerialized] private FilterExprAnalyzerAffector _filterExprAnalyzerAffector;
        private int? _streamNumReferenced;
        private string _rootPropertyName;

        public ExprDotNodeImpl(IEnumerable<ExprChainedSpec> chainSpec, bool isDuckTyping, bool isUDFCache)
        {
            _chainSpec = new List<ExprChainedSpec>(chainSpec); // for safety, copy the list
            _isDuckTyping = isDuckTyping;
            _isUdfCache = isUDFCache;
        }

        public int? StreamReferencedIfAny
        {
            get
            {
                if (_exprEvaluator == null)
                {
                    throw new IllegalStateException("Identifier expression has not been validated");
                }

                return _streamNumReferenced;
            }
        }

        public string RootPropertyNameIfAny
        {
            get
            {
                if (_exprEvaluator == null)
                {
                    throw new IllegalStateException("Identifier expression has not been validated");
                }

                return _rootPropertyName;
            }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // check for plannable methods: these are validated according to different rules
            var appDotMethod = GetAppDotMethod(validationContext.IsFilterExpression);
            if (appDotMethod != null)
            {
                return appDotMethod;
            }

            // validate all parameters
            ExprNodeUtility.Validate(ExprNodeOrigin.DOTNODEPARAMETER, _chainSpec, validationContext);

            // determine if there are enumeration method expressions in the chain
            var hasEnumerationMethod = _chainSpec
                .Any(chain => chain.Name.IsEnumerationMethod());

            // determine if there is an implied binding, replace first chain element with evaluation node if there is
            if (validationContext.StreamTypeService.HasTableTypes &&
                validationContext.TableService != null &&
                _chainSpec.Count > 1 && _chainSpec[0].IsProperty)
            {
                var tableNode = validationContext.TableService.GetTableNodeChainable(
                    validationContext.StreamTypeService, _chainSpec, validationContext.EngineImportService);
                if (tableNode != null)
                {
                    var node = ExprNodeUtility.GetValidatedSubtree(
                        ExprNodeOrigin.DOTNODE, tableNode.First, validationContext);
                    if (tableNode.Second.IsEmpty())
                    {
                        return node;
                    }

                    _chainSpec.Clear();
                    _chainSpec.AddAll(tableNode.Second);
                    AddChildNode(node);
                }
            }

            // The root node expression may provide the input value:
            //   Such as "Window(*).DoIt(...)" or "(select * from Window).DoIt()" or "Prevwindow(sb).DoIt(...)", in which case the expression to act on is a child expression
            //
            var streamTypeService = validationContext.StreamTypeService;
            if (ChildNodes.Count != 0)
            {
                // the root expression is the first child node
                var rootNode = ChildNodes[0];
                var rootNodeEvaluator = rootNode.ExprEvaluator;

                // the root expression may also provide a lambda-function input (Iterator<EventBean>)
                // Determine collection-type and evaluator if any for root node
                var enumSrc = ExprDotNodeUtility.GetEnumerationSource(
                    rootNode, validationContext.StreamTypeService, validationContext.EventAdapterService,
                    validationContext.StatementId, hasEnumerationMethod,
                    validationContext.IsDisablePropertyExpressionEventCollCache);

                EPType typeInfoX;
                if (enumSrc.ReturnType == null)
                {
                    typeInfoX = EPTypeHelper.SingleValue(rootNodeEvaluator.ReturnType);
                    // not a collection type, treat as scalar
                }
                else
                {
                    typeInfoX = enumSrc.ReturnType;
                }

                var evalsX =
                    ExprDotNodeUtility.GetChainEvaluators(
                        enumSrc.StreamOfProviderIfApplicable, typeInfoX, _chainSpec, validationContext, _isDuckTyping,
                        new ExprDotNodeFilterAnalyzerInputExpr());
                _exprEvaluator = new ExprDotEvalRootChild(
                    hasEnumerationMethod, this, rootNodeEvaluator, enumSrc.Enumeration, typeInfoX, evalsX.Chain,
                    evalsX.ChainWithUnpack, false);
                return null;
            }

            // No root node, and this is a 1-element chain i.e. "Something(param,...)".
            // Plug-in single-row methods are not handled here.
            // Plug-in aggregation methods are not handled here.
            Pair<PropertyResolutionDescriptor, string> propertyInfoPair;
            if (_chainSpec.Count == 1)
            {
                var spec = _chainSpec[0];
                if (spec.Parameters.IsEmpty())
                {
                    throw HandleNotFound(spec.Name);
                }

                // single-parameter can resolve to a property
                propertyInfoPair = null;
                try
                {
                    propertyInfoPair = ExprIdentNodeUtil.GetTypeFromStream(
                        streamTypeService, spec.Name, streamTypeService.HasPropertyAgnosticType, false);
                }
                catch (ExprValidationPropertyException)
                {
                    // fine
                }

                // if not a property then try built-in single-row non-grammar functions
                if (propertyInfoPair == null &&
                    String.Equals(
                        spec.Name, EngineImportServiceConstants.EXT_SINGLEROW_FUNCTION_TRANSPOSE,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    if (spec.Parameters.Count != 1)
                    {
                        throw new ExprValidationException(
                            "The " + EngineImportServiceConstants.EXT_SINGLEROW_FUNCTION_TRANSPOSE +
                            " function requires a single parameter expression");
                    }

                    _exprEvaluator = new ExprDotEvalTransposeAsStream(_chainSpec[0].Parameters[0].ExprEvaluator);
                }
                else if (spec.Parameters.Count != 1)
                {
                    throw HandleNotFound(spec.Name);
                }
                else
                {
                    if (propertyInfoPair == null)
                    {
                        throw new ExprValidationException(
                            "Unknown single-row function, aggregation function or mapped or indexed property named '" +
                            spec.Name + "' could not be resolved");
                    }

                    _exprEvaluator = GetPropertyPairEvaluator(
                        spec.Parameters[0].ExprEvaluator, propertyInfoPair, validationContext);
                    _streamNumReferenced = propertyInfoPair.First.StreamNum;
                }

                return null;
            }

            // handle the case where the first chain spec element is a stream name.
            ExprValidationException prefixedStreamNumException = null;
            var prefixedStreamNumber = PrefixedStreamName(_chainSpec, validationContext.StreamTypeService);
            EPType typeInfo;
            if (prefixedStreamNumber != -1)
            {

                var specAfterStreamName = _chainSpec[1];

                // Attempt to resolve as property
                propertyInfoPair = null;
                try
                {
                    var propName = _chainSpec[0].Name + "." + specAfterStreamName.Name;
                    propertyInfoPair = ExprIdentNodeUtil.GetTypeFromStream(
                        streamTypeService, propName, streamTypeService.HasPropertyAgnosticType, false);
                }
                catch (ExprValidationPropertyException)
                {
                    // fine
                }

                if (propertyInfoPair != null)
                {
                    if (specAfterStreamName.Parameters.Count != 1)
                    {
                        throw HandleNotFound(specAfterStreamName.Name);
                    }

                    _exprEvaluator = GetPropertyPairEvaluator(
                        specAfterStreamName.Parameters[0].ExprEvaluator, propertyInfoPair, validationContext);
                    _streamNumReferenced = propertyInfoPair.First.StreamNum;
                    return null;
                }

                // Attempt to resolve as event-underlying object instance method
                var eventType = validationContext.StreamTypeService.EventTypes[prefixedStreamNumber];
                var type = eventType.UnderlyingType;

                var remainderChain = new List<ExprChainedSpec>(_chainSpec);
                remainderChain.RemoveAt(0);

                ExprValidationException methodEx = null;
                ExprDotEval[] underlyingMethodChain = null;
                try
                {
                    typeInfo = EPTypeHelper.SingleValue(type);
                    underlyingMethodChain =
                        ExprDotNodeUtility.GetChainEvaluators(
                            prefixedStreamNumber, typeInfo, remainderChain, validationContext, false,
                            new ExprDotNodeFilterAnalyzerInputStream(prefixedStreamNumber)).ChainWithUnpack;
                }
                catch (ExprValidationException ex)
                {
                    methodEx = ex;
                    // expected - may not be able to find the methods on the underlying
                }

                ExprDotEval[] eventTypeMethodChain = null;
                ExprValidationException enumDatetimeEx = null;
                try
                {
                    typeInfo = EPTypeHelper.SingleEvent(eventType);
                    var chain = ExprDotNodeUtility.GetChainEvaluators(
                        prefixedStreamNumber, typeInfo, remainderChain, validationContext, false,
                        new ExprDotNodeFilterAnalyzerInputStream(prefixedStreamNumber));
                    eventTypeMethodChain = chain.ChainWithUnpack;
                    _filterExprAnalyzerAffector = chain.FilterAnalyzerDesc;
                }
                catch (ExprValidationException ex)
                {
                    enumDatetimeEx = ex;
                    // expected - may not be able to find the methods on the underlying
                }

                if (underlyingMethodChain != null)
                {
                    _exprEvaluator = new ExprDotEvalStreamMethod(this, prefixedStreamNumber, underlyingMethodChain);
                    _streamNumReferenced = prefixedStreamNumber;
                }
                else if (eventTypeMethodChain != null)
                {
                    _exprEvaluator = new ExprDotEvalStreamEventBean(this, prefixedStreamNumber, eventTypeMethodChain);
                    _streamNumReferenced = prefixedStreamNumber;
                }

                if (_exprEvaluator != null)
                {
                    return null;
                }
                else
                {
                    if (ExprDotNodeUtility.IsDatetimeOrEnumMethod(remainderChain[0].Name))
                    {
                        prefixedStreamNumException = enumDatetimeEx;
                    }
                    else
                    {
                        prefixedStreamNumException =
                            new ExprValidationException(
                                "Failed to solve '" + remainderChain[0].Name +
                                "' to either a date-time or enumeration method, an event property or a method on the event underlying object: " +
                                methodEx.Message, methodEx);
                    }
                }
            }

            // There no root node, in this case the classname or property name is provided as part of the chain.
            // Such as "MyClass.MyStaticLib(...)" or "mycollectionproperty.DoIt(...)"
            //
            var modifiedChain = new List<ExprChainedSpec>(_chainSpec);
            var firstItem = modifiedChain.DeleteAt(0);

            propertyInfoPair = null;
            try
            {
                propertyInfoPair = ExprIdentNodeUtil.GetTypeFromStream(
                    streamTypeService, firstItem.Name, streamTypeService.HasPropertyAgnosticType, true);
            }
            catch (ExprValidationPropertyException)
            {
                // not a property
            }

            // If property then treat it as such
            ExprDotNodeRealizedChain evals;
            if (propertyInfoPair != null)
            {

                var propertyName = propertyInfoPair.First.PropertyName;
                var streamId = propertyInfoPair.First.StreamNum;
                var streamType = streamTypeService.EventTypes[streamId];
                ExprEvaluatorEnumeration enumerationEval = null;
                EPType inputType;
                ExprEvaluator rootNodeEvaluator = null;
                EventPropertyGetter getter;

                if (firstItem.Parameters.IsEmpty())
                {
                    getter = streamType.GetGetter(propertyInfoPair.First.PropertyName);

                    var propertyEval = ExprDotNodeUtility.GetPropertyEnumerationSource(
                        propertyInfoPair.First.PropertyName, streamId, streamType, hasEnumerationMethod,
                        validationContext.IsDisablePropertyExpressionEventCollCache);
                    typeInfo = propertyEval.ReturnType;
                    enumerationEval = propertyEval.Enumeration;
                    inputType = propertyEval.ReturnType;
                    rootNodeEvaluator = new PropertyExprEvaluatorNonLambda(
                        streamId, getter, propertyInfoPair.First.PropertyType);
                }
                else
                {
                    // property with parameter - mapped or indexed property
                    var desc =
                        EventTypeUtility.GetNestablePropertyDescriptor(
                            streamTypeService.EventTypes[propertyInfoPair.First.StreamNum], firstItem.Name);
                    if (firstItem.Parameters.Count > 1)
                    {
                        throw new ExprValidationException(
                            "Property '" + firstItem.Name + "' may not be accessed passing 2 or more parameters");
                    }

                    var paramEval = firstItem.Parameters[0].ExprEvaluator;
                    typeInfo = EPTypeHelper.SingleValue(desc.PropertyComponentType);
                    inputType = typeInfo;
                    getter = null;
                    if (desc.IsMapped)
                    {
                        if (paramEval.ReturnType != typeof(string))
                        {
                            throw new ExprValidationException(
                                "Parameter expression to mapped property '" + propertyName +
                                "' is expected to return a string-type value but returns " +
                                paramEval.ReturnType.GetCleanName());
                        }

                        var mappedGetter =
                            propertyInfoPair.First.StreamEventType.GetGetterMapped(propertyInfoPair.First.PropertyName);
                        if (mappedGetter == null)
                        {
                            throw new ExprValidationException(
                                "Mapped property named '" + propertyName + "' failed to obtain getter-object");
                        }

                        rootNodeEvaluator = new PropertyExprEvaluatorNonLambdaMapped(
                            streamId, mappedGetter, paramEval, desc.PropertyComponentType);
                    }

                    if (desc.IsIndexed)
                    {
                        if (paramEval.ReturnType.IsNotInt32())
                        {
                            throw new ExprValidationException(
                                "Parameter expression to mapped property '" + propertyName +
                                "' is expected to return a int?-type value but returns " +
                                paramEval.ReturnType.GetCleanName());
                        }

                        var indexedGetter =
                            propertyInfoPair.First.StreamEventType.GetGetterIndexed(propertyInfoPair.First
                                .PropertyName);
                        if (indexedGetter == null)
                        {
                            throw new ExprValidationException(
                                "Mapped property named '" + propertyName + "' failed to obtain getter-object");
                        }

                        rootNodeEvaluator = new PropertyExprEvaluatorNonLambdaIndexed(
                            streamId, indexedGetter, paramEval, desc.PropertyComponentType);
                    }
                }

                if (typeInfo == null)
                {
                    throw new ExprValidationException(
                        "Property '" + propertyName + "' is not a mapped or indexed property");
                }

                // try to build chain based on the input (non-fragment)
                var filterAnalyzerInputProp = new ExprDotNodeFilterAnalyzerInputProp(
                    propertyInfoPair.First.StreamNum, propertyInfoPair.First.PropertyName);
                var rootIsEventBean = false;
                try
                {
                    evals = ExprDotNodeUtility.GetChainEvaluators(
                        streamId, inputType, modifiedChain, validationContext, _isDuckTyping, filterAnalyzerInputProp);
                }
                catch (ExprValidationException)
                {

                    // try building the chain based on the fragment event type (i.e. A.After(B) based on A-configured start time where A is a fragment)
                    var fragment = propertyInfoPair.First.FragmentEventType;
                    if (fragment == null)
                    {
                        throw;
                    }

                    EPType fragmentTypeInfo;
                    if (fragment.IsIndexed)
                    {
                        fragmentTypeInfo = EPTypeHelper.CollectionOfEvents(fragment.FragmentType);
                    }
                    else
                    {
                        fragmentTypeInfo = EPTypeHelper.SingleEvent(fragment.FragmentType);
                    }

                    rootIsEventBean = true;
                    evals = ExprDotNodeUtility.GetChainEvaluators(
                        propertyInfoPair.First.StreamNum, fragmentTypeInfo, modifiedChain, validationContext,
                        _isDuckTyping, filterAnalyzerInputProp);
                    rootNodeEvaluator = new PropertyExprEvaluatorNonLambdaFragment(
                        streamId, getter, fragment.FragmentType.UnderlyingType);
                }

                _exprEvaluator = new ExprDotEvalRootChild(
                    hasEnumerationMethod, this, rootNodeEvaluator, enumerationEval, inputType, evals.Chain,
                    evals.ChainWithUnpack, !rootIsEventBean);
                _filterExprAnalyzerAffector = evals.FilterAnalyzerDesc;
                _streamNumReferenced = propertyInfoPair.First.StreamNum;
                _rootPropertyName = propertyInfoPair.First.PropertyName;
                return null;
            }

            // If variable then resolve as such
            var contextNameVariable = validationContext.VariableService.IsContextVariable(firstItem.Name);
            if (contextNameVariable != null)
            {
                throw new ExprValidationException("Method invocation on context-specific variable is not supported");
            }

            var variableReader = validationContext.VariableService.GetReader(
                firstItem.Name, EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID);
            if (variableReader != null)
            {
                EPType typeInfoX;
                ExprDotStaticMethodWrap wrap;
                if (variableReader.VariableMetaData.VariableType.IsArray)
                {
                    typeInfoX =
                        EPTypeHelper.CollectionOfSingleValue(
                            variableReader.VariableMetaData.VariableType.GetElementType());
                    wrap = new ExprDotStaticMethodWrapArrayScalar(
                        variableReader.VariableMetaData.VariableName,
                        variableReader.VariableMetaData.VariableType.GetElementType());
                }
                else if (variableReader.VariableMetaData.EventType != null)
                {
                    typeInfoX = EPTypeHelper.SingleEvent(variableReader.VariableMetaData.EventType);
                    wrap = null;
                }
                else
                {
                    typeInfoX = EPTypeHelper.SingleValue(variableReader.VariableMetaData.VariableType);
                    wrap = null;
                }

                var evalsX = ExprDotNodeUtility.GetChainEvaluators(
                    null, typeInfoX, modifiedChain, validationContext, false,
                    new ExprDotNodeFilterAnalyzerInputStatic());
                _exprEvaluator = new ExprDotEvalVariable(this, variableReader, wrap, evalsX.ChainWithUnpack);
                return null;
            }

            // try resolve as enumeration class with value
            var enumconstant = TypeHelper.ResolveIdentAsEnumConst(
                firstItem.Name, validationContext.EngineImportService, false);
            if (enumconstant != null)
            {

                // try resolve method
                var methodSpec = modifiedChain[0];
                var enumvalue = firstItem.Name;
                var handler = new ProxyExprNodeUtilResolveExceptionHandler
                {
                    ProcHandle = ex => new ExprValidationException(
                        "Failed to resolve method '" + methodSpec.Name +
                        "' on enumeration value '" + enumvalue + "': " + ex.Message)
                };
                var wildcardType = validationContext.StreamTypeService.EventTypes.Length != 1
                    ? null
                    : validationContext.StreamTypeService.EventTypes[0];
                var methodDesc =
                    ExprNodeUtility.ResolveMethodAllowWildcardAndStream(
                        enumconstant.GetType().FullName,
                        enumconstant.GetType(),
                        methodSpec.Name,
                        methodSpec.Parameters,
                        validationContext.EngineImportService, validationContext.EventAdapterService,
                        validationContext.StatementId, wildcardType != null, wildcardType, handler, methodSpec.Name,
                        validationContext.TableService, streamTypeService.EngineURIQualifier);

                // method resolved, hook up
                modifiedChain.RemoveAt(0); // we identified this piece
                var optionalLambdaWrapX =
                    ExprDotStaticMethodWrapFactory.Make(
                        methodDesc.ReflectionMethod, validationContext.EventAdapterService, modifiedChain, null);
                var typeInfoX = optionalLambdaWrapX != null
                    ? optionalLambdaWrapX.TypeInfo
                    : EPTypeHelper.SingleValue(methodDesc.FastMethod.ReturnType);

                var evalsX = ExprDotNodeUtility.GetChainEvaluators(
                    null, typeInfoX, modifiedChain, validationContext, false,
                    new ExprDotNodeFilterAnalyzerInputStatic());
                _exprEvaluator = new ExprDotEvalStaticMethod(
                    validationContext.StatementName, firstItem.Name,
                    methodDesc.FastMethod,
                    methodDesc.ChildEvals,
                    false, optionalLambdaWrapX, evalsX.ChainWithUnpack,
                    false, enumconstant);
                return null;
            }

            // if prefixed by a stream name, we are giving up
            if (prefixedStreamNumException != null)
            {
                throw prefixedStreamNumException;
            }

            // If class then resolve as class
            var secondItem = modifiedChain.DeleteAt(0);

            var allowWildcard = validationContext.StreamTypeService.EventTypes.Length == 1;
            EventType streamZeroType = null;
            if (validationContext.StreamTypeService.EventTypes.Length > 0)
            {
                streamZeroType = validationContext.StreamTypeService.EventTypes[0];
            }

            var method = ExprNodeUtility.ResolveMethodAllowWildcardAndStream(
                firstItem.Name, null, secondItem.Name, secondItem.Parameters, validationContext.EngineImportService,
                validationContext.EventAdapterService, validationContext.StatementId, allowWildcard, streamZeroType,
                new ExprNodeUtilResolveExceptionHandlerDefault(firstItem.Name + "." + secondItem.Name, false),
                secondItem.Name, validationContext.TableService, streamTypeService.EngineURIQualifier);

            var isConstantParameters = method.IsAllConstants && _isUdfCache;
            _isReturnsConstantResult = isConstantParameters && modifiedChain.IsEmpty();

            // this may return a pair of null if there is no lambda or the result cannot be wrapped for lambda-function use
            var optionalLambdaWrap = ExprDotStaticMethodWrapFactory.Make(
                method.ReflectionMethod, validationContext.EventAdapterService, modifiedChain, null);
            typeInfo = optionalLambdaWrap != null
                ? optionalLambdaWrap.TypeInfo
                : EPTypeHelper.SingleValue(method.ReflectionMethod.ReturnType);

            evals = ExprDotNodeUtility.GetChainEvaluators(
                null, typeInfo, modifiedChain, validationContext, false, new ExprDotNodeFilterAnalyzerInputStatic());
            _exprEvaluator = new ExprDotEvalStaticMethod(
                validationContext.StatementName, firstItem.Name, method.FastMethod, method.ChildEvals,
                isConstantParameters, optionalLambdaWrap, evals.ChainWithUnpack, false, null);
            return null;
        }

        public FilterExprAnalyzerAffector GetAffector(bool isOuterJoin)
        {
            return isOuterJoin ? null : _filterExprAnalyzerAffector;
        }

        private ExprEvaluator GetPropertyPairEvaluator(
            ExprEvaluator parameterEval,
            Pair<PropertyResolutionDescriptor, string> propertyInfoPair,
            ExprValidationContext validationContext)
        {
            var propertyName = propertyInfoPair.First.PropertyName;
            var propertyDesc =
                EventTypeUtility.GetNestablePropertyDescriptor(propertyInfoPair.First.StreamEventType, propertyName);
            if (propertyDesc == null || (!propertyDesc.IsMapped && !propertyDesc.IsIndexed))
            {
                throw new ExprValidationException(
                    "Unknown single-row function, aggregation function or mapped or indexed property named '" +
                    propertyName + "' could not be resolved");
            }

            var streamNum = propertyInfoPair.First.StreamNum;
            if (propertyDesc.IsMapped)
            {
                if (parameterEval.ReturnType != typeof(string))
                {
                    throw new ExprValidationException(
                        "Parameter expression to mapped property '" + propertyDesc.PropertyName +
                        "' is expected to return a string-type value but returns " +
                        parameterEval.ReturnType.GetCleanName());
                }

                var mappedGetter =
                    propertyInfoPair.First.StreamEventType.GetGetterMapped(propertyInfoPair.First.PropertyName);
                if (mappedGetter == null)
                {
                    throw new ExprValidationException(
                        "Mapped property named '" + propertyName + "' failed to obtain getter-object");
                }

                return new ExprDotEvalPropertyExprMapped(
                    validationContext.StatementName, propertyDesc.PropertyName, streamNum, parameterEval,
                    propertyDesc.PropertyComponentType, mappedGetter);
            }
            else
            {
                if (parameterEval.ReturnType.IsNotInt32())
                {
                    throw new ExprValidationException(
                        "Parameter expression to indexed property '" + propertyDesc.PropertyName +
                        "' is expected to return a int-type value but returns " +
                        parameterEval.ReturnType.GetCleanName());
                }

                var indexedGetter =
                    propertyInfoPair.First.StreamEventType.GetGetterIndexed(propertyInfoPair.First.PropertyName);
                if (indexedGetter == null)
                {
                    throw new ExprValidationException(
                        "Indexed property named '" + propertyName + "' failed to obtain getter-object");
                }

                return new ExprDotEvalPropertyExprIndexed(
                    validationContext.StatementName, propertyDesc.PropertyName, streamNum, parameterEval,
                    propertyDesc.PropertyComponentType, indexedGetter);
            }
        }

        private int PrefixedStreamName(IList<ExprChainedSpec> chainSpec, StreamTypeService streamTypeService)
        {
            if (chainSpec.Count < 1)
            {
                return -1;
            }

            var spec = chainSpec[0];
            if (spec.Parameters.Count > 0 && !spec.IsProperty)
            {
                return -1;
            }

            return streamTypeService.GetStreamNumForStreamName(spec.Name);
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            base.Accept(visitor);
            ExprNodeUtility.AcceptChain(visitor, _chainSpec);
        }

        public override void Accept(ExprNodeVisitorWithParent visitor)
        {
            base.Accept(visitor);
            ExprNodeUtility.AcceptChain(visitor, _chainSpec);
        }

        public override void AcceptChildnodes(ExprNodeVisitorWithParent visitor, ExprNode parent)
        {
            base.AcceptChildnodes(visitor, parent);
            ExprNodeUtility.AcceptChain(visitor, _chainSpec, this);
        }

        public override void ReplaceUnlistedChildNode(ExprNode nodeToReplace, ExprNode newNode)
        {
            ExprNodeUtility.ReplaceChainChildNode(nodeToReplace, newNode, _chainSpec);
        }

        public IList<ExprChainedSpec> ChainSpec => _chainSpec;

        public override ExprEvaluator ExprEvaluator => _exprEvaluator;

        public override bool IsConstantResult => _isReturnsConstantResult;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (ChildNodes.Count != 0)
            {
                writer.Write(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(ChildNodes[0]));
            }

            ExprNodeUtility.ToExpressionString(_chainSpec, writer, ChildNodes.Count != 0, null);
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.MINIMUM;

        public IDictionary<string, object> EventType => null;

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            var other = node as ExprDotNodeImpl;
            if (other == null)
            {
                return false;
            }

            if (other._chainSpec.Count != _chainSpec.Count)
            {
                return false;
            }

            for (var i = 0; i < _chainSpec.Count; i++)
            {
                if (!Equals(_chainSpec[i], other._chainSpec[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public IList<ExprNode> AdditionalNodes => ExprNodeUtility.CollectChainParameters(_chainSpec);

        public string IsVariableOpGetName(VariableService variableService)
        {
            VariableMetaData metaData = null;
            if (_chainSpec.Count > 0 && _chainSpec[0].IsProperty)
            {
                metaData = variableService.GetVariableMetaData(_chainSpec[0].Name);
            }

            return metaData == null ? null : metaData.VariableName;
        }

        private ExprValidationException HandleNotFound(string name)
        {
            var appDotMethodDidYouMean = GetAppDotMethodDidYouMean();
            var message =
                "Unknown single-row function, expression declaration, script or aggregation function named '" + name +
                "' could not be resolved";
            if (appDotMethodDidYouMean != null)
            {
                message += " (did you mean '" + appDotMethodDidYouMean + "')";
            }

            return new ExprValidationException(message);
        }

        private String GetAppDotMethodDidYouMean()
        {
            switch (_chainSpec[0].Name.ToLowerInvariant())
            {
                case "rectangle":
                    return "rectangle.intersects";
                case "point":
                    return "point.inside";
            }

            return null;
        }

        private ExprAppDotMethodImpl GetAppDotMethod(bool filterExpression)
        {
            if (_chainSpec.Count < 2)
            {
                return null;
            }

            var lhsName = _chainSpec[0].Name.ToLowerInvariant();
            var operationName = _chainSpec[1].Name.ToLowerInvariant();
            var pointInside = lhsName.Equals("point") && operationName.Equals("inside");
            var rectangleIntersects = lhsName.Equals("rectangle") && operationName.Equals("intersects");
            if (!pointInside && !rectangleIntersects)
            {
                return null;
            }

            if (_chainSpec[1].Parameters.Count != 1)
            {
                throw GetAppDocMethodException(lhsName, operationName);
            }

            var param = _chainSpec[1].Parameters[0];
            if (!(param is ExprDotNode))
            {
                throw GetAppDocMethodException(lhsName, operationName);
            }

            var compared = (ExprDotNode) _chainSpec[1].Parameters[0];
            if (compared.ChainSpec.Count != 1)
            {
                throw GetAppDocMethodException(lhsName, operationName);
            }

            var rhsName = compared.ChainSpec[0].Name.ToLowerInvariant();
            var pointInsideRectangle = pointInside && rhsName.Equals("rectangle");
            var rectangleIntersectsRectangle = rectangleIntersects && rhsName.Equals("rectangle");
            if (!pointInsideRectangle && !rectangleIntersectsRectangle)
            {
                throw GetAppDocMethodException(lhsName, operationName);
            }

            var lhsExpressions = _chainSpec[0].Parameters;
            IList<ExprNode> indexNamedParameter = null;
            IList<ExprNode> lhsExpressionsValues = new List<ExprNode>();
            foreach (var lhsExpression in lhsExpressions)
            {
                if (lhsExpression is ExprNamedParameterNode)
                {
                    var named = (ExprNamedParameterNode) lhsExpression;
                    if (String.Equals(named.ParameterName, ExprDotNodeConstants.FILTERINDEX_NAMED_PARAMETER,
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!filterExpression)
                        {
                            throw new ExprValidationException(
                                "The '" + named.ParameterName +
                                "' named parameter can only be used in in filter expressions");
                        }

                        indexNamedParameter = named.ChildNodes;
                    }
                    else
                    {
                        throw new ExprValidationException(lhsName + " does not accept '" + named.ParameterName +
                                                          "' as a named parameter");
                    }
                }
                else
                {
                    lhsExpressionsValues.Add(lhsExpression);
                }
            }

            var lhs = ExprNodeUtility.ToArray(lhsExpressionsValues);
            var rhs = ExprNodeUtility.ToArray(compared.ChainSpec[0].Parameters);

            EngineImportApplicationDotMethod predefined;
            if (pointInsideRectangle)
            {
                predefined = new EngineImportApplicationDotMethodPointInsideRectangle(
                    lhsName, lhs, operationName, rhsName, rhs, indexNamedParameter);
            }
            else
            {
                predefined = new EngineImportApplicationDotMethodRectangeIntersectsRectangle(
                    lhsName, lhs, operationName, 
                    rhsName, rhs, indexNamedParameter);
            }

            return new ExprAppDotMethodImpl(predefined);
        }

        private ExprValidationException GetAppDocMethodException(String lhsName, String operationName)
        {
            return new ExprValidationException(lhsName + "." + operationName +
                                               " requires a single rectangle as parameter");
        }
    }
} // end of namespace
