///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.@event.infra;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.util.CollectionUtil;

using static NEsper.Avro.Core.AvroConstant;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;
using SupportBeanSimple = com.espertech.esper.regressionlib.support.bean.SupportBeanSimple;
using SupportMarkerInterface = com.espertech.esper.regressionlib.support.bean.SupportMarkerInterface;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventInfra
    {
        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                typeof(SupportBean), typeof(SupportMarkerInterface), typeof(SupportBeanSimple),
                typeof(SupportBeanComplexProps), typeof(SupportBeanDynRoot), typeof(SupportBeanCombinedProps),
                typeof(EventInfraPropertyNestedSimple.InfraNestedSimplePropTop),
                typeof(EventInfraPropertyNestedIndexed.InfraNestedIndexPropTop),
                typeof(EventInfraEventRenderer.MyEvent)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            ConfigureRenderTypes(configuration);
            ConfigureSenderTypes(configuration);
            ConfigureDynamicNonSimpleTypes(configuration);
            ConfigureDynamicSimpleTypes(configuration);
            ConfigureMappedIndexed(configuration);
            ConfigureNestedDynamic(configuration);
            ConfigureNestedDynamicDeep(configuration);
            ConfiguredNestedDynamicRootedSimple(configuration);
            ConfigureNestedDynamicRootedNonSimple(configuration);
            ConfigureNestedIndexed(configuration);
            ConfigureNestedSimple(configuration);
            ConfigureUnderlyingSimple(configuration);
            ConfigureSuperType(configuration);
        }

        private static void ConfigureSuperType(Configuration configuration)
        {
            configuration.Common.AddEventType("Map_Type_Root", Collections.EmptyDataMap);
            configuration.Common.AddEventType("Map_Type_1", Collections.EmptyDataMap, new[] {"Map_Type_Root"});
            configuration.Common.AddEventType("Map_Type_2", Collections.EmptyDataMap, new[] {"Map_Type_Root"});
            configuration.Common.AddEventType("Map_Type_2_1", Collections.EmptyDataMap, new[] {"Map_Type_2"});

            configuration.Common.AddEventType("OA_Type_Root", new string[0], new object[0]);

            var array_1 = new ConfigurationCommonEventTypeObjectArray();
            array_1.SuperTypes = Collections.SingletonSet("OA_Type_Root");
            configuration.Common.AddEventType("OA_Type_1", new string[0], new object[0], array_1);

            var array_2 = new ConfigurationCommonEventTypeObjectArray();
            array_2.SuperTypes = Collections.SingletonSet("OA_Type_Root");
            configuration.Common.AddEventType("OA_Type_2", new string[0], new object[0], array_2);

            var array_2_1 = new ConfigurationCommonEventTypeObjectArray();
            array_2_1.SuperTypes = Collections.SingletonSet("OA_Type_2");
            configuration.Common.AddEventType("OA_Type_2_1", new string[0], new object[0], array_2_1);

            var fake = SchemaBuilder.Record("fake");
            var avro_root = new ConfigurationCommonEventTypeAvro();
            avro_root.AvroSchema = fake;
            configuration.Common.AddEventTypeAvro("Avro_Type_Root", avro_root);
            var avro_1 = new ConfigurationCommonEventTypeAvro();
            avro_1.SuperTypes = Collections.SingletonSet("Avro_Type_Root");
            avro_1.AvroSchema = fake;
            configuration.Common.AddEventTypeAvro("Avro_Type_1", avro_1);
            var avro_2 = new ConfigurationCommonEventTypeAvro();
            avro_2.SuperTypes = Collections.SingletonSet("Avro_Type_Root");
            avro_2.AvroSchema = fake;
            configuration.Common.AddEventTypeAvro("Avro_Type_2", avro_2);
            var avro_2_1 = new ConfigurationCommonEventTypeAvro();
            avro_2_1.SuperTypes = Collections.SingletonSet("Avro_Type_2");
            avro_2_1.AvroSchema = fake;
            configuration.Common.AddEventTypeAvro("Avro_Type_2_1", avro_2_1);

            foreach (var clazz in Arrays.AsList(
                typeof(EventInfraSuperType.Bean_Type_Root),
                typeof(EventInfraSuperType.Bean_Type_1),
                typeof(EventInfraSuperType.Bean_Type_2),
                typeof(EventInfraSuperType.Bean_Type_2_1))) {
                configuration.Common.AddEventType(clazz);
            }
        }

        private static void ConfigureUnderlyingSimple(Configuration configuration)
        {
            var properties = new Dictionary<string, object>();
            properties.Put("myInt", typeof(int));
            properties.Put("myString", "string");
            configuration.Common.AddEventType(EventInfraPropertyUnderlyingSimple.MAP_TYPENAME, properties);

            string[] names = {"myInt", "myString"};
            object[] types = {typeof(int), typeof(string)};
            configuration.Common.AddEventType(EventInfraPropertyUnderlyingSimple.OA_TYPENAME, names, types);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:attribute name=\"myInt\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t\t<xs:attribute name=\"myString\" type=\"xs:string\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraPropertyUnderlyingSimple.XML_TYPENAME, eventTypeMeta);

            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyUnderlyingSimple.AVRO_TYPENAME,
                TypeBuilder.Field("myInt", TypeBuilder.IntType()),
                TypeBuilder.Field(
                    "myString",
                    TypeBuilder.StringType(
                        TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE))));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyUnderlyingSimple.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        private static void ConfigureNestedSimple(Configuration configuration)
        {
            var mapTypeName = EventInfraPropertyNestedSimple.MAP_TYPENAME;
            configuration.Common.AddEventType(mapTypeName + "_4", Collections.SingletonDataMap("lvl4", typeof(int)));
            configuration.Common.AddEventType(
                mapTypeName + "_3",
                TwoEntryMap<string, object>("l4", mapTypeName + "_4", "lvl3", typeof(int)));
            configuration.Common.AddEventType(
                mapTypeName + "_2",
                TwoEntryMap<string, object>("l3", mapTypeName + "_3", "lvl2", typeof(int)));
            configuration.Common.AddEventType(
                mapTypeName + "_1",
                TwoEntryMap<string, object>("l2", mapTypeName + "_2", "lvl1", typeof(int)));
            configuration.Common.AddEventType(mapTypeName, Collections.SingletonDataMap("l1", mapTypeName + "_1"));

            var oaTypeName = EventInfraPropertyNestedSimple.OA_TYPENAME;
            var type_4 = oaTypeName + "_4";
            string[] names_4 = {"lvl4"};
            object[] types_4 = {typeof(int)};
            configuration.Common.AddEventType(type_4, names_4, types_4);
            var type_3 = oaTypeName + "_3";
            string[] names_3 = {"l4", "lvl3"};
            object[] types_3 = {type_4, typeof(int)};
            configuration.Common.AddEventType(type_3, names_3, types_3);
            var type_2 = oaTypeName + "_2";
            string[] names_2 = {"l3", "lvl2"};
            object[] types_2 = {type_3, typeof(int)};
            configuration.Common.AddEventType(type_2, names_2, types_2);
            var type_1 = oaTypeName + "_1";
            string[] names_1 = {"l2", "lvl1"};
            object[] types_1 = {type_2, typeof(int)};
            configuration.Common.AddEventType(type_1, names_1, types_1);
            string[] names = {"l1"};
            object[] types = {type_1};
            configuration.Common.AddEventType(oaTypeName, names, types);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:l1\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"l1\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:l2\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"lvl1\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"l2\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:l3\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"lvl2\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"l3\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:l4\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"lvl3\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"l4\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:attribute name=\"lvl4\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraPropertyNestedSimple.XML_TYPENAME, eventTypeMeta);

            var avroTypeName = EventInfraPropertyNestedSimple.AVRO_TYPENAME;
            var s4 = SchemaBuilder.Record(
                avroTypeName + "_4",
                TypeBuilder.RequiredInt("lvl4"));
            var s3 = SchemaBuilder.Record(
                avroTypeName + "_3",
                TypeBuilder.Field("l4", s4),
                TypeBuilder.RequiredInt("lvl3"));
            var s2 = SchemaBuilder.Record(
                avroTypeName + "_2",
                TypeBuilder.Field("l3", s3),
                TypeBuilder.RequiredInt("lvl2"));
            var s1 = SchemaBuilder.Record(
                avroTypeName + "_1",
                TypeBuilder.Field("l2", s2),
                TypeBuilder.RequiredInt("lvl1"));
            var avroSchema = SchemaBuilder.Record(
                avroTypeName,
                TypeBuilder.Field("l1", s1));
            configuration.Common.AddEventTypeAvro(avroTypeName, new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        private static void ConfigureNestedIndexed(Configuration configuration)
        {
            configuration.Common.AddEventType(typeof(EventInfraPropertyNestedIndexed.InfraNestedIndexPropTop));

            var mapTypeName = EventInfraPropertyNestedIndexed.MAP_TYPENAME;
            configuration.Common.AddEventType(mapTypeName + "_4", Collections.SingletonDataMap("lvl4", typeof(int)));
            configuration.Common.AddEventType(
                mapTypeName + "_3",
                TwoEntryMap<string, object>("l4", mapTypeName + "_4[]", "lvl3", typeof(int)));
            configuration.Common.AddEventType(
                mapTypeName + "_2",
                TwoEntryMap<string, object>("l3", mapTypeName + "_3[]", "lvl2", typeof(int)));
            configuration.Common.AddEventType(
                mapTypeName + "_1",
                TwoEntryMap<string, object>("l2", mapTypeName + "_2[]", "lvl1", typeof(int)));
            configuration.Common.AddEventType(mapTypeName, Collections.SingletonDataMap("l1", mapTypeName + "_1[]"));

            var oaTypeName = EventInfraPropertyNestedIndexed.OA_TYPENAME;
            var type_4 = oaTypeName + "_4";
            string[] names_4 = {"lvl4"};
            object[] types_4 = {typeof(int)};
            configuration.Common.AddEventType(type_4, names_4, types_4);
            var type_3 = oaTypeName + "_3";
            string[] names_3 = {"l4", "lvl3"};
            object[] types_3 = {type_4 + "[]", typeof(int)};
            configuration.Common.AddEventType(type_3, names_3, types_3);
            var type_2 = oaTypeName + "_2";
            string[] names_2 = {"l3", "lvl2"};
            object[] types_2 = {type_3 + "[]", typeof(int)};
            configuration.Common.AddEventType(type_2, names_2, types_2);
            var type_1 = oaTypeName + "_1";
            string[] names_1 = {"l2", "lvl1"};
            object[] types_1 = {type_2 + "[]", typeof(int)};
            configuration.Common.AddEventType(type_1, names_1, types_1);
            string[] names = {"l1"};
            object[] types = {type_1 + "[]"};
            configuration.Common.AddEventType(oaTypeName, names, types);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:l1\" maxOccurs=\"unbounded\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"l1\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:l2\" maxOccurs=\"unbounded\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"lvl1\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"l2\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:l3\" maxOccurs=\"unbounded\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"lvl2\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"l3\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:l4\" maxOccurs=\"unbounded\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"lvl3\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"l4\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:attribute name=\"lvl4\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraPropertyNestedIndexed.XML_TYPENAME, eventTypeMeta);

            var s4 = SchemaBuilder.Record(
                EventInfraPropertyNestedIndexed.AVRO_TYPENAME + "_4",
                TypeBuilder.RequiredInt("lvl4"));
            var s3 = SchemaBuilder.Record(
                EventInfraPropertyNestedIndexed.AVRO_TYPENAME + "_3",
                TypeBuilder.Field("l4", TypeBuilder.Array(s4)),
                TypeBuilder.RequiredInt("lvl3"));
            var s2 = SchemaBuilder.Record(
                EventInfraPropertyNestedIndexed.AVRO_TYPENAME + "_2",
                TypeBuilder.Field("l3", TypeBuilder.Array(s3)),
                TypeBuilder.RequiredInt("lvl2"));
            var s1 = SchemaBuilder.Record(
                EventInfraPropertyNestedIndexed.AVRO_TYPENAME + "_1",
                TypeBuilder.Field("l2", TypeBuilder.Array(s2)),
                TypeBuilder.RequiredInt("lvl1"));
            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyNestedIndexed.AVRO_TYPENAME,
                TypeBuilder.Field("l1", TypeBuilder.Array(s1)));

            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyNestedIndexed.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        private static void ConfiguredNestedDynamicRootedSimple(Configuration configuration)
        {
            configuration.Common.AddEventType(
                EventInfraPropertyNestedDynamicRootedSimple.MAP_TYPENAME,
                Collections.EmptyDataMap);

            var type_2 = EventInfraPropertyNestedDynamicRootedSimple.OA_TYPENAME + "_2";
            string[] names_2 = {"nestedNestedValue"};
            object[] types_2 = {typeof(object)};
            configuration.Common.AddEventType(type_2, names_2, types_2);
            var type_1 = EventInfraPropertyNestedDynamicRootedSimple.OA_TYPENAME + "_1";
            string[] names_1 = {"NestedValue", "nestedNested"};
            object[] types_1 = {typeof(object), type_2};
            configuration.Common.AddEventType(type_1, names_1, types_1);
            string[] names = {"simpleProperty", "nested"};
            object[] types = {typeof(object), type_1};
            configuration.Common.AddEventType(EventInfraPropertyNestedDynamicRootedSimple.OA_TYPENAME, names, types);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraPropertyNestedDynamicRootedSimple.XML_TYPENAME, eventTypeMeta);

            var s3 = SchemaBuilder.Record(
                EventInfraPropertyNestedDynamicRootedSimple.AVRO_TYPENAME + "_3",
                TypeBuilder.OptionalInt("nestedNestedValue"));
            var s2 = SchemaBuilder.Record(
                EventInfraPropertyNestedDynamicRootedSimple.AVRO_TYPENAME + "_2",
                TypeBuilder.OptionalInt("NestedValue"),
                TypeBuilder.Field(
                    "nestedNested",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        s3)));
            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyNestedDynamicRootedSimple.AVRO_TYPENAME + "_1",
                TypeBuilder.Field(
                    "simpleProperty",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        TypeBuilder.StringType())),
                TypeBuilder.Field(
                    "nested",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        s2)));

            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyNestedDynamicRootedSimple.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        private static void ConfigureNestedDynamicRootedNonSimple(Configuration configuration)
        {
            configuration.Common.AddEventType(
                EventInfraPropertyNestedDynamicRootedNonSimple.MAP_TYPENAME,
                Collections.EmptyDataMap);

            var nestedName = EventInfraPropertyNestedDynamicRootedNonSimple.OA_TYPENAME + "_1";
            string[] namesNested = {"indexed", "mapped", "arrayProperty", "mapProperty"};
            object[] typesNested = {
                typeof(int[]), typeof(IDictionary<string, object>), typeof(int[]), typeof(IDictionary<string, object>)
            };
            configuration.Common.AddEventType(nestedName, namesNested, typesNested);
            string[] names = {"someprop", "item"};
            object[] types = {typeof(string), nestedName};
            configuration.Common.AddEventType(EventInfraPropertyNestedDynamicRootedNonSimple.OA_TYPENAME, names, types);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(
                EventInfraPropertyNestedDynamicRootedNonSimple.XML_TYPENAME,
                eventTypeMeta);

            var s1 = SchemaBuilder.Record(
                EventInfraPropertyNestedDynamicRootedNonSimple.AVRO_TYPENAME + "_1",
                TypeBuilder.Field(
                    "indexed",
                    TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.Array(
                            TypeBuilder.IntType()))),
                TypeBuilder.Field(
                    "mapped",
                    TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.Map(
                            TypeBuilder.IntType()))));
            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyNestedDynamicRootedNonSimple.AVRO_TYPENAME,
                TypeBuilder.Field(
                    "item",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        s1)));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyNestedDynamicRootedNonSimple.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        private static void ConfigureNestedDynamicDeep(Configuration configuration)
        {
            var top = Collections.SingletonDataMap("item", typeof(IDictionary<string, object>));
            configuration.Common.AddEventType(EventInfraPropertyNestedDynamicDeep.MAP_TYPENAME, top);

            var type_3 = EventInfraPropertyNestedDynamicDeep.OA_TYPENAME + "_3";
            string[] names_3 = {"nestedNestedValue"};
            object[] types_3 = {typeof(object)};
            configuration.Common.AddEventType(type_3, names_3, types_3);
            var type_2 = EventInfraPropertyNestedDynamicDeep.OA_TYPENAME + "_2";
            string[] names_2 = {"nestedNested", "NestedValue"};
            object[] types_2 = {type_3, typeof(object)};
            configuration.Common.AddEventType(type_2, names_2, types_2);
            var type_1 = EventInfraPropertyNestedDynamicDeep.OA_TYPENAME + "_1";
            string[] names_1 = {"nested"};
            object[] types_1 = {type_2};
            configuration.Common.AddEventType(type_1, names_1, types_1);
            string[] names = {"item"};
            object[] types = {type_1};
            configuration.Common.AddEventType(EventInfraPropertyNestedDynamicDeep.OA_TYPENAME, names, types);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
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
            configuration.Common.AddEventType(EventInfraPropertyNestedDynamicDeep.XML_TYPENAME, eventTypeMeta);

            var s3 = SchemaBuilder.Record(
                EventInfraPropertyNestedDynamicDeep.AVRO_TYPENAME + "_3",
                TypeBuilder.OptionalInt("nestedNestedValue"));
            var s2 = SchemaBuilder.Record(
                EventInfraPropertyNestedDynamicDeep.AVRO_TYPENAME + "_2",
                TypeBuilder.OptionalInt("NestedValue"),
                TypeBuilder.Field(
                    "nestedNested",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        s3)));
            var s1 = SchemaBuilder.Record(
                EventInfraPropertyNestedDynamicDeep.AVRO_TYPENAME + "_1",
                TypeBuilder.Field(
                    "nested",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        s2)));
            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyNestedDynamicDeep.AVRO_TYPENAME,
                TypeBuilder.Field("item", s1));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyNestedDynamicDeep.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        private static void ConfigureNestedDynamic(Configuration configuration)
        {
            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
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
            configuration.Common.AddEventType(EventInfraPropertyNestedDynamic.XML_TYPENAME, eventTypeMeta);

            var top = Collections.SingletonDataMap("item", typeof(IDictionary<string, object>));
            configuration.Common.AddEventType(EventInfraPropertyNestedDynamic.MAP_TYPENAME, top);

            string[] names = {"item"};
            object[] types = {typeof(object)};
            configuration.Common.AddEventType(EventInfraPropertyNestedDynamic.OA_TYPENAME, names, types);

            var s1 = SchemaBuilder.Record(
                EventInfraPropertyNestedDynamic.AVRO_TYPENAME + "_1",
                TypeBuilder.Field(
                    "id",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        TypeBuilder.StringType(
                            TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE)),
                        TypeBuilder.NullType())));
            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyNestedDynamic.AVRO_TYPENAME,
                TypeBuilder.Field("item", s1));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyNestedDynamic.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        private static void ConfigureMappedIndexed(Configuration configuration)
        {
            configuration.Common.AddEventType(typeof(EventInfraPropertyMappedIndexed.MyIMEvent));

            configuration.Common.AddEventType(
                EventInfraPropertyMappedIndexed.MAP_TYPENAME,
                TwoEntryMap<string, object>(
                    "indexed",
                    typeof(string[]),
                    "mapped",
                    typeof(IDictionary<string, object>)));

            string[] names = {"indexed", "mapped"};
            object[] types = {typeof(string[]), typeof(IDictionary<string, object>)};
            configuration.Common.AddEventType(EventInfraPropertyMappedIndexed.OA_TYPENAME, names, types);

            var avroSchema = SchemaBuilder.Record(
                "AvroSchema",
                TypeBuilder.Field(
                    "indexed",
                    TypeBuilder.Array(
                        TypeBuilder.StringType(
                            TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE)))),
                TypeBuilder.Field(
                    "mapped",
                    TypeBuilder.Map(
                        TypeBuilder.StringType(TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE)))));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyMappedIndexed.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        private static void ConfigureDynamicSimpleTypes(Configuration configuration)
        {
            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraPropertyDynamicSimple.XML_TYPENAME, eventTypeMeta);

            configuration.Common.AddEventType(EventInfraPropertyDynamicSimple.MAP_TYPENAME, Collections.EmptyDataMap);
            string[] names = {"somefield", "id"};
            object[] types = {typeof(object), typeof(object)};
            configuration.Common.AddEventType(EventInfraPropertyDynamicSimple.OA_TYPENAME, names, types);

            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyDynamicSimple.AVRO_TYPENAME,
                TypeBuilder.Field(
                    "id",
                    TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.BooleanType())));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyDynamicSimple.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        private static void ConfigureDynamicNonSimpleTypes(Configuration configuration)
        {
            configuration.Common.AddEventType(
                EventInfraPropertyDynamicNonSimple.MAP_TYPENAME,
                Collections.EmptyDataMap);

            string[] names = {"indexed", "mapped"};
            object[] types = {typeof(int[]), typeof(IDictionary<string, object>)};
            configuration.Common.AddEventType(EventInfraPropertyDynamicNonSimple.OA_TYPENAME, names, types);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraPropertyDynamicNonSimple.XML_TYPENAME, eventTypeMeta);

            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyDynamicNonSimple.AVRO_TYPENAME,
                TypeBuilder.Field(
                    "indexed",
                    TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.Array(
                            TypeBuilder.IntType()))),
                TypeBuilder.Field(
                    "mapped",
                    TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.Map(
                            TypeBuilder.IntType()))));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyDynamicNonSimple.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        private static void ConfigureSenderTypes(Configuration configuration)
        {
            var eventInfraEventSenderMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventInfraEventSenderMeta.RootElementName = "myevent";
            var eventInfraEventSenderSchema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                              "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                                              "\t<xs:element name=\"myevent\">\n" +
                                              "\t\t<xs:complexType>\n" +
                                              "\t\t</xs:complexType>\n" +
                                              "\t</xs:element>\n" +
                                              "</xs:schema>\n";
            eventInfraEventSenderMeta.SchemaText = eventInfraEventSenderSchema;
            configuration.Common.AddEventType(EventInfraEventSender.XML_TYPENAME, eventInfraEventSenderMeta);

            configuration.Common.AddEventType(EventInfraEventSender.MAP_TYPENAME, Collections.EmptyDataMap);

            string[] names = { };
            object[] types = { };
            configuration.Common.AddEventType(EventInfraEventSender.OA_TYPENAME, names, types);
            configuration.Common.AddEventTypeAvro(
                EventInfraEventSender.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(
                    SchemaBuilder.Record(EventInfraEventSender.AVRO_TYPENAME)));
        }

        private static void ConfigureRenderTypes(Configuration configuration)
        {
            var myXMLEventConfig = new ConfigurationCommonEventTypeXMLDOM();
            myXMLEventConfig.RootElementName = "myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence minOccurs=\"0\" maxOccurs=\"unbounded\">\n" +
                         "\t\t\t\t<xs:choice>\n" +
                         "\t\t\t\t\t<xs:element ref=\"esper:nested\" minOccurs=\"1\" maxOccurs=\"1\"/>\n" +
                         "\t\t\t\t</xs:choice>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"myInt\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t\t<xs:attribute name=\"myString\" type=\"xs:string\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"nested\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:attribute name=\"myInsideInt\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            myXMLEventConfig.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraEventRenderer.XML_TYPENAME, myXMLEventConfig);

            IDictionary<string, object> inner = new Dictionary<string, object>();
            inner.Put("myInsideInt", "int");
            IDictionary<string, object> top = new Dictionary<string, object>();
            top.Put("myInt", "int");
            top.Put("myString", "string");
            top.Put("nested", inner);
            configuration.Common.AddEventType(EventInfraEventRenderer.MAP_TYPENAME, top);

            string[] namesInner = {"myInsideInt"};
            object[] typesInner = {typeof(int)};
            configuration.Common.AddEventType(EventInfraEventRenderer.OA_TYPENAME + "_1", namesInner, typesInner);

            string[] names = {"myInt", "myString", "nested"};
            object[] types = {typeof(int), typeof(string), EventInfraEventRenderer.OA_TYPENAME + "_1"};
            configuration.Common.AddEventType(EventInfraEventRenderer.OA_TYPENAME, names, types);

            var eventInfraEventRenderSchemaInner = SchemaBuilder.Record(
                EventInfraEventRenderer.AVRO_TYPENAME + "_inside",
                TypeBuilder.Field("myInsideInt", TypeBuilder.IntType()));
            var eventInfraEventRenderSchema = SchemaBuilder.Record(
                EventInfraEventRenderer.AVRO_TYPENAME,
                TypeBuilder.Field("myInt", TypeBuilder.IntType()),
                TypeBuilder.Field(
                    "myString",
                    TypeBuilder.StringType(
                        TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE))),
                TypeBuilder.Field("nested", eventInfraEventRenderSchemaInner));
            configuration.Common.AddEventTypeAvro(
                EventInfraEventRenderer.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(eventInfraEventRenderSchema));
        }

        [Test]
        public void TestEventInfraEventRenderer()
        {
            RegressionRunner.Run(session, new EventInfraEventRenderer());
        }

        [Test]
        public void TestEventInfraEventSender()
        {
            RegressionRunner.Run(session, new EventInfraEventSender());
        }

        [Test]
        public void TestEventInfraPropertyAccessPerformance()
        {
            RegressionRunner.Run(session, new EventInfraPropertyAccessPerformance());
        }

        [Test]
        public void TestEventInfraPropertyDynamicNonSimple()
        {
            RegressionRunner.Run(session, new EventInfraPropertyDynamicNonSimple());
        }

        [Test]
        public void TestEventInfraPropertyDynamicSimple()
        {
            RegressionRunner.Run(session, new EventInfraPropertyDynamicSimple());
        }

        [Test]
        public void TestEventInfraPropertyIndexedKeyExpr()
        {
            RegressionRunner.Run(session, new EventInfraPropertyIndexedKeyExpr());
        }

        [Test]
        public void TestEventInfraPropertyMappedIndexed()
        {
            RegressionRunner.Run(session, new EventInfraPropertyMappedIndexed());
        }

        [Test]
        public void TestEventInfraPropertyNestedDynamic()
        {
            RegressionRunner.Run(session, new EventInfraPropertyNestedDynamic());
        }

        [Test]
        public void TestEventInfraPropertyNestedDynamicDeep()
        {
            RegressionRunner.Run(session, new EventInfraPropertyNestedDynamicDeep());
        }

        [Test]
        public void TestEventInfraPropertyNestedDynamicRootedNonSimple()
        {
            RegressionRunner.Run(session, new EventInfraPropertyNestedDynamicRootedNonSimple());
        }

        [Test]
        public void TestEventInfraPropertyNestedDynamicRootedSimple()
        {
            RegressionRunner.Run(session, new EventInfraPropertyNestedDynamicRootedSimple());
        }

        [Test]
        public void TestEventInfraPropertyNestedIndexed()
        {
            RegressionRunner.Run(session, new EventInfraPropertyNestedIndexed());
        }

        [Test]
        public void TestEventInfraPropertyNestedSimple()
        {
            RegressionRunner.Run(session, new EventInfraPropertyNestedSimple());
        }

        [Test]
        public void TestEventInfraPropertyUnderlyingSimple()
        {
            RegressionRunner.Run(session, new EventInfraPropertyUnderlyingSimple());
        }

        [Test]
        public void TestEventInfraSuperType()
        {
            RegressionRunner.Run(session, new EventInfraSuperType());
        }
    }
} // end of namespace