///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.enummethod.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.methodbase;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.enummethod.dot
{
    public abstract class ExprDotEvalEnumMethodBase
        : ExprDotEvalEnumMethod
        , ExpressionResultCacheStackEntry
    {
        private EnumMethodEnum _enumMethodEnum;
        private String _enumMethodUsedName;
        private int _streamCountIncoming;
        private EnumEval _enumEval;
        private int _enumEvalNumRequiredEvents;
        private EPType _typeInfo;

        private bool _cache;
        private long _contextNumber = 0;

        protected ExprDotEvalEnumMethodBase()
        {
        }

        public abstract EventType[] GetAddStreamTypes(string enumMethodUsedName, IList<string> goesToNames, EventType inputEventType, Type collectionComponentType, IList<ExprDotEvalParam> bodiesAndParameters, EventAdapterService eventAdapterService);

        public abstract EnumEval GetEnumEval(
            MethodResolutionService methodResolutionService,
            EventAdapterService eventAdapterService,
            StreamTypeService streamTypeService,
            String statementId,
            String enumMethodUsedName,
            IList<ExprDotEvalParam> bodiesAndParameters,
            EventType inputEventType,
            Type collectionComponentType,
            int numStreamsIncoming,
            bool disablePropertyExpressionEventCollCache);

        public EnumMethodEnum EnumMethodEnum
        {
            get { return _enumMethodEnum; }
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitEnumeration(_enumMethodEnum.GetNameCamel());
        }

        public void Init(
            int? streamOfProviderIfApplicable,
            EnumMethodEnum enumMethodEnum,
            String enumMethodUsedName,
            EPType typeInfo,
            IList<ExprNode> parameters,
            ExprValidationContext validationContext)
        {
            var eventTypeColl = EPTypeHelper.GetEventTypeMultiValued(typeInfo);
            var eventTypeBean = EPTypeHelper.GetEventTypeSingleValued(typeInfo);
            var collectionComponentType = EPTypeHelper.GetClassMultiValued(typeInfo);

            _enumMethodEnum = enumMethodEnum;
            _enumMethodUsedName = enumMethodUsedName;
            _streamCountIncoming = validationContext.StreamTypeService.EventTypes.Length;

            if (eventTypeColl == null && collectionComponentType == null && eventTypeBean == null)
            {
                throw new ExprValidationException(
                    "Invalid input for built-in enumeration method '" + enumMethodUsedName +
                    "', expecting collection of event-type or scalar values as input, received " +
                    EPTypeHelper.ToTypeDescriptive(typeInfo));
            }

            // compile parameter abstract for validation against available footprints
            var footprintProvided = DotMethodUtil.GetProvidedFootprint(parameters);

            // validate parameters
            DotMethodInputTypeMatcher inputTypeMatcher = new ProxyDotMethodInputTypeMatcher
            {
                ProcMatches = fp =>
                {
                    if (fp.Input == DotMethodFPInputEnum.EVENTCOLL && eventTypeBean == null && eventTypeColl == null)
                    {
                        return false;
                    }
                    if (fp.Input == DotMethodFPInputEnum.SCALAR_ANY && collectionComponentType == null)
                    {
                        return false;
                    }
                    return true;
                }
            };

            var footprint = DotMethodUtil.ValidateParametersDetermineFootprint(
                enumMethodEnum.GetFootprints(), DotMethodTypeEnum.ENUM, enumMethodUsedName, footprintProvided,
                inputTypeMatcher);

            // validate input criteria met for this footprint
            if (footprint.Input != DotMethodFPInputEnum.ANY)
            {
                var message = "Invalid input for built-in enumeration method '" + enumMethodUsedName + "' and " +
                                 footprint.Parameters.Length + "-parameter footprint, expecting collection of ";
                var received = " as input, received " + EPTypeHelper.ToTypeDescriptive(typeInfo);
                if (footprint.Input == DotMethodFPInputEnum.EVENTCOLL && eventTypeColl == null)
                {
                    throw new ExprValidationException(message + "events" + received);
                }
                if (footprint.Input.IsScalar() && collectionComponentType == null)
                {
                    throw new ExprValidationException(message + "values (typically scalar values)" + received);
                }
                if (footprint.Input == DotMethodFPInputEnum.SCALAR_NUMERIC && !collectionComponentType.IsNumeric())
                {
                    throw new ExprValidationException(message + "numeric values" + received);
                }
            }

            // manage context of this lambda-expression in regards to outer lambda-expression that may call this one.
            validationContext.ExprEvaluatorContext.ExpressionResultCacheService.PushStack(this);

            var bodiesAndParameters = new List<ExprDotEvalParam>();
            var count = 0;
            var inputEventType = eventTypeBean ?? eventTypeColl;
            foreach (var node in parameters)
            {
                var bodyAndParameter = GetBodyAndParameter(
                    enumMethodUsedName, count++, node, inputEventType, collectionComponentType, validationContext,
                    bodiesAndParameters, footprint);
                bodiesAndParameters.Add(bodyAndParameter);
            }

            _enumEval = GetEnumEval(
                validationContext.MethodResolutionService, validationContext.EventAdapterService,
                validationContext.StreamTypeService, validationContext.StatementId, enumMethodUsedName,
                bodiesAndParameters, inputEventType, collectionComponentType, _streamCountIncoming,
                validationContext.IsDisablePropertyExpressionEventCollCache);
            _enumEvalNumRequiredEvents = _enumEval.StreamNumSize;

            // determine the stream ids of event properties asked for in the Evaluator(s)
            var streamsRequired = new HashSet<int>();
            var visitor = new ExprNodeIdentifierCollectVisitor();
            foreach (var desc in bodiesAndParameters)
            {
                desc.Body.Accept(visitor);
                foreach (var ident in visitor.ExprProperties)
                {
                    streamsRequired.Add(ident.StreamId);
                }
            }
            if (streamOfProviderIfApplicable != null)
            {
                streamsRequired.Add(streamOfProviderIfApplicable.Value);
            }

            // We turn on caching if the stack is not empty (we are an inner lambda) and the dependency does not include the stream.
            var isInner = !validationContext.ExprEvaluatorContext.ExpressionResultCacheService.PopLambda();
            if (isInner)
            {
                // If none of the properties that the current lambda uses comes from the ultimate Parent(s) or subsequent streams, then cache.
                var parents = validationContext.ExprEvaluatorContext.ExpressionResultCacheService.Stack;
                var found = false;
                foreach (var req in streamsRequired)
                {
                    var first = (ExprDotEvalEnumMethodBase) parents.First.Value;
                    var parentIncoming = first._streamCountIncoming - 1;
                    var selfAdded = _streamCountIncoming; // the one we use ourselfs
                    if (req > parentIncoming && req < selfAdded)
                    {
                        found = true;
                    }
                }
                _cache = !found;
            }
        }

        public EPType TypeInfo
        {
            get { return _typeInfo; }
            set { _typeInfo = value; }
        }

        public Object Evaluate(
            Object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target is EventBean)
            {
                target = Collections.SingletonList((EventBean) target);
            }
            if (_cache)
            {
                var cacheValue =
                    exprEvaluatorContext.ExpressionResultCacheService.GetEnumerationMethodLastValue(this);
                if (cacheValue != null)
                {
                    return cacheValue.Result;
                }
                var coll = target.Unwrap<object>();
                if (coll == null)
                {
                    return null;
                }
                var eventsLambda = AllocateCopyEventLambda(eventsPerStream);
                var result = _enumEval.EvaluateEnumMethod(eventsLambda, coll, isNewData, exprEvaluatorContext);
                exprEvaluatorContext.ExpressionResultCacheService.SaveEnumerationMethodLastValue(this, result);
                return result;
            }

            _contextNumber++;
            try
            {
                exprEvaluatorContext.ExpressionResultCacheService.PushContext(_contextNumber);
                var coll = target.Unwrap<object>();
                if (coll == null)
                {
                    return null;
                }
                var eventsLambda = AllocateCopyEventLambda(eventsPerStream);
                return _enumEval.EvaluateEnumMethod(eventsLambda, coll, isNewData, exprEvaluatorContext);
            }
            finally
            {
                exprEvaluatorContext.ExpressionResultCacheService.PopContext();
            }
        }

        private EventBean[] AllocateCopyEventLambda(EventBean[] eventsPerStream)
        {
            var eventsLambda = new EventBean[_enumEvalNumRequiredEvents];
            EventBeanUtility.SafeArrayCopy(eventsPerStream, eventsLambda);
            return eventsLambda;
        }

        private ExprDotEvalParam GetBodyAndParameter(
            String enumMethodUsedName,
            int parameterNum,
            ExprNode parameterNode,
            EventType inputEventType,
            Type collectionComponentType,
            ExprValidationContext validationContext,
            IList<ExprDotEvalParam> priorParameters,
            DotMethodFP footprint)
        {
            // handle an expression that is a constant or other (not =>)
            if (!(parameterNode is ExprLambdaGoesNode))
            {
                // no node subtree validation is required here, the chain parameter validation has taken place in ExprDotNode.validate
                // validation of parameter types has taken place in footprint matching
                return new ExprDotEvalParamExpr(parameterNum, parameterNode, parameterNode.ExprEvaluator);
            }

            var goesNode = (ExprLambdaGoesNode) parameterNode;

            // Get secondary
            var additionalTypes = GetAddStreamTypes(
                enumMethodUsedName, goesNode.GoesToNames, inputEventType, collectionComponentType, priorParameters, validationContext.EventAdapterService);
            var additionalStreamNames = goesNode.GoesToNames.ToArray();

            ValidateDuplicateStreamNames(validationContext.StreamTypeService.StreamNames, additionalStreamNames);

            // add name and type to list of known types
            var addTypes =
                (EventType[])
                    CollectionUtil.ArrayExpandAddElements(
                        validationContext.StreamTypeService.EventTypes, additionalTypes);
            var addNames =
                (String[])
                    CollectionUtil.ArrayExpandAddElements(
                        validationContext.StreamTypeService.StreamNames, additionalStreamNames);

            var types = new StreamTypeServiceImpl(
                addTypes, addNames, new bool[addTypes.Length], null, false);

            // validate expression body
            var filter = goesNode.ChildNodes[0];
            try
            {
                var filterValidationContext = new ExprValidationContext(types, validationContext);
                filter = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.DECLAREDEXPRBODY, filter, filterValidationContext);
            }
            catch (ExprValidationException ex)
            {
                throw new ExprValidationException(
                    "Error validating enumeration method '" + enumMethodUsedName + "' parameter " + parameterNum + ": " +
                    ex.Message, ex);
            }

            var filterEvaluator = filter.ExprEvaluator;
            var expectedType = footprint.Parameters[parameterNum].ParamType;
            // Lambda-methods don't use a specific expected return-type, so passing null for type is fine.
            DotMethodUtil.ValidateSpecificType(
                enumMethodUsedName, DotMethodTypeEnum.ENUM, expectedType, null, filterEvaluator.ReturnType, parameterNum,
                filter);

            var numStreamsIncoming = validationContext.StreamTypeService.EventTypes.Length;
            return new ExprDotEvalParamLambda(
                parameterNum, filter, filterEvaluator,
                numStreamsIncoming, goesNode.GoesToNames, additionalTypes);
        }

        private void ValidateDuplicateStreamNames(String[] streamNames, String[] additionalStreamNames)
        {
            for (var added = 0; added < additionalStreamNames.Length; added++)
            {
                for (var exist = 0; exist < streamNames.Length; exist++)
                {
                    if (streamNames[exist] != null &&
                        string.Equals(
                            streamNames[exist], additionalStreamNames[added], StringComparison.CurrentCultureIgnoreCase))
                    {
                        var message = "Error validating enumeration method '" + _enumMethodUsedName +
                                         "', the lambda-parameter name '" + additionalStreamNames[added] +
                                         "' has already been declared in this context";
                        throw new ExprValidationException(message);
                    }
                }
            }
        }

        public override String ToString()
        {
            return GetType().Name + " lambda=" + _enumMethodEnum;
        }
    }
}