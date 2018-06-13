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
using com.espertech.esper.collection;
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
    /// <summary>Provides runtime engine configuration operations.</summary>
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
        private readonly IDictionary<string, Object> _transientConfiguration;
        private readonly IResourceManager _resourceManager;

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
            TableService tableService,
            IResourceManager resourceManager,
            IDictionary<string, object> transientConfiguration)
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
            _transientConfiguration = transientConfiguration;
            _resourceManager = resourceManager;
        }
    
        public void AddEventTypeAutoName(string @namespace) {
            _eventAdapterService.AddAutoNamePackage(@namespace);
        }
    
        public void AddPlugInView(string @namespace, string name, string viewFactoryClass) {
            var configurationPlugInView = new ConfigurationPlugInView();
            configurationPlugInView.Namespace = @namespace;
            configurationPlugInView.Name = name;
            configurationPlugInView.FactoryClassName = viewFactoryClass;
            _plugInViews.AddViews(
                Collections.SingletonList<ConfigurationPlugInView>(configurationPlugInView), 
                Collections.GetEmptyList<ConfigurationPlugInVirtualDataWindow>(), 
                _engineImportService);
        }

        public void AddPlugInView(string @namespace, string name, Type viewFactoryClass)
        {
            AddPlugInView(@namespace, name, viewFactoryClass.AssemblyQualifiedName);
        }

        public void AddPlugInAggregationMultiFunction(ConfigurationPlugInAggregationMultiFunction config) {
            try {
                _engineImportService.AddAggregationMultiFunction(config);
            } catch (EngineImportException e) {
                throw new ConfigurationException(e.Message, e);
            }
        }

        public void AddPlugInAggregationFunctionFactory(string functionName, Type aggregationFactoryClass)
        {
            AddPlugInAggregationFunctionFactory(functionName, aggregationFactoryClass.AssemblyQualifiedName);
        }

        public void AddPlugInAggregationFunctionFactory(string functionName, string aggregationFactoryClassName) {
            try {
                var desc = new ConfigurationPlugInAggregationFunction(functionName, aggregationFactoryClassName);
                _engineImportService.AddAggregation(functionName, desc);
            } catch (EngineImportException e) {
                throw new ConfigurationException(e.Message, e);
            }
        }

        public void AddPlugInSingleRowFunction(string functionName, Type clazz, string methodName) {
            AddPlugInSingleRowFunction(functionName, clazz.AssemblyQualifiedName, methodName);
        }
        public void AddPlugInSingleRowFunction(string functionName, string className, string methodName) {
            InternalAddPlugInSingleRowFunction(functionName, className, methodName, ValueCacheEnum.DISABLED, FilterOptimizableEnum.ENABLED, false, null);
        }

        public void AddPlugInSingleRowFunction(string functionName, Type clazz, string methodName, ValueCacheEnum valueCache) {
            AddPlugInSingleRowFunction(functionName, clazz.AssemblyQualifiedName, methodName, valueCache);
        }
        public void AddPlugInSingleRowFunction(string functionName, string className, string methodName, ValueCacheEnum valueCache) {
            InternalAddPlugInSingleRowFunction(functionName, className, methodName, valueCache, FilterOptimizableEnum.ENABLED, false, null);
        }

        public void AddPlugInSingleRowFunction(string functionName, Type clazz, string methodName, FilterOptimizableEnum filterOptimizable) {
            AddPlugInSingleRowFunction(functionName, clazz.AssemblyQualifiedName, methodName, filterOptimizable);
        }
        public void AddPlugInSingleRowFunction(string functionName, string className, string methodName, FilterOptimizableEnum filterOptimizable) {
            InternalAddPlugInSingleRowFunction(functionName, className, methodName, ValueCacheEnum.DISABLED, filterOptimizable, false, null);
        }

        public void AddPlugInSingleRowFunction(string functionName, Type clazz, string methodName, ValueCacheEnum valueCache, FilterOptimizableEnum filterOptimizable, bool rethrowExceptions) {
            AddPlugInSingleRowFunction(functionName, clazz.AssemblyQualifiedName, methodName, valueCache, filterOptimizable, rethrowExceptions);
        }
        public void AddPlugInSingleRowFunction(string functionName, string className, string methodName, ValueCacheEnum valueCache, FilterOptimizableEnum filterOptimizable, bool rethrowExceptions) {
            InternalAddPlugInSingleRowFunction(functionName, className, methodName, valueCache, filterOptimizable, rethrowExceptions, null);
        }
    
        public void AddPlugInSingleRowFunction(ConfigurationPlugInSingleRowFunction config)
        {
            InternalAddPlugInSingleRowFunction(
                config.Name,
                config.FunctionClassName, 
                config.FunctionMethodName,
                config.ValueCache,
                config.FilterOptimizable, 
                config.IsRethrowExceptions, 
                config.EventTypeName);
        }
    
        private void InternalAddPlugInSingleRowFunction(string functionName, string className, string methodName, ValueCacheEnum valueCache, FilterOptimizableEnum filterOptimizable, bool rethrowExceptions, string optionalEventTypeName) {
            try {
                _engineImportService.AddSingleRow(functionName, className, methodName, valueCache, filterOptimizable, rethrowExceptions, optionalEventTypeName);
            } catch (EngineImportException e) {
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
            AddAnnotationImport(autoImport.FullName, autoImport.Assembly.FullName);
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
            AddImport(typeof(T));
        }

        public void AddNamespaceImport<T>()
        {
            var importClass = typeof(T);
            if (importClass.IsNested)
                AddImport(importClass.DeclaringType.FullName, null);
            else
                AddImport(importClass.Namespace, null);
        }

        public bool IsEventTypeExists(string eventTypeName)
        {
            return _eventAdapterService.GetEventTypeByName(eventTypeName) != null;
        }

        public void AddEventType<T>(String eventTypeName)
        {
            AddEventType(eventTypeName, typeof(T).AssemblyQualifiedName);
        }

        public void AddEventType<T>()
        {
            AddEventType(typeof(T).Name, typeof(T).AssemblyQualifiedName);
        }

        public void AddEventType(string eventTypeName, string eventTypeTypeName)
        {
            CheckTableExists(eventTypeName);
            try {
                _eventAdapterService.AddBeanType(eventTypeName, eventTypeTypeName, false, false, true, true);
            } catch (EventAdapterException t) {
                throw new ConfigurationException(t.Message, t);
            }
        }

        public void AddEventType(string eventTypeName, Type eventType)
        {
            CheckTableExists(eventTypeName);
            try {
                _eventAdapterService.AddBeanType(eventTypeName, eventType, false, true, true);
            } catch (EventAdapterException t) {
                throw new ConfigurationException(t.Message, t);
            }
        }
    
        public void AddEventType(Type eventType) {
            CheckTableExists(eventType.Name);
            try {
                _eventAdapterService.AddBeanType(eventType.Name, eventType, false, true, true);
            } catch (EventAdapterException t) {
                throw new ConfigurationException(t.Message, t);
            }
        }
    
        public void AddEventType(string eventTypeName, Properties typeMap) {
            CheckTableExists(eventTypeName);
            IDictionary<string, Object> types = TypeHelper.GetClassObjectFromPropertyTypeNames(
                typeMap, _engineImportService.GetClassForNameProvider());
            try {
                _eventAdapterService.AddNestableMapType(eventTypeName, types, null, false, true, true, false, false);
            } catch (EventAdapterException t) {
                throw new ConfigurationException(t.Message, t);
            }
        }
    
        public void AddEventType(string eventTypeName, IDictionary<string, Object> typeMap) {
            CheckTableExists(eventTypeName);
            try {
                IDictionary<string, Object> compiledProperties = EventTypeUtility.CompileMapTypeProperties(typeMap, _eventAdapterService);
                _eventAdapterService.AddNestableMapType(eventTypeName, compiledProperties, null, false, true, true, false, false);
            } catch (EventAdapterException t) {
                throw new ConfigurationException(t.Message, t);
            }
        }
    
        public void AddEventType(string eventTypeName, IDictionary<string, Object> typeMap, string[] superTypes) {
            CheckTableExists(eventTypeName);
            ConfigurationEventTypeMap optionalConfig = null;
            if ((superTypes != null) && (superTypes.Length > 0)) {
                optionalConfig = new ConfigurationEventTypeMap();
                optionalConfig.SuperTypes.AddAll(superTypes);
            }
    
            try {
                _eventAdapterService.AddNestableMapType(eventTypeName, typeMap, optionalConfig, false, true, true, false, false);
            } catch (EventAdapterException t) {
                throw new ConfigurationException(t.Message, t);
            }
        }
    
        public void AddEventType(string eventTypeName, IDictionary<string, Object> typeMap, ConfigurationEventTypeMap mapConfig) {
            CheckTableExists(eventTypeName);
            try {
                _eventAdapterService.AddNestableMapType(eventTypeName, typeMap, mapConfig, false, true, true, false, false);
            } catch (EventAdapterException t) {
                throw new ConfigurationException(t.Message, t);
            }
        }
    
        public void AddEventType(string eventTypeName, string[] propertyNames, Object[] propertyTypes) {
            AddEventType(eventTypeName, propertyNames, propertyTypes, null);
        }
    
        public void AddEventType(string eventTypeName, string[] propertyNames, Object[] propertyTypes, ConfigurationEventTypeObjectArray optionalConfiguration) {
            CheckTableExists(eventTypeName);
            try {
                var propertyTypesMap = EventTypeUtility.ValidateObjectArrayDef(propertyNames, propertyTypes);
                var compiledProperties = EventTypeUtility.CompileMapTypeProperties(propertyTypesMap, _eventAdapterService);
                _eventAdapterService.AddNestableObjectArrayType(eventTypeName, compiledProperties, optionalConfiguration, false, true, true, false, false, false, null);
            } catch (EventAdapterException t) {
                throw new ConfigurationException(t.Message, t);
            }
        }
    
        public void AddEventType(string eventTypeName, ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc) {
            CheckTableExists(eventTypeName);
            SchemaModel schemaModel = null;
    
            if ((xmlDOMEventTypeDesc.SchemaResource != null) || (xmlDOMEventTypeDesc.SchemaText != null)) {
                try {
                    schemaModel = XSDSchemaMapper.LoadAndMap(
                        xmlDOMEventTypeDesc.SchemaResource, 
                        xmlDOMEventTypeDesc.SchemaText, 
                        _engineImportService,
                        _resourceManager);
                } catch (Exception ex) {
                    throw new ConfigurationException(ex.Message, ex);
                }
            }
    
            try {
                _eventAdapterService.AddXMLDOMType(eventTypeName, xmlDOMEventTypeDesc, schemaModel, false);
            } catch (EventAdapterException t) {
                throw new ConfigurationException(t.Message, t);
            }
        }

        public void AddVariable<TValue>(string variableName, TValue initializationValue)
        {
            AddVariable(variableName, typeof(TValue).FullName, initializationValue, false);
        }

        public void AddVariable(string variableName, Type type, Object initializationValue) {
            AddVariable(variableName, type.FullName, initializationValue, false);
        }
    
        public void AddVariable(string variableName, string eventTypeName, Object initializationValue) {
            AddVariable(variableName, eventTypeName, initializationValue, false);
        }
    
        public void AddVariable(string variableName, string type, Object initializationValue, bool constant) {
            try {
                Pair<string, bool> arrayType = TypeHelper.IsGetArrayType(type);
                _variableService.CreateNewVariable(null, variableName, arrayType.First, constant, arrayType.Second, false, initializationValue, _engineImportService);
                _variableService.AllocateVariableState(variableName, EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID, null, false);
                _statementVariableRef.AddConfiguredVariable(variableName);
            } catch (VariableExistsException e) {
                throw new ConfigurationException("Error creating variable: " + e.Message, e);
            } catch (VariableTypeException e) {
                throw new ConfigurationException("Error creating variable: " + e.Message, e);
            }
        }
    
        public void AddPlugInEventType(string eventTypeName, Uri[] resolutionURIs, object initializer) {
            if (initializer == null)
            {
                throw new ArgumentNullException("initializer");
            }

            if (!initializer.GetType().IsSerializable)
            {
                throw new ArgumentException("initializer is not serializable", "initializer");
            }

            try {
                _eventAdapterService.AddPlugInEventType(eventTypeName, resolutionURIs, initializer);
            } catch (EventAdapterException e) {
                throw new ConfigurationException("Error adding plug-in event type: " + e.Message, e);
            }
        }

        public IList<Uri> PlugInEventTypeResolutionURIs
        {
            get { return _engineSettingsService.PlugInEventTypeResolutionURIs; }
            set { _engineSettingsService.PlugInEventTypeResolutionURIs = value; }
        }

        public void AddRevisionEventType(string revisioneventTypeName, ConfigurationRevisionEventType revisionEventTypeConfig) {
            CheckTableExists(revisioneventTypeName);
            _valueAddEventService.AddRevisionEventType(revisioneventTypeName, revisionEventTypeConfig, _eventAdapterService);
        }
    
        public void AddVariantStream(string varianteventTypeName, ConfigurationVariantStream variantStreamConfig) {
            CheckTableExists(varianteventTypeName);
            _valueAddEventService.AddVariantStream(varianteventTypeName, variantStreamConfig, _eventAdapterService, _eventTypeIdGenerator);
        }
    
        public void UpdateMapEventType(string mapeventTypeName, IDictionary<string, Object> typeMap) {
            try {
                _eventAdapterService.UpdateMapEventType(mapeventTypeName, typeMap);
            } catch (EventAdapterException e) {
                throw new ConfigurationException("Error updating Map event type: " + e.Message, e);
            }
        }
    
        public void UpdateObjectArrayEventType(string objectArrayEventTypeName, string[] propertyNames, Object[] propertyTypes) {
            try {
                var typeMap = EventTypeUtility.ValidateObjectArrayDef(propertyNames, propertyTypes);
                _eventAdapterService.UpdateObjectArrayEventType(objectArrayEventTypeName, typeMap);
            } catch (EventAdapterException e) {
                throw new ConfigurationException("Error updating Object-array event type: " + e.Message, e);
            }
        }
    
        public void ReplaceXMLEventType(string xmlEventTypeName, ConfigurationEventTypeXMLDOM config) {
            SchemaModel schemaModel = null;
            if (config.SchemaResource != null || config.SchemaText != null) {
                try {
                    schemaModel = XSDSchemaMapper.LoadAndMap(
                        config.SchemaResource, 
                        config.SchemaText, 
                        _engineImportService,
                        _resourceManager);
                } catch (Exception ex) {
                    throw new ConfigurationException(ex.Message, ex);
                }
            }
    
            try {
                _eventAdapterService.ReplaceXMLEventType(xmlEventTypeName, config, schemaModel);
            } catch (EventAdapterException e) {
                throw new ConfigurationException("Error updating XML event type: " + e.Message, e);
            }
        }
    
        public void SetMetricsReportingInterval(string stmtGroupName, long newInterval) {
            try
            {
                _metricReportingService.SetMetricsReportingInterval(stmtGroupName, newInterval);
            } catch (Exception e) {
                throw new ConfigurationException("Error updating interval for metric reporting: " + e.Message, e);
            }
        }
    
    
        public void SetMetricsReportingStmtEnabled(string statementName) {
            try {
                _metricReportingService.SetMetricsReportingStmtEnabled(statementName);
            } catch (Exception e) {
                throw new ConfigurationException("Error enabling metric reporting for statement: " + e.Message, e);
            }
        }
    
        public void SetMetricsReportingStmtDisabled(string statementName) {
            try {
                _metricReportingService.SetMetricsReportingStmtDisabled(statementName);
            } catch (Exception e) {
                throw new ConfigurationException("Error enabling metric reporting for statement: " + e.Message, e);
            }
        }
    
        public void SetMetricsReportingEnabled() {
            try {
                _metricReportingService.SetMetricsReportingEnabled();
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Error enabling metric reporting: " + e.Message, e);
            }
        }
    
        public void SetMetricsReportingDisabled() {
            try {
                _metricReportingService.SetMetricsReportingDisabled();
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Error enabling metric reporting: " + e.Message, e);
            }
        }
    
        public bool IsVariantStreamExists(string name) {
            ValueAddEventProcessor processor = _valueAddEventService.GetValueAddProcessor(name);
            if (processor == null) {
                return false;
            }
            return processor.ValueAddEventType is VariantEventType;
        }
    
        public bool RemoveEventType(string eventTypeName, bool force) {
            if (!force) {
                var statements = _statementEventTypeRef.GetStatementNamesForType(eventTypeName);
                if ((statements != null) && (!statements.IsEmpty())) {
                    throw new ConfigurationException("Event type '" + eventTypeName + "' is in use by one or more statements");
                }
            }
    
            EventType type = _eventAdapterService.GetEventTypeByName(eventTypeName);
            if (type == null) {
                return false;
            }
    
            _eventAdapterService.RemoveType(eventTypeName);
            _statementEventTypeRef.RemoveReferencesType(eventTypeName);
            _filterService.RemoveType(type);
            return true;
        }
    
        public bool RemoveVariable(string name, bool force) {
            if (!force) {
                var statements = _statementVariableRef.GetStatementNamesForVar(name);
                if ((statements != null) && (!statements.IsEmpty())) {
                    throw new ConfigurationException("Variable '" + name + "' is in use by one or more statements");
                }
            }
    
            VariableReader reader = _variableService.GetReader(name, EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID);
            if (reader == null) {
                return false;
            }
    
            _variableService.RemoveVariableIfFound(name);
            _statementVariableRef.RemoveReferencesVariable(name);
            _statementVariableRef.RemoveConfiguredVariable(name);
            return true;
        }
    
        public ICollection<string> GetEventTypeNameUsedBy(string name) {
            var statements = _statementEventTypeRef.GetStatementNamesForType(name);
            if ((statements == null) || (statements.IsEmpty())) {
                return Collections.GetEmptySet<string>();
            }
            return statements.AsReadOnlyCollection();
        }
    
        public ICollection<string> GetVariableNameUsedBy(string variableName) {
            ICollection<string> statements = _statementVariableRef.GetStatementNamesForVar(variableName);
            if ((statements == null) || (statements.IsEmpty())) {
                return Collections.GetEmptySet<string>();
            }
            return statements.AsReadOnlyCollection();
        }
    
        public EventType GetEventType(string eventTypeName) {
            return _eventAdapterService.GetEventTypeByName(eventTypeName);
        }

        public ICollection<EventType> EventTypes
        {
            get { return _eventAdapterService.AllTypes; }
        }

        public void AddEventType<T>(ConfigurationEventTypeLegacy legacyEventTypeDesc)
        {
            AddEventType(typeof(T).Name, typeof(T).AssemblyQualifiedName, legacyEventTypeDesc);
        }

        public void AddEventType<T>(string eventTypeName, ConfigurationEventTypeLegacy legacyEventTypeDesc)
        {
            AddEventType(eventTypeName, typeof(T).AssemblyQualifiedName, legacyEventTypeDesc);
        }

        public void AddEventType(
            string eventTypeName,
            Type eventClass,
            ConfigurationEventTypeLegacy legacyEventTypeDesc)
        {
            AddEventType(eventTypeName, eventClass.AssemblyQualifiedName, legacyEventTypeDesc);
        }

        public void AddEventType(string eventTypeName, string eventClass, ConfigurationEventTypeLegacy legacyEventTypeDesc)
        {
            // To ensure proper usage, we have to convert the type to its assembly qualified name.
            try
            {
                eventClass = TypeHelper.ResolveType(eventClass, true).AssemblyQualifiedName;
            }
            catch (TypeLoadException ex)
            {
                throw new ConfigurationException("Failed to add legacy event type definition for type '" + eventTypeName + "': " + ex.Message, ex);
            }

            CheckTableExists(eventTypeName);
            try {
                var map = new Dictionary<string, ConfigurationEventTypeLegacy>();
                map.Put(eventClass, legacyEventTypeDesc);
                _eventAdapterService.TypeLegacyConfigs = map;
                _eventAdapterService.AddBeanType(eventTypeName, eventClass, false, false, false, true);
            } catch (EventAdapterException ex) {
                throw new ConfigurationException("Failed to add legacy event type definition for type '" + eventTypeName + "': " + ex.Message, ex);
            }
        }
    
        private void CheckTableExists(string eventTypeName) {
            try {
                EPLValidationUtil.ValidateTableExists(_tableService, eventTypeName);
            } catch (ExprValidationException ex) {
                throw new ConfigurationException(ex.Message, ex);
            }
        }

        public long PatternMaxSubexpressions
        {
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
            set
            {
                if (_matchRecognizeStatePoolEngineSvc != null)
                {
                    _matchRecognizeStatePoolEngineSvc.MatchRecognizeMaxStates = value;
                }
            }
        }

        public IDictionary<string, object> TransientConfiguration
        {
            get { return _transientConfiguration; }
        }

        public void AddEventTypeAvro(string eventTypeName, ConfigurationEventTypeAvro avro) {
            CheckTableExists(eventTypeName);
            try {
                _eventAdapterService.AddAvroType(eventTypeName, avro, false, true, true, false, false);
            } catch (EventAdapterException t) {
                throw new ConfigurationException(t.Message, t);
            }
        }
    }
} // end of namespace
