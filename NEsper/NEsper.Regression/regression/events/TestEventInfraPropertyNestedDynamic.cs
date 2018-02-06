///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
using com.espertech.esper.util;

using Newtonsoft.Json.Linq;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestEventInfraPropertyNestedDynamic
    {
	    private static readonly string BEAN_TYPENAME = typeof(SupportBeanDynRoot).FullName;

	    private static readonly FunctionSendEvent FAVRO = (epService, value) => {
	        var schema = GetAvroSchema();
	        var itemSchema = schema.GetField("item").Schema.AsRecordSchema();
	        var itemDatum = new GenericRecord(itemSchema);
	        itemDatum.Put("id", value);
	        var datum = new GenericRecord(schema);
	        datum.Put("item", itemDatum);
	        epService.EPRuntime.SendEventAvro(datum, SupportEventInfra.AVRO_TYPENAME);
	    };

	    private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        AddXMLEventType(configuration);

	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();

	        AddMapEventType();
	        AddOAEventType();
	        _epService.EPAdministrator.Configuration.AddEventType(BEAN_TYPENAME, typeof(SupportBeanDynRoot));
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
	    public void TestDynamicProp() {
	        RunAssertion(EventRepresentationChoice.ARRAY, "");
	        RunAssertion(EventRepresentationChoice.MAP, "");
	        RunAssertion(EventRepresentationChoice.AVRO, "@AvroSchemaField(Name='myid',Schema='[\"int\",{\"type\":\"string\",\"avro.string\":\"string\"},\"null\"]')");
	        RunAssertion(EventRepresentationChoice.DEFAULT, "");
	    }

	    private void RunAssertion(EventRepresentationChoice outputEventRep, string additionalAnnotations) {

	        // Bean
            var beanTests = new Pair<SupportBeanDynRoot, ValueWithExistsFlag>[] {
	            new Pair<SupportBeanDynRoot, ValueWithExistsFlag>(new SupportBeanDynRoot(new SupportBean_S0(101)), ValueWithExistsFlag.Exists(101)),
	            new Pair<SupportBeanDynRoot, ValueWithExistsFlag>(new SupportBeanDynRoot("abc"), ValueWithExistsFlag.NotExists()),
	            new Pair<SupportBeanDynRoot, ValueWithExistsFlag>(new SupportBeanDynRoot(new SupportBean_A("e1")), ValueWithExistsFlag.Exists("e1")),
	            new Pair<SupportBeanDynRoot, ValueWithExistsFlag>(new SupportBeanDynRoot(new SupportBean_B("e2")), ValueWithExistsFlag.Exists("e2")),
	            new Pair<SupportBeanDynRoot, ValueWithExistsFlag>(new SupportBeanDynRoot(new SupportBean_S1(102)), ValueWithExistsFlag.Exists(102))
	        };
	        RunAssertion(outputEventRep, additionalAnnotations, BEAN_TYPENAME, SupportEventInfra.FBEAN, null, beanTests, typeof(object));

	        // Map
	        var mapTests = new Pair<IDictionary<string, object>, ValueWithExistsFlag>[] {
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag>(Collections.EmptyDataMap, ValueWithExistsFlag.NotExists()),
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag>(Collections.SingletonDataMap("item", Collections.SingletonDataMap("id", 101)), ValueWithExistsFlag.Exists(101)),
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag>(Collections.SingletonDataMap("item", Collections.EmptyDataMap), ValueWithExistsFlag.NotExists()),
	        };
            RunAssertion(outputEventRep, additionalAnnotations, SupportEventInfra.MAP_TYPENAME, SupportEventInfra.FMAP, null, mapTests, typeof(object));

	        // Object array
	        var oaTests = new Pair<object[], ValueWithExistsFlag>[] {
	            new Pair<object[], ValueWithExistsFlag>(new object[] {null}, ValueWithExistsFlag.NotExists()),
	            //new Pair<>(new Object[] {new SupportBean_S0(101)}, exists(101)),
	            //new Pair<>(new Object[] {"abc"}, notExists()),
	        };
            RunAssertion(outputEventRep, additionalAnnotations, SupportEventInfra.OA_TYPENAME, SupportEventInfra.FOA, null, oaTests, typeof(object));

	        // XML
	        var xmlTests = new Pair<string, ValueWithExistsFlag>[] {
	            new Pair<string, ValueWithExistsFlag>("<item id=\"101\"/>", ValueWithExistsFlag.Exists("101")),
	            new Pair<string, ValueWithExistsFlag>("<item/>", ValueWithExistsFlag.NotExists()),
	        };
	        if (!outputEventRep.IsAvroEvent()) {
                RunAssertion(outputEventRep, additionalAnnotations, SupportEventInfra.XML_TYPENAME, SupportEventInfra.FXML, SupportEventInfra.XML_TO_VALUE, xmlTests, typeof(XmlNode));
	        }

	        // Avro
	        var avroTests = new Pair<object, ValueWithExistsFlag>[] {
	            new Pair<object, ValueWithExistsFlag>(null, ValueWithExistsFlag.Exists(null)),
	            new Pair<object, ValueWithExistsFlag>(101, ValueWithExistsFlag.Exists(101)),
	            new Pair<object, ValueWithExistsFlag>("abc", ValueWithExistsFlag.Exists("abc")),
	        };
            RunAssertion(outputEventRep, additionalAnnotations, SupportEventInfra.AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object));
	    }

        private void RunAssertion<T>(
            EventRepresentationChoice eventRepresentationEnum,
            string additionalAnnotations,
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            IEnumerable<Pair<T, ValueWithExistsFlag>> tests,
            Type expectedPropertyType)
        {
	        var stmtText = eventRepresentationEnum.GetAnnotationText() + additionalAnnotations + " select " +
	                          "item.id? as myid, " +
	                          "exists(item.id?) as exists_myid " +
	                          "from " + typename;
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        Assert.AreEqual(expectedPropertyType, stmt.EventType.GetPropertyType("myid"));
	        Assert.AreEqual(typeof(bool?), TypeHelper.GetBoxedType(stmt.EventType.GetPropertyType("exists_myid")));
	        Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

	        foreach (var pair in tests) {
	            send.Invoke(_epService, pair.First);
	            var @event = listener.AssertOneGetNewAndReset();
	            SupportEventInfra.AssertValueMayConvert(@event, "myid", (ValueWithExistsFlag) pair.Second, optionalValueConversion);
	        }

	        stmt.Dispose();
	    }

	    private void AddMapEventType() {
            var top = Collections.SingletonDataMap("item", typeof(IDictionary<string, object>));
	        _epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.MAP_TYPENAME, top);
	    }

	    private void AddOAEventType() {
	        string[] names = {"item"};
	        object[] types = {typeof(object)};
            _epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.OA_TYPENAME, names, types);
	    }

	    private void AddXMLEventType(Configuration configuration) {
	        var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
	        eventTypeMeta.RootElementName = "myevent";
	        var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
	                        "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
	                        "\t<xs:element name=\"myevent\">\n" +
	                        "\t\t<xs:complexType>\n" +
	                        "\t\t\t<xs:sequence>\n" +
	                        "\t\t\t\t<xs:element ref=\"esper:item\"/>\n" +
	                        "\t\t\t</xs:sequence>\n" +
	                        "\t\t</xs:complexType>\n" +
	                        "\t</xs:element>\n" +
	                        "\t<xs:element name=\"item\">\n" +
	                        "\t\t<xs:complexType>\n" +
	                        "\t\t</xs:complexType>\n" +
	                        "\t</xs:element>\n" +
	                        "</xs:schema>\n";
	        eventTypeMeta.SchemaText = schema;
            configuration.AddEventType(SupportEventInfra.XML_TYPENAME, eventTypeMeta);
	    }

	    private void AddAvroEventType() {
            _epService.EPAdministrator.Configuration.AddEventTypeAvro(SupportEventInfra.AVRO_TYPENAME, new ConfigurationEventTypeAvro(GetAvroSchema()));
	    }

	    private static RecordSchema GetAvroSchema()
	    {
	        var s1 = TypeBuilder.Record(
	            SupportEventInfra.AVRO_TYPENAME + "_1",
                TypeBuilder.Field((string) "id", TypeBuilder.Union(
                    TypeBuilder.Int(),
                    TypeBuilder.String(TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE)),
                    TypeBuilder.Null())));

            //Schema s1 = SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME + "_1").Fields()
            //         .Name("id").Type().Union()
            //         .IntBuilder().EndInt()
            //         .And()
            //         .StringBuilder().Prop(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE).EndString()
            //         .And()
            //         .NullType()
            //         .EndUnion().NoDefault()
            //         .EndRecord();

	        return SchemaBuilder.Record(
	            SupportEventInfra.AVRO_TYPENAME,
	            TypeBuilder.Field("item", s1));
                
            //SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME).Fields().Name("item").Type(s1).NoDefault().EndRecord();
	    }
	}
} // end of namespace
