///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.events;

using Newtonsoft.Json.Linq;

using NUnit.Framework;
using NEsper.Avro.Extensions;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestEventInfraPropertyDynamicSimple
    {
	    private readonly static Type BEAN_TYPE = typeof(SupportMarkerInterface);

	    private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
	        Configuration configuration = SupportConfigFactory.GetConfiguration();
	        AddXMLEventType(configuration);

	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();

	        AddMapEventType();
	        AddOAEventType();
	        _epService.EPAdministrator.Configuration.AddEventType(BEAN_TYPE);
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

	        // Bean
	        var beanTests = new Pair<SupportMarkerInterface,ValueWithExistsFlag>[] {
	            new Pair<SupportMarkerInterface,ValueWithExistsFlag>(new SupportMarkerImplA("e1"), ValueWithExistsFlag.Exists("e1")),
	            new Pair<SupportMarkerInterface,ValueWithExistsFlag>(new SupportMarkerImplB(1), ValueWithExistsFlag.Exists(1)),
	            new Pair<SupportMarkerInterface,ValueWithExistsFlag>(new SupportMarkerImplC(), ValueWithExistsFlag.NotExists())
	        };
            RunAssertion(BEAN_TYPE.Name, SupportEventInfra.FBEAN, null, beanTests, typeof(object));

	        // Map
	        var mapTests = new Pair<IDictionary<string, object>, ValueWithExistsFlag>[] {
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag>(Collections.SingletonDataMap("somekey", "10"), ValueWithExistsFlag.NotExists()),
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag>(Collections.SingletonDataMap("id", "abc"), ValueWithExistsFlag.Exists("abc")),
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag>(Collections.SingletonDataMap("id", 10), ValueWithExistsFlag.Exists(10)),
	        };
            RunAssertion(SupportEventInfra.MAP_TYPENAME, SupportEventInfra.FMAP, null, mapTests, typeof(object));

	        // Object-Array
	        var oaTests = new Pair<object[], ValueWithExistsFlag>[] {
	            new Pair<object[], ValueWithExistsFlag>(new object[] {1, null}, ValueWithExistsFlag.Exists(null)),
	            new Pair<object[], ValueWithExistsFlag>(new object[] {2, "abc"}, ValueWithExistsFlag.Exists("abc")),
	            new Pair<object[], ValueWithExistsFlag>(new object[] {3, 10}, ValueWithExistsFlag.Exists(10)),
	        };
            RunAssertion(SupportEventInfra.OA_TYPENAME, SupportEventInfra.FOA, null, oaTests, typeof(object));

	        // XML
	        var xmlTests = new Pair<string, ValueWithExistsFlag>[] {
	            new Pair<string, ValueWithExistsFlag>("", ValueWithExistsFlag.NotExists()),
	            new Pair<string, ValueWithExistsFlag>("<id>10</id>", ValueWithExistsFlag.Exists("10")),
	            new Pair<string, ValueWithExistsFlag>("<id>abc</id>", ValueWithExistsFlag.Exists("abc")),
	        };
            RunAssertion(SupportEventInfra.XML_TYPENAME, SupportEventInfra.FXML, SupportEventInfra.XML_TO_VALUE, xmlTests, typeof(XmlNode));

            // Avro
            var datumEmpty = new GenericRecord(SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME));
            var datumOne = new GenericRecord(GetAvroSchema());
	        datumOne.Put("id", 101);
            var datumTwo = new GenericRecord(GetAvroSchema());
	        datumTwo.Put("id", null);
	        var avroTests = new Pair<GenericRecord, ValueWithExistsFlag>[] {
	            new Pair<GenericRecord, ValueWithExistsFlag>(datumEmpty, ValueWithExistsFlag.NotExists()),
	            new Pair<GenericRecord, ValueWithExistsFlag>(datumOne, ValueWithExistsFlag.Exists(101)),
	            new Pair<GenericRecord, ValueWithExistsFlag>(datumTwo, ValueWithExistsFlag.Exists(null))
	        };
            RunAssertion(SupportEventInfra.AVRO_TYPENAME, SupportEventInfra.FAVRO, null, avroTests, typeof(object));
	    }

        private void RunAssertion<T>(
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            IEnumerable<Pair<T, ValueWithExistsFlag>> tests,
            Type expectedPropertyType)
        {
	        var stmtText = "select id? as myid, exists(id?) as exists_myid from " + typename;
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        Assert.AreEqual(expectedPropertyType, stmt.EventType.GetPropertyType("myid"));
	        Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("exists_myid"));

	        foreach (var pair in tests) {
	            send.Invoke(_epService, pair.First);
	            var @event = listener.AssertOneGetNewAndReset();
	            SupportEventInfra.AssertValueMayConvert(@event, "myid", (ValueWithExistsFlag) pair.Second, optionalValueConversion);
	        }

	        stmt.Dispose();
	    }

	    private void AddMapEventType() {
	        _epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.MAP_TYPENAME, Collections.EmptyDataMap);
	    }

	    private void AddOAEventType() {
	        string[] names = {"somefield", "id"};
	        object[] types = {typeof(object), typeof(object)};
	        _epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.OA_TYPENAME, names, types);
	    }

	    private void AddXMLEventType(Configuration configuration) {
	        var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
	        eventTypeMeta.RootElementName = "myevent";
	        var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
	                        "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
	                        "\t<xs:element name=\"myevent\">\n" +
	                        "\t\t<xs:complexType>\n" +
	                        "\t\t</xs:complexType>\n" +
	                        "\t</xs:element>\n" +
	                        "</xs:schema>\n";
	        eventTypeMeta.SchemaText = schema;
	        configuration.AddEventType(SupportEventInfra.XML_TYPENAME, eventTypeMeta);
	    }

	    private void AddAvroEventType()
	    {
	        _epService.EPAdministrator.Configuration.AddEventTypeAvro(
	            SupportEventInfra.AVRO_TYPENAME,
	            new ConfigurationEventTypeAvro(SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME)));
	    }

	    private static RecordSchema GetAvroSchema() {
            return SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME,
                TypeBuilder.Field((string) "id", TypeBuilder.Union(
                    TypeBuilder.Null(), TypeBuilder.Int(), TypeBuilder.Boolean())));

            //return SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME).Fields()
	           //    .Name("id").Type().Union()
	           //    .NullType().And().IntType().And().BooleanType().EndUnion().NoDefault()
	           //    .EndRecord();
	    }
	}
} // end of namespace
