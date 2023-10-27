using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using Common.Logging;

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
            foreach (var entry in variables)
            {
                string variableName = entry.Key.Trim();
                if (repo.GetMetadata(variableName) != null) {
                    continue;
                }

                VariableMetaData meta;
                try {
                    ClassDescriptor variableType = ClassDescriptor.ParseTypeText(entry.Value.VariableType);
                    VariableMetadataWithForgables result = GetTypeInfo(
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
                        beanEventTypeFactory,
                        null,
                        null);
                    meta = result.VariableMetaData;
                }
                catch (Exception ex) {
                    throw new ConfigurationException(
                        "Error configuring variable '" + variableName + "': " + ex.Message,
                        ex);
                }

                repo.AddVariable(variableName, meta);
            }
        }

        public static VariableMetadataWithForgables CompileVariable(
            string variableName,
            string variableModuleName,
            NameAccessModifier variableVisibility,
            string optionalContextName,
            NameAccessModifier? optionalContextVisibility,
            string optionalModuleName,
            ClassDescriptor variableType,
            bool isConstant,
            bool compileTimeConstant,
            object initializationValue,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
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
                    services.ImportServiceCompileTime,
                    services.ClassProvidedExtension,
                    EventBeanTypedEventFactoryCompileTime.INSTANCE,
                    services.EventTypeRepositoryPreconfigured,
                    services.BeanEventTypeFactoryPrivate,
                    raw,
                    services);
            }
            catch (VariableTypeException ex) {
                throw new ExprValidationException(ex.Message, ex);
            }
            catch (Exception ex) {
                throw new ExprValidationException(
                    "Failed to compile variable '" + variableName + "': " + ex.Message,
                    ex);
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

        private static VariableMetadataWithForgables GetTypeInfo(
            string variableName,
            string variableModuleName,
            NameAccessModifier variableVisibility,
            string optionalContextName,
            NameAccessModifier? optionalContextVisibility,
            string optionalContextModule,
            ClassDescriptor variableTypeWArray,
            bool preconfigured,
            bool constant,
            bool compileTimeConstant,
            object valueAsProvided,
            ImportService classpathImportService,
            ExtensionClass classpathExtension,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeRepositoryImpl eventTypeRepositoryPreconfigured,
            BeanEventTypeFactory beanEventTypeFactory,
            StatementRawInfo optionalRaw,
            StatementCompileTimeServices optionalServices)
        {

            Type variableClass = null;
            IList<StmtClassForgeableFactory> serdeForgeables = EmptyList<StmtClassForgeableFactory>.Instance;
            ExprValidationException exTypeResolution = null;
            try {
                variableClass = ImportTypeUtil.ResolveClassIdentifierToType(
                    variableTypeWArray,
                    true,
                    classpathImportService,
                    classpathExtension);
                if (variableClass == null) {
                    throw new ExprValidationException(
                        "Failed to resolve type parameter '" + variableTypeWArray.ToEPL() + "'");
                }
            }
            catch (ExprValidationException ex) {
                exTypeResolution = ex;
            }

            EventType variableEventType = null;
            if (variableClass == null) {
                variableEventType =
                    eventTypeRepositoryPreconfigured.GetTypeByName(variableTypeWArray.ClassIdentifier);

                if (variableEventType == null && optionalServices != null) {
                    variableEventType = optionalServices.EventTypeCompileTimeResolver
                        .GetTypeByName(variableTypeWArray.ClassIdentifier);
                    if (variableEventType != null) {
                        serdeForgeables = SerdeEventTypeUtility.Plan(
                            variableEventType,
                            optionalRaw,
                            optionalServices.SerdeEventTypeRegistry,
                            optionalServices.SerdeResolver,
                            optionalServices.StateMgmtSettingsProvider);
                    }
                }

                if (variableEventType != null) {
                    variableClass = variableEventType.UnderlyingType;
                }
            }

            if (variableClass == null) {
                throw new VariableTypeException(
                    "Cannot create variable '" +
                    variableName +
                    "', type '" +
                    variableTypeWArray.ClassIdentifier +
                    "' is not a recognized type",
                    exTypeResolution);
            }

            if (variableEventType != null &&
                (variableTypeWArray.ArrayDimensions > 0 || !variableTypeWArray.TypeParameters.IsEmpty())) {
                throw new VariableTypeException(
                    "Cannot create variable '" +
                    variableName +
                    "', type '" +
                    variableTypeWArray.ClassIdentifier +
                    "' cannot be declared as an array type and cannot receive type parameters as it is an event type",
                    exTypeResolution);
            }

            if (variableEventType == null &&
                !variableClass.IsBuiltinDataType() &&
                variableClass != typeof(object) &&
                !variableClass.IsArray() &&
                !variableClass.IsEnum &&
                variableClass.IsFragmentableType()) {
                if (variableTypeWArray.ArrayDimensions > 0) {
                    throw new VariableTypeException(
                        "Cannot create variable '" +
                        variableName +
                        "', type '" +
                        variableTypeWArray.ClassIdentifier +
                        "' cannot be declared as an array, only scalar types can be array");
                }

                variableEventType = beanEventTypeFactory.GetCreateBeanType(variableClass, false);
            }

            object coerced = GetCoercedValue(
                valueAsProvided,
                variableEventType,
                variableName,
                variableClass,
                eventBeanTypedEventFactory);
            VariableMetaData variableMetaData = new VariableMetaData(
                variableName,
                variableModuleName,
                variableVisibility,
                optionalContextName,
                optionalContextVisibility,
                optionalContextModule,
                variableClass,
                variableEventType,
                preconfigured,
                constant,
                compileTimeConstant,
                coerced,
                true);
            return new VariableMetadataWithForgables(variableMetaData, serdeForgeables);
        }

        private static object GetCoercedValue(
            object value,
            EventType eventType,
            string variableName,
            Type variableType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            var coercedValue = value;
            var valueType = value?.GetType();
            
            if (eventType != null) {
                if (value != null && !TypeHelper.IsSubclassOrImplementsInterface(valueType, eventType.UnderlyingType)) {
                    throw new VariableTypeException(
                        "Variable '" +
                        variableName +
                        "' of declared event type '" +
                        eventType.Name +
                        "' underlying type '" +
                        eventType.UnderlyingType.CleanName() +
                        "' cannot be assigned a value of type '" +
                        valueType.CleanName() +
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
                        coercedValue = TypeHelper.Parse(variableType, (string)coercedValue);
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

                var coercedValueType = coercedValue?.GetType();
                if (coercedValue != null &&
                    !TypeHelper.IsSubclassOrImplementsInterface(coercedValueType, variableType)) {
                    // if the declared type is not numeric or the init value is not numeric, fail
                    if (!variableType.IsTypeNumeric() || !coercedValue.IsNumber()) {
                        throw GetVariableTypeException(variableName, variableType, coercedValueType);
                    }

                    if (!coercedValueType.CanCoerce(variableType)) {
                        throw GetVariableTypeException(variableName, variableType, coercedValueType);
                    }

                    // coerce
                    coercedValue = TypeHelper.CoerceBoxed(coercedValue, variableType);
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
}
