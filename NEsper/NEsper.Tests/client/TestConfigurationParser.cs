///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Xml.XPath;

using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.type;

using NUnit.Framework;

namespace com.espertech.esper.client
{
    [TestFixture]
    public class TestConfigurationParser 
    {
        private Configuration _config;
    
        [SetUp]
        public void SetUp()
        {
            _config = new Configuration();
        }
    
        [Test]
        public void TestConfigureFromStream()
        {
            var uri = ResourceManager.ResolveResourceURL(TestConfiguration.ESPER_TEST_CONFIG);
            var client = new WebClient();
            using (var stream = client.OpenRead(uri))
            {
                ConfigurationParser.DoConfigure(_config, stream, uri.ToString());
                AssertFileConfig(_config);
            }
        }
    
        [Test]
        public void TestEngineDefaults()
        {
            _config = new Configuration();
    
            Assert.IsTrue(_config.EngineDefaults.ThreadingConfig.IsInsertIntoDispatchPreserveOrder);
            Assert.AreEqual(100, _config.EngineDefaults.ThreadingConfig.InsertIntoDispatchTimeout);
            Assert.IsTrue(_config.EngineDefaults.ThreadingConfig.IsListenerDispatchPreserveOrder);
            Assert.AreEqual(1000, _config.EngineDefaults.ThreadingConfig.ListenerDispatchTimeout);
            Assert.IsTrue(_config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled);
            Assert.AreEqual(100, _config.EngineDefaults.ThreadingConfig.InternalTimerMsecResolution);
            Assert.AreEqual(ConfigurationEngineDefaults.Threading.Locking.SPIN, _config.EngineDefaults.ThreadingConfig.InsertIntoDispatchLocking);
            Assert.AreEqual(ConfigurationEngineDefaults.Threading.Locking.SPIN, _config.EngineDefaults.ThreadingConfig.ListenerDispatchLocking);
            Assert.IsFalse(_config.EngineDefaults.ThreadingConfig.IsThreadPoolInbound);
            Assert.IsFalse(_config.EngineDefaults.ThreadingConfig.IsThreadPoolOutbound);
            Assert.IsFalse(_config.EngineDefaults.ThreadingConfig.IsThreadPoolRouteExec);
            Assert.IsFalse(_config.EngineDefaults.ThreadingConfig.IsThreadPoolTimerExec);
            Assert.AreEqual(2, _config.EngineDefaults.ThreadingConfig.ThreadPoolInboundNumThreads);
            Assert.AreEqual(2, _config.EngineDefaults.ThreadingConfig.ThreadPoolOutboundNumThreads);
            Assert.AreEqual(2, _config.EngineDefaults.ThreadingConfig.ThreadPoolRouteExecNumThreads);
            Assert.AreEqual(2, _config.EngineDefaults.ThreadingConfig.ThreadPoolTimerExecNumThreads);
            Assert.AreEqual(null, _config.EngineDefaults.ThreadingConfig.ThreadPoolInboundCapacity);
            Assert.AreEqual(null, _config.EngineDefaults.ThreadingConfig.ThreadPoolOutboundCapacity);
            Assert.AreEqual(null, _config.EngineDefaults.ThreadingConfig.ThreadPoolRouteExecCapacity);
            Assert.AreEqual(null, _config.EngineDefaults.ThreadingConfig.ThreadPoolTimerExecCapacity);
            Assert.IsFalse(_config.EngineDefaults.ThreadingConfig.IsEngineFairlock);
            //Assert.IsFalse(_config.EngineDefaults.MetricsReportingConfig.IsJmxEngineMetrics);
    
            Assert.AreEqual(PropertyResolutionStyle.CASE_SENSITIVE, _config.EngineDefaults.EventMetaConfig.ClassPropertyResolutionStyle);
            Assert.AreEqual(AccessorStyleEnum.NATIVE, _config.EngineDefaults.EventMetaConfig.DefaultAccessorStyle);
            Assert.AreEqual(EventRepresentation.MAP, _config.EngineDefaults.EventMetaConfig.DefaultEventRepresentation);
            Assert.AreEqual(5, _config.EngineDefaults.EventMetaConfig.AnonymousCacheSize);
    
            Assert.IsTrue(_config.EngineDefaults.ViewResourcesConfig.IsShareViews);
            Assert.IsFalse(_config.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies);
            Assert.IsFalse(_config.EngineDefaults.ViewResourcesConfig.IsIterableUnbound);
            Assert.IsFalse(_config.EngineDefaults.LoggingConfig.IsEnableExecutionDebug);
            Assert.IsTrue(_config.EngineDefaults.LoggingConfig.IsEnableTimerDebug);
            Assert.IsFalse(_config.EngineDefaults.LoggingConfig.IsEnableQueryPlan);
            Assert.IsFalse(_config.EngineDefaults.LoggingConfig.IsEnableADO);
            Assert.IsNull(_config.EngineDefaults.LoggingConfig.AuditPattern);
            Assert.AreEqual(15000, _config.EngineDefaults.VariablesConfig.MsecVersionRelease);
            Assert.AreEqual(null, _config.EngineDefaults.PatternsConfig.MaxSubexpressions);
            Assert.AreEqual(true, _config.EngineDefaults.PatternsConfig.IsMaxSubexpressionPreventStart);
            Assert.AreEqual(null, _config.EngineDefaults.MatchRecognizeConfig.MaxStates);
            Assert.AreEqual(true, _config.EngineDefaults.MatchRecognizeConfig.IsMaxStatesPreventStart);
            Assert.AreEqual(ConfigurationEngineDefaults.TimeSourceType.MILLI, _config.EngineDefaults.TimeSourceConfig.TimeSourceType);
            Assert.IsFalse(_config.EngineDefaults.ExecutionConfig.IsPrioritized);
            Assert.IsFalse(_config.EngineDefaults.ExecutionConfig.IsDisableLocking);
            Assert.IsFalse(_config.EngineDefaults.ExecutionConfig.IsAllowIsolatedService);
            Assert.AreEqual(ConfigurationEngineDefaults.ThreadingProfile.NORMAL, _config.EngineDefaults.ExecutionConfig.ThreadingProfile);
            Assert.AreEqual(ConfigurationEngineDefaults.FilterServiceProfile.READMOSTLY, _config.EngineDefaults.ExecutionConfig.FilterServiceProfile);
            Assert.AreEqual(16, _config.EngineDefaults.ExecutionConfig.FilterServiceMaxFilterWidth);

            Assert.AreEqual(StreamSelector.ISTREAM_ONLY, _config.EngineDefaults.StreamSelectionConfig.DefaultStreamSelector);
            Assert.IsFalse(_config.EngineDefaults.LanguageConfig.IsSortUsingCollator);
            Assert.IsFalse(_config.EngineDefaults.ExpressionConfig.IsIntegerDivision);
            Assert.IsFalse(_config.EngineDefaults.ExpressionConfig.IsDivisionByZeroReturnsNull);
            Assert.IsTrue(_config.EngineDefaults.ExpressionConfig.IsSelfSubselectPreeval);
            Assert.IsTrue(_config.EngineDefaults.ExpressionConfig.IsUdfCache);
            Assert.IsTrue(_config.EngineDefaults.ExpressionConfig.IsExtendedAggregation);
            Assert.IsFalse(_config.EngineDefaults.ExpressionConfig.IsDuckTyping);
            Assert.IsNull(_config.EngineDefaults.ExpressionConfig.MathContext);
            Assert.AreEqual(TimeZoneInfo.Local, _config.EngineDefaults.ExpressionConfig.TimeZone);
            Assert.IsNull(_config.EngineDefaults.ExceptionHandlingConfig.HandlerFactories);
            Assert.IsNull(_config.EngineDefaults.ConditionHandlingConfig.HandlerFactories);
            Assert.AreEqual("js", _config.EngineDefaults.ScriptsConfig.DefaultDialect);
    
            var domType = new ConfigurationEventTypeXMLDOM();
            Assert.IsFalse(domType.IsXPathPropertyExpr);
            Assert.IsTrue(domType.IsXPathResolvePropertiesAbsolute);
            Assert.IsTrue(domType.IsEventSenderValidatesRoot);
            Assert.IsTrue(domType.IsAutoFragment);
        }
    
