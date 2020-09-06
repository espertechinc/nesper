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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    public class VariableUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetAssigmentExMessage(
            string variableName,
            Type variableType,
            Type initValueClass)
        {
            return "Variable '" +
                   variableName +
                   "' of declared type " +
                   variableType.CleanName() +
                   " cannot be assigned a value of type " +
                   initValueClass.CleanName();
        }

        public static void ConfigureVariables(
            VariableRepositoryPreconfigured repo,
            IDictionary<string, ConfigurationCommonVariable> variables,
            ImportService importService,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeRepositoryImpl eventTypeRepositoryPreconfigured,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            foreach (var entry in variables) {
                string variableName = entry.Key.Trim();
                if (repo.GetMetadata(variableName) != null) {
                    throw new ConfigurationException(
                        "Variable by name '" + entry.Key + "' has already been configured");
                }

                VariableMetaData meta;
                try {
                    var variableType = ClassIdentifierWArray.ParseSODA(entry.Value.VariableType);
                    meta = GetTypeInfo(
                        variableName,
                        null,
                        NameAccessModifier.PRECONFIGURED,
                        null,
                        null,
                        null,
                        variableType,
                        true,
                        entry.Value.IsConstant,
                        entry.Value.IsConstant,
                        entry.Value.InitializationValue,
                        importService,
                        ExtensionClassEmpty.INSTANCE,
                        eventBeanTypedEventFactory,
                        eventTypeRepositoryPreconfigured,
                        beanEventTypeFactory);
                }
                catch (Exception t) {
                    throw new ConfigurationException("Error configuring variable '" + entry.Key + "': " + t.Message, t);
                }

                repo.AddVariable(entry.Key, meta);
            }
        }

        public static VariableMetaData CompileVariable(
            String variableName,
            String variableModuleName,
            NameAccessModifier variableVisibility,
            String optionalContextName,
            NameAccessModifier? optionalContextVisibility,
            String optionalModuleName,
            ClassIdentifierWArray variableType,
            bool isConstant,
            bool compileTimeConstant,
            Object initializationValue,
            ImportService importService,
            ExtensionClass extensionClass,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeRepositoryImpl eventTypeRepositoryPreconfigured,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            try {
                return GetTypeInfo(
                    variableName,
                    variableModuleName,
                    variableVisibility,
                    optionalContextName,
                    optionalContextVisibility,
                    optionalModuleName,
                    variableType,
                    false,
                    isConstant,
                    compileTimeConstant,
                    initializationValue,
                    importService,
                    extensionClass,
                    eventBeanTypedEventFactory,
                    eventTypeRepositoryPreconfigured,
                    beanEventTypeFactory);
            }
            catch (VariableTypeException t) {
                throw new ExprValidationException(t.Message, t);
            }
            catch (Exception t) {
                throw new ExprValidationException("Failed to compile variable '" + variableName + "': " + t.Message, t);
            }
        }

        public static string CheckVariableContextName(
            string optionalStatementContextName,
            VariableMetaData variableMetaData)
        {
            if (optionalStatementContextName == null) {
                if (variableMetaData.OptionalContextName != null) {
                    return "Variable '" +
                           variableMetaData.VariableName +
                           "' defined for use with context '" +
                           variableMetaData.OptionalContextName +
                           "' can only be accessed within that context";
                }
            }
            else {
                if (variableMetaData.OptionalContextName != null &&
                    !variableMetaData.OptionalContextName.Equals(optionalStatementContextName)) {
                    return "Variable '" +
                           variableMetaData.VariableName +
                           "' defined for use with context '" +
                           variableMetaData.OptionalContextName +
                           "' is not available for use with context '" +
                           optionalStatementContextName +
                           "'";
                }
            }

            return null;
        }

        private static VariableMetaData GetTypeInfo(
            string variableName,
            string variableModuleName,
            NameAccessModifier variableVisibility,
            string optionalContextName,
            NameAccessModifier? optionalContextVisibility,
            string optionalContextModule,
            ClassIdentifierWArray variableTypeWArray,
            bool preconfigured,
            bool constant,
            bool compileTimeConstant,
            object valueAsProvided,
            ImportService importService,
            ExtensionClass extensionClass,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeRepositoryImpl eventTypeRepositoryPreconfigured,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            // Determine the variable type
            var primitiveType = TypeHelper.GetPrimitiveTypeForName(variableTypeWArray.ClassIdentifier);
            var type = TypeHelper.GetTypeForSimpleName(variableTypeWArray.ClassIdentifier).GetBoxedType();
            Type arrayType = null;
            EventType eventType = null;
            if (type == null) {
                if (variableTypeWArray.ClassIdentifier.Equals("object", StringComparison.InvariantCultureIgnoreCase)) {
                    type = TypeHelper.GetArrayType(typeof(object), variableTypeWArray.ArrayDimensions);
                }

                if (type == null) {
                    eventType = eventTypeRepositoryPreconfigured.GetTypeByName(variableTypeWArray.ClassIdentifier);
                    if (eventType != null) {
                        type = eventType.UnderlyingType;
                    }
                }

                ImportException lastException = null;
                if (type == null) {
                    try {
                        type = importService.ResolveClass(variableTypeWArray.ClassIdentifier, false, extensionClass);
                        type = TypeHelper.GetArrayType(type, variableTypeWArray.ArrayDimensions);
                    }
                    catch (ImportException e) {
                        Log.Debug("Not found '" + type + "': " + e.Message, e);
                        lastException = e;
                        // expected
                    }
                }

                if (type == null) {
                    throw new VariableTypeException(
                        "Cannot create variable '" +
                        variableName +
                        "', type '" +
                        variableTypeWArray.ClassIdentifier +
                        "' is not a recognized type",
                        lastException);
                }

                if (variableTypeWArray.ArrayDimensions > 0 && eventType != null) {
                    throw new VariableTypeException(
                        "Cannot create variable '" +
                        variableName +
                        "', type '" +
                        variableTypeWArray.ClassIdentifier +
                        "' cannot be declared as an array type as it is an event type",
                        lastException);
                }
            }
            else {
                if (variableTypeWArray.ArrayDimensions > 0) {
                    if (variableTypeWArray.IsArrayOfPrimitive) {
                        if (primitiveType == null) {
                            throw new VariableTypeException(
                                "Cannot create variable '" +
                                variableName +
                                "', type '" +
                                variableTypeWArray.ClassIdentifier +
                                "' is not a primitive type");
                        }

                        arrayType = TypeHelper.GetArrayType(primitiveType, variableTypeWArray.ArrayDimensions);
                    }
                    else {
                        arrayType = TypeHelper.GetArrayType(type, variableTypeWArray.ArrayDimensions);
                    }
                }
            }

            if (eventType == null &&
                !type.IsBuiltinDataType() &&
                type != typeof(object) &&
                !type.IsArray &&
                !type.IsEnum) {
                if (variableTypeWArray.ArrayDimensions > 0) {
                    throw new VariableTypeException(
                        "Cannot create variable '" +
                        variableName +
                        "', type '" +
                        variableTypeWArray.ClassIdentifier +
                        "' cannot be declared as an array, only scalar types can be array");
                }

                eventType = beanEventTypeFactory.GetCreateBeanType(type, false);
            }

            if (arrayType != null) {
                type = arrayType;
            }

            var coerced = GetCoercedValue(valueAsProvided, eventType, variableName, type, eventBeanTypedEventFactory);
            return new VariableMetaData(
                variableName,
                variableModuleName,
                variableVisibility,
                optionalContextName,
                optionalContextVisibility,
                optionalContextModule,
                type,
                eventType,
                preconfigured,
                constant,
                compileTimeConstant,
                coerced,
                true);
        }

        private static object GetCoercedValue(
            object value,
            EventType eventType,
            string variableName,
            Type variableType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            var coercedValue = value;

            if (eventType != null) {
                if (value != null &&
                    !TypeHelper.IsSubclassOrImplementsInterface(
                        value.GetType(),
                        eventType.UnderlyingType)) {
                    throw new VariableTypeException(
                        "Variable '" +
                        variableName +
                        "' of declared event type '" +
                        eventType.Name +
                        "' underlying type '" +
                        eventType.UnderlyingType.CleanName() +
                        "' cannot be assigned a value of type '" +
                        value.GetType().CleanName() +
                        "'");
                }

                if (eventBeanTypedEventFactory != EventBeanTypedEventFactoryCompileTime.INSTANCE) {
                    coercedValue = eventBeanTypedEventFactory.AdapterForTypedObject(value, eventType);
                }
            }
            else if (variableType == typeof(object)) {
                // no validation
            }
            else {
                // allow string assignments to non-string variables
                if (coercedValue != null && coercedValue is string) {
                    try {
                        coercedValue = TypeHelper.Parse(variableType, (string) coercedValue);
                    }
                    catch (Exception ex) {
                        throw new VariableTypeException(
                            "Variable '" +
                            variableName +
                            "' of declared type " +
                            variableType.CleanName() +
                            " cannot be initialized by value '" +
                            coercedValue +
                            "': " +
                            ex);
                    }
                }

                if (coercedValue != null &&
                    !TypeHelper.IsSubclassOrImplementsInterface(coercedValue.GetType(), variableType)) {
                    var coercedValueType = coercedValue.GetType();
                    
                    // Lets see if the coerced value is an array of an element, whereas the variable is an array
                    // of the boxed variant of the type.  Technically, the arrays are not compatible and we need
                    // to coerce as a result.
                    
                    if (coercedValueType.IsArray &&
                        variableType.IsArray &&
                        coercedValueType.GetElementType().GetBoxedType() == variableType.GetElementType()) {
                        var coercedSourceArray = (Array) coercedValue;
                        var coercedDestArray = Array.CreateInstance(variableType.GetElementType(), coercedSourceArray.Length);
                        for (int ii = 0; ii < coercedSourceArray.Length; ii++) {
                            coercedDestArray.SetValue(coercedSourceArray.GetValue(ii), ii);
                        }

                        coercedValue = coercedDestArray;
                    }
                    else {
                        // if the declared type is not numeric or the init value is not numeric, fail
                        if (!variableType.IsNumeric() || !coercedValue.IsNumber()) {
                            throw GetVariableTypeException(variableName, variableType, coercedValue.GetType());
                        }

                        if (!coercedValue.GetType().CanCoerce(variableType)) {
                            throw GetVariableTypeException(variableName, variableType, coercedValueType);
                        }

                        // coerce
                        coercedValue = TypeHelper.CoerceBoxed(coercedValue, variableType);
                    }
                }
            }

            return coercedValue;
        }

        private static VariableTypeException GetVariableTypeException(
            string variableName,
            Type variableType,
            Type initValueClass)
        {
            return new VariableTypeException(
                "Variable '" +
                variableName +
                "' of declared type " +
                variableType.CleanName() +
                " cannot be initialized by a value of type " +
                initValueClass.CleanName());
        }
    }
} // end of namespace