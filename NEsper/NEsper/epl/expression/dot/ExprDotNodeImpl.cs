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
using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.visitor;
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

        [NonSerialized] private ExprDotNodeFilterAnalyzerDesc _exprDotNodeFilterAnalyzerDesc;
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
            // validate all parameters
            ExprNodeUtility.Validate(ExprNodeOrigin.DOTNODEPARAMETER, _chainSpec, validationContext);

            // determine if there are enumeration method expressions in the chain
            bool hasEnumerationMethod = _chainSpec
                .Any(chain => EnumMethodEnumExtensions.IsEnumerationMethod(chain.Name));

            // determine if there is an implied binding, replace first chain element with evaluation node if there is
            if (validationContext.StreamTypeService.HasTableTypes &&
                validationContext.TableService != null &&
                _chainSpec.Count > 1 && _chainSpec[0].IsProperty)
            {
                var tableNode = validationContext.TableService.GetTableNodeChainable(
                    validationContext.StreamTypeService, _chainSpec, validationContext.EngineImportService);
                if (tableNode != null)
                {
                    ExprNode node = ExprNodeUtility.GetValidatedSubtree(
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
            StreamTypeService streamTypeService = validationContext.StreamTypeService;
            if (ChildNodes.Length != 0)
            {
                // the root expression is the first child node
                ExprNode rootNode = ChildNodes[0];
                ExprEvaluator rootNodeEvaluator = rootNode.ExprEvaluator;

                // the root expression may also provide a lambda-function input (Iterator<EventBean>)
                // Determine collection-type and evaluator if any for root node
                ExprDotEnumerationSource enumSrc = ExprDotNodeUtility.GetEnumerationSource(
                    rootNode, validationContext.StreamTypeService, validationContext.EventAdapterService,
                    validationContext.StatementId, hasEnumerationMethod,
                    validationContext.IsDisablePropertyExpressionEventCollCache);

                EPType typeInfo;
                if (enumSrc.ReturnType == null)
                {
                    typeInfo = EPTypeHelper.SingleValue(rootNodeEvaluator.ReturnType);
                        // not a collection type, treat as scalar
                }
                else
                {
                    typeInfo = enumSrc.ReturnType;
                }

                ExprDotNodeRealizedChain evals =
                    ExprDotNodeUtility.GetChainEvaluators(
                        enumSrc.StreamOfProviderIfApplicable, typeInfo, _chainSpec, validationContext, _isDuckTyping,
                        new ExprDotNodeFilterAnalyzerInputExpr());
                _exprEvaluator = new ExprDotEvalRootChild(
                    hasEnumerationMethod, this, rootNodeEvaluator, enumSrc.Enumeration, typeInfo, evals.Chain,
                    evals.ChainWithUnpack, false);
                return null;
            }

            // No root node, and this is a 1-element chain i.e. "Something(param,...)".
            // Plug-in single-row methods are not handled here.
            // Plug-in aggregation methods are not handled here.
            if (_chainSpec.Count == 1)
            {
                ExprChainedSpec spec = _chainSpec[0];
                if (spec.Parameters.IsEmpty())
                {
                    throw HandleNotFound(spec.Name);
                }

                // single-parameter can resolve to a property
                Pair<PropertyResolutionDescriptor, string> propertyInfoPair = null;
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
                        spec.Name, EngineImportService.EXT_SINGLEROW_FUNCTION_TRANSPOSE,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    if (spec.Parameters.Count != 1)
                    {
                        throw new ExprValidationException(
                            "The " + EngineImportService.EXT_SINGLEROW_FUNCTION_TRANSPOSE +
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
            int prefixedStreamNumber = PrefixedStreamName(_chainSpec, validationContext.StreamTypeService);
            if (prefixedStreamNumber != -1)
            {

                ExprChainedSpec specAfterStreamName = _chainSpec[1];

                // Attempt to resolve as property
                Pair<PropertyResolutionDescriptor, string> propertyInfoPair = null;
                try
                {
                    string propName = _chainSpec[0].Name + "." + specAfterStreamName.Name;
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
                EventType eventType = validationContext.StreamTypeService.EventTypes[prefixedStreamNumber];
                Type type = eventType.UnderlyingType;

                var remainderChain = new List<ExprChainedSpec>(_chainSpec);
                remainderChain.RemoveAt(0);

                ExprValidationException methodEx = null;
                ExprDotEval[] underlyingMethodChain = null;
                try
                {
                    EPType typeInfo = EPTypeHelper.SingleValue(type);
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
                    EPType typeInfo = EPTypeHelper.SingleEvent(eventType);
                    ExprDotNodeRealizedChain chain = ExprDotNodeUtility.GetChainEvaluators(
                        prefixedStreamNumber, typeInfo, remainderChain, validationContext, false,
                        new ExprDotNodeFilterAnalyzerInputStream(prefixedStreamNumber));
                    eventTypeMethodChain = chain.ChainWithUnpack;
                    _exprDotNodeFilterAnalyzerDesc = chain.FilterAnalyzerDesc;
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
                                "' to either an date-time or enumeration method, an event property or a method on the event underlying object: " +
                                methodEx.Message, methodEx);
                    }
                }
            }

            // There no root node, in this case the classname or property name is provided as part of the chain.
            // Such as "MyClass.MyStaticLib(...)" or "mycollectionproperty.DoIt(...)"
            //
            var modifiedChain = new List<ExprChainedSpec>(_chainSpec);
            ExprChainedSpec firstItem = modifiedChain.Delete(0);

            Pair<PropertyResolutionDescriptor, string> propertyInfoPair = null;
            try
            {
                propertyInfoPair = ExprIdentNodeUtil.GetTypeFromStream(
                    streamTypeService, firstItem.Name, streamTypeService.HasPropertyAgnosticType, true);
            }
            catch (ExprValidationPropertyException ex)
            {
                // not a property
            }

            // If property then treat it as such
            if (propertyInfoPair != null)
            {

                string propertyName = propertyInfoPair.First.PropertyName;
                int streamId = propertyInfoPair.First.StreamNum;
                EventType streamType = streamTypeService.EventTypes[streamId];
                EPType typeInfo;
                ExprEvaluatorEnumeration enumerationEval = null;
                EPType inputType;
                ExprEvaluator rootNodeEvaluator = null;
                EventPropertyGetter getter;

                if (firstItem.Parameters.IsEmpty())
                {
                    getter = streamType.GetGetter(propertyInfoPair.First.PropertyName);

                    ExprDotEnumerationSourceForProps propertyEval =
                        ExprDotNodeUtility.GetPropertyEnumerationSource(
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
                    EventPropertyDescriptor desc =
                        EventTypeUtility.GetNestablePropertyDescriptor(
                            streamTypeService.EventTypes[propertyInfoPair.First.StreamNum], firstItem.Name);
                    if (firstItem.Parameters.Count > 1)
                    {
                        throw new ExprValidationException(
                            "Property '" + firstItem.Name + "' may not be accessed passing 2 or more parameters");
                    }
                    ExprEvaluator paramEval = firstItem.Parameters[0].ExprEvaluator;
                    typeInfo = EPTypeHelper.SingleValue(desc.PropertyComponentType);
                    inputType = typeInfo;
                    getter = null;
                    if (desc.IsMapped)
                    {
                        if (paramEval.ReturnType != typeof (string))
                        {
                            throw new ExprValidationException(
                                "Parameter expression to mapped property '" + propertyName +
                                "' is expected to return a string-type value but returns " +
                                TypeHelper.GetTypeNameFullyQualPretty(paramEval.ReturnType));
                        }
                        EventPropertyGetterMapped mappedGetter =
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
                        if (TypeHelper.GetBoxedType(paramEval.ReturnType) != typeof (int?))
                        {
                            throw new ExprValidationException(
                                "Parameter expression to mapped property '" + propertyName +
                                "' is expected to return a int?-type value but returns " +
                                TypeHelper.GetTypeNameFullyQualPretty(paramEval.ReturnType));
                        }
                        EventPropertyGetterIndexed indexedGetter =
                            propertyInfoPair.First.StreamEventType.GetGetterIndexed(propertyInfoPair.First.PropertyName);
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
                ExprDotNodeRealizedChain evals;
                var filterAnalyzerInputProp = new ExprDotNodeFilterAnalyzerInputProp(
                    propertyInfoPair.First.StreamNum, propertyInfoPair.First.PropertyName);
                bool rootIsEventBean = false;
                try
                {
                    evals = ExprDotNodeUtility.GetChainEvaluators(
                        streamId, inputType, modifiedChain, validationContext, _isDuckTyping, filterAnalyzerInputProp);
                }
                catch (ExprValidationException ex)
                {

                    // try building the chain based on the fragment event type (i.e. A.After(B) based on A-configured start time where A is a fragment)
                    FragmentEventType fragment = propertyInfoPair.First.FragmentEventType;
                    if (fragment == null)
                    {
                        throw ex;
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
                _exprDotNodeFilterAnalyzerDesc = evals.FilterAnalyzerDesc;
                _streamNumReferenced = propertyInfoPair.First.StreamNum;
                _rootPropertyName = propertyInfoPair.First.PropertyName;
                return null;
            }

            // If variable then resolve as such
            string contextNameVariable = validationContext.VariableService.IsContextVariable(firstItem.Name);
            if (contextNameVariable != null)
            {
                throw new ExprValidationException("Method invocation on context-specific variable is not supported");
            }
            VariableReader variableReader = validationContext.VariableService.GetReader(
                firstItem.Name, EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID);
            if (variableReader != null)
            {
                EPType typeInfo;
                ExprDotStaticMethodWrap wrap;
                if (variableReader.VariableMetaData.VariableType.IsArray)
                {
                    typeInfo =
                        EPTypeHelper.CollectionOfSingleValue(variableReader.VariableMetaData.VariableType.GetElementType());
                    wrap = new ExprDotStaticMethodWrapArrayScalar(
                        variableReader.VariableMetaData.VariableName,
                        variableReader.VariableMetaData.VariableType.GetElementType());
                }
                else if (variableReader.VariableMetaData.EventType != null)
                {
                    typeInfo = EPTypeHelper.SingleEvent(variableReader.VariableMetaData.EventType);
                    wrap = null;
                }
                else
                {
                    typeInfo = EPTypeHelper.SingleValue(variableReader.VariableMetaData.VariableType);
                    wrap = null;
                }

                ExprDotNodeRealizedChain evals = ExprDotNodeUtility.GetChainEvaluators(
                    null, typeInfo, modifiedChain, validationContext, false, new ExprDotNodeFilterAnalyzerInputStatic());
                _exprEvaluator = new ExprDotEvalVariable(this, variableReader, wrap, evals.ChainWithUnpack);
                return null;
            }

            // try resolve as enumeration class with value
            Object enumconstant = TypeHelper.ResolveIdentAsEnumConst(
                firstItem.Name, validationContext.EngineImportService, false);
            if (enumconstant != null)
            {

                // try resolve method
                ExprChainedSpec methodSpec = modifiedChain[0];
                string enumvalue = firstItem.Name;
                var handler = new ProxyExprNodeUtilResolveExceptionHandler()
                {
                    ProcHandle = (ex) =>
                    {
                        return
                            new ExprValidationException(
                                "Failed to resolve method '" + methodSpec.Name +
                                "' on enumeration value '" + enumvalue + "': " + ex.Message);
                    };
                };
                EventType wildcardType = validationContext.StreamTypeService.EventTypes.Length != 1
                    ? null
                    : validationContext.StreamTypeService.EventTypes[0];
                ExprNodeUtilMethodDesc methodDesc =
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
                ExprDotStaticMethodWrap optionalLambdaWrap =
                    ExprDotStaticMethodWrapFactory.Make(
                        methodDesc.ReflectionMethod, validationContext.EventAdapterService, modifiedChain, null);
                EPType typeInfo = optionalLambdaWrap != null
                    ? optionalLambdaWrap.TypeInfo
                    : EPTypeHelper.SingleValue(methodDesc.FastMethod.ReturnType);

                ExprDotNodeRealizedChain evals = ExprDotNodeUtility.GetChainEvaluators(
                    null, typeInfo, modifiedChain, validationContext, false, new ExprDotNodeFilterAnalyzerInputStatic());
                _exprEvaluator = new ExprDotEvalStaticMethod(
                    validationContext.StatementName, firstItem.Name,
                    methodDesc.FastMethod,
                    methodDesc.ChildEvals, 
                    false, optionalLambdaWrap, evals.ChainWithUnpack,
                    false, enumconstant);
                return null;
            }

            // if prefixed by a stream name, we are giving up
            if (prefixedStreamNumException != null)
            {
                throw prefixedStreamNumException;
            }

            // If class then resolve as class
            ExprChainedSpec secondItem = modifiedChain.Remove(0);

            bool allowWildcard = validationContext.StreamTypeService.EventTypes.Length == 1;
            EventType streamZeroType = null;
            if (validationContext.StreamTypeService.EventTypes.Length > 0)
            {
                streamZeroType = validationContext.StreamTypeService.EventTypes[0];
            }

            ExprNodeUtilMethodDesc method = ExprNodeUtility.ResolveMethodAllowWildcardAndStream(
                firstItem.Name, null, secondItem.Name, secondItem.Parameters, validationContext.EngineImportService,
                validationContext.EventAdapterService, validationContext.StatementId, allowWildcard, streamZeroType,
                new ExprNodeUtilResolveExceptionHandlerDefault(firstItem.Name + "." + secondItem.Name, false),
                secondItem.Name, validationContext.TableService, streamTypeService.EngineURIQualifier);

            bool isConstantParameters = method.IsAllConstants && _isUdfCache;
            _isReturnsConstantResult = isConstantParameters && modifiedChain.IsEmpty();

            // this may return a pair of null if there is no lambda or the result cannot be wrapped for lambda-function use
            ExprDotStaticMethodWrap optionalLambdaWrap = ExprDotStaticMethodWrapFactory.Make(
                method.ReflectionMethod, validationContext.EventAdapterService, modifiedChain, null);
            EPType typeInfo = optionalLambdaWrap != null
                ? OptionalLambdaWrap.TypeInfo
                : EPTypeHelper.SingleValue(method.ReflectionMethod.ReturnType);

            ExprDotNodeRealizedChain evals = ExprDotNodeUtility.GetChainEvaluators(
                null, typeInfo, modifiedChain, validationContext, false, new ExprDotNodeFilterAnalyzerInputStatic());
            _exprEvaluator = new ExprDotEvalStaticMethod(
                validationContext.StatementName, firstItem.Name, method.FastMethod, method.ChildEvals,
                isConstantParameters, optionalLambdaWrap, evals.ChainWithUnpack, false, null);
            return null;
        }

        public ExprDotNodeFilterAnalyzerDesc GetExprDotNodeFilterAnalyzerDesc()
        {
            return _exprDotNodeFilterAnalyzerDesc;
        }

        private ExprEvaluator GetPropertyPairEvaluator(
            ExprEvaluator parameterEval,
            Pair<PropertyResolutionDescriptor, string> propertyInfoPair,
            ExprValidationContext validationContext)
        {
            string propertyName = propertyInfoPair.First.PropertyName;
            EventPropertyDescriptor propertyDesc =
                EventTypeUtility.GetNestablePropertyDescriptor(propertyInfoPair.First.StreamEventType, propertyName);
            if (propertyDesc == null || (!propertyDesc.IsMapped && !propertyDesc.IsIndexed))
            {
                throw new ExprValidationException(
                    "Unknown single-row function, aggregation function or mapped or indexed property named '" +
                    propertyName + "' could not be resolved");
            }

            int streamNum = propertyInfoPair.First.StreamNum;
            if (propertyDesc.IsMapped)
            {
                if (parameterEval.ReturnType != typeof (string))
                {
                    throw new ExprValidationException(
                        "Parameter expression to mapped property '" + propertyDesc.PropertyName +
                        "' is expected to return a string-type value but returns " +
                        TypeHelper.GetTypeNameFullyQualPretty(parameterEval.ReturnType));
                }
                EventPropertyGetterMapped mappedGetter =
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
                if (TypeHelper.GetBoxedType(parameterEval.ReturnType) != typeof(int?))
                {
                    throw new ExprValidationException(
                        "Parameter expression to indexed property '" + propertyDesc.PropertyName +
                        "' is expected to return a int?-type value but returns " +
                        TypeHelper.GetTypeNameFullyQualPretty(parameterEval.ReturnType));
                }
                EventPropertyGetterIndexed indexedGetter =
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

        private int PrefixedStreamName(List<ExprChainedSpec> chainSpec, StreamTypeService streamTypeService)
        {
            if (chainSpec.Count < 1)
            {
                return -1;
            }
            ExprChainedSpec spec = chainSpec[0];
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

        public IList<ExprChainedSpec> ChainSpec
        {
            get { return _chainSpec; }
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return _exprEvaluator; }
        }

        public override bool IsConstantResult
        {
            get { return _isReturnsConstantResult; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (ChildNodes.Length != 0)
            {
                writer.Write(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(ChildNodes[0]));
            }
            ExprNodeUtility.ToExpressionString(_chainSpec, writer, ChildNodes.Length != 0, null);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.MINIMUM; }
        }

        public IDictionary<string, object> EventType
        {
            get { return null; }
        }

        public override bool EqualsNode(ExprNode node)
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
            for (int i = 0; i < _chainSpec.Count; i++)
            {
                if (!Equals(_chainSpec[i], other._chainSpec[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public IList<ExprNode> AdditionalNodes
        {
            get { return ExprNodeUtility.CollectChainParameters(_chainSpec); }
        }

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
            return
                new ExprValidationException(
                    "Unknown single-row function, expression declaration, script or aggregation function named '" + name +
                    "' could not be resolved");
        }
    }

} // end of namespace