        internal static void AssertFileConfig(Configuration config)
        {
            // assert name for class
            Assert.AreEqual(2, config.EventTypeAutoNamePackages.Count);
            Assert.AreEqual("com.mycompany.eventsone", config.EventTypeAutoNamePackages.ToArray()[0]);
            Assert.AreEqual("com.mycompany.eventstwo", config.EventTypeAutoNamePackages.ToArray()[1]);
    
            // assert name for class
            Assert.AreEqual(3, config.EventTypeNames.Count);
            Assert.AreEqual("com.mycompany.myapp.MySampleEventOne", config.EventTypeNames.Get("MySampleEventOne"));
            Assert.AreEqual("com.mycompany.myapp.MySampleEventTwo", config.EventTypeNames.Get("MySampleEventTwo"));
            Assert.AreEqual("com.mycompany.package.MyLegacyTypeEvent", config.EventTypeNames.Get("MyLegacyTypeEvent"));
    
            // assert auto imports
            Assert.AreEqual(10, config.Imports.Count);
            Assert.IsTrue(config.Imports.Contains(new AutoImportDesc(typeof(NameAttribute).Namespace)));
            Assert.IsTrue(config.Imports.Contains(new AutoImportDesc("com.espertech.esper.client.annotation")));
            Assert.IsTrue(config.Imports.Contains(new AutoImportDesc("com.espertech.esper.dataflow.ops")));
            Assert.IsTrue(config.Imports.Contains(new AutoImportDesc("com.mycompany.myapp")));
            Assert.IsTrue(config.Imports.Contains(new AutoImportDesc("com.mycompany.myapp.ClassOne")));
            Assert.IsTrue(config.Imports.Contains(new AutoImportDesc("com.mycompany.myapp", "AssemblyA")));
            Assert.IsTrue(config.Imports.Contains(new AutoImportDesc("com.mycompany.myapp", "AssemblyB.dll")));
            Assert.IsTrue(config.Imports.Contains(new AutoImportDesc("com.mycompany.myapp.ClassTwo", "AssemblyB.dll")));

            // assert XML DOM - no schema
            Assert.AreEqual(2, config.EventTypesXMLDOM.Count);
            var noSchemaDesc = config.EventTypesXMLDOM.Get("MyNoSchemaXMLEventName");
            Assert.AreEqual("MyNoSchemaEvent", noSchemaDesc.RootElementName);
            Assert.AreEqual("/myevent/element1", noSchemaDesc.XPathProperties.Get("element1").XPath);
            Assert.AreEqual(XPathResultType.Number, noSchemaDesc.XPathProperties.Get("element1").ResultType);
            Assert.AreEqual(null, noSchemaDesc.XPathProperties.Get("element1").OptionalCastToType);
            Assert.IsNull(noSchemaDesc.XPathFunctionResolver);
            Assert.IsNull(noSchemaDesc.XPathVariableResolver);
            Assert.IsFalse(noSchemaDesc.IsXPathPropertyExpr);
    
            // assert XML DOM - with schema
            var schemaDesc = config.EventTypesXMLDOM.Get("MySchemaXMLEventName");
            Assert.AreEqual("MySchemaEvent", schemaDesc.RootElementName);
            Assert.AreEqual("MySchemaXMLEvent.xsd", schemaDesc.SchemaResource);
            Assert.AreEqual("actual-xsd-text-here", schemaDesc.SchemaText);
            Assert.AreEqual("samples:schemas:simpleSchema", schemaDesc.RootElementNamespace);
            Assert.AreEqual("default-name-space", schemaDesc.DefaultNamespace);
            Assert.AreEqual("/myevent/element2", schemaDesc.XPathProperties.Get("element2").XPath);
            Assert.AreEqual(XPathResultType.String, schemaDesc.XPathProperties.Get("element2").ResultType);
            Assert.AreEqual(typeof(long), schemaDesc.XPathProperties.Get("element2").OptionalCastToType);
            Assert.AreEqual("/bookstore/book", schemaDesc.XPathProperties.Get("element3").XPath);
            Assert.AreEqual(XPathResultType.NodeSet, schemaDesc.XPathProperties.Get("element3").ResultType);
            Assert.AreEqual(null, schemaDesc.XPathProperties.Get("element3").OptionalCastToType);
            Assert.AreEqual("MyOtherXMLNodeEvent", schemaDesc.XPathProperties.Get("element3").OptionalEventTypeName);
            Assert.AreEqual(1, schemaDesc.NamespacePrefixes.Count);
            Assert.AreEqual("samples:schemas:simpleSchema", schemaDesc.NamespacePrefixes.Get("ss"));
            Assert.IsFalse(schemaDesc.IsXPathResolvePropertiesAbsolute);
            Assert.AreEqual("com.mycompany.OptionalFunctionResolver", schemaDesc.XPathFunctionResolver);
            Assert.AreEqual("com.mycompany.OptionalVariableResolver", schemaDesc.XPathVariableResolver);
            Assert.IsTrue(schemaDesc.IsXPathPropertyExpr);
            Assert.IsFalse(schemaDesc.IsEventSenderValidatesRoot);
            Assert.IsFalse(schemaDesc.IsAutoFragment);
            Assert.AreEqual("startts", schemaDesc.StartTimestampPropertyName);
            Assert.AreEqual("endts", schemaDesc.EndTimestampPropertyName);
    
            // assert mapped events
            Assert.AreEqual(1, config.EventTypesMapEvents.Count);
            Assert.IsTrue(config.EventTypesMapEvents.Keys.Contains("MyMapEvent"));

            Properties expectedProps = new Properties();
            expectedProps.Put("myInt", "int");
            expectedProps.Put("myString", "string");

            Assert.AreEqual(expectedProps, config.EventTypesMapEvents.Get("MyMapEvent"));
            Assert.AreEqual(1, config.MapTypeConfigurations.Count);
            var superTypes = config.MapTypeConfigurations.Get("MyMapEvent").SuperTypes;
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{"MyMapSuperType1", "MyMapSuperType2"}, superTypes.ToArray());
            Assert.AreEqual("startts", config.MapTypeConfigurations.Get("MyMapEvent").StartTimestampPropertyName);
            Assert.AreEqual("endts", config.MapTypeConfigurations.Get("MyMapEvent").EndTimestampPropertyName);
    
