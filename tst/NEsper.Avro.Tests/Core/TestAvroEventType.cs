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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

using static NEsper.Avro.Core.AvroConstant;
using static NEsper.Avro.Extensions.TypeBuilder;

namespace NEsper.Avro.Core
{
    public class TestAvroEventType
    {
        [Test]
        public void TestGetPropertyType()
        {
            var lvl2Schema = SchemaBuilder.Record(
                "lvl2Schema",
                Field("NestedValue", StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE))),
                Field("NestedIndexed", Array(IntType())),
                Field("NestedMapped", Map(StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE)))));

            var lvl1Schema = SchemaBuilder.Record(
                "lvl1Schema",
                Field("lvl2", lvl2Schema),
                RequiredInt("IntPrimitive"),
                Field("indexed", Array(IntType())),
                Field("mapped", Map(StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE)))));

            var schema = SchemaBuilder.Record(
                "typename",
                RequiredInt("myInt"),
                OptionalInt("myIntBoxed"),
                Field("myString", StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE))),
                Field("lvl1", lvl1Schema),
                Field("myNullValue", NullType()));

            EventType eventType = SupportAvroUtil.MakeAvroSupportEventType(schema);

            AssertPropertyType(typeof(int), null, eventType, "myInt");
            AssertPropertyType(typeof(int?), null, eventType, "myIntBoxed");
            AssertPropertyType(typeof(string), typeof(char), eventType, "myString");
            AssertPropertyType(null, null, eventType, "myNullValue");
            AssertPropertyType(typeof(GenericRecord), null, eventType, "lvl1");
            AssertPropertyType(typeof(int), null, eventType, "lvl1.IntPrimitive");
            AssertPropertyType(typeof(string), typeof(char), eventType, "lvl1.lvl2.NestedValue");
            AssertPropertyType(typeof(int), null, eventType, "lvl1.indexed[1]");
            AssertPropertyType(typeof(string), typeof(char), eventType, "lvl1.mapped('a')");
            AssertPropertyType(typeof(string), typeof(char), eventType, "lvl1.lvl2.NestedMapped('a')");
            AssertPropertyType(typeof(int), null, eventType, "lvl1.lvl2.NestedIndexed[1]");

            AssertNotAProperty(eventType, "dummy");
            AssertNotAProperty(eventType, "lvl1.dfgdg");
            AssertNotAProperty(eventType, "xxx.IntPrimitive");
            AssertNotAProperty(eventType, "lvl1.lvl2.NestedValueXXX");
            AssertNotAProperty(eventType, "myInt[1]");
            AssertNotAProperty(eventType, "lvl1.IntPrimitive[1]");
            AssertNotAProperty(eventType, "myInt('a')");
            AssertNotAProperty(eventType, "lvl1.IntPrimitive('a')");
            AssertNotAProperty(eventType, "lvl1.lvl2.NestedIndexed('a')");
            AssertNotAProperty(eventType, "lvl1.lvl2.NestedMapped[1]");

            var lvl2Rec = new GenericRecord(lvl2Schema);
            lvl2Rec.Put("NestedValue", 100);
            lvl2Rec.Put("NestedIndexed", Collections.List(19, 21));
            lvl2Rec.Put("NestedMapped", Collections.SingletonDataMap("Nestedkey", "Nestedvalue"));
            var lvl1Rec = new GenericRecord(lvl1Schema);
            lvl1Rec.Put("lvl2", lvl2Rec);
            lvl1Rec.Put("IntPrimitive", 10);
            lvl1Rec.Put("indexed", Collections.List(1, 2, 3));
            lvl1Rec.Put("mapped", Collections.SingletonDataMap("key", "value"));
            var record = new GenericRecord(schema);
            record.Put("lvl1", lvl1Rec);
            record.Put("myInt", 99);
            record.Put("myIntBoxed", 554);
            record.Put("myString", "hugo");
            record.Put("myNullValue", null);

            var eventBean = new AvroGenericDataEventBean(record, eventType);
            Assert.AreEqual(99, eventBean.Get("myInt"));
            Assert.AreEqual(554, eventBean.Get("myIntBoxed"));
            Assert.AreEqual("hugo", eventBean.Get("myString"));
            Assert.AreEqual(lvl1Rec, eventBean.Get("lvl1"));
            Assert.AreEqual(10, eventBean.Get("lvl1.IntPrimitive"));
            Assert.AreEqual(100, eventBean.Get("lvl1.lvl2.NestedValue"));
            Assert.AreEqual(2, eventBean.Get("lvl1.indexed[1]"));
            Assert.AreEqual("value", eventBean.Get("lvl1.mapped('key')"));
            Assert.AreEqual(null, eventBean.Get("myNullValue"));
            Assert.AreEqual("Nestedvalue", eventBean.Get("lvl1.lvl2.NestedMapped('nestedkey')"));
            Assert.AreEqual(21, eventBean.Get("lvl1.lvl2.NestedIndexed[1]"));
        }

        [Test]
        public void TestRequiredType()
        {
            var schema = SchemaBuilder.Record(
                "typename",
                RequiredInt("myInt"),
                RequiredString("myCharSeq"),
                Field("myString", StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE))),
                RequiredBoolean("myBoolean"),
                RequiredBytes("myBytes"),
                RequiredDouble("myDouble"),
                RequiredFloat("myFloat"),
                RequiredLong("myLong"));

            var propNames = "myInt,myCharSeq,myString,myBoolean,myBytes,myDouble,myFloat,myLong".SplitCsv();
            EventType eventType = SupportAvroUtil.MakeAvroSupportEventType(schema);
            EPAssertionUtil.AssertEqualsExactOrder(eventType.PropertyNames, propNames);
            Assert.AreEqual(typeof(GenericRecord), eventType.UnderlyingType);
            Assert.IsNull(eventType.SuperTypes);

            AssertPropertyType(typeof(int), null, eventType, "myInt");
            AssertPropertyType(typeof(string), typeof(char), eventType, "myString");
            AssertPropertyType(typeof(bool), null, eventType, "myBoolean");
            AssertPropertyType(typeof(byte[]), null, eventType, "myBytes");
            AssertPropertyType(typeof(double), null, eventType, "myDouble");
            AssertPropertyType(typeof(float), null, eventType, "myFloat");
            AssertPropertyType(typeof(long), null, eventType, "myLong");

            foreach (var propName in propNames) {
                Assert.IsTrue(eventType.IsProperty(propName));
            }

            var datum = GetRecordWithValues(schema);
            AssertValuesRequired(new AvroGenericDataEventBean(datum, eventType));

            var jsonWValues = "{'myInt': 10, 'myCharSeq': 'x', 'myString': 'y', 'myBoolean': true, 'myBytes': '\\u00AA\'," +
                              "'myDouble' : 50, 'myFloat':100, 'myLong':20}";
            AssertValuesRequired(new AvroGenericDataEventBean(SupportAvroUtil.ParseQuoted(schema, jsonWValues), eventType));
        }

        [Test]
        public void TestOptionalType()
        {
            var schema = SchemaBuilder.Record(
                "typename",
                OptionalInt("myInt"),
                OptionalString("myCharSeq"),
                Field("myString", Union(NullType(), StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE)))),
                OptionalBoolean("myBoolean"),
                OptionalBytes("myBytes"),
                OptionalDouble("myDouble"),
                OptionalFloat("myFloat"),
                OptionalLong("myLong"));

            RunAssertionNullableOrOptTypes(schema);
        }

        [Test]
        public void TestNullableType()
        {
            var schema = SchemaBuilder.Record(
                "typename",
                OptionalInt("myInt"),
                OptionalString("myCharSeq"),
                Field("myString", Union(NullType(), StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE)))),
                OptionalBoolean("myBoolean"),
                OptionalBytes("myBytes"),
                OptionalDouble("myDouble"),
                OptionalFloat("myFloat"),
                OptionalLong("myLong"));

            RunAssertionNullableOrOptTypes(schema);
        }

        [Test]
        public void TestNestedSimple()
        {
            var schemaText = "{" +
                             "  'type' : 'record'," +
                             "  'name' : 'MyEvent'," +
                             "  'fields' : [ {" +
                             "    'name' : 'innerEvent'," +
                             "    'type' : {" +
                             "      'type' : 'record'," +
                             "      'name' : 'innerEventTypeName'," +
                             "      'fields' : [ {" +
                             "        'name' : 'innerValue'," +
                             "        'type' : {'type':'string','avro.string':'String'}" +
                             "      } ]" +
                             "    }" +
                             "  }]" +
                             "}";

            var schema = Schema.Parse(schemaText.Replace("'", "\"")).AsRecordSchema();
            EventType eventType = SupportAvroUtil.MakeAvroSupportEventType(schema);

            AssertPropertyType(typeof(GenericRecord), null, eventType, "innerEvent");

            var propNames = "innerEvent".SplitCsv();
            EPAssertionUtil.AssertEqualsExactOrder(eventType.PropertyNames, propNames);
            Assert.IsTrue(eventType.IsProperty("innerEvent"));

            var datumInner = new GenericRecord(schema.GetField("innerEvent").Schema.AsRecordSchema());
            datumInner.Put("innerValue", "i1");
            var datum = new GenericRecord(schema);
            datum.Put("innerEvent", datumInner);

            AssertValuesNested(datum, new AvroGenericDataEventBean(datum, eventType));

            var jsonWValues = "{'innerEvent': {'innerValue' : 'i1'}}";
            datum = SupportAvroUtil.ParseQuoted(schema, jsonWValues);
            AssertValuesNested(datum, new AvroGenericDataEventBean(datum, eventType));
        }

        [Test]
        public void TestArrayOfPrimitive()
        {
            var schema = SchemaBuilder.Record(
                "typename",
                Field("intArray", Array(IntType())));
            var eventType = SupportAvroUtil.MakeAvroSupportEventType(schema);

            AssertPropertyType(typeof(int[]), typeof(int), eventType, "intArray");

            Consumer<EventBean> asserter = eventBean => {
                    Assert.AreEqual(1, eventBean.Get("intArray[0]"));
                    Assert.AreEqual(2, eventBean.Get("intArray[1]"));
                    Assert.AreEqual(1, eventType.GetGetter("intArray[0]").Get(eventBean));
                    Assert.AreEqual(2, eventType.GetGetter("intArray[1]").Get(eventBean));
                };

            var datum = new GenericRecord(schema);
            datum.Put("intArray", Collections.List(1, 2));
            asserter.Invoke(new AvroGenericDataEventBean(datum, eventType));

            var jsonWValues = "{'intArray':[1,2]}";
            datum = SupportAvroUtil.ParseQuoted(schema, jsonWValues);
            asserter.Invoke(new AvroGenericDataEventBean(datum, eventType));
        }

        [Test]
        public void TestMapOfString()
        {
            var schema = SchemaBuilder.Record(
                "typename",
                Field(
                    "anMap",
                    Map(
                        StringType(
                            Property(PROP_STRING_KEY, PROP_STRING_VALUE)),
                        Property(PROP_STRING_KEY, PROP_STRING_VALUE))));
                    
            var eventType = SupportAvroUtil.MakeAvroSupportEventType(schema);

            AssertPropertyType(typeof(IDictionary<string, string>), typeof(string), eventType, "anMap");

            Consumer<EventBean> asserter = eventBean => {
                Assert.AreEqual("myValue", eventBean.Get("anMap('myKey')"));
                Assert.AreEqual("myValue", eventType.GetGetter("anMap('myKey')").Get(eventBean));
            };

            var datum = new GenericRecord(schema);
            datum.Put("anMap", Collections.SingletonDataMap("myKey", "myValue"));
            asserter.Invoke(new AvroGenericDataEventBean(datum, eventType));

            var jsonWValues = "{'anMap':{'myKey':'myValue'}}";
            datum = SupportAvroUtil.ParseQuoted(schema, jsonWValues);
            asserter.Invoke(new AvroGenericDataEventBean(datum, eventType));
        }

