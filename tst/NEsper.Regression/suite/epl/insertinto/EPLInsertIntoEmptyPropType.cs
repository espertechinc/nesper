///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertEquals

// assertTrue

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
	/// <summary>
	/// Test for populating an empty type:
	/// - an empty insert-into property list is allowed, i.e. "insert into EmptySchema()"
	/// - an empty select-clause is not allowed, i.e. "select from xxx" fails
	/// - we require "select null from" (unnamed null column) for populating an empty type
	/// </summary>
	public class EPLInsertIntoEmptyPropType
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EPLInsertIntoNamedWindowModelAfter());
			execs.Add(new EPLInsertIntoCreateSchemaInsertInto());
			return execs;
		}

		private class EPLInsertIntoNamedWindowModelAfter : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				var path = new RegressionPath();
				env.CompileDeploy("@public create schema EmptyPropSchema()", path);
				env.CompileDeploy(
					"@name('window') @public create window EmptyPropWin#keepall as EmptyPropSchema",
					path);
				env.CompileDeploy("insert into EmptyPropWin() select null from SupportBean", path);

				env.SendEventBean(new SupportBean());

				env.AssertIterator(
					"window",
					iterator => {
						var events = EPAssertionUtil.EnumeratorToArray(iterator);
						Assert.AreEqual(1, events.Length);
						Assert.AreEqual("EmptyPropWin", events[0].EventType.Name);
					});

				// try fire-and-forget query
				env.CompileExecuteFAFNoResult("insert into EmptyPropWin select null", path);
				env.AssertIterator(
					"window",
					iterator => Assert.AreEqual(2, EPAssertionUtil.EnumeratorToArray(iterator).Length));
				env.CompileExecuteFAFNoResult("delete from EmptyPropWin", path); // empty window

				// try on-merge
				env.CompileDeploy(
					"on SupportBean_S0 merge EmptyPropWin " +
					"when not matched then insert select null",
					path);
				env.SendEventBean(new SupportBean_S0(0));
				env.AssertIterator(
					"window",
					iterator => Assert.AreEqual(1, EPAssertionUtil.EnumeratorToArray(iterator).Length));

				// try on-insert
				env.CompileDeploy("on SupportBean_S1 insert into EmptyPropWin select null", path);
				env.SendEventBean(new SupportBean_S1(0));
				env.AssertIterator(
					"window",
					iterator => Assert.AreEqual(2, EPAssertionUtil.EnumeratorToArray(iterator).Length));

				env.UndeployAll();
			}
		}

		private class EPLInsertIntoCreateSchemaInsertInto : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryAssertionInsertMap(env, true);
				TryAssertionInsertMap(env, false);
				TryAssertionInsertOA(env);
				TryAssertionInsertBean(env);
			}
		}

		private static void TryAssertionInsertBean(RegressionEnvironment env)
		{
			var path = new RegressionPath();
			env.CompileDeploy(
				"@public create schema MyBeanWithoutProps as " + typeof(SupportBeanWithoutProps).FullName,
				path);
			env.CompileDeploy("@public insert into MyBeanWithoutProps select null from SupportBean", path);
			env.CompileDeploy("@name('s0') select * from MyBeanWithoutProps", path).AddListener("s0");

			env.SendEventBean(new SupportBean());
			env.AssertEventNew("s0", @event => Assert.IsTrue(@event.Underlying is SupportBeanWithoutProps));

			env.UndeployAll();
		}

		private static void TryAssertionInsertMap(
			RegressionEnvironment env,
			bool soda)
		{
			var path = new RegressionPath();
			env.CompileDeploy(soda, "@public create map schema EmptyMapSchema as ()", path);
			env.CompileDeploy("insert into EmptyMapSchema() select null from SupportBean", path);
			env.CompileDeploy("@name('s0') select * from EmptyMapSchema", path).AddListener("s0");

			env.SendEventBean(new SupportBean());
			env.AssertEventNew(
				"s0",
				@event => {
					Assert.IsTrue(((IDictionary<string, object>)@event.Underlying).IsEmpty());
					Assert.AreEqual(0, @event.EventType.PropertyDescriptors.Count);
				});
			env.UndeployAll();
		}

		private static void TryAssertionInsertOA(RegressionEnvironment env)
		{
			var path = new RegressionPath();
			env.CompileDeploy("@public create objectarray schema EmptyOASchema()", path);
			env.CompileDeploy("@public insert into EmptyOASchema select null from SupportBean", path);
			env.CompileDeploy("@name('s0') select * from EmptyOASchema", path).AddListener("s0").SetSubscriber("s0");

			env.SendEventBean(new SupportBean());
			env.AssertEventNew("s0", @event => Assert.AreEqual(0, ((object[])@event.Underlying).Length));

			env.AssertSubscriber(
				"s0",
				subscriber => {
					var lastNewSubscriberData = subscriber.LastNewData;
					Assert.AreEqual(1, lastNewSubscriberData.Length);
					Assert.AreEqual(0, ((object[])lastNewSubscriberData[0]).Length);
				});

			env.UndeployAll();
		}
	}
} // end of namespace
