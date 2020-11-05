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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public abstract class ExprDotForgeEnumMethodBase : ExprDotForgeEnumMethod,
        ExpressionResultCacheStackEntry
    {
        private EnumMethodDesc _enumMethodDesc;
        private string _enumMethodUsedName;
        private int _streamCountIncoming;
        private EPType _typeInfo;

        public EnumForge EnumForge { get; set; }
        public int EnumEvalNumRequiredEvents { get; set; }
        public bool IsCache { get; set; }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitEnumeration(_enumMethodDesc.EnumMethodName);
        }

        public ExprDotEval DotEvaluator => new ExprDotForgeEnumMethodEval(this, EnumForge.EnumEvaluator, EnumEvalNumRequiredEvents);

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return ExprDotForgeEnumMethodEval.Codegen(this, inner, innerType, parent, symbols, classScope);
        }

        public void Init(
            int? streamOfProviderIfApplicable,
            EnumMethodDesc enumMethodDesc,
            string enumMethodUsedName,
            EPType typeInfo,
            IList<ExprNode> parameters,
            ExprValidationContext validationContext)
        {
            var eventTypeColl = typeInfo.GetEventTypeMultiValued();
            var eventTypeBean = typeInfo.GetEventTypeSingleValued();
            var collectionComponentType = typeInfo.GetClassMultiValued();

            _enumMethodDesc = enumMethodDesc;
            _enumMethodUsedName = enumMethodUsedName;
            _streamCountIncoming = validationContext.StreamTypeService.EventTypes.Length;

            if (eventTypeColl == null && collectionComponentType == null && eventTypeBean == null) {
                throw new ExprValidationException(
                    "Invalid input for built-in enumeration method '" +
                    enumMethodUsedName +
                    "', expecting collection of event-type or scalar values as input, received " +
                    typeInfo.ToTypeDescriptive());
            }

            // compile parameter abstract for validation against available footprints
            var footprintProvided = DotMethodUtil.GetProvidedFootprint(parameters);

            // validate parameters
            DotMethodInputTypeMatcher inputTypeMatcher = new ProxyDotMethodInputTypeMatcher {
                ProcMatches = footprint => {
                    if (footprint.Input == DotMethodFPInputEnum.EVENTCOLL && eventTypeBean == null && eventTypeColl == null) {
                        return false;
                    }

                    if (footprint.Input == DotMethodFPInputEnum.SCALAR_ANY && collectionComponentType == null) {
                        return false;
                    }

                    return true;
                },
            };
            var footprint = DotMethodUtil.ValidateParametersDetermineFootprint(
                enumMethodDesc.Footprints,
                DotMethodTypeEnum.ENUM,
                enumMethodUsedName,
                footprintProvided,
                inputTypeMatcher);

            // validate input criteria met for this footprint
            if (footprint.Input != DotMethodFPInputEnum.ANY) {
                var message = "Invalid input for built-in enumeration method '" +
                              enumMethodUsedName +
                              "' and " +
                              footprint.Parameters.Length +
                              "-parameter footprint, expecting collection of ";
                var received = " as input, received " + typeInfo.ToTypeDescriptive();
                if (footprint.Input == DotMethodFPInputEnum.EVENTCOLL && eventTypeColl == null) {
                    throw new ExprValidationException(message + "events" + received);
                }

                if (footprint.Input.IsScalar() && collectionComponentType == null) {
                    throw new ExprValidationException(message + "values (typically scalar values)" + received);
                }

                if (footprint.Input == DotMethodFPInputEnum.SCALAR_NUMERIC && !collectionComponentType.IsNumeric()) {
                    throw new ExprValidationException(message + "numeric values" + received);
                }
            }

            // manage context of this lambda-expression in regards to outer lambda-expression that may call this one.
            var enumCallStackHelper = validationContext.EnumMethodCallStackHelper;
            enumCallStackHelper.PushStack(this);

            try {
                // initialize
                var inputEventType = eventTypeBean ?? eventTypeColl;
                Initialize(
                    footprint,
                    enumMethodDesc.EnumMethod,
                    enumMethodUsedName,
                    inputEventType,
                    collectionComponentType,
                    parameters,
                    validationContext.StreamTypeService,
                    validationContext.StatementRawInfo,
                    validationContext.StatementCompileTimeService);

                // get-forge-desc-factory
                var forgeDescFactory = GetForgeFactory(
                    footprint,
                    parameters,
                    enumMethodDesc.EnumMethod,
                    enumMethodUsedName,
                    inputEventType,
                    collectionComponentType,
                    validationContext);

                // handle body and parameter list
                var bodiesAndParameters = new List<ExprDotEvalParam>();
                var count = 0;
                foreach (var node in parameters) {
                    var bodyAndParameter = GetBodyAndParameter(forgeDescFactory, enumMethodUsedName, count++, node, validationContext, footprint);
                    bodiesAndParameters.Add(bodyAndParameter);
                }

                var forgeDesc = forgeDescFactory.MakeEnumForgeDesc(
                    bodiesAndParameters,
                    _streamCountIncoming,
                    validationContext.StatementCompileTimeService);
                EnumForge = forgeDesc.Forge;
                _typeInfo = forgeDesc.Type;
                EnumEvalNumRequiredEvents = EnumForge.StreamNumSize;

                // determine the stream ids of event properties asked for in the evaluator(s)
                var streamsRequired = new HashSet<int?>();
                var visitor = new ExprNodeIdentifierCollectVisitor();
                foreach (var desc in bodiesAndParameters) {
                    desc.Body.Accept(visitor);
                    foreach (var ident in visitor.ExprProperties) {
                        streamsRequired.Add(ident.StreamId);
                    }
                }

                if (streamOfProviderIfApplicable != null) {
                    streamsRequired.Add(streamOfProviderIfApplicable);
                }

                // We turn on caching if the stack is not empty (we are an inner lambda) and the dependency does not include the stream.
                var isInner = !enumCallStackHelper.PopLambda();
                if (isInner) {
                    // If none of the properties that the current lambda uses comes from the ultimate parent(s) or subsequent streams, then cache.
                    var parents = enumCallStackHelper.GetStack();
                    var found = false;
                    foreach (int req in streamsRequired) {
                        var first = (ExprDotForgeEnumMethodBase) parents.First;
                        var parentIncoming = first._streamCountIncoming - 1;
                        var selfAdded = _streamCountIncoming; // the one we use ourselfs
                        if (req > parentIncoming && req < selfAdded) {
                            found = true;
                        }
                    }

                    IsCache = !found;
                }
            }
            catch (ExprValidationException) {
                enumCallStackHelper.PopLambda();
                throw;
            }
        }

        public virtual void Initialize(
            DotMethodFP footprint,
            EnumMethodEnum enumMethod,
            string enumMethodUsedName,
            EventType inputEventType,
            Type collectionComponentType,
            IList<ExprNode> parameters,
            StreamTypeService streamTypeService,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            // override as required
        }

        public abstract EnumForgeDescFactory GetForgeFactory(
            DotMethodFP footprint,
            IList<ExprNode> parameters,
            EnumMethodEnum enumMethod,
            string enumMethodUsedName,
            EventType inputEventType,
            Type collectionComponentType,
            ExprValidationContext validationContext);

        public EPType TypeInfo => _typeInfo;

        private ExprDotEvalParam GetBodyAndParameter(
            EnumForgeDescFactory forgeDescFactory,
            string enumMethodUsedName,
            int parameterNum,
            ExprNode parameterNode,
            ExprValidationContext validationContext,
            DotMethodFP footprint)
        {
            // handle an expression that is a constant or other (not =>)
            if (!(parameterNode is ExprLambdaGoesNode)) {
                // no node subtree validation is required here, the chain parameter validation has taken place in ExprDotNode.validate
                // validation of parameter types has taken place in footprint matching
                return new ExprDotEvalParamExpr(parameterNum, parameterNode, parameterNode.Forge);
            }

            var goesNode = (ExprLambdaGoesNode) parameterNode;

            // Get secondary
            var lambdaDesc = forgeDescFactory.GetLambdaStreamTypesForParameter(parameterNum);
            string[] additionalStreamNames = lambdaDesc.StreamNames;
            var additionalEventTypes = lambdaDesc.Types;

            ValidateDuplicateStreamNames(validationContext.StreamTypeService.StreamNames, goesNode.GoesToNames);

            // add name and type to list of known types
            var addTypes = CollectionUtil.ArrayExpandAddElements<EventType>(
                validationContext.StreamTypeService.EventTypes, additionalEventTypes);
            var addNames = CollectionUtil.ArrayExpandAddElements<string>(
                validationContext.StreamTypeService.StreamNames, additionalStreamNames);

            var types = new StreamTypeServiceImpl(
                addTypes,
                addNames,
                new bool[addTypes.Length],
                false,
                validationContext.StreamTypeService.IsOptionalStreams);

            // validate expression body
            var filter = goesNode.ChildNodes[0];
            try {
                var filterValidationContext = new ExprValidationContext(types, validationContext);
                filter = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.DECLAREDEXPRBODY, filter, filterValidationContext);
            }
            catch (ExprValidationException ex) {
                throw new ExprValidationException(
                    "Failed to validate enumeration method '" + enumMethodUsedName + "' parameter " + parameterNum + ": " + ex.Message,
                    ex);
            }

            var filterForge = filter.Forge;
            var expectedType = footprint.Parameters[parameterNum].ParamType;
            // Lambda-methods don't use a specific expected return-type, so passing null for type is fine.
            EPLValidationUtil.ValidateParameterType(
                enumMethodUsedName,
                DotMethodTypeEnum.ENUM.GetTypeName(),
                false,
                expectedType,
                null,
                filterForge.EvaluationType,
                parameterNum,
                filter);

            var numStreamsIncoming = validationContext.StreamTypeService.EventTypes.Length;
            return new ExprDotEvalParamLambda(
                parameterNum,
                filter,
                filterForge,
                numStreamsIncoming,
                goesNode.GoesToNames,
                lambdaDesc);
        }

        private void ValidateDuplicateStreamNames(
            string[] streamNames,
            IList<string> goesToNames)
        {
            for (var nameIdx = 0; nameIdx < goesToNames.Count; nameIdx++) {
                for (var exist = 0; exist < streamNames.Length; exist++) {
                    var currName = goesToNames[nameIdx];
                    if (streamNames[exist] != null && string.Equals(streamNames[exist], currName, StringComparison.InvariantCultureIgnoreCase)) {
                        var message = "Failed to validate enumeration method '" +
                                      _enumMethodUsedName +
                                      "', the lambda-parameter name '" +
                                      goesToNames[nameIdx] +
                                      "' has already been declared in this context";
                        throw new ExprValidationException(message);
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name} lambda={_enumMethodDesc}";
        }
    }
} // end of namespace