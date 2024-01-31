///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

using Newtonsoft.Json.Linq;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace NEsper.Avro.Core
{
	[TestFixture]
	public class TestAvroSchemaUtil
	{
		private static readonly EventTypeNameResolver EVENT_ADAPTER_SERVICE = new ProxyEventTypeNameResolver() {
			ProcGetTypeByName = (typeName) => null,
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

			ClassicAssert.AreEqual(Schema.Type.Bytes, Assemble(typeof(byte[]), null, defaults, EVENT_ADAPTER_SERVICE).Tag);
			
			var bytesUnion = Assemble(typeof(byte[]), null, disableRequired, EVENT_ADAPTER_SERVICE).AsUnionSchema();
			ClassicAssert.AreEqual(2, bytesUnion.Count);
			ClassicAssert.AreEqual(Schema.Type.Null, bytesUnion.Schemas[0].Tag);
			ClassicAssert.AreEqual(Schema.Type.Bytes, bytesUnion.Schemas[1].Tag);
			
			foreach (var mapClass in new Type[] {
				typeof(LinkedHashMap<string, object>),
				typeof(IDictionary<string, object>)
			}) {
				var schemaReq = Assemble(mapClass, null, defaults, EVENT_ADAPTER_SERVICE);
				ClassicAssert.AreEqual(Schema.Type.Map, schemaReq.Tag);
				ClassicAssert.AreEqual(Schema.Type.String, schemaReq.AsMapSchema().ValueSchema.Tag);
				
				Console.Out.WriteLine(schemaReq);

				var schemaOpt = Assemble(mapClass, null, disableRequired, EVENT_ADAPTER_SERVICE);
				ClassicAssert.AreEqual(Schema.Type.Union, schemaOpt.Tag);
				var unionOpt = schemaOpt.AsUnionSchema();
				ClassicAssert.AreEqual(2, unionOpt.Schemas.Count);
				ClassicAssert.AreEqual(Schema.Type.Null, unionOpt.Schemas[0].Tag);
				ClassicAssert.AreEqual(Schema.Type.Map, unionOpt.Schemas[1].Tag);

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
				ClassicAssert.AreEqual(Schema.Type.Array, schema.Tag);
				elementSchema = arraySchema.ItemSchema;
			}
			else {
				var unionSchema = schema.AsUnionSchema();
				ClassicAssert.AreEqual(2, unionSchema.Count);
				ClassicAssert.AreEqual(Schema.Type.Null, unionSchema.Schemas[0].Tag);
				ClassicAssert.AreEqual(Schema.Type.Array, unionSchema.Schemas[1].Tag);
				elementSchema = unionSchema.Schemas[1].AsArraySchema().ItemSchema;
			}

			// assert element type
			if (!unionOfNullElements) {
				ClassicAssert.AreEqual(expectedElementType, elementSchema.Tag);
				AssertStringNative(elementSchema, avroSettings);
			}
			else {
				var unionSchema = elementSchema.AsUnionSchema();
				ClassicAssert.AreEqual(2, unionSchema.Count);
				ClassicAssert.AreEqual(Schema.Type.Null, unionSchema.Schemas[0].Tag);
				ClassicAssert.AreEqual(expectedElementType, unionSchema.Schemas[1].Tag);
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
				ClassicAssert.AreEqual(expected, schema.Tag);
				AssertStringNative(schema, avroSettings);
			}
			else {
				UnionSchema unionSchema = schema.AsUnionSchema();
				ClassicAssert.AreEqual(Schema.Type.Union, schema.Tag);
				ClassicAssert.AreEqual(2, unionSchema.Schemas.Count);
				ClassicAssert.AreEqual(Schema.Type.Null, unionSchema.Schemas[0].Tag);
				ClassicAssert.AreEqual(expected, unionSchema.Schemas[1].Tag);
			}
		}

		private Schema Assemble(
			object value,
			Attribute[] annotations,
			ConfigurationCommonEventTypeMeta.AvroSettingsConfig avroSettings,
			EventTypeNameResolver eventTypeNameResolver)
		{
			var fields = new JArray();
			AvroSchemaUtil.AssembleField("somefield", value, fields, annotations, avroSettings, eventTypeNameResolver, "stmtname", null);
			var schema = SchemaBuilder.Record("myrecord", fields);
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
				ClassicAssert.AreEqual(prop, AvroConstant.PROP_STRING_VALUE);
			}
			else {
				ClassicAssert.IsNull(prop);
			}
		}
	}
} // end of namespace
