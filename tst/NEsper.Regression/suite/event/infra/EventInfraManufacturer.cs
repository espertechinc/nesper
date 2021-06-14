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

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.@internal.kernel.service;

using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraManufacturer : RegressionExecution
	{
		public static readonly string XML_TYPENAME = nameof(EventInfraManufacturer) + "XML";
		public static readonly string AVRO_TYPENAME = nameof(EventInfraManufacturer) + "AVRO";

		public void Run(RegressionEnvironment env)
		{
			// Bean
			RunAssertion(
				env,
				"create schema BeanEvent as " + typeof(MyLocalBeanEvent).MaskTypeName(),
				und => {
					var bean = (MyLocalBeanEvent) und;
					Assert.AreEqual("a", bean.P1);
					Assert.AreEqual(1, bean.P2);
				});

			// Map
			RunAssertion(
				env,
				"create map schema MapEvent(P1 string, P2 int)",
				und => {
					EPAssertionUtil.AssertEqualsExactOrder(new object[] {"a", 1}, ((IDictionary<string, object>) und).Values);
				});

			// Object-array
			RunAssertion(
				env,
				"create objectarray schema MapEvent(P1 string, P2 int)",
				und => {
					EPAssertionUtil.AssertEqualsExactOrder(new object[] {"a", 1}, (object[]) und);
				});

			// Avro
			RunAssertion(
				env,
				"select * from " + AVRO_TYPENAME,
				und => {
					var rec = (GenericRecord) und;
					Assert.AreEqual("a", rec.Get("P1"));
					Assert.AreEqual(1, rec.Get("P2"));
				});

			// Json
			RunAssertion(
				env,
				"create json schema JsonEvent(P1 string, P2 int)",
				und => {
					EPAssertionUtil.AssertEqualsExactOrder(new object[] {"a", 1}, ((IDictionary<string, object>) und).Values);
				});

			// Json-Class-Provided
			RunAssertion(
				env,
				"@JsonSchema(ClassName='" + typeof(MyLocalJsonProvided).MaskTypeName() + "') create json schema JsonEvent()",
				und => {
					var received = (MyLocalJsonProvided) und;
					Assert.AreEqual("a", received.P1);
					Assert.AreEqual(1, received.P2);
				});
		}

		private void RunAssertion(
			RegressionEnvironment env,
			string epl,
			Consumer<object> underlyingAssertion)
		{
			env.CompileDeploy("@public @name('schema') " + epl);

			var type = (EventTypeSPI) env.Deployment.GetDeployment(env.DeploymentId("schema")).Statements[0].EventType;

			var writables = EventTypeUtility.GetWriteableProperties(type, true, true);
			var props = new WriteablePropertyDescriptor[2];
			props[0] = FindProp(writables, "P1");
			props[1] = FindProp(writables, "P2");

			var spi = (EPRuntimeSPI) env.Runtime;
			EventBeanManufacturer manufacturer;

			var forge = EventTypeUtility.GetManufacturer(
				type,
				props,
				spi.ServicesContext.ImportServiceRuntime,
				true,
				spi.ServicesContext.EventTypeAvroHandler);
			manufacturer = forge.GetManufacturer(spi.ServicesContext.EventBeanTypedEventFactory);

			var @event = manufacturer.Make(new object[] {"a", 1});
			underlyingAssertion.Invoke(@event.Underlying);
			Assert.AreSame(@event.EventType, type);

			var underlying = manufacturer.MakeUnderlying(new object[] {"a", 1});
			underlyingAssertion.Invoke(underlying);

			env.UndeployAll();
		}

		private WriteablePropertyDescriptor FindProp(
			ISet<WriteablePropertyDescriptor> writables,
			string propertyName)
		{
			foreach (var prop in writables) {
				if (prop.PropertyName == propertyName) {
					return prop;
				}
			}

			Assert.Fail($"Unable to find property {propertyName}");
			return null;
		}

		public class MyLocalBeanEvent
		{
			public string P1 { get; set; }
			public int P2 { get; set; }
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public string P1;
			public int P2;
		}
	}
} // end of namespace