            // assert objectarray events
            Assert.AreEqual(1, config.EventTypesNestableObjectArrayEvents.Count);
            Assert.IsTrue(config.EventTypesNestableObjectArrayEvents.ContainsKey("MyObjectArrayEvent"));
            IDictionary<string, object> expectedPropsObjectArray = new Dictionary<string, object>();
            expectedPropsObjectArray.Put("myInt", "int");
            expectedPropsObjectArray.Put("myString", "string");
            Assert.That(config.EventTypesNestableObjectArrayEvents.Get("MyObjectArrayEvent"),
               Is.InstanceOf<IDictionary<string, object>>());
            Assert.That(config.EventTypesNestableObjectArrayEvents.Get("MyObjectArrayEvent").AsBasicDictionary<string, object>(),
               Is.EqualTo(expectedPropsObjectArray));

            Assert.AreEqual(expectedPropsObjectArray, config.EventTypesNestableObjectArrayEvents.Get("MyObjectArrayEvent"));
            Assert.AreEqual(1, config.ObjectArrayTypeConfigurations.Count);
            var superTypesOA = config.ObjectArrayTypeConfigurations.Get("MyObjectArrayEvent").SuperTypes;
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{"MyObjectArraySuperType1", "MyObjectArraySuperType2"}, superTypesOA.ToArray());
            Assert.AreEqual("startts", config.ObjectArrayTypeConfigurations.Get("MyObjectArrayEvent").StartTimestampPropertyName);
            Assert.AreEqual("endts", config.ObjectArrayTypeConfigurations.Get("MyObjectArrayEvent").EndTimestampPropertyName);
    
