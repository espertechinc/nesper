///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.dot
{
    public class ExprDotNodeUtility
    {
        public static bool IsDatetimeOrEnumMethod(string name)
        {
            return
                name.IsEnumerationMethod() ||
                name.IsDateTimeMethod();
        }

        public static ExprDotNodeRealizedChain GetChainEvaluators(
            int? streamOfProviderIfApplicable,
            EPType inputType,
            IList<ExprChainedSpec> chainSpec,
            ExprValidationContext validationContext,
            bool isDuckTyping,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            var methodEvals = new List<ExprDotEval>();
            var currentInputType = inputType;
            EnumMethodEnum? lastLambdaFunc = null;
            var lastElement = chainSpec.IsEmpty() ? null : chainSpec[chainSpec.Count - 1];
            FilterExprAnalyzerAffector filterAnalyzerDesc = null;

            var chainSpecStack = new ArrayDeque<ExprChainedSpec>(chainSpec);
            while (!chainSpecStack.IsEmpty())
            {
                var chainElement = chainSpecStack.RemoveFirst();
                lastLambdaFunc = null;  // reset

                // compile parameters for chain element
                var paramEvals = new ExprEvaluator[chainElement.Parameters.Count];
                var paramTypes = new Type[chainElement.Parameters.Count];
                for (var i = 0; i < chainElement.Parameters.Count; i++)
                {
                    paramEvals[i] = chainElement.Parameters[i].ExprEvaluator;
                    paramTypes[i] = paramEvals[i].ReturnType;
                }

                // check if special 'size' method
                if (currentInputType is ClassMultiValuedEPType)
                {
                    var type = (ClassMultiValuedEPType)currentInputType;
                    if ((chainElement.Name.ToLower() == "size") && paramTypes.Length == 0 && Equals(lastElement, chainElement))
                    {
                        var sizeExpr = new ExprDotEvalArraySize();
                        methodEvals.Add(sizeExpr);
                        currentInputType = sizeExpr.TypeInfo;
                        continue;
                    }
                    if ((chainElement.Name.ToLower() == "get") && paramTypes.Length == 1 && paramTypes[0].IsInt32())
                    {
                        var componentType = type.Component;
                        var get = new ExprDotEvalArrayGet(paramEvals[0], componentType);
                        methodEvals.Add(get);
                        currentInputType = get.TypeInfo;
                        continue;
                    }
                }

                // determine if there is a matching method
                var matchingMethod = false;
                var methodTarget = GetMethodTarget(currentInputType);
                if (methodTarget != null)
                {
                    try
                    {
                        GetValidateMethodDescriptor(methodTarget, chainElement.Name, chainElement.Parameters, validationContext);
                        matchingMethod = true;
                    }
                    catch (ExprValidationException)
                    {
                        // expected
                    }
                }

                // resolve lambda
                if (chainElement.Name.IsEnumerationMethod() && (!matchingMethod || methodTarget.IsArray || methodTarget.IsImplementsInterface(typeof(ICollection<object>))))
                {
                    var enumerationMethod = EnumMethodEnumExtensions.FromName(chainElement.Name);
                    var eval = TypeHelper.Instantiate<ExprDotEvalEnumMethod>(enumerationMethod.GetImplementation());
                    eval.Init(streamOfProviderIfApplicable, enumerationMethod, chainElement.Name, currentInputType, chainElement.Parameters, validationContext);
                    currentInputType = eval.TypeInfo;
                    if (currentInputType == null)
                    {
                        throw new IllegalStateException("Enumeration method '" + chainElement.Name + "' has not returned type information");
                    }
                    methodEvals.Add(eval);
                    lastLambdaFunc = enumerationMethod;
                    continue;
                }

                // resolve datetime
                if (chainElement.Name.IsDateTimeMethod() && (!matchingMethod || methodTarget == typeof(DateTimeOffset?)))
                {
                    var datetimeMethod = DatetimeMethodEnumExtensions.FromName(chainElement.Name);
                    var datetimeImpl = ExprDotEvalDTFactory.ValidateMake(
                        validationContext.StreamTypeService, chainSpecStack, datetimeMethod, chainElement.Name,
                        currentInputType, chainElement.Parameters, inputDesc,
                        validationContext.EngineImportService.TimeZone,
                        validationContext.EngineImportService.TimeAbacus);
                    currentInputType = datetimeImpl.ReturnType;
                    if (currentInputType == null)
                    {
                        throw new IllegalStateException("Date-time method '" + chainElement.Name + "' has not returned type information");
                    }
                    methodEvals.Add(datetimeImpl.Eval);
                    filterAnalyzerDesc = datetimeImpl.IntervalFilterDesc;
                    continue;
                }

                // try to resolve as property if the last method returned a type
                if (currentInputType is EventEPType)
                {
                    var inputEventType = ((EventEPType)currentInputType).EventType;
                    var type = inputEventType.GetPropertyType(chainElement.Name);
                    var getter = inputEventType.GetGetter(chainElement.Name);
                    if (type != null && getter != null)
                    {
                        var noduck = new ExprDotEvalProperty(getter, EPTypeHelper.SingleValue(type.GetBoxedType()));
                        methodEvals.Add(noduck);
                        currentInputType = EPTypeHelper.SingleValue(EPTypeHelper.GetClassSingleValued(noduck.TypeInfo));
                        continue;
                    }
                }

                // Finally try to resolve the method
                if (methodTarget != null)
                {
                    try
                    {
                        // find descriptor again, allow for duck typing
                        var desc = GetValidateMethodDescriptor(methodTarget, chainElement.Name, chainElement.Parameters, validationContext);
                        var fastMethod = desc.FastMethod;
                        paramEvals = desc.ChildEvals;

                        ExprDotEval eval;
                        if (currentInputType is ClassEPType)
                        {
                            // if followed by an enumeration method, convert array to collection
                            if (fastMethod.ReturnType.IsArray && !chainSpecStack.IsEmpty() && chainSpecStack.First.Name.IsEnumerationMethod())
                            {
                                eval = new ExprDotMethodEvalNoDuckWrapArray(validationContext.StatementName, fastMethod, paramEvals);
                            }
                            else
                            {
                                eval = new ExprDotMethodEvalNoDuck(validationContext.StatementName, fastMethod, paramEvals);
                            }
                        }
                        else
                        {
                            eval = new ExprDotMethodEvalNoDuckUnderlying(validationContext.StatementName, fastMethod, paramEvals);
                        }
                        methodEvals.Add(eval);
                        currentInputType = eval.TypeInfo;
                    }
                    catch (Exception e)
                    {
                        if (!isDuckTyping)
                        {
                            throw new ExprValidationException(e.Message, e);
                        }
                        else
                        {
                            var duck = new ExprDotMethodEvalDuck(validationContext.StatementName, validationContext.EngineImportService, chainElement.Name, paramTypes, paramEvals);
                            methodEvals.Add(duck);
                            currentInputType = duck.TypeInfo;
                        }
                    }
                    continue;
                }

                var message = "Could not find event property, enumeration method or instance method named '" +
                        chainElement.Name + "' in " + currentInputType.ToTypeDescriptive();
                throw new ExprValidationException(message);
            }

            var intermediateEvals = methodEvals.ToArray();

            if (lastLambdaFunc != null)
            {
                ExprDotEval finalEval = null;
                if (currentInputType is EventMultiValuedEPType)
                {
                    var mvType = (EventMultiValuedEPType)currentInputType;
                    var tableMetadata = validationContext.TableService.GetTableMetadataFromEventType(mvType.Component);
                    if (tableMetadata != null)
                    {
                        finalEval = new ExprDotEvalUnpackCollEventBeanTable(mvType.Component, tableMetadata);
                    }
                    else
                    {
                        finalEval = new ExprDotEvalUnpackCollEventBean(mvType.Component);
                    }
                }
                else if (currentInputType is EventEPType)
                {
                    var epType = (EventEPType)currentInputType;
                    var tableMetadata = validationContext.TableService.GetTableMetadataFromEventType(epType.EventType);
                    if (tableMetadata != null)
                    {
                        finalEval = new ExprDotEvalUnpackBeanTable(epType.EventType, tableMetadata);
                    }
                    else
                    {
                        finalEval = new ExprDotEvalUnpackBean(epType.EventType);
                    }
                }
                if (finalEval != null)
                {
                    methodEvals.Add(finalEval);
                }
            }

            var unpackingEvals = methodEvals.ToArray();
            return new ExprDotNodeRealizedChain(intermediateEvals, unpackingEvals, filterAnalyzerDesc);
        }

        private static Type GetMethodTarget(EPType currentInputType)
        {
            if (currentInputType is ClassEPType)
            {
                return ((ClassEPType)currentInputType).Clazz;
            }
            else if (currentInputType is EventEPType)
            {
                return ((EventEPType)currentInputType).EventType.UnderlyingType;
            }
            return null;
        }

        public static ObjectArrayEventType MakeTransientOAType(string enumMethod, string propertyName, Type type, EventAdapterService eventAdapterService)
        {
            var propsResult = new Dictionary<string, object>();
            propsResult.Put(propertyName, type);
            var typeName = enumMethod + "__" + propertyName;
            return new ObjectArrayEventType(
                EventTypeMetadata.CreateAnonymous(typeName, ApplicationType.OBJECTARR), typeName, 0, eventAdapterService,
                propsResult, 
                null, 
                null, 
                null);
        }

        public static EventType[] GetSingleLambdaParamEventType(string enumMethodUsedName, IList<string> goesToNames, EventType inputEventType, Type collectionComponentType, EventAdapterService eventAdapterService)
        {
            if (inputEventType != null)
            {
                return new EventType[] { inputEventType };
            }
            else
            {
                return new EventType[] { ExprDotNodeUtility.MakeTransientOAType(enumMethodUsedName, goesToNames[0], collectionComponentType, eventAdapterService) };
            }
        }

        public static object EvaluateChain(ExprDotEval[] evaluators, object inner, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED)
            {
                var i = -1;
                foreach (var methodEval in evaluators)
                {
                    i++;
                    InstrumentationHelper.Get().QExprDotChainElement(i, methodEval);
                    inner = methodEval.Evaluate(inner, new EvaluateParams(eventsPerStream, isNewData, context));
                    InstrumentationHelper.Get().AExprDotChainElement(methodEval.TypeInfo, inner);
                    if (inner == null)
                    {
                        break;
                    }
                }
                return inner;
            }
            else
            {
                foreach (var methodEval in evaluators)
                {
                    inner = methodEval.Evaluate(inner, new EvaluateParams(eventsPerStream, isNewData, context));
                    if (inner == null)
                    {
                        break;
                    }
                }
                return inner;
            }
        }

        public static object EvaluateChainWithWrap(ExprDotStaticMethodWrap resultWrapLambda,
                                                   object result,
                                                   EventType optionalResultSingleEventType,
                                                   Type resultType,
                                                   ExprDotEval[] chainEval,
                                                   EventBean[] eventsPerStream,
                                                   bool newData,
                                                   ExprEvaluatorContext exprEvaluatorContext)
        {
            if (result == null)
            {
                return null;
            }

            if (resultWrapLambda != null)
            {
                result = resultWrapLambda.Convert(result);
            }

            var evaluateParams = new EvaluateParams(eventsPerStream, newData, exprEvaluatorContext);

            if (InstrumentationHelper.ENABLED)
            {
                EPType typeInfo;
                if (resultWrapLambda != null)
                {
                    typeInfo = resultWrapLambda.TypeInfo;
                }
                else
                {
                    if (optionalResultSingleEventType != null)
                    {
                        typeInfo = EPTypeHelper.SingleEvent(optionalResultSingleEventType);
                    }
                    else
                    {
                        typeInfo = EPTypeHelper.SingleValue(resultType);
                    }
                }
                InstrumentationHelper.Get().QExprDotChain(typeInfo, result, chainEval);

                var i = -1;
                foreach (var aChainEval in chainEval)
                {
                    i++;
                    InstrumentationHelper.Get().QExprDotChainElement(i, aChainEval);
                    result = aChainEval.Evaluate(result, evaluateParams);
                    InstrumentationHelper.Get().AExprDotChainElement(aChainEval.TypeInfo, result);
                    if (result == null)
                    {
                        break;
                    }
                }

                InstrumentationHelper.Get().AExprDotChain();
                return result;
            }

            foreach (var aChainEval in chainEval)
            {
                result = aChainEval.Evaluate(result, evaluateParams);
                if (result == null)
                {
                    return result;
                }
            }
            return result;
        }

        public static ExprDotEnumerationSource GetEnumerationSource(ExprNode inputExpression, StreamTypeService streamTypeService, EventAdapterService eventAdapterService, int statementId, bool hasEnumerationMethod, bool disablePropertyExpressionEventCollCache)
        {
            var rootNodeEvaluator = inputExpression.ExprEvaluator;
            ExprEvaluatorEnumeration rootLambdaEvaluator = null;
            EPType info = null;

            if (rootNodeEvaluator is ExprEvaluatorEnumeration)
            {
                rootLambdaEvaluator = (ExprEvaluatorEnumeration)rootNodeEvaluator;

                if (rootLambdaEvaluator.GetEventTypeCollection(eventAdapterService, statementId) != null)
                {
                    info = EPTypeHelper.CollectionOfEvents(rootLambdaEvaluator.GetEventTypeCollection(eventAdapterService, statementId));
                }
                else if (rootLambdaEvaluator.GetEventTypeSingle(eventAdapterService, statementId) != null)
                {
                    info = EPTypeHelper.SingleEvent(rootLambdaEvaluator.GetEventTypeSingle(eventAdapterService, statementId));
                }
                else if (rootLambdaEvaluator.ComponentTypeCollection != null)
                {
                    info = EPTypeHelper.CollectionOfSingleValue(rootLambdaEvaluator.ComponentTypeCollection);
                }
                else
                {
                    rootLambdaEvaluator = null; // not a lambda evaluator
                }
            }
            else if (inputExpression is ExprIdentNode)
            {
                var identNode = (ExprIdentNode)inputExpression;
                var streamId = identNode.StreamId;
                var streamType = streamTypeService.EventTypes[streamId];
                return GetPropertyEnumerationSource(identNode.ResolvedPropertyName, streamId, streamType, hasEnumerationMethod, disablePropertyExpressionEventCollCache);
            }
            return new ExprDotEnumerationSource(info, null, rootLambdaEvaluator);
        }

        public static ExprDotEnumerationSourceForProps GetPropertyEnumerationSource(string propertyName, int streamId, EventType streamType, bool allowEnumType, bool disablePropertyExpressionEventCollCache)
        {
            var propertyType = streamType.GetPropertyType(propertyName);
            var typeInfo = EPTypeHelper.SingleValue(propertyType);  // assume scalar for now

            // no enumeration methods, no need to expose as an enumeration
            if (!allowEnumType)
            {
                return new ExprDotEnumerationSourceForProps(null, typeInfo, streamId, null);
            }

            var fragmentEventType = streamType.GetFragmentType(propertyName);
            var getter = streamType.GetGetter(propertyName);

            ExprEvaluatorEnumeration enumEvaluator = null;
            if (getter != null && fragmentEventType != null)
            {
                if (fragmentEventType.IsIndexed)
                {
                    enumEvaluator = new PropertyExprEvaluatorEventCollection(propertyName, streamId, fragmentEventType.FragmentType, getter, disablePropertyExpressionEventCollCache);
                    typeInfo = EPTypeHelper.CollectionOfEvents(fragmentEventType.FragmentType);
                }
                else
                {   // we don't want native to require an eventbean instance
                    enumEvaluator = new PropertyExprEvaluatorEventSingle(streamId, fragmentEventType.FragmentType, getter);
                    typeInfo = EPTypeHelper.SingleEvent(fragmentEventType.FragmentType);
                }
            }
            else
            {
                var desc = EventTypeUtility.GetNestablePropertyDescriptor(streamType, propertyName);
                if (desc != null && desc.IsIndexed && !desc.RequiresIndex && desc.PropertyComponentType != null)
                {
                    if (propertyType.IsGenericCollection())
                    {
                        enumEvaluator = new PropertyExprEvaluatorScalarCollection(propertyName, streamId, getter, desc.PropertyComponentType);
                    }
                    else if (propertyType.IsImplementsInterface(typeof(System.Collections.IEnumerable)))
                    {
                        enumEvaluator = new PropertyExprEvaluatorScalarIterable(propertyName, streamId, getter, desc.PropertyComponentType);
                    }
                    else if (propertyType.IsArray)
                    {
                        enumEvaluator = new PropertyExprEvaluatorScalarArray(propertyName, streamId, getter, desc.PropertyComponentType);
                    }
                    else
                    {
                        throw new IllegalStateException("Property indicated indexed-type but failed to find proper collection adapter for use with enumeration methods");
                    }
                    typeInfo = EPTypeHelper.CollectionOfSingleValue(desc.PropertyComponentType);
                }
            }
            var enumEvaluatorGivenEvent = (ExprEvaluatorEnumerationGivenEvent)enumEvaluator;
            return new ExprDotEnumerationSourceForProps(enumEvaluator, typeInfo, streamId, enumEvaluatorGivenEvent);
        }

        private static ExprNodeUtilMethodDesc GetValidateMethodDescriptor(Type methodTarget, string methodName, IList<ExprNode> parameters, ExprValidationContext validationContext)
        {
            ExprNodeUtilResolveExceptionHandler exceptionHandler = new ProxyExprNodeUtilResolveExceptionHandler
            {
                ProcHandle = e => new ExprValidationException("Failed to resolve method '" + methodName + "': " + e.Message, e),
            };
            var wildcardType = validationContext.StreamTypeService.EventTypes.Length != 1 ? null : validationContext.StreamTypeService.EventTypes[0];
            return ExprNodeUtility.ResolveMethodAllowWildcardAndStream(
                methodTarget.Name, methodTarget, methodName, parameters, validationContext.EngineImportService,
                validationContext.EventAdapterService, validationContext.StatementId, wildcardType != null, wildcardType,
                exceptionHandler, methodName, validationContext.TableService,
                validationContext.StreamTypeService.EngineURIQualifier);
        }
    }
} // end of namespace
