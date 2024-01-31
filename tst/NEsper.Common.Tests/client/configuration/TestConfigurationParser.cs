///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.configuration.runtime;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.db;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.client.configuration
{
    public class TestConfigurationParser : AbstractCommonTest
    {
        [Test]
        public void TestRegressionFileConfig()
        {
            var config = new Configuration(container);
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
            var config = new Configuration(container);

            var common = config.Common;
            ClassicAssert.AreEqual(PropertyResolutionStyle.CASE_SENSITIVE, common.EventMeta.ClassPropertyResolutionStyle);
            ClassicAssert.AreEqual(AccessorStyle.NATIVE, common.EventMeta.DefaultAccessorStyle);
            ClassicAssert.AreEqual(EventUnderlyingType.MAP, common.EventMeta.DefaultEventRepresentation);
            ClassicAssert.IsFalse(common.EventMeta.IsEnableXmlXsd);
            ClassicAssert.IsFalse(common.EventMeta.AvroSettings.IsEnableAvro);
            ClassicAssert.IsTrue(common.EventMeta.AvroSettings.IsEnableNativeString);
            ClassicAssert.IsTrue(common.EventMeta.AvroSettings.IsEnableSchemaDefaultNonNull);
            ClassicAssert.IsNull(common.EventMeta.AvroSettings.ObjectValueTypeWidenerFactoryClass);
            ClassicAssert.IsNull(common.EventMeta.AvroSettings.TypeRepresentationMapperClass);
            ClassicAssert.IsFalse(common.Logging.IsEnableQueryPlan);
            ClassicAssert.IsFalse(common.Logging.IsEnableADO);
            ClassicAssert.AreEqual(TimeUnit.MILLISECONDS, common.TimeSource.TimeUnit);
            ClassicAssert.AreEqual(ThreadingProfile.NORMAL, common.Execution.ThreadingProfile);

            var compiler = config.Compiler;
            ClassicAssert.IsFalse(compiler.ViewResources.IsIterableUnbound);
            ClassicAssert.IsTrue(compiler.ViewResources.IsOutputLimitOpt);
            ClassicAssert.IsFalse(compiler.Logging.IsEnableCode);
            ClassicAssert.IsFalse(compiler.Logging.IsEnableFilterPlan);
            ClassicAssert.AreEqual(16, compiler.Execution.FilterServiceMaxFilterWidth);
            ClassicAssert.AreEqual(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, compiler.Execution.FilterIndexPlanning);
            ClassicAssert.IsTrue(compiler.Execution.IsEnabledDeclaredExprValueCache);
            var byteCode = compiler.ByteCode;
            ClassicAssert.IsFalse(byteCode.IsIncludeComments);
            ClassicAssert.IsFalse(byteCode.IsIncludeDebugSymbols);
            ClassicAssert.IsTrue(byteCode.IsAttachEPL);
            ClassicAssert.IsFalse(byteCode.IsAttachModuleEPL);
            ClassicAssert.IsFalse(byteCode.IsAttachPatternEPL);
            ClassicAssert.IsFalse(byteCode.IsInstrumented);
            ClassicAssert.IsFalse(byteCode.IsAllowSubscriber);
            ClassicAssert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierContext);
            ClassicAssert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierEventType);
            ClassicAssert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierExpression);
            ClassicAssert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierNamedWindow);
            ClassicAssert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierScript);
            ClassicAssert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierTable);
            ClassicAssert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierVariable);
            ClassicAssert.AreEqual(NameAccessModifier.PRIVATE, byteCode.AccessModifierInlinedClass);
            ClassicAssert.AreEqual(EventTypeBusModifier.NONBUS, byteCode.BusModifierEventType);
            ClassicAssert.AreEqual(8, byteCode.ThreadPoolCompilerNumThreads);
            ClassicAssert.IsNull(byteCode.ThreadPoolCompilerCapacity);
            ClassicAssert.AreEqual(1024, byteCode.MaxMethodsPerClass);
            ClassicAssert.IsTrue(byteCode.IsAllowInlinedClass);
            ClassicAssert.AreEqual(StreamSelector.ISTREAM_ONLY, compiler.StreamSelection.DefaultStreamSelector);
            ClassicAssert.IsFalse(compiler.Language.IsSortUsingCollator);
            ClassicAssert.IsFalse(compiler.Expression.IsIntegerDivision);
            ClassicAssert.IsFalse(compiler.Expression.IsDivisionByZeroReturnsNull);
            ClassicAssert.IsTrue(compiler.Expression.IsUdfCache);
            ClassicAssert.IsTrue(compiler.Expression.IsExtendedAggregation);
            ClassicAssert.IsFalse(compiler.Expression.IsDuckTyping);
            ClassicAssert.IsNull(compiler.Expression.MathContext);
            ClassicAssert.AreEqual("js", compiler.Scripts.DefaultDialect);
            ClassicAssert.IsTrue(compiler.Scripts.IsEnabled);
            ClassicAssert.IsTrue(compiler.Serde.IsEnableExtendedBuiltin);
            ClassicAssert.IsFalse(compiler.Serde.IsEnableExternalizable);
            ClassicAssert.IsFalse(compiler.Serde.IsEnableSerializable);
            ClassicAssert.IsFalse(compiler.Serde.IsEnableSerializationFallback);
            ClassicAssert.IsTrue(compiler.Serde.SerdeProviderFactories.IsEmpty());
            
            var runtime = config.Runtime;
            ClassicAssert.IsTrue(runtime.Threading.IsInsertIntoDispatchPreserveOrder);
            ClassicAssert.AreEqual(100, runtime.Threading.InsertIntoDispatchTimeout);
            ClassicAssert.IsTrue(runtime.Threading.IsListenerDispatchPreserveOrder);
            ClassicAssert.AreEqual(1000, runtime.Threading.ListenerDispatchTimeout);
            ClassicAssert.IsTrue(runtime.Threading.IsInternalTimerEnabled);
            ClassicAssert.AreEqual(100, runtime.Threading.InternalTimerMsecResolution);
            ClassicAssert.AreEqual(Locking.SPIN, runtime.Threading.InsertIntoDispatchLocking);
            ClassicAssert.AreEqual(Locking.SPIN, runtime.Threading.ListenerDispatchLocking);
            ClassicAssert.IsFalse(runtime.Threading.IsThreadPoolInbound);
            ClassicAssert.IsFalse(runtime.Threading.IsThreadPoolOutbound);
            ClassicAssert.IsFalse(runtime.Threading.IsThreadPoolRouteExec);
            ClassicAssert.IsFalse(runtime.Threading.IsThreadPoolTimerExec);
            ClassicAssert.AreEqual(2, runtime.Threading.ThreadPoolInboundNumThreads);
            ClassicAssert.AreEqual(2, runtime.Threading.ThreadPoolOutboundNumThreads);
            ClassicAssert.AreEqual(2, runtime.Threading.ThreadPoolRouteExecNumThreads);
            ClassicAssert.AreEqual(2, runtime.Threading.ThreadPoolTimerExecNumThreads);
            ClassicAssert.IsNull(runtime.Threading.ThreadPoolInboundCapacity);
            ClassicAssert.IsNull(runtime.Threading.ThreadPoolOutboundCapacity);
            ClassicAssert.IsNull(runtime.Threading.ThreadPoolRouteExecCapacity);
            ClassicAssert.IsNull(runtime.Threading.ThreadPoolTimerExecCapacity);
            ClassicAssert.IsFalse(runtime.Threading.IsRuntimeFairlock);
            ClassicAssert.IsFalse(runtime.MetricsReporting.IsRuntimeMetrics);
            ClassicAssert.IsTrue(runtime.Threading.IsNamedWindowConsumerDispatchPreserveOrder);
            ClassicAssert.AreEqual(Int32.MaxValue, runtime.Threading.NamedWindowConsumerDispatchTimeout);
            ClassicAssert.AreEqual(Locking.SPIN, runtime.Threading.NamedWindowConsumerDispatchLocking);
            ClassicAssert.IsFalse(runtime.Logging.IsEnableExecutionDebug);
            ClassicAssert.IsTrue(runtime.Logging.IsEnableTimerDebug);
            ClassicAssert.IsNull(runtime.Logging.AuditPattern);
            ClassicAssert.IsFalse(runtime.Logging.IsEnableLockActivity);
            ClassicAssert.AreEqual(15000, runtime.Variables.MsecVersionRelease);
            ClassicAssert.IsNull(runtime.Patterns.MaxSubexpressions);
            ClassicAssert.IsTrue(runtime.Patterns.IsMaxSubexpressionPreventStart);
            ClassicAssert.IsNull(runtime.MatchRecognize.MaxStates);
            ClassicAssert.IsTrue(runtime.MatchRecognize.IsMaxStatesPreventStart);
            ClassicAssert.AreEqual(TimeSourceType.MILLI, runtime.TimeSource.TimeSourceType);
            ClassicAssert.IsFalse(runtime.Execution.IsPrioritized);
            ClassicAssert.IsFalse(runtime.Execution.IsPrecedenceEnabled);
            ClassicAssert.IsFalse(runtime.Execution.IsDisableLocking);
            ClassicAssert.AreEqual(FilterServiceProfile.READMOSTLY, runtime.Execution.FilterServiceProfile);
            ClassicAssert.AreEqual(1, runtime.Execution.DeclaredExprValueCacheSize);
            ClassicAssert.IsTrue(runtime.Expression.IsSelfSubselectPreeval);
            ClassicAssert.AreEqual(TimeZoneInfo.Utc, runtime.Expression.TimeZone);
            ClassicAssert.IsNull(runtime.ExceptionHandling.HandlerFactories);
            ClassicAssert.AreEqual(UndeployRethrowPolicy.WARN, runtime.ExceptionHandling.UndeployRethrowPolicy);
            ClassicAssert.IsNull(runtime.ConditionHandling.HandlerFactories);

            var domType = new ConfigurationCommonEventTypeXMLDOM();
            ClassicAssert.IsFalse(domType.IsXPathPropertyExpr);
            ClassicAssert.IsTrue(domType.IsXPathResolvePropertiesAbsolute);
            ClassicAssert.IsTrue(domType.IsEventSenderValidatesRoot);
            ClassicAssert.IsTrue(domType.IsAutoFragment);
        }

        internal static void AssertFileConfig(Configuration config)
        {
            var container = config.Container;
            var common = config.Common;
            var compiler = config.Compiler;
            var runtime = config.Runtime;

            /*
             * COMMON
             *
             */

            // assert name for class
            ClassicAssert.AreEqual(3, common.EventTypeNames.Count);
            ClassicAssert.AreEqual("com.mycompany.myapp.MySampleEventOne", common.EventTypeNames.Get("MySampleEventOne"));
            ClassicAssert.AreEqual("com.mycompany.myapp.MySampleEventTwo", common.EventTypeNames.Get("MySampleEventTwo"));
            ClassicAssert.AreEqual("com.mycompany.package.MyLegacyTypeEvent", common.EventTypeNames.Get("MyLegacyTypeEvent"));

            // need the assembly for commons - to be certain, we are using a class that is not in any of the
            // namespaces listed below, but is in the NEsper.Commons assembly.
            var commonsAssembly = typeof(BeanEventBean).Assembly;
            
            // assert auto imports
            ClassicAssert.AreEqual(9, common.Imports.Count);
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
            ClassicAssert.AreEqual(2, common.EventTypesXMLDOM.Count);
            var noSchemaDesc = common.EventTypesXMLDOM.Get("MyNoSchemaXMLEventName");
            ClassicAssert.AreEqual("MyNoSchemaEvent", noSchemaDesc.RootElementName);
            ClassicAssert.AreEqual("/myevent/element1", noSchemaDesc.XPathProperties.Get("element1").XPath);
            ClassicAssert.AreEqual(XPathResultType.Number, noSchemaDesc.XPathProperties.Get("element1").Type);
            ClassicAssert.IsNull(noSchemaDesc.XPathProperties.Get("element1").OptionalCastToType);
            ClassicAssert.IsNull(noSchemaDesc.XPathFunctionResolver);
            ClassicAssert.IsNull(noSchemaDesc.XPathVariableResolver);
            ClassicAssert.IsFalse(noSchemaDesc.IsXPathPropertyExpr);

            // assert XML DOM - with schema
            var schemaDesc = common.EventTypesXMLDOM.Get("MySchemaXMLEventName");
            ClassicAssert.AreEqual("MySchemaEvent", schemaDesc.RootElementName);
            ClassicAssert.AreEqual("MySchemaXMLEvent.xsd", schemaDesc.SchemaResource);
            ClassicAssert.AreEqual("actual-xsd-text-here", schemaDesc.SchemaText);
            ClassicAssert.AreEqual("samples:schemas:simpleSchema", schemaDesc.RootElementNamespace);
            ClassicAssert.AreEqual("default-name-space", schemaDesc.DefaultNamespace);
            ClassicAssert.AreEqual("/myevent/element2", schemaDesc.XPathProperties.Get("element2").XPath);
            ClassicAssert.AreEqual(XPathResultType.String, schemaDesc.XPathProperties.Get("element2").Type);
            ClassicAssert.AreEqual(typeof(long), schemaDesc.XPathProperties.Get("element2").OptionalCastToType);
            ClassicAssert.AreEqual("/bookstore/book", schemaDesc.XPathProperties.Get("element3").XPath);
            ClassicAssert.AreEqual(XPathResultType.NodeSet, schemaDesc.XPathProperties.Get("element3").Type);
            ClassicAssert.IsNull(schemaDesc.XPathProperties.Get("element3").OptionalCastToType);
            ClassicAssert.AreEqual("MyOtherXMLNodeEvent", schemaDesc.XPathProperties.Get("element3").OptionalEventTypeName);
            ClassicAssert.AreEqual(1, schemaDesc.NamespacePrefixes.Count);
            ClassicAssert.AreEqual("samples:schemas:simpleSchema", schemaDesc.NamespacePrefixes.Get("ss"));
            ClassicAssert.IsFalse(schemaDesc.IsXPathResolvePropertiesAbsolute);
            ClassicAssert.AreEqual("com.mycompany.OptionalFunctionResolver", schemaDesc.XPathFunctionResolver);
            ClassicAssert.AreEqual("com.mycompany.OptionalVariableResolver", schemaDesc.XPathVariableResolver);
            ClassicAssert.IsTrue(schemaDesc.IsXPathPropertyExpr);
            ClassicAssert.IsFalse(schemaDesc.IsEventSenderValidatesRoot);
            ClassicAssert.IsFalse(schemaDesc.IsAutoFragment);
            ClassicAssert.AreEqual("startts", schemaDesc.StartTimestampPropertyName);
            ClassicAssert.AreEqual("endts", schemaDesc.EndTimestampPropertyName);

            // assert mapped events
            ClassicAssert.AreEqual(1, common.EventTypesMapEvents.Count);
            ClassicAssert.IsTrue(common.EventTypesMapEvents.Keys.Contains("MyMapEvent"));
            var expectedProps = new HashMap<string, string>();
            expectedProps.Put("myInt", "int");
            expectedProps.Put("myString", "string");
            ClassicAssert.AreEqual(expectedProps, common.EventTypesMapEvents.Get("MyMapEvent"));
            ClassicAssert.AreEqual(1, common.MapTypeConfigurations.Count);
            var superTypes = common.MapTypeConfigurations.Get("MyMapEvent").SuperTypes;
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { "MyMapSuperType1", "MyMapSuperType2" }, superTypes.ToArray());
            ClassicAssert.AreEqual("startts", common.MapTypeConfigurations.Get("MyMapEvent").StartTimestampPropertyName);
            ClassicAssert.AreEqual("endts", common.MapTypeConfigurations.Get("MyMapEvent").EndTimestampPropertyName);

            // assert objectarray events
            ClassicAssert.AreEqual(1, common.EventTypesNestableObjectArrayEvents.Count);
            ClassicAssert.IsTrue(common.EventTypesNestableObjectArrayEvents.ContainsKey("MyObjectArrayEvent"));
            var expectedPropsObjectArray = new HashMap<string, string>();
            expectedPropsObjectArray.Put("myInt", "int");
            expectedPropsObjectArray.Put("myString", "string");
            ClassicAssert.AreEqual(expectedPropsObjectArray, common.EventTypesNestableObjectArrayEvents.Get("MyObjectArrayEvent"));
            ClassicAssert.AreEqual(1, common.ObjectArrayTypeConfigurations.Count);
            var superTypesOA = common.ObjectArrayTypeConfigurations.Get("MyObjectArrayEvent").SuperTypes;
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { "MyObjectArraySuperType1", "MyObjectArraySuperType2" }, superTypesOA.ToArray());
            ClassicAssert.AreEqual("startts", common.ObjectArrayTypeConfigurations.Get("MyObjectArrayEvent").StartTimestampPropertyName);
            ClassicAssert.AreEqual("endts", common.ObjectArrayTypeConfigurations.Get("MyObjectArrayEvent").EndTimestampPropertyName);

            // assert avro events
            ClassicAssert.AreEqual(2, common.EventTypesAvro.Count);
            var avroOne = common.EventTypesAvro.Get("MyAvroEvent");
            ClassicAssert.AreEqual(
                "{\"type\":\"record\",\"name\":\"typename\",\"fields\":[{\"name\":\"num\",\"type\":\"int\"}]}", avroOne.AvroSchemaText);
            ClassicAssert.IsNull(avroOne.AvroSchema);
            ClassicAssert.IsNull(avroOne.StartTimestampPropertyName);
            ClassicAssert.IsNull(avroOne.EndTimestampPropertyName);
            ClassicAssert.IsTrue(avroOne.SuperTypes.IsEmpty());
            var avroTwo = common.EventTypesAvro.Get("MyAvroEventTwo");
            ClassicAssert.AreEqual(
                "{\"type\":\"record\",\"name\":\"MyAvroEvent\",\"fields\":[{\"name\":\"carId\",\"type\":\"int\"},{\"name\":\"carType\",\"type\":{\"type\":\"string\",\"avro.string\":\"string\"}}]}",
                avroTwo.AvroSchemaText);
            ClassicAssert.AreEqual("startts", avroTwo.StartTimestampPropertyName);
            ClassicAssert.AreEqual("endts", avroTwo.EndTimestampPropertyName);
            ClassicAssert.AreEqual("[\"SomeSuperAvro\", \"SomeSuperAvroTwo\"]", avroTwo.SuperTypes.RenderAny());

            // assert legacy type declaration
            ClassicAssert.AreEqual(1, common.EventTypesBean.Count);
            var legacy = common.EventTypesBean.Get("MyLegacyTypeEvent");
            ClassicAssert.AreEqual(AccessorStyle.PUBLIC, legacy.AccessorStyle);
            ClassicAssert.AreEqual(1, legacy.FieldProperties.Count);
            ClassicAssert.AreEqual("myFieldName", legacy.FieldProperties[0].AccessorFieldName);
            ClassicAssert.AreEqual("myfieldprop", legacy.FieldProperties[0].Name);
            ClassicAssert.AreEqual(1, legacy.MethodProperties.Count);
            ClassicAssert.AreEqual("myAccessorMethod", legacy.MethodProperties[0].AccessorMethodName);
            ClassicAssert.AreEqual("mymethodprop", legacy.MethodProperties[0].Name);
            ClassicAssert.AreEqual(PropertyResolutionStyle.CASE_INSENSITIVE, legacy.PropertyResolutionStyle);
            ClassicAssert.AreEqual("com.mycompany.myapp.MySampleEventFactory.createMyLegacyTypeEvent", legacy.FactoryMethod);
            ClassicAssert.AreEqual("myCopyMethod", legacy.CopyMethod);
            ClassicAssert.AreEqual("startts", legacy.StartTimestampPropertyName);
            ClassicAssert.AreEqual("endts", legacy.EndTimestampPropertyName);

            // assert database reference - data source config
            ClassicAssert.AreEqual(2, common.DatabaseReferences.Count);
            var configDBRef = common.DatabaseReferences.Get("mydb1");
            var dbDef = (DriverConnectionFactoryDesc) configDBRef.ConnectionFactoryDesc;
            var dbDriver = DbDriverConnectionHelper.ResolveDriver(container, dbDef);

            ClassicAssert.AreEqual("com.espertech.esper.epl.db.drivers.DbDriverPgSQL", dbDriver.GetType().FullName);
            ClassicAssert.AreEqual("Host=localhost;Database=esper;Username=esper;Password=3sp3rP@ssw0rd;", dbDriver.ConnectionString);
            ClassicAssert.AreEqual(ConnectionLifecycleEnum.POOLED, configDBRef.ConnectionLifecycleEnum);
            ClassicAssert.IsNull(configDBRef.ConnectionSettings.AutoCommit);
            ClassicAssert.IsNull(configDBRef.ConnectionSettings.Catalog);
            ClassicAssert.IsNull(configDBRef.ConnectionSettings.TransactionIsolation);

            var lruCache = (ConfigurationCommonCacheLRU) configDBRef.DataCacheDesc;
            ClassicAssert.AreEqual(10, lruCache.Size);
            ClassicAssert.AreEqual(ColumnChangeCaseEnum.LOWERCASE, configDBRef.ColumnChangeCase);
            ClassicAssert.AreEqual(MetadataOriginEnum.SAMPLE, configDBRef.MetadataRetrievalEnum);
            //ClassicAssert.AreEqual(2, configDBRef.DataTypesMapping.Count);
            //ClassicAssert.AreEqual("int", configDBRef.DataTypesMapping[2]);
            //ClassicAssert.AreEqual("float", configDBRef.DataTypesMapping[6]);

            // assert database reference - driver manager config
            configDBRef = common.DatabaseReferences.Get("mydb2");

            var dmDef = (DriverConnectionFactoryDesc) configDBRef.ConnectionFactoryDesc;
            var dmDriver = DbDriverConnectionHelper.ResolveDriver(container, dmDef);
            ClassicAssert.AreEqual("com.espertech.esper.epl.db.drivers.DbDriverPgSQL", dmDriver.GetType().FullName);
            ClassicAssert.AreEqual("Host=localhost;Database=esper;Username=esper;Password=3sp3rP@ssw0rd;", dmDriver.ConnectionString);

            ClassicAssert.AreEqual(ConnectionLifecycleEnum.RETAIN, configDBRef.ConnectionLifecycleEnum);
            ClassicAssert.AreEqual(false, configDBRef.ConnectionSettings.AutoCommit);
            ClassicAssert.AreEqual("esper", configDBRef.ConnectionSettings.Catalog);
            ClassicAssert.AreEqual(IsolationLevel.ReadCommitted, configDBRef.ConnectionSettings.TransactionIsolation);
            var expCache = (ConfigurationCommonCacheExpiryTime) configDBRef.DataCacheDesc;

            ClassicAssert.AreEqual(60.5, expCache.MaxAgeSeconds);
            ClassicAssert.AreEqual(120.1, expCache.PurgeIntervalSeconds);
            ClassicAssert.AreEqual(CacheReferenceType.HARD, expCache.CacheReferenceType);
            ClassicAssert.AreEqual(ColumnChangeCaseEnum.UPPERCASE, configDBRef.ColumnChangeCase);
            ClassicAssert.AreEqual(MetadataOriginEnum.METADATA, configDBRef.MetadataRetrievalEnum);
            //ClassicAssert.AreEqual(1, configDBRef.DataTypesMapping.Count);
            //ClassicAssert.AreEqual("System.String", configDBRef.DataTypesMapping[99]);

            ClassicAssert.AreEqual(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE, common.EventMeta.ClassPropertyResolutionStyle);
            ClassicAssert.AreEqual(AccessorStyle.PUBLIC, common.EventMeta.DefaultAccessorStyle);
            ClassicAssert.AreEqual(EventUnderlyingType.MAP, common.EventMeta.DefaultEventRepresentation);
            ClassicAssert.IsTrue(common.EventMeta.IsEnableXmlXsd);
            ClassicAssert.IsTrue(common.EventMeta.AvroSettings.IsEnableAvro);
            ClassicAssert.IsFalse(common.EventMeta.AvroSettings.IsEnableNativeString);
            ClassicAssert.IsFalse(common.EventMeta.AvroSettings.IsEnableSchemaDefaultNonNull);
            ClassicAssert.AreEqual("myObjectValueTypeWidenerFactoryClass", common.EventMeta.AvroSettings.ObjectValueTypeWidenerFactoryClass);
            ClassicAssert.AreEqual("myTypeToRepresentationMapperClass", common.EventMeta.AvroSettings.TypeRepresentationMapperClass);

            ClassicAssert.IsTrue(common.Logging.IsEnableQueryPlan);
            ClassicAssert.IsTrue(common.Logging.IsEnableADO);

            ClassicAssert.AreEqual(TimeUnit.MICROSECONDS, common.TimeSource.TimeUnit);

            // variables
            ClassicAssert.AreEqual(3, common.Variables.Count);
            var variable = common.Variables.Get("var1");
            ClassicAssert.AreEqual(typeof(int?).FullName, variable.VariableType);
            ClassicAssert.AreEqual("1", variable.InitializationValue);
            ClassicAssert.IsFalse(variable.IsConstant);
            variable = common.Variables.Get("var2");
            ClassicAssert.AreEqual(typeof(string).FullName, variable.VariableType);
            ClassicAssert.IsNull(variable.InitializationValue);
            ClassicAssert.IsFalse(variable.IsConstant);
            variable = common.Variables.Get("var3");
            ClassicAssert.IsTrue(variable.IsConstant);

            // method references
            ClassicAssert.AreEqual(2, common.MethodInvocationReferences.Count);
            var methodRef = common.MethodInvocationReferences.Get("abc");
            expCache = (ConfigurationCommonCacheExpiryTime) methodRef.DataCacheDesc;
            ClassicAssert.AreEqual(91.0, expCache.MaxAgeSeconds);
            ClassicAssert.AreEqual(92.2, expCache.PurgeIntervalSeconds);
            ClassicAssert.AreEqual(CacheReferenceType.WEAK, expCache.CacheReferenceType);

            methodRef = common.MethodInvocationReferences.Get("def");
            lruCache = (ConfigurationCommonCacheLRU) methodRef.DataCacheDesc;
            ClassicAssert.AreEqual(20, lruCache.Size);

            // variance types
            ClassicAssert.AreEqual(1, common.VariantStreams.Count);
            var configVStream = common.VariantStreams.Get("MyVariantStream");
            ClassicAssert.AreEqual(2, configVStream.VariantTypeNames.Count);
            ClassicAssert.IsTrue(configVStream.VariantTypeNames.Contains("MyEvenTypetNameOne"));
            ClassicAssert.IsTrue(configVStream.VariantTypeNames.Contains("MyEvenTypetNameTwo"));
            ClassicAssert.AreEqual(TypeVariance.ANY, configVStream.TypeVariance);

            ClassicAssert.AreEqual(ThreadingProfile.LARGE, common.Execution.ThreadingProfile);

            ClassicAssert.AreEqual(2, common.EventTypeAutoNameNamespaces.Count);
            ClassicAssert.IsTrue(common.EventTypeAutoNameNamespaces.Contains("com.mycompany.eventsone"));
            ClassicAssert.IsTrue(common.EventTypeAutoNameNamespaces.Contains("com.mycompany.eventstwo"));
            ClassicAssert.AreEqual("com.mycompany.eventsone", common.EventTypeAutoNameNamespaces.ToArray()[0]);
            ClassicAssert.AreEqual("com.mycompany.eventstwo", common.EventTypeAutoNameNamespaces.ToArray()[1]);

            /*
             * COMPILER
             *
             */

            // assert custom view implementations
            var configViews = compiler.PlugInViews;
            ClassicAssert.AreEqual(2, configViews.Count);
            for (var i = 0; i < configViews.Count; i++)
            {
                var entry = configViews[i];
                ClassicAssert.AreEqual("ext" + i, entry.Namespace);
                ClassicAssert.AreEqual("myview" + i, entry.Name);
                ClassicAssert.AreEqual("com.mycompany.MyViewForge" + i, entry.ForgeClassName);
            }

            // assert custom virtual data window implementations
            var configVDW = compiler.PlugInVirtualDataWindows;
            ClassicAssert.AreEqual(2, configVDW.Count);
            for (var i = 0; i < configVDW.Count; i++)
            {
                var entry = configVDW[i];
                ClassicAssert.AreEqual("vdw" + i, entry.Namespace);
                ClassicAssert.AreEqual("myvdw" + i, entry.Name);
                ClassicAssert.AreEqual("com.mycompany.MyVdwForge" + i, entry.ForgeClassName);
                if (i == 1)
                {
                    ClassicAssert.AreEqual("abc", entry.Config);
                }
            }

            // assert plug-in aggregation function loaded
            ClassicAssert.AreEqual(2, compiler.PlugInAggregationFunctions.Count);
            var pluginAgg = compiler.PlugInAggregationFunctions[0];
            ClassicAssert.AreEqual("func1a", pluginAgg.Name);
            ClassicAssert.AreEqual("com.mycompany.MyMatrixAggregationMethod0Forge", pluginAgg.ForgeClassName);
            pluginAgg = compiler.PlugInAggregationFunctions[1];
            ClassicAssert.AreEqual("func2a", pluginAgg.Name);
            ClassicAssert.AreEqual("com.mycompany.MyMatrixAggregationMethod1Forge", pluginAgg.ForgeClassName);

            // assert plug-in aggregation multi-function loaded
            ClassicAssert.AreEqual(1, compiler.PlugInAggregationMultiFunctions.Count);
            var pluginMultiAgg = compiler.PlugInAggregationMultiFunctions[0];
            EPAssertionUtil.AssertEqualsExactOrder(new[] { "func1", "func2" }, pluginMultiAgg.FunctionNames);
            ClassicAssert.AreEqual("com.mycompany.MyAggregationMultiFunctionForge", pluginMultiAgg.MultiFunctionForgeClassName);
            ClassicAssert.AreEqual(1, pluginMultiAgg.AdditionalConfiguredProperties.Count);
            ClassicAssert.AreEqual("value1", pluginMultiAgg.AdditionalConfiguredProperties.Get("prop1"));

            // assert plug-in single-row function loaded
            ClassicAssert.AreEqual(2, compiler.PlugInSingleRowFunctions.Count);
            var pluginSingleRow = compiler.PlugInSingleRowFunctions[0];
            ClassicAssert.AreEqual("com.mycompany.MyMatrixSingleRowMethod0", pluginSingleRow.FunctionClassName);
            ClassicAssert.AreEqual("method1", pluginSingleRow.FunctionMethodName);
            ClassicAssert.AreEqual("func3", pluginSingleRow.Name);
            ClassicAssert.AreEqual(ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED, pluginSingleRow.ValueCache);
            ClassicAssert.AreEqual(ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED, pluginSingleRow.FilterOptimizable);
            ClassicAssert.IsFalse(pluginSingleRow.RethrowExceptions);
            pluginSingleRow = compiler.PlugInSingleRowFunctions[1];
            ClassicAssert.AreEqual("com.mycompany.MyMatrixSingleRowMethod1", pluginSingleRow.FunctionClassName);
            ClassicAssert.AreEqual("func4", pluginSingleRow.Name);
            ClassicAssert.AreEqual("method2", pluginSingleRow.FunctionMethodName);
            ClassicAssert.AreEqual(ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.ENABLED, pluginSingleRow.ValueCache);
            ClassicAssert.AreEqual(ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.DISABLED, pluginSingleRow.FilterOptimizable);
            ClassicAssert.IsTrue(pluginSingleRow.RethrowExceptions);
            ClassicAssert.AreEqual("XYZEventTypeName", pluginSingleRow.EventTypeName);

            // assert plug-in guard objects loaded
            ClassicAssert.AreEqual(4, compiler.PlugInPatternObjects.Count);
            var pluginPattern = compiler.PlugInPatternObjects[0];
            ClassicAssert.AreEqual("com.mycompany.MyGuardForge0", pluginPattern.ForgeClassName);
            ClassicAssert.AreEqual("ext0", pluginPattern.Namespace);
            ClassicAssert.AreEqual("guard1", pluginPattern.Name);
            ClassicAssert.AreEqual(PatternObjectType.GUARD, pluginPattern.PatternObjectType);
            pluginPattern = compiler.PlugInPatternObjects[1];
            ClassicAssert.AreEqual("com.mycompany.MyGuardForge1", pluginPattern.ForgeClassName);
            ClassicAssert.AreEqual("ext1", pluginPattern.Namespace);
            ClassicAssert.AreEqual("guard2", pluginPattern.Name);
            ClassicAssert.AreEqual(PatternObjectType.GUARD, pluginPattern.PatternObjectType);
            pluginPattern = compiler.PlugInPatternObjects[2];
            ClassicAssert.AreEqual("com.mycompany.MyObserverForge0", pluginPattern.ForgeClassName);
            ClassicAssert.AreEqual("ext0", pluginPattern.Namespace);
            ClassicAssert.AreEqual("observer1", pluginPattern.Name);
            ClassicAssert.AreEqual(PatternObjectType.OBSERVER, pluginPattern.PatternObjectType);
            pluginPattern = compiler.PlugInPatternObjects[3];
            ClassicAssert.AreEqual("com.mycompany.MyObserverForge1", pluginPattern.ForgeClassName);
            ClassicAssert.AreEqual("ext1", pluginPattern.Namespace);
            ClassicAssert.AreEqual("observer2", pluginPattern.Name);
            ClassicAssert.AreEqual(PatternObjectType.OBSERVER, pluginPattern.PatternObjectType);

            // assert plug-in date-time method and enum-method
            IList<ConfigurationCompilerPlugInDateTimeMethod> configDTM = compiler.PlugInDateTimeMethods;
            ClassicAssert.AreEqual(1, configDTM.Count);
            ConfigurationCompilerPlugInDateTimeMethod dtmOne = configDTM[0];
            ClassicAssert.AreEqual("methodname1", dtmOne.Name);
            ClassicAssert.AreEqual("com.mycompany.MyDateTimeMethodForge", dtmOne.ForgeClassName);
            IList<ConfigurationCompilerPlugInEnumMethod> configENM = compiler.PlugInEnumMethods;
            ClassicAssert.AreEqual(1, configENM.Count);
            ConfigurationCompilerPlugInEnumMethod enmOne = configENM[0];
            ClassicAssert.AreEqual("methodname2", enmOne.Name);
            ClassicAssert.AreEqual("com.mycompany.MyEnumMethodForge", enmOne.ForgeClassName);

            ClassicAssert.IsTrue(compiler.ViewResources.IsIterableUnbound);
            ClassicAssert.IsFalse(compiler.ViewResources.IsOutputLimitOpt);

            ClassicAssert.IsTrue(compiler.Logging.IsEnableCode);
            ClassicAssert.IsTrue(compiler.Logging.IsEnableFilterPlan);

            ClassicAssert.AreEqual(StreamSelector.RSTREAM_ISTREAM_BOTH, compiler.StreamSelection.DefaultStreamSelector);

            var byteCode = compiler.ByteCode;
            ClassicAssert.IsTrue(byteCode.IsIncludeComments);
            ClassicAssert.IsTrue(byteCode.IsIncludeDebugSymbols);
            ClassicAssert.IsFalse(byteCode.IsAttachEPL);
            ClassicAssert.IsTrue(byteCode.IsAttachModuleEPL);
            ClassicAssert.IsTrue(byteCode.IsAttachPatternEPL);
            ClassicAssert.IsTrue(byteCode.IsInstrumented);
            ClassicAssert.IsTrue(byteCode.IsAllowSubscriber);
            ClassicAssert.AreEqual(NameAccessModifier.INTERNAL, byteCode.AccessModifierContext);
            ClassicAssert.AreEqual(NameAccessModifier.PUBLIC, byteCode.AccessModifierEventType);
            ClassicAssert.AreEqual(NameAccessModifier.INTERNAL, byteCode.AccessModifierExpression);
            ClassicAssert.AreEqual(NameAccessModifier.PUBLIC, byteCode.AccessModifierNamedWindow);
            ClassicAssert.AreEqual(NameAccessModifier.INTERNAL, byteCode.AccessModifierScript);
            ClassicAssert.AreEqual(NameAccessModifier.PUBLIC, byteCode.AccessModifierTable);
            ClassicAssert.AreEqual(NameAccessModifier.INTERNAL, byteCode.AccessModifierVariable);
            ClassicAssert.AreEqual(NameAccessModifier.PUBLIC, byteCode.AccessModifierInlinedClass);
            ClassicAssert.AreEqual(EventTypeBusModifier.BUS, byteCode.BusModifierEventType);
            ClassicAssert.AreEqual(1234, byteCode.ThreadPoolCompilerNumThreads);
            ClassicAssert.AreEqual(4321, (int) byteCode.ThreadPoolCompilerCapacity);
            ClassicAssert.AreEqual(5555, byteCode.MaxMethodsPerClass);
            ClassicAssert.IsFalse(byteCode.IsAllowInlinedClass);
            ClassicAssert.AreEqual(StreamSelector.RSTREAM_ISTREAM_BOTH, compiler.StreamSelection.DefaultStreamSelector);

            ClassicAssert.AreEqual(100, compiler.Execution.FilterServiceMaxFilterWidth);
            ClassicAssert.AreEqual(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, compiler.Execution.FilterIndexPlanning);
            ClassicAssert.IsFalse(compiler.Execution.IsEnabledDeclaredExprValueCache);

            ClassicAssert.IsTrue(compiler.Language.IsSortUsingCollator);

            ClassicAssert.IsTrue(compiler.Expression.IsIntegerDivision);
            ClassicAssert.IsTrue(compiler.Expression.IsDivisionByZeroReturnsNull);
            ClassicAssert.IsFalse(compiler.Expression.IsUdfCache);
            ClassicAssert.IsFalse(compiler.Expression.IsExtendedAggregation);
            ClassicAssert.IsTrue(compiler.Expression.IsDuckTyping);
            ClassicAssert.AreEqual(2, compiler.Expression.MathContext.Precision);
            ClassicAssert.AreEqual(MidpointRounding.ToEven, compiler.Expression.MathContext.RoundingMode);

            ClassicAssert.AreEqual("abc", compiler.Scripts.DefaultDialect);
            ClassicAssert.IsFalse(compiler.Scripts.IsEnabled);

            ClassicAssert.IsFalse(compiler.Serde.IsEnableExtendedBuiltin);
            ClassicAssert.IsTrue(compiler.Serde.IsEnableExternalizable);
            ClassicAssert.IsTrue(compiler.Serde.IsEnableSerializable);
            ClassicAssert.IsTrue(compiler.Serde.IsEnableSerializationFallback);
            IList<String> serdeProviderFactories = compiler.Serde.SerdeProviderFactories;
            ClassicAssert.AreEqual(2, serdeProviderFactories.Count);
            ClassicAssert.AreEqual("a.b.c.MySerdeProviderFactoryOne", serdeProviderFactories[0]);
            ClassicAssert.AreEqual("a.b.c.MySerdeProviderFactoryTwo", serdeProviderFactories[1]);
            
            /*
             * RUNTIME
             *
             */

            // assert runtime defaults
            ClassicAssert.IsFalse(runtime.Threading.IsInsertIntoDispatchPreserveOrder);
            ClassicAssert.AreEqual(3000, runtime.Threading.InsertIntoDispatchTimeout);
            ClassicAssert.AreEqual(Locking.SUSPEND, runtime.Threading.InsertIntoDispatchLocking);
            ClassicAssert.IsFalse(runtime.Threading.IsNamedWindowConsumerDispatchPreserveOrder);
            ClassicAssert.AreEqual(4000, runtime.Threading.NamedWindowConsumerDispatchTimeout);
            ClassicAssert.AreEqual(Locking.SUSPEND, runtime.Threading.NamedWindowConsumerDispatchLocking);

            ClassicAssert.IsFalse(runtime.Threading.IsListenerDispatchPreserveOrder);
            ClassicAssert.AreEqual(2000, runtime.Threading.ListenerDispatchTimeout);
            ClassicAssert.AreEqual(Locking.SUSPEND, runtime.Threading.ListenerDispatchLocking);
            ClassicAssert.IsTrue(runtime.Threading.IsThreadPoolInbound);
            ClassicAssert.IsTrue(runtime.Threading.IsThreadPoolOutbound);
            ClassicAssert.IsTrue(runtime.Threading.IsThreadPoolRouteExec);
            ClassicAssert.IsTrue(runtime.Threading.IsThreadPoolTimerExec);
            ClassicAssert.AreEqual(1, runtime.Threading.ThreadPoolInboundNumThreads);
            ClassicAssert.AreEqual(2, runtime.Threading.ThreadPoolOutboundNumThreads);
            ClassicAssert.AreEqual(3, runtime.Threading.ThreadPoolTimerExecNumThreads);
            ClassicAssert.AreEqual(4, runtime.Threading.ThreadPoolRouteExecNumThreads);
            ClassicAssert.AreEqual(1000, (int) runtime.Threading.ThreadPoolInboundCapacity);
            ClassicAssert.AreEqual(1500, (int) runtime.Threading.ThreadPoolOutboundCapacity);
            ClassicAssert.IsNull(runtime.Threading.ThreadPoolTimerExecCapacity);
            ClassicAssert.AreEqual(2000, (int) runtime.Threading.ThreadPoolRouteExecCapacity);
            ClassicAssert.IsTrue(runtime.Threading.IsRuntimeFairlock);

            ClassicAssert.IsFalse(runtime.Threading.IsInternalTimerEnabled);
            ClassicAssert.AreEqual(1234567, runtime.Threading.InternalTimerMsecResolution);
            ClassicAssert.IsTrue(runtime.Logging.IsEnableExecutionDebug);
            ClassicAssert.IsFalse(runtime.Logging.IsEnableTimerDebug);
            ClassicAssert.IsTrue(runtime.Logging.IsEnableLockActivity);
            ClassicAssert.AreEqual("[%u] %m", runtime.Logging.AuditPattern);
            ClassicAssert.AreEqual(30000, runtime.Variables.MsecVersionRelease);
            ClassicAssert.AreEqual(3L, (long) runtime.Patterns.MaxSubexpressions);
            ClassicAssert.IsFalse(runtime.Patterns.IsMaxSubexpressionPreventStart);
            ClassicAssert.AreEqual(3L, (long) runtime.MatchRecognize.MaxStates);
            ClassicAssert.IsFalse(runtime.MatchRecognize.IsMaxStatesPreventStart);

            // assert adapter loaders parsed
            IList<ConfigurationRuntimePluginLoader> plugins = runtime.PluginLoaders;
            ClassicAssert.AreEqual(2, plugins.Count);
            var pluginOne = plugins[0];
            ClassicAssert.AreEqual("Loader1", pluginOne.LoaderName);
            ClassicAssert.AreEqual("com.espertech.esper.support.plugin.SupportLoaderOne", pluginOne.ClassName);
            ClassicAssert.AreEqual(2, pluginOne.ConfigProperties.Count);
            ClassicAssert.AreEqual("val1", pluginOne.ConfigProperties.Get("name1"));
            ClassicAssert.AreEqual("val2", pluginOne.ConfigProperties.Get("name2"));
            ClassicAssert.AreEqual(
                "<sample-initializer xmlns=\"http://www.espertech.com/schema/esper\"><some-any-xml-can-be-here>This section for use by a plugin loader.</some-any-xml-can-be-here></sample-initializer>",
                pluginOne.ConfigurationXML);

            var pluginTwo = plugins[1];
            ClassicAssert.AreEqual("Loader2", pluginTwo.LoaderName);
            ClassicAssert.AreEqual("com.espertech.esper.support.plugin.SupportLoaderTwo", pluginTwo.ClassName);
            ClassicAssert.AreEqual(0, pluginTwo.ConfigProperties.Count);

            ClassicAssert.AreEqual(TimeSourceType.NANO, runtime.TimeSource.TimeSourceType);
            ClassicAssert.IsTrue(runtime.Execution.IsPrioritized);
            ClassicAssert.IsTrue(runtime.Execution.IsPrecedenceEnabled);
            ClassicAssert.IsTrue(runtime.Execution.IsFairlock);
            ClassicAssert.IsTrue(runtime.Execution.IsDisableLocking);
            ClassicAssert.AreEqual(FilterServiceProfile.READWRITE, runtime.Execution.FilterServiceProfile);
            ClassicAssert.AreEqual(101, runtime.Execution.DeclaredExprValueCacheSize);

            var metrics = runtime.MetricsReporting;
            ClassicAssert.IsTrue(metrics.IsEnableMetricsReporting);
            ClassicAssert.AreEqual(4000L, metrics.RuntimeInterval);
            ClassicAssert.AreEqual(500L, metrics.StatementInterval);
            ClassicAssert.IsFalse(metrics.IsThreading);
            ClassicAssert.AreEqual(2, metrics.StatementGroups.Count);
            ClassicAssert.IsTrue(metrics.IsRuntimeMetrics);
            var def = metrics.StatementGroups.Get("MyStmtGroup");
            ClassicAssert.AreEqual(5000, def.Interval);
            ClassicAssert.IsTrue(def.IsDefaultInclude);
            ClassicAssert.AreEqual(50, def.NumStatements);
            ClassicAssert.IsTrue(def.IsReportInactive);
            ClassicAssert.AreEqual(5, def.Patterns.Count);
            ClassicAssert.AreEqual(def.Patterns[0], new Pair<StringPatternSet, bool>(new StringPatternSetRegex(".*"), true));
            ClassicAssert.AreEqual(def.Patterns[1], new Pair<StringPatternSet, bool>(new StringPatternSetRegex(".*test.*"), false));
            ClassicAssert.AreEqual(def.Patterns[2], new Pair<StringPatternSet, bool>(new StringPatternSetLike("%MyMetricsStatement%"), false));
            ClassicAssert.AreEqual(
                def.Patterns[3], new Pair<StringPatternSet, bool>(new StringPatternSetLike("%MyFraudAnalysisStatement%"), true));
            ClassicAssert.AreEqual(def.Patterns[4], new Pair<StringPatternSet, bool>(new StringPatternSetLike("%SomerOtherStatement%"), true));
            def = metrics.StatementGroups.Get("MyStmtGroupTwo");
            ClassicAssert.AreEqual(200, def.Interval);
            ClassicAssert.IsFalse(def.IsDefaultInclude);
            ClassicAssert.AreEqual(100, def.NumStatements);
            ClassicAssert.IsFalse(def.IsReportInactive);
            ClassicAssert.AreEqual(0, def.Patterns.Count);
            ClassicAssert.IsFalse(runtime.Expression.IsSelfSubselectPreeval);
            ClassicAssert.AreEqual(TimeZoneHelper.GetTimeZoneInfo("GMT-4:00"), runtime.Expression.TimeZone);
            ClassicAssert.AreEqual(2, runtime.ExceptionHandling.HandlerFactories.Count);
            ClassicAssert.AreEqual("my.company.cep.LoggingExceptionHandlerFactory", runtime.ExceptionHandling.HandlerFactories[0]);
            ClassicAssert.AreEqual("my.company.cep.AlertExceptionHandlerFactory", runtime.ExceptionHandling.HandlerFactories[1]);
            ClassicAssert.AreEqual(UndeployRethrowPolicy.RETHROW_FIRST, runtime.ExceptionHandling.UndeployRethrowPolicy);
            ClassicAssert.AreEqual(2, runtime.ConditionHandling.HandlerFactories.Count);
            ClassicAssert.AreEqual("my.company.cep.LoggingConditionHandlerFactory", runtime.ConditionHandling.HandlerFactories[0]);
            ClassicAssert.AreEqual("my.company.cep.AlertConditionHandlerFactory", runtime.ConditionHandling.HandlerFactories[1]);
        }
    }
} // end of namespace
