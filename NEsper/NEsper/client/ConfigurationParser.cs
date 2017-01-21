///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using com.espertech.esper.client.soda;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.client
{
    using Stream = System.IO.Stream;

    /// <summary>
    /// Parser for configuration XML.
    /// </summary>
    public class ConfigurationParser
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly XslCompiledTransform InitializerTransform;

        /// <summary>
        /// Initializes the <see cref="ConfigurationParser"/> class.
        /// </summary>
        static ConfigurationParser()
        {
            var transformDocument = new XmlDocument();
            using (
                var transformDocumentStream =
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(
                        "com.espertech.esper.client.InitializerTransform.xslt"))
            {
                transformDocument.Load(transformDocumentStream);
            }

            InitializerTransform = new XslCompiledTransform(false);
            InitializerTransform.Load(new XmlNodeReader(transformDocument));
        }

        /// <summary>
        /// Use the configuration specified in the given input stream.
        /// </summary>
        /// <param name="configuration">is the configuration object to populate</param>
        /// <param name="stream">The stream.</param>
        /// <param name="resourceName">The name to use in warning/error messages</param>
        /// <throws>  com.espertech.esper.client.EPException </throws>
        public static void DoConfigure(Configuration configuration, Stream stream, String resourceName)
        {
            var document = GetDocument(stream, resourceName);
            DoConfigure(configuration, document);
        }

        public static XmlDocument GetDocument(Stream stream, String resourceName)
        {
            XmlDocument document;

            try
            {
                document = new XmlDocument();
                document.Load(stream);
            }
            catch (XmlException ex)
            {
                throw new EPException("Could not parse configuration: " + resourceName, ex);
            }
            catch (IOException ex)
            {
                throw new EPException("Could not read configuration: " + resourceName, ex);
            }
            finally
            {
                try
                {
                    stream.Close();
                }
                catch (IOException ioe)
                {
                    Log.Warn("could not close input stream for: " + resourceName, ioe);
                }
            }

            return document;
        }

        /// <summary>
        /// Parse the W3C DOM document.
        /// </summary>
        /// <param name="configuration">is the configuration object to populate</param>
        /// <param name="doc">to parse</param>
        /// <throws>  com.espertech.esper.client.EPException </throws>
        public static void DoConfigure(Configuration configuration, XmlDocument doc)
        {
            var rootElement = doc.DocumentElement;
            DoConfigure(configuration, rootElement);
        }

        /// <summary>
        /// Parse the configuration starting at the specified root node.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="rootNode">The root node.</param>
        public static void DoConfigure(Configuration configuration, XmlNode rootNode)
        {
            foreach (var element in CreateElementEnumerable(rootNode.ChildNodes))
            {
                var nodeName = element.Name;
                switch (nodeName)
                {
                    case "event-type-auto-name":
                        HandleEventTypeAutoNames(configuration, element);
                        break;
                    case "event-type":
                        HandleEventTypes(configuration, element);
                        break;
                    case "auto-import":
                        HandleAutoImports(configuration, element);
                        break;
                    case "auto-import-annotations":
                        HandleAutoImportAnnotations(configuration, element);
                        break;
                    case "method-reference":
                        HandleMethodReference(configuration, element);
                        break;
                    case "database-reference":
                        HandleDatabaseRefs(configuration, element);
                        break;
                    case "plugin-view":
                        HandlePlugInView(configuration, element);
                        break;
                    case "plugin-virtualdw":
                        HandlePlugInVirtualDW(configuration, element);
                        break;
                    case "plugin-aggregation-function":
                        HandlePlugInAggregation(configuration, element);
                        break;
                    case "plugin-aggregation-multifunction":
                        HandlePlugInMultiFunctionAggregation(configuration, element);
                        break;
                    case "plugin-singlerow-function":
                        HandlePlugInSingleRow(configuration, element);
                        break;
                    case "plugin-pattern-guard":
                        HandlePlugInPatternGuard(configuration, element);
                        break;
                    case "plugin-pattern-observer":
                        HandlePlugInPatternObserver(configuration, element);
                        break;
                    case "variable":
                        HandleVariable(configuration, element);
                        break;
                    case "plugin-loader":
                        HandlePluginLoaders(configuration, element);
                        break;
                    case "engine-settings":
                        HandleEngineSettings(configuration, element);
                        break;
                    case "plugin-event-representation":
                        HandlePlugInEventRepresentation(configuration, element);
                        break;
                    case "plugin-event-type":
                        HandlePlugInEventType(configuration, element);
                        break;
                    case "plugin-event-type-name-resolution":
                        HandlePlugInEventTypeNameResolution(configuration, element);
                        break;
                    case "revision-event-type":
                        HandleRevisionEventType(configuration, element);
                        break;
                    case "variant-stream":
                        HandleVariantStream(configuration, element);
                        break;
                }
            }
        }

        private static void HandleEventTypeAutoNames(Configuration configuration, XmlElement element)
        {
            var name = GetRequiredAttribute(element, "package-name");
            configuration.AddEventTypeAutoName(name);
        }


        private static void HandleEventTypes(Configuration configuration, XmlElement element)
        {
            var name = GetRequiredAttribute(element, "name");
            var classNode = element.Attributes.GetNamedItem("class");

            String optionalClassName = null;
            if (classNode != null)
            {
                optionalClassName = classNode.InnerText;
                configuration.AddEventType(name, optionalClassName);
            }

            HandleEventTypeDef(name, optionalClassName, configuration, element);
        }

        private static void HandleEventTypeDef(String name, String optionalClassName, Configuration configuration,
                                               XmlNode parentNode)
        {
            foreach (var eventTypeElement in CreateElementEnumerable(parentNode.ChildNodes))
            {
                var nodeName = eventTypeElement.Name;
                switch (nodeName)
                {
                    case "xml-dom":
                        HandleXMLDOM(name, configuration, eventTypeElement);
                        break;
                    case "map":
                        HandleMap(name, configuration, eventTypeElement);
                        break;
                    case "legacy-type":
                        HandleLegacy(name, optionalClassName, configuration, eventTypeElement);
                        break;
                    case "objectarray":
                        HandleObjectArray(name, configuration, eventTypeElement);
                        break;
                }
            }
        }

        private static void HandleMap(String name, Configuration configuration, XmlElement eventTypeElement)
        {
            ConfigurationEventTypeMap config;
            var startTimestampProp = GetOptionalAttribute(eventTypeElement, "start-timestamp-property-name");
            var endTimestampProp = GetOptionalAttribute(eventTypeElement, "end-timestamp-property-name");
            var superTypesList = eventTypeElement.Attributes.GetNamedItem("supertype-names");
            if (superTypesList != null || startTimestampProp != null || endTimestampProp != null)
            {
                config = new ConfigurationEventTypeMap();
                if (superTypesList != null)
                {
                    var value = superTypesList.InnerText;
                    var names = value.Split(',');
                    foreach (var superTypeName in names)
                    {
                        config.SuperTypes.Add(superTypeName.Trim());
                    }
                }
                config.EndTimestampPropertyName = endTimestampProp;
                config.StartTimestampPropertyName = startTimestampProp;
                configuration.AddMapConfiguration(name, config);
            }

            var propertyTypeNames = new Properties();
            var propertyList = eventTypeElement.GetElementsByTagName("map-property");
            foreach (XmlNode propertyNode in propertyList)
            {
                var nameProperty = GetRequiredAttribute(propertyNode, "name");
                var type = GetRequiredAttribute(propertyNode, "class");
                propertyTypeNames[nameProperty] = type;
            }
            configuration.AddEventType(name, propertyTypeNames);
        }

        private static void HandleObjectArray(String name, Configuration configuration, XmlElement eventTypeElement)
        {
            ConfigurationEventTypeObjectArray config;
            var startTimestampProp = GetOptionalAttribute(eventTypeElement, "start-timestamp-property-name");
            var endTimestampProp = GetOptionalAttribute(eventTypeElement, "end-timestamp-property-name");
            var superTypesList = eventTypeElement.Attributes.GetNamedItem("supertype-names");
            if (superTypesList != null || startTimestampProp != null || endTimestampProp != null)
            {
                config = new ConfigurationEventTypeObjectArray();
                if (superTypesList != null)
                {
                    var value = superTypesList.InnerText;
                    var names = value.Split(',');
                    foreach (var superTypeName in names)
                    {
                        config.SuperTypes.Add(superTypeName.Trim());
                    }
                }
                config.EndTimestampPropertyName = endTimestampProp;
                config.StartTimestampPropertyName = startTimestampProp;
                configuration.AddObjectArrayConfiguration(name, config);
            }

            var propertyNames = new List<String>();
            var propertyTypes = new List<Object>();
            var propertyList = eventTypeElement.GetElementsByTagName("objectarray-property");
            for (var i = 0; i < propertyList.Count; i++)
            {
                var nameProperty = GetRequiredAttribute(propertyList.Item(i), "name");
                var clazz = GetRequiredAttribute(propertyList.Item(i), "class");
                propertyNames.Add(nameProperty);
                propertyTypes.Add(clazz);
            }
            configuration.AddEventType(name, propertyNames.ToArray(), propertyTypes.ToArray());
        }

        private static void HandleXMLDOM(String name, Configuration configuration, XmlElement xmldomElement)
        {
            var rootElementName = GetRequiredAttribute(xmldomElement, "root-element-name");
            var rootElementNamespace = GetOptionalAttribute(xmldomElement, "root-element-namespace");
            var schemaResource = GetOptionalAttribute(xmldomElement, "schema-resource");
            var schemaText = GetOptionalAttribute(xmldomElement, "schema-text");
            var defaultNamespace = GetOptionalAttribute(xmldomElement, "default-namespace");
            var resolvePropertiesAbsoluteStr = GetOptionalAttribute(xmldomElement,
                                                                       "xpath-resolve-properties-absolute");
            var propertyExprXPathStr = GetOptionalAttribute(xmldomElement, "xpath-property-expr");
            var eventSenderChecksRootStr = GetOptionalAttribute(xmldomElement, "event-sender-validates-root");
            var xpathFunctionResolverClass = GetOptionalAttribute(xmldomElement, "xpath-function-resolver");
            var xpathVariableResolverClass = GetOptionalAttribute(xmldomElement, "xpath-variable-resolver");
            var autoFragmentStr = GetOptionalAttribute(xmldomElement, "auto-fragment");
            var startTimestampProperty = GetOptionalAttribute(xmldomElement, "start-timestamp-property-name");
            var endTimestampProperty = GetOptionalAttribute(xmldomElement, "end-timestamp-property-name");

            var xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = rootElementName;
            xmlDOMEventTypeDesc.SchemaResource = schemaResource;
            xmlDOMEventTypeDesc.SchemaText = schemaText;
            xmlDOMEventTypeDesc.RootElementNamespace = rootElementNamespace;
            xmlDOMEventTypeDesc.DefaultNamespace = defaultNamespace;
            xmlDOMEventTypeDesc.XPathFunctionResolver = xpathFunctionResolverClass;
            xmlDOMEventTypeDesc.XPathVariableResolver = xpathVariableResolverClass;
            xmlDOMEventTypeDesc.StartTimestampPropertyName = startTimestampProperty;
            xmlDOMEventTypeDesc.EndTimestampPropertyName = endTimestampProperty;

            if (resolvePropertiesAbsoluteStr != null)
            {
                xmlDOMEventTypeDesc.IsXPathResolvePropertiesAbsolute = Boolean.Parse(resolvePropertiesAbsoluteStr);
            }

            if (propertyExprXPathStr != null)
            {
                xmlDOMEventTypeDesc.IsXPathPropertyExpr = Boolean.Parse(propertyExprXPathStr);
            }
            if (eventSenderChecksRootStr != null)
            {
                xmlDOMEventTypeDesc.IsEventSenderValidatesRoot = Boolean.Parse(eventSenderChecksRootStr);
            }
            if (autoFragmentStr != null)
            {
                xmlDOMEventTypeDesc.IsAutoFragment = Boolean.Parse(autoFragmentStr);
            }

            configuration.AddEventType(name, xmlDOMEventTypeDesc);

            foreach (var propertyElement in CreateElementEnumerable(xmldomElement.ChildNodes))
            {
                if (propertyElement.Name.Equals("namespace-prefix"))
                {
                    var prefix = GetRequiredAttribute(propertyElement, "prefix");
                    var namespace_ = GetRequiredAttribute(propertyElement, "namespace");
                    xmlDOMEventTypeDesc.AddNamespacePrefix(prefix, namespace_);
                }
                if (propertyElement.Name.Equals("xpath-property"))
                {
                    var propertyName = GetRequiredAttribute(propertyElement, "property-name");
                    var xPath = GetRequiredAttribute(propertyElement, "xpath");
                    var propertyType = GetRequiredAttribute(propertyElement, "type");

                    XPathResultType xpathConstantType;
                    switch (propertyType.ToUpperInvariant())
                    {
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
                        case "NODESET":
                            xpathConstantType = XPathResultType.NodeSet;
                            break;
                        default:
                            throw new ArgumentException("Invalid xpath property type for property '" + propertyName +
                                                        "' and type '" + propertyType + "'");
                    }

                    String castToClass = null;
                    if (propertyElement.Attributes.GetNamedItem("cast") != null)
                    {
                        castToClass = GetRequiredAttribute(propertyElement, "cast");
                    }

                    String optionalEventTypeName = null;
                    if (propertyElement.Attributes.GetNamedItem("event-type-name") != null)
                    {
                        optionalEventTypeName = GetRequiredAttribute(propertyElement, "event-type-name");
                    }

                    if (optionalEventTypeName != null)
                    {
                        xmlDOMEventTypeDesc.AddXPathPropertyFragment(propertyName, xPath, xpathConstantType,
                                                                     optionalEventTypeName);
                    }
                    else
                    {
                        xmlDOMEventTypeDesc.AddXPathProperty(propertyName, xPath, xpathConstantType, castToClass);
                    }
                }
            }
        }

        private static void HandleLegacy(String name, String className, Configuration configuration,
                                         XmlElement xmldomElement)
        {
            // Class name is required for legacy classes
            if (className == null)
            {
                throw new ConfigurationException("Required class name not supplied for legacy type definition");
            }

            var accessorStyle = GetRequiredAttribute(xmldomElement, "accessor-style");
            var codeGeneration = GetRequiredAttribute(xmldomElement, "code-generation");
            var propertyResolution = GetRequiredAttribute(xmldomElement, "property-resolution-style");
            var factoryMethod = GetOptionalAttribute(xmldomElement, "factory-method");
            var copyMethod = GetOptionalAttribute(xmldomElement, "copy-method");
            var startTimestampProp = GetOptionalAttribute(xmldomElement, "start-timestamp-property-name");
            var endTimestampProp = GetOptionalAttribute(xmldomElement, "end-timestamp-property-name");

            var legacyDesc = new ConfigurationEventTypeLegacy();
            if (accessorStyle != null)
            {
                legacyDesc.AccessorStyle =
                    EnumHelper.Parse<AccessorStyleEnum>(accessorStyle);
            }
            if (codeGeneration != null)
            {
                legacyDesc.CodeGeneration =
                    EnumHelper.Parse<CodeGenerationEnum>(codeGeneration);
            }
            if (propertyResolution != null)
            {
                legacyDesc.PropertyResolutionStyle = EnumHelper.Parse<PropertyResolutionStyle>(propertyResolution);
            }

            legacyDesc.FactoryMethod = factoryMethod;
            legacyDesc.CopyMethod = copyMethod;
            legacyDesc.StartTimestampPropertyName = startTimestampProp;
            legacyDesc.EndTimestampPropertyName = endTimestampProp;

            configuration.AddEventType(name, className, legacyDesc);

            foreach (var propertyElement in CreateElementEnumerable(xmldomElement.ChildNodes))
            {
                switch (propertyElement.Name)
                {
                    case "method-property":
                        {
                            var nameProperty = GetRequiredAttribute(propertyElement, "name");
                            var method = GetRequiredAttribute(propertyElement, "accessor-method");
                            legacyDesc.AddMethodProperty(nameProperty, method);
                            break;
                        }
                    case "field-property":
                        {
                            var nameProperty = GetRequiredAttribute(propertyElement, "name");
                            var field = GetRequiredAttribute(propertyElement, "accessor-field");
                            legacyDesc.AddFieldProperty(nameProperty, field);
                            break;
                        }
                    default:
                        throw new ConfigurationException("Invalid node " + propertyElement.Name +
                                                         " encountered while parsing legacy type definition");
                }
            }
        }

        private static void HandleAutoImports(Configuration configuration, XmlElement element)
        {
            var name = GetRequiredAttribute(element, "import-name");
            var assembly = GetOptionalAttribute(element, "assembly");
            if (String.IsNullOrEmpty(assembly))
            {
                configuration.AddImport(name);
            }
            else
            {
                configuration.AddImport(name, assembly);
            }
        }

        private static void HandleAutoImportAnnotations(Configuration configuration, XmlElement element)
        {
            var name = GetRequiredAttribute(element, "import-name");
            configuration.AddAnnotationImport(name);
        }

        private static void HandleDatabaseRefs(Configuration configuration, XmlElement element)
        {
            var name = GetRequiredAttribute(element, "name");
            var configDBRef = new ConfigurationDBRef();
            configuration.AddDatabaseReference(name, configDBRef);

            foreach (var subElement in CreateElementEnumerable(element.ChildNodes))
            {
                switch (subElement.Name)
                {
                    case "driver":
                        {
                            var properties = CreatePropertiesFromAttributes(subElement.Attributes);
                            var driverName = subElement.Attributes.GetNamedItem("type").Value;
                            configDBRef.SetDatabaseDriver(driverName, properties);
                            break;
                        }
                    case "connection-lifecycle":
                        {
                            var value = GetRequiredAttribute(subElement, "value");
                            configDBRef.ConnectionLifecycle =
                                EnumHelper.Parse<ConnectionLifecycleEnum>(value);
                            break;
                        }
                    case "connection-settings":
                        {
                            if (subElement.Attributes.GetNamedItem("auto-commit") != null)
                            {
                                var autoCommit = GetRequiredAttribute(subElement, "auto-commit");
                                configDBRef.ConnectionAutoCommit = Boolean.Parse(autoCommit);
                            }
                            if (subElement.Attributes.GetNamedItem("transaction-isolation") != null)
                            {
                                var transactionIsolation =
                                    GetRequiredAttribute(subElement, "transaction-isolation");
                                configDBRef.ConnectionTransactionIsolation =
                                    EnumHelper.Parse<IsolationLevel>(transactionIsolation);
                            }
                            if (subElement.Attributes.GetNamedItem("catalog") != null)
                            {
                                var catalog = GetRequiredAttribute(subElement, "catalog");
                                configDBRef.ConnectionCatalog = catalog;
                            }
#if NOT_SUPPORTED_IN_DOTNET
                            if (subElement.Attributes.GetNamedItem("read-only") != null)
                            {
                                String readOnly = GetRequiredAttribute(subElement, "read-only");
                                configDBRef.ConnectionReadOnly = Boolean.Parse(readOnly);
                            }
#endif
                            break;
                        }
                    case "column-change-case":
                        {
                            var value = GetRequiredAttribute(subElement, "value");
                            var parsed =
                                EnumHelper.Parse<ConfigurationDBRef.ColumnChangeCaseEnum>(value);
                            configDBRef.ColumnChangeCase = parsed;
                            break;
                        }
                    case "metadata-origin":
                        {
                            var value = GetRequiredAttribute(subElement, "value");
                            var parsed =
                                EnumHelper.Parse<ConfigurationDBRef.MetadataOriginEnum>(value);
                            configDBRef.MetadataOrigin = parsed;
                            break;
                        }

                    // NOTE: How does this translate in a world based on ADO.NET
                    //        Does it translate at all?
                    //case "sql-types-mapping":
                    //    {
                    //        String sqlType = GetRequiredAttribute(subElement, "sql-type");
                    //        String dataType = GetRequiredAttribute(subElement, "data-type");
                    //        int sqlTypeInt;

                    //        if (!Int32.TryParse(sqlType, out sqlTypeInt))
                    //        {
                    //            throw new ConfigurationException("Error converting sql type '" + sqlType +
                    //                                             "' to integer constant");
                    //        }
                    //        configDBRef.AddSqlTypesBinding(sqlTypeInt, dataType);
                    //        break;
                    //    }

                    case "expiry-time-cache":
                        {
                            var maxAge = subElement.Attributes.GetNamedItem("max-age-seconds").Value;
                            var purgeInterval =
                                subElement.Attributes.GetNamedItem("purge-interval-seconds").Value;

                            var refTypeEnum = ConfigurationCacheReferenceTypeHelper.GetDefault();
                            if (subElement.Attributes.GetNamedItem("ref-type") != null)
                            {
                                var refType = subElement.Attributes.GetNamedItem("ref-type").Value;
                                refTypeEnum = EnumHelper.Parse<ConfigurationCacheReferenceType>(refType);
                            }
                            configDBRef.SetExpiryTimeCache(Double.Parse(maxAge), Double.Parse(purgeInterval), refTypeEnum);
                            break;
                        }
                    case "lru-cache":
                        {
                            var size = GetRequiredAttribute(subElement, "size");
                            configDBRef.LRUCache = Int32.Parse(size);
                            break;
                        }
                }
            }
        }

        private static void HandleMethodReference(Configuration configuration, XmlElement element)
        {
            var className = element.Attributes.GetNamedItem("class-name").Value;
            var configMethodRef = new ConfigurationMethodRef();
            configuration.AddMethodRef(className, configMethodRef);

            foreach (var subElement in CreateElementEnumerable(element.ChildNodes))
            {
                switch (subElement.Name)
                {
                    case "expiry-time-cache":
                        {
                            var maxAge = subElement.Attributes.GetNamedItem("max-age-seconds").Value;
                            var purgeInterval = subElement.Attributes.GetNamedItem("purge-interval-seconds").Value;
                            var refTypeEnum = ConfigurationCacheReferenceTypeHelper.GetDefault();
                            if (subElement.Attributes.GetNamedItem("ref-type") != null)
                            {
                                var refType = subElement.Attributes.GetNamedItem("ref-type").Value;
                                refTypeEnum = EnumHelper.Parse<ConfigurationCacheReferenceType>(refType);
                            }
                            configMethodRef.SetExpiryTimeCache(Double.Parse(maxAge),
                                                               Double.Parse(purgeInterval), refTypeEnum);
                        }
                        break;
                    case "lru-cache":
                        {
                            var size = subElement.Attributes.GetNamedItem("size").Value;
                            configMethodRef.SetLRUCache(Int32.Parse(size));
                            break;
                        }
                }
            }
        }

        private static void HandlePlugInView(Configuration configuration, XmlElement element)
        {
            var @namespace = GetRequiredAttribute(element, "namespace");
            var name = GetRequiredAttribute(element, "name");
            var factoryClassName = GetRequiredAttribute(element, "factory-class");
            configuration.AddPlugInView(@namespace, name, factoryClassName);
        }

        private static void HandlePlugInVirtualDW(Configuration configuration, XmlElement element)
        {
            var @namespace = GetRequiredAttribute(element, "namespace");
            var name = GetRequiredAttribute(element, "name");
            var factoryClassName = GetRequiredAttribute(element, "factory-class");
            var config = GetOptionalAttribute(element, "config");
            configuration.AddPlugInVirtualDataWindow(@namespace, name, factoryClassName, config);
        }

        private static void HandlePlugInAggregation(Configuration configuration, XmlElement element)
        {
            var name = GetRequiredAttribute(element, "name");
            var factoryClassName = GetRequiredAttribute(element, "factory-class");
            configuration.AddPlugInAggregationFunctionFactory(name, factoryClassName);
        }

        private static void HandlePlugInMultiFunctionAggregation(Configuration configuration, XmlElement element)
        {
            var functionNames = GetRequiredAttribute(element, "function-names");
            var factoryClassName = GetOptionalAttribute(element, "factory-class");

            IDictionary<String, Object> additionalProps = null;
            foreach (var subElement in CreateElementEnumerable(element.ChildNodes))
            {
                if (subElement.Name == "init-arg")
                {
                    var name = GetRequiredAttribute(subElement, "name");
                    var value = GetRequiredAttribute(subElement, "value");
                    if (additionalProps == null)
                    {
                        additionalProps = new Dictionary<String, Object>();
                    }
                    additionalProps.Put(name, value);
                }
            }

            var config = new ConfigurationPlugInAggregationMultiFunction(functionNames.Split(','), factoryClassName);
            config.AdditionalConfiguredProperties = additionalProps;
            configuration.AddPlugInAggregationMultiFunction(config);
        }

        private static void HandlePlugInSingleRow(Configuration configuration, XmlElement element)
        {
            var name = GetRequiredAttribute(element, "name");
            var functionClassName = GetRequiredAttribute(element, "function-class");
            var functionMethodName = GetRequiredAttribute(element, "function-method");
            var valueCache = ValueCache.DISABLED;
            var filterOptimizable = FilterOptimizable.ENABLED;
            var valueCacheStr = GetOptionalAttribute(element, "value-cache");
            if (valueCacheStr != null)
            {
                valueCache = EnumHelper.Parse<ValueCache>(valueCacheStr, true);
            }
            var filterOptimizableStr = GetOptionalAttribute(element, "filter-optimizable");
            if (filterOptimizableStr != null)
            {
                filterOptimizable = EnumHelper.Parse<FilterOptimizable>(filterOptimizableStr, true);
            }
            var rethrowExceptionsStr = GetOptionalAttribute(element, "rethrow-exceptions");
            var rethrowExceptions = false;
            if (rethrowExceptionsStr != null)
            {
                rethrowExceptions = Boolean.Parse(rethrowExceptionsStr);
            }
            configuration.AddPlugInSingleRowFunction(name, functionClassName, functionMethodName, valueCache, filterOptimizable, rethrowExceptions);
        }

        private static void HandlePlugInPatternGuard(Configuration configuration, XmlElement element)
        {
            var @namespace = GetRequiredAttribute(element, "namespace");
            var name = GetRequiredAttribute(element, "name");
            var factoryClassName = GetRequiredAttribute(element, "factory-class");
            configuration.AddPlugInPatternGuard(@namespace, name, factoryClassName);
        }

        private static void HandlePlugInPatternObserver(Configuration configuration, XmlElement element)
        {
            var @namespace = GetRequiredAttribute(element, "namespace");
            var name = GetRequiredAttribute(element, "name");
            var factoryClassName = GetRequiredAttribute(element, "factory-class");
            configuration.AddPlugInPatternObserver(@namespace, name, factoryClassName);
        }

        private static void HandleVariable(Configuration configuration, XmlElement element)
        {
            var variableName = element.Attributes.GetNamedItem("name").Value;
            var type = element.Attributes.GetNamedItem("type").Value;

            var variableType = TypeHelper.GetTypeForSimpleName(type);
            if (variableType == null)
            {
                throw new ConfigurationException("Invalid variable type for variable '" + variableName + "', the type is not recognized");
            }

            var initValueNode = element.Attributes.GetNamedItem("initialization-value");
            String initValue = null;
            if (initValueNode != null)
            {
                initValue = initValueNode.Value;
            }

            var isConstant = false;
            if (GetOptionalAttribute(element, "constant") != null)
            {
                isConstant = Boolean.Parse(GetOptionalAttribute(element, "constant"));
            }

            configuration.AddVariable(variableName, variableType, initValue, isConstant);
        }

        private static void HandlePluginLoaders(Configuration configuration, XmlElement element)
        {
            var loaderName = GetRequiredAttribute(element, "name");
            var className = GetRequiredAttribute(element, "class-name");
            var properties = new Properties();
            String configXML = null;

            foreach (var subElement in CreateElementEnumerable(element.ChildNodes))
            {
                if (subElement.Name == "init-arg")
                {
                    var name = GetRequiredAttribute(subElement, "name");
                    var value = GetRequiredAttribute(subElement, "value");
                    properties.Put(name, value);
                }
                else if (subElement.Name == "config-xml")
                {
                    var stringWriter = new StringWriter();
                    var textWriter = new XmlTextWriter(stringWriter);
                    textWriter.WriteStartDocument();
                    subElement.WriteContentTo(textWriter);
                    textWriter.WriteEndDocument();
                    configXML = stringWriter.ToString();
                }
            }
            configuration.AddPluginLoader(loaderName, className, properties, configXML);
        }

        private static void HandlePlugInEventRepresentation(Configuration configuration, XmlElement element)
        {
            var uri = GetRequiredAttribute(element, "uri");
            var className = GetRequiredAttribute(element, "class-name");
            String initializer = null;

            foreach (var subElement in CreateElementEnumerable(element.ChildNodes))
            {
                if (subElement.Name == "initializer")
                {
                    var elementList = CreateElementList(subElement.ChildNodes);
                    if (elementList.Count == 0)
                    {
                        throw new ConfigurationException(
                            "Error handling initializer for plug-in event representation '" + uri +
                            "', no child node found under initializer element, expecting an element node");
                    }

                    try
                    {
                        // Convert the contents of the initializer element into a form that is more suitable for the initializer.
                        // The transform that we have put together for this takes the child nodes and removes them from the
                        // parent namespace.
                        var sWriter = new StringWriter();
                        XmlWriter xWriter = new XmlTextWriter(sWriter);

                        // Esper expects that the fragment below is enough to construct a complete document.  We need to write
                        // the start document element so that we end up with a valid XML document complete with header.
                        xWriter.WriteStartDocument();

                        InitializerTransform.Transform(new XmlNodeReader(subElement), xWriter);

                        // The result of the transform should be a document with a single node.  I'm not 100% certain though because
                        // the transform does not take into account the fact that the initializer can have more than one child and
                        // this would cause the transform to be "technically" invalid.
                        initializer = sWriter.ToString();
                    }
                    catch (XsltException e)
                    {
                        throw new ConfigurationException(
                            "Error handling initializer for plug-in event representation '" + uri + "' :" + e.Message, e);
                    }
                    catch (XmlException e)
                    {
                        throw new ConfigurationException(
                            "Error handling initializer for plug-in event representation '" + uri + "' :" + e.Message, e);
                    }
                }
            }

            Uri uriParsed;
            try
            {
                uriParsed = new Uri(uri);
            }
            catch (UriFormatException ex)
            {
                throw new ConfigurationException(
                    "Error parsing URI '" + uri + "' as a valid System.Uri string:" + ex.Message, ex);
            }
            configuration.AddPlugInEventRepresentation(uriParsed, className, initializer);
        }

        private static void HandlePlugInEventType(Configuration configuration, XmlElement element)
        {
            var uris = new List<Uri>();
            var name = GetRequiredAttribute(element, "name");
            String initializer = null;
            foreach (var subElement in CreateElementEnumerable(element.ChildNodes))
            {
                switch (subElement.Name)
                {
                    case "resolution-uri":
                        do
                        {
                            var uriValue = GetRequiredAttribute(subElement, "value");
                            Uri uri;
                            try
                            {
                                uri = new Uri(uriValue);
                            }
                            catch (UriFormatException ex)
                            {
                                throw new ConfigurationException(
                                    "Error parsing URI '" + uriValue + "' as a valid System.Uri string:" +
                                    ex.Message,
                                    ex);
                            }
                            uris.Add(uri);
                        } while (false);
                        break;

                    case "initializer":
                        var elementList = CreateElementList(subElement.ChildNodes);
                        if (elementList.Count == 0)
                        {
                            throw new ConfigurationException(
                                "Error handling initializer for plug-in event type '" + name +
                                "', no child node found under initializer element, expecting an element node");
                        }

                        try
                        {
                            // Convert the contents of the initializer element into a form that is more suitable for the initializer.
                            // The transform that we have put together for this takes the child nodes and removes them from the
                            // parent namespace.
                            var sWriter = new StringWriter();
                            XmlWriter xWriter = new XmlTextWriter(sWriter);

                            // Esper expects that the fragment below is enough to construct a complete document.  We need to write
                            // the start document element so that we end up with a valid XML document complete with header.
                            xWriter.WriteStartDocument();

                            InitializerTransform.Transform(new XmlNodeReader(subElement), xWriter);

                            // The result of the transform should be a document with a single node.  I'm not 100% certain though because
                            // the transform does not take into account the fact that the initializer can have more than one child and
                            // this would cause the transform to be "technically" invalid.
                            initializer = sWriter.ToString();
                        }
                        catch (XsltException e)
                        {
                            throw new ConfigurationException(
                                "Error handling initializer for plug-in event type '" + name + "' :" + e.Message, e);
                        }
                        catch (XmlException e)
                        {
                            throw new ConfigurationException(
                                "Error handling initializer for plug-in event type '" + name + "' :" + e.Message, e);
                        }

                        break;
                }
            }

            configuration.AddPlugInEventType(name, uris, initializer);
        }

        private static void HandlePlugInEventTypeNameResolution(Configuration configuration, XmlElement element)
        {
            var uris = new List<Uri>();
            foreach (var subElement in CreateElementEnumerable(element.ChildNodes))
            {
                if (subElement.Name == "resolution-uri")
                {
                    var uriValue = GetRequiredAttribute(subElement, "value");
                    Uri uri;
                    try
                    {
                        uri = new Uri(uriValue);
                    }
                    catch (UriFormatException ex)
                    {
                        throw new ConfigurationException(
                            "Error parsing URI '" + uriValue + "' as a valid System.Uri string:" + ex.Message, ex);
                    }
                    uris.Add(uri);
                }
            }

            configuration.PlugInEventTypeResolutionURIs = uris;
        }

        private static void HandleRevisionEventType(Configuration configuration, XmlElement element)
        {
            var revEventType = new ConfigurationRevisionEventType();
            var revTypeName = GetRequiredAttribute(element, "name");

            if (element.Attributes.GetNamedItem("property-revision") != null)
            {
                var propertyRevision = GetRequiredAttribute(element, "property-revision");
                try
                {
                    var propertyRevisionEnum = EnumHelper.Parse<PropertyRevisionEnum>(propertyRevision.Trim(), true);
                    revEventType.PropertyRevision = propertyRevisionEnum;
                }
                catch
                {
                    throw new ConfigurationException("Invalid enumeration value for property-revision attribute '" +
                                                     propertyRevision + "'");
                }
            }

            ICollection<String> keyProperties = new HashSet<String>();

            foreach (var subElement in CreateElementEnumerable(element.ChildNodes))
            {
                switch (subElement.Name)
                {
                    case "base-event-type":
                        do
                        {
                            var name = GetRequiredAttribute(subElement, "name");
                            revEventType.AddNameBaseEventType(name);
                        } while (false);
                        break;
                    case "delta-event-type":
                        do
                        {
                            var name = GetRequiredAttribute(subElement, "name");
                            revEventType.AddNameDeltaEventType(name);
                        } while (false);
                        break;
                    case "key-property":
                        do
                        {
                            var name = GetRequiredAttribute(subElement, "name");
                            keyProperties.Add(name);
                        } while (false);
                        break;
                }
            }

            var keyProps = keyProperties.ToArray();
            revEventType.KeyPropertyNames = keyProps;

            configuration.AddRevisionEventType(revTypeName, revEventType);
        }

        private static void HandleVariantStream(Configuration configuration, XmlElement element)
        {
            var variantStream = new ConfigurationVariantStream();
            var varianceName = GetRequiredAttribute(element, "name");

            if (element.Attributes.GetNamedItem("type-variance") != null)
            {
                var typeVar = GetRequiredAttribute(element, "type-variance");
                try
                {
                    var typeVarianceEnum = EnumHelper.Parse<TypeVarianceEnum>(typeVar.Trim(), true);
                    variantStream.TypeVariance = typeVarianceEnum;
                }
                catch
                {
                    throw new ConfigurationException("Invalid enumeration value for type-variance attribute '" + typeVar +
                                                     "'");
                }
            }

            foreach (var subElement in CreateElementEnumerable(element.ChildNodes))
            {
                if (subElement.Name == "variant-event-type")
                {
                    var name = GetRequiredAttribute(subElement, "name");
                    variantStream.AddEventTypeName(name);
                }
            }

            configuration.AddVariantStream(varianceName, variantStream);
        }

        private static void HandleEngineSettings(Configuration configuration, XmlElement element)
        {
            foreach (var subElement in CreateElementEnumerable(element.ChildNodes))
            {
                if (subElement.Name == "defaults")
                {
                    HandleEngineSettingsDefaults(configuration, subElement);
                }
            }
        }

        private static void HandlePlugInPatternObjects(Configuration configuration, XmlElement parentElement)
        {
            var nodes = parentElement.GetElementsByTagName("plugin-pattern-guard");
            foreach (XmlNode node in nodes)
            {
                var nspace = GetRequiredAttribute(node, "namespace");
                var name = GetRequiredAttribute(node, "name");
                var factoryClassName = GetRequiredAttribute(node, "factory-class");
                configuration.AddPlugInPatternGuard(nspace, name, factoryClassName);
            }

            nodes = parentElement.GetElementsByTagName("plugin-pattern-observer");
            foreach (XmlNode node in nodes)
            {
                var nspace = GetRequiredAttribute(node, "namespace");
                var name = GetRequiredAttribute(node, "name");
                var factoryClassName = GetRequiredAttribute(node, "factory-class");
                configuration.AddPlugInPatternObserver(nspace, name, factoryClassName);
            }
        }

        private static void HandleEngineSettingsDefaults(Configuration configuration, XmlElement parentElement)
        {
            foreach (var subElement in CreateElementEnumerable(parentElement.ChildNodes))
            {
                switch (subElement.Name)
                {
                    case "threading":
                        HandleDefaultsThreading(configuration, subElement);
                        break;
                    case "event-meta":
                        HandleDefaultsEventMeta(configuration, subElement);
                        break;
                    case "view-resources":
                        HandleDefaultsViewResources(configuration, subElement);
                        break;
                    case "logging":
                        HandleDefaultsLogging(configuration, subElement);
                        break;
                    case "variables":
                        HandleDefaultsVariables(configuration, subElement);
                        break;
                    case "patterns":
                        HandleDefaultsPatterns(configuration, subElement);
                        break;
                    case "match-recognize":
                        HandleDefaultsMatchRecognize(configuration, subElement);
                        break;
                    case "stream-selection":
                        HandleDefaultsStreamSelection(configuration, subElement);
                        break;
                    case "time-source":
                        HandleDefaultsTimeSource(configuration, subElement);
                        break;
                    case "metrics-reporting":
                        HandleMetricsReporting(configuration, subElement);
                        break;
                    case "language":
                        HandleLanguage(configuration, subElement);
                        break;
                    case "expression":
                        HandleExpression(configuration, subElement);
                        break;
                    case "execution":
                        HandleExecution(configuration, subElement);
                        break;
                    case "exceptionHandling":
                        {
                            configuration.EngineDefaults.ExceptionHandlingConfig.AddClasses(
                                GetHandlerFactories(subElement));
                            var enableUndeployRethrowStr = GetOptionalAttribute(subElement, "undeploy-rethrow-policy");
                            if (enableUndeployRethrowStr != null)
                            {
                                configuration.EngineDefaults.ExceptionHandlingConfig.UndeployRethrowPolicy = 
                                    EnumHelper.Parse<ConfigurationEngineDefaults.UndeployRethrowPolicy>(enableUndeployRethrowStr);
                            }
                        }

                        break;
                    case "conditionHandling":
                        {
                            configuration.EngineDefaults.ConditionHandlingConfig.AddClasses(
                                GetHandlerFactories(subElement));
                            var enableUndeployRethrowStr = GetOptionalAttribute(subElement, "undeploy-rethrow-policy");
                            if (enableUndeployRethrowStr != null)
                            {
                                configuration.EngineDefaults.ExceptionHandlingConfig.UndeployRethrowPolicy =
                                    EnumHelper.Parse<ConfigurationEngineDefaults.UndeployRethrowPolicy>(enableUndeployRethrowStr);
                            }
                        }

                        break;
                    case "scripts":
                        HandleDefaultScriptConfig(configuration, subElement);
                        break;
                }
            }
        }

        private static void HandleDefaultsThreading(Configuration configuration, XmlElement parentElement)
        {
            var engineFairlockStr = GetOptionalAttribute(parentElement, "engine-fairlock");
            if (engineFairlockStr != null)
            {
                var isEngineFairlock = Boolean.Parse(engineFairlockStr);
                configuration.EngineDefaults.ThreadingConfig.IsEngineFairlock = isEngineFairlock;
            }

            foreach (var subElement in CreateElementEnumerable(parentElement.ChildNodes))
            {
                switch (subElement.Name)
                {
                    case "listener-dispatch":
                        {
                            var preserveOrderText =
                                GetRequiredAttribute(subElement, "preserve-order");
                            var preserveOrder = Boolean.Parse(preserveOrderText);
                            configuration.EngineDefaults.ThreadingConfig.IsListenerDispatchPreserveOrder = preserveOrder;

                            if (subElement.Attributes.GetNamedItem("timeout-msec") != null)
                            {
                                var timeoutMSecText = subElement.Attributes.GetNamedItem("timeout-msec").Value;
                                var timeoutMSec = Int64.Parse(timeoutMSecText);
                                configuration.EngineDefaults.ThreadingConfig.ListenerDispatchTimeout = timeoutMSec;
                            }

                            if (subElement.Attributes.GetNamedItem("locking") != null)
                            {
                                var value = subElement.Attributes.GetNamedItem("locking").Value;
                                configuration.EngineDefaults.ThreadingConfig.ListenerDispatchLocking =
                                    EnumHelper.Parse<ConfigurationEngineDefaults.Threading.Locking>(value);
                            }
                            break;
                        }
                    case "insert-into-dispatch":
                        {
                            var preserveOrderText =
                                GetRequiredAttribute(subElement, "preserve-order");
                            var preserveOrder = Boolean.Parse(preserveOrderText);
                            configuration.EngineDefaults.ThreadingConfig.IsInsertIntoDispatchPreserveOrder = preserveOrder;

                            if (subElement.Attributes.GetNamedItem("timeout-msec") != null)
                            {
                                var timeoutMSecText = subElement.Attributes.GetNamedItem("timeout-msec").Value;
                                var timeoutMSec = Int64.Parse(timeoutMSecText);
                                configuration.EngineDefaults.ThreadingConfig.InsertIntoDispatchTimeout = timeoutMSec;
                            }

                            if (subElement.Attributes.GetNamedItem("locking") != null)
                            {
                                var value = subElement.Attributes.GetNamedItem("locking").Value;
                                configuration.EngineDefaults.ThreadingConfig.InsertIntoDispatchLocking =
                                    EnumHelper.Parse<ConfigurationEngineDefaults.Threading.Locking>(value);
                            }
                            break;
                        }
                    case "named-window-consumer-dispatch":
                        {
                            var preserveOrderText = GetRequiredAttribute(subElement, "preserve-order");
                            var preserveOrder = Boolean.Parse(preserveOrderText);
                            configuration.EngineDefaults.ThreadingConfig.IsNamedWindowConsumerDispatchPreserveOrder = preserveOrder;

                            if (subElement.Attributes.GetNamedItem("timeout-msec") != null)
                            {
                                var timeoutMSecText = subElement.Attributes.GetNamedItem("timeout-msec").Value;
                                var timeoutMSec = Int64.Parse(timeoutMSecText);
                                configuration.EngineDefaults.ThreadingConfig.NamedWindowConsumerDispatchTimeout = timeoutMSec;
                            }

                            if (subElement.Attributes.GetNamedItem("locking") != null)
                            {
                                var value = subElement.Attributes.GetNamedItem("locking").Value;
                                configuration.EngineDefaults.ThreadingConfig.NamedWindowConsumerDispatchLocking =
                                    EnumHelper.Parse<ConfigurationEngineDefaults.Threading.Locking>(value);
                            }

                            break;
                        }
                    case "internal-timer":
                        {
                            var enabledText = GetRequiredAttribute(subElement, "enabled");
                            var enabled = Boolean.Parse(enabledText);
                            var msecResolutionText =
                                GetRequiredAttribute(subElement, "msec-resolution");
                            var msecResolution = Int64.Parse(msecResolutionText);
                            configuration.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = enabled;
                            configuration.EngineDefaults.ThreadingConfig.InternalTimerMsecResolution = msecResolution;
                            break;
                        }
                    case "threadpool-inbound":
                        {
                            var result = ParseThreadPoolConfig(subElement);
                            configuration.EngineDefaults.ThreadingConfig.IsThreadPoolInbound = result.IsEnabled;
                            configuration.EngineDefaults.ThreadingConfig.ThreadPoolInboundNumThreads = result.ThreadCount;
                            configuration.EngineDefaults.ThreadingConfig.ThreadPoolInboundCapacity = result.Capacity;
                        }
                        break;
                    case "threadpool-outbound":
                        {
                            var result = ParseThreadPoolConfig(subElement);
                            configuration.EngineDefaults.ThreadingConfig.IsThreadPoolOutbound = result.IsEnabled;
                            configuration.EngineDefaults.ThreadingConfig.ThreadPoolOutboundNumThreads = result.ThreadCount;
                            configuration.EngineDefaults.ThreadingConfig.ThreadPoolOutboundCapacity = result.Capacity;
                        }
                        break;
                    case "threadpool-timerexec":
                        {
                            var result = ParseThreadPoolConfig(subElement);
                            configuration.EngineDefaults.ThreadingConfig.IsThreadPoolTimerExec = result.IsEnabled;
                            configuration.EngineDefaults.ThreadingConfig.ThreadPoolTimerExecNumThreads = result.ThreadCount;
                            configuration.EngineDefaults.ThreadingConfig.ThreadPoolTimerExecCapacity = result.Capacity;
                        }
                        break;
                    case "threadpool-routeexec":
                        {
                            var result = ParseThreadPoolConfig(subElement);
                            configuration.EngineDefaults.ThreadingConfig.IsThreadPoolRouteExec = result.IsEnabled;
                            configuration.EngineDefaults.ThreadingConfig.ThreadPoolRouteExecNumThreads = result.ThreadCount;
                            configuration.EngineDefaults.ThreadingConfig.ThreadPoolRouteExecCapacity = result.Capacity;
                        }
                        break;
                    case "thread-local":
                        {
                            var value = subElement.Attributes.GetNamedItem("style").Value;
                            configuration.EngineDefaults.ThreadingConfig.ThreadLocalStyle =
                                EnumHelper.Parse<ConfigurationEngineDefaults.Threading.ThreadLocal>(value);
                            break;
                        }
                }
            }
        }

        private static ThreadPoolConfig ParseThreadPoolConfig(XmlElement parentElement)
        {
            var enabled = GetRequiredAttribute(parentElement, "enabled");
            var isEnabled = Boolean.Parse(enabled);
            if (isEnabled)
            {

                var numThreadsStr = GetRequiredAttribute(parentElement, "num-threads");
                var numThreads = Int32.Parse(numThreadsStr);

                var capacityStr = GetOptionalAttribute(parentElement, "capacity");
                int? capacity = null;
                if (capacityStr != null)
                {
                    capacity = Int32.Parse(capacityStr);
                }

                return new ThreadPoolConfig(isEnabled, numThreads, capacity);
            }

            return new ThreadPoolConfig(false, 0, null);
        }

        private static void HandleDefaultsViewResources(Configuration configuration, XmlElement parentElement)
        {
            foreach (var subElement in CreateElementEnumerable(parentElement.ChildNodes))
            {
                if (subElement.Name == "share-views")
                {
                    var valueText = GetRequiredAttribute(subElement, "enabled");
                    var value = Boolean.Parse(valueText);
                    configuration.EngineDefaults.ViewResourcesConfig.IsShareViews = value;
                }
                else if (subElement.Name == "allow-multiple-expiry-policy")
                {
                    var valueText = GetRequiredAttribute(subElement, "enabled");
                    var value = Boolean.Parse(valueText);
                    configuration.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies = value;
                }
                else if (subElement.Name == "iterable-unbound")
                {
                    var valueText = GetRequiredAttribute(subElement, "enabled");
                    var value = Boolean.Parse(valueText);
                    configuration.EngineDefaults.ViewResourcesConfig.IsIterableUnbound = value;
                }
            }
        }

        private static void HandleDefaultsVariables(Configuration configuration, XmlElement parentElement)
        {
            foreach (var subElement in CreateElementEnumerable(parentElement.ChildNodes))
            {
                if (subElement.Name == "msec-version-release")
                {
                    var valueText = subElement.Attributes.GetNamedItem("value").Value;
                    var value = Int64.Parse(valueText);
                    configuration.EngineDefaults.VariablesConfig.MsecVersionRelease = value;
                }
            }
        }

        private static void HandleDefaultsPatterns(Configuration configuration, XmlElement parentElement)
        {
            foreach (var subElement in CreateElementEnumerable(parentElement.ChildNodes))
            {
                if (subElement.Name == "max-subexpression")
                {
                    var valueText = GetRequiredAttribute(subElement, "value");
                    var value = long.Parse(valueText);
                    configuration.EngineDefaults.PatternsConfig.MaxSubexpressions = value;

                    var preventText = GetOptionalAttribute(subElement, "prevent-start");
                    if (preventText != null)
                    {
                        configuration.EngineDefaults.PatternsConfig.IsMaxSubexpressionPreventStart = 
                            Boolean.Parse(preventText);
                    }
                }
            }
        }

        private static void HandleDefaultsLogging(Configuration configuration, XmlElement parentElement)
        {
            foreach (var subElement in CreateElementEnumerable(parentElement.ChildNodes))
            {
                switch (subElement.Name)
                {
                    case "execution-path":
                    {
                        var valueText = GetRequiredAttribute(subElement, "enabled");
                        var value = Boolean.Parse(valueText);
                        configuration.EngineDefaults.LoggingConfig.IsEnableExecutionDebug = value;
                    }
                        break;
                    case "timer-debug":
                    {
                        var valueText = GetRequiredAttribute(subElement, "enabled");
                        var value = Boolean.Parse(valueText);
                        configuration.EngineDefaults.LoggingConfig.IsEnableTimerDebug = value;
                    }
                        break;
                    case "query-plan":
                    {
                        var valueText = GetRequiredAttribute(subElement, "enabled");
                        var value = Boolean.Parse(valueText);
                        configuration.EngineDefaults.LoggingConfig.IsEnableQueryPlan = value;
                    }
                        break;
                    case "ado":
                    {
                        var valueText = GetRequiredAttribute(subElement, "enabled");
                        var value = Boolean.Parse(valueText);
                        configuration.EngineDefaults.LoggingConfig.IsEnableADO = value;
                    }
                        break;
                    case "audit":
                        configuration.EngineDefaults.LoggingConfig.AuditPattern = GetOptionalAttribute(subElement, "pattern");
                        break;
                }
            }
        }

        private static void HandleDefaultsMatchRecognize(Configuration configuration, XmlElement parentElement)
        {
            foreach (var subElement in CreateElementEnumerable(parentElement.ChildNodes))
            {
                if (subElement.Name == "max-state")
                {
                    var valueText = GetRequiredAttribute(subElement, "value");
                    var value = Int64.Parse(valueText);
                    configuration.EngineDefaults.MatchRecognizeConfig.MaxStates = value;

                    var preventText = GetOptionalAttribute(subElement, "prevent-start");
                    if (preventText != null)
                    {
                        configuration.EngineDefaults.MatchRecognizeConfig.IsMaxStatesPreventStart = Boolean.Parse(preventText);
                    }
                }
            }
        }

        private static void HandleDefaultsStreamSelection(Configuration configuration, XmlElement parentElement)
        {
            foreach (var subElement in CreateElementEnumerable(parentElement.ChildNodes))
            {
                if (subElement.Name == "stream-selector")
                {
                    var valueText = GetRequiredAttribute(subElement, "value");
                    if (valueText == null)
                    {
                        throw new ConfigurationException("No value attribute supplied for stream-selector element");
                    }
                    StreamSelector defaultSelector;
                    switch (valueText.ToUpper().Trim())
                    {
                        case "ISTREAM":
                            defaultSelector = StreamSelector.ISTREAM_ONLY;
                            break;
                        case "RSTREAM":
                            defaultSelector = StreamSelector.RSTREAM_ONLY;
                            break;
                        case "IRSTREAM":
                            defaultSelector = StreamSelector.RSTREAM_ISTREAM_BOTH;
                            break;
                        default:
                            throw new ConfigurationException("ConstantValue attribute for stream-selector element invalid, " +
                                                             "expected one of the following keywords: istream, irstream, rstream");
                    }

                    configuration.EngineDefaults.StreamSelectionConfig.DefaultStreamSelector = defaultSelector;
                }
            }
        }

        private static void HandleDefaultsTimeSource(Configuration configuration, XmlElement parentElement)
        {
            foreach (var subElement in CreateElementEnumerable(parentElement.ChildNodes))
            {
                if (subElement.Name == "time-source-type")
                {
                    var valueText = GetRequiredAttribute(subElement, "value");
                    if (valueText == null)
                    {
                        throw new ConfigurationException("No value attribute supplied for time-source element");
                    }

                    var timeSourceType = EnumHelper.Parse<ConfigurationEngineDefaults.TimeSourceType>(valueText, true);
                    configuration.EngineDefaults.TimeSourceConfig.TimeSourceType = timeSourceType;
                }
            }
        }

        private static void HandleExecution(Configuration configuration, XmlElement parentElement)
        {
            var prioritizedStr = GetOptionalAttribute(parentElement, "prioritized");
            if (prioritizedStr != null)
            {
                var isPrioritized = Boolean.Parse(prioritizedStr);
                configuration.EngineDefaults.ExecutionConfig.IsPrioritized = isPrioritized;
            }
            var fairlockStr = GetOptionalAttribute(parentElement, "fairlock");
            if (fairlockStr != null)
            {
                var isFairlock = Boolean.Parse(fairlockStr);
                configuration.EngineDefaults.ExecutionConfig.IsFairlock = isFairlock;
            }
            var disableLockingStr = GetOptionalAttribute(parentElement, "disable-locking");
            if (disableLockingStr != null)
            {
                var isDisablelock = Boolean.Parse(disableLockingStr);
                configuration.EngineDefaults.ExecutionConfig.IsDisableLocking = isDisablelock;
            }
            var threadingProfileStr = GetOptionalAttribute(parentElement, "threading-profile");
            if (threadingProfileStr != null)
            {
                var profile = EnumHelper.Parse<ConfigurationEngineDefaults.ThreadingProfile>(threadingProfileStr, true);
                configuration.EngineDefaults.ExecutionConfig.ThreadingProfile = profile;
            }
            var filterServiceProfileStr = GetOptionalAttribute(parentElement, "filter-service-profile");
            if (filterServiceProfileStr != null)
            {
                var profile = EnumHelper.Parse<ConfigurationEngineDefaults.FilterServiceProfile>(filterServiceProfileStr);
                configuration.EngineDefaults.ExecutionConfig.FilterServiceProfile = profile;
            }
            var filterServiceMaxFilterWidthStr = GetOptionalAttribute(parentElement, "filter-service-max-filter-width");
            if (filterServiceMaxFilterWidthStr != null)
            {
                configuration.EngineDefaults.ExecutionConfig.FilterServiceMaxFilterWidth = Int32.Parse(filterServiceMaxFilterWidthStr);
            } 
            var allowIsolatedServiceStr = GetOptionalAttribute(parentElement, "allow-isolated-service");
            if (allowIsolatedServiceStr != null)
            {
                var isAllowIsolatedService = Boolean.Parse(allowIsolatedServiceStr);
                configuration.EngineDefaults.ExecutionConfig.IsAllowIsolatedService = isAllowIsolatedService;
            }
            var declExprValueCacheSizeStr = GetOptionalAttribute(parentElement, "declared-expr-value-cache-size");
            if (declExprValueCacheSizeStr != null)
            {
                configuration.EngineDefaults.ExecutionConfig.DeclaredExprValueCacheSize = Int32.Parse(declExprValueCacheSizeStr);
            }
        }

        private static void HandleMetricsReporting(Configuration configuration, XmlElement parentElement)
        {
            var enabled = GetRequiredAttribute(parentElement, "enabled");
            var isEnabled = Boolean.Parse(enabled);
            configuration.EngineDefaults.MetricsReportingConfig.IsEnableMetricsReporting = isEnabled;

            var engineInterval = GetOptionalAttribute(parentElement, "engine-interval");
            if (engineInterval != null)
            {
                configuration.EngineDefaults.MetricsReportingConfig.EngineInterval = Int64.Parse(engineInterval);
            }

            var statementInterval = GetOptionalAttribute(parentElement, "statement-interval");
            if (statementInterval != null)
            {
                configuration.EngineDefaults.MetricsReportingConfig.StatementInterval = Int64.Parse(statementInterval);
            }

            var threading = GetOptionalAttribute(parentElement, "threading");
            if (threading != null)
            {
                configuration.EngineDefaults.MetricsReportingConfig.IsThreading = Boolean.Parse(threading);
            }

            foreach (var subElement in CreateElementEnumerable(parentElement.ChildNodes))
            {
                if (subElement.Name == "stmtgroup")
                {
                    var name = GetRequiredAttribute(subElement, "name");
                    var interval = Int64.Parse(GetRequiredAttribute(subElement, "interval"));

                    var metrics = new ConfigurationMetricsReporting.StmtGroupMetrics();
                    metrics.Interval = interval;
                    configuration.EngineDefaults.MetricsReportingConfig.AddStmtGroup(name, metrics);

                    var defaultInclude = GetOptionalAttribute(subElement, "default-include");
                    if (defaultInclude != null)
                    {
                        metrics.IsDefaultInclude = Boolean.Parse(defaultInclude);
                    }

                    var numStmts = GetOptionalAttribute(subElement, "num-stmts");
                    if (numStmts != null)
                    {
                        metrics.NumStatements = Int32.Parse(numStmts);
                    }

                    var reportInactive = GetOptionalAttribute(subElement, "report-inactive");
                    if (reportInactive != null)
                    {
                        metrics.IsReportInactive = Boolean.Parse(reportInactive);
                    }

                    HandleMetricsReportingPatterns(metrics, subElement);
                }
            }
        }

        private static void HandleLanguage(Configuration configuration, XmlElement parentElement)
        {
            var sortUsingCollator = GetOptionalAttribute(parentElement, "sort-using-collator");
            if (sortUsingCollator != null)
            {
                var isSortUsingCollator = Boolean.Parse(sortUsingCollator);
                configuration.EngineDefaults.LanguageConfig.IsSortUsingCollator = isSortUsingCollator;
            }
        }

        private static void HandleExpression(Configuration configuration, XmlElement parentElement)
        {
            var integerDivision = GetOptionalAttribute(parentElement, "integer-division");
            if (integerDivision != null)
            {
                var isIntegerDivision = Boolean.Parse(integerDivision);
                configuration.EngineDefaults.ExpressionConfig.IsIntegerDivision = isIntegerDivision;
            }

            var divZero = GetOptionalAttribute(parentElement, "division-by-zero-is-null");
            if (divZero != null)
            {
                var isDivZero = Boolean.Parse(divZero);
                configuration.EngineDefaults.ExpressionConfig.IsDivisionByZeroReturnsNull = isDivZero;
            }

            var udfCache = GetOptionalAttribute(parentElement, "udf-cache");
            if (udfCache != null)
            {
                var isUdfCache = Boolean.Parse(udfCache);
                configuration.EngineDefaults.ExpressionConfig.IsUdfCache = isUdfCache;
            }

            var selfSubselectPreeval = GetOptionalAttribute(parentElement, "self-subselect-preeval");
            if (selfSubselectPreeval != null)
            {
                var isSelfSubselectPreeval = Boolean.Parse(selfSubselectPreeval);
                configuration.EngineDefaults.ExpressionConfig.IsSelfSubselectPreeval = isSelfSubselectPreeval;
            }

            var extendedAggregationStr = GetOptionalAttribute(parentElement, "extended-agg");
            if (extendedAggregationStr != null)
            {
                var extendedAggregation = Boolean.Parse(extendedAggregationStr);
                configuration.EngineDefaults.ExpressionConfig.IsExtendedAggregation = extendedAggregation;
            }
            var duckTypingStr = GetOptionalAttribute(parentElement, "ducktyping");
            if (duckTypingStr != null)
            {
                var duckTyping = Boolean.Parse(duckTypingStr);
                configuration.EngineDefaults.ExpressionConfig.IsDuckTyping = duckTyping;
            }
            var mathContextStr = GetOptionalAttribute(parentElement, "math-context");
            if (mathContextStr != null)
            {
                try
                {
                    var mathContext = new MathContext(mathContextStr);
                    configuration.EngineDefaults.ExpressionConfig.MathContext = mathContext;
                }
                catch (ArgumentException)
                {
                    throw new ConfigurationException("Failed to parse '" + mathContextStr + "' as a MathContext");
                }
            }
            var timeZoneStr = GetOptionalAttribute(parentElement, "time-zone");
            if (timeZoneStr != null)
            {
                configuration.EngineDefaults.ExpressionConfig.TimeZone = TimeZoneHelper.GetTimeZoneInfo(timeZoneStr);
            }
        }

        private static void HandleDefaultScriptConfig(Configuration configuration, XmlElement parentElement)
        {
            var defaultDialect = GetOptionalAttribute(parentElement, "default-dialect");
            if (defaultDialect != null)
            {
                configuration.EngineDefaults.ScriptsConfig.DefaultDialect = defaultDialect;
            }
        }

        private static IEnumerable<string> GetHandlerFactories(XmlElement parentElement)
        {
            return CreateElementEnumerable(parentElement.ChildNodes)
                .Where(subElement => subElement.Name == "handlerFactory")
                .Select(subElement => GetRequiredAttribute(subElement, "class"))
                .ToArray();
        }

        private static void HandleMetricsReportingPatterns(ConfigurationMetricsReporting.StmtGroupMetrics groupDef,
                                                           XmlElement parentElement)
        {
            foreach (var subElement in CreateElementEnumerable(parentElement.ChildNodes))
            {
                string text;
                switch (subElement.Name)
                {
                    case "include-regex":
                        text = subElement.ChildNodes[0].InnerText;
                        groupDef.Patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetRegex(text), true));
                        break;
                    case "exclude-regex":
                        text = subElement.ChildNodes[0].InnerText;
                        groupDef.Patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetRegex(text), false));
                        break;
                    case "include-like":
                        text = subElement.ChildNodes[0].InnerText;
                        groupDef.Patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetLike(text), true));
                        break;
                    case "exclude-like":
                        text = subElement.ChildNodes[0].InnerText;
                        groupDef.Patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetLike(text), false));
                        break;
                }
            }
        }

        private static void HandleDefaultsEventMeta(Configuration configuration, XmlElement parentElement)
        {
            foreach (var subElement in CreateElementEnumerable(parentElement.ChildNodes))
            {
                switch (subElement.Name)
                {
                    case "class-property-resolution":
                    {
                        var styleNode = subElement.Attributes.GetNamedItem("style");
                        if (styleNode != null)
                        {
                            var styleText = styleNode.InnerText;
                            var value = EnumHelper.Parse<PropertyResolutionStyle>(styleText);
                            configuration.EngineDefaults.EventMetaConfig.ClassPropertyResolutionStyle = value;
                        }

                        var accessorStyleNode = subElement.Attributes.GetNamedItem("accessor-style");
                        if (accessorStyleNode != null)
                        {
                            var accessorStyleText = accessorStyleNode.InnerText;
                            var value = EnumHelper.Parse<AccessorStyleEnum>(accessorStyleText);
                            configuration.EngineDefaults.EventMetaConfig.DefaultAccessorStyle = value;
                        }
                        break;
                    }
                    case "event-representation":
                    {
                        var typeNode = subElement.Attributes.GetNamedItem("type");
                        if (typeNode != null)
                        {
                            var typeText = typeNode.InnerText;
                            var value = EnumHelper.Parse<EventRepresentation>(typeText);
                            configuration.EngineDefaults.EventMetaConfig.DefaultEventRepresentation = value;
                        }
                        break;
                    }
                    case "anonymous-cache":
                    {
                        var sizeNode = subElement.Attributes.GetNamedItem("size");
                        if (sizeNode != null)
                        {
                            configuration.EngineDefaults.EventMetaConfig.AnonymousCacheSize = int.Parse(sizeNode.InnerText);
                        }
                        break;
                    }
                }

            }
        }

        public static Properties CreatePropertiesFromAttributes(XmlAttributeCollection attributes)
        {
            var properties = new Properties();
            foreach (XmlAttribute attribute in attributes)
            {
                switch (attribute.Name)
                {
                    case "type":
                        break;
                    default:
                        properties[attribute.Name] = attribute.InnerText;
                        break;
                }
            }
            return properties;
        }

        /// <summary>
        /// Returns an input stream from an application resource in the classpath.
        /// </summary>
        /// <param name="resource">to get input stream for</param>
        /// <returns>input stream for resource</returns>
        public static Stream GetResourceAsStream(String resource)
        {
            var stripped = resource.StartsWith("/") ? resource.Substring(1) : resource;
            var stream = ResourceManager.GetResourceAsStream(resource) ??
                            ResourceManager.GetResourceAsStream(stripped);
            if (stream == null)
            {
                throw new EPException(resource + " not found");
            }
            return stream;
        }

        private static String GetOptionalAttribute(XmlNode node, String key)
        {
            var valueNode = node.Attributes.GetNamedItem(key);
            if (valueNode != null)
            {
                return valueNode.InnerText;
            }
            return null;
        }

        private static String GetRequiredAttribute(XmlNode node, String key)
        {
            var valueNode = node.Attributes.GetNamedItem(key);
            if (valueNode == null)
            {
                var name = String.IsNullOrEmpty(node.Name)
                    ? node.LocalName
                    : node.Name;
                throw new ConfigurationException("Required attribute by name '" + key + "' not found for element '" + name + "'");
            }
            return valueNode.InnerText;
        }

        private static IList<XmlElement> CreateElementList(XmlNodeList nodeList)
        {
            return new List<XmlElement>(CreateElementEnumerable(nodeList));
        }

        private static IEnumerable<XmlElement> CreateElementEnumerable(XmlNodeList nodeList)
        {
            foreach (XmlNode node in nodeList)
            {
                if (node is XmlElement)
                {
                    yield return node as XmlElement;
                }
            }
        }

        #region Nested type: ThreadPoolConfig

        private class ThreadPoolConfig
        {
            public ThreadPoolConfig(bool enabled, int numThreads, int? capacity)
            {
                IsEnabled = enabled;
                ThreadCount = numThreads;
                Capacity = capacity;
            }

            public bool IsEnabled { get; private set; }
            public int ThreadCount { get; private set; }
            public int? Capacity { get; private set; }
        }

        #endregion
    }
}
