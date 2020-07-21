///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{

	public class EventInfraGetterSimpleNoFragment : RegressionExecution
	{
		public const string XMLTYPENAME = nameof(EventInfraGetterSimpleNoFragment) + "XML";

		public void Run(RegressionEnvironment env)
		{
			// Bean
			BiConsumer<EventType, string> bean = (
				type,
				property) => {
				env.SendEventBean(new LocalEvent(property));
			};
			var beanepl = "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).FullName + ";\n";
			RunAssertion(env, "LocalEvent", beanepl, bean);

			// Map
			BiConsumer<EventType, string> map = (
				type,
				property) => {
				env.SendEventMap(Collections.SingletonDataMap("property", property), "LocalEvent");
			};
			var mapepl = "@public @buseventtype create schema LocalEvent(property string);\n";
			RunAssertion(env, "LocalEvent", mapepl, map);

			// Object-array
			BiConsumer<EventType, string> oa = (
				type,
				property) => {
				env.SendEventObjectArray(new object[] {property}, "LocalEvent");
			};
			var oaepl = "@public @buseventtype create objectarray schema LocalEvent(property string);\n";
			RunAssertion(env, "LocalEvent", oaepl, oa);

			// Json
			BiConsumer<EventType, string> json = (
				type,
				property) => {
				env.SendEventJson(new JObject(new JProperty("property", property)).ToString(), "LocalEvent");
			};
			RunAssertion(env, "LocalEvent", "@public @buseventtype create json schema LocalEvent(property string);\n", json);

			// Json-Class-Provided
			RunAssertion(
				env,
				"LocalEvent",
				"@JsonSchema(className='" + typeof(MyLocalJsonProvided).FullName + "') @public @buseventtype create json schema LocalEvent();\n",
				json);

			// Avro
			BiConsumer<EventType, string> avro = (
				type,
				property) => {
				var theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(type).AsRecordSchema());
				theEvent.Put("property", property);
				env.SendEventAvro(theEvent, type.Name);
			};
			RunAssertion(env, "LocalEvent", "@public @buseventtype create avro schema LocalEvent(property string);\n", avro);

			// XML
			BiConsumer<EventType, string> xml = (
				type,
				property) => {
				var doc = "<" + XMLTYPENAME + (property != null ? " property=\"" + property + "\"" : "") + "/>";
				SupportXML.SendXMLEvent(env, doc, XMLTYPENAME);
			};
			RunAssertion(env, XMLTYPENAME, "", xml);
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string typeName,
			string createSchemaEPL,
			BiConsumer<EventType, string> sender)
		{

			var epl = createSchemaEPL +
			          "@name('s0') select * from " +
			          typeName +
			          ";\n" +
			          "@name('s1') select property as c0, exists(property) as c1, typeof(property) as c2 from " +
			          typeName +
			          ";\n";
			env.CompileDeploy(epl).AddListener("s0").AddListener("s1");
			var eventType = env.Statement("s0").EventType;

			var g0 = eventType.GetGetter("property");

			sender.Invoke(eventType, "a");
			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, "a");
			AssertProps(env, "a");

			sender.Invoke(eventType, null);
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, null);
			AssertProps(env, null);

			env.UndeployAll();
		}

		private void AssertProps(
			RegressionEnvironment env,
			string expected)
		{
			EPAssertionUtil.AssertProps(
				env.Listener("s1").AssertOneGetNewAndReset(),
				"c0,c1,c2".SplitCsv(),
				new object[] {expected, true, expected == null ? null : nameof(String)});
		}

		private void AssertGetter(
			EventBean @event,
			EventPropertyGetter getter,
			string value)
		{
			Assert.IsTrue(getter.IsExistsProperty(@event));
			Assert.AreEqual(value, getter.Get(@event));
			Assert.IsNull(getter.GetFragment(@event));
		}

		public class LocalEvent
		{
			public LocalEvent(string property)
			{
				this.Property = property;
			}

			public string Property { get; }
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public string property;
		}
	}
} // end of namespace
