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

using com.espertech.esper.collection;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.configuration.runtime;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.db;
using com.espertech.esper.common.@internal.epl.dataflow.ops;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.client.configuration
{
    public class TestConfigurationParser : AbstractCommonTest
    {
        [Test]
        public void TestRegressionFileConfig()
        {
            var config = new Configuration();
            var url = container.ResourceManager().ResolveResourceURL(TestConfiguration.ESPER_TEST_CONFIG);
            using (var client = new WebClient()) {
                using (var stream = client.OpenRead(url)) {
                    ConfigurationParser.DoConfigure(config, stream, url.ToString());
                    AssertFileConfig(config);
                }
            }
        }

        [Test]
        public void TestConfigurationDefaults()
        {
            var config = new Configuration();

            var common = config.Common;
            Assert.AreEqual(PropertyResolutionStyle.CASE_SENSITIVE, common.EventMeta.ClassPropertyResolutionStyle);
            Assert.AreEqual(AccessorStyle.NATIVE, common.EventMeta.DefaultAccessorStyle);
            Assert.AreEqual(EventUnderlyingType.MAP, common.EventMeta.DefaultEventRepresentation);
            Assert.IsTrue(common.EventMeta.AvroSettings.IsEnableAvro);
            Assert.IsTrue(common.EventMeta.AvroSettings.IsEnableNativeString);
            Assert.IsTrue(common.EventMeta.AvroSettings.IsEnableSchemaDefaultNonNull);
            Assert.IsNull(common.EventMeta.AvroSettings.ObjectValueTypeWidenerFactoryClass);
            Assert.IsNull(common.EventMeta.AvroSettings.TypeRepresentationMapperClass);
            Assert.IsTrue(common.Logging.IsEnableQueryPlan);
            Assert.IsTrue(common.Logging.IsEnableADO);
            Assert.AreEqual(TimeUnit.MILLISECONDS, common.TimeSource.TimeUnit);
            Assert.AreEqual(ThreadingProfile.NORMAL, common.Execution.ThreadingProfile);

            var compiler = config.Compiler;
            Assert.IsTrue(compiler.ViewResources.IsIterableUnbound);
            Assert.IsTrue(compiler.ViewResources.IsOutputLimitOpt);
            Assert.IsTrue(compiler.Logging.IsEnableCode);
            Assert.AreEqual(16, compiler.Execution.FilterServiceMaxFilterWidth);
            Assert.IsTrue(compiler.Execution.IsEnabledDeclaredExprValueCache);
            var byteCode = compiler.ByteCode;
            Assert.IsTrue(byteCode.IsIncludeComments);
            Assert.IsTrue(byteCode.IsIncludeDebugSymbols);
            Assert.IsTrue(byteCode.IsAttachEPL);
            Assert.IsTrue(byteCode.IsAttachModuleEPL);
            Assert.IsTrue(byteCode.IsInstrumented);
            Assert.IsTrue(byteCode.IsAllowSubscriber);
            Assert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierContext);
            Assert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierEventType);
            Assert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierExpression);
            Assert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierNamedWindow);
            Assert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierScript);
            Assert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierTable);
            Assert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierVariable);
            Assert.AreEqual(EventTypeBusModifier.NONBUS, byteCode.BusModifierEventType);
            Assert.AreEqual(StreamSelector.ISTREAM_ONLY, compiler.StreamSelection.DefaultStreamSelector);
            Assert.IsTrue(compiler.Language.IsSortUsingCollator);
            Assert.IsTrue(compiler.Expression.IsIntegerDivision);
            Assert.IsTrue(compiler.Expression.IsDivisionByZeroReturnsNull);
            Assert.IsTrue(compiler.Expression.IsUdfCache);
            Assert.IsTrue(compiler.Expression.IsExtendedAggregation);
            Assert.IsTrue(compiler.Expression.IsDuckTyping);
            Assert.IsNull(compiler.Expression.MathContext);
            Assert.AreEqual("js", compiler.Scripts.DefaultDialect);

            var runtime = config.Runtime;
            Assert.IsTrue(runtime.Threading.IsInsertIntoDispatchPreserveOrder);
            Assert.AreEqual(100, runtime.Threading.InsertIntoDispatchTimeout);
            Assert.IsTrue(runtime.Threading.IsListenerDispatchPreserveOrder);
            Assert.AreEqual(1000, runtime.Threading.ListenerDispatchTimeout);
            Assert.IsTrue(runtime.Threading.IsInternalTimerEnabled);
            Assert.AreEqual(100, runtime.Threading.InternalTimerMsecResolution);
            Assert.AreEqual(Locking.SPIN, runtime.Threading.InsertIntoDispatchLocking);
            Assert.AreEqual(Locking.SPIN, runtime.Threading.ListenerDispatchLocking);
            Assert.IsTrue(runtime.Threading.IsThreadPoolInbound);
            Assert.IsTrue(runtime.Threading.IsThreadPoolOutbound);
            Assert.IsTrue(runtime.Threading.IsThreadPoolRouteExec);
            Assert.IsTrue(runtime.Threading.IsThreadPoolTimerExec);
            Assert.AreEqual(2, runtime.Threading.ThreadPoolInboundNumThreads);
            Assert.AreEqual(2, runtime.Threading.ThreadPoolOutboundNumThreads);
            Assert.AreEqual(2, runtime.Threading.ThreadPoolRouteExecNumThreads);
            Assert.AreEqual(2, runtime.Threading.ThreadPoolTimerExecNumThreads);
            Assert.IsNull(runtime.Threading.ThreadPoolInboundCapacity);
            Assert.IsNull(runtime.Threading.ThreadPoolOutboundCapacity);
            Assert.IsNull(runtime.Threading.ThreadPoolRouteExecCapacity);
            Assert.IsNull(runtime.Threading.ThreadPoolTimerExecCapacity);
            Assert.IsTrue(runtime.Threading.IsRuntimeFairlock);
            Assert.IsTrue(runtime.MetricsReporting.IsRuntimeMetrics);
            Assert.IsTrue(runtime.Threading.IsNamedWindowConsumerDispatchPreserveOrder);
            Assert.AreEqual(Int64.MaxValue, runtime.Threading.NamedWindowConsumerDispatchTimeout);
            Assert.AreEqual(Locking.SPIN, runtime.Threading.NamedWindowConsumerDispatchLocking);
            Assert.IsTrue(runtime.Logging.IsEnableExecutionDebug);
            Assert.IsTrue(runtime.Logging.IsEnableTimerDebug);
            Assert.IsNull(runtime.Logging.AuditPattern);
            Assert.AreEqual(15000, runtime.Variables.MsecVersionRelease);
            Assert.IsNull(runtime.Patterns.MaxSubexpressions);
            Assert.IsTrue(runtime.Patterns.IsMaxSubexpressionPreventStart);
            Assert.IsNull(runtime.MatchRecognize.MaxStates);
            Assert.IsTrue(runtime.MatchRecognize.IsMaxStatesPreventStart);
            Assert.AreEqual(TimeSourceType.MILLI, runtime.TimeSource.TimeSourceType);
            Assert.IsTrue(runtime.Execution.IsPrioritized);
            Assert.IsTrue(runtime.Execution.IsDisableLocking);
            Assert.AreEqual(FilterServiceProfile.READMOSTLY, runtime.Execution.FilterServiceProfile);
            Assert.AreEqual(1, runtime.Execution.DeclaredExprValueCacheSize);
            Assert.IsTrue(runtime.Expression.IsSelfSubselectPreeval);
            Assert.AreEqual(TimeZoneInfo.Utc, runtime.Expression.TimeZone);
            Assert.IsNull(runtime.ExceptionHandling.HandlerFactories);
            Assert.AreEqual(UndeployRethrowPolicy.WARN, runtime.ExceptionHandling.UndeployRethrowPolicy);
            Assert.IsNull(runtime.ConditionHandling.HandlerFactories);

            var domType = new ConfigurationCommonEventTypeXMLDOM();
            Assert.IsTrue(domType.IsXPathPropertyExpr);
            Assert.IsTrue(domType.IsXPathResolvePropertiesAbsolute);
            Assert.IsTrue(domType.IsEventSenderValidatesRoot);
            Assert.IsTrue(domType.IsAutoFragment);
        }

        internal static void AssertFileConfig(Configuration config)
        {
            var common = config.Common;
            var compiler = config.Compiler;
            var runtime = config.Runtime;

            /*
             * COMMON
             *
             */

            // assert name for class
            Assert.AreEqual(3, common.EventTypeNames.Count);
            Assert.AreEqual("com.mycompany.myapp.MySampleEventOne", common.EventTypeNames.Get("MySampleEventOne"));
            Assert.AreEqual("com.mycompany.myapp.MySampleEventTwo", common.EventTypeNames.Get("MySampleEventTwo"));
            Assert.AreEqual("com.mycompany.package.MyLegacyTypeEvent", common.EventTypeNames.Get("MyLegacyTypeEvent"));

            // need the assembly for commons - to be certain, we are using a class that is not in any of the
            // namespaces listed below, but is in the NEsper.Commons assembly.
            var commonsAssembly = typeof(BeanEventBean).Assembly;
            
            // assert auto imports
            Assert.AreEqual(9, common.Imports.Count);
            CollectionAssert.AreEquivalent(
                new Import[] {
                    new ImportNamespace("System"),
                    new ImportNamespace("System.Text"),
                    new ImportNamespace("com.espertech.esper.common.internal.epl.dataflow.ops", commonsAssembly.FullName),
                    new ImportNamespace("com.mycompany.myapp"),
                    new ImportNamespace("com.mycompany.myapp", "AssemblyA"),
                    new ImportNamespace("com.mycompany.myapp", "AssemblyB.dll"),
                    new ImportType("com.mycompany.myapp.ClassOne"),
                    new ImportType("com.mycompany.myapp.ClassTwo", "AssemblyB.dll"),
                    ImportBuiltinAnnotations.Instance
                },
                common.Imports);

            // assert XML DOM - no schema
            Assert.AreEqual(2, common.EventTypesXMLDOM.Count);
            var noSchemaDesc = common.EventTypesXMLDOM.Get("MyNoSchemaXMLEventName");
            Assert.AreEqual("MyNoSchemaEvent", noSchemaDesc.RootElementName);
            Assert.AreEqual("/myevent/element1", noSchemaDesc.XPathProperties.Get("element1").XPath);
            Assert.AreEqual(XPathResultType.Number, noSchemaDesc.XPathProperties.Get("element1").Type);
            Assert.IsNull(noSchemaDesc.XPathProperties.Get("element1").OptionalCastToType);
            Assert.IsNull(noSchemaDesc.XPathFunctionResolver);
            Assert.IsNull(noSchemaDesc.XPathVariableResolver);
            Assert.IsTrue(noSchemaDesc.IsXPathPropertyExpr);

            // assert XML DOM - with schema
            var schemaDesc = common.EventTypesXMLDOM.Get("MySchemaXMLEventName");
            Assert.AreEqual("MySchemaEvent", schemaDesc.RootElementName);
            Assert.AreEqual("MySchemaXMLEvent.xsd", schemaDesc.SchemaResource);
            Assert.AreEqual("actual-xsd-text-here", schemaDesc.SchemaText);
            Assert.AreEqual("samples:schemas:simpleSchema", schemaDesc.RootElementNamespace);
            Assert.AreEqual("default-name-space", schemaDesc.DefaultNamespace);
            Assert.AreEqual("/myevent/element2", schemaDesc.XPathProperties.Get("element2").XPath);
            Assert.AreEqual(XPathResultType.String, schemaDesc.XPathProperties.Get("element2").Type);
            Assert.AreEqual(typeof(long?), schemaDesc.XPathProperties.Get("element2").OptionalCastToType);
            Assert.AreEqual("/bookstore/book", schemaDesc.XPathProperties.Get("element3").XPath);
            Assert.AreEqual(XPathResultType.NodeSet, schemaDesc.XPathProperties.Get("element3").Type);
            Assert.IsNull(schemaDesc.XPathProperties.Get("element3").OptionalCastToType);
            Assert.AreEqual("MyOtherXMLNodeEvent", schemaDesc.XPathProperties.Get("element3").OptionalEventTypeName);
            Assert.AreEqual(1, schemaDesc.NamespacePrefixes.Count);
            Assert.AreEqual("samples:schemas:simpleSchema", schemaDesc.NamespacePrefixes.Get("ss"));
            Assert.IsTrue(schemaDesc.IsXPathResolvePropertiesAbsolute);
            Assert.AreEqual("com.mycompany.OptionalFunctionResolver", schemaDesc.XPathFunctionResolver);
            Assert.AreEqual("com.mycompany.OptionalVariableResolver", schemaDesc.XPathVariableResolver);
            Assert.IsTrue(schemaDesc.IsXPathPropertyExpr);
            Assert.IsTrue(schemaDesc.IsEventSenderValidatesRoot);
            Assert.IsTrue(schemaDesc.IsAutoFragment);
            Assert.AreEqual("startts", schemaDesc.StartTimestampPropertyName);
            Assert.AreEqual("endts", schemaDesc.EndTimestampPropertyName);

            // assert mapped events
            Assert.AreEqual(1, common.EventTypesMapEvents.Count);
            Assert.IsTrue(common.EventTypesMapEvents.Keys.Contains("MyMapEvent"));
            var expectedProps = new HashMap<string, string>();
            expectedProps.Put("MyInt", "int");
            expectedProps.Put("MyString", "string");
            Assert.AreEqual(expectedProps, common.EventTypesMapEvents.Get("MyMapEvent"));
            Assert.AreEqual(1, common.MapTypeConfigurations.Count);
            var superTypes = common.MapTypeConfigurations.Get("MyMapEvent").SuperTypes;
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { "MyMapSuperType1", "MyMapSuperType2" }, superTypes.ToArray());
            Assert.AreEqual("startts", common.MapTypeConfigurations.Get("MyMapEvent").StartTimestampPropertyName);
            Assert.AreEqual("endts", common.MapTypeConfigurations.Get("MyMapEvent").EndTimestampPropertyName);

            // assert objectarray events
            Assert.AreEqual(1, common.EventTypesNestableObjectArrayEvents.Count);
            Assert.IsTrue(common.EventTypesNestableObjectArrayEvents.ContainsKey("MyObjectArrayEvent"));
            var expectedPropsObjectArray = new HashMap<string, string>();
            expectedPropsObjectArray.Put("MyInt", "int");
            expectedPropsObjectArray.Put("MyString", "string");
            Assert.AreEqual(expectedPropsObjectArray, common.EventTypesNestableObjectArrayEvents.Get("MyObjectArrayEvent"));
            Assert.AreEqual(1, common.ObjectArrayTypeConfigurations.Count);
            var superTypesOA = common.ObjectArrayTypeConfigurations.Get("MyObjectArrayEvent").SuperTypes;
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { "MyObjectArraySuperType1", "MyObjectArraySuperType2" }, superTypesOA.ToArray());
            Assert.AreEqual("startts", common.ObjectArrayTypeConfigurations.Get("MyObjectArrayEvent").StartTimestampPropertyName);
            Assert.AreEqual("endts", common.ObjectArrayTypeConfigurations.Get("MyObjectArrayEvent").EndTimestampPropertyName);

            // assert avro events
            Assert.AreEqual(2, common.EventTypesAvro.Count);
            var avroOne = common.EventTypesAvro.Get("MyAvroEvent");
            Assert.AreEqual(
                "{\"type\":\"record\",\"name\":\"typename\",\"fields\":[{\"name\":\"num\",\"type\":\"int\"}]}", avroOne.AvroSchemaText);
            Assert.IsNull(avroOne.AvroSchema);
            Assert.IsNull(avroOne.StartTimestampPropertyName);
            Assert.IsNull(avroOne.EndTimestampPropertyName);
            Assert.IsTrue(avroOne.SuperTypes.IsEmpty());
            var avroTwo = common.EventTypesAvro.Get("MyAvroEventTwo");
            Assert.AreEqual(
                "{\"type\":\"record\",\"name\":\"MyAvroEvent\",\"fields\":[{\"name\":\"carId\",\"type\":\"int\"},{\"name\":\"carType\",\"type\":{\"type\":\"string\",\"avro.string\":\"String\"}}]}",
                avroTwo.AvroSchemaText);
            Assert.AreEqual("startts", avroTwo.StartTimestampPropertyName);
            Assert.AreEqual("endts", avroTwo.EndTimestampPropertyName);
            Assert.AreEqual("[SomeSuperAvro, SomeSuperAvroTwo]", avroTwo.SuperTypes.ToString());

            // assert legacy type declaration
            Assert.AreEqual(1, common.EventTypesBean.Count);
            var legacy = common.EventTypesBean.Get("MyLegacyTypeEvent");
            Assert.AreEqual(AccessorStyle.PUBLIC, legacy.AccessorStyle);
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
            Assert.AreEqual(3, common.DatabaseReferences.Count);
            var configDBRef = common.DatabaseReferences.Get("mydb1");
            var dsDef = (DriverConnectionFactoryDesc) configDBRef.ConnectionFactoryDesc;

            Assert.AreEqual("com.espertech.esper.epl.db.drivers.DbDriverPgSQL", dsDef.Driver.GetType().FullName);
            Assert.AreEqual("Host=nesper-pgsql-integ.local;Database=test;Username=esper;Password=3sp3rP@ssw0rd;", dsDef.Driver.ConnectionString);
            Assert.AreEqual(ConnectionLifecycleEnum.POOLED, configDBRef.ConnectionLifecycleEnum);
            Assert.IsNull(configDBRef.ConnectionSettings.AutoCommit);
            Assert.IsNull(configDBRef.ConnectionSettings.Catalog);
            Assert.IsNull(configDBRef.ConnectionSettings.TransactionIsolation);

            var lruCache = (ConfigurationCommonCacheLRU) configDBRef.DataCacheDesc;
            Assert.AreEqual(10, lruCache.Size);
            Assert.AreEqual(ColumnChangeCaseEnum.LOWERCASE, configDBRef.ColumnChangeCase);
            Assert.AreEqual(MetadataOriginEnum.SAMPLE, configDBRef.MetadataRetrievalEnum);
            //Assert.AreEqual(2, configDBRef.DataTypesMapping.Count);
            //Assert.AreEqual("int", configDBRef.DataTypesMapping[2]);
            //Assert.AreEqual("float", configDBRef.DataTypesMapping[6]);

            // assert database reference - driver manager config
            configDBRef = common.DatabaseReferences.Get("mydb2");

            var dmDef = (DriverConnectionFactoryDesc) configDBRef.ConnectionFactoryDesc;
            Assert.AreEqual("com.espertech.esper.epl.db.drivers.DbDriverPgSQL", dmDef.Driver.GetType().FullName);
            Assert.AreEqual("Host=nesper-pgsql-integ.local;Database=test;Username=esper;Password=3sp3rP@ssw0rd;", dmDef.Driver.ConnectionString);

            Assert.AreEqual(ConnectionLifecycleEnum.RETAIN, configDBRef.ConnectionLifecycleEnum);
            Assert.AreEqual(false, configDBRef.ConnectionSettings.AutoCommit);
            Assert.AreEqual("test", configDBRef.ConnectionSettings.Catalog);
            Assert.AreEqual(IsolationLevel.ReadCommitted, configDBRef.ConnectionSettings.TransactionIsolation);
            var expCache = (ConfigurationCommonCacheExpiryTime) configDBRef.DataCacheDesc;

            Assert.AreEqual(60.5, expCache.MaxAgeSeconds);
            Assert.AreEqual(120.1, expCache.PurgeIntervalSeconds);
            Assert.AreEqual(CacheReferenceType.HARD, expCache.CacheReferenceType);
            Assert.AreEqual(ColumnChangeCaseEnum.UPPERCASE, configDBRef.ColumnChangeCase);
            Assert.AreEqual(MetadataOriginEnum.METADATA, configDBRef.MetadataRetrievalEnum);
            //Assert.AreEqual(1, configDBRef.DataTypesMapping.Count);
            //Assert.AreEqual("System.String", configDBRef.DataTypesMapping[99]);

            Assert.AreEqual(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE, common.EventMeta.ClassPropertyResolutionStyle);
            Assert.AreEqual(AccessorStyle.PUBLIC, common.EventMeta.DefaultAccessorStyle);
            Assert.AreEqual(EventUnderlyingType.MAP, common.EventMeta.DefaultEventRepresentation);
            Assert.IsTrue(common.EventMeta.AvroSettings.IsEnableAvro);
            Assert.IsTrue(common.EventMeta.AvroSettings.IsEnableNativeString);
            Assert.IsTrue(common.EventMeta.AvroSettings.IsEnableSchemaDefaultNonNull);
            Assert.AreEqual("myObjectValueTypeWidenerFactoryClass", common.EventMeta.AvroSettings.ObjectValueTypeWidenerFactoryClass);
            Assert.AreEqual("myTypeToRepresentationMapperClass", common.EventMeta.AvroSettings.TypeRepresentationMapperClass);

            Assert.IsTrue(common.Logging.IsEnableQueryPlan);
            Assert.IsTrue(common.Logging.IsEnableADO);

            Assert.AreEqual(TimeUnit.MICROSECONDS, common.TimeSource.TimeUnit);

            // variables
            Assert.AreEqual(3, common.Variables.Count);
            var variable = common.Variables.Get("var1");
            Assert.AreEqual(typeof(int).FullName, variable.VariableType);
            Assert.AreEqual("1", variable.InitializationValue);
            Assert.IsTrue(variable.IsConstant);
            variable = common.Variables.Get("var2");
            Assert.AreEqual(typeof(string).FullName, variable.VariableType);
            Assert.IsNull(variable.InitializationValue);
            Assert.IsTrue(variable.IsConstant);
            variable = common.Variables.Get("var3");
            Assert.IsTrue(variable.IsConstant);

            // method references
            Assert.AreEqual(2, common.MethodInvocationReferences.Count);
            var methodRef = common.MethodInvocationReferences.Get("abc");
            expCache = (ConfigurationCommonCacheExpiryTime) methodRef.DataCacheDesc;
            Assert.AreEqual(91.0, expCache.MaxAgeSeconds);
            Assert.AreEqual(92.2, expCache.PurgeIntervalSeconds);
            Assert.AreEqual(CacheReferenceType.WEAK, expCache.CacheReferenceType);

            methodRef = common.MethodInvocationReferences.Get("def");
            lruCache = (ConfigurationCommonCacheLRU) methodRef.DataCacheDesc;
            Assert.AreEqual(20, lruCache.Size);

            // variance types
            Assert.AreEqual(1, common.VariantStreams.Count);
            var configVStream = common.VariantStreams.Get("MyVariantStream");
            Assert.AreEqual(2, configVStream.VariantTypeNames.Count);
            Assert.IsTrue(configVStream.VariantTypeNames.Contains("MyEvenTypetNameOne"));
            Assert.IsTrue(configVStream.VariantTypeNames.Contains("MyEvenTypetNameTwo"));
            Assert.AreEqual(TypeVariance.ANY, configVStream.TypeVariance);

            Assert.AreEqual(ThreadingProfile.LARGE, common.Execution.ThreadingProfile);

            Assert.AreEqual(2, common.EventTypeAutoNameNamespaces.Count);
            Assert.AreEqual("com.mycompany.eventsone", common.EventTypeAutoNameNamespaces.ToArray()[0]);
            Assert.AreEqual("com.mycompany.eventstwo", common.EventTypeAutoNameNamespaces.ToArray()[1]);

            /*
             * COMPILER
             *
             */

            // assert custom view implementations
            var configViews = compiler.PlugInViews;
            Assert.AreEqual(2, configViews.Count);
            for (var i = 0; i < configViews.Count; i++)
            {
                var entry = configViews[i];
                Assert.AreEqual("ext" + i, entry.Namespace);
                Assert.AreEqual("myview" + i, entry.Name);
                Assert.AreEqual("com.mycompany.MyViewForge" + i, entry.ForgeClassName);
            }

            // assert custom virtual data window implementations
            var configVDW = compiler.PlugInVirtualDataWindows;
            Assert.AreEqual(2, configVDW.Count);
            for (var i = 0; i < configVDW.Count; i++)
            {
                var entry = configVDW[i];
                Assert.AreEqual("vdw" + i, entry.Namespace);
                Assert.AreEqual("myvdw" + i, entry.Name);
                Assert.AreEqual("com.mycompany.MyVdwForge" + i, entry.ForgeClassName);
                if (i == 1)
                {
                    Assert.AreEqual("abc", entry.Config);
                }
            }

            // assert plug-in aggregation function loaded
            Assert.AreEqual(2, compiler.PlugInAggregationFunctions.Count);
            var pluginAgg = compiler.PlugInAggregationFunctions[0];
            Assert.AreEqual("func1a", pluginAgg.Name);
            Assert.AreEqual("com.mycompany.MyMatrixAggregationMethod0Forge", pluginAgg.ForgeClassName);
            pluginAgg = compiler.PlugInAggregationFunctions[1];
            Assert.AreEqual("func2a", pluginAgg.Name);
            Assert.AreEqual("com.mycompany.MyMatrixAggregationMethod1Forge", pluginAgg.ForgeClassName);

            // assert plug-in aggregation multi-function loaded
            Assert.AreEqual(1, compiler.PlugInAggregationMultiFunctions.Count);
            var pluginMultiAgg = compiler.PlugInAggregationMultiFunctions[0];
            EPAssertionUtil.AssertEqualsExactOrder(new[] { "func1", "func2" }, pluginMultiAgg.FunctionNames);
            Assert.AreEqual("com.mycompany.MyAggregationMultiFunctionForge", pluginMultiAgg.MultiFunctionForgeClassName);
            Assert.AreEqual(1, pluginMultiAgg.AdditionalConfiguredProperties.Count);
            Assert.AreEqual("value1", pluginMultiAgg.AdditionalConfiguredProperties.Get("prop1"));

            // assert plug-in singlerow function loaded
            Assert.AreEqual(2, compiler.PlugInSingleRowFunctions.Count);
            var pluginSingleRow = compiler.PlugInSingleRowFunctions[0];
            Assert.AreEqual("com.mycompany.MyMatrixSingleRowMethod0", pluginSingleRow.FunctionClassName);
            Assert.AreEqual("method1", pluginSingleRow.FunctionMethodName);
            Assert.AreEqual("func3", pluginSingleRow.Name);
            Assert.AreEqual(ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED, pluginSingleRow.ValueCache);
            Assert.AreEqual(ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED, pluginSingleRow.FilterOptimizable);
            Assert.IsTrue(pluginSingleRow.RethrowExceptions);
            pluginSingleRow = compiler.PlugInSingleRowFunctions[1];
            Assert.AreEqual("com.mycompany.MyMatrixSingleRowMethod1", pluginSingleRow.FunctionClassName);
            Assert.AreEqual("func4", pluginSingleRow.Name);
            Assert.AreEqual("method2", pluginSingleRow.FunctionMethodName);
            Assert.AreEqual(ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.ENABLED, pluginSingleRow.ValueCache);
            Assert.AreEqual(ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.DISABLED, pluginSingleRow.FilterOptimizable);
            Assert.IsTrue(pluginSingleRow.RethrowExceptions);
            Assert.AreEqual("XYZEventTypeName", pluginSingleRow.EventTypeName);

            // assert plug-in guard objects loaded
            Assert.AreEqual(4, compiler.PlugInPatternObjects.Count);
            var pluginPattern = compiler.PlugInPatternObjects[0];
            Assert.AreEqual("com.mycompany.MyGuardForge0", pluginPattern.ForgeClassName);
            Assert.AreEqual("ext0", pluginPattern.Namespace);
            Assert.AreEqual("guard1", pluginPattern.Name);
            Assert.AreEqual(PatternObjectType.GUARD, pluginPattern.PatternObjectType);
            pluginPattern = compiler.PlugInPatternObjects[1];
            Assert.AreEqual("com.mycompany.MyGuardForge1", pluginPattern.ForgeClassName);
            Assert.AreEqual("ext1", pluginPattern.Namespace);
            Assert.AreEqual("guard2", pluginPattern.Name);
            Assert.AreEqual(PatternObjectType.GUARD, pluginPattern.PatternObjectType);
            pluginPattern = compiler.PlugInPatternObjects[2];
            Assert.AreEqual("com.mycompany.MyObserverForge0", pluginPattern.ForgeClassName);
            Assert.AreEqual("ext0", pluginPattern.Namespace);
            Assert.AreEqual("observer1", pluginPattern.Name);
            Assert.AreEqual(PatternObjectType.OBSERVER, pluginPattern.PatternObjectType);
            pluginPattern = compiler.PlugInPatternObjects[3];
            Assert.AreEqual("com.mycompany.MyObserverForge1", pluginPattern.ForgeClassName);
            Assert.AreEqual("ext1", pluginPattern.Namespace);
            Assert.AreEqual("observer2", pluginPattern.Name);
            Assert.AreEqual(PatternObjectType.OBSERVER, pluginPattern.PatternObjectType);

            Assert.IsTrue(compiler.ViewResources.IsIterableUnbound);
            Assert.IsTrue(compiler.ViewResources.IsOutputLimitOpt);

            Assert.IsTrue(compiler.Logging.IsEnableCode);

            Assert.AreEqual(StreamSelector.RSTREAM_ISTREAM_BOTH, compiler.StreamSelection.DefaultStreamSelector);

            var byteCode = compiler.ByteCode;
            Assert.IsTrue(byteCode.IsIncludeComments);
            Assert.IsTrue(byteCode.IsIncludeDebugSymbols);
            Assert.IsTrue(byteCode.IsAttachEPL);
            Assert.IsTrue(byteCode.IsAttachModuleEPL);
            Assert.IsTrue(byteCode.IsInstrumented);
            Assert.IsTrue(byteCode.IsAllowSubscriber);
            Assert.AreEqual(NameAccessModifier.INTERNAL, byteCode.AccessModifierContext);
            Assert.AreEqual(NameAccessModifier.PUBLIC, byteCode.AccessModifierEventType);
            Assert.AreEqual(NameAccessModifier.INTERNAL, byteCode.AccessModifierExpression);
            Assert.AreEqual(NameAccessModifier.PUBLIC, byteCode.AccessModifierNamedWindow);
            Assert.AreEqual(NameAccessModifier.INTERNAL, byteCode.AccessModifierScript);
            Assert.AreEqual(NameAccessModifier.PUBLIC, byteCode.AccessModifierTable);
            Assert.AreEqual(NameAccessModifier.INTERNAL, byteCode.AccessModifierVariable);
            Assert.AreEqual(EventTypeBusModifier.BUS, byteCode.BusModifierEventType);
            Assert.AreEqual(StreamSelector.RSTREAM_ISTREAM_BOTH, compiler.StreamSelection.DefaultStreamSelector);

            Assert.AreEqual(100, compiler.Execution.FilterServiceMaxFilterWidth);
            Assert.IsTrue(compiler.Execution.IsEnabledDeclaredExprValueCache);

            Assert.IsTrue(compiler.Language.IsSortUsingCollator);

            Assert.IsTrue(compiler.Expression.IsIntegerDivision);
            Assert.IsTrue(compiler.Expression.IsDivisionByZeroReturnsNull);
            Assert.IsTrue(compiler.Expression.IsUdfCache);
            Assert.IsTrue(compiler.Expression.IsExtendedAggregation);
            Assert.IsTrue(compiler.Expression.IsDuckTyping);
            Assert.AreEqual(2, compiler.Expression.MathContext.Precision);
            Assert.AreEqual(MidpointRounding.AwayFromZero, compiler.Expression.MathContext.RoundingMode);

            Assert.AreEqual("abc", compiler.Scripts.DefaultDialect);

            /*
             * RUNTIME
             *
             */

            // assert runtime defaults
            Assert.IsTrue(runtime.Threading.IsInsertIntoDispatchPreserveOrder);
            Assert.AreEqual(3000, runtime.Threading.InsertIntoDispatchTimeout);
            Assert.AreEqual(Locking.SUSPEND, runtime.Threading.InsertIntoDispatchLocking);
            Assert.IsTrue(runtime.Threading.IsNamedWindowConsumerDispatchPreserveOrder);
            Assert.AreEqual(4000, runtime.Threading.NamedWindowConsumerDispatchTimeout);
            Assert.AreEqual(Locking.SUSPEND, runtime.Threading.NamedWindowConsumerDispatchLocking);

            Assert.IsTrue(runtime.Threading.IsListenerDispatchPreserveOrder);
            Assert.AreEqual(2000, runtime.Threading.ListenerDispatchTimeout);
            Assert.AreEqual(Locking.SUSPEND, runtime.Threading.ListenerDispatchLocking);
            Assert.IsTrue(runtime.Threading.IsThreadPoolInbound);
            Assert.IsTrue(runtime.Threading.IsThreadPoolOutbound);
            Assert.IsTrue(runtime.Threading.IsThreadPoolRouteExec);
            Assert.IsTrue(runtime.Threading.IsThreadPoolTimerExec);
            Assert.AreEqual(1, runtime.Threading.ThreadPoolInboundNumThreads);
            Assert.AreEqual(2, runtime.Threading.ThreadPoolOutboundNumThreads);
            Assert.AreEqual(3, runtime.Threading.ThreadPoolTimerExecNumThreads);
            Assert.AreEqual(4, runtime.Threading.ThreadPoolRouteExecNumThreads);
            Assert.AreEqual(1000, (int) runtime.Threading.ThreadPoolInboundCapacity);
            Assert.AreEqual(1500, (int) runtime.Threading.ThreadPoolOutboundCapacity);
            Assert.IsNull(runtime.Threading.ThreadPoolTimerExecCapacity);
            Assert.AreEqual(2000, (int) runtime.Threading.ThreadPoolRouteExecCapacity);
            Assert.IsTrue(runtime.Threading.IsRuntimeFairlock);

            Assert.IsTrue(runtime.Threading.IsInternalTimerEnabled);
            Assert.AreEqual(1234567, runtime.Threading.InternalTimerMsecResolution);
            Assert.IsTrue(runtime.Logging.IsEnableExecutionDebug);
            Assert.IsTrue(runtime.Logging.IsEnableTimerDebug);
            Assert.AreEqual("[%u] %m", runtime.Logging.AuditPattern);
            Assert.AreEqual(30000, runtime.Variables.MsecVersionRelease);
            Assert.AreEqual(3L, (long) runtime.Patterns.MaxSubexpressions);
            Assert.IsTrue(runtime.Patterns.IsMaxSubexpressionPreventStart);
            Assert.AreEqual(3L, (long) runtime.MatchRecognize.MaxStates);
            Assert.IsTrue(runtime.MatchRecognize.IsMaxStatesPreventStart);

            // assert adapter loaders parsed
            IList<ConfigurationRuntimePluginLoader> plugins = runtime.PluginLoaders;
            Assert.AreEqual(2, plugins.Count);
            var pluginOne = plugins[0];
            Assert.AreEqual("Loader1", pluginOne.LoaderName);
            Assert.AreEqual("com.espertech.esper.support.plugin.SupportLoaderOne", pluginOne.ClassName);
            Assert.AreEqual(2, pluginOne.ConfigProperties.Count);
            Assert.AreEqual("val1", pluginOne.ConfigProperties.Get("name1"));
            Assert.AreEqual("val2", pluginOne.ConfigProperties.Get("name2"));
            Assert.AreEqual(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><sample-initializer><some-any-xml-can-be-here>This section for use by a plugin loader.</some-any-xml-can-be-here></sample-initializer>",
                pluginOne.ConfigurationXML);

            var pluginTwo = plugins[1];
            Assert.AreEqual("Loader2", pluginTwo.LoaderName);
            Assert.AreEqual("com.espertech.esper.support.plugin.SupportLoaderTwo", pluginTwo.ClassName);
            Assert.AreEqual(0, pluginTwo.ConfigProperties.Count);

            Assert.AreEqual(TimeSourceType.NANO, runtime.TimeSource.TimeSourceType);
            Assert.IsTrue(runtime.Execution.IsPrioritized);
            Assert.IsTrue(runtime.Execution.IsFairlock);
            Assert.IsTrue(runtime.Execution.IsDisableLocking);
            Assert.AreEqual(FilterServiceProfile.READWRITE, runtime.Execution.FilterServiceProfile);
            Assert.AreEqual(101, runtime.Execution.DeclaredExprValueCacheSize);

            var metrics = runtime.MetricsReporting;
            Assert.IsTrue(metrics.IsEnableMetricsReporting);
            Assert.AreEqual(4000L, metrics.RuntimeInterval);
            Assert.AreEqual(500L, metrics.StatementInterval);
            Assert.IsTrue(metrics.IsThreading);
            Assert.AreEqual(2, metrics.StatementGroups.Count);
            Assert.IsTrue(metrics.IsRuntimeMetrics);
            var def = metrics.StatementGroups.Get("MyStmtGroup");
            Assert.AreEqual(5000, def.Interval);
            Assert.IsTrue(def.IsDefaultInclude);
            Assert.AreEqual(50, def.NumStatements);
            Assert.IsTrue(def.IsReportInactive);
            Assert.AreEqual(5, def.Patterns.Count);
            Assert.AreEqual(def.Patterns[0], new Pair<StringPatternSet, bool>(new StringPatternSetRegex(".*"), true));
            Assert.AreEqual(def.Patterns[1], new Pair<StringPatternSet, bool>(new StringPatternSetRegex(".*test.*"), false));
            Assert.AreEqual(def.Patterns[2], new Pair<StringPatternSet, bool>(new StringPatternSetLike("%MyMetricsStatement%"), false));
            Assert.AreEqual(
                def.Patterns[3], new Pair<StringPatternSet, bool>(new StringPatternSetLike("%MyFraudAnalysisStatement%"), true));
            Assert.AreEqual(def.Patterns[4], new Pair<StringPatternSet, bool>(new StringPatternSetLike("%SomerOtherStatement%"), true));
            def = metrics.StatementGroups.Get("MyStmtGroupTwo");
            Assert.AreEqual(200, def.Interval);
            Assert.IsTrue(def.IsDefaultInclude);
            Assert.AreEqual(100, def.NumStatements);
            Assert.IsTrue(def.IsReportInactive);
            Assert.AreEqual(0, def.Patterns.Count);
            Assert.IsTrue(runtime.Expression.IsSelfSubselectPreeval);
            Assert.AreEqual(TimeZoneHelper.GetTimeZoneInfo("GMT-4:00"), runtime.Expression.TimeZone);
            Assert.AreEqual(2, runtime.ExceptionHandling.HandlerFactories.Count);
            Assert.AreEqual("my.company.cep.LoggingExceptionHandlerFactory", runtime.ExceptionHandling.HandlerFactories[0]);
            Assert.AreEqual("my.company.cep.AlertExceptionHandlerFactory", runtime.ExceptionHandling.HandlerFactories[1]);
            Assert.AreEqual(UndeployRethrowPolicy.RETHROW_FIRST, runtime.ExceptionHandling.UndeployRethrowPolicy);
            Assert.AreEqual(2, runtime.ConditionHandling.HandlerFactories.Count);
            Assert.AreEqual("my.company.cep.LoggingConditionHandlerFactory", runtime.ConditionHandling.HandlerFactories[0]);
            Assert.AreEqual("my.company.cep.AlertConditionHandlerFactory", runtime.ConditionHandling.HandlerFactories[1]);
        }
    }
} // end of namespace
