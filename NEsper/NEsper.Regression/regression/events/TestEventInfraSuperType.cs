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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.events;

using NUnit.Framework;
using NEsper.Avro.Extensions;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestEventInfraSuperType
    {
	    private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();

	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();

	        AddMapEventTypes();
	        AddOAEventTypes();
	        AddAvroEventTypes();
	        AddBeanTypes();
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportMarkerInterface));

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
	        // Bean
	        RunAssertion(
                "Bean", SupportEventInfra.FBEANWTYPE, 
                new Bean_Type_Root(),
                new Bean_Type_1(), 
                new Bean_Type_2(), 
                new Bean_Type_2_1());

	        // Map
	        RunAssertion(
                "Map", SupportEventInfra.FMAPWTYPE, 
                new Dictionary<string, object>(),
                new Dictionary<string, object>(),
                new Dictionary<string, object>(),
                new Dictionary<string, object>());

	        // OA
	        RunAssertion(
                "OA", SupportEventInfra.FOAWTYPE, 
                new object[0], 
                new object[0],
                new object[0], 
                new object[0]);

            // Avro
            RecordSchema fake = SchemaBuilder.Record("fake");
            RunAssertion(
                "Avro", SupportEventInfra.FAVROWTYPE, 
                new GenericRecord(fake),
                new GenericRecord(fake),
                new GenericRecord(fake),
                new GenericRecord(fake));
	    }

	    private void RunAssertion(string typePrefix, FunctionSendEventWType sender, object root, object type_1, object type_2, object type_2_1)
        {
	        var typeNames = "Type_Root,Type_1,Type_2,Type_2_1".SplitCsv();
	        var statements = new EPStatement[4];
	        var listeners = new SupportUpdateListener[4];
	        for (var i = 0; i < typeNames.Length; i++) {
	            statements[i] = _epService.EPAdministrator.CreateEPL("select * from " + typePrefix + "_" + typeNames[i]);
	            listeners[i] = new SupportUpdateListener();
	            statements[i].AddListener(listeners[i]);
	        }

	        sender.Invoke(_epService, root, typePrefix + "_" + typeNames[0]);
	        EPAssertionUtil.AssertEqualsExactOrder(new bool[] {true, false, false, false}, SupportUpdateListener.GetInvokedFlagsAndReset(listeners));

            sender.Invoke(_epService, type_1, typePrefix + "_" + typeNames[1]);
            EPAssertionUtil.AssertEqualsExactOrder(new bool[] { true, true, false, false }, SupportUpdateListener.GetInvokedFlagsAndReset(listeners));

            sender.Invoke(_epService, type_2, typePrefix + "_" + typeNames[2]);
            EPAssertionUtil.AssertEqualsExactOrder(new bool[] { true, false, true, false }, SupportUpdateListener.GetInvokedFlagsAndReset(listeners));

            sender.Invoke(_epService, type_2_1, typePrefix + "_" + typeNames[3]);
            EPAssertionUtil.AssertEqualsExactOrder(new bool[] { true, false, true, true }, SupportUpdateListener.GetInvokedFlagsAndReset(listeners));

	        for (var i = 0; i < statements.Length; i++) {
	            statements[i].Dispose();
	        }
	    }

	    private void AddMapEventTypes() {
            _epService.EPAdministrator.Configuration.AddEventType("Map_Type_Root", Collections.EmptyDataMap);
            _epService.EPAdministrator.Configuration.AddEventType("Map_Type_1", Collections.EmptyDataMap, new string[] { "Map_Type_Root" });
	        _epService.EPAdministrator.Configuration.AddEventType("Map_Type_2", Collections.EmptyDataMap, new string[] {"Map_Type_Root"});
            _epService.EPAdministrator.Configuration.AddEventType("Map_Type_2_1", Collections.EmptyDataMap, new string[] { "Map_Type_2" });
	    }

	    private void AddOAEventTypes() {
	        _epService.EPAdministrator.Configuration.AddEventType("OA_Type_Root", new string[0], new object[0]);

	        var array_1 = new ConfigurationEventTypeObjectArray();
            array_1.SuperTypes = Collections.SingletonList("OA_Type_Root");
	        _epService.EPAdministrator.Configuration.AddEventType("OA_Type_1", new string[0], new object[0], array_1);

	        var array_2 = new ConfigurationEventTypeObjectArray();
            array_2.SuperTypes = Collections.SingletonList("OA_Type_Root");
	        _epService.EPAdministrator.Configuration.AddEventType("OA_Type_2", new string[0], new object[0], array_2);

	        var array_2_1 = new ConfigurationEventTypeObjectArray();
            array_2_1.SuperTypes = Collections.SingletonList("OA_Type_2");
	        _epService.EPAdministrator.Configuration.AddEventType("OA_Type_2_1", new string[0], new object[0], array_2_1);
	    }

	    private void AddAvroEventTypes() {
	        Schema fake = SchemaBuilder.Record("fake");
	        var avro_root = new ConfigurationEventTypeAvro();
	        avro_root.AvroSchema = fake;
	        _epService.EPAdministrator.Configuration.AddEventTypeAvro("Avro_Type_Root", avro_root);
	        var avro_1 = new ConfigurationEventTypeAvro();
            avro_1.SuperTypes = Collections.SingletonList("Avro_Type_Root");
	        avro_1.AvroSchema = fake;
	        _epService.EPAdministrator.Configuration.AddEventTypeAvro("Avro_Type_1", avro_1);
	        var avro_2 = new ConfigurationEventTypeAvro();
            avro_2.SuperTypes = Collections.SingletonList("Avro_Type_Root");
	        avro_2.AvroSchema = fake;
	        _epService.EPAdministrator.Configuration.AddEventTypeAvro("Avro_Type_2", avro_2);
	        var avro_2_1 = new ConfigurationEventTypeAvro();
	        avro_2_1.SuperTypes = Collections.SingletonList("Avro_Type_2");
	        avro_2_1.AvroSchema = fake;
	        _epService.EPAdministrator.Configuration.AddEventTypeAvro("Avro_Type_2_1", avro_2_1);
	    }

	    private void AddBeanTypes() {
            foreach (var clazz in Collections.List(typeof(Bean_Type_Root), typeof(Bean_Type_1), typeof(Bean_Type_2), typeof(Bean_Type_2_1)))
            {
	            _epService.EPAdministrator.Configuration.AddEventType(clazz);
	        }
	    }

	    internal class Bean_Type_Root {}
        internal class Bean_Type_1 : Bean_Type_Root { }
        internal class Bean_Type_2 : Bean_Type_Root { }
        internal class Bean_Type_2_1 : Bean_Type_2 { }
	}
} // end of namespace
