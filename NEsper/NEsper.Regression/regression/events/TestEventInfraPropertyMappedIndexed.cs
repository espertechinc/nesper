///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util.support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using com.espertech.esper.client.annotation;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestEventInfraPropertyMappedIndexed 
    {
	    private readonly static Type BEAN_TYPE = typeof(MyIMEvent);

	    private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();

	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();

	        _epService.EPAdministrator.Configuration.AddEventType(BEAN_TYPE);
	        AddMapEventType();
	        AddOAEventType();
	        AddAvroEventType();

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestIt() {
            RunAssertion(
                BEAN_TYPE.Name, 
                SupportEventInfra.FBEAN, 
                new MyIMEvent(new string[] { "v1", "v2" }, Collections.SingletonMap("k1", "v1")));

            RunAssertion(
                SupportEventInfra.MAP_TYPENAME, 
                SupportEventInfra.FMAP, 
                SupportEventInfra.TwoEntryMap("indexed", new string[] { "v1", "v2" }, "mapped", Collections.SingletonMap("k1", "v1")));

            RunAssertion(
                SupportEventInfra.OA_TYPENAME, 
                SupportEventInfra.FOA, 
                new object[] { new string[] { "v1", "v2" }, Collections.SingletonMap("k1", "v1") });

	        // Avro
	        GenericRecord datum = new GenericRecord(GetAvroSchema());
	        datum.Put("indexed", Collections.List("v1", "v2"));
            datum.Put("mapped", Collections.SingletonMap("k1", "v1"));
            RunAssertion(SupportEventInfra.AVRO_TYPENAME, SupportEventInfra.FAVRO, datum);
	    }

	    private void RunAssertion(string typename, FunctionSendEvent send, object underlying) {

	        RunAssertionTypeValidProp(typename, underlying);
	        RunAssertionTypeInvalidProp(typename);

	        string stmtText = "select * from " + typename;

	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        SupportUpdateListener listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        send.Invoke(_epService, underlying);
	        EventBean @event = listener.AssertOneGetNewAndReset();

	        EventPropertyGetterMapped mappedGetter = @event.EventType.GetGetterMapped("mapped");
	        Assert.AreEqual("v1", mappedGetter.Get(@event, "k1"));

	        EventPropertyGetterIndexed indexedGetter = @event.EventType.GetGetterIndexed("indexed");
	        Assert.AreEqual("v2", indexedGetter.Get(@event, 1));

	        RunAssertionEventInvalidProp(@event);
	        SupportEventTypeAssertionUtil.AssertConsistency(@event);

	        stmt.Dispose();
	    }

	    private void AddMapEventType() {
            _epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.MAP_TYPENAME, SupportEventInfra.TwoEntryMap("indexed", typeof(string[]), "mapped", typeof(IDictionary<string, string>)));
	    }

	    private void AddOAEventType() {
	        string[] names = { "indexed", "mapped" };
            object[] types = { typeof(string[]), typeof(IDictionary<string, string>) };
            _epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.OA_TYPENAME, names, types);
	    }

	    private void AddAvroEventType() {
            _epService.EPAdministrator.Configuration.AddEventTypeAvro(SupportEventInfra.AVRO_TYPENAME, new ConfigurationEventTypeAvro(GetAvroSchema()));
	    }

	    private static RecordSchema GetAvroSchema()
	    {
	        return SchemaBuilder.Record(
	            "AvroSchema",
	            TypeBuilder.Field(
	                "indexed", TypeBuilder.Array(
	                    TypeBuilder.String(
                            TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE)))),
	            TypeBuilder.Field(
	                "mapped", TypeBuilder.Map(
	                    TypeBuilder.String(TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE))))
	        );

            //return SchemaBuilder.Record("AvroSchema").Fields()
	           //    .Name("indexed").Type(Array().Items().StringBuilder().Prop(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE).EndString()).NoDefault()
	           //    .Name("mapped").Type(Map().Values().StringBuilder().Prop(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE).EndString()).NoDefault()
	           //    .EndRecord();
	    }

	    private void RunAssertionEventInvalidProp(EventBean @event) {
	        foreach (string prop in Collections.List("xxxx", "mapped[1]", "indexed('a')", "mapped.x", "indexed.x")) {
	            SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
	            SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
	        }
	    }

	    private void RunAssertionTypeValidProp(string typeName, object underlying) {
	        EventType eventType = _epService.EPAdministrator.Configuration.GetEventType(typeName);

            object[][] expectedType = {
                new object[]{ "indexed", typeof(string[]), null, null }, 
                new object[]{ "mapped", typeof(IDictionary<string, string>), null, null }
            };
	        SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, eventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

	        EPAssertionUtil.AssertEqualsAnyOrder(new string[] {"indexed", "mapped"}, eventType.PropertyNames);

	        Assert.IsNotNull(eventType.GetGetter("mapped"));
	        Assert.IsNotNull(eventType.GetGetter("mapped('a')"));
	        Assert.IsNotNull(eventType.GetGetter("indexed"));
	        Assert.IsNotNull(eventType.GetGetter("indexed[0]"));
	        Assert.IsTrue(eventType.IsProperty("mapped"));
	        Assert.IsTrue(eventType.IsProperty("mapped('a')"));
	        Assert.IsTrue(eventType.IsProperty("indexed"));
	        Assert.IsTrue(eventType.IsProperty("indexed[0]"));
            Assert.AreEqual(typeof(IDictionary<string, string>), eventType.GetPropertyType("mapped"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("mapped('a')"));
	        Assert.AreEqual(typeof(string[]), eventType.GetPropertyType("indexed"));
	        Assert.AreEqual(typeof(string), eventType.GetPropertyType("indexed[0]"));

	        Assert.AreEqual(new EventPropertyDescriptor("indexed", typeof(string[]), typeof(string), false, false, true, false, false), eventType.GetPropertyDescriptor("indexed"));
            Assert.AreEqual(new EventPropertyDescriptor("mapped", typeof(IDictionary<string, string>), typeof(string), false, false, false, true, false), eventType.GetPropertyDescriptor("mapped"));

	        Assert.IsNull(eventType.GetFragmentType("indexed"));
	        Assert.IsNull(eventType.GetFragmentType("mapped"));
	    }

	    private void RunAssertionTypeInvalidProp(string typeName) {
	        EventType eventType = _epService.EPAdministrator.Configuration.GetEventType(typeName);

	        foreach (string prop in Collections.List("xxxx", "myString[0]", "indexed('a')", "indexed.x", "mapped[0]", "mapped.x")) {
	            Assert.AreEqual(false, eventType.IsProperty(prop));
	            Assert.AreEqual(null, eventType.GetPropertyType(prop));
	            Assert.IsNull(eventType.GetPropertyDescriptor(prop));
	        }
	    }

	    public class MyIMEvent
        {
	        public MyIMEvent(string[] indexed, IDictionary<string, string> mapped)
            {
	            Indexed = indexed;
	            Mapped = mapped;
	        }

            [PropertyName("indexed")]
	        public string[] Indexed { get; private set; }

            [PropertyName("mapped")]
            public IDictionary<string, string> Mapped { get; private set; }
	    }
	}
} // end of namespace
