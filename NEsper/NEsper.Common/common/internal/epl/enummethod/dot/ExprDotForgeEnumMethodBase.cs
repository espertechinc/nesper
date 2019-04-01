///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public abstract class ExprDotForgeEnumMethodBase : ExprDotForgeEnumMethod,
        ExpressionResultCacheStackEntry
    {
        internal bool cache;
        internal int enumEvalNumRequiredEvents;
        internal EnumForge enumForge;

        internal EnumMethodEnum enumMethodEnum;
        internal string enumMethodUsedName;
        internal int streamCountIncoming;
        internal EPType typeInfo;

        public EnumMethodEnum EnumMethodEnum => enumMethodEnum;

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitEnumeration(enumMethodEnum.NameCamel);
        }

        public ExprDotEval DotEvaluator => new ExprDotForgeEnumMethodEval(
            this, enumForge.EnumEvaluator, cache, enumEvalNumRequiredEvents);

        public CodegenExpression Codegen(
            CodegenExpression inner, Type innerType, CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope)
        {
            return ExprDotForgeEnumMethodEval.Codegen(
                this, inner, innerType, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public void Init(
            int? streamOfProviderIfApplicable, EnumMethodEnum enumMethodEnum, string enumMethodUsedName,
            EPType typeInfo, IList<ExprNode> parameters, ExprValidationContext validationContext)
        {
            var eventTypeColl = typeInfo.GetEventTypeMultiValued();
            var eventTypeBean = typeInfo.GetEventTypeSingleValued();
            var collectionComponentType = typeInfo.GetClassMultiValued();

            this.enumMethodEnum = enumMethodEnum;
            this.enumMethodUsedName = enumMethodUsedName;
            streamCountIncoming = validationContext.StreamTypeService.EventTypes.Length;

            if (eventTypeColl == null && collectionComponentType == null && eventTypeBean == null) {
                throw new ExprValidationException(
                    "Invalid input for built-in enumeration method '" + enumMethodUsedName +
                    "', expecting collection of event-type or scalar values as input, received " +
                    typeInfo.ToTypeDescriptive());
            }

            // compile parameter abstract for validation against available footprints
            DotMethodFPProvided footprintProvided = DotMethodUtil.GetProvidedFootprint(parameters);

            // validate parameters
            DotMethodInputTypeMatcher inputTypeMatcher = new ProxyDotMethodInputTypeMatcher {
                ProcMatches = fp => {
                    if (fp.Input == DotMethodFPInputEnum.EVENTCOLL && eventTypeBean == null &&
                        eventTypeColl == null) {
                        return false;
                    }

                    if (fp.Input == DotMethodFPInputEnum.SCALAR_ANY && collectionComponentType == null) {
                        return false;
                    }

                    return true;
                }
            };

            DotMethodFP footprint = DotMethodUtil.ValidateParametersDetermineFootprint(
                enumMethodEnum.Footprints, DotMethodTypeEnum.ENUM, enumMethodUsedName, footprintProvided,
                inputTypeMatcher);

            // validate input criteria met for this footprint
            if (footprint.Input != DotMethodFPInputEnum.ANY) {
                var message = "Invalid input for built-in enumeration method '" + enumMethodUsedName + "' and " +
                              footprint.Parameters.Length + "-parameter footprint, expecting collection of ";
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

            IList<ExprDotEvalParam> bodiesAndParameters = new List<ExprDotEvalParam>();
            var count = 0;
            var inputEventType = eventTypeBean == null ? eventTypeColl : eventTypeBean;
            foreach (var node in parameters) {
                var bodyAndParameter = GetBodyAndParameter(
                    enumMethodUsedName, count++, node, inputEventType, collectionComponentType, validationContext,
                    bodiesAndParameters, footprint);
                bodiesAndParameters.Add(bodyAndParameter);
            }

            enumForge = GetEnumForge(
                validationContext.StreamTypeService, enumMethodUsedName, bodiesAndParameters, inputEventType,
                collectionComponentType, streamCountIncoming,
                validationContext.IsDisablePropertyExpressionEventCollCache, validationContext.StatementRawInfo,
                validationContext.StatementCompileTimeService);
            enumEvalNumRequiredEvents = enumForge.StreamNumSize;

            // determine the stream ids of event properties asked for in the evaluator(s)
            var streamsRequired = new HashSet<int>();
            var visitor = new ExprNodeIdentifierCollectVisitor();
            foreach (var desc in bodiesAndParameters) {
                desc.Body.Accept(visitor);
                foreach (var ident in visitor.ExprProperties) {
                    streamsRequired.Add(ident.StreamId);
                }
            }

            if (streamOfProviderIfApplicable != null) {
                streamsRequired.Add(streamOfProviderIfApplicable.Value);
            }

            // We turn on caching if the stack is not empty (we are an inner lambda) and the dependency does not include the stream.
            var isInner = !enumCallStackHelper.PopLambda();
            if (isInner) {
                // If none of the properties that the current lambda uses comes from the ultimate parent(s) or subsequent streams, then cache.
                Deque<ExpressionResultCacheStackEntry> parents = enumCallStackHelper.GetStack();
                var found = false;
                foreach (var req in streamsRequired) {
                    var first = (ExprDotForgeEnumMethodBase) parents.First;
                    var parentIncoming = first.streamCountIncoming - 1;
                    var selfAdded = streamCountIncoming; // the one we use ourselfs
                    if (req > parentIncoming && req < selfAdded) {
                        found = true;
                    }
                }

                cache = !found;
            }
        }

        public EPType TypeInfo {
            get => typeInfo;
            set => typeInfo = value;
        }

        public abstract EventType[] GetAddStreamTypes(
            string enumMethodUsedName, IList<string> goesToNames, EventType inputEventType,
            Type collectionComponentType, IList<ExprDotEvalParam> bodiesAndParameters,
            StatementRawInfo statementRawInfo, StatementCompileTimeServices services);

        public abstract EnumForge GetEnumForge(
            StreamTypeService streamTypeService, string enumMethodUsedName, IList<ExprDotEvalParam> bodiesAndParameters,
            EventType inputEventType, Type collectionComponentType, int numStreamsIncoming,
            bool disablePropertyExpressionEventCollCache, StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services);

        private ExprDotEvalParam GetBodyAndParameter(
            string enumMethodUsedName,
            int parameterNum,
            ExprNode parameterNode,
            EventType inputEventType,
            Type collectionComponentType,
            ExprValidationContext validationContext,
            IList<ExprDotEvalParam> priorParameters,
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
            var additionalTypes = GetAddStreamTypes(
                enumMethodUsedName, goesNode.GoesToNames, inputEventType, collectionComponentType, priorParameters,
                validationContext.StatementRawInfo, validationContext.StatementCompileTimeService);
            string[] additionalStreamNames = goesNode.GoesToNames.ToArray();

            ValidateDuplicateStreamNames(validationContext.StreamTypeService.StreamNames, additionalStreamNames);

            // add name and type to list of known types
            var addTypes = (EventType[]) CollectionUtil.ArrayExpandAddElements(
                validationContext.StreamTypeService.EventTypes, additionalTypes);
            var addNames = (string[]) CollectionUtil.ArrayExpandAddElements(
                validationContext.StreamTypeService.StreamNames, additionalStreamNames);

            var types = new StreamTypeServiceImpl(
                addTypes, addNames, new bool[addTypes.Length], false,
                validationContext.StreamTypeService.IsOptionalStreams);

            // validate expression body
            var filter = goesNode.ChildNodes[0];
            try {
                var filterValidationContext = new ExprValidationContext(types, validationContext);
                filter = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.DECLAREDEXPRBODY, filter, filterValidationContext);
            }
            catch (ExprValidationException ex) {
                throw new ExprValidationException(
                    "Error validating enumeration method '" + enumMethodUsedName + "' parameter " + parameterNum +
                    ": " + ex.Message, ex);
            }

            var filterForge = filter.Forge;
            var expectedType = footprint.Parameters[parameterNum].Type;
            // Lambda-methods don't use a specific expected return-type, so passing null for type is fine.
            EPLValidationUtil.ValidateParameterType(
                enumMethodUsedName, DotMethodTypeEnum.ENUM.GetTypeName(), false, expectedType, null,
                filterForge.EvaluationType, parameterNum, filter);

            var numStreamsIncoming = validationContext.StreamTypeService.EventTypes.Length;
            return new ExprDotEvalParamLambda(
                parameterNum, filter, filterForge,
                numStreamsIncoming, goesNode.GoesToNames, additionalTypes);
        }

        private void ValidateDuplicateStreamNames(string[] streamNames, string[] additionalStreamNames)
        {
            for (var added = 0; added < additionalStreamNames.Length; added++) {
                for (var exist = 0; exist < streamNames.Length; exist++) {
                    if (streamNames[exist] != null &&
                        streamNames[exist].Equals(additionalStreamNames[added], StringComparison.InvariantCultureIgnoreCase)) {
                        var message = "Error validating enumeration method '" + enumMethodUsedName +
                                      "', the lambda-parameter name '" + additionalStreamNames[added] +
                                      "' has already been declared in this context";
                        throw new ExprValidationException(message);
                    }
                }
            }
        }

        public override string ToString()
        {
            return GetType().GetSimpleName() +
                   " lambda=" + enumMethodEnum;
        }
    }
} // end of namespace