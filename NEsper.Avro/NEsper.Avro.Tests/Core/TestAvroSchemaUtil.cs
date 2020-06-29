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

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace NEsper.Avro.Core
{
	[TestFixture]
	public class TestAvroSchemaUtil
	{
		private readonly static EventTypeNameResolver EVENT_ADAPTER_SERVICE = new ProxyEventTypeNameResolver() {
			ProcGetTypeByName = (typeName) => { return null; },
		};

		[Test]
		public void TestAssemble()
		{
			var defaults = new ConfigurationCommonEventTypeMeta().AvroSettings;

			var disableNativeString = new ConfigurationCommonEventTypeMeta().AvroSettings;
			disableNativeString.IsEnableNativeString = false;

			var disableRequired = new ConfigurationCommonEventTypeMeta().AvroSettings;
			disableRequired.IsEnableSchemaDefaultNonNull = false;

			AssertType(typeof(bool), Schema.Type.Boolean, false, defaults);
			AssertType(typeof(bool?), Schema.Type.Boolean, false, defaults);
			AssertType(typeof(int), Schema.Type.Int, false, defaults);
			AssertType(typeof(int?), Schema.Type.Int, false, defaults);
			AssertType(typeof(byte), Schema.Type.Int, false, defaults);
			AssertType(typeof(byte?), Schema.Type.Int, false, defaults);
			AssertType(typeof(long), Schema.Type.Long, false, defaults);
			AssertType(typeof(long?), Schema.Type.Long, false, defaults);
			AssertType(typeof(float), Schema.Type.Float, false, defaults);
			AssertType(typeof(float?), Schema.Type.Float, false, defaults);
			AssertType(typeof(double), Schema.Type.Double, false, defaults);
			AssertType(typeof(double?), Schema.Type.Double, false, defaults);
			AssertType(typeof(string), Schema.Type.String, false, defaults);
			AssertType(typeof(string), Schema.Type.String, false, disableNativeString);

			AssertType(typeof(bool), Schema.Type.Boolean, false, disableRequired);
			AssertType(typeof(bool?), Schema.Type.Boolean, true, disableRequired);
			AssertType(typeof(int), Schema.Type.Int, false, disableRequired);
			AssertType(typeof(int?), Schema.Type.Int, true, disableRequired);
			AssertType(typeof(byte), Schema.Type.Int, false, disableRequired);
			AssertType(typeof(byte?), Schema.Type.Int, true, disableRequired);
			AssertType(typeof(long), Schema.Type.Long, false, disableRequired);
			AssertType(typeof(long?), Schema.Type.Long, true, disableRequired);
			AssertType(typeof(float), Schema.Type.Float, false, disableRequired);
			AssertType(typeof(float?), Schema.Type.Float, true, disableRequired);
			AssertType(typeof(double), Schema.Type.Double, false, disableRequired);
			AssertType(typeof(double?), Schema.Type.Double, true, disableRequired);
			AssertType(typeof(string), Schema.Type.String, true, disableRequired);

			// Array rules:
			// - Array-of-primitive: default non-null and non-null elements
			// - Array-of-boxed: default nullable and nullable elements
			// - Array-of-String: default non-nullable and non-null elements
			AssertTypeArray(typeof(bool), Schema.Type.Boolean, false, false, defaults);
			AssertTypeArray(typeof(bool?), Schema.Type.Boolean, false, true, defaults);
			AssertTypeArray(typeof(int), Schema.Type.Int, false, false, defaults);
			AssertTypeArray(typeof(int?), Schema.Type.Int, false, true, defaults);
			AssertTypeArray(typeof(byte?), Schema.Type.Int, false, true, defaults);
			AssertTypeArray(typeof(long), Schema.Type.Long, false, false, defaults);
			AssertTypeArray(typeof(long?), Schema.Type.Long, false, true, defaults);
			AssertTypeArray(typeof(float), Schema.Type.Float, false, false, defaults);
			AssertTypeArray(typeof(float?), Schema.Type.Float, false, true, defaults);
			AssertTypeArray(typeof(double), Schema.Type.Double, false, false, defaults);
			AssertTypeArray(typeof(double?), Schema.Type.Double, false, true, defaults);
			AssertTypeArray(typeof(string), Schema.Type.String, false, false, defaults);
			AssertTypeArray(typeof(string), Schema.Type.String, false, false, disableNativeString);

			AssertTypeArray(typeof(bool), Schema.Type.Boolean, true, false, disableRequired);
			AssertTypeArray(typeof(bool?), Schema.Type.Boolean, true, true, disableRequired);
			AssertTypeArray(typeof(int), Schema.Type.Int, true, false, disableRequired);
			AssertTypeArray(typeof(int?), Schema.Type.Int, true, true, disableRequired);
			AssertTypeArray(typeof(byte?), Schema.Type.Int, true, true, disableRequired);
			AssertTypeArray(typeof(long), Schema.Type.Long, true, false, disableRequired);
			AssertTypeArray(typeof(long?), Schema.Type.Long, true, true, disableRequired);
			AssertTypeArray(typeof(float), Schema.Type.Float, true, false, disableRequired);
			AssertTypeArray(typeof(float?), Schema.Type.Float, true, true, disableRequired);
			AssertTypeArray(typeof(double), Schema.Type.Double, true, false, disableRequired);
			AssertTypeArray(typeof(double?), Schema.Type.Double, true, true, disableRequired);
			AssertTypeArray(typeof(string), Schema.Type.String, true, false, disableRequired);

			Assert.AreEqual(Schema.Type.Bytes, Assemble(typeof(byte[]), null, defaults, EVENT_ADAPTER_SERVICE).Tag);
			
			var bytesUnion = Assemble(typeof(byte[]), null, disableRequired, EVENT_ADAPTER_SERVICE).AsUnionSchema();
			Assert.AreEqual(2, bytesUnion.Count);
			Assert.AreEqual(Schema.Type.Null, bytesUnion.Schemas[0].Tag);
			Assert.AreEqual(Schema.Type.Bytes, bytesUnion.Schemas[1].Tag);

			foreach (var mapClass in new Type[] {
				typeof(LinkedHashMap<string, object>),
				typeof(IDictionary<string, object>)
			}) {
				Schema schemaReq = Assemble(mapClass, null, defaults, EVENT_ADAPTER_SERVICE);
				Assert.AreEqual(Schema.Type.Map, schemaReq.Tag);
				
				Console.Out.WriteLine(schemaReq);

				var schemaOpt = Assemble(mapClass, null, disableRequired, EVENT_ADAPTER_SERVICE).AsRecordSchema();
				Assert.AreEqual(2, schemaOpt.Fields.Count);
				Assert.AreEqual(Schema.Type.Null, schemaOpt.Fields[0].Schema.Tag);
				Assert.AreEqual(Schema.Type.Map, schemaOpt.Fields[1].Schema.Tag);
				Console.Out.WriteLine(schemaOpt);
			}
		}

		private void AssertTypeArray(
			Type componentType,
			Schema.Type expectedElementType,
			bool unionOfNull,
			bool unionOfNullElements,
			ConfigurationCommonEventTypeMeta.AvroSettingsConfig avroSettings)
		{
			var schema = Assemble(
				TypeHelper.GetArrayType(componentType),
				null,
				avroSettings,
				EVENT_ADAPTER_SERVICE);

			Schema elementSchema;
			if (!unionOfNull) {
				var arraySchema = schema.AsArraySchema();
				Assert.AreEqual(Schema.Type.Array, schema.Tag);
				elementSchema = arraySchema.ItemSchema;
			}
			else {
				var unionSchema = schema.AsUnionSchema();
				Assert.AreEqual(2, unionSchema.Count);
				Assert.AreEqual(Schema.Type.Null, unionSchema.Schemas[0].Tag);
				Assert.AreEqual(Schema.Type.Array, unionSchema.Schemas[1].Tag);
				elementSchema = unionSchema.Schemas[1];
			}

			// assert element type
			if (!unionOfNullElements) {
				Assert.AreEqual(expectedElementType, elementSchema.Tag);
				AssertStringNative(elementSchema, avroSettings);
			}
			else {
				var unionSchema = elementSchema.AsUnionSchema();
				Assert.AreEqual(2, unionSchema.Count);
				Assert.AreEqual(Schema.Type.Null, unionSchema.Schemas[0].Tag);
				Assert.AreEqual(expectedElementType, unionSchema.Schemas[1].Tag);
			}
		}

		private void AssertType(
			Type clazz,
			Schema.Type expected,
			bool unionOfNull,
			ConfigurationCommonEventTypeMeta.AvroSettingsConfig avroSettings)
		{
			var schema = Assemble(clazz, null, avroSettings, EVENT_ADAPTER_SERVICE);
			if (!unionOfNull) {
				Assert.AreEqual(expected, schema.Tag);
				AssertStringNative(schema, avroSettings);
			}
			else {
				UnionSchema unionSchema = schema.AsUnionSchema();
				Assert.AreEqual(Schema.Type.Union, schema.Tag);
				Assert.AreEqual(2, unionSchema.Schemas.Count);
				Assert.AreEqual(Schema.Type.Null, unionSchema.Schemas[0].Tag);
				Assert.AreEqual(expected, unionSchema.Schemas[1].Tag);
			}
		}

		private Schema Assemble(
			object value,
			Attribute[] annotations,
			ConfigurationCommonEventTypeMeta.AvroSettingsConfig avroSettings,
			EventTypeNameResolver eventTypeNameResolver)
		{
			var assembler = SchemaBuilder.Record("myrecord");
			AvroSchemaUtil.AssembleField("somefield", value, assembler, annotations, avroSettings, eventTypeNameResolver, "stmtname", null);
			Schema schema = assembler.EndRecord();
			return schema.GetField("somefield").Schema;
		}

		private void AssertStringNative(
			Schema elementType,
			ConfigurationCommonEventTypeMeta.AvroSettingsConfig avroSettings)
		{
			if (elementType.Tag != Schema.Type.String) {
				return;
			}

			var prop = elementType.GetProp(AvroConstant.PROP_STRING_KEY);
			if (avroSettings.IsEnableNativeString) {
				Assert.AreEqual(prop, AvroConstant.PROP_STRING_VALUE);
			}
			else {
				Assert.IsNull(prop);
			}
		}
	}
} // end of namespace
