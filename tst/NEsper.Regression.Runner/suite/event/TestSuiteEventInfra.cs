///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.@event.infra;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using JetBrains.dotMemoryUnit;

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
    public class TestSuiteEventInfra : AbstractTestBase
    {
        public TestSuiteEventInfra() : base(Configure)
        {
        }

        public static void Configure(Configuration configuration)
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

            configuration.Common.EventMeta.AvroSettings.IsEnableAvro = true;

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
            ConfigureManufacturerTypes(configuration);
            ConfigureGetterTypes(configuration);
        }

        public static void ConfigureGetterTypes(Configuration configuration)
        {
            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = EventInfraGetterSimpleNoFragment.XMLTYPENAME;
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"" + EventInfraGetterSimpleNoFragment.XMLTYPENAME + "\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:attribute name=\"Property\" type=\"xs:string\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraGetterSimpleNoFragment.XMLTYPENAME, eventTypeMeta);
        }

        public static void ConfigureSuperType(Configuration configuration)
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

        public static void ConfigureUnderlyingSimple(Configuration configuration)
        {
            var properties = new Dictionary<string, object>();
            properties.Put("MyInt", typeof(int));
            properties.Put("MyString", "string");
            configuration.Common.AddEventType(EventInfraPropertyUnderlyingSimple.MAP_TYPENAME, properties);

            string[] names = {"MyInt", "MyString"};
            object[] types = {typeof(int), typeof(string)};
            configuration.Common.AddEventType(EventInfraPropertyUnderlyingSimple.OA_TYPENAME, names, types);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "Myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"Myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:attribute name=\"MyInt\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t\t<xs:attribute name=\"MyString\" type=\"xs:string\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraPropertyUnderlyingSimple.XML_TYPENAME, eventTypeMeta);

            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyUnderlyingSimple.AVRO_TYPENAME,
                TypeBuilder.Field("MyInt", TypeBuilder.IntType()),
                TypeBuilder.Field(
                    "MyString",
                    TypeBuilder.StringType(
                        TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE))));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyUnderlyingSimple.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        public static void ConfigureNestedSimple(Configuration configuration)
        {
            var mapTypeName = EventInfraPropertyNestedSimple.MAP_TYPENAME;
            configuration.Common.AddEventType(mapTypeName + "_4", Collections.SingletonDataMap("Lvl4", typeof(int)));
            configuration.Common.AddEventType(
                mapTypeName + "_3",
                TwoEntryMap<string, object>("L4", mapTypeName + "_4", "Lvl3", typeof(int)));
            configuration.Common.AddEventType(
                mapTypeName + "_2",
                TwoEntryMap<string, object>("L3", mapTypeName + "_3", "Lvl2", typeof(int)));
            configuration.Common.AddEventType(
                mapTypeName + "_1",
                TwoEntryMap<string, object>("L2", mapTypeName + "_2", "Lvl1", typeof(int)));
            configuration.Common.AddEventType(mapTypeName, Collections.SingletonDataMap("L1", mapTypeName + "_1"));

            var oaTypeName = EventInfraPropertyNestedSimple.OA_TYPENAME;
            var type_4 = oaTypeName + "_4";
            string[] names_4 = {"Lvl4"};
            object[] types_4 = {typeof(int)};
            configuration.Common.AddEventType(type_4, names_4, types_4);
            var type_3 = oaTypeName + "_3";
            string[] names_3 = {"L4", "Lvl3"};
            object[] types_3 = {type_4, typeof(int)};
            configuration.Common.AddEventType(type_3, names_3, types_3);
            var type_2 = oaTypeName + "_2";
            string[] names_2 = {"L3", "Lvl2"};
            object[] types_2 = {type_3, typeof(int)};
            configuration.Common.AddEventType(type_2, names_2, types_2);
            var type_1 = oaTypeName + "_1";
            string[] names_1 = {"L2", "Lvl1"};
            object[] types_1 = {type_2, typeof(int)};
            configuration.Common.AddEventType(type_1, names_1, types_1);
            string[] names = {"L1"};
            object[] types = {type_1};
            configuration.Common.AddEventType(oaTypeName, names, types);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "Myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"Myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:L1\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"L1\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:L2\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"Lvl1\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"L2\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:L3\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"Lvl2\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"L3\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:L4\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"Lvl3\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"L4\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:attribute name=\"Lvl4\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraPropertyNestedSimple.XML_TYPENAME, eventTypeMeta);

            var avroTypeName = EventInfraPropertyNestedSimple.AVRO_TYPENAME;
            var s4 = SchemaBuilder.Record(
                avroTypeName + "_4",
                TypeBuilder.RequiredInt("Lvl4"));
            var s3 = SchemaBuilder.Record(
                avroTypeName + "_3",
                TypeBuilder.Field("L4", s4),
                TypeBuilder.RequiredInt("Lvl3"));
            var s2 = SchemaBuilder.Record(
                avroTypeName + "_2",
                TypeBuilder.Field("L3", s3),
                TypeBuilder.RequiredInt("Lvl2"));
            var s1 = SchemaBuilder.Record(
                avroTypeName + "_1",
                TypeBuilder.Field("L2", s2),
                TypeBuilder.RequiredInt("Lvl1"));
            var avroSchema = SchemaBuilder.Record(
                avroTypeName,
                TypeBuilder.Field("L1", s1));
            configuration.Common.AddEventTypeAvro(avroTypeName, new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        public static void ConfigureNestedIndexed(Configuration configuration)
        {
            configuration.Common.AddEventType(typeof(EventInfraPropertyNestedIndexed.InfraNestedIndexPropTop));

            var mapTypeName = EventInfraPropertyNestedIndexed.MAP_TYPENAME;
            configuration.Common.AddEventType(
                mapTypeName + "_4",
                Collections.SingletonDataMap("lvl4", typeof(int)));
            configuration.Common.AddEventType(
                mapTypeName + "_3",
                TwoEntryMap<string, object>("l4", mapTypeName + "_4[]", "lvl3", typeof(int)));
            configuration.Common.AddEventType(
                mapTypeName + "_2",
                TwoEntryMap<string, object>("l3", mapTypeName + "_3[]", "lvl2", typeof(int)));
            configuration.Common.AddEventType(
                mapTypeName + "_1",
                TwoEntryMap<string, object>("l2", mapTypeName + "_2[]", "lvl1", typeof(int)));
            configuration.Common.AddEventType(
                mapTypeName,
                Collections.SingletonDataMap("l1", mapTypeName + "_1[]"));

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
            eventTypeMeta.RootElementName = "Myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"Myevent\">\n" +
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

        public static void ConfiguredNestedDynamicRootedSimple(Configuration configuration)
        {
            configuration.Common.AddEventType(
                EventInfraPropertyDynamicNestedRootedSimple.MAP_TYPENAME,
                Collections.EmptyDataMap);

            var type_2 = EventInfraPropertyDynamicNestedRootedSimple.OA_TYPENAME + "_2";
            string[] names_2 = {"NestedNestedValue"};
            object[] types_2 = {typeof(object)};
            configuration.Common.AddEventType(type_2, names_2, types_2);

            var type_1 = EventInfraPropertyDynamicNestedRootedSimple.OA_TYPENAME + "_1";
            string[] names_1 = {"NestedValue", "NestedNested"};
            object[] types_1 = {typeof(object), type_2};
            configuration.Common.AddEventType(type_1, names_1, types_1);

            string[] names = {"SimpleProperty", "Nested"};
            object[] types = {typeof(object), type_1};
            configuration.Common.AddEventType(EventInfraPropertyDynamicNestedRootedSimple.OA_TYPENAME, names, types);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "Myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"Myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraPropertyDynamicNestedRootedSimple.XML_TYPENAME, eventTypeMeta);

            var s3 = SchemaBuilder.Record(
                EventInfraPropertyDynamicNestedRootedSimple.AVRO_TYPENAME + "_3",
                TypeBuilder.OptionalInt("NestedNestedValue"));
            var s2 = SchemaBuilder.Record(
                EventInfraPropertyDynamicNestedRootedSimple.AVRO_TYPENAME + "_2",
                TypeBuilder.OptionalInt("NestedValue"),
                TypeBuilder.Field(
                    "NestedNested",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        s3)));
            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyDynamicNestedRootedSimple.AVRO_TYPENAME + "_1",
                TypeBuilder.Field(
                    "SimpleProperty",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        TypeBuilder.StringType())),
                TypeBuilder.Field(
                    "Nested",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        s2)));

            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyDynamicNestedRootedSimple.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        public static void ConfigureNestedDynamicRootedNonSimple(Configuration configuration)
        {
            configuration.Common.AddEventType(
                EventInfraPropertyDynamicNestedRootedNonSimple.MAP_TYPENAME,
                Collections.EmptyDataMap);

            var nestedName = EventInfraPropertyDynamicNestedRootedNonSimple.OA_TYPENAME + "_1";
            string[] namesNested = {"Indexed", "Mapped", "ArrayProperty", "MapProperty"};
            object[] typesNested = {
                typeof(int[]),
                typeof(IDictionary<string, object>),
                typeof(int[]),
                typeof(IDictionary<string, object>)
            };
            configuration.Common.AddEventType(nestedName, namesNested, typesNested);
            string[] names = {"someprop", "Item"};
            object[] types = {typeof(string), nestedName};
            configuration.Common.AddEventType(EventInfraPropertyDynamicNestedRootedNonSimple.OA_TYPENAME, names, types);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "Myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"Myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(
                EventInfraPropertyDynamicNestedRootedNonSimple.XML_TYPENAME,
                eventTypeMeta);

            var s1 = SchemaBuilder.Record(
                EventInfraPropertyDynamicNestedRootedNonSimple.AVRO_TYPENAME + "_1",
                TypeBuilder.Field(
                    "Indexed",
                    TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.Array(
                            TypeBuilder.IntType()))),
                TypeBuilder.Field(
                    "Mapped",
                    TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.Map(
                            TypeBuilder.IntType()))));
            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyDynamicNestedRootedNonSimple.AVRO_TYPENAME,
                TypeBuilder.Field(
                    "Item",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        s1)));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyDynamicNestedRootedNonSimple.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        public static void ConfigureNestedDynamicDeep(Configuration configuration)
        {
            var top = Collections.SingletonDataMap("Item", typeof(IDictionary<string, object>));
            configuration.Common.AddEventType(EventInfraPropertyDynamicNestedDeep.MAP_TYPENAME, top);

            var type_3 = EventInfraPropertyDynamicNestedDeep.OA_TYPENAME + "_3";
            string[] names_3 = {"NestedNestedValue"};
            object[] types_3 = {typeof(object)};
            configuration.Common.AddEventType(type_3, names_3, types_3);
            var type_2 = EventInfraPropertyDynamicNestedDeep.OA_TYPENAME + "_2";
            string[] names_2 = {"NestedNested", "NestedValue"};
            object[] types_2 = {type_3, typeof(object)};
            configuration.Common.AddEventType(type_2, names_2, types_2);
            var type_1 = EventInfraPropertyDynamicNestedDeep.OA_TYPENAME + "_1";
            string[] names_1 = {"Nested"};
            object[] types_1 = {type_2};
            configuration.Common.AddEventType(type_1, names_1, types_1);
            string[] names = {"Item"};
            object[] types = {type_1};
            configuration.Common.AddEventType(EventInfraPropertyDynamicNestedDeep.OA_TYPENAME, names, types);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "Myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"Myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:Item\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"Item\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraPropertyDynamicNestedDeep.XML_TYPENAME, eventTypeMeta);

            var s3 = SchemaBuilder.Record(
                EventInfraPropertyDynamicNestedDeep.AVRO_TYPENAME + "_3",
                TypeBuilder.OptionalInt("NestedNestedValue"));
            var s2 = SchemaBuilder.Record(
                EventInfraPropertyDynamicNestedDeep.AVRO_TYPENAME + "_2",
                TypeBuilder.OptionalInt("NestedValue"),
                TypeBuilder.Field(
                    "NestedNested",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        s3)));
            var s1 = SchemaBuilder.Record(
                EventInfraPropertyDynamicNestedDeep.AVRO_TYPENAME + "_1",
                TypeBuilder.Field(
                    "Nested",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        s2)));
            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyDynamicNestedDeep.AVRO_TYPENAME,
                TypeBuilder.Field("Item", s1));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyDynamicNestedDeep.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        public static void ConfigureNestedDynamic(Configuration configuration)
        {
            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "Myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"Myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:Item\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"Item\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraPropertyDynamicNested.XML_TYPENAME, eventTypeMeta);

            var top = Collections.SingletonDataMap("Item", typeof(IDictionary<string, object>));
            configuration.Common.AddEventType(EventInfraPropertyDynamicNested.MAP_TYPENAME, top);

            string[] names = {"Item"};
            object[] types = {typeof(object)};
            configuration.Common.AddEventType(EventInfraPropertyDynamicNested.OA_TYPENAME, names, types);

            var s1 = SchemaBuilder.Record(
                EventInfraPropertyDynamicNested.AVRO_TYPENAME + "_1",
                TypeBuilder.Field(
                    "Id",
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        TypeBuilder.StringType(
                            TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE)),
                        TypeBuilder.NullType())));
            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyDynamicNested.AVRO_TYPENAME,
                TypeBuilder.Field("Item", s1));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyDynamicNested.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        public static void ConfigureMappedIndexed(Configuration configuration)
        {
            configuration.Common.AddEventType(typeof(EventInfraPropertyMappedIndexed.MyIMEvent));

            configuration.Common.AddEventType(
                EventInfraPropertyMappedIndexed.MAP_TYPENAME,
                TwoEntryMap<string, object>(
                    "Indexed",
                    typeof(string[]),
                    "Mapped",
                    typeof(IDictionary<string, object>)));

            string[] names = {"Indexed", "Mapped"};
            object[] types = {typeof(string[]), typeof(IDictionary<string, object>)};
            configuration.Common.AddEventType(EventInfraPropertyMappedIndexed.OA_TYPENAME, names, types);

            var avroSchema = SchemaBuilder.Record(
                "AvroSchema",
                TypeBuilder.Field(
                    "Indexed",
                    TypeBuilder.Array(
                        TypeBuilder.StringType(
                            TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE)))),
                TypeBuilder.Field(
                    "Mapped",
                    TypeBuilder.Map(
                        TypeBuilder.StringType(TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE)))));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyMappedIndexed.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        public static void ConfigureDynamicSimpleTypes(Configuration configuration)
        {
            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "Myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"Myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraPropertyDynamicSimple.XML_TYPENAME, eventTypeMeta);

            configuration.Common.AddEventType(EventInfraPropertyDynamicSimple.MAP_TYPENAME, Collections.EmptyDataMap);
            string[] names = {"somefield", "Id"};
            object[] types = {typeof(object), typeof(object)};
            configuration.Common.AddEventType(EventInfraPropertyDynamicSimple.OA_TYPENAME, names, types);

            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyDynamicSimple.AVRO_TYPENAME,
                TypeBuilder.Field(
                    "Id",
                    TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.BooleanType())));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyDynamicSimple.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        public static void ConfigureDynamicNonSimpleTypes(Configuration configuration)
        {
            configuration.Common.AddEventType(
                EventInfraPropertyDynamicNonSimple.MAP_TYPENAME,
                Collections.EmptyDataMap);

            string[] names = {"Indexed", "Mapped"};
            object[] types = {typeof(int[]), typeof(IDictionary<string, object>)};
            configuration.Common.AddEventType(EventInfraPropertyDynamicNonSimple.OA_TYPENAME, names, types);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "Myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"Myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraPropertyDynamicNonSimple.XML_TYPENAME, eventTypeMeta);

            var avroSchema = SchemaBuilder.Record(
                EventInfraPropertyDynamicNonSimple.AVRO_TYPENAME,
                TypeBuilder.Field(
                    "Indexed",
                    TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.Array(
                            TypeBuilder.IntType()))),
                TypeBuilder.Field(
                    "Mapped",
                    TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.Map(
                            TypeBuilder.IntType()))));
            configuration.Common.AddEventTypeAvro(
                EventInfraPropertyDynamicNonSimple.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        public static void ConfigureSenderTypes(Configuration configuration)
        {
            var eventInfraEventSenderMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventInfraEventSenderMeta.RootElementName = "Myevent";
            var eventInfraEventSenderSchema =
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                "\t<xs:element name=\"Myevent\">\n" +
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

        public static void ConfigureRenderTypes(Configuration configuration)
        {
            var myXMLEventConfig = new ConfigurationCommonEventTypeXMLDOM();
            myXMLEventConfig.RootElementName = "Myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"Myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence minOccurs=\"0\" maxOccurs=\"unbounded\">\n" +
                         "\t\t\t\t<xs:choice>\n" +
                         "\t\t\t\t\t<xs:element ref=\"esper:Nested\" minOccurs=\"1\" maxOccurs=\"1\"/>\n" +
                         "\t\t\t\t</xs:choice>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t\t<xs:attribute name=\"MyInt\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t\t<xs:attribute name=\"MyString\" type=\"xs:string\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"Nested\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:attribute name=\"MyInsideInt\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            myXMLEventConfig.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraEventRenderer.XML_TYPENAME, myXMLEventConfig);

            IDictionary<string, object> inner = new Dictionary<string, object>();
            inner.Put("MyInsideInt", "int");
            IDictionary<string, object> top = new Dictionary<string, object>();
            top.Put("MyInt", "int");
            top.Put("MyString", "string");
            top.Put("Nested", inner);
            configuration.Common.AddEventType(EventInfraEventRenderer.MAP_TYPENAME, top);

            string[] namesInner = {"MyInsideInt"};
            object[] typesInner = {typeof(int)};
            configuration.Common.AddEventType(EventInfraEventRenderer.OA_TYPENAME + "_1", namesInner, typesInner);

            string[] names = {"MyInt", "MyString", "Nested"};
            object[] types = {typeof(int), typeof(string), EventInfraEventRenderer.OA_TYPENAME + "_1"};
            configuration.Common.AddEventType(EventInfraEventRenderer.OA_TYPENAME, names, types);

            var eventInfraEventRenderSchemaInner = SchemaBuilder.Record(
                EventInfraEventRenderer.AVRO_TYPENAME + "_inside",
                TypeBuilder.Field("MyInsideInt", TypeBuilder.IntType()));
            var eventInfraEventRenderSchema = SchemaBuilder.Record(
                EventInfraEventRenderer.AVRO_TYPENAME,
                TypeBuilder.Field("MyInt", TypeBuilder.IntType()),
                TypeBuilder.Field(
                    "MyString",
                    TypeBuilder.StringType(
                        TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE))),
                TypeBuilder.Field("Nested", eventInfraEventRenderSchemaInner));
            configuration.Common.AddEventTypeAvro(
                EventInfraEventRenderer.AVRO_TYPENAME,
                new ConfigurationCommonEventTypeAvro(eventInfraEventRenderSchema));
        }

        public static void ConfigureManufacturerTypes(Configuration configuration)
        {
            var myXMLEventConfig = new ConfigurationCommonEventTypeXMLDOM();
            myXMLEventConfig.RootElementName = "myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:attribute name=\"P0\" type=\"xs:string\" use=\"required\"/>\n" +
                         "\t\t\t<xs:attribute name=\"P1\" type=\"xs:int\" use=\"required\"/>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            myXMLEventConfig.SchemaText = schema;
            configuration.Common.AddEventType(EventInfraManufacturer.XML_TYPENAME, myXMLEventConfig);

            var avroSchema = SchemaBuilder.Record(
                EventInfraManufacturer.AVRO_TYPENAME,
                TypeBuilder.Field(
                    "P1",
                    TypeBuilder.StringType(
                        TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE))),
                TypeBuilder.Field(
                    "P2",
                    TypeBuilder.IntType()));

            configuration.Common.AddEventTypeAvro(EventInfraManufacturer.AVRO_TYPENAME, new ConfigurationCommonEventTypeAvro(avroSchema));
        }

        [Test]
        public void TestEventInfraPropertyUnderlyingSimple()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyUnderlyingSimple());
        }

        [Test]
        public void TestEventInfraPropertyMappedIndexed()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyMappedIndexed());
        }

        [Test]
        public void TestEventInfraPropertyDynamicSimple()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyDynamicSimple());
        }

        [Test]
        public void TestEventInfraPropertyNestedSimple()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyNestedSimple());
        }

        [Test]
        public void TestEventInfraPropertyDynamicNonSimple()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyDynamicNonSimple());
        }

        [Test]
        public void TestEventInfraPropertyNestedIndexed()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyNestedIndexed());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraPropertyDynamicNested()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyDynamicNested());
        }

        [Test]
        public void TestEventInfraPropertyDynamicNestedRootedSimple()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyDynamicNestedRootedSimple());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraPropertyDynamicNestedDeep()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyDynamicNestedDeep());
        }

        [Test]
        public void TestEventInfraPropertyDynamicNestedRootedNonSimple()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyDynamicNestedRootedNonSimple());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraEventRenderer()
        {
            RegressionRunner.Run(_session, new EventInfraEventRenderer());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraEventSender()
        {
            RegressionRunner.Run(_session, new EventInfraEventSender());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraSuperType()
        {
            RegressionRunner.Run(_session, new EventInfraSuperType());
        }

        [Test]
        public void TestEventInfraPropertyIndexedKeyExpr()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyIndexedKeyExpr());
        }

        [Test]
        public void TestEventInfraManufacturer()
        {
            RegressionRunner.Run(_session, new EventInfraManufacturer());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraPropertyAccessPerformance()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyAccessPerformance());
        }

        [Test]
        public void TestEventInfraGetterSimpleNoFragment()
        {
            RegressionRunner.Run(_session, new EventInfraGetterSimpleNoFragment());
        }

        [Test]
        public void TestEventInfraGetterSimpleFragment()
        {
            RegressionRunner.Run(_session, new EventInfraGetterSimpleFragment());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraGetterMapped()
        {
            RegressionRunner.Run(_session, new EventInfraGetterMapped());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraGetterIndexed()
        {
            RegressionRunner.Run(_session, new EventInfraGetterIndexed());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraGetterDynamicIndexed()
        {
            RegressionRunner.Run(_session, new EventInfraGetterDynamicIndexed());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraGetterDynamicIndexedPropertyPredefined()
        {
            RegressionRunner.Run(_session, new EventInfraGetterDynamicIndexedPropertyPredefined());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraGetterDynamicSimplePropertyPredefined()
        {
            RegressionRunner.Run(_session, new EventInfraGetterDynamicSimplePropertyPredefined());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraGetterDynamicMapped()
        {
            RegressionRunner.Run(_session, new EventInfraGetterDynamicMapped());
        }

        [Test]
        public void TestEventInfraGetterDynamicSimple()
        {
            RegressionRunner.Run(_session, new EventInfraGetterDynamicSimple());
        }

        [Test]
        public void TestEventInfraGetterDynamicNested()
        {
            RegressionRunner.Run(_session, new EventInfraGetterDynamicNested());
        }

        [Test]
        public void TestEventInfraGetterNestedSimpleNoFragment()
        {
            RegressionRunner.Run(_session, new EventInfraGetterNestedSimple());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraGetterNestedArray()
        {
            RegressionRunner.Run(_session, new EventInfraGetterNestedArray());
        }

        [Test]
        public void TestEventInfraGetterNestedSimpleDeep()
        {
            RegressionRunner.Run(_session, new EventInfraGetterNestedSimpleDeep());
        }

        [Test]
        public void TestEventInfraGetterDynamicNestedDeep()
        {
            RegressionRunner.Run(_session, new EventInfraGetterDynamicNestedDeep());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraContainedSimple()
        {
            RegressionRunner.Run(_session, new EventInfraContainedSimple());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraContainedIndexedWithIndex()
        {
            RegressionRunner.Run(_session, new EventInfraContainedIndexedWithIndex());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraContainedNested()
        {
            RegressionRunner.Run(_session, new EventInfraContainedNested());
        }

        [Test, RunInApplicationDomain]
        public void TestEventInfraContainedNestedArray()
        {
            RegressionRunner.Run(_session, new EventInfraContainedNestedArray());
            // memory leak checking
#if FALSE
            dotMemory.Check(
                memory => {
                    Assert.That(memory.GetObjects(where => where.Type.Is<ArtifactRepositoryAssemblyLoadContext>()).ObjectsCount, Is.Zero);
                });
#endif
        }

        [Test]
        public void TestEventInfraPropertyIndexedRuntimeIndex()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyIndexedRuntimeIndex());
        }

        [Test]
        public void TestEventInfraPropertyMappedRuntimeKey()
        {
            RegressionRunner.Run(_session, new EventInfraPropertyMappedRuntimeKey());
        }
    }
} // end of namespace