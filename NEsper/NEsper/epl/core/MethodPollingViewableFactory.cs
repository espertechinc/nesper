///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.db;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.schedule;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
    using Map = IDictionary<string, object>;

    /// <summary>Factory for method-invocation data provider streams.</summary>
    public class MethodPollingViewableFactory
    {
        /// <summary>
        /// Creates a method-invocation polling view for use as a stream that calls a method, or pulls results from cache.
        /// </summary>
        /// <param name="streamNumber">the stream number</param>
        /// <param name="methodStreamSpec">defines the class and method to call</param>
        /// <param name="eventAdapterService">for creating event types and events</param>
        /// <param name="epStatementAgentInstanceHandle">for time-based callbacks</param>
        /// <param name="engineImportService">for resolving configurations</param>
        /// <param name="schedulingService">for scheduling callbacks in expiry-time based caches</param>
        /// <param name="scheduleBucket">for schedules within the statement</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <param name="variableService">variable service</param>
        /// <param name="statementContext">statement context</param>
        /// <param name="contextName">context name</param>
        /// <param name="dataCacheFactory">factory for cache</param>
        /// <exception cref="ExprValidationException">
        /// if the expressions cannot be validated or the method descriptor
        /// has incorrect class and method names, or parameter number and types don't match
        /// </exception>
        /// <returns>pollable view</returns>
        public static HistoricalEventViewable CreatePollMethodView(
            int streamNumber,
            MethodStreamSpec methodStreamSpec,
            EventAdapterService eventAdapterService,
            EPStatementAgentInstanceHandle epStatementAgentInstanceHandle,
            EngineImportService engineImportService,
            SchedulingService schedulingService,
            ScheduleBucket scheduleBucket,
            ExprEvaluatorContext exprEvaluatorContext,
            VariableService variableService,
            string contextName,
            DataCacheFactory dataCacheFactory,
            StatementContext statementContext)
        {
            VariableMetaData variableMetaData = variableService.GetVariableMetaData(methodStreamSpec.ClassName);
            MethodPollingExecStrategyEnum strategy;
            VariableReader variableReader = null;
            string variableName = null;
            MethodInfo methodReflection = null;
            object invocationTarget = null;
            string eventTypeNameProvidedUDFOrScript = null;

            // see if this is a script in the from-clause
            ExprNodeScript scriptExpression = null;
            if (methodStreamSpec.ClassName == null && methodStreamSpec.MethodName != null)
            {
                var scriptsByName = statementContext.ExprDeclaredService.GetScriptsByName(methodStreamSpec.MethodName);
                if (scriptsByName != null)
                {
                    scriptExpression =
                        ExprDeclaredHelper.GetExistsScript(
                            statementContext.ConfigSnapshot.EngineDefaults.Scripts.DefaultDialect,
                            methodStreamSpec.MethodName, methodStreamSpec.Expressions, scriptsByName,
                            statementContext.ExprDeclaredService);
                }
            }

            try
            {
                if (scriptExpression != null)
                {
                    eventTypeNameProvidedUDFOrScript = scriptExpression.EventTypeNameAnnotation;
                    strategy = MethodPollingExecStrategyEnum.TARGET_SCRIPT;
                    ExprNodeUtility.ValidateSimpleGetSubtree(
                        ExprNodeOrigin.METHODINVJOIN, scriptExpression, statementContext, null, false);
                }
                else if (variableMetaData != null)
                {
                    variableName = variableMetaData.VariableName;
                    if (variableMetaData.ContextPartitionName != null)
                    {
                        if (contextName == null || !contextName.Equals(variableMetaData.ContextPartitionName))
                        {
                            throw new ExprValidationException(
                                "Variable by name '" + variableMetaData.VariableName +
                                "' has been declared for context '" + variableMetaData.ContextPartitionName +
                                "' and can only be used within the same context");
                        }
                        strategy = MethodPollingExecStrategyEnum.TARGET_VAR_CONTEXT;
                        variableReader = null;
                        invocationTarget = null;
                    }
                    else
                    {
                        variableReader = variableService.GetReader(
                            methodStreamSpec.ClassName, EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID);
                        if (variableMetaData.IsConstant)
                        {
                            invocationTarget = variableReader.Value;
                            if (invocationTarget is EventBean)
                            {
                                invocationTarget = ((EventBean) invocationTarget).Underlying;
                            }
                            strategy = MethodPollingExecStrategyEnum.TARGET_CONST;
                        }
                        else
                        {
                            invocationTarget = null;
                            strategy = MethodPollingExecStrategyEnum.TARGET_VAR;
                        }
                    }
                    methodReflection = engineImportService.ResolveNonStaticMethodOverloadChecked(
                            variableMetaData.VariableType, methodStreamSpec.MethodName);
                }
                else if (methodStreamSpec.ClassName == null)
                {
                    // must be either UDF or script
                    Pair<Type, EngineImportSingleRowDesc> udf = null;
                    try
                    {
                        udf = engineImportService.ResolveSingleRow(methodStreamSpec.MethodName);
                    }
                    catch (EngineImportException ex)
                    {
                        throw new ExprValidationException(
                            "Failed to find user-defined function '" + methodStreamSpec.MethodName + "': " + ex.Message,
                            ex);
                    }
                    methodReflection = engineImportService.ResolveMethodOverloadChecked(udf.First, methodStreamSpec.MethodName);
                    invocationTarget = null;
                    variableReader = null;
                    variableName = null;
                    strategy = MethodPollingExecStrategyEnum.TARGET_CONST;
                    eventTypeNameProvidedUDFOrScript = udf.Second.OptionalEventTypeName;
                }
                else
                {
                    methodReflection = engineImportService.ResolveMethodOverloadChecked(methodStreamSpec.ClassName, methodStreamSpec.MethodName);
                    invocationTarget = null;
                    variableReader = null;
                    variableName = null;
                    strategy = MethodPollingExecStrategyEnum.TARGET_CONST;
                }
            }
            catch (ExprValidationException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ExprValidationException(e.Message, e);
            }

            Type methodProviderClass = null;
            Type beanClass;
            IDictionary<string, object> oaType = null;
            IDictionary<string, object> mapType = null;
            bool isCollection = false;
            bool isIterator = false;
            EventType eventType;
            EventType eventTypeWhenMethodReturnsEventBeans = null;
            bool isStaticMethod = false;

            if (methodReflection != null)
            {
                methodProviderClass = methodReflection.DeclaringType;
                isStaticMethod = variableMetaData == null;

                // Determine object type returned by method
                beanClass = methodReflection.ReturnType;
                if ((beanClass == typeof (void)) || (beanClass.IsBuiltinDataType()))
                {
                    throw new ExprValidationException(
                        "Invalid return type for static method '" + methodReflection.Name + "' of class '" +
                        methodStreamSpec.ClassName + "', expecting a class");
                }

                if (methodReflection.ReturnType.IsArray &&
                    methodReflection.ReturnType.GetElementType() != typeof (EventBean))
                {
                    beanClass = methodReflection.ReturnType.GetElementType();
                }

                Type collectionClass = null;
                Type iteratorClass = null;

                if (!beanClass.IsGenericDictionary())
                {
                    isCollection = beanClass.IsGenericCollection();
                    if (isCollection)
                    {
                        collectionClass = TypeHelper.GetGenericReturnType(methodReflection, true);
                        beanClass = collectionClass;
                    }

                    isIterator = beanClass.IsGenericEnumerator() && !beanClass.IsGenericDictionary();
                    if (isIterator)
                    {
                        iteratorClass = TypeHelper.GetGenericReturnType(methodReflection, true);
                        beanClass = iteratorClass;
                    }
                }

                // If the method returns a Map, look up the map type
                string mapTypeName = null;

                if ((methodReflection.ReturnType.IsGenericStringDictionary()) ||
                    (methodReflection.ReturnType.IsArray && methodReflection.ReturnType.GetElementType().IsGenericStringDictionary()) ||
                    (isCollection && collectionClass.IsImplementsInterface(typeof (Map))) ||
                    (isIterator && iteratorClass.IsImplementsInterface(typeof (Map))))
                {
                    MethodMetadataDesc metadata;
                    if (variableMetaData != null)
                    {
                        metadata = GetCheckMetadataVariable(
                            methodStreamSpec.MethodName, variableMetaData, variableReader, engineImportService,
                            typeof (Map));
                    }
                    else
                    {
                        metadata = GetCheckMetadataNonVariable(
                            methodStreamSpec.MethodName, methodStreamSpec.ClassName, engineImportService, typeof (Map));
                    }
                    mapTypeName = metadata.TypeName;
                    mapType = (IDictionary<string, object>) metadata.TypeMetadata;
                }

                // If the method returns an object[] or object[][], look up the type information
                string oaTypeName = null;
                if (methodReflection.ReturnType == typeof (object[]) ||
                    methodReflection.ReturnType == typeof (object[][]) ||
                    (isCollection && collectionClass == typeof (object[])) ||
                    (isIterator && iteratorClass == typeof (object[])))
                {
                    MethodMetadataDesc metadata;
                    if (variableMetaData != null)
                    {
                        metadata = GetCheckMetadataVariable(
                            methodStreamSpec.MethodName, variableMetaData, variableReader, engineImportService,
                            typeof (IDictionary<string, object>));
                    }
                    else
                    {
                        metadata = GetCheckMetadataNonVariable(
                            methodStreamSpec.MethodName, methodStreamSpec.ClassName, engineImportService,
                            typeof (IDictionary<string, object>));
                    }
                    oaTypeName = metadata.TypeName;
                    oaType = (IDictionary<string, object>) metadata.TypeMetadata;
                }

                // Determine event type from class and method name
                // If the method returns EventBean[], require the event type
                if ((methodReflection.ReturnType.IsArray &&
                     methodReflection.ReturnType.GetElementType() == typeof (EventBean)) ||
                    (isCollection && collectionClass == typeof (EventBean)) ||
                    (isIterator && iteratorClass == typeof (EventBean)))
                {
                    string typeName = methodStreamSpec.EventTypeName == null
                        ? eventTypeNameProvidedUDFOrScript
                        : methodStreamSpec.EventTypeName;
                    eventType = EventTypeUtility.RequireEventType(
                        "Method", methodReflection.Name, eventAdapterService, typeName);
                    eventTypeWhenMethodReturnsEventBeans = eventType;
                }
                else if (mapType != null)
                {
                    eventType = eventAdapterService.AddNestableMapType(
                        mapTypeName, mapType, null, false, true, true, false, false);
                }
                else if (oaType != null)
                {
                    eventType = eventAdapterService.AddNestableObjectArrayType(
                        oaTypeName, oaType, null, false, true, true, false, false, false, null);
                }
                else
                {
                    eventType = eventAdapterService.AddBeanType(beanClass.GetDefaultTypeName(), beanClass, false, true, true);
                }

                // the @type is only allowed in conjunction with EventBean return types
                if (methodStreamSpec.EventTypeName != null && eventTypeWhenMethodReturnsEventBeans == null)
                {
                    throw new ExprValidationException(EventTypeUtility.DisallowedAtTypeMessage());
                }
            }
            else
            {
                string eventTypeName = methodStreamSpec.EventTypeName == null
                    ? scriptExpression.EventTypeNameAnnotation
                    : methodStreamSpec.EventTypeName;
                eventType = EventTypeUtility.RequireEventType(
                    "Script", scriptExpression.Script.Name, eventAdapterService, eventTypeName);
            }

            // get configuration for cache
            string configName = methodProviderClass != null ? methodProviderClass.FullName : methodStreamSpec.MethodName;
            ConfigurationMethodRef configCache = engineImportService.GetConfigurationMethodRef(configName);
            if (configCache == null)
            {
                configCache = engineImportService.GetConfigurationMethodRef(configName);
            }
            ConfigurationDataCache dataCacheDesc = (configCache != null) ? configCache.DataCacheDesc : null;
            DataCache dataCache = dataCacheFactory.GetDataCache(
                dataCacheDesc, statementContext, epStatementAgentInstanceHandle, schedulingService, scheduleBucket,
                streamNumber);

            // metadata
            var meta = new MethodPollingViewableMeta(
                methodProviderClass, isStaticMethod, mapType, oaType, invocationTarget, strategy, isCollection,
                isIterator, variableReader, variableName, eventTypeWhenMethodReturnsEventBeans, scriptExpression);
            return new MethodPollingViewable(methodStreamSpec, dataCache, eventType, exprEvaluatorContext, meta, statementContext.ThreadLocalManager);
        }

        private static MethodMetadataDesc GetCheckMetadataVariable(
            string methodName,
            VariableMetaData variableMetaData,
            VariableReader variableReader,
            EngineImportService engineImportService,
            Type metadataClass)
        {
            MethodInfo typeGetterMethod = GetRequiredTypeGetterMethodCanNonStatic(
                methodName, null, variableMetaData.VariableType, engineImportService, metadataClass);

            if (typeGetterMethod.IsStatic)
            {
                return InvokeMetadataMethod(null, variableMetaData.GetType().Name, typeGetterMethod);
            }

            // if the metadata is not a static method and we don't have an instance this is a problem
            const string messagePrefix = "Failed to access variable method invocation metadata: ";
            if (variableReader == null)
            {
                throw new ExprValidationException(
                    messagePrefix +
                    "The metadata method is an instance method however the variable is contextual, please declare the metadata method as static or remove the context declaration for the variable");
            }

            object value = variableReader.Value;
            if (value == null)
            {
                throw new ExprValidationException(
                    messagePrefix + "The variable value is null and the metadata method is an instance method");
            }

            if (value is EventBean)
            {
                value = ((EventBean) value).Underlying;
            }
            return InvokeMetadataMethod(value, variableMetaData.GetType().Name, typeGetterMethod);
        }


        private static MethodMetadataDesc GetCheckMetadataNonVariable(
            string methodName,
            string className,
            EngineImportService engineImportService,
            Type metadataClass)
        {
            MethodInfo typeGetterMethod = GetRequiredTypeGetterMethodCanNonStatic(
                methodName, className, null, engineImportService, metadataClass);
            return InvokeMetadataMethod(null, className, typeGetterMethod);
        }

        private static MethodInfo GetRequiredTypeGetterMethodCanNonStatic(
            string methodName,
            string classNameWhenNoClass,
            Type clazzWhenAvailable,
            EngineImportService engineImportService,
            Type metadataClass)
        {
            MethodInfo typeGetterMethod;
            string getterMethodName = methodName + "Metadata";
            try
            {
                if (clazzWhenAvailable != null)
                {
                    typeGetterMethod = engineImportService.ResolveMethod(
                        clazzWhenAvailable, getterMethodName, new Type[0], new bool[0], new bool[0]);
                }
                else
                {
                    typeGetterMethod = engineImportService.ResolveMethodOverloadChecked(
                        classNameWhenNoClass, getterMethodName, new Type[0], new bool[0], new bool[0]);
                }
            }
            catch (Exception)
            {
                throw new ExprValidationException(
                    "Could not find getter method for method invocation, expected a method by name '" + getterMethodName +
                    "' accepting no parameters");
            }

            bool fail;
            if (metadataClass.IsInterface)
            {
                fail = !typeGetterMethod.ReturnType.IsImplementsInterface(metadataClass);
            }
            else
            {
                fail = typeGetterMethod.ReturnType != metadataClass;
            }
            if (fail)
            {
                throw new ExprValidationException(
                    "Getter method '" + typeGetterMethod.Name + "' does not return " +
                    metadataClass.GetCleanName());
            }

            return typeGetterMethod;
        }

        private static MethodMetadataDesc InvokeMetadataMethod(
            object target,
            string className,
            MethodInfo typeGetterMethod)
        {
            object resultType;
            try
            {
                resultType = typeGetterMethod.Invoke(target, new object[0]);
            }
            catch (Exception e)
            {
                throw new ExprValidationException(
                    "Error invoking metadata getter method for method invocation, for method by name '" +
                    typeGetterMethod.Name + "' accepting no parameters: " + e.Message, e);
            }
            if (resultType == null)
            {
                throw new ExprValidationException(
                    "Error invoking metadata getter method for method invocation, method returned a null value");
            }

            return new MethodMetadataDesc(className + "." + typeGetterMethod.Name, resultType);
        }

        public class MethodMetadataDesc
        {
            public MethodMetadataDesc(string typeName, object typeMetadata)
            {
                TypeName = typeName;
                TypeMetadata = typeMetadata;
            }

            public string TypeName { get; private set; }

            public object TypeMetadata { get; private set; }
        }
    }
} // end of namespace