#if FALSE        
        [Test]
        public void TestFixed()
        {
            var schema = SchemaBuilder.Record(
                "typename",
                Field("aFixed", TypeBuilder.Fixed(
                    "abc", // name
                    2, // size
                    new ByteBuffer(new byte[0]) // default
                    )));

            EventType eventType = SupportAvroUtil.MakeAvroSupportEventType(schema);

            AssertPropertyType(typeof(GenericFixed), null, eventType, "aFixed");

            Consumer<EventBean> asserter = eventBean => {
                    var @fixed = (GenericFixed) eventBean.Get("aFixed");
                    Assert.IsTrue(Arrays.AreEqual(@fixed.Bytes, new byte[] {1, 2}));
                };

            var datum = new GenericRecord(schema);
            datum.Put("aFixed", new GenericFixed(
                schema.GetField("aFixed").Schema.AsFixedSchema(), new byte[] {1, 2}));
            asserter.Invoke(new AvroGenericDataEventBean(datum, eventType));

            var jsonWValues = "{'aFixed': '\\u0001\\u0002\'}";
            datum = SupportAvroUtil.ParseQuoted(schema, jsonWValues);
            asserter.Invoke(new AvroGenericDataEventBean(datum, eventType));
        }

        [Test]
        public void TestEnumSymbol()
        {
            var schema = SchemaBuilder.Record(
                "typename", ...);

            var schema = SchemaBuilder.Record("typename")
                .fields()
                .name("aEnum")
                .type()
                .enumeration("myEnum")
                .symbols("a", "b")
                .enumDefault("x")
                .endRecord();
            EventType eventType = SupportAvroUtil.MakeAvroSupportEventType(schema);

            AssertPropertyType(typeof(GenericEnum), null, eventType, "aEnum");

            Consumer<EventBean> asserter = eventBean => {
                GenericEnum v = (GenericEnum) eventBean.Get("aEnum");
                Assert.AreEqual("b", v.ToString());
            };

            var datum = new GenericRecord(schema);
            datum.Put("aEnum", new GenericEnum(schema.GetField("aEnum").Schema, "b"));
            asserter.Invoke(new AvroGenericDataEventBean(datum, eventType));

            var jsonWValues = "{'aEnum': 'b'}";
            datum = SupportAvroUtil.ParseQuoted(schema, jsonWValues);
            asserter.Invoke(new AvroGenericDataEventBean(datum, eventType));
        }
#endif

        [Test]
        public void TestUnionResultingInObject()
        {
            var schema = SchemaBuilder.Record(
                "typename",
                Field(
                    "anUnion",
                    Union(
                        IntType(),
                        StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE)),
                        NullType())));

            EventType eventType = SupportAvroUtil.MakeAvroSupportEventType(schema);

            AssertPropertyType(typeof(object), null, eventType, "anUnion");

            Consumer<Object> asserterFromDatum = (value) => {
                var datum = new GenericRecord(schema);
                datum.Put("anUnion", value);
                Assert.AreEqual(value, new AvroGenericDataEventBean(datum, eventType).Get("anUnion"));
            };
            
            asserterFromDatum.Invoke("a");
            asserterFromDatum.Invoke(1);
            asserterFromDatum.Invoke(null);

            BiConsumer<String, Object> asserterFromJson = (
                    json,
                    value) => {
                    var datum = SupportAvroUtil.ParseQuoted(schema, json);
                    Assert.AreEqual(value, new AvroGenericDataEventBean(datum, eventType).Get("anUnion"));
                }
                ;
            asserterFromJson.Invoke("{'anUnion':{'int':1}}", 1);
            asserterFromJson.Invoke("{'anUnion':{'string':'abc'}}", "abc");
            asserterFromJson.Invoke("{'anUnion':null}", null);
        }

        [Test]
        public void TestUnionResultingInNumber()
        {
            var schema = SchemaBuilder.Record(
                "typename",
                Field("anUnion", Union(IntType(), FloatType())));

            EventType eventType = SupportAvroUtil.MakeAvroSupportEventType(schema);

            AssertPropertyType(typeof(object), null, eventType, "anUnion");

            Consumer<Object> asserterFromDatum = (value) => {
                var datum = new GenericRecord(schema);
                datum.Put("anUnion", value);
                Assert.AreEqual(value, new AvroGenericDataEventBean(datum, eventType).Get("anUnion"));
            };

            asserterFromDatum.Invoke(1);
            asserterFromDatum.Invoke(2f);

            BiConsumer<String, Object> asserterFromJson = (
                json,
                value) => {
                var datum = SupportAvroUtil.ParseQuoted(schema, json);
                Assert.AreEqual(value, new AvroGenericDataEventBean(datum, eventType).Get("anUnion"));
            };

            asserterFromJson.Invoke("{'anUnion':{'int':1}}", 1);
            asserterFromJson.Invoke("{'anUnion':{'float':2}}", 2f);
        }

        private void AssertValuesNested(
            GenericRecord datum,
            AvroGenericDataEventBean bean)
        {
            Assert.AreEqual("i1", bean.Get("innerEvent.innerValue"));
            Assert.AreEqual("i1", bean.EventType.GetGetter("innerEvent.innerValue").Get(bean));

            Assert.AreSame(datum.Get("innerEvent"), bean.Get("innerEvent"));
            Assert.AreSame(datum.Get("innerEvent"), bean.EventType.GetGetter("innerEvent").Get(bean));
        }

        private void RunAssertionNullableOrOptTypes(RecordSchema schema)
        {
            EventType eventType = SupportAvroUtil.MakeAvroSupportEventType(schema);

            AssertTypesBoxed(eventType);

            var datum = GetRecordWithValues(schema);
            AssertValuesRequired(new AvroGenericDataEventBean(datum, eventType));

            var jsonWValues =
                "{" +
                "'myInt': {'int': 10}, " +
                "'myCharSeq': {'string': 'x'}, " +
                "'myString': {'string': 'y'}," +
                "'myBoolean': {'boolean': true}, " +
                "'myBytes': {'bytes': '\\u00AA\'}, " +
                "'myDouble': {'double': 50}, " +
                "'myFloat': {'float': 100}, " +
                "'myLong': {'long': 20}" +
                "}";
            datum = SupportAvroUtil.ParseQuoted(schema, jsonWValues);
            AssertValuesRequired(new AvroGenericDataEventBean(datum, eventType));

            var jsonWNull = "{'myInt': null, 'myCharSeq': null, 'myString':null," +
                            "'myBoolean': null, 'myBytes': null, " +
                            "'myDouble': null, 'myFloat': null, 'myLong': null}";
            AssertValuesNull(new AvroGenericDataEventBean(SupportAvroUtil.ParseQuoted(schema, jsonWNull), eventType));
        }

        private void AssertValuesRequired(AvroGenericDataEventBean bean)
        {
            AssertValue(10, bean, "myInt");
            AssertValue("y", bean, "myString");
            AssertValue(true, bean, "myBoolean");
            AssertValue(new[] {(byte) 170}, bean, "myBytes");
            AssertValue(50d, bean, "myDouble");
            AssertValue(100f, bean, "myFloat");
            AssertValue(20L, bean, "myLong");
        }

        private void AssertValuesNull(AvroGenericDataEventBean bean)
        {
            AssertValue(null, bean, "myInt");
            AssertValue(null, bean, "myCharSeq");
            AssertValue(null, bean, "myString");
            AssertValue(null, bean, "myBoolean");
            AssertValue(null, bean, "myBytes");
            AssertValue(null, bean, "myDouble");
            AssertValue(null, bean, "myFloat");
            AssertValue(null, bean, "myLong");
        }

        private void AssertValue(
            Object expected,
            AvroGenericDataEventBean bean,
            String propertyName)
        {
            if (expected is byte[]) {
                AreEqualsBytes((byte[]) expected, (byte[]) bean.Get(propertyName));
                var getter = bean.EventType.GetGetter(propertyName);
                AreEqualsBytes((byte[]) expected, (byte[]) getter.Get(bean));
            }
            else {
                Assert.AreEqual(expected, bean.Get(propertyName));
                var getter = bean.EventType.GetGetter(propertyName);
                Assert.AreEqual(expected, getter.Get(bean));
            }
        }

        private void AssertPropertyType(
            Type expectedType,
            Type expectedComponentType,
            EventType eventType,
            String propertyName)
        {
            Assert.AreEqual(expectedType, eventType.GetPropertyType(propertyName));
            Assert.IsTrue(eventType.IsProperty(propertyName));

            if (!propertyName.Contains(".")) {
                var descriptor = eventType.GetPropertyDescriptor(propertyName);
                Assert.AreEqual(expectedType, descriptor.PropertyType);
                Assert.AreEqual(expectedComponentType, descriptor.PropertyComponentType);
            }
        }

        private void AreEqualsBytes(
            byte[] expected,
            byte[] received)
        {
            Arrays.AreEqual(expected, received);
        }

        private GenericRecord GetRecordWithValues(RecordSchema schema)
        {
            var datum = new GenericRecord(schema);
            datum.Put("myInt", 10);
            datum.Put("myString", "y");
            datum.Put("myBoolean", true);
            datum.Put("myBytes", new[] {(byte) 170});
            datum.Put("myDouble", 50d);
            datum.Put("myFloat", 100f);
            datum.Put("myLong", 20L);
            return datum;
        }

        private void AssertTypesBoxed(EventType eventType)
        {
            AssertPropertyType(typeof(int?), null, eventType, "myInt");
            AssertPropertyType(typeof(string), typeof(char), eventType, "myString");
            AssertPropertyType(typeof(bool?), null, eventType, "myBoolean");
            AssertPropertyType(typeof(byte[]), null, eventType, "myBytes");
            AssertPropertyType(typeof(double?), null, eventType, "myDouble");
            AssertPropertyType(typeof(float?), null, eventType, "myFloat");
            AssertPropertyType(typeof(long?), null, eventType, "myLong");
        }

        private void AssertNotAProperty(
            EventType type,
            String propertyName)
        {
            Assert.IsFalse(type.IsProperty(propertyName));
            Assert.IsNull(type.GetPropertyType(propertyName));
            Assert.IsNull(type.GetGetter(propertyName));
            Assert.IsNull(type.GetPropertyDescriptor(propertyName));
        }
    }
}