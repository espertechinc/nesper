///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.util.DOMExtensions;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    /// Parser for the common section of configuration.
    /// </summary>
    public class ConfigurationCommonParser
    {
        /// <summary>
        /// Configure the common section from a provided element
        /// </summary>
        /// <param name="common">common section</param>
        /// <param name="commonElement">element</param>
        public static void DoConfigure(
            ConfigurationCommon common,
            XmlElement commonElement)
        {
            var eventTypeNodeEnumerator = DOMElementEnumerator.Create(commonElement.ChildNodes);
            while (eventTypeNodeEnumerator.MoveNext()) {
                var element = eventTypeNodeEnumerator.Current;
                var nodeName = element.Name;
                switch (nodeName) {
                    case "event-type":
                        HandleEventTypes(common, element);
                        break;

                    case "auto-import":
                        HandleAutoImports(common, element);
                        break;

                    case "auto-import-annotations":
                        HandleAutoImportAnnotations(common, element);
                        break;

                    case "method-reference":
                        HandleMethodReference(common, element);
                        break;

                    case "database-reference":
                        HandleDatabaseRefs(common, element);
                        break;

                    case "variable":
                        HandleVariable(common, element);
                        break;

                    case "variant-stream":
                        HandleVariantStream(common, element);
                        break;

                    case "event-meta":
                        HandleEventMeta(common, element);
                        break;

                    case "logging":
                        HandleLogging(common, element);
                        break;

                    case "scripting":
                        HandleScripting(common, element);
                        break;

                    case "time-source":
                        HandleTimeSource(common, element);
                        break;

                    case "execution":
                        HandleExecution(common, element);
                        break;

                    case "event-type-auto-name":
                        HandleEventTypeAutoNames(common, element);
                        break;
                }
            }
        }

        private static void HandleEventTypeAutoNames(
            ConfigurationCommon configuration,
            XmlElement element)
        {
            var name = GetRequiredAttribute(element, "package-name");
            configuration.AddEventTypeAutoName(name);
        }

        private static void HandleExecution(
            ConfigurationCommon common,
            XmlElement parentElement)
        {
            var threadingProfileStr = GetOptionalAttribute(parentElement, "threading-profile");
            if (threadingProfileStr != null) {
                var profile = EnumHelper.Parse<ThreadingProfile>(threadingProfileStr);
                common.Execution.ThreadingProfile = profile;
            }
        }

        private static void HandleTimeSource(
            ConfigurationCommon common,
            XmlElement element)
        {
            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                var subElement = nodeEnumerator.Current;
                switch (subElement.Name) {
                    case "time-unit":
                        var valueText = GetRequiredAttribute(subElement, "value");
                        if (valueText == null) {
                            throw new ConfigurationException("No value attribute supplied for time-unit element");
                        }

                        try {
                            var timeUnit = EnumHelper.Parse<TimeUnit>(valueText);
                            common.TimeSource.TimeUnit = timeUnit;
                        }
                        catch (EPException) {
                            throw;
                        }
                        catch (Exception e) {
                            throw new ConfigurationException(
                                "Value attribute for time-unit element invalid: " + e.Message,
                                e);
                        }

                        break;
                }
            }
        }

        private static void HandleLogging(
            ConfigurationCommon common,
            XmlElement element)
        {
            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                var subElement = nodeEnumerator.Current;
                switch (subElement.Name) {
                    case "query-plan": {
                        var valueText = GetRequiredAttribute(subElement, "enabled");
                        var value = bool.Parse(valueText);
                        common.Logging.IsEnableQueryPlan = value;
                        break;
                    }

                    case "jdbc": {
                        var valueText = GetRequiredAttribute(subElement, "enabled");
                        var value = bool.Parse(valueText);
                        common.Logging.IsEnableADO = value;
                        break;
                    }
                }
            }
        }
        
        private static void HandleScripting(
            ConfigurationCommon common,
            XmlElement element)
        {
            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                var subElement = nodeEnumerator.Current;
                if (subElement.Name == "engine") {
                    var name = subElement.Attributes.GetNamedItem("type").InnerText;
                    common.Scripting.AddEngine(name);
                }
            }
        }

        private static void HandleVariantStream(
            ConfigurationCommon configuration,
            XmlElement element)
        {
            var variantStream = new ConfigurationCommonVariantStream();
            var varianceName = GetRequiredAttribute(element, "name");

            if (element.Attributes.GetNamedItem("type-variance") != null) {
                var typeVar = element.Attributes.GetNamedItem("type-variance").InnerText;
                TypeVariance typeVarianceEnum;
                try {
                    typeVarianceEnum = EnumHelper.Parse<TypeVariance>(typeVar.Trim());
                    variantStream.TypeVariance = typeVarianceEnum;
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception) {
                    throw new ConfigurationException(
                        "Invalid enumeration value for type-variance attribute '" + typeVar + "'");
                }
            }

            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                var subElement = nodeEnumerator.Current;
                if (subElement.Name == "variant-event-type") {
                    var name = subElement.Attributes.GetNamedItem("name").InnerText;
                    variantStream.AddEventTypeName(name);
                }
            }

            configuration.AddVariantStream(varianceName, variantStream);
        }

        private static void HandleDatabaseRefs(
            ConfigurationCommon configuration,
            XmlElement element)
        {
            var name = GetRequiredAttribute(element, "name");
            var configDBRef = new ConfigurationCommonDBRef();
            configuration.AddDatabaseReference(name, configDBRef);

            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                var subElement = nodeEnumerator.Current;
                switch (subElement.Name) {
                    case "datasource-connection": {
                        var lookup = GetRequiredAttribute(subElement, "context-lookup-name");
                        var properties = DOMUtil.GetProperties(subElement, "env-property");
                        configDBRef.SetDataSourceConnection(lookup, properties);
                        break;
                    }

                    case "datasourcefactory-connection": {
                        var className = GetRequiredAttribute(subElement, "class-name");
                        var properties = DOMUtil.GetProperties(subElement, "env-property");
                        configDBRef.SetDataSourceFactory(properties, className);
                        break;
                    }

                    case "driver": {
                        var driverType = GetRequiredAttribute(subElement, "type");
                        var connectionString = GetRequiredAttribute(subElement, "connection-string");
                        var properties = DOMUtil.GetProperties(subElement, "env-property");
                        configDBRef.SetDatabaseDriver(driverType, connectionString, properties);
                        break;
                    }

                    case "connection-lifecycle": {
                        var value = GetRequiredAttribute(subElement, "value");
                        configDBRef.ConnectionLifecycleEnum = EnumHelper.Parse<ConnectionLifecycleEnum>(value);
                        break;
                    }

                    case "connection-settings":
                        if (subElement.Attributes.GetNamedItem("auto-commit") != null) {
                            var autoCommit = subElement.Attributes.GetNamedItem("auto-commit").InnerText;
                            configDBRef.ConnectionAutoCommit = bool.Parse(autoCommit);
                        }

                        if (subElement.Attributes.GetNamedItem("transaction-isolation") != null) {
                            var transactionIsolation =
                                subElement.Attributes.GetNamedItem("transaction-isolation").InnerText;
                            configDBRef.ConnectionTransactionIsolation =
                                EnumHelper.Parse<IsolationLevel>(transactionIsolation);
                        }

                        if (subElement.Attributes.GetNamedItem("catalog") != null) {
                            var catalog = subElement.Attributes.GetNamedItem("catalog").InnerText;
                            configDBRef.ConnectionCatalog = catalog;
                        }

                        if (subElement.Attributes.GetNamedItem("read-only") != null) {
                            var readOnly = subElement.Attributes.GetNamedItem("read-only").InnerText;
                            configDBRef.ConnectionReadOnly = bool.Parse(readOnly);
                        }

                        break;

                    case "column-change-case": {
                        var value = GetRequiredAttribute(subElement, "value");
                        var parsed = EnumHelper.Parse<ColumnChangeCaseEnum>(value);
                        configDBRef.ColumnChangeCase = parsed;
                        break;
                    }

                    case "metadata-origin": {
                        var value = GetRequiredAttribute(subElement, "value");
                        var parsed = EnumHelper.Parse<MetadataOriginEnum>(value);
                        configDBRef.MetadataOrigin = parsed;
                        break;
                    }
#if NOT_SUPPORTED
                    // NOTE: How does this translate in a world based on ADO.NET
                    //        Does it translate at all?
                    case "sql-types-mapping":
                        var sqlType = GetRequiredAttribute(subElement, "sql-type");
                        var javaType = GetRequiredAttribute(subElement, "java-type");
                        int sqlTypeInt;
                        try {
                            sqlTypeInt = Int32.Parse(sqlType);
                        }
                        catch (FormatException ex) {
                            throw new ConfigurationException("Error converting sql type '" + sqlType + "' to integer SqlTypes constant");
                        }

                        configDBRef.AddSqlTypesBinding(sqlTypeInt, javaType);
                        break;
#endif
                    case "expiry-time-cache":
                        var maxAge = GetRequiredAttribute(subElement, "max-age-seconds");
                        var purgeInterval = GetRequiredAttribute(subElement, "purge-interval-seconds");
                        var refTypeEnum = CacheReferenceType.DEFAULT;
                        if (subElement.Attributes.GetNamedItem("ref-type") != null) {
                            var refType = subElement.Attributes.GetNamedItem("ref-type").InnerText;
                            refTypeEnum = EnumHelper.Parse<CacheReferenceType>(refType);
                        }

                        configDBRef.SetExpiryTimeCache(double.Parse(maxAge), double.Parse(purgeInterval), refTypeEnum);
                        break;

                    case "lru-cache":
                        var size = GetRequiredAttribute(subElement, "size");
                        configDBRef.SetLRUCache(int.Parse(size));
                        break;
                }
            }
        }

        private static void HandleVariable(
            ConfigurationCommon configuration,
            XmlElement element)
        {
            var variableName = GetRequiredAttribute(element, "name");
            var type = GetRequiredAttribute(element, "type");

            var variableType = TypeHelper.GetTypeForSimpleName(type, TypeResolverDefault.INSTANCE);
            if (variableType == null) {
                throw new ConfigurationException(
                    "Invalid variable type for variable '" + variableName + "', the type is not recognized");
            }

            var initValueNode = element.Attributes.GetNamedItem("initialization-value");
            string initValue = null;
            if (initValueNode != null) {
                initValue = initValueNode.InnerText;
            }

            var isConstant = false;
            if (GetOptionalAttribute(element, "constant") != null) {
                isConstant = bool.Parse(GetOptionalAttribute(element, "constant"));
            }

            configuration.AddVariable(variableName, variableType, initValue, isConstant);
        }

        private static void HandleEventTypes(
            ConfigurationCommon configuration,
            XmlElement element)
        {
            var name = GetRequiredAttribute(element, "name");

            var optionalClassName = GetOptionalAttribute(element, "class");
            if (optionalClassName != null) {
                configuration.AddEventType(name, optionalClassName);
            }

            HandleEventTypeDef(name, optionalClassName, configuration, element);
        }

        private static void HandleEventTypeDef(
            string name,
            string optionalClassName,
            ConfigurationCommon configuration,
            XmlNode parentNode)
        {
            var eventTypeNodeEnumerator = DOMElementEnumerator.Create(parentNode.ChildNodes);
            while (eventTypeNodeEnumerator.MoveNext()) {
                var eventTypeElement = eventTypeNodeEnumerator.Current;
                var nodeName = eventTypeElement.Name;
                switch (nodeName) {
                    case "xml-dom":
                        HandleXMLDOM(name, configuration, eventTypeElement);
                        break;

                    case "map":
                        HandleMap(name, configuration, eventTypeElement);
                        break;

                    case "objectarray":
                        HandleObjectArray(name, configuration, eventTypeElement);
                        break;

                    case "legacy-type":
                        HandleLegacy(name, optionalClassName, configuration, eventTypeElement);
                        break;

                    case "avro":
                        HandleAvro(name, configuration, eventTypeElement);
                        break;

                    default:
                        throw new ConfigurationException($"unknown eventType \"{nodeName}\"");
                }
            }
        }

        private static void HandleXMLDOM(
            string name,
            ConfigurationCommon configuration,
            XmlElement xmldomElement)
        {
            var rootElementName = GetRequiredAttribute(xmldomElement, "root-element-name");
            var rootElementNamespace = GetOptionalAttribute(xmldomElement, "root-element-namespace");
            var schemaResource = GetOptionalAttribute(xmldomElement, "schema-resource");
            var schemaText = GetOptionalAttribute(xmldomElement, "schema-text");
            var defaultNamespace = GetOptionalAttribute(xmldomElement, "default-namespace");
            var resolvePropertiesAbsoluteStr = GetOptionalAttribute(xmldomElement, "xpath-resolve-properties-absolute");
            var propertyExprXPathStr = GetOptionalAttribute(xmldomElement, "xpath-property-expr");
            var eventSenderChecksRootStr = GetOptionalAttribute(xmldomElement, "event-sender-validates-root");
            var xpathFunctionResolverClass = GetOptionalAttribute(xmldomElement, "xpath-function-resolver");
            var xpathVariableResolverClass = GetOptionalAttribute(xmldomElement, "xpath-variable-resolver");
            var autoFragmentStr = GetOptionalAttribute(xmldomElement, "auto-fragment");
            var startTimestampProperty = GetOptionalAttribute(xmldomElement, "start-timestamp-property-name");
            var endTimestampProperty = GetOptionalAttribute(xmldomElement, "end-timestamp-property-name");

            var xmlDOMEventTypeDesc = new ConfigurationCommonEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = rootElementName;
            xmlDOMEventTypeDesc.SchemaResource = schemaResource;
            xmlDOMEventTypeDesc.SchemaText = schemaText;
            xmlDOMEventTypeDesc.RootElementNamespace = rootElementNamespace;
            xmlDOMEventTypeDesc.DefaultNamespace = defaultNamespace;
            xmlDOMEventTypeDesc.XPathFunctionResolver = xpathFunctionResolverClass;
            xmlDOMEventTypeDesc.XPathVariableResolver = xpathVariableResolverClass;
            xmlDOMEventTypeDesc.StartTimestampPropertyName = startTimestampProperty;
            xmlDOMEventTypeDesc.EndTimestampPropertyName = endTimestampProperty;
            if (resolvePropertiesAbsoluteStr != null) {
                xmlDOMEventTypeDesc.IsXPathResolvePropertiesAbsolute = bool.Parse(resolvePropertiesAbsoluteStr);
            }

            if (propertyExprXPathStr != null) {
                xmlDOMEventTypeDesc.IsXPathPropertyExpr = bool.Parse(propertyExprXPathStr);
            }

            if (eventSenderChecksRootStr != null) {
                xmlDOMEventTypeDesc.IsEventSenderValidatesRoot = bool.Parse(eventSenderChecksRootStr);
            }

            if (autoFragmentStr != null) {
                xmlDOMEventTypeDesc.IsAutoFragment = bool.Parse(autoFragmentStr);
            }

            configuration.AddEventType(name, xmlDOMEventTypeDesc);

            var propertyNodeEnumerator = DOMElementEnumerator.Create(xmldomElement.ChildNodes);
            while (propertyNodeEnumerator.MoveNext()) {
                var propertyElement = propertyNodeEnumerator.Current;
                switch (propertyElement.Name) {
                    case "namespace-prefix":
                        var prefix = GetRequiredAttribute(propertyElement, "prefix");
                        var @namespace = GetRequiredAttribute(propertyElement, "namespace");
                        xmlDOMEventTypeDesc.AddNamespacePrefix(prefix, @namespace);
                        break;

                    case "xpath-property":
                        var propertyName = GetRequiredAttribute(propertyElement, "property-name");
                        var xPath = GetRequiredAttribute(propertyElement, "xpath");
                        var propertyType = GetRequiredAttribute(propertyElement, "type");
                        XPathResultType xpathConstantType;
                        var propertyTypeInvariant = propertyType.ToUpperInvariant();
                        switch (propertyTypeInvariant) {
                            case "NUMBER":
                                xpathConstantType = XPathResultType.Number;
                                break;

                            case "STRING":
                                xpathConstantType = XPathResultType.String;
                                break;

                            case "BOOLEAN":
                                xpathConstantType = XPathResultType.Boolean;
                                break;

                            case "NODE":
                                xpathConstantType = XPathResultType.Any;
                                break;

                            case "NODESET":
                                xpathConstantType = XPathResultType.NodeSet;
                                break;

                            default:
                                throw new ArgumentException(
                                    "Invalid xpath property type for property '" +
                                    propertyName +
                                    "' and type '" +
                                    propertyType +
                                    '\'');
                        }

                        string castToClass = null;
                        if (propertyElement.Attributes.GetNamedItem("cast") != null) {
                            castToClass = propertyElement.Attributes.GetNamedItem("cast").InnerText;
                        }

                        string optionaleventTypeName = null;
                        if (propertyElement.Attributes.GetNamedItem("event-type-name") != null) {
                            optionaleventTypeName =
                                propertyElement.Attributes.GetNamedItem("event-type-name").InnerText;
                        }

                        if (optionaleventTypeName != null) {
                            xmlDOMEventTypeDesc.AddXPathPropertyFragment(
                                propertyName,
                                xPath,
                                xpathConstantType,
                                optionaleventTypeName);
                        }
                        else {
                            xmlDOMEventTypeDesc.AddXPathProperty(propertyName, xPath, xpathConstantType, castToClass);
                        }

                        break;
                }
            }
        }

        private static void HandleAvro(
            string name,
            ConfigurationCommon configuration,
            XmlElement element)
        {
            var schemaText = GetOptionalAttribute(element, "schema-text");

            var avroEventTypeDesc = new ConfigurationCommonEventTypeAvro();
            avroEventTypeDesc.AvroSchemaText = schemaText;
            configuration.AddEventTypeAvro(name, avroEventTypeDesc);

            avroEventTypeDesc.StartTimestampPropertyName =
                GetOptionalAttribute(element, "start-timestamp-property-name");
            avroEventTypeDesc.EndTimestampPropertyName = GetOptionalAttribute(element, "end-timestamp-property-name");

            var names = GetOptionalAttribute(element, "supertype-names");
            if (names != null) {
                var split = names.SplitCsv();
                for (var i = 0; i < split.Length; i++) {
                    avroEventTypeDesc.SuperTypes.Add(split[i].Trim());
                }
            }
        }

        private static void HandleLegacy(
            string name,
            string className,
            ConfigurationCommon configuration,
            XmlElement xmldomElement)
        {
            // Class name is required for legacy classes
            if (className == null) {
                throw new ConfigurationException("Required class name not supplied for legacy type definition");
            }

            var accessorStyle = GetRequiredAttribute(xmldomElement, "accessor-style");
            var propertyResolution = GetRequiredAttribute(xmldomElement, "property-resolution-style");
            var factoryMethod = GetOptionalAttribute(xmldomElement, "factory-method");
            var copyMethod = GetOptionalAttribute(xmldomElement, "copy-method");
            var startTimestampProp = GetOptionalAttribute(xmldomElement, "start-timestamp-property-name");
            var endTimestampProp = GetOptionalAttribute(xmldomElement, "end-timestamp-property-name");

            var legacyDesc = new ConfigurationCommonEventTypeBean();
            if (accessorStyle != null) {
                legacyDesc.AccessorStyle = EnumHelper.Parse<AccessorStyle>(accessorStyle);
            }

            if (propertyResolution != null) {
                legacyDesc.PropertyResolutionStyle = EnumHelper.Parse<PropertyResolutionStyle>(propertyResolution);
            }

            legacyDesc.FactoryMethod = factoryMethod;
            legacyDesc.CopyMethod = copyMethod;
            legacyDesc.StartTimestampPropertyName = startTimestampProp;
            legacyDesc.EndTimestampPropertyName = endTimestampProp;
            configuration.AddEventType(name, className, legacyDesc);

            var propertyNodeEnumerator = DOMElementEnumerator.Create(xmldomElement.ChildNodes);
            while (propertyNodeEnumerator.MoveNext()) {
                var propertyElement = propertyNodeEnumerator.Current;
                switch (propertyElement.Name) {
                    case "method-property": {
                        var nameProperty = GetRequiredAttribute(propertyElement, "name");
                        var method = GetRequiredAttribute(propertyElement, "accessor-method");
                        legacyDesc.AddMethodProperty(nameProperty, method);
                        break;
                    }

                    case "field-property": {
                        var nameProperty = GetRequiredAttribute(propertyElement, "name");
                        var field = GetRequiredAttribute(propertyElement, "accessor-field");
                        legacyDesc.AddFieldProperty(nameProperty, field);
                        break;
                    }

                    default:
                        throw new ConfigurationException(
                            "Invalid node " +
                            propertyElement.Name +
                            " encountered while parsing legacy type definition");
                }
            }
        }

        private static void HandleMap(
            string name,
            ConfigurationCommon configuration,
            XmlElement eventTypeElement)
        {
            ConfigurationCommonEventTypeMap config;
            var startTimestampProp = GetOptionalAttribute(eventTypeElement, "start-timestamp-property-name");
            var endTimestampProp = GetOptionalAttribute(eventTypeElement, "end-timestamp-property-name");
            var superTypesList = eventTypeElement.Attributes.GetNamedItem("supertype-names");
            if (superTypesList != null || startTimestampProp != null || endTimestampProp != null) {
                config = new ConfigurationCommonEventTypeMap();
                if (superTypesList != null) {
                    var value = superTypesList.InnerText;
                    var names = value.SplitCsv();
                    foreach (var superTypeName in names) {
                        config.SuperTypes.Add(superTypeName.Trim());
                    }
                }

                config.EndTimestampPropertyName = endTimestampProp;
                config.StartTimestampPropertyName = startTimestampProp;
                configuration.AddMapConfiguration(name, config);
            }

            var propertyTypeNames = new Properties();
            var propertyList = eventTypeElement.GetElementsByTagName("map-property");
            for (var i = 0; i < propertyList.Count; i++) {
                var nameProperty = GetRequiredAttribute(propertyList.Item(i), "name");
                var clazz = GetRequiredAttribute(propertyList.Item(i), "class");
                propertyTypeNames.Put(nameProperty, clazz);
            }

            configuration.AddEventType(name, propertyTypeNames);
        }

        private static void HandleObjectArray(
            string name,
            ConfigurationCommon configuration,
            XmlElement eventTypeElement)
        {
            ConfigurationCommonEventTypeObjectArray config;
            var startTimestampProp = GetOptionalAttribute(eventTypeElement, "start-timestamp-property-name");
            var endTimestampProp = GetOptionalAttribute(eventTypeElement, "end-timestamp-property-name");
            var superTypesList = eventTypeElement.Attributes.GetNamedItem("supertype-names");
            if (superTypesList != null || startTimestampProp != null || endTimestampProp != null) {
                config = new ConfigurationCommonEventTypeObjectArray();
                if (superTypesList != null) {
                    var value = superTypesList.InnerText;
                    var names = value.SplitCsv();
                    foreach (var superTypeName in names) {
                        config.SuperTypes.Add(superTypeName.Trim());
                    }
                }

                config.EndTimestampPropertyName = endTimestampProp;
                config.StartTimestampPropertyName = startTimestampProp;
                configuration.AddObjectArrayConfiguration(name, config);
            }

            IList<string> propertyNames = new List<string>();
            IList<object> propertyTypes = new List<object>();
            var propertyList = eventTypeElement.GetElementsByTagName("objectarray-property");
            for (var i = 0; i < propertyList.Count; i++) {
                var nameProperty = GetRequiredAttribute(propertyList.Item(i), "name");
                var clazz = GetRequiredAttribute(propertyList.Item(i), "class");
                propertyNames.Add(nameProperty);
                propertyTypes.Add(clazz);
            }

            configuration.AddEventType(name, propertyNames.ToArray(), propertyTypes.ToArray());
        }

        private static void HandleAutoImports(
            ConfigurationCommon configuration,
            XmlElement element)
        {
            var assembly = GetOptionalAttribute(element, "assembly");
            var @namespace = GetOptionalAttribute(element, "import-namespace");
            if (@namespace != null) {
                configuration.AddImportNamespace(@namespace, assembly);
                return;
            }

            var type = GetOptionalAttribute(element, "import-type");
            if (type != null) {
                configuration.AddImportType(type, assembly);
                return;
            }

            throw new ConfigurationException("Auto-import requires a namespace or a type");
        }

        private static void HandleAutoImportAnnotations(
            ConfigurationCommon configuration,
            XmlElement element)
        {
            var assembly = GetOptionalAttribute(element, "assembly");
            var @namespace = GetOptionalAttribute(element, "import-namespace");
            if (@namespace != null)
            {
                configuration.AddAnnotationImportNamespace(@namespace, assembly);
                return;
            }

            var type = GetOptionalAttribute(element, "import-type");
            if (type != null)
            {
                configuration.AddAnnotationImportType(type, assembly);
                return;
            }

            throw new ConfigurationException("Annotation-import requires a namespace or a type");
        }

        private static void HandleMethodReference(
            ConfigurationCommon configuration,
            XmlElement element)
        {
            var className = GetRequiredAttribute(element, "class-name");
            var configMethodRef = new ConfigurationCommonMethodRef();
            configuration.AddMethodRef(className, configMethodRef);

            var nodeEnumerator = DOMElementEnumerator.Create(element.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                var subElement = nodeEnumerator.Current;
                switch (subElement.Name) {
                    case "expiry-time-cache":
                        var maxAge = GetRequiredAttribute(subElement, "max-age-seconds");
                        var purgeInterval = GetRequiredAttribute(subElement, "purge-interval-seconds");
                        var refTypeEnum = CacheReferenceType.DEFAULT;
                        if (subElement.Attributes.GetNamedItem("ref-type") != null) {
                            var refType = subElement.Attributes.GetNamedItem("ref-type").InnerText;
                            refTypeEnum = EnumHelper.Parse<CacheReferenceType>(refType);
                        }

                        configMethodRef.SetExpiryTimeCache(
                            double.Parse(maxAge),
                            double.Parse(purgeInterval),
                            refTypeEnum);
                        break;

                    case "lru-cache":
                        var size = GetRequiredAttribute(subElement, "size");
                        configMethodRef.SetLRUCache(int.Parse(size));
                        break;
                }
            }
        }

        private static void HandleEventMeta(
            ConfigurationCommon common,
            XmlElement parentElement)
        {
            var nodeEnumerator = DOMElementEnumerator.Create(parentElement.ChildNodes);
            while (nodeEnumerator.MoveNext()) {
                var subElement = nodeEnumerator.Current;
                switch (subElement.Name) {
                    case "class-property-resolution":
                        var styleNode = subElement.Attributes.GetNamedItem("style");
                        if (styleNode != null) {
                            var styleText = styleNode.InnerText;
                            var value = EnumHelper.Parse<PropertyResolutionStyle>(styleText);
                            common.EventMeta.ClassPropertyResolutionStyle = value;
                        }

                        var accessorStyleNode = subElement.Attributes.GetNamedItem("accessor-style");
                        if (accessorStyleNode != null) {
                            var accessorStyleText = accessorStyleNode.InnerText;
                            var value = EnumHelper.Parse<AccessorStyle>(accessorStyleText);
                            common.EventMeta.DefaultAccessorStyle = value;
                        }

                        break;

                    case "event-representation":
                        var typeNode = subElement.Attributes.GetNamedItem("type");
                        if (typeNode != null) {
                            var typeText = typeNode.InnerText;
                            var value = EnumHelper.Parse<EventUnderlyingType>(typeText);
                            common.EventMeta.DefaultEventRepresentation = value;
                        }

                        break;

                    case "avro-settings":
                        var enableAvroStr = GetOptionalAttribute(subElement, "enable-avro");
                        if (enableAvroStr != null) {
                            common.EventMeta.AvroSettings.IsEnableAvro = bool.Parse(enableAvroStr);
                        }

                        var enableNativeStringStr = GetOptionalAttribute(subElement, "enable-native-string");
                        if (enableNativeStringStr != null) {
                            common.EventMeta.AvroSettings.IsEnableNativeString = bool.Parse(enableNativeStringStr);
                        }

                        var enableSchemaDefaultNonNullStr = GetOptionalAttribute(
                            subElement,
                            "enable-schema-default-nonnull");
                        if (enableSchemaDefaultNonNullStr != null) {
                            common.EventMeta.AvroSettings.IsEnableSchemaDefaultNonNull =
                                bool.Parse(enableSchemaDefaultNonNullStr);
                        }

                        var objectvalueTypewidenerFactoryClass = GetOptionalAttribute(
                            subElement,
                            "objectvalue-typewidener-factory-class");
                        if (objectvalueTypewidenerFactoryClass != null &&
                            objectvalueTypewidenerFactoryClass.Trim().Length > 0) {
                            common.EventMeta.AvroSettings.ObjectValueTypeWidenerFactoryClass =
                                objectvalueTypewidenerFactoryClass.Trim();
                        }

                        var typeRepresentationMapperClass = GetOptionalAttribute(
                            subElement,
                            "type-representation-mapper-class");
                        common.EventMeta.AvroSettings.TypeRepresentationMapperClass = typeRepresentationMapperClass;
                        break;
                }
            }
        }
    }
} // end of namespace