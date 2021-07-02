///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.io;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using Microsoft.CodeAnalysis;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil; // TryInvalidCompile;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
	public class EventBeanSchemaGenericType
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EventBeanSchemaParamsSingleParameter());
			execs.Add(new EventBeanSchemaParamsTwoParameter());
			execs.Add(new EventBeanSchemaParamsInvalid());
			return execs;
		}

		internal class EventBeanSchemaParamsInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl;

				epl = "create schema MyEvent as " + typeof(MyLocalUnparameterized).Name + "<Integer>";
				TryInvalidCompile(
					env,
					epl,
					"Number of type parameters mismatch, the class '" +
					typeof(MyLocalUnparameterized).Name +
					"' has 0 type parameters but specified are 1 type parameters");

				epl = "create schema MyEvent as " + typeof(MyLocalOneParameter<>).Name + "<Integer, String>";
				TryInvalidCompile(
					env,
					epl,
					"Number of type parameters mismatch, the class '" +
					typeof(MyLocalOneParameter<>).Name +
					"' has 1 type parameters but specified are 2 type parameters");

				epl = "create schema MyEvent as " + typeof(MyLocalUnparameterized).Name + "[]";
				TryInvalidCompile(
					env,
					epl,
					"Array dimensions are not allowed");

				epl = "create schema MyEvent as " + typeof(MyLocalOneParameter<>).Name + "<Dummy>";
				TryInvalidCompile(
					env,
					epl,
					"Failed to resolve type parameter 0 of type 'Dummy': Could not load class by name 'Dummy', please check imports");

				epl = "create schema MyEvent as " + typeof(MyLocalBoundParameter<>).Name + "<String>";
				TryInvalidCompile(
					env,
					epl,
					"Bound type parameters 0 named 'T' expects 'java.lang.Number' but receives 'java.lang.String'");

				epl = "create schema MyEvent as " + typeof(MyLocalBoundParameter<>).Name + "<int>";
				TryInvalidCompile(
					env,
					epl,
					"Failed to resolve type parameter 0 of type 'int': Could not load class by name 'int', please check imports");
			}
		}

		internal class EventBeanSchemaParamsSingleParameter : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string single = typeof(SupportBeanParameterizedSingle<>).Name;

				RunAssertionSingleParam(
					env,
					single + "<int>",
					typeof(SupportBeanParameterizedSingle<int>),
					typeof(int),
					new SupportBeanParameterizedSingle<int>(10),
					10);

				RunAssertionSingleParam(
					env,
					single + "<string>",
					typeof(SupportBeanParameterizedSingle<string>),
					typeof(string),
					new SupportBeanParameterizedSingle<string>("x"),
					"x");

				string[] data = "a,b".SplitCsv();
				RunAssertionSingleParam(
					env,
					single + "<string[]>",
					typeof(SupportBeanParameterizedSingle<string[]>),
					typeof(string[]),
					new SupportBeanParameterizedSingle<string[]>(data),
					data);

				RunAssertionSingleParam(
					env,
					single,
					typeof(SupportBeanParameterizedSingle<object>),
					typeof(object),
					new SupportBeanParameterizedSingle<object>(100L),
					100L);

				var optionalLong = typeof(Optional<long>);
				var optionalLongValue = new Optional<long>(10L);
				RunAssertionSingleParam(
					env,
					single + "<" + optionalLong.CleanName() + ">",
					typeof(SupportBeanParameterizedSingle<Optional<long>>),
					optionalLong,
					new SupportBeanParameterizedSingle<Optional<long>>(optionalLongValue),
					optionalLongValue);

				RunAssertionSingleParam(
					env,
					typeof(MyLocalBoundParameter<>).Name + "<long>",
					typeof(MyLocalBoundParameter<long>),
					typeof(long),
					new MyLocalBoundParameter<long>(100L),
					100L);
			}
		}

		internal class EventBeanSchemaParamsTwoParameter : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string two = typeof(SupportBeanParameterizedTwo<,>).Name;

				RunAssertionTwoParam(
					env,
					two + "<Double, String>",
					typeof(SupportBeanParameterizedTwo<double, string>),
					typeof(double),
					typeof(string),
					new SupportBeanParameterizedTwo<double, string>(10d, "A"),
					10d,
					"A");

				var dtx = DateTimeEx.NowUtc();
				var buf = new ByteBuffer(new byte[] {1, 2});
				RunAssertionTwoParam(
					env,
					two + "<java.nio.ByteBuffer, java.util.Calendar>",
					typeof(SupportBeanParameterizedTwo<ByteBuffer, DateTimeEx>),
					typeof(ByteBuffer),
					typeof(DateTimeEx),
					new SupportBeanParameterizedTwo<ByteBuffer, DateTimeEx>(buf, dtx),
					buf,
					dtx);

				RunAssertionTwoParam(
					env,
					two,
					typeof(SupportBeanParameterizedTwo<object, object>),
					typeof(object),
					typeof(object),
					new SupportBeanParameterizedTwo<object, object>(1, "a"),
					1,
					"a");
			}
		}

		private static void RunAssertionSingleParam(
			RegressionEnvironment env,
			string typeName,
			Type expectedUnderlying,
			Type expectedProperty,
			object @event,
			object expected)
		{
			string epl =
				"@name('schema') @public @buseventtype create schema MyEvent as " + typeName + ";\n" +
				"@name('s0') select simpleProperty as c0 from MyEvent;\n";
			env.CompileDeploy(epl).AddListener("s0");

			EventType schemaType = env.Statement("schema").EventType;
			Assert.AreEqual(expectedUnderlying, schemaType.UnderlyingType);
			IList<EventPropertyDescriptor> received = schemaType.PropertyDescriptors;
			bool fragment = received[0].IsFragment; // ignore fragment, mapped, indexed flags
			bool indexed = received[0].IsIndexed; // ignore fragment, mapped, indexed flags
			bool mapped = received[0].IsMapped; // ignore fragment, mapped, indexed flags
			SupportEventPropUtil.AssertPropsEquals(
				received,
				new SupportEventPropDesc("simpleProperty", expectedProperty).WithFragment(fragment).WithIndexed(indexed).WithMapped(mapped));

			SupportEventPropUtil.AssertPropsEquals(
				env.Statement("s0").EventType.PropertyDescriptors,
				new SupportEventPropDesc("c0", expectedProperty).WithFragment(fragment).WithIndexed(indexed).WithMapped(mapped));

			env.SendEventBean(@event, "MyEvent");
			Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

			env.UndeployAll();
		}

		private static void RunAssertionTwoParam(
			RegressionEnvironment env,
			string typeName,
			Type expectedUnderlying,
			Type expectedOne,
			Type expectedTwo,
			object @event,
			object valueOne,
			object valueTwo)
		{
			string epl =
				"@name('schema') @public @buseventtype create schema MyEvent as " + typeName + ";\n" +
				"@name('s0') select one as c0, two as c1 from MyEvent;\n";
			env.CompileDeploy(epl).AddListener("s0");

			EventType schemaType = env.Statement("schema").EventType;
			Assert.AreEqual(expectedUnderlying, schemaType.UnderlyingType);
			Assert.AreEqual(expectedOne, schemaType.GetPropertyType("one"));
			Assert.AreEqual(expectedTwo, schemaType.GetPropertyType("two"));

			EventType s0Type = env.Statement("s0").EventType;
			Assert.AreEqual(expectedOne, s0Type.GetPropertyType("c0"));
			Assert.AreEqual(expectedTwo, s0Type.GetPropertyType("c1"));

			env.SendEventBean(@event, "MyEvent");
			EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "c0,c1".SplitCsv(), new object[] {valueOne, valueTwo});

			env.UndeployAll();
		}

		public class MyLocalUnparameterized
		{
		}

		public class MyLocalOneParameter<T>
		{
		}

		public class MyLocalBoundParameter<T>
		{
			public MyLocalBoundParameter(T simpleProperty)
			{
				this.SimpleProperty = simpleProperty;
			}

			public T SimpleProperty { get; }
		}
	}
} // end of namespace
