///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.util;

using NUnit.Framework;

using Newtonsoft.Json.Linq;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestInsertIntoPopulateSingleColByMethodCall
    {
	    private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp() {
	        var config = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
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
	    public void TestStreamSelectConversionFunction() {
	        // define Bean
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportMarketDataBean));

	        // define Map
	        IDictionary<string, object> mapTypeInfo = new Dictionary<string, object>();
	        mapTypeInfo.Put("one", typeof(string));
	        mapTypeInfo.Put("two", typeof(string));
	        _epService.EPAdministrator.Configuration.AddEventType("MapOne", mapTypeInfo);
	        _epService.EPAdministrator.Configuration.AddEventType("MapTwo", mapTypeInfo);

	        // define OA
	        string[] props = {"one", "two"};
	        object[] types = {typeof(string), typeof(string)};
	        _epService.EPAdministrator.Configuration.AddEventType("OAOne", props, types);
	        _epService.EPAdministrator.Configuration.AddEventType("OATwo", props, types);

            // define Avro
            var schema = SchemaBuilder.Record(
                "name",
                TypeBuilder.RequiredString("one"),
                TypeBuilder.RequiredString("two"));

	        _epService.EPAdministrator.Configuration.AddEventTypeAvro("AvroOne", new ConfigurationEventTypeAvro(schema));
	        _epService.EPAdministrator.Configuration.AddEventTypeAvro("AvroTwo", new ConfigurationEventTypeAvro(schema));

	        // Bean
	        RunAssertionConversionImplicitType(
                "Bean", typeof(SupportBean).Name, "ConvertEvent", 
                typeof(BeanEventType), typeof(SupportBean), typeof(SupportMarketDataBean).FullName, 
                new SupportMarketDataBean("ACME", 0, 0L, null),
                SupportEventInfra.FBEANWTYPE, "theString".SplitCsv(), new object[] {"ACME"});

	        // Map
	        IDictionary<string, object> mapEventOne = new Dictionary<string, object>();
	        mapEventOne.Put("one", "1");
	        mapEventOne.Put("two", "2");
	        RunAssertionConversionImplicitType("Map", "MapOne", "ConvertEventMap", typeof(WrapperEventType), typeof(IDictionary<string, object>),
                                               "MapTwo", mapEventOne, SupportEventInfra.FMAPWTYPE, "one,two".SplitCsv(), new object[] { "1", "|2|" });

	        IDictionary<string, object> mapEventTwo = new Dictionary<string, object>();
	        mapEventTwo.Put("one", "3");
	        mapEventTwo.Put("two", "4");
            RunAssertionConversionConfiguredType("MapOne", "ConvertEventMap", "MapTwo", typeof(MappedEventBean), typeof(Dictionary<string, object>), mapEventTwo, SupportEventInfra.FMAPWTYPE, "one,two".SplitCsv(), new object[] { "3", "|4|" });

	        // Object-Array
	        RunAssertionConversionImplicitType("OA", "OAOne", "ConvertEventObjectArray", typeof(WrapperEventType), typeof(object[]),
                                               "OATwo", new object[] { "1", "2" }, SupportEventInfra.FOAWTYPE, "one,two".SplitCsv(), new object[] { "1", "|2|" });
            RunAssertionConversionConfiguredType("OAOne", "ConvertEventObjectArray", "OATwo", typeof(ObjectArrayBackedEventBean), typeof(object[]), new object[] { "3", "4" }, SupportEventInfra.FOAWTYPE, "one,two".SplitCsv(), new object[] { "3", "|4|" });

	        // Avro
	        var rowOne = new GenericRecord(schema);
	        rowOne.Put("one", "1");
	        rowOne.Put("two", "2");
	        RunAssertionConversionImplicitType("Avro", "AvroOne", "ConvertEventAvro", typeof(WrapperEventType), typeof(GenericRecord),
                                               "AvroTwo", rowOne, SupportEventInfra.FAVROWTYPE, "one,two".SplitCsv(), new object[] { "1", "|2|" });

	        var rowTwo = new GenericRecord(schema);
	        rowTwo.Put("one", "3");
	        rowTwo.Put("two", "4");
            RunAssertionConversionConfiguredType("AvroOne", "ConvertEventAvro", "AvroTwo", typeof(AvroGenericDataBackedEventBean), typeof(GenericRecord), rowTwo, SupportEventInfra.FAVROWTYPE, "one,two".SplitCsv(), new object[] { "3", "|4|" });
	    }

	    private void RunAssertionConversionImplicitType(string prefix,
	            string typeNameOrigin,
	            string functionName,
	            Type eventTypeType,
	            Type underlyingType,
	            string typeNameEvent,
	            object @event,
	            FunctionSendEventWType sendEvent,
	            string[] propertyName,
	            object[] propertyValues) {
	        var streamName = prefix + "_Stream";
	        var textOne = "insert into " + streamName + " select * from " + typeNameOrigin;
	        var textTwo = "insert into " + streamName + " select " + typeof(SupportStaticMethodLib).FullName + "." + functionName + "(s0) from " + typeNameEvent + " as s0";

	        var stmtOne = _epService.EPAdministrator.CreateEPL(textOne);
	        var listenerOne = new SupportUpdateListener();
	        stmtOne.AddListener(listenerOne);
	        var type = stmtOne.EventType;
	        Assert.AreEqual(underlyingType, type.UnderlyingType);

	        var stmtTwo = _epService.EPAdministrator.CreateEPL(textTwo);
	        var listenerTwo = new SupportUpdateListener();
	        stmtTwo.AddListener(listenerTwo);
	        type = stmtTwo.EventType;
	        Assert.AreEqual(underlyingType, type.UnderlyingType);

            sendEvent.Invoke(_epService, @event, typeNameEvent);

	        var theEvent = listenerTwo.AssertOneGetNewAndReset();
	        Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(theEvent.EventType.GetType(), eventTypeType));
	        Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(theEvent.Underlying.GetType(), underlyingType));
	        EPAssertionUtil.AssertProps(theEvent, propertyName, propertyValues);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionConversionConfiguredType(string typeNameTarget,
	            string functionName,
	            string typeNameOrigin,
	            Type eventBeanType,
	            Type underlyingType,
	            object @event,
	            FunctionSendEventWType sendEvent,
	            string[] propertyName,
	            object[] propertyValues) {

	        // test native
	        _epService.EPAdministrator.CreateEPL("insert into " + typeNameTarget + " select " + typeof(SupportStaticMethodLib).FullName + "." + functionName + "(s0) from " + typeNameOrigin + " as s0");
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from " + typeNameTarget);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        sendEvent.Invoke(_epService, @event, typeNameOrigin);

	        var eventBean = listener.AssertOneGetNewAndReset();
	        Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(eventBean.Underlying.GetType(), underlyingType));
	        Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(eventBean.GetType(), eventBeanType));
	        EPAssertionUtil.AssertProps(eventBean, propertyName, propertyValues);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }
	}
} // end of namespace
