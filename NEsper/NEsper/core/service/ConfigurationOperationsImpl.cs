///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.util;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.events.xml;
using com.espertech.esper.filter;
using com.espertech.esper.pattern.pool;
using com.espertech.esper.rowregex;
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Provides runtime engine configuration operations.
    /// </summary>
    public class ConfigurationOperationsImpl : ConfigurationOperations
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventTypeIdGenerator _eventTypeIdGenerator;
        private readonly EngineImportService _engineImportService;
        private readonly VariableService _variableService;
        private readonly EngineSettingsService _engineSettingsService;
        private readonly ValueAddEventService _valueAddEventService;
        private readonly MetricReportingService _metricReportingService;
        private readonly StatementEventTypeRef _statementEventTypeRef;
        private readonly StatementVariableRef _statementVariableRef;
        private readonly PluggableObjectCollection _plugInViews;
        private readonly FilterService _filterService;
        private readonly PatternSubexpressionPoolEngineSvc _patternSubexpressionPoolSvc;
        private readonly MatchRecognizeStatePoolEngineSvc _matchRecognizeStatePoolEngineSvc;
        private readonly TableService _tableService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventAdapterService">is the event wrapper and type service</param>
        /// <param name="eventTypeIdGenerator">The event type id generator.</param>
        /// <param name="engineImportService">for imported aggregation functions and static functions</param>
        /// <param name="variableService">provides access to variable values</param>
        /// <param name="engineSettingsService">some engine settings are writable</param>
        /// <param name="valueAddEventService">Update event handling</param>
        /// <param name="metricReportingService">for metric reporting</param>
        /// <param name="statementEventTypeRef">statement to event type reference holding</param>
        /// <param name="statementVariableRef">statement to variable reference holding</param>
        /// <param name="plugInViews">The plug in views.</param>
        /// <param name="filterService">The filter service.</param>
        /// <param name="patternSubexpressionPoolSvc">The pattern subexpression pool SVC.</param>
        /// <param name="tableService">The table service.</param>
        public ConfigurationOperationsImpl(
            EventAdapterService eventAdapterService,
            EventTypeIdGenerator eventTypeIdGenerator,
            EngineImportService engineImportService,
            VariableService variableService,
            EngineSettingsService engineSettingsService,
            ValueAddEventService valueAddEventService,
            MetricReportingService metricReportingService,
            StatementEventTypeRef statementEventTypeRef,
            StatementVariableRef statementVariableRef,
            PluggableObjectCollection plugInViews,
            FilterService filterService,
            PatternSubexpressionPoolEngineSvc patternSubexpressionPoolSvc,
            MatchRecognizeStatePoolEngineSvc matchRecognizeStatePoolEngineSvc, 
            TableService tableService)
        {
            _eventAdapterService = eventAdapterService;
            _eventTypeIdGenerator = eventTypeIdGenerator;
            _engineImportService = engineImportService;
            _variableService = variableService;
            _engineSettingsService = engineSettingsService;
            _valueAddEventService = valueAddEventService;
            _metricReportingService = metricReportingService;
            _statementEventTypeRef = statementEventTypeRef;
            _statementVariableRef = statementVariableRef;
            _plugInViews = plugInViews;
            _filterService = filterService;
            _patternSubexpressionPoolSvc = patternSubexpressionPoolSvc;
            _matchRecognizeStatePoolEngineSvc = matchRecognizeStatePoolEngineSvc;
            _tableService = tableService;
        }
    
        public void AddEventTypeAutoName(String @namespace)
        {
            _eventAdapterService.AddAutoNamePackage(@namespace);
        }
    
        public void AddPlugInView(String @namespace, String name, String viewFactoryClass)
        {
            var configurationPlugInView = new ConfigurationPlugInView();
            configurationPlugInView.Namespace = @namespace;
            configurationPlugInView.Name = name;
            configurationPlugInView.FactoryClassName = viewFactoryClass;
            _plugInViews.AddViews(
                Collections.SingletonList(configurationPlugInView),
                Collections.GetEmptyList<ConfigurationPlugInVirtualDataWindow>());
        }

        public void AddPlugInAggregationMultiFunction(ConfigurationPlugInAggregationMultiFunction config)
        {
            try
            {
                _engineImportService.AddAggregationMultiFunction(config);
            }
            catch (EngineImportException e)
            {
                throw new ConfigurationException(e.Message, e);
            }
        }

        /// <summary>
        /// Adds a plug-in aggregation function given a EPL function name and an aggregation factory class name.
        /// <para />
        /// The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new aggregation function name for use in EPL</param>
        /// <param name="aggregationFactoryClassName">is the fully-qualified class name of the class implementing the aggregation function factory interface <seealso cref="AggregationFunctionFactory" /></param>
        /// <exception cref="ConfigurationException"></exception>
        /// <throws>ConfigurationException is thrown to indicate a problem adding the aggregation function</throws>
        public void AddPlugInAggregationFunctionFactory(String functionName, String aggregationFactoryClassName)
        {
            try
            {
                var desc = new ConfigurationPlugInAggregationFunction(functionName, aggregationFactoryClassName);
                _engineImportService.AddAggregation(functionName, desc);
            }
            catch (EngineImportException e)
            {
                throw new ConfigurationException(e.Message, e);
            }
        }

        /// <summary>
        /// Adds a plug-in aggregation function given a EPL function name and an aggregation factory class name.
        /// <para />
        /// The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new aggregation function name for use in EPL</param>
        /// <param name="aggregationFactoryType">Type of the aggregation factory.  Must implement <seealso cref="AggregationFunctionFactory"/></param>
        /// <throws>ConfigurationException is thrown to indicate a problem adding the aggregation function</throws>
        public void AddPlugInAggregationFunctionFactory(String functionName, Type aggregationFactoryType)
        {
            AddPlugInAggregationFunctionFactory(functionName, aggregationFactoryType.AssemblyQualifiedName);
        }

        /// <summary>
        /// Adds the plug in aggregation function factory.
        /// </summary>
        /// <typeparam name="T">Type of the aggregation factory.  Must implement <seealso cref="AggregationFunctionFactory"/></typeparam>
        /// <param name="functionName">Name of the function.</param>
        public void AddPlugInAggregationFunctionFactory<T>(String functionName) where T : AggregationFunctionFactory
        {
            AddPlugInAggregationFunctionFactory(functionName, typeof(T).AssemblyQualifiedName);
        }

        public void AddPlugInSingleRowFunction(String functionName, String className, String methodName)
        {
            InternalAddPlugInSingleRowFunction(
                functionName, className, methodName, ValueCache.DISABLED, FilterOptimizable.ENABLED, false);
        }

        public void AddPlugInSingleRowFunction(String functionName, String className, String methodName, ValueCache valueCache)
        {
            InternalAddPlugInSingleRowFunction(
                functionName, className, methodName, valueCache, FilterOptimizable.ENABLED, false);
        }

        public void AddPlugInSingleRowFunction(String functionName, String className, String methodName, FilterOptimizable filterOptimizable)
        {
            InternalAddPlugInSingleRowFunction(
                functionName, className, methodName, ValueCache.DISABLED, filterOptimizable, false);
        }

        public void AddPlugInSingleRowFunction(String functionName, String className, String methodName, ValueCache valueCache, FilterOptimizable filterOptimizable, bool rethrowExceptions)
        {
            InternalAddPlugInSingleRowFunction(functionName, className, methodName, valueCache, filterOptimizable, rethrowExceptions);
        }

        private void InternalAddPlugInSingleRowFunction(String functionName, String className, String methodName, ValueCache valueCache, FilterOptimizable filterOptimizable, bool rethrowExceptions)
        {
            try
            {
                _engineImportService.AddSingleRow(
                    functionName, className, methodName, valueCache, filterOptimizable, rethrowExceptions);
            }
            catch (EngineImportException e)
            {
                throw new ConfigurationException(e.Message, e);
            }
        }

        public void AddAnnotationImport(string importName, string assemblyNameOrFile)
        {
            try
            {
                _engineImportService.AddAnnotationImport(new AutoImportDesc(importName, assemblyNameOrFile));
            }
            catch (EngineImportException e)
            {
                throw new ConfigurationException(e.Message, e);
            }
        }

        public void AddAnnotationImport(string importName)
        {
            AddAnnotationImport(importName, null);
        }

        public void AddAnnotationImport(Type autoImport)
        {
            AddAnnotationImport(autoImport.FullName, autoImport.AssemblyQualifiedName);
        }

        public void AddAnnotationImport<T>(bool importNamespace)
        {
            if (importNamespace)
            {
                AddAnnotationImport(typeof(T).Namespace, typeof(T).Assembly.FullName);
            }
            else
            {
                AddAnnotationImport(typeof(T).FullName, typeof(T).Assembly.FullName);
            }
        }

        public void AddImport(string importName, string assemblyNameOrFile)
        {
            try
            {
                _engineImportService.AddImport(new AutoImportDesc(importName, assemblyNameOrFile));
            }
            catch (EngineImportException e)
            {
                throw new ConfigurationException(e.Message, e);
            }
        }

        public void AddImport(string importName)
        {
            string[] importParts = importName.Split(',');
            if (importParts.Length == 1)
            {
                AddImport(importName, null);
            }
            else
            {
                AddImport(importParts[0], importParts[1]);
            }
        }
    
        public void AddImport(Type importClass)
        {
            if (importClass.IsNested)
                AddImport(importClass.DeclaringType.FullName, null);
            else
                AddImport(importClass.Namespace, null);
        }

        public void AddImport<T>()
        {
            AddImport(typeof (T));
        }

        public void AddNamespaceImport<T>()
        {
            var importClass = typeof (T);
            if (importClass.IsNested)
                AddImport(importClass.DeclaringType.FullName, null);
            else
                AddImport(importClass.Namespace, null);
        }

        public bool IsEventTypeExists(String eventTypeName)
        {
            return _eventAdapterService.GetEventTypeByName(eventTypeName) != null;
        }

        public void AddEventType(String eventTypeName, String nativeEventTypeName)
        {
            CheckTableExists(eventTypeName);

            try
            {
                _eventAdapterService.AddBeanType(eventTypeName, nativeEventTypeName, false, false, true, true);
            }
            catch (EventAdapterException t)
            {
                throw new ConfigurationException(t.Message, t);
            }
        }
    
        public void AddEventType(String eventTypeName, Type eventType)
        {
            CheckTableExists(eventTypeName);

            try
            {
                _eventAdapterService.AddBeanType(eventTypeName, eventType, false, true, true);
            }
            catch (EventAdapterException t)
            {
                throw new ConfigurationException(t.Message, t);
            }
        }
    
        public void AddEventType(Type eventType)
        {
            CheckTableExists(eventType.Name);

            try
            {
                _eventAdapterService.AddBeanType(eventType.Name, eventType, false, true, true);
            }
            catch (EventAdapterException t)
            {
                throw new ConfigurationException(t.Message, t);
            }
        }

        public void AddEventType<T>(string eventTypeName)
        {
            AddEventType(eventTypeName, typeof(T));
        }

        public void AddEventType<T>()
        {
            AddEventType(typeof(T));
        }
    
        public void AddEventType(String eventTypeName, Properties typeMap)
        {
            CheckTableExists(eventTypeName);

            var types = TypeHelper.GetClassObjectFromPropertyTypeNames(typeMap);
            try
            {
                _eventAdapterService.AddNestableMapType(eventTypeName, types, null, false, true, true, false, false);
            }
            catch (EventAdapterException t)
            {
                throw new ConfigurationException(t.Message, t);
            }
        }
    
        public void AddEventType(String eventTypeName, IDictionary<String, Object> typeMap)
        {
            CheckTableExists(eventTypeName);

            try
            {
                IDictionary<String, Object> compiledProperties = EventTypeUtility.CompileMapTypeProperties(typeMap, _eventAdapterService);
                _eventAdapterService.AddNestableMapType(eventTypeName, compiledProperties, null, false, true, true, false, false);
            }
            catch (EventAdapterException t)
            {
                throw new ConfigurationException(t.Message, t);
            }
        }
    
        public void AddEventType(String eventTypeName, IDictionary<String, Object> typeMap, String[] superTypes)
        {
            CheckTableExists(eventTypeName);

            ConfigurationEventTypeMap optionalConfig = null;
            if ((superTypes != null) && (superTypes.Length > 0))
            {
                optionalConfig = new ConfigurationEventTypeMap();
                optionalConfig.SuperTypes.AddAll(superTypes);
            }
    
            try
            {
                _eventAdapterService.AddNestableMapType(eventTypeName, typeMap, optionalConfig, false, true, true, false, false);
            }
            catch (EventAdapterException t)
            {
                throw new ConfigurationException(t.Message, t);
            }
        }
    
        public void AddEventType(String eventTypeName, IDictionary<String, Object> typeMap, ConfigurationEventTypeMap mapConfig)
        {
            CheckTableExists(eventTypeName);

            try
            {
                _eventAdapterService.AddNestableMapType(eventTypeName, typeMap, mapConfig, false, true, true, false, false);
            }
            catch (EventAdapterException t)
            {
                throw new ConfigurationException(t.Message, t);
            }
        }

        public void AddEventType(String eventTypeName, String[] propertyNames, Object[] propertyTypes) 
        {
            AddEventType(eventTypeName, propertyNames, propertyTypes, null);
        }

        public void AddEventType(String eventTypeName, String[] propertyNames, Object[] propertyTypes, ConfigurationEventTypeObjectArray optionalConfiguration)
        {
            CheckTableExists(eventTypeName);

            try
            {
                LinkedHashMap<String, Object> propertyTypesMap = EventTypeUtility.ValidateObjectArrayDef(propertyNames,
                                                                                                         propertyTypes);
                IDictionary<String, Object> compiledProperties =
                    EventTypeUtility.CompileMapTypeProperties(propertyTypesMap, _eventAdapterService);
                _eventAdapterService.AddNestableObjectArrayType(eventTypeName, compiledProperties, optionalConfiguration,
                                                                false, true, true, false, false, false, null);
            }
            catch (EventAdapterException t)
            {
                throw new ConfigurationException(t.Message, t);
            }
        }

        public void AddEventType(String eventTypeName, ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc)
        {
            SchemaModel schemaModel = null;

            CheckTableExists(eventTypeName);

            if ((xmlDOMEventTypeDesc.SchemaResource != null) || (xmlDOMEventTypeDesc.SchemaText != null))
            {
                try
                {
                    schemaModel = XSDSchemaMapper.LoadAndMap(xmlDOMEventTypeDesc.SchemaResource, xmlDOMEventTypeDesc.SchemaText, 2);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException(ex.Message, ex);
                }
            }
    
            try
            {
                _eventAdapterService.AddXMLDOMType(eventTypeName, xmlDOMEventTypeDesc, schemaModel, false);
            }
            catch (EventAdapterException t)
            {
                throw new ConfigurationException(t.Message, t);
            }
        }

        public void AddVariable<T>(string variableName, T initializationValue)
        {
            AddVariable(variableName, typeof (T), initializationValue);
        }

        public void AddVariable(String variableName, Type type, Object initializationValue)
        {
            AddVariable(variableName, type.GetBoxedType().FullName, initializationValue, false);
        }
    
        public void AddVariable(String variableName, String eventTypeName, Object initializationValue)
        {
            AddVariable(variableName, eventTypeName, initializationValue, false);
        }
    
        public void AddVariable(String variableName, String type, Object initializationValue, bool constant)
        {
            try
            {
                var arrayType = TypeHelper.IsGetArrayType(type);
                _variableService.CreateNewVariable(null, variableName, arrayType.First, constant, arrayType.Second, false, initializationValue, _engineImportService);
                _variableService.AllocateVariableState(variableName, EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID, null, false);
                _statementVariableRef.AddConfiguredVariable(variableName);
            }
            catch (VariableExistsException e)
            {
                throw new ConfigurationException("Error creating variable: " + e.Message, e);
            }
            catch (VariableTypeException e)
            {
                throw new ConfigurationException("Error creating variable: " + e.Message, e);
            }
        }

        public void AddPlugInEventType(string eventTypeName, IList<Uri> resolutionURIs, object initializer)
        {
            try
            {
                _eventAdapterService.AddPlugInEventType(eventTypeName, resolutionURIs, initializer);
            }
            catch (EventAdapterException e)
            {
                throw new ConfigurationException("Error adding plug-in event type: " + e.Message, e);
            }
        }

        public IList<Uri> PlugInEventTypeResolutionURIs
        {
            set { _engineSettingsService.PlugInEventTypeResolutionURIs = value; }
            get { return _engineSettingsService.PlugInEventTypeResolutionURIs; }
        }

        public void AddRevisionEventType(String revisionEventTypeName, ConfigurationRevisionEventType revisionEventTypeConfig)
        {
            CheckTableExists(revisionEventTypeName);
            _valueAddEventService.AddRevisionEventType(revisionEventTypeName, revisionEventTypeConfig, _eventAdapterService);
        }
    
        public void AddVariantStream(String variantEventTypeName, ConfigurationVariantStream variantStreamConfig)
        {
            CheckTableExists(variantEventTypeName);
            _valueAddEventService.AddVariantStream(variantEventTypeName, variantStreamConfig, _eventAdapterService, _eventTypeIdGenerator);
        }
    
        public void UpdateMapEventType(String mapeventTypeName, IDictionary<String, Object> typeMap)
        {
            try
            {
                _eventAdapterService.UpdateMapEventType(mapeventTypeName, typeMap);
            }
            catch (EventAdapterException e)
            {
                throw new ConfigurationException("Error updating Map event type: " + e.Message, e);
            }
        }

        public void UpdateObjectArrayEventType(String objectArrayEventTypeName, String[] propertyNames, Object[] propertyTypes)
        {
            try
            {
                LinkedHashMap<String, Object> typeMap = EventTypeUtility.ValidateObjectArrayDef(propertyNames, propertyTypes);
                _eventAdapterService.UpdateObjectArrayEventType(objectArrayEventTypeName, typeMap);
            }
            catch (EventAdapterException e)
            {
                throw new ConfigurationException("Error updating Object-array event type: " + e.Message, e);
            }
        }
    
        public void ReplaceXMLEventType(String xmlEventTypeName, ConfigurationEventTypeXMLDOM config)
        {
            SchemaModel schemaModel = null;
            if (config.SchemaResource != null || config.SchemaText != null)
            {
                try
                {
                    schemaModel = XSDSchemaMapper.LoadAndMap(config.SchemaResource, config.SchemaText, 2);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException(ex.Message, ex);
                }
            }
    
            try
            {
                _eventAdapterService.ReplaceXMLEventType(xmlEventTypeName, config, schemaModel);
            }
            catch (EventAdapterException e)
            {
                throw new ConfigurationException("Error updating XML event type: " + e.Message, e);
            }
        }
    
        public void SetMetricsReportingInterval(String stmtGroupName, long newInterval)
        {
            try
            {
                _metricReportingService.SetMetricsReportingInterval(stmtGroupName, newInterval);
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Error updating interval for metric reporting: " + e.Message, e);
            }
        }
    
    
        public void SetMetricsReportingStmtEnabled(String statementName)
        {
            try
            {
                _metricReportingService.SetMetricsReportingStmtEnabled(statementName);
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Error enabling metric reporting for statement: " + e.Message, e);
            }
        }
    
        public void SetMetricsReportingStmtDisabled(String statementName)
        {
            try
            {
                _metricReportingService.SetMetricsReportingStmtDisabled(statementName);
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Error enabling metric reporting for statement: " + e.Message, e);
            }
        }
    
        public void SetMetricsReportingEnabled()
        {
            try
            {
                _metricReportingService.SetMetricsReportingEnabled();
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Error enabling metric reporting: " + e.Message, e);
            }
        }
    
        public void SetMetricsReportingDisabled()
        {
            try
            {
                _metricReportingService.SetMetricsReportingDisabled();
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Error enabling metric reporting: " + e.Message, e);
            }
        }
    
        public bool IsVariantStreamExists(String name)
        {
            ValueAddEventProcessor processor = _valueAddEventService.GetValueAddProcessor(name);
            if (processor == null)
            {
                return false;
            }
            return processor.ValueAddEventType is VariantEventType;
        }
    
        public bool RemoveEventType(String name, bool force)
        {
            if (!force) {
                ICollection<String> statements = _statementEventTypeRef.GetStatementNamesForType(name);
                if ((statements != null) && (statements.IsNotEmpty())) {
                    throw new ConfigurationException("Event type '" + name + "' is in use by one or more statements");
                }
            }
    
            EventType type = _eventAdapterService.GetEventTypeByName(name);
            if (type == null)
            {
                return false;
            }
    
            _eventAdapterService.RemoveType(name);
            _statementEventTypeRef.RemoveReferencesType(name);
            _filterService.RemoveType(type);
            return true;
        }
    
        public bool RemoveVariable(String name, bool force)
        {
            if (!force) {
                ICollection<String> statements = _statementVariableRef.GetStatementNamesForVar(name);
                if ((statements != null) && (statements.IsNotEmpty())) {
                    throw new ConfigurationException("Variable '" + name + "' is in use by one or more statements");
                }
            }

            var reader = _variableService.GetReader(name, EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID);
            if (reader == null)
            {
                return false;
            }
    
            _variableService.RemoveVariableIfFound(name);
            _statementVariableRef.RemoveReferencesVariable(name);
            _statementVariableRef.RemoveConfiguredVariable(name);
            return true;
        }
    
        public ICollection<String> GetEventTypeNameUsedBy(String name)
        {
            ICollection<String> statements = _statementEventTypeRef.GetStatementNamesForType(name);
            if ((statements == null) || (statements.IsEmpty()))
            {
                return Collections.GetEmptyList<string>();
            }
            return statements.AsReadOnlyCollection();
        }
    
        public ICollection<String> GetVariableNameUsedBy(String variableName)
        {
            ICollection<String> statements = _statementVariableRef.GetStatementNamesForVar(variableName);
            if ((statements == null) || (statements.IsEmpty()))
            {
                return Collections.GetEmptyList<string>();
            }
            return statements.AsReadOnlyCollection();
        }
    
        public EventType GetEventType(String eventTypeName)
        {
            return _eventAdapterService.GetEventTypeByName(eventTypeName);
        }

        public ICollection<EventType> EventTypes
        {
            get { return _eventAdapterService.AllTypes; }
        }

        public void AddEventType(String eventTypeName, String eventType, ConfigurationEventTypeLegacy legacyEventTypeDesc)
        {
            CheckTableExists(eventTypeName);

            try {
                IDictionary<String, ConfigurationEventTypeLegacy> map = new Dictionary<String, ConfigurationEventTypeLegacy>();
                map.Put(eventType, legacyEventTypeDesc);
                _eventAdapterService.TypeLegacyConfigs = map;
                _eventAdapterService.AddBeanType(eventTypeName, eventType, false, false, false, true);
            }
            catch (EventAdapterException ex) {
                throw new ConfigurationException("Failed to add legacy event type definition for type '" + eventTypeName + "': " + ex.Message, ex);
            }
        }

        private void CheckTableExists(String eventTypeName)
        {
            try
            {
                EPLValidationUtil.ValidateTableExists(_tableService, eventTypeName);
            }
            catch (ExprValidationException ex)
            {
                throw new ConfigurationException(ex.Message, ex);
            }
        }

        public long PatternMaxSubexpressions
        {
            get { return _patternSubexpressionPoolSvc.PatternMaxSubexpressions.GetValueOrDefault(); }
            set
            {
                if (_patternSubexpressionPoolSvc != null)
                {
                    _patternSubexpressionPoolSvc.PatternMaxSubexpressions = value;
                }
            }
        }

        public long? MatchRecognizeMaxStates
        {
            get
            {
                if (_matchRecognizeStatePoolEngineSvc != null)
                {
                    return _matchRecognizeStatePoolEngineSvc.MatchRecognizeMaxStates;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (_matchRecognizeStatePoolEngineSvc != null)
                {
                    _matchRecognizeStatePoolEngineSvc.MatchRecognizeMaxStates = value;
                }
            }
        }
    }
}