            // assert legacy type declaration
            Assert.AreEqual(1, config.EventTypesLegacy.Count);
            var legacy = config.EventTypesLegacy.Get("MyLegacyTypeEvent");
            Assert.AreEqual(CodeGenerationEnum.ENABLED, legacy.CodeGeneration);
            Assert.AreEqual(AccessorStyleEnum.PUBLIC, legacy.AccessorStyle);
            Assert.AreEqual(1, legacy.FieldProperties.Count);
            Assert.AreEqual("myFieldName", legacy.FieldProperties[0].AccessorFieldName);
            Assert.AreEqual("myfieldprop", legacy.FieldProperties[0].Name);
            Assert.AreEqual(1, legacy.MethodProperties.Count);
            Assert.AreEqual("myAccessorMethod", legacy.MethodProperties[0].AccessorMethodName);
            Assert.AreEqual("mymethodprop", legacy.MethodProperties[0].Name);
            Assert.AreEqual(PropertyResolutionStyle.CASE_INSENSITIVE, legacy.PropertyResolutionStyle);
            Assert.AreEqual("com.mycompany.myapp.MySampleEventFactory.createMyLegacyTypeEvent", legacy.FactoryMethod);
            Assert.AreEqual("myCopyMethod", legacy.CopyMethod);
            Assert.AreEqual("startts", legacy.StartTimestampPropertyName);
            Assert.AreEqual("endts", legacy.EndTimestampPropertyName);
    
            // assert database reference - data source config
            Assert.AreEqual(2, config.DatabaseReferences.Count);
            ConfigurationDBRef configDBRef = config.DatabaseReferences.Get("mydb1");
            DbDriverFactoryConnection dsDef = (DbDriverFactoryConnection)configDBRef.ConnectionFactoryDesc;

            Assert.AreEqual("com.espertech.esper.epl.db.drivers.DbDriverMySQL", dsDef.Driver.GetType().FullName);
            Assert.AreEqual("Server=localhost;Database=tempdb;Trusted_Connection=True;", dsDef.Driver.ConnectionString);
            Assert.AreEqual(ConnectionLifecycleEnum.POOLED, configDBRef.ConnectionLifecycle);

            Assert.IsNull(configDBRef.ConnectionSettings.AutoCommit);
            Assert.IsNull(configDBRef.ConnectionSettings.Catalog);
            Assert.IsNull(configDBRef.ConnectionSettings.TransactionIsolation);

            var lruCache = (ConfigurationLRUCache) configDBRef.DataCacheDesc;
            Assert.AreEqual(10, lruCache.Size);
            Assert.AreEqual(ConfigurationDBRef.ColumnChangeCaseEnum.LOWERCASE, configDBRef.ColumnChangeCase);
            Assert.AreEqual(ConfigurationDBRef.MetadataOriginEnum.SAMPLE, configDBRef.MetadataRetrievalEnum);
            //Assert.AreEqual(2, configDBRef.SqlTypesMapping.Count);
            //Assert.AreEqual("int", configDBRef.SqlTypesMapping[2]);
            //Assert.AreEqual("float", configDBRef.SqlTypesMapping[6]);
    
            // assert database reference - driver manager config
            configDBRef = config.DatabaseReferences.Get("mydb2");

            DbDriverFactoryConnection dmDef = (DbDriverFactoryConnection)configDBRef.ConnectionFactoryDesc;
            Assert.AreEqual("com.espertech.esper.epl.db.drivers.DbDriverODBC", dmDef.Driver.GetType().FullName);
            Assert.AreEqual(
               "Driver={MySQL ODBC 5.1 Driver};Server=localhost;Database=test;User=esper;Password=Esp3rP@ssw0rd;Option=3",
               dmDef.Driver.ConnectionString);

            Assert.AreEqual(ConnectionLifecycleEnum.RETAIN, configDBRef.ConnectionLifecycle);
            Assert.AreEqual(false, configDBRef.ConnectionSettings.AutoCommit);
            Assert.AreEqual("test", configDBRef.ConnectionSettings.Catalog);
            Assert.AreEqual(IsolationLevel.ReadCommitted, configDBRef.ConnectionSettings.TransactionIsolation);
            ConfigurationExpiryTimeCache expCache = (ConfigurationExpiryTimeCache)configDBRef.DataCacheDesc;

            Assert.AreEqual(60.5, expCache.MaxAgeSeconds);
            Assert.AreEqual(120.1, expCache.PurgeIntervalSeconds);
            Assert.AreEqual(ConfigurationCacheReferenceType.HARD, expCache.CacheReferenceType);
            Assert.AreEqual(ConfigurationDBRef.ColumnChangeCaseEnum.UPPERCASE, configDBRef.ColumnChangeCase);
            Assert.AreEqual(ConfigurationDBRef.MetadataOriginEnum.METADATA, configDBRef.MetadataRetrievalEnum);
            //Assert.AreEqual(1, configDBRef.SqlTypesMapping.Count);
            //Assert.AreEqual("System.String", configDBRef.SqlTypesMapping.Get(99));
    
            // assert custom view implementations
            var configViews = config.PlugInViews;
            Assert.AreEqual(2, configViews.Count);
            for (var i = 0; i < configViews.Count; i++)
            {
                var entry = configViews[i];
                Assert.AreEqual("ext" + i, entry.Namespace);
                Assert.AreEqual("myview" + i, entry.Name);
                Assert.AreEqual("com.mycompany.MyViewFactory" + i, entry.FactoryClassName);
            }
    
            // assert custom virtual data window implementations
            var configVDW = config.PlugInVirtualDataWindows;
            Assert.AreEqual(2, configVDW.Count);
            for (var i = 0; i < configVDW.Count; i++)
            {
                var entry = configVDW[i];
                Assert.AreEqual("vdw" + i, entry.Namespace);
                Assert.AreEqual("myvdw" + i, entry.Name);
                Assert.AreEqual("com.mycompany.MyVdwFactory" + i, entry.FactoryClassName);
                if (i == 1) {
                    Assert.AreEqual("abc", entry.Config);
                }
            }
    
            // assert adapter loaders parsed
            var plugins = config.PluginLoaders;
            Assert.AreEqual(2, plugins.Count);
            var pluginOne = plugins[0];
            Assert.AreEqual("Loader1", pluginOne.LoaderName);
            Assert.AreEqual("com.espertech.esper.support.plugin.SupportLoaderOne", pluginOne.TypeName);
            Assert.AreEqual(2, pluginOne.ConfigProperties.Count);
            Assert.AreEqual("val1", pluginOne.ConfigProperties.Get("name1"));
            Assert.AreEqual("val2", pluginOne.ConfigProperties.Get("name2"));
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-16\"?><sample-initializer xmlns=\"http://www.espertech.com/schema/esper\"><some-any-xml-can-be-here>This section for use by a plugin loader.</some-any-xml-can-be-here></sample-initializer>", pluginOne.ConfigurationXML);
    
            var pluginTwo = plugins[1];
            Assert.AreEqual("Loader2", pluginTwo.LoaderName);
            Assert.AreEqual("com.espertech.esper.support.plugin.SupportLoaderTwo", pluginTwo.TypeName);
            Assert.AreEqual(0, pluginTwo.ConfigProperties.Count);
    
            // assert plug-in aggregation function loaded
            Assert.AreEqual(2, config.PlugInAggregationFunctions.Count);
            var pluginAgg = config.PlugInAggregationFunctions[0];
            Assert.AreEqual("func1a", pluginAgg.Name);
            Assert.AreEqual("com.mycompany.MyMatrixAggregationMethod0Factory", pluginAgg.FactoryClassName);
            pluginAgg = config.PlugInAggregationFunctions[1];
            Assert.AreEqual("func2a", pluginAgg.Name);
            Assert.AreEqual("com.mycompany.MyMatrixAggregationMethod1Factory", pluginAgg.FactoryClassName);
    
            // assert plug-in aggregation multi-function loaded
            Assert.AreEqual(1, config.PlugInAggregationMultiFunctions.Count);
            var pluginMultiAgg = config.PlugInAggregationMultiFunctions[0];
            EPAssertionUtil.AssertEqualsExactOrder(new String[] {"func1", "func2"}, pluginMultiAgg.FunctionNames);
            Assert.AreEqual("com.mycompany.MyAggregationMultiFunctionFactory", pluginMultiAgg.MultiFunctionFactoryClassName);
            Assert.AreEqual(1, pluginMultiAgg.AdditionalConfiguredProperties.Count);
            Assert.AreEqual("value1", pluginMultiAgg.AdditionalConfiguredProperties.Get("prop1"));
    
            // assert plug-in singlerow function loaded
            Assert.AreEqual(2, config.PlugInSingleRowFunctions.Count);
            var pluginSingleRow = config.PlugInSingleRowFunctions[0];
            Assert.AreEqual("com.mycompany.MyMatrixSingleRowMethod0", pluginSingleRow.FunctionClassName);
            Assert.AreEqual("method1", pluginSingleRow.FunctionMethodName);
            Assert.AreEqual("func3", pluginSingleRow.Name);
            Assert.AreEqual(ValueCache.DISABLED, pluginSingleRow.ValueCache);
            Assert.AreEqual(FilterOptimizable.ENABLED, pluginSingleRow.FilterOptimizable);
            Assert.IsFalse(pluginSingleRow.RethrowExceptions);
            pluginSingleRow = config.PlugInSingleRowFunctions[1];
            Assert.AreEqual("com.mycompany.MyMatrixSingleRowMethod1", pluginSingleRow.FunctionClassName);
            Assert.AreEqual("func4", pluginSingleRow.Name);
            Assert.AreEqual("method2", pluginSingleRow.FunctionMethodName);
            Assert.AreEqual(ValueCache.ENABLED, pluginSingleRow.ValueCache);
            Assert.AreEqual(FilterOptimizable.DISABLED, pluginSingleRow.FilterOptimizable);
            Assert.IsTrue(pluginSingleRow.RethrowExceptions);
    
            // assert plug-in guard objects loaded
            Assert.AreEqual(4, config.PlugInPatternObjects.Count);
            var pluginPattern = config.PlugInPatternObjects[0];
            Assert.AreEqual("com.mycompany.MyGuardFactory0", pluginPattern.FactoryClassName);
            Assert.AreEqual("ext0", pluginPattern.Namespace);
            Assert.AreEqual("guard1", pluginPattern.Name);
            Assert.AreEqual(ConfigurationPlugInPatternObject.PatternObjectTypeEnum.GUARD, pluginPattern.PatternObjectType);
            pluginPattern = config.PlugInPatternObjects[1];
            Assert.AreEqual("com.mycompany.MyGuardFactory1", pluginPattern.FactoryClassName);
            Assert.AreEqual("ext1", pluginPattern.Namespace);
            Assert.AreEqual("guard2", pluginPattern.Name);
            Assert.AreEqual(ConfigurationPlugInPatternObject.PatternObjectTypeEnum.GUARD, pluginPattern.PatternObjectType);
            pluginPattern = config.PlugInPatternObjects[2];
            Assert.AreEqual("com.mycompany.MyObserverFactory0", pluginPattern.FactoryClassName);
            Assert.AreEqual("ext0", pluginPattern.Namespace);
            Assert.AreEqual("observer1", pluginPattern.Name);
            Assert.AreEqual(ConfigurationPlugInPatternObject.PatternObjectTypeEnum.OBSERVER, pluginPattern.PatternObjectType);
            pluginPattern = config.PlugInPatternObjects[3];
            Assert.AreEqual("com.mycompany.MyObserverFactory1", pluginPattern.FactoryClassName);
            Assert.AreEqual("ext1", pluginPattern.Namespace);
            Assert.AreEqual("observer2", pluginPattern.Name);
            Assert.AreEqual(ConfigurationPlugInPatternObject.PatternObjectTypeEnum.OBSERVER, pluginPattern.PatternObjectType);
    
            // assert engine defaults
            Assert.IsFalse(config.EngineDefaults.ThreadingConfig.IsInsertIntoDispatchPreserveOrder);
            Assert.AreEqual(3000, config.EngineDefaults.ThreadingConfig.InsertIntoDispatchTimeout);
            Assert.AreEqual(ConfigurationEngineDefaults.Threading.Locking.SUSPEND, config.EngineDefaults.ThreadingConfig.InsertIntoDispatchLocking);
    
            Assert.IsFalse(config.EngineDefaults.ThreadingConfig.IsListenerDispatchPreserveOrder);
            Assert.AreEqual(2000, config.EngineDefaults.ThreadingConfig.ListenerDispatchTimeout);
            Assert.AreEqual(ConfigurationEngineDefaults.Threading.Locking.SUSPEND, config.EngineDefaults.ThreadingConfig.ListenerDispatchLocking);
            Assert.IsTrue(config.EngineDefaults.ThreadingConfig.IsThreadPoolInbound);
            Assert.IsTrue(config.EngineDefaults.ThreadingConfig.IsThreadPoolOutbound);
            Assert.IsTrue(config.EngineDefaults.ThreadingConfig.IsThreadPoolRouteExec);
            Assert.IsTrue(config.EngineDefaults.ThreadingConfig.IsThreadPoolTimerExec);
            Assert.AreEqual(1, config.EngineDefaults.ThreadingConfig.ThreadPoolInboundNumThreads);
            Assert.AreEqual(2, config.EngineDefaults.ThreadingConfig.ThreadPoolOutboundNumThreads);
            Assert.AreEqual(3, config.EngineDefaults.ThreadingConfig.ThreadPoolTimerExecNumThreads);
            Assert.AreEqual(4, config.EngineDefaults.ThreadingConfig.ThreadPoolRouteExecNumThreads);
            Assert.AreEqual(1000, (int) config.EngineDefaults.ThreadingConfig.ThreadPoolInboundCapacity);
            Assert.AreEqual(1500, (int) config.EngineDefaults.ThreadingConfig.ThreadPoolOutboundCapacity);
            Assert.AreEqual(null, config.EngineDefaults.ThreadingConfig.ThreadPoolTimerExecCapacity);
            Assert.AreEqual(2000, (int) config.EngineDefaults.ThreadingConfig.ThreadPoolRouteExecCapacity);
    
            Assert.IsFalse(config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled);
            Assert.AreEqual(1234567, config.EngineDefaults.ThreadingConfig.InternalTimerMsecResolution);
            Assert.IsFalse(config.EngineDefaults.ViewResourcesConfig.IsShareViews);
            Assert.IsTrue(config.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies);
            Assert.IsTrue(config.EngineDefaults.ViewResourcesConfig.IsIterableUnbound);
            Assert.AreEqual(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE, config.EngineDefaults.EventMetaConfig.ClassPropertyResolutionStyle);
            Assert.AreEqual(AccessorStyleEnum.PUBLIC, config.EngineDefaults.EventMetaConfig.DefaultAccessorStyle);
            Assert.AreEqual(EventRepresentation.MAP, config.EngineDefaults.EventMetaConfig.DefaultEventRepresentation);
            Assert.AreEqual(100, config.EngineDefaults.EventMetaConfig.AnonymousCacheSize);
            Assert.IsTrue(config.EngineDefaults.LoggingConfig.IsEnableExecutionDebug);
            Assert.IsFalse(config.EngineDefaults.LoggingConfig.IsEnableTimerDebug);
            Assert.IsTrue(config.EngineDefaults.LoggingConfig.IsEnableQueryPlan);
            Assert.IsTrue(config.EngineDefaults.LoggingConfig.IsEnableADO);
            Assert.AreEqual("[%u] %m", config.EngineDefaults.LoggingConfig.AuditPattern);
            Assert.AreEqual(30000, config.EngineDefaults.VariablesConfig.MsecVersionRelease);
            Assert.AreEqual(3L, (long) config.EngineDefaults.PatternsConfig.MaxSubexpressions);
            Assert.AreEqual(false, config.EngineDefaults.PatternsConfig.IsMaxSubexpressionPreventStart);
            Assert.AreEqual(3L, (long)config.EngineDefaults.MatchRecognizeConfig.MaxStates);
            Assert.AreEqual(false, config.EngineDefaults.MatchRecognizeConfig.IsMaxStatesPreventStart);
            Assert.AreEqual(StreamSelector.RSTREAM_ISTREAM_BOTH, config.EngineDefaults.StreamSelectionConfig.DefaultStreamSelector);
    
            Assert.AreEqual(ConfigurationEngineDefaults.TimeSourceType.NANO, config.EngineDefaults.TimeSourceConfig.TimeSourceType);
            Assert.IsTrue(config.EngineDefaults.ExecutionConfig.IsPrioritized);
            Assert.IsTrue(config.EngineDefaults.ExecutionConfig.IsFairlock);
            Assert.IsTrue(config.EngineDefaults.ExecutionConfig.IsDisableLocking);
            Assert.IsTrue(config.EngineDefaults.ExecutionConfig.IsAllowIsolatedService);
            Assert.AreEqual(ConfigurationEngineDefaults.ThreadingProfile.LARGE, config.EngineDefaults.ExecutionConfig.ThreadingProfile);
            Assert.AreEqual(ConfigurationEngineDefaults.FilterServiceProfile.READWRITE, config.EngineDefaults.ExecutionConfig.FilterServiceProfile);
            Assert.AreEqual(100, config.EngineDefaults.ExecutionConfig.FilterServiceMaxFilterWidth);
    
            var metrics = config.EngineDefaults.MetricsReportingConfig;
            Assert.IsTrue(metrics.IsEnableMetricsReporting);
            Assert.AreEqual(4000L, metrics.EngineInterval);
            Assert.AreEqual(500L, metrics.StatementInterval);
            Assert.IsFalse(metrics.IsThreading);
            Assert.AreEqual(2, metrics.StatementGroups.Count);
            //Assert.IsTrue(metrics.IsJmxEngineMetrics);
            var def = metrics.StatementGroups.Get("MyStmtGroup");
            Assert.AreEqual(5000, def.Interval);
            Assert.IsTrue(def.IsDefaultInclude);
            Assert.AreEqual(50, def.NumStatements);
            Assert.IsTrue(def.IsReportInactive);
            Assert.AreEqual(5, def.Patterns.Count);
            Assert.AreEqual(def.Patterns[0], new Pair<StringPatternSet, Boolean>(new StringPatternSetRegex(".*"), true));
            Assert.AreEqual(def.Patterns[1], new Pair<StringPatternSet, Boolean>(new StringPatternSetRegex(".*test.*"), false));
            Assert.AreEqual(def.Patterns[2], new Pair<StringPatternSet, Boolean>(new StringPatternSetLike("%MyMetricsStatement%"), false));
            Assert.AreEqual(def.Patterns[3], new Pair<StringPatternSet, Boolean>(new StringPatternSetLike("%MyFraudAnalysisStatement%"), true));
            Assert.AreEqual(def.Patterns[4], new Pair<StringPatternSet, Boolean>(new StringPatternSetLike("%SomerOtherStatement%"), true));
            def = metrics.StatementGroups.Get("MyStmtGroupTwo");
            Assert.AreEqual(200, def.Interval);
            Assert.IsFalse(def.IsDefaultInclude);
            Assert.AreEqual(100, def.NumStatements);
            Assert.IsFalse(def.IsReportInactive);
            Assert.AreEqual(0, def.Patterns.Count);
            Assert.IsTrue(config.EngineDefaults.LanguageConfig.IsSortUsingCollator);
            Assert.IsTrue(config.EngineDefaults.ExpressionConfig.IsIntegerDivision);
            Assert.IsTrue(config.EngineDefaults.ExpressionConfig.IsDivisionByZeroReturnsNull);
            Assert.IsFalse(config.EngineDefaults.ExpressionConfig.IsSelfSubselectPreeval);
            Assert.IsFalse(config.EngineDefaults.ExpressionConfig.IsUdfCache);
            Assert.IsFalse(config.EngineDefaults.ExpressionConfig.IsExtendedAggregation);
            Assert.IsTrue(config.EngineDefaults.ExpressionConfig.IsDuckTyping);
            Assert.AreEqual(2, config.EngineDefaults.ExpressionConfig.MathContext.Precision);
            Assert.AreEqual(MidpointRounding.ToEven, config.EngineDefaults.ExpressionConfig.MathContext.RoundingMode);
            Assert.AreEqual(TimeZoneHelper.GetTimeZoneInfo("GMT-4:00"), config.EngineDefaults.ExpressionConfig.TimeZone);
            Assert.AreEqual(2, config.EngineDefaults.ExceptionHandlingConfig.HandlerFactories.Count);
            Assert.AreEqual("my.company.cep.LoggingExceptionHandlerFactory", config.EngineDefaults.ExceptionHandlingConfig.HandlerFactories[0]);
            Assert.AreEqual("my.company.cep.AlertExceptionHandlerFactory", config.EngineDefaults.ExceptionHandlingConfig.HandlerFactories[1]);
            Assert.AreEqual(2, config.EngineDefaults.ConditionHandlingConfig.HandlerFactories.Count);
            Assert.AreEqual("my.company.cep.LoggingConditionHandlerFactory", config.EngineDefaults.ConditionHandlingConfig.HandlerFactories[0]);
            Assert.AreEqual("my.company.cep.AlertConditionHandlerFactory", config.EngineDefaults.ConditionHandlingConfig.HandlerFactories[1]);
            Assert.AreEqual("abc", config.EngineDefaults.ScriptsConfig.DefaultDialect);
    
            // variables
            Assert.AreEqual(3, config.Variables.Count);
            var variable = config.Variables.Get("var1");
            Assert.AreEqual(typeof(int).FullName, variable.VariableType);
            Assert.AreEqual("1", variable.InitializationValue);
            Assert.IsFalse(variable.IsConstant);
            variable = config.Variables.Get("var2");
            Assert.AreEqual(typeof(string).FullName, variable.VariableType);
            Assert.AreEqual(null, variable.InitializationValue);
            Assert.IsFalse(variable.IsConstant);
            variable = config.Variables.Get("var3");
            Assert.IsTrue(variable.IsConstant);
    
            // method references
            Assert.AreEqual(2, config.MethodInvocationReferences.Count);
            var methodRef = config.MethodInvocationReferences.Get("abc");
            expCache = (ConfigurationExpiryTimeCache) methodRef.DataCacheDesc;
            Assert.AreEqual(91.0, expCache.MaxAgeSeconds);
            Assert.AreEqual(92.2, expCache.PurgeIntervalSeconds);
            Assert.AreEqual(ConfigurationCacheReferenceType.WEAK, expCache.CacheReferenceType);
    
            methodRef = config.MethodInvocationReferences.Get("def");
            lruCache = (ConfigurationLRUCache) methodRef.DataCacheDesc;
            Assert.AreEqual(20, lruCache.Size);
    
            // plug-in event representations
            Assert.AreEqual(2, config.PlugInEventRepresentation.Count);
            var rep = config.PlugInEventRepresentation.Get(new Uri("type://format/rep/name"));
            Assert.AreEqual("com.mycompany.MyPlugInEventRepresentation", rep.EventRepresentationTypeName);
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-16\"?><anyxml>test string event rep init</anyxml>", rep.Initializer);
            rep = config.PlugInEventRepresentation.Get(new Uri("type://format/rep/name2"));
            Assert.AreEqual("com.mycompany.MyPlugInEventRepresentation2", rep.EventRepresentationTypeName);
            Assert.AreEqual(null, rep.Initializer);
    
            // plug-in event types
            Assert.AreEqual(2, config.PlugInEventTypes.Count);
            var type = config.PlugInEventTypes.Get("MyEvent");
            Assert.AreEqual(2, type.EventRepresentationResolutionURIs.Count);
            Assert.AreEqual("type://format/rep", type.EventRepresentationResolutionURIs[0].ToString());
            Assert.AreEqual("type://format/rep2", type.EventRepresentationResolutionURIs[1].ToString());
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-16\"?><anyxml>test string event type init</anyxml>", type.Initializer);
            type = config.PlugInEventTypes.Get("MyEvent2");
            Assert.AreEqual(1, type.EventRepresentationResolutionURIs.Count);
            Assert.AreEqual("type://format/rep2", type.EventRepresentationResolutionURIs[0].ToString());
            Assert.AreEqual(null, type.Initializer);
    
            // plug-in event representation resolution URIs when using a new name in a statement
            Assert.AreEqual(2, config.PlugInEventTypeResolutionURIs.Count);
            Assert.AreEqual("type://format/rep", config.PlugInEventTypeResolutionURIs[0].ToString());
            Assert.AreEqual("type://format/rep2", config.PlugInEventTypeResolutionURIs[1].ToString());
    
            // revision types
            Assert.AreEqual(1, config.RevisionEventTypes.Count);
            var configRev = config.RevisionEventTypes.Get("MyRevisionEvent");
            Assert.AreEqual(1, configRev.NameBaseEventTypes.Count);
            Assert.IsTrue(configRev.NameBaseEventTypes.Contains("MyBaseEventName"));
            Assert.IsTrue(configRev.NameDeltaEventTypes.Contains("MyDeltaEventNameOne"));
            Assert.IsTrue(configRev.NameDeltaEventTypes.Contains("MyDeltaEventNameTwo"));
            EPAssertionUtil.AssertEqualsAnyOrder(new String[]{"id", "id2"}, configRev.KeyPropertyNames);
            Assert.AreEqual(PropertyRevisionEnum.MERGE_NON_NULL, configRev.PropertyRevision);
    
            // variance types
            Assert.AreEqual(1, config.VariantStreams.Count);
            var configVStream = config.VariantStreams.Get("MyVariantStream");
            Assert.AreEqual(2, configVStream.VariantTypeNames.Count);
            Assert.IsTrue(configVStream.VariantTypeNames.Contains("MyEvenTypetNameOne"));
            Assert.IsTrue(configVStream.VariantTypeNames.Contains("MyEvenTypetNameTwo"));
            Assert.AreEqual(TypeVarianceEnum.ANY, configVStream.TypeVariance);
        }
    }

    public static class TransformExtensions
    {
        public static IDictionary<K, V> AsBasicDictionary<K, V>(this object anyEntity)
        {
            var asRawDictionary = anyEntity as Dictionary<K, V>;
            if (asRawDictionary != null)
                return asRawDictionary;

            var asFuzzyDictionary = anyEntity as IDictionary<K, V>;
            if (asFuzzyDictionary != null)
                return new Dictionary<K, V>(asFuzzyDictionary);

            throw new ArgumentException("unable to translate dictionary", "anyEntity");
        }
    }
}
