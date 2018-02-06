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

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NEsper.Avro;
using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestInsertIntoPopulateCreateStreamAvro
    {
	    private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp() {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
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
	    public void TestAvroSchema() {
	        RunAssertionCompatExisting();
	        RunAssertionNewSchema();
	    }

	    private void RunAssertionCompatExisting() {

	        string epl = "insert into AvroExistingType select 1 as myLong," +
	                     "{1L, 2L} as myLongArray," +
	                     this.GetType().FullName + ".MakeByteArray() as myByteArray, " +
	                     this.GetType().FullName + ".MakeMapStringString() as myMap " +
	                     "from SupportBean";

            Schema schema = SchemaBuilder.Record("name",
                TypeBuilder.RequiredLong("myLong"),
                TypeBuilder.Field("myLongArray", TypeBuilder.Array(TypeBuilder.Long())),
                TypeBuilder.Field("myByteArray", TypeBuilder.Bytes()),
                TypeBuilder.Field("myMap", TypeBuilder.Map(TypeBuilder.String(
                    TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE)))));

            _epService.EPAdministrator.Configuration.AddEventTypeAvro("AvroExistingType", new ConfigurationEventTypeAvro(schema));

	        EPStatement statement = _epService.EPAdministrator.CreateEPL(epl);
	        SupportUpdateListener listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        EventBean @event = listener.AssertOneGetNewAndReset();
	        SupportAvroUtil.AvroToJson(@event);
	        Assert.AreEqual(1L, @event.Get("myLong"));
	        EPAssertionUtil.AssertEqualsExactOrder(new long[] {1L, 2L}, @event.Get("myLongArray").UnwrapIntoArray<long>());
	        Assert.IsTrue(CompatExtensions.IsEqual(new byte[] {1, 2, 3}, ((byte[]) @event.Get("myByteArray"))));
            Assert.AreEqual("[k1=v1]", CompatExtensions.Render(@event.Get("myMap").AsStringDictionary()));

	        statement.Dispose();
	    }

	    private void RunAssertionNewSchema() {

	        string epl = EventRepresentationChoice.AVRO.GetAnnotationText() + " select 1 as myInt," +
	                     "{1L, 2L} as myLongArray," +
	                     this.GetType().FullName + ".MakeByteArray() as myByteArray, " +
	                     this.GetType().FullName + ".MakeMapStringString() as myMap " +
	                     "from SupportBean";

	        EPStatement statement = _epService.EPAdministrator.CreateEPL(epl);
	        SupportUpdateListener listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        EventBean @event = listener.AssertOneGetNewAndReset();
	        string json = SupportAvroUtil.AvroToJson(@event);
	        Console.WriteLine(json);
	        Assert.AreEqual(1, @event.Get("myInt"));
            EPAssertionUtil.AssertEqualsExactOrder(new long[] { 1L, 2L }, @event.Get("myLongArray").UnwrapIntoArray<long>());
	        Assert.IsTrue(CompatExtensions.IsEqual(new byte[] {1, 2, 3}, ((byte[]) @event.Get("myByteArray"))));
	        Assert.AreEqual("[k1=v1]", CompatExtensions.Render(@event.Get("myMap").AsStringDictionary()));

            var designSchema = SchemaBuilder.Record("name",
                TypeBuilder.RequiredInt("myInt"),
                TypeBuilder.Field("myLongArray", TypeBuilder.Array(TypeBuilder.Union(TypeBuilder.Null(), TypeBuilder.Long()))),
                TypeBuilder.Field("myByteArray", TypeBuilder.Bytes()),
                TypeBuilder.Field("myMap", TypeBuilder.Map(
                    TypeBuilder.String(TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE)))));

	        var assembledSchema = ((AvroEventType) @event.EventType).SchemaAvro;
	        var compareMsg = SupportAvroUtil.CompareSchemas(designSchema, assembledSchema);
	        Assert.IsNull(compareMsg, compareMsg);

	        statement.Dispose();
	    }

	    public static byte[] MakeByteArray() {
	        return new byte[] {1, 2, 3};
	    }

	    public static IDictionary<string, string> MakeMapStringString() {
	        return Collections.SingletonMap("k1", "v1");
	    }
	}
} // end of namespace
