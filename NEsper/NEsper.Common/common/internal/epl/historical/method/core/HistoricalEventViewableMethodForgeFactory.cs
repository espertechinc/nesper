///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.method.poll;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.method.core
{
    public class HistoricalEventViewableMethodForgeFactory
    {
        public static HistoricalEventViewableMethodForge CreateMethodStatementView(
            int stream,
            MethodStreamSpec methodStreamSpec,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            var variableMetaData = services.VariableCompileTimeResolver.Resolve(methodStreamSpec.ClassName);
            MethodPollingExecStrategyEnum strategy;
            MethodInfo methodReflection = null;
            string eventTypeNameProvidedUDFOrScript = null;
            var contextName = @base.StatementSpec.Raw.OptionalContextName;
            var classpathImportService = services.ImportServiceCompileTime;

            // see if this is a script in the from-clause
            ExprNodeScript scriptExpression = null;
            if (methodStreamSpec.ClassName == null && methodStreamSpec.MethodName != null) {
                var script = services.ScriptCompileTimeResolver.Resolve(
                    methodStreamSpec.MethodName, methodStreamSpec.Expressions.Count);
                if (script != null) {
                    scriptExpression = new ExprNodeScript(
                        services.Configuration.Compiler.Scripts.DefaultDialect, script, methodStreamSpec.Expressions);
                }
            }

            try {
                if (scriptExpression != null) {
                    eventTypeNameProvidedUDFOrScript = scriptExpression.EventTypeNameAnnotation;
                    strategy = MethodPollingExecStrategyEnum.TARGET_SCRIPT;
                    EPLValidationUtil.ValidateSimpleGetSubtree(
                        ExprNodeOrigin.METHODINVJOIN, scriptExpression, null, false, @base.StatementRawInfo, services);
                }
                else if (variableMetaData != null) {
                    var variableName = variableMetaData.VariableName;
                    if (variableMetaData.OptionalContextName != null) {
                        if (contextName == null || !contextName.Equals(variableMetaData.OptionalContextName)) {
                            throw new ExprValidationException(
                                "Variable by name '" + variableMetaData.VariableName +
                                "' has been declared for context '" + variableMetaData.OptionalContextName +
                                "' and can only be used within the same context");
                        }

                        strategy = MethodPollingExecStrategyEnum.TARGET_VAR_CONTEXT;
                    }
                    else {
                        if (variableMetaData.IsConstant) {
                            strategy = MethodPollingExecStrategyEnum.TARGET_CONST;
                        }
                        else {
                            strategy = MethodPollingExecStrategyEnum.TARGET_VAR;
                        }
                    }

                    methodReflection = classpathImportService.ResolveNonStaticMethodOverloadChecked(
                        variableMetaData.Type, methodStreamSpec.MethodName);
                }
                else if (methodStreamSpec.ClassName == null) { // must be either UDF or script
                    Pair<Type, ImportSingleRowDesc> udf;
                    try {
                        udf = classpathImportService.ResolveSingleRow(methodStreamSpec.MethodName);
                    }
                    catch (ImportException ex) {
                        throw new ExprValidationException(
                            "Failed to find user-defined function '" + methodStreamSpec.MethodName + "': " + ex.Message,
                            ex);
                    }

                    methodReflection = classpathImportService.ResolveMethodOverloadChecked(
                        udf.First, methodStreamSpec.MethodName);
                    eventTypeNameProvidedUDFOrScript = udf.Second.OptionalEventTypeName;
                    strategy = MethodPollingExecStrategyEnum.TARGET_CONST;
                }
                else {
                    methodReflection = classpathImportService.ResolveMethodOverloadChecked(
                        methodStreamSpec.ClassName, methodStreamSpec.MethodName);
                    strategy = MethodPollingExecStrategyEnum.TARGET_CONST;
                }
            }
            catch (ExprValidationException) {
                throw;
            }
            catch (Exception e) {
                throw new ExprValidationException(e.Message, e);
            }

            Type methodProviderClass = null;
            Type beanClass;
            LinkedHashMap<string, object> oaType = null;
            IDictionary<string, object> mapType = null;
            var isCollection = false;
            var isIterator = false;
            EventType eventType;
            EventType eventTypeWhenMethodReturnsEventBeans = null;
            var isStaticMethod = false;

            if (methodReflection != null) {
                methodProviderClass = methodReflection.DeclaringType;
                isStaticMethod = variableMetaData == null;

                // Determine object type returned by method
                beanClass = methodReflection.ReturnType;
                if (beanClass == typeof(void) || beanClass == typeof(void) || beanClass.IsBuiltinDataType()) {
                    throw new ExprValidationException(
                        "Invalid return type for static method '" + methodReflection.Name + "' of class '" +
                        methodStreamSpec.ClassName + "', expecting a Java class");
                }

                if (methodReflection.ReturnType.IsArray &&
                    methodReflection.ReturnType.GetElementType() != typeof(EventBean)) {
                    beanClass = methodReflection.ReturnType.GetElementType();
                }

                isCollection = TypeHelper.IsImplementsInterface(beanClass, typeof(ICollection<object>));
                Type collectionClass = null;
                if (isCollection) {
                    collectionClass = TypeHelper.GetGenericReturnType(methodReflection, true);
                    beanClass = collectionClass;
                }

                isIterator = TypeHelper.IsImplementsInterface(beanClass, typeof(IEnumerator));
                Type iteratorClass = null;
                if (isIterator) {
                    iteratorClass = TypeHelper.GetGenericReturnType(methodReflection, true);
                    beanClass = iteratorClass;
                }

                // If the method returns a Map, look up the map type
                string mapTypeName = null;
                if (TypeHelper.IsImplementsInterface(
                        methodReflection.ReturnType, typeof(IDictionary<object, object>)) ||
                    methodReflection.ReturnType.IsArray && TypeHelper.IsImplementsInterface(
                        methodReflection.ReturnType.GetElementType(), typeof(IDictionary<object, object>)) ||
                    isCollection && TypeHelper.IsImplementsInterface(
                        collectionClass, typeof(IDictionary<object, object>)) ||
                    isIterator && TypeHelper.IsImplementsInterface(
                        iteratorClass, typeof(IDictionary<object, object>))) {
                    MethodMetadataDesc metadata;
                    if (variableMetaData != null) {
                        metadata = GetCheckMetadataVariable(
                            methodStreamSpec.MethodName, variableMetaData, classpathImportService,
                            typeof(IDictionary<object, object>));
                    }
                    else {
                        metadata = GetCheckMetadataNonVariable(
                            methodStreamSpec.MethodName, methodStreamSpec.ClassName, classpathImportService,
                            typeof(IDictionary<object, object>));
                    }

                    mapTypeName = metadata.TypeName;
                    mapType = (IDictionary<string, object>) metadata.TypeMetadata;
                }

                // If the method returns an Object[] or Object[][], look up the type information
                string oaTypeName = null;
                if (methodReflection.ReturnType == typeof(object[]) ||
                    methodReflection.ReturnType == typeof(object[][]) ||
                    isCollection && collectionClass == typeof(object[]) ||
                    isIterator && iteratorClass == typeof(object[])) {
                    MethodMetadataDesc metadata;
                    if (variableMetaData != null) {
                        metadata = GetCheckMetadataVariable(
                            methodStreamSpec.MethodName, variableMetaData, classpathImportService,
                            typeof(LinkedHashMap<object, object>));
                    }
                    else {
                        metadata = GetCheckMetadataNonVariable(
                            methodStreamSpec.MethodName, methodStreamSpec.ClassName, classpathImportService,
                            typeof(LinkedHashMap<object, object>));
                    }

                    oaTypeName = metadata.TypeName;
                    oaType = (LinkedHashMap<string, object>) metadata.TypeMetadata;
                }

                // Determine event type from class and method name
                // If the method returns EventBean[], require the event type
                Func<EventTypeApplicationType, EventTypeMetadata> metadataFunction = apptype => {
                    var eventTypeName = services.EventTypeNameGeneratorStatement.GetAnonymousMethodHistorical(stream);
                    return new EventTypeMetadata(
                        eventTypeName, @base.ModuleName, EventTypeTypeClass.METHODPOLLDERIVED, apptype,
                        NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
                };
                if (methodReflection.ReturnType.IsArray &&
                    methodReflection.ReturnType.GetElementType() == typeof(EventBean) ||
                    isCollection && collectionClass == typeof(EventBean) ||
                    isIterator && iteratorClass == typeof(EventBean)) {
                    var typeName = methodStreamSpec.EventTypeName == null
                        ? eventTypeNameProvidedUDFOrScript
                        : methodStreamSpec.EventTypeName;
                    eventType = EventTypeUtility.RequireEventType(
                        "Method", methodReflection.Name, typeName, services.EventTypeCompileTimeResolver);
                    eventTypeWhenMethodReturnsEventBeans = eventType;
                }
                else if (mapType != null) {
                    eventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                        metadataFunction.Invoke(EventTypeApplicationType.MAP), mapType, null, null, null, null,
                        services.BeanEventTypeFactoryPrivate, services.EventTypeCompileTimeResolver);
                    services.EventTypeCompileTimeRegistry.NewType(eventType);
                }
                else if (oaType != null) {
                    eventType = BaseNestableEventUtil.MakeOATypeCompileTime(
                        metadataFunction.Invoke(EventTypeApplicationType.OBJECTARR), oaType, null, null, null, null,
                        services.BeanEventTypeFactoryPrivate, services.EventTypeCompileTimeResolver);
                    services.EventTypeCompileTimeRegistry.NewType(eventType);
                }
                else {
                    var stem = services.BeanEventTypeStemService.GetCreateStem(beanClass, null);
                    eventType = new BeanEventType(
                        stem, metadataFunction.Invoke(EventTypeApplicationType.CLASS), services.BeanEventTypeFactoryPrivate,
                        null, null, null, null);
                    services.EventTypeCompileTimeRegistry.NewType(eventType);
                }

                // the @type is only allowed in conjunction with EventBean return types
                if (methodStreamSpec.EventTypeName != null && eventTypeWhenMethodReturnsEventBeans == null) {
                    throw new ExprValidationException(EventTypeUtility.DisallowedAtTypeMessage());
                }
            }
            else {
                var eventTypeName = methodStreamSpec.EventTypeName == null
                    ? scriptExpression.EventTypeNameAnnotation
                    : methodStreamSpec.EventTypeName;
                eventType = EventTypeUtility.RequireEventType(
                    "Script", scriptExpression.Script.Name, eventTypeName, services.EventTypeCompileTimeResolver);
            }

            // metadata
            var meta = new MethodPollingViewableMeta(
                methodProviderClass, isStaticMethod, mapType, oaType, strategy, isCollection, isIterator,
                variableMetaData, eventTypeWhenMethodReturnsEventBeans, scriptExpression);
            return new HistoricalEventViewableMethodForge(stream, eventType, methodStreamSpec, meta);
        }

        private static MethodMetadataDesc GetCheckMetadataVariable(
            string methodName,
            VariableMetaData variableMetaData,
            ImportServiceCompileTime importService,
            Type metadataClass)
        {
            var typeGetterMethod = GetRequiredTypeGetterMethodCanNonStatic(
                methodName, null, variableMetaData.Type, importService, metadataClass);

            if (typeGetterMethod.IsStatic) {
                return InvokeMetadataMethod(null, variableMetaData.GetType().GetSimpleName(), typeGetterMethod);
            }

            // if the metadata is not a static method and we don't have an instance this is a problem
            var messagePrefix = "Failed to access variable method invocation metadata: ";
            var value = variableMetaData.ValueWhenAvailable;
            if (value == null) {
                throw new ExprValidationException(
                    messagePrefix + "The variable value is null and the metadata method is an instance method");
            }

            if (value is EventBean) {
                value = ((EventBean) value).Underlying;
            }

            return InvokeMetadataMethod(value, variableMetaData.GetType().GetSimpleName(), typeGetterMethod);
        }

        private static MethodMetadataDesc GetCheckMetadataNonVariable(
            string methodName,
            string className,
            ImportServiceCompileTime importService,
            Type metadataClass)
        {
            var typeGetterMethod = GetRequiredTypeGetterMethodCanNonStatic(
                methodName, className, null, importService, metadataClass);
            return InvokeMetadataMethod(null, className, typeGetterMethod);
        }

        private static MethodInfo GetRequiredTypeGetterMethodCanNonStatic(
            string methodName,
            string classNameWhenNoClass,
            Type clazzWhenAvailable,
            ImportServiceCompileTime importService,
            Type metadataClass)
        {
            MethodInfo typeGetterMethod;
            var getterMethodName = methodName + "Metadata";
            try {
                if (clazzWhenAvailable != null) {
                    typeGetterMethod = importService.ResolveMethod(
                        clazzWhenAvailable, getterMethodName, new Type[0], new bool[0]);
                }
                else {
                    typeGetterMethod = importService.ResolveMethodOverloadChecked(
                        classNameWhenNoClass, getterMethodName, new Type[0], new bool[0], new bool[0]);
                }
            }
            catch (Exception) {
                throw new ExprValidationException(
                    "Could not find getter method for method invocation, expected a method by name '" +
                    getterMethodName + "' accepting no parameters");
            }

            bool fail;
            if (metadataClass.IsInterface) {
                fail = !typeGetterMethod.ReturnType.IsImplementsInterface(metadataClass);
            }
            else {
                fail = typeGetterMethod.ReturnType != metadataClass;
            }

            if (fail) {
                throw new ExprValidationException(
                    "Getter method '" + typeGetterMethod.Name + "' does not return " + metadataClass.GetCleanName());
            }

            return typeGetterMethod;
        }

        private static MethodMetadataDesc InvokeMetadataMethod(
            object target,
            string className,
            MethodInfo typeGetterMethod)
        {
            object resultType;
            try {
                resultType = typeGetterMethod.Invoke(target, null);
            }
            catch (Exception e) {
                throw new ExprValidationException(
                    "Error invoking metadata getter method for method invocation, for method by name '" +
                    typeGetterMethod.Name + "' accepting no parameters: " + e.Message, e);
            }

            if (resultType == null) {
                throw new ExprValidationException(
                    "Error invoking metadata getter method for method invocation, method returned a null value");
            }

            return new MethodMetadataDesc(className + "." + typeGetterMethod.Name, resultType);
        }

        public class MethodMetadataDesc
        {
            public MethodMetadataDesc(
                string typeName,
                object typeMetadata)
            {
                TypeName = typeName;
                TypeMetadata = typeMetadata;
            }

            public string TypeName { get; }

            public object TypeMetadata { get; }
        }
    }
} // end of namespace