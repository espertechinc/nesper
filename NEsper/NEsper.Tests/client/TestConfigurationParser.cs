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
using System.Linq;
using System.Net;
using System.Xml.XPath;

using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.util;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.type;

using NUnit.Framework;

namespace com.espertech.esper.client
{
    [TestFixture]
    public class TestConfigurationParser 
    {
        private Configuration _config;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _config = new Configuration(_container);
        }
    
        [Test]
        public void TestConfigureFromStream()
        {
            var uri = _container.Resolve<IResourceManager>().ResolveResourceURL(TestConfiguration.ESPER_TEST_CONFIG);
            var client = new WebClient();
            using (var stream = client.OpenRead(uri))
            {
                _container.Resolve<IConfigurationParser>()
                    .DoConfigure(_config, stream, uri.ToString());
                AssertFileConfig(_config);
            }
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

            Assert.That(config.AnnotationImports.Count, Is.EqualTo(2));
            Assert.That(config.AnnotationImports, Contains.Item(new AutoImportDesc("com.mycompany.myapp.annotations")));
            Assert.That(config.AnnotationImports, Contains.Item(new AutoImportDesc("com.mycompany.myapp.annotations.ClassOne")));

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

            var expectedProps = new Properties();
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

            // assert avro events
            Assert.AreEqual(2, config.EventTypesAvro.Count);
            var avroOne = config.EventTypesAvro.Get("MyAvroEvent");
            Assert.AreEqual("{\"type\":\"record\",\"name\":\"typename\",\"fields\":[{\"name\":\"num\",\"type\":\"int\"}]}", avroOne.AvroSchemaText);
            Assert.IsNull(avroOne.AvroSchema);
            Assert.IsNull(avroOne.StartTimestampPropertyName);
            Assert.IsNull(avroOne.EndTimestampPropertyName);
            Assert.IsTrue(avroOne.SuperTypes.IsEmpty());
            var avroTwo = config.EventTypesAvro.Get("MyAvroEventTwo");
            Assert.AreEqual("{\"type\":\"record\",\"name\":\"MyAvroEvent\",\"fields\":[{\"name\":\"carId\",\"type\":\"int\"},{\"name\":\"carType\",\"type\":{\"type\":\"string\",\"avro.string\":\"string\"}}]}", avroTwo.AvroSchemaText);
            Assert.AreEqual("startts", avroTwo.StartTimestampPropertyName);
            Assert.AreEqual("endts", avroTwo.EndTimestampPropertyName);
            Assert.AreEqual("[SomeSuperAvro, SomeSuperAvroTwo]", CompatExtensions.Render(avroTwo.SuperTypes));

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
            var configDBRef = config.DatabaseReferences.Get("mydb1");
            var dsDef = (DbDriverFactoryConnection)configDBRef.ConnectionFactoryDesc;

            Assert.AreEqual("com.espertech.esper.epl.db.drivers.DbDriverPgSQL", dsDef.Driver.GetType().FullName);
            Assert.AreEqual("Host=nesper-pgsql-integ.local;Database=test;Username=esper;Password=3sp3rP@ssw0rd;", dsDef.Driver.ConnectionString);
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

            var dmDef = (DbDriverFactoryConnection)configDBRef.ConnectionFactoryDesc;
            Assert.AreEqual("com.espertech.esper.epl.db.drivers.DbDriverPgSQL", dmDef.Driver.GetType().FullName);
            Assert.AreEqual("Host=nesper-pgsql-integ.local;Database=test;Username=esper;Password=3sp3rP@ssw0rd;", dmDef.Driver.ConnectionString);

            Assert.AreEqual(ConnectionLifecycleEnum.RETAIN, configDBRef.ConnectionLifecycle);
            Assert.AreEqual(false, configDBRef.ConnectionSettings.AutoCommit);
            Assert.AreEqual("test", configDBRef.ConnectionSettings.Catalog);
            Assert.AreEqual(IsolationLevel.ReadCommitted, configDBRef.ConnectionSettings.TransactionIsolation);
            var expCache = (ConfigurationExpiryTimeCache)configDBRef.DataCacheDesc;

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
            Assert.AreEqual(ValueCacheEnum.DISABLED, pluginSingleRow.ValueCache);
            Assert.AreEqual(FilterOptimizableEnum.ENABLED, pluginSingleRow.FilterOptimizable);
            Assert.IsFalse(pluginSingleRow.IsRethrowExceptions);
            pluginSingleRow = config.PlugInSingleRowFunctions[1];
            Assert.AreEqual("com.mycompany.MyMatrixSingleRowMethod1", pluginSingleRow.FunctionClassName);
            Assert.AreEqual("func4", pluginSingleRow.Name);
            Assert.AreEqual("method2", pluginSingleRow.FunctionMethodName);
            Assert.AreEqual(ValueCacheEnum.ENABLED, pluginSingleRow.ValueCache);
            Assert.AreEqual(FilterOptimizableEnum.DISABLED, pluginSingleRow.FilterOptimizable);
            Assert.IsTrue(pluginSingleRow.IsRethrowExceptions);
            Assert.AreEqual("XYZEventTypeName", pluginSingleRow.EventTypeName);

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
            Assert.IsFalse(config.EngineDefaults.Threading.IsInsertIntoDispatchPreserveOrder);
            Assert.AreEqual(3000, config.EngineDefaults.Threading.InsertIntoDispatchTimeout);
            Assert.AreEqual(ConfigurationEngineDefaults.ThreadingConfig.Locking.SUSPEND, config.EngineDefaults.Threading.InsertIntoDispatchLocking);

            Assert.IsFalse(config.EngineDefaults.Threading.IsNamedWindowConsumerDispatchPreserveOrder);
            Assert.AreEqual(4000, config.EngineDefaults.Threading.NamedWindowConsumerDispatchTimeout);
            Assert.AreEqual(ConfigurationEngineDefaults.ThreadingConfig.Locking.SUSPEND, config.EngineDefaults.Threading.NamedWindowConsumerDispatchLocking);

            Assert.IsFalse(config.EngineDefaults.Threading.IsListenerDispatchPreserveOrder);
            Assert.AreEqual(2000, config.EngineDefaults.Threading.ListenerDispatchTimeout);
            Assert.AreEqual(ConfigurationEngineDefaults.ThreadingConfig.Locking.SUSPEND, config.EngineDefaults.Threading.ListenerDispatchLocking);
            Assert.IsTrue(config.EngineDefaults.Threading.IsThreadPoolInbound);
            Assert.IsTrue(config.EngineDefaults.Threading.IsThreadPoolOutbound);
            Assert.IsTrue(config.EngineDefaults.Threading.IsThreadPoolRouteExec);
            Assert.IsTrue(config.EngineDefaults.Threading.IsThreadPoolTimerExec);
            Assert.AreEqual(1, config.EngineDefaults.Threading.ThreadPoolInboundNumThreads);
            Assert.AreEqual(2, config.EngineDefaults.Threading.ThreadPoolOutboundNumThreads);
            Assert.AreEqual(3, config.EngineDefaults.Threading.ThreadPoolTimerExecNumThreads);
            Assert.AreEqual(4, config.EngineDefaults.Threading.ThreadPoolRouteExecNumThreads);
            Assert.AreEqual(1000, (int) config.EngineDefaults.Threading.ThreadPoolInboundCapacity);
            Assert.AreEqual(1500, (int) config.EngineDefaults.Threading.ThreadPoolOutboundCapacity);
            Assert.AreEqual(null, config.EngineDefaults.Threading.ThreadPoolTimerExecCapacity);
            Assert.AreEqual(2000, (int) config.EngineDefaults.Threading.ThreadPoolRouteExecCapacity);
    
            Assert.IsFalse(config.EngineDefaults.Threading.IsInternalTimerEnabled);
            Assert.AreEqual(1234567, config.EngineDefaults.Threading.InternalTimerMsecResolution);
            Assert.IsTrue(config.EngineDefaults.ViewResources.IsShareViews);
            Assert.IsTrue(config.EngineDefaults.ViewResources.IsAllowMultipleExpiryPolicies);
            Assert.IsTrue(config.EngineDefaults.ViewResources.IsIterableUnbound);
            Assert.AreEqual(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE, config.EngineDefaults.EventMeta.ClassPropertyResolutionStyle);
            Assert.AreEqual(AccessorStyleEnum.PUBLIC, config.EngineDefaults.EventMeta.DefaultAccessorStyle);
            Assert.AreEqual(EventUnderlyingType.MAP, config.EngineDefaults.EventMeta.DefaultEventRepresentation);
            Assert.AreEqual(100, config.EngineDefaults.EventMeta.AnonymousCacheSize);
            Assert.IsFalse(config.EngineDefaults.EventMeta.AvroSettings.IsEnableAvro);
            Assert.IsFalse(config.EngineDefaults.EventMeta.AvroSettings.IsEnableNativeString);
            Assert.IsFalse(config.EngineDefaults.EventMeta.AvroSettings.IsEnableSchemaDefaultNonNull);
            Assert.AreEqual("myObjectValueTypeWidenerFactoryClass", config.EngineDefaults.EventMeta.AvroSettings.ObjectValueTypeWidenerFactoryClass);
            Assert.AreEqual("myTypeToRepresentationMapperClass", config.EngineDefaults.EventMeta.AvroSettings.TypeRepresentationMapperClass);
            Assert.IsTrue(config.EngineDefaults.Logging.IsEnableExecutionDebug);
            Assert.IsFalse(config.EngineDefaults.Logging.IsEnableTimerDebug);
            Assert.IsTrue(config.EngineDefaults.Logging.IsEnableQueryPlan);
            Assert.IsTrue(config.EngineDefaults.Logging.IsEnableADO);
            Assert.AreEqual("[%u] %m", config.EngineDefaults.Logging.AuditPattern);
            Assert.AreEqual(30000, config.EngineDefaults.Variables.MsecVersionRelease);
            Assert.AreEqual(3L, (long) config.EngineDefaults.Patterns.MaxSubexpressions);
            Assert.AreEqual(false, config.EngineDefaults.Patterns.IsMaxSubexpressionPreventStart);
            Assert.AreEqual(3L, (long)config.EngineDefaults.MatchRecognize.MaxStates);
            Assert.AreEqual(false, config.EngineDefaults.MatchRecognize.IsMaxStatesPreventStart);
            Assert.AreEqual(StreamSelector.RSTREAM_ISTREAM_BOTH, config.EngineDefaults.StreamSelection.DefaultStreamSelector);
    
            Assert.AreEqual(ConfigurationEngineDefaults.TimeSourceType.NANO, config.EngineDefaults.TimeSource.TimeSourceType);
            Assert.AreEqual(TimeUnit.MICROSECONDS, config.EngineDefaults.TimeSource.TimeUnit);
            Assert.IsTrue(config.EngineDefaults.Execution.IsPrioritized);
            Assert.IsTrue(config.EngineDefaults.Execution.IsFairlock);
            Assert.IsTrue(config.EngineDefaults.Execution.IsDisableLocking);
            Assert.IsTrue(config.EngineDefaults.Execution.IsAllowIsolatedService);
            Assert.AreEqual(ConfigurationEngineDefaults.ThreadingProfile.LARGE, config.EngineDefaults.Execution.ThreadingProfile);
            Assert.AreEqual(ConfigurationEngineDefaults.FilterServiceProfile.READWRITE, config.EngineDefaults.Execution.FilterServiceProfile);
            Assert.AreEqual(100, config.EngineDefaults.Execution.FilterServiceMaxFilterWidth);
    
            var metrics = config.EngineDefaults.MetricsReporting;
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
            Assert.IsTrue(config.EngineDefaults.Language.IsSortUsingCollator);
            Assert.IsTrue(config.EngineDefaults.Expression.IsIntegerDivision);
            Assert.IsTrue(config.EngineDefaults.Expression.IsDivisionByZeroReturnsNull);
            Assert.IsFalse(config.EngineDefaults.Expression.IsSelfSubselectPreeval);
            Assert.IsFalse(config.EngineDefaults.Expression.IsUdfCache);
            Assert.IsFalse(config.EngineDefaults.Expression.IsExtendedAggregation);
            Assert.IsTrue(config.EngineDefaults.Expression.IsDuckTyping);
            Assert.AreEqual(2, config.EngineDefaults.Expression.MathContext.Precision);
            Assert.AreEqual(MidpointRounding.ToEven, config.EngineDefaults.Expression.MathContext.RoundingMode);
            Assert.AreEqual(TimeZoneHelper.GetTimeZoneInfo("GMT-4:00"), config.EngineDefaults.Expression.TimeZone);
            Assert.AreEqual(2, config.EngineDefaults.ExceptionHandling.HandlerFactories.Count);
            Assert.AreEqual("my.company.cep.LoggingExceptionHandlerFactory", config.EngineDefaults.ExceptionHandling.HandlerFactories[0]);
            Assert.AreEqual("my.company.cep.AlertExceptionHandlerFactory", config.EngineDefaults.ExceptionHandling.HandlerFactories[1]);
            Assert.AreEqual(2, config.EngineDefaults.ConditionHandling.HandlerFactories.Count);
            Assert.AreEqual("my.company.cep.LoggingConditionHandlerFactory", config.EngineDefaults.ConditionHandling.HandlerFactories[0]);
            Assert.AreEqual("my.company.cep.AlertConditionHandlerFactory", config.EngineDefaults.ConditionHandling.HandlerFactories[1]);
            Assert.AreEqual("abc", config.EngineDefaults.Scripts.DefaultDialect);

            Assert.AreEqual(ConfigurationEngineDefaults.UndeployRethrowPolicy.RETHROW_FIRST, config.EngineDefaults.ExceptionHandling.UndeployRethrowPolicy);

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

        [Test]
        public void TestRegressionFileConfig() 
        {
            var config = new Configuration(_container);
            var uri = _container.Resolve<IResourceManager>().ResolveResourceURL(TestConfiguration.ESPER_TEST_CONFIG);
            var client = new WebClient();
            using (var stream = client.OpenRead(uri))
            {
                _container.Resolve<IConfigurationParser>().DoConfigure(config, stream, uri.ToString());
                AssertFileConfig(config);
            }
        }

        [Test]
        public void TestEngineDefaults()
        {
            var config = new Configuration(_container);

            Assert.IsTrue(config.EngineDefaults.Threading.IsInsertIntoDispatchPreserveOrder);
            Assert.AreEqual(100, config.EngineDefaults.Threading.InsertIntoDispatchTimeout);
            Assert.IsTrue(config.EngineDefaults.Threading.IsListenerDispatchPreserveOrder);
            Assert.AreEqual(1000, config.EngineDefaults.Threading.ListenerDispatchTimeout);
            Assert.IsTrue(config.EngineDefaults.Threading.IsInternalTimerEnabled);
            Assert.AreEqual(100, config.EngineDefaults.Threading.InternalTimerMsecResolution);
            Assert.AreEqual(ConfigurationEngineDefaults.ThreadingConfig.Locking.SPIN, config.EngineDefaults.Threading.InsertIntoDispatchLocking);
            Assert.AreEqual(ConfigurationEngineDefaults.ThreadingConfig.Locking.SPIN, config.EngineDefaults.Threading.ListenerDispatchLocking);
            Assert.IsFalse(config.EngineDefaults.Threading.IsThreadPoolInbound);
            Assert.IsFalse(config.EngineDefaults.Threading.IsThreadPoolOutbound);
            Assert.IsFalse(config.EngineDefaults.Threading.IsThreadPoolRouteExec);
            Assert.IsFalse(config.EngineDefaults.Threading.IsThreadPoolTimerExec);
            Assert.AreEqual(2, config.EngineDefaults.Threading.ThreadPoolInboundNumThreads);
            Assert.AreEqual(2, config.EngineDefaults.Threading.ThreadPoolOutboundNumThreads);
            Assert.AreEqual(2, config.EngineDefaults.Threading.ThreadPoolRouteExecNumThreads);
            Assert.AreEqual(2, config.EngineDefaults.Threading.ThreadPoolTimerExecNumThreads);
            Assert.AreEqual(null, config.EngineDefaults.Threading.ThreadPoolInboundCapacity);
            Assert.AreEqual(null, config.EngineDefaults.Threading.ThreadPoolOutboundCapacity);
            Assert.AreEqual(null, config.EngineDefaults.Threading.ThreadPoolRouteExecCapacity);
            Assert.AreEqual(null, config.EngineDefaults.Threading.ThreadPoolTimerExecCapacity);
            Assert.IsFalse(config.EngineDefaults.Threading.IsEngineFairlock);
            Assert.IsFalse(config.EngineDefaults.MetricsReporting.IsEnableMetricsReporting);
            Assert.IsTrue(config.EngineDefaults.Threading.IsNamedWindowConsumerDispatchPreserveOrder);
            Assert.AreEqual(Int64.MaxValue, config.EngineDefaults.Threading.NamedWindowConsumerDispatchTimeout);
            Assert.AreEqual(ConfigurationEngineDefaults.ThreadingConfig.Locking.SPIN, config.EngineDefaults.Threading.NamedWindowConsumerDispatchLocking);

            Assert.AreEqual(PropertyResolutionStyle.CASE_SENSITIVE, config.EngineDefaults.EventMeta.ClassPropertyResolutionStyle);
            Assert.AreEqual(AccessorStyleEnum.NATIVE, config.EngineDefaults.EventMeta.DefaultAccessorStyle);
            Assert.AreEqual(EventUnderlyingType.MAP, config.EngineDefaults.EventMeta.DefaultEventRepresentation);
            Assert.AreEqual(5, config.EngineDefaults.EventMeta.AnonymousCacheSize);
            Assert.IsTrue(config.EngineDefaults.EventMeta.AvroSettings.IsEnableAvro);
            Assert.IsTrue(config.EngineDefaults.EventMeta.AvroSettings.IsEnableNativeString);
            Assert.IsTrue(config.EngineDefaults.EventMeta.AvroSettings.IsEnableSchemaDefaultNonNull);
            Assert.IsNull(config.EngineDefaults.EventMeta.AvroSettings.ObjectValueTypeWidenerFactoryClass);
            Assert.IsNull(config.EngineDefaults.EventMeta.AvroSettings.TypeRepresentationMapperClass);

            Assert.IsFalse(config.EngineDefaults.ViewResources.IsShareViews);
            Assert.IsFalse(config.EngineDefaults.ViewResources.IsAllowMultipleExpiryPolicies);
            Assert.IsFalse(config.EngineDefaults.ViewResources.IsIterableUnbound);
            Assert.IsFalse(config.EngineDefaults.Logging.IsEnableExecutionDebug);
            Assert.IsTrue(config.EngineDefaults.Logging.IsEnableTimerDebug);
            Assert.IsFalse(config.EngineDefaults.Logging.IsEnableQueryPlan);
            Assert.IsFalse(config.EngineDefaults.Logging.IsEnableADO);
            Assert.IsNull(config.EngineDefaults.Logging.AuditPattern);
            Assert.AreEqual(15000, config.EngineDefaults.Variables.MsecVersionRelease);
            Assert.AreEqual(null, config.EngineDefaults.Patterns.MaxSubexpressions);
            Assert.AreEqual(true, config.EngineDefaults.Patterns.IsMaxSubexpressionPreventStart);
            Assert.AreEqual(null, config.EngineDefaults.MatchRecognize.MaxStates);
            Assert.AreEqual(true, config.EngineDefaults.MatchRecognize.IsMaxStatesPreventStart);
            Assert.AreEqual(ConfigurationEngineDefaults.TimeSourceType.MILLI, config.EngineDefaults.TimeSource.TimeSourceType);
            Assert.AreEqual(TimeUnit.MILLISECONDS, config.EngineDefaults.TimeSource.TimeUnit);
            Assert.IsFalse(config.EngineDefaults.Execution.IsPrioritized);
            Assert.IsFalse(config.EngineDefaults.Execution.IsDisableLocking);
            Assert.IsFalse(config.EngineDefaults.Execution.IsAllowIsolatedService);
            Assert.AreEqual(ConfigurationEngineDefaults.ThreadingProfile.NORMAL, config.EngineDefaults.Execution.ThreadingProfile);
            Assert.AreEqual(ConfigurationEngineDefaults.FilterServiceProfile.READMOSTLY, config.EngineDefaults.Execution.FilterServiceProfile);
            Assert.AreEqual(16, config.EngineDefaults.Execution.FilterServiceMaxFilterWidth);
            Assert.AreEqual(1, config.EngineDefaults.Execution.DeclaredExprValueCacheSize);

            Assert.AreEqual(StreamSelector.ISTREAM_ONLY, config.EngineDefaults.StreamSelection.DefaultStreamSelector);
            Assert.IsFalse(config.EngineDefaults.Language.IsSortUsingCollator);
            Assert.IsFalse(config.EngineDefaults.Expression.IsIntegerDivision);
            Assert.IsFalse(config.EngineDefaults.Expression.IsDivisionByZeroReturnsNull);
            Assert.IsTrue(config.EngineDefaults.Expression.IsSelfSubselectPreeval);
            Assert.IsTrue(config.EngineDefaults.Expression.IsUdfCache);
            Assert.IsTrue(config.EngineDefaults.Expression.IsExtendedAggregation);
            Assert.IsFalse(config.EngineDefaults.Expression.IsDuckTyping);
            Assert.IsNull(config.EngineDefaults.Expression.MathContext);
            Assert.AreEqual(TimeZoneInfo.Local, config.EngineDefaults.Expression.TimeZone);
            Assert.IsNull(config.EngineDefaults.ExceptionHandling.HandlerFactories);
            Assert.AreEqual(ConfigurationEngineDefaults.UndeployRethrowPolicy.WARN, config.EngineDefaults.ExceptionHandling.UndeployRethrowPolicy);
            Assert.IsNull(config.EngineDefaults.ConditionHandling.HandlerFactories);
            Assert.AreEqual("jscript", config.EngineDefaults.Scripts.DefaultDialect);

            var domType = new ConfigurationEventTypeXMLDOM();
            Assert.IsFalse(domType.IsXPathPropertyExpr);
            Assert.IsTrue(domType.IsXPathResolvePropertiesAbsolute);
            Assert.IsTrue(domType.IsEventSenderValidatesRoot);
            Assert.IsTrue(domType.IsAutoFragment);
        }
    }
}
