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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.util.CollectionUtil;
using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraPropertyMappedIndexed : RegressionExecution
	{
		public static readonly Type BEAN_TYPE = typeof(MyIMEvent);
		public static readonly string XML_TYPENAME = nameof(EventInfraPropertyMappedIndexed) + "XML";
		public static readonly string MAP_TYPENAME = nameof(EventInfraPropertyMappedIndexed) + "Map";
		public static readonly string OA_TYPENAME = nameof(EventInfraPropertyMappedIndexed) + "OA";
		public static readonly string AVRO_TYPENAME = nameof(EventInfraPropertyMappedIndexed) + "Avro";
		public static readonly string JSON_TYPENAME = nameof(EventInfraPropertyMappedIndexed) + "Json";
		public static readonly string JSONPROVIDED_TYPENAME = nameof(EventInfraPropertyMappedIndexed) + "JsonProvided";

		public void Run(RegressionEnvironment env)
		{
			var path = new RegressionPath();

			RunAssertion(
				env,
				BEAN_TYPE.Name,
				FBEAN,
				new MyIMEvent(new string[] {"v1", "v2"}, Collections.SingletonDataMap("k1", "v1")),
				path);

			RunAssertion(
				env,
				MAP_TYPENAME,
				FMAP,
				TwoEntryMap<string, object>("Indexed", new string[] {"v1", "v2"}, "Mapped", Collections.SingletonDataMap("k1", "v1")),
				path);

			RunAssertion(env, OA_TYPENAME, FOA, new object[] {new string[] {"v1", "v2"}, Collections.SingletonDataMap("k1", "v1")}, path);

			// Avro
			var avroSchema = AvroSchemaUtil.ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME)).AsRecordSchema();
			var datum = new GenericRecord(avroSchema);
			datum.Put("Indexed", Arrays.AsList("v1", "v2"));
			datum.Put("Mapped", Collections.SingletonMap("k1", "v1"));
			RunAssertion(env, AVRO_TYPENAME, FAVRO, datum, path);

			// Json
			var mapType = typeof(IDictionary<string, object>).CleanName();
			env.CompileDeploy($"@public @buseventtype @name('schema') create json schema {JSON_TYPENAME} (Indexed string[], Mapped `{mapType}`)", path);
			var json = "{\"Mapped\":{\"k1\":\"v1\"},\"Indexed\":[\"v1\",\"v2\"]}";
			RunAssertion(env, JSON_TYPENAME, FJSON, json, path);

			// Json+ProvidedClass
			env.CompileDeploy(
				$"@public @buseventtype @name('schema') @JsonSchema(ClassName='{typeof(MyLocalJsonProvided).FullName}') " +
				$"create json schema {JSONPROVIDED_TYPENAME}()",
				path);
			RunAssertion(env, JSONPROVIDED_TYPENAME, FJSON, json, path);
		}

		private void RunAssertion(
			RegressionEnvironment env,
			string typename,
			FunctionSendEvent send,
			object underlying,
			RegressionPath path)
		{
			RunAssertionTypeValidProp(env, typename, underlying);
			RunAssertionTypeInvalidProp(env, typename);

			var stmtText = "@Name('s0') select * from " + typename;
			env.CompileDeploy(stmtText, path).AddListener("s0");

			send.Invoke(env, underlying, typename);
			var @event = env.Listener("s0").AssertOneGetNewAndReset();

			var mappedGetter = @event.EventType.GetGetterMapped("Mapped");
			Assert.AreEqual("v1", mappedGetter.Get(@event, "k1"));

			var indexedGetter = @event.EventType.GetGetterIndexed("Indexed");
			Assert.AreEqual("v2", indexedGetter.Get(@event, 1));

			RunAssertionEventInvalidProp(@event);
			SupportEventTypeAssertionUtil.AssertConsistency(@event);

			env.UndeployAll();
		}

		private void RunAssertionEventInvalidProp(EventBean @event)
		{
			foreach (var prop in Arrays.AsList("xxxx", "Mapped[1]", "Indexed('a')", "Mapped.x", "Indexed.x")) {
				SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
				SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
			}
		}

		private void RunAssertionTypeValidProp(
			RegressionEnvironment env,
			string typeName,
			object underlying)
		{
			var eventType = env.Runtime.EventTypeService.GetBusEventType(typeName);
			var mapType = underlying is GenericRecord
				? typeof(IDictionary<string, string>)
				: typeof(IDictionary<string, object>);
			var mapValueType = mapType.GetDictionaryValueType();

			object[][] expectedType = {
				new object[] { "Indexed", typeof(string[]), null, null },
				new object[] { "Mapped", mapType, null, null }
			};
			SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, eventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

			EPAssertionUtil.AssertEqualsAnyOrder(new string[] {"Indexed", "Mapped"}, eventType.PropertyNames);

			Assert.IsNotNull(eventType.GetGetter("Mapped"));
			Assert.IsNotNull(eventType.GetGetter("Mapped('a')"));
			Assert.IsNotNull(eventType.GetGetter("Indexed"));
			Assert.IsNotNull(eventType.GetGetter("Indexed[0]"));
			Assert.IsTrue(eventType.IsProperty("Mapped"));
			Assert.IsTrue(eventType.IsProperty("Mapped('a')"));
			Assert.IsTrue(eventType.IsProperty("Indexed"));
			Assert.IsTrue(eventType.IsProperty("Indexed[0]"));
			
			Assert.AreEqual(mapType, eventType.GetPropertyType("Mapped"));
			Assert.AreEqual(mapValueType, eventType.GetPropertyType("Mapped('a')"));
			
			// underlying is GenericRecord ? typeof(ICollection) : typeof(string[]), eventType.GetPropertyType("Indexed")
			
			Assert.AreEqual(typeof(string[]), eventType.GetPropertyType("Indexed"));
			Assert.AreEqual(typeof(string), eventType.GetPropertyType("Indexed[0]"));

			Assert.AreEqual(
				new EventPropertyDescriptor(
					"Indexed",
					typeof(string[]),
					typeof(string),
					false,
					false,
					true,
					false,
					false),
				eventType.GetPropertyDescriptor("Indexed"));
			
			Assert.AreEqual(
				new EventPropertyDescriptor(
					"Mapped",
					mapType,
					mapValueType,
					false,
					false,
					false,
					true,
					false),
				eventType.GetPropertyDescriptor("Mapped"));

			Assert.IsNull(eventType.GetFragmentType("Indexed"));
			Assert.IsNull(eventType.GetFragmentType("Mapped"));
		}

		private void RunAssertionTypeInvalidProp(
			RegressionEnvironment env,
			string typeName)
		{
			var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

			foreach (var prop in Arrays.AsList("xxxx", "myString[0]", "Indexed('a')", "Indexed.x", "Mapped[0]", "Mapped.x")) {
				Assert.AreEqual(false, eventType.IsProperty(prop));
				Assert.AreEqual(null, eventType.GetPropertyType(prop));
				Assert.IsNull(eventType.GetPropertyDescriptor(prop));
			}
		}

		public class MyIMEvent
		{
			public MyIMEvent(
				string[] indexed,
				IDictionary<string, object> mapped)
			{
				this.Indexed = indexed;
				this.Mapped = mapped;
			}

			public string[] Indexed { get; }

			public IDictionary<string, object> Mapped { get; }
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public string[] Indexed;
			public IDictionary<string, object> Mapped;
		}
	}
} // end of namespace
