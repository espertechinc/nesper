///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.json;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
	public class EventJsonInherits
	{

		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EventJsonInheritsTwoLevel());
			execs.Add(new EventJsonInheritsFourLevel());
			execs.Add(new EventJsonInheritsFourLevelSparseOne());
			execs.Add(new EventJsonInheritsFourLevelSparseTwo());
			execs.Add(new EventJsonInheritsFourLevelEmpty());
			execs.Add(new EventJsonInheritsTwoLevelBranched());
			execs.Add(new EventJsonInheritsTwoLevelWArrayAndObject());
			execs.Add(new EventJsonInheritsAcrossModules());
			execs.Add(new EventJsonInheritsDynamicPropsParentOnly());
			execs.Add(new EventJsonInheritsDynamicPropsChildOnly());
			return execs;
		}

		internal class EventJsonInheritsDynamicPropsParentOnly : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@JsonSchema(Dynamic=true) create json schema ParentEvent();\n" +
					"@public @buseventtype create json schema ChildEvent() inherits ParentEvent;\n" +
					"@Name('s0') select value? as c0 from ChildEvent#keepall;\n" +
					"@Name('s1') select * from ChildEvent#keepall;\n";
				env.CompileDeploy(epl).AddListener("s0").AddListener("s1");
				RunAssertionDynamicProps(env);
				env.UndeployAll();
			}
		}

		internal class EventJsonInheritsDynamicPropsChildOnly : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"create json schema ParentEvent();\n" +
					"@JsonSchema(Dynamic=true) @public @buseventtype create json schema ChildEvent() inherits ParentEvent;\n" +
					"@Name('s0') select value? as c0 from ChildEvent#keepall;\n" +
					"@Name('s1') select * from ChildEvent#keepall;\n";
				env.CompileDeploy(epl).AddListener("s0").AddListener("s1");
				RunAssertionDynamicProps(env);
				env.UndeployAll();
			}
		}

		internal class EventJsonInheritsAcrossModules : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.CompileDeploy("module A; create json schema A(a1 string)", path);
				env.CompileDeploy("module B; create json schema B(b1 string) inherits A", path);
				env.CompileDeploy("module C; @public @buseventtype create json schema C(c1 string) inherits B", path);
				env.CompileDeploy("@Name('s0') select * from C#keepall", path).AddListener("s0");

				env.SendEventJson("{ \"a1\": \"a\", \"b1\": \"b\", \"c1\": \"c\"}", "C");
				AssertEvent(env.Listener("s0").AssertOneGetNewAndReset());

				env.Milestone(0);

				using (var it = env.Statement("s0").GetEnumerator()) {
					AssertEvent(it.Advance());
				}

				env.UndeployAll();
			}

			private void AssertEvent(EventBean @event)
			{
				EPAssertionUtil.AssertProps(@event, "a1,b1,c1".SplitCsv(), new object[] {"a", "b", "c"});
			}
		}

		internal class EventJsonInheritsTwoLevelWArrayAndObject : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "create json schema NestedObject(n1 string);\n" +
				          "@public @buseventtype create json schema P(pn NestedObject, pa int[primitive]);\n" +
				          "@public @buseventtype create json schema C(cn NestedObject, ca int[primitive]) inherits P;\n" +
				          "@Name('s0') select * from C#keepall;\n";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventJson("{ \"pn\": {\"n1\": \"a\"}, \"pa\": [1, 2], \"cn\": {\"n1\": \"b\"}, \"ca\": [3, 4] }", "C");
				AssertEvent(env.Listener("s0").AssertOneGetNewAndReset());

				env.Milestone(0);

				using (var it = env.Statement("s0").GetEnumerator()) {
					AssertEvent(it.Advance());
				}

				env.UndeployAll();
			}

			private void AssertEvent(EventBean @event)
			{
				var und = (JsonEventObjectBase) @event.Underlying;

				Assert.AreEqual("{\"n1\":\"a\"}", und.Get("pn").ToString());
				Assert.AreEqual("{\"n1\":\"b\"}", und.Get("cn").ToString());
				EPAssertionUtil.AssertEqualsExactOrder(new int[] {1, 2}, (int[]) und.Get("pa"));
				EPAssertionUtil.AssertEqualsExactOrder(new int[] {3, 4}, (int[]) und.Get("ca"));
				Assert.AreEqual("{\"pn\":{\"n1\":\"a\"},\"pa\":[1,2],\"cn\":{\"n1\":\"b\"},\"ca\":[3,4]}", und.ToString());
			}
		}

		internal class EventJsonInheritsTwoLevelBranched : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				var epl =
					"@public @buseventtype create json schema P(p1 string);\n" +
					"@public @buseventtype create json schema C1(c11 string, c12 int) inherits P;\n" +
					"@public @buseventtype create json schema C2(c21 string) inherits P;\n" +
					"@public @buseventtype create json schema C3() inherits P;\n" +
					"@Name('sp') select * from P;\n" +
					"@Name('sc1') select * from C1#keepall;\n" +
					"@Name('sc2') select * from C2#keepall;\n" +
					"@Name('sc3') select * from C3#keepall;\n";
				env.CompileDeploy(epl).AddListener("sp").AddListener("sc1").AddListener("sc2").AddListener("sc3");

				var jsonOne = "{\"p1\":\"PA\",\"c11\":\"x\",\"c12\":50}";
				env.SendEventJson(jsonOne, "C1");
				var eventOne = AssertInvoked(env, "sc1", "sp", "sc2,sc3");
				AssertC1(jsonOne, eventOne);

				var jsonTwo = "{\"p1\":\"PB\",\"c21\":\"y\"}";
				env.SendEventJson(jsonTwo, "C2");
				var eventTwo = AssertInvoked(env, "sc2", "sp", "sc1,sc3");
				AssertC2(jsonTwo, eventTwo);

				var jsonThree = "{\"p1\":\"PC\"}";
				env.SendEventJson(jsonThree, "C3");
				var eventThree = AssertInvoked(env, "sc3", "sp", "sc1,sc2");
				AssertC3(jsonThree, eventThree);

				var jsonFour = "{\"p1\":\"PD\"}";
				env.SendEventJson(jsonFour, "P");
				var eventFour = AssertInvoked(env, "sp", null, "sc1,sc2,sc3");
				AssertP(jsonFour, eventFour);

				env.Milestone(0);

				using (var itSC1 = env.Statement("sc1").GetEnumerator()) {
					AssertC1(jsonOne, itSC1.Advance());
				}

				using (var itSC2 = env.Statement("sc2").GetEnumerator()) {
					AssertC2(jsonTwo, itSC2.Advance());
				}

				using (var itSC3 = env.Statement("sc3").GetEnumerator()) {
					AssertC3(jsonThree, itSC3.Advance());
				}

				env.UndeployAll();
			}

			private void AssertP(
				string jsonFour,
				EventBean eventFour)
			{
				var expectedP = new LinkedHashMap<string, object>();
				expectedP.Put("p1", "PD");
				AssertAny(expectedP, jsonFour, eventFour);
			}

			private void AssertC3(
				string jsonThree,
				EventBean eventThree)
			{
				var expectedC3 = new LinkedHashMap<string, object>();
				expectedC3.Put("p1", "PC");
				AssertAny(expectedC3, jsonThree, eventThree);
			}

			private void AssertC2(
				string jsonTwo,
				EventBean eventTwo)
			{
				var expectedC2 = new LinkedHashMap<string, object>();
				expectedC2.Put("p1", "PB");
				expectedC2.Put("c21", "y");
				AssertAny(expectedC2, jsonTwo, eventTwo);
			}

			private void AssertC1(
				string jsonOne,
				EventBean @event)
			{
				var expectedC1 = new LinkedHashMap<string, object>();
				expectedC1.Put("p1", "PA");
				expectedC1.Put("c11", "x");
				expectedC1.Put("c12", 50);
				AssertAny(expectedC1, jsonOne, @event);
			}

			private void AssertAny(
				LinkedHashMap<string, object> expected,
				string jsonOne,
				EventBean @event)
			{
				var und = (JsonEventObject) @event.Underlying;
				SupportJsonEventTypeUtil.CompareDictionaries(expected, und);
				Assert.AreEqual(jsonOne, und.ToString());
			}

			private EventBean AssertInvoked(
				RegressionEnvironment env,
				string undStmt,
				string invokedOther,
				string notInvokedCsv)
			{
				var @event = env.Listener(undStmt).AssertOneGetNewAndReset();
				if (invokedOther != null) {
					env.Listener(invokedOther).AssertInvokedAndReset();
				}

				var splitNotInvoked = notInvokedCsv.SplitCsv();
				foreach (var s in splitNotInvoked) {
					Assert.IsFalse(env.Listener(s).IsInvoked);
				}

				return @event;
			}
		}

		internal class EventJsonInheritsFourLevelEmpty : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@public @buseventtype create json schema A();\n" +
					"@public @buseventtype create json schema B() inherits A;\n" +
					"@public @buseventtype create json schema C() inherits B;\n" +
					"@public @buseventtype create json schema D() inherits C;\n" +
					"@Name('sd') select * from D#keepall;\n";
				env.CompileDeploy(epl).AddListener("sd");

				env.SendEventJson("{}", "D");
				AssertEvent(env.Listener("sd").AssertOneGetNewAndReset());

				env.Milestone(0);

				using (var it = env.Statement("sd").GetEnumerator()) {
					AssertEvent(it.Advance());
				}

				env.UndeployAll();
			}

			private void AssertEvent(EventBean @event)
			{
				var und = (JsonEventObjectBase) @event.Underlying;

				Assert.That(und.NativeCount, Is.Zero);
				Assert.That(und.TryGetValue("x", out var result), Is.False);
				Assert.IsTrue(und.JsonValues.IsEmpty());

				SupportJsonEventTypeUtil.CompareDictionaries(new LinkedHashMap<string, object>(), und);
				Assert.AreEqual("{}", und.ToString());
			}
		}

		internal class EventJsonInheritsFourLevelSparseTwo : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@public @buseventtype create json schema A();\n" +
					"@public @buseventtype create json schema B(b1 double) inherits A;\n" +
					"@public @buseventtype create json schema C() inherits B;\n" +
					"@public @buseventtype create json schema D(d1 string) inherits C;\n" +
					"@Name('sd') select * from D#keepall;\n";
				env.CompileDeploy(epl).AddListener("sd");

				env.SendEventJson("{\"b1\": 4, \"d1\": \"def\"}", "D");
				AssertEvent(env.Listener("sd").AssertOneGetNewAndReset());

				env.Milestone(0);

				using (var it = env.Statement("sd").GetEnumerator()) {
					AssertEvent(it.Advance());
				}

				env.UndeployAll();
			}

			private void AssertEvent(EventBean @event)
			{
				var und = (JsonEventObjectBase) @event.Underlying;

				Assert.That(und.NativeCount, Is.EqualTo(2));
				Assert.That(und.NativeContainsKey("b1"), Is.True);
				Assert.That(und.NativeContainsKey("d1"), Is.True);
				CollectionAssert.AreEquivalent(
					new [] {
						new KeyValuePair<string, object>("b1", 4d),
						new KeyValuePair<string, object>("d1", "def")
					},
					und.NativeEnumerable);

				Assert.IsTrue(und.JsonValues.IsEmpty());

				IDictionary<string, object> compared = new LinkedHashMap<string, object>();
				compared.Put("b1", 4d);
				compared.Put("d1", "def");
				SupportJsonEventTypeUtil.CompareDictionaries(compared, und);

				Assert.AreEqual("{\"b1\":4.0,\"d1\":\"def\"}", und.ToString());
			}
		}

		internal class EventJsonInheritsFourLevelSparseOne : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@public @buseventtype create json schema A(a1 double);\n" +
					"@public @buseventtype create json schema B() inherits A;\n" +
					"@public @buseventtype create json schema C(c1 string) inherits B;\n" +
					"@public @buseventtype create json schema D() inherits C;\n" +
					"@Name('sd') select * from D#keepall;\n";
				env.CompileDeploy(epl).AddListener("sd");

				env.SendEventJson("{\"a1\": 4, \"c1\": \"def\"}", "D");
				AssertEvent(env.Listener("sd").AssertOneGetNewAndReset());

				env.Milestone(0);

				using (var it = env.Statement("sd").GetEnumerator()) {
					AssertEvent(it.Advance());
				}

				env.UndeployAll();
			}

			private void AssertEvent(EventBean @event)
			{
				var und = (JsonEventObjectBase) @event.Underlying;

				Assert.That(und.NativeCount, Is.EqualTo(2));
				Assert.That(und.NativeContainsKey("a1"), Is.True);
				Assert.That(und.NativeContainsKey("c1"), Is.True);
				CollectionAssert.AreEquivalent(
					new[] {"a1", "c1"},
					und.NativeKeys);
				CollectionAssert.AreEquivalent(
					new [] {
						new KeyValuePair<string, object>("a1", 4d),
						new KeyValuePair<string, object>("c1", "def")
					},
					und.NativeEnumerable);

				Assert.IsTrue(und.JsonValues.IsEmpty());

				IDictionary<string, object> compared = new LinkedHashMap<string, object>();
				compared.Put("a1", 4d);
				compared.Put("c1", "def");
				SupportJsonEventTypeUtil.CompareDictionaries(compared, und);

				Assert.AreEqual("{\"a1\":4.0,\"c1\":\"def\"}", und.ToString());
			}
		}

		internal class EventJsonInheritsFourLevel : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@public @buseventtype create json schema A(a1 double);\n" +
					"@public @buseventtype create json schema B(b1 string, b2 int) inherits A;\n" +
					"@public @buseventtype create json schema C(c1 string) inherits B;\n" +
					"@public @buseventtype create json schema D(d1 double, d2 int) inherits C;\n" +
					"@Name('sa') select * from A#keepall;\n" +
					"@Name('sb') select * from B#keepall;\n" +
					"@Name('sc') select * from C#keepall;\n" +
					"@Name('sd') select * from D#keepall;\n";
				env.CompileDeploy(epl).AddListener("sa").AddListener("sb").AddListener("sc").AddListener("sd");

				env.SendEventJson("{\"d2\": 1, \"d1\": 2, \"c1\": \"def\", \"b2\": 3, \"b1\": \"x\", \"a1\": 4}", "D");
				var eventOne = env.Listener("sd").AssertOneGetNewAndReset();
				AssertEvent(eventOne);
				env.Listener("sa").AssertInvokedAndReset();
				env.Listener("sb").AssertInvokedAndReset();
				env.Listener("sc").AssertInvokedAndReset();

				env.Milestone(0);

				using (var it = env.Statement("sd").GetEnumerator()) {
					AssertEvent(it.Advance());
				}

				env.UndeployAll();
			}

			private void AssertEvent(EventBean @event)
			{
				var und = (JsonEventObjectBase) @event.Underlying;
				
				Assert.That(und.NativeCount, Is.EqualTo(6));
				Assert.That(und.NativeContainsKey("a1"), Is.True);
				Assert.That(und.NativeContainsKey("b1"), Is.True);
				Assert.That(und.NativeContainsKey("b2"), Is.True);

				CollectionAssert.AreEquivalent(
					new[] {"a1", "b1", "b2", "c1", "d1", "d2"},
					und.NativeKeys);
				
				CollectionAssert.AreEquivalent(
					new [] {
						new KeyValuePair<string, object>("a1", 4d),
						new KeyValuePair<string, object>("c1", "def")
					},
					und.NativeEnumerable);

				Assert.That(und.NativeCount, Is.EqualTo(6));
				Assert.That(und.NativeContainsKey("a1"), Is.True);
				Assert.That(und.NativeContainsKey("a2"), Is.False);
				Assert.That(und.NativeContainsKey("c1"), Is.True);
				CollectionAssert.AreEquivalent(
					new[] {"a1", "b1", "b2", "c1", "d1", "d2"},
					und.NativeKeys);
				CollectionAssert.AreEquivalent(
					new [] {
						new KeyValuePair<string, object>("a1", 4d),
						new KeyValuePair<string, object>("b1", "x"),
						new KeyValuePair<string, object>("b2", 3),
						new KeyValuePair<string, object>("c1", "def"),
						new KeyValuePair<string, object>("d1", 2d),
						new KeyValuePair<string, object>("d2", 1)
					},
					und.NativeEnumerable);
				
				Assert.IsTrue(und.JsonValues.IsEmpty());

				IDictionary<string, object> compared = new LinkedHashMap<string, object>();
				compared.Put("a1", 4d);
				compared.Put("b1", "x");
				compared.Put("b2", 3);
				compared.Put("c1", "def");
				compared.Put("d1", 2d);
				compared.Put("d2", 1);
				SupportJsonEventTypeUtil.CompareDictionaries(compared, und);

				Assert.AreEqual("{\"a1\":4.0,\"b1\":\"x\",\"b2\":3,\"c1\":\"def\",\"d1\":2.0,\"d2\":1}", und.ToString());
			}
		}

		internal class EventJsonInheritsTwoLevel : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@public @buseventtype create json schema ParentJson(p1 string, p2 int);\n" +
				          "@public @buseventtype create json schema ChildJson(c1 string, c2 int) inherits ParentJson;\n" +
				          "@Name('s0') select * from ChildJson#keepall;\n";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventJson("{\"p1\": \"abc\", \"p2\": 10, \"c1\": \"def\", \"c2\": 20}", "ChildJson");
				AssertEvent(env.Listener("s0").AssertOneGetNewAndReset());

				env.Milestone(0);

				using (var it = env.Statement("s0").GetEnumerator()) {
					AssertEvent(it.Advance());
				}

				env.UndeployAll();
			}

			private void AssertEvent(EventBean @event)
			{
				var und = (JsonEventObjectBase) @event.Underlying;

				Assert.That(und.NativeCount, Is.EqualTo(4));
                Assert.That(und.NativeContainsKey("p1"), Is.True);
                Assert.That(und.NativeContainsKey("p3"), Is.False);
                Assert.That(und.NativeContainsKey("c1"), Is.True);
                CollectionAssert.AreEquivalent(
                	new[] {"p1", "p2", "c1", "c2"},
                	und.NativeKeys);
                CollectionAssert.AreEquivalent(
                	new [] {
	                    new KeyValuePair<string, object>("p1", "abc"), 
	                    new KeyValuePair<string, object>("p2", 10), 
	                    new KeyValuePair<string, object>("c1", "def"), 
	                    new KeyValuePair<string, object>("c2", 20)
                	},
                	und.NativeEnumerable);

				Assert.IsTrue(und.JsonValues.IsEmpty());

				IDictionary<string, object> compared = new LinkedHashMap<string, object>();
				compared.Put("p1", "abc");
				compared.Put("p2", 10);
				compared.Put("c1", "def");
				compared.Put("c2", 20);
				SupportJsonEventTypeUtil.CompareDictionaries(compared, und);

				Assert.AreEqual("{\"p1\":\"abc\",\"p2\":10,\"c1\":\"def\",\"c2\":20}", und.ToString());
			}
		}

		private static void RunAssertionDynamicProps(RegressionEnvironment env)
		{
			var jsonOne = "{\"value\":10}";
			object expectedOne = 10;
			SendAssertDynamicProp(env, jsonOne, "ChildEvent", expectedOne);

			var jsonTwo = "{\"value\":\"abc\"}";
			object expectedTwo = "abc";
			SendAssertDynamicProp(env, jsonTwo, "ChildEvent", expectedTwo);

			env.Milestone(0);

			using (var itS0 = env.Statement("s0").GetEnumerator()) {
				Assert.AreEqual(10, itS0.Advance().Get("c0"));
				Assert.AreEqual("abc", itS0.Advance().Get("c0"));
			}

			using (var itS1 = env.Statement("s1").GetEnumerator()) {
				AssertEventJson(itS1.Advance(), jsonOne, expectedOne);
				AssertEventJson(itS1.Advance(), jsonTwo, expectedTwo);
			}
		}

		private static void SendAssertDynamicProp(
			RegressionEnvironment env,
			string json,
			string eventTypeName,
			object expected)
		{
			env.SendEventJson(json, eventTypeName);
			Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

			AssertEventJson(env.Listener("s1").AssertOneGetNewAndReset(), json, expected);
		}

		private static void AssertEventJson(
			EventBean eventBean,
			string json,
			object expected)
		{
			var @event = (JsonEventObjectBase) eventBean.Underlying;
			SupportJsonEventTypeUtil.CompareDictionaries(Collections.SingletonDataMap("value", expected), @event);
			Assert.AreEqual(json, @event.ToString());
		}
	}
} // end of namespace
