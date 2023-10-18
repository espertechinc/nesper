//////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraEventSender : RegressionExecution
	{
		public const string XML_TYPENAME = "EventInfraEventSenderXML";
		public const string MAP_TYPENAME = "EventInfraEventSenderMap";
		public const string OA_TYPENAME = "EventInfraEventSenderOA";
		public const string AVRO_TYPENAME = "EventInfraEventSenderAvro";
		public const string JSON_TYPENAME = "EventInfraEventSenderJson";

		public void Run(RegressionEnvironment env)
		{
			var path = new RegressionPath();

			// Bean
			RunAssertionSendEvent(env, path, "SupportBean", new SupportBean());
			RunAssertionInvalid(
				env,
				"SupportBean",
				new SupportBean_G("G1"),
				"Event object of type " +
				typeof(SupportBean_G).MaskTypeName() +
				" does not equal, extend or implement the type " +
				typeof(SupportBean).MaskTypeName() +
				" of event type 'SupportBean'");
			RunAssertionSendEvent(env, path, "SupportMarkerInterface", new SupportMarkerImplA("Q2"), new SupportBean_G("Q3"));
			RunAssertionRouteEvent(env, path, "SupportBean", new SupportBean());

			// Map
			RunAssertionSendEvent(env, path, MAP_TYPENAME, new Dictionary<string, object>());
			RunAssertionRouteEvent(env, path, MAP_TYPENAME, new Dictionary<string, object>());
			RunAssertionInvalid(
				env,
				MAP_TYPENAME,
				new SupportBean(),
				"Unexpected event object of type " + typeof(SupportBean).MaskTypeName() + ", expected " + typeof(IDictionary<string, object>).CleanName());

			// Object-Array
			RunAssertionSendEvent(env, path, OA_TYPENAME, new object[] { });
			RunAssertionRouteEvent(env, path, OA_TYPENAME, new object[] { });
			RunAssertionInvalid(
				env,
				OA_TYPENAME,
				new SupportBean(),
				"Unexpected event object of type " + typeof(SupportBean).MaskTypeName() + ", expected Object[]");

			// XML
			RunAssertionSendEvent(env, path, XML_TYPENAME, SupportXML.GetDocument("<Myevent/>").DocumentElement);
			RunAssertionRouteEvent(env, path, XML_TYPENAME, SupportXML.GetDocument("<Myevent/>").DocumentElement);
			RunAssertionInvalid(
				env,
				XML_TYPENAME,
				new SupportBean(),
				"Unexpected event object type '" + typeof(SupportBean).MaskTypeName() + "' encountered, please supply a XmlDocument or XmlElement node");
			RunAssertionInvalid(
				env,
				XML_TYPENAME,
				SupportXML.GetDocument("<xxxx/>"),
				"Unexpected root element name 'xxxx' encountered, expected a root element name of 'Myevent'");

			// Avro
			var schema = env.RuntimeAvroSchemaPreconfigured(AVRO_TYPENAME).AsRecordSchema();
			RunAssertionSendEvent(env, path, AVRO_TYPENAME, new GenericRecord(schema));
			RunAssertionRouteEvent(env, path, AVRO_TYPENAME, new GenericRecord(schema));
			RunAssertionInvalid(
				env,
				AVRO_TYPENAME,
				new SupportBean(),
				"Unexpected event object type '" + typeof(SupportBean).MaskTypeName() + "' encountered, please supply a GenericRecord");

			// Json
			var schemas = "@public @buseventtype @name('schema') create json schema " + JSON_TYPENAME + "()";
			env.CompileDeploy(schemas, path);
			RunAssertionSendEvent(env, path, JSON_TYPENAME, "{}");
			RunAssertionRouteEvent(env, path, JSON_TYPENAME, "{}");
			RunAssertionInvalid(
				env,
				JSON_TYPENAME,
				new SupportBean(),
				"Unexpected event object of type '" + nameof(SupportBean) + "', expected a Json-formatted string-type value");

			// No such type
			try {
				env.EventService.GetEventSender("ABC");
				Assert.Fail();
			}
			catch (EventTypeException ex) {
				Assert.AreEqual("Event type named 'ABC' could not be found", ex.Message);
			}

			// Internal implicit wrapper type
			env.CompileDeploy("insert into ABC select *, TheString as value from SupportBean");
			try {
				env.EventService.GetEventSender("ABC");
				Assert.Fail("Event type named 'ABC' could not be found");
			}
			catch (EventTypeException ex) {
				Assert.AreEqual("Event type named 'ABC' could not be found", ex.Message);
			}

			env.UndeployAll();
		}

		public ISet<RegressionFlag> Flags()
		{
			return Collections.Set(RegressionFlag.OBSERVEROPS);
		}

		private void RunAssertionRouteEvent(
			RegressionEnvironment env,
			RegressionPath path,
			string typename,
			object underlying)
		{

			var stmtText = "@name('s0') select * from " + typename;
			env.CompileDeploy(stmtText, path).AddListener("s0");

			var sender = env.EventService.GetEventSender(typename);
			env.CompileDeploy(
				"@public @buseventtype create schema TriggerEvent();\n" +
				"@name('trigger') select * from TriggerEvent;\n");
			env.Statement("trigger").Events += (esender, args) => {
				sender.RouteEvent(underlying);
			};

			env.SendEventMap(EmptyDictionary<string, object>.Instance, "TriggerEvent");
			AssertUnderlying(env, typename, underlying);

			env.UndeployModuleContaining("s0").UndeployModuleContaining("trigger");
		}

		private void RunAssertionSendEvent(
			RegressionEnvironment env,
			RegressionPath path,
			string typename,
			params object[] correctUnderlyings)
		{

			var stmtText = "@name('s0') select * from " + typename;
			env.CompileDeploy(stmtText, path).AddListener("s0");

			var sender = env.EventService.GetEventSender(typename);
			foreach (var underlying in correctUnderlyings) {
				sender.SendEvent(underlying);
				AssertUnderlying(env, typename, underlying);
			}

			env.UndeployModuleContaining("s0");
		}

		private void AssertUnderlying(
			RegressionEnvironment env,
			string typename,
			object underlying)
		{
			if (typename.Equals(JSON_TYPENAME)) {
				env.AssertEventNew("s0", eventBean => Assert.NotNull(eventBean.Underlying));
			} else {
				env.AssertEventNew("s0", eventBean => Assert.AreSame(underlying, eventBean.Underlying));
			}
		}

		private void RunAssertionInvalid(
			RegressionEnvironment env,
			string typename,
			object incorrectUnderlying,
			string message)
		{

			var sender = env.EventService.GetEventSender(typename);

			try {
				sender.SendEvent(incorrectUnderlying);
				Assert.Fail();
			}
			catch (EPException ex) {
				SupportMessageAssertUtil.AssertMessage(ex, message);
			}

			try {
				sender.RouteEvent(incorrectUnderlying);
				Assert.Fail();
			}
			catch (EPException ex) {
				SupportMessageAssertUtil.AssertMessage(ex, message);
			}
		}
	}
} // end of namespace
