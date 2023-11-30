///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
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
            WithTwoLevel(execs);
            WithFourLevel(execs);
            WithFourLevelSparseOne(execs);
            WithFourLevelSparseTwo(execs);
            WithFourLevelEmpty(execs);
            WithTwoLevelBranched(execs);
            WithTwoLevelWArrayAndObject(execs);
            WithAcrossModules(execs);
            WithDynamicPropsParentOnly(execs);
            WithDynamicPropsChildOnly(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicPropsChildOnly(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonInheritsDynamicPropsChildOnly());
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicPropsParentOnly(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonInheritsDynamicPropsParentOnly());
            return execs;
        }

        public static IList<RegressionExecution> WithAcrossModules(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonInheritsAcrossModules());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoLevelWArrayAndObject(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonInheritsTwoLevelWArrayAndObject());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoLevelBranched(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonInheritsTwoLevelBranched());
            return execs;
        }

        public static IList<RegressionExecution> WithFourLevelEmpty(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonInheritsFourLevelEmpty());
            return execs;
        }

        public static IList<RegressionExecution> WithFourLevelSparseTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonInheritsFourLevelSparseTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithFourLevelSparseOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonInheritsFourLevelSparseOne());
            return execs;
        }

        public static IList<RegressionExecution> WithFourLevel(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonInheritsFourLevel());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoLevel(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonInheritsTwoLevel());
            return execs;
        }

        private class EventJsonInheritsDynamicPropsParentOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@JsonSchema(Dynamic=true) create json schema ParentEvent();\n" +
                    "@public @buseventtype create json schema ChildEvent() inherits ParentEvent;\n" +
                    "@name('s0') select value? as c0 from ChildEvent#keepall;\n" +
                    "@name('s1') select * from ChildEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0").AddListener("s1");
                RunAssertionDynamicProps(env);
                env.UndeployAll();
            }
        }

        private class EventJsonInheritsDynamicPropsChildOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create json schema ParentEvent();\n" +
                    "@JsonSchema(Dynamic=true) @public @buseventtype create json schema ChildEvent() inherits ParentEvent;\n" +
                    "@name('s0') select value? as c0 from ChildEvent#keepall;\n" +
                    "@name('s1') select * from ChildEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0").AddListener("s1");
                RunAssertionDynamicProps(env);
                env.UndeployAll();
            }
        }

        private class EventJsonInheritsAcrossModules : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("module A; @public create json schema A(a1 string)", path);
                env.CompileDeploy("module B; @public create json schema B(b1 string) inherits A", path);
                env.CompileDeploy("module C; @public @buseventtype create json schema C(c1 string) inherits B", path);
                env.CompileDeploy("@name('s0') select * from C#keepall", path).AddListener("s0");

                env.SendEventJson("{ \"a1\": \"a\", \"b1\": \"b\", \"c1\": \"c\"}", "C");
                env.AssertEventNew("s0", this.AssertEvent);

                env.Milestone(0);

                env.AssertIterator("s0", iterator => AssertEvent(iterator.Advance()));

                env.UndeployAll();
            }

            private void AssertEvent(EventBean @event)
            {
                EPAssertionUtil.AssertProps(@event, "a1,b1,c1".SplitCsv(), new object[] { "a", "b", "c" });
            }
        }

        private class EventJsonInheritsTwoLevelWArrayAndObject : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create json schema NestedObject(n1 string);\n" +
                          "@public @buseventtype create json schema P(pn NestedObject, pa int[primitive]);\n" +
                          "@public @buseventtype create json schema C(cn NestedObject, ca int[primitive]) inherits P;\n" +
                          "@name('s0') select * from C#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventJson(
                    "{ \"pn\": {\"n1\": \"a\"}, \"pa\": [1, 2], \"cn\": {\"n1\": \"b\"}, \"ca\": [3, 4] }",
                    "C");
                env.AssertEventNew("s0", this.AssertEvent);

                env.Milestone(0);

                env.AssertIterator("s0", iterator => AssertEvent(iterator.Advance()));

                env.UndeployAll();
            }

            private void AssertEvent(EventBean @event)
            {
                var und = (JsonEventObjectBase)@event.Underlying;

                Assert.AreEqual("{\"n1\":\"a\"}", und.Get("pn").ToString());
                Assert.AreEqual("{\"n1\":\"b\"}", und.Get("cn").ToString());
                EPAssertionUtil.AssertEqualsExactOrder(new int[] { 1, 2 }, (int[])und.Get("pa"));
                EPAssertionUtil.AssertEqualsExactOrder(new int[] { 3, 4 }, (int[])und.Get("ca"));
                Assert.AreEqual(
                    "{\"pn\":{\"n1\":\"a\"},\"pa\":[1,2],\"cn\":{\"n1\":\"b\"},\"ca\":[3,4]}",
                    und.ToString());
            }
        }

        private class EventJsonInheritsTwoLevelBranched : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create json schema P(p1 string);\n" +
                    "@public @buseventtype create json schema C1(c11 string, c12 int) inherits P;\n" +
                    "@public @buseventtype create json schema C2(c21 string) inherits P;\n" +
                    "@public @buseventtype create json schema C3() inherits P;\n" +
                    "@name('sp') select * from P;\n" +
                    "@name('sc1') select * from C1#keepall;\n" +
                    "@name('sc2') select * from C2#keepall;\n" +
                    "@name('sc3') select * from C3#keepall;\n";
                env.CompileDeploy(epl).AddListener("sp").AddListener("sc1").AddListener("sc2").AddListener("sc3");

                var jsonOne = "{\"p1\":\"PA\",\"c11\":\"x\",\"c12\":50}";
                env.SendEventJson(jsonOne, "C1");
                AssertInvoked(env, "sc1", "sp", "sc2,sc3", @event => AssertC1(jsonOne, @event));

                var jsonTwo = "{\"p1\":\"PB\",\"c21\":\"y\"}";
                env.SendEventJson(jsonTwo, "C2");
                AssertInvoked(env, "sc2", "sp", "sc1,sc3", @event => AssertC2(jsonTwo, @event));

                var jsonThree = "{\"p1\":\"PC\"}";
                env.SendEventJson(jsonThree, "C3");
                AssertInvoked(env, "sc3", "sp", "sc1,sc2", @event => AssertC3(jsonThree, @event));

                var jsonFour = "{\"p1\":\"PD\"}";
                env.SendEventJson(jsonFour, "P");
                AssertInvoked(env, "sp", null, "sc1,sc2,sc3", @event => AssertP(jsonFour, @event));

                env.Milestone(0);

                env.AssertIterator("sc1", it => AssertC1(jsonOne, it.Advance()));
                env.AssertIterator("sc2", it => AssertC2(jsonTwo, it.Advance()));
                env.AssertIterator("sc3", it => AssertC3(jsonThree, it.Advance()));

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
                var und = (JsonEventObject)@event.Underlying;
                SupportJsonEventTypeUtil.CompareDictionaries(expected, und);
                Assert.AreEqual(jsonOne, und.ToString());
            }

            private void AssertInvoked(
                RegressionEnvironment env,
                string undStmt,
                string invokedOther,
                string notInvokedCsv,
                Consumer<EventBean> assertion)
            {
                env.AssertEventNew(
                    undStmt,
                    @event => {
                        if (invokedOther != null) {
                            env.Listener(invokedOther).AssertInvokedAndReset();
                        }

                        var splitNotInvoked = notInvokedCsv.SplitCsv();
                        foreach (var s in splitNotInvoked) {
                            Assert.IsFalse(env.Listener(s).IsInvoked);
                        }

                        assertion.Invoke(@event);
                    });
            }
        }

        private class EventJsonInheritsFourLevelEmpty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create json schema A();\n" +
                    "@public @buseventtype create json schema B() inherits A;\n" +
                    "@public @buseventtype create json schema C() inherits B;\n" +
                    "@public @buseventtype create json schema D() inherits C;\n" +
                    "@name('sd') select * from D#keepall;\n";
                env.CompileDeploy(epl).AddListener("sd");

                env.SendEventJson("{}", "D");
                env.AssertEventNew("sd", this.AssertEvent);

                env.Milestone(0);

                env.AssertIterator("sd", it => AssertEvent(it.Advance()));

                env.UndeployAll();
            }

            private void AssertEvent(EventBean @event)
            {
                var und = (JsonEventObjectBase)@event.Underlying;

                Assert.That(und.NativeCount, Is.Zero);
                Assert.That(und.TryGetValue("x", out var result), Is.False);
                AssertNoSuchElement(() => und.GetNativeValue(0));
                AssertNoSuchElement(() => und.GetNativeKeyName(0));
                AssertNoSuchElement(() => und.GetNativeEntry(0));
                Assert.That(und.TryGetNativeKey("x", out var index), Is.False);
                Assert.IsTrue(und.JsonValues.IsEmpty());

                SupportJsonEventTypeUtil.CompareDictionaries(new LinkedHashMap<string, object>(), und);
                Assert.AreEqual("{}", und.ToString());
            }
        }

        private class EventJsonInheritsFourLevelSparseTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create json schema A();\n" +
                    "@public @buseventtype create json schema B(b1 double) inherits A;\n" +
                    "@public @buseventtype create json schema C() inherits B;\n" +
                    "@public @buseventtype create json schema D(d1 string) inherits C;\n" +
                    "@name('sd') select * from D#keepall;\n";
                env.CompileDeploy(epl).AddListener("sd");

                env.SendEventJson("{\"b1\": 4, \"d1\": \"def\"}", "D");
                env.AssertEventNew("sd", this.AssertEvent);

                env.Milestone(0);

                env.AssertIterator("sd", it => AssertEvent(it.Advance()));

                env.UndeployAll();
            }

            private void AssertEvent(EventBean @event)
            {
                var und = (JsonEventObjectBase)@event.Underlying;

                Assert.That(und.NativeCount, Is.EqualTo(2));
                AssertByIndex(2, und.GetNativeValue, new object[] { 4d, "def" });
                AssertByIndex(2, und.GetNativeKeyName, new object[] { "b1", "d1" });
                AssertByIndex(2, i => und.GetNativeEntry(i), new object[] { ToEntry("b1", 4d), ToEntry("d1", "def") });
                AssertByName(und, "b1,d1");
                Assert.IsTrue(und.JsonValues.IsEmpty());

                IDictionary<string, object> compared = new LinkedHashMap<string, object>();
                compared.Put("b1", 4d);
                compared.Put("d1", "def");
                SupportJsonEventTypeUtil.CompareDictionaries(compared, und);

                Assert.AreEqual("{\"b1\":4.0,\"d1\":\"def\"}", und.ToString());
            }
        }

        private class EventJsonInheritsFourLevelSparseOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create json schema A(a1 double);\n" +
                    "@public @buseventtype create json schema B() inherits A;\n" +
                    "@public @buseventtype create json schema C(c1 string) inherits B;\n" +
                    "@public @buseventtype create json schema D() inherits C;\n" +
                    "@name('sd') select * from D#keepall;\n";
                env.CompileDeploy(epl).AddListener("sd");

                env.SendEventJson("{\"a1\": 4, \"c1\": \"def\"}", "D");
                env.AssertEventNew("sd", this.AssertEvent);

                env.Milestone(0);

                env.AssertIterator("sd", it => AssertEvent(it.Advance()));

                env.UndeployAll();
            }

            private void AssertEvent(EventBean @event)
            {
                var und = (JsonEventObjectBase)@event.Underlying;

                Assert.That(und.NativeCount, Is.EqualTo(2));
                AssertByIndex(2, und.GetNativeValue, new object[] { 4d, "def" });
                AssertByIndex(2, und.GetNativeKeyName, new object[] { "a1", "c1" });
                AssertByIndex(
                    2,
                    und.GetNativeEntry,
                    new[] {
                        ToEntry("a1", 4d),
                        ToEntry("c1", "def")
                    });
                AssertByName(und, "a1,c1");
                Assert.IsTrue(und.JsonValues.IsEmpty());

                IDictionary<string, object> compared = new LinkedHashMap<string, object>();
                compared.Put("a1", 4d);
                compared.Put("c1", "def");
                SupportJsonEventTypeUtil.CompareDictionaries(compared, und);

                Assert.AreEqual("{\"a1\":4.0,\"c1\":\"def\"}", und.ToString());
            }
        }

        private class EventJsonInheritsFourLevel : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@public @buseventtype create json schema A(a1 double);\n" +
                          "@public @buseventtype create json schema B(b1 string, b2 int) inherits A;\n" +
                          "@public @buseventtype create json schema C(c1 string) inherits B;\n" +
                          "@public @buseventtype create json schema D(d1 double, d2 int) inherits C;\n";
                env.CompileDeploy(epl, path);
                env.CompileDeploy("@name('sa') select * from A#keepall", path).AddListener("sa");
                env.CompileDeploy("@name('sb') select * from B#keepall", path).AddListener("sb");
                env.CompileDeploy("@name('sc') select * from C#keepall", path).AddListener("sc");
                env.CompileDeploy("@name('sd') select * from D#keepall", path).AddListener("sd");

                env.SendEventJson("{\"d2\": 1, \"d1\": 2, \"c1\": \"def\", \"b2\": 3, \"b1\": \"x\", \"a1\": 4}", "D");
                env.AssertEventNew("sd", this.AssertEvent);
                env.AssertListenerInvoked("sa");
                env.AssertListenerInvoked("sb");
                env.AssertListenerInvoked("sc");

                env.UndeployModuleContaining("sa");
                env.UndeployModuleContaining("sb");
                env.UndeployModuleContaining("sc");

                env.Milestone(0);

                env.AssertIterator("sd", it => AssertEvent(it.Advance()));

                env.UndeployAll();
            }

            private void AssertEvent(EventBean @event)
            {
                var und = (JsonEventObjectBase)@event.Underlying;
                Assert.AreEqual(6, und.NativeCount);
                AssertByIndex(6, und.GetNativeValue, new object[] { 4d, "x", 3, "def", 2d, 1 });
                AssertByIndex(6, und.GetNativeKeyName, new object[] { "a1", "b1", "b2", "c1", "d1", "d2" });
                AssertByIndex(
                    6,
                    und.GetNativeEntry,
                    new[] {
                        ToEntry("a1", 4d),
                        ToEntry("b1", "x"),
                        ToEntry("b2", 3),
                        ToEntry("c1", "def"),
                        ToEntry("d1", 2d),
                        ToEntry("d2", 1)
                    });
                AssertByName(und, "a1,b1,b2,c1,d1,d2");
                Assert.IsTrue(und.JsonValues.IsEmpty());

                IDictionary<string, object> compared = new LinkedHashMap<string, object>();
                compared.Put("a1", 4d);
                compared.Put("b1", "x");
                compared.Put("b2", 3);
                compared.Put("c1", "def");
                compared.Put("d1", 2d);
                compared.Put("d2", 1);
                SupportJsonEventTypeUtil.CompareDictionaries(compared, und);

                Assert.AreEqual(
                    "{\"a1\":4.0,\"b1\":\"x\",\"b2\":3,\"c1\":\"def\",\"d1\":2.0,\"d2\":1}",
                    und.ToString());
            }
        }

        private class EventJsonInheritsTwoLevel : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema ParentJson(p1 string, p2 int);\n" +
                          "@public @buseventtype create json schema ChildJson(c1 string, c2 int) inherits ParentJson;\n" +
                          "@name('s0') select * from ChildJson#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventJson("{\"p1\": \"abc\", \"p2\": 10, \"c1\": \"def\", \"c2\": 20}", "ChildJson");
                env.AssertEventNew("s0", this.AssertEvent);

                env.Milestone(0);

                env.AssertIterator("s0", iterator => AssertEvent(iterator.Advance()));

                env.UndeployAll();
            }

            private void AssertEvent(EventBean @event)
            {
                var und = (JsonEventObjectBase)@event.Underlying;

                AssertByIndex(4, und.GetNativeValue, new object[] { "abc", 10, "def", 20 });
                AssertByIndex(4, und.GetNativeKeyName, new object[] { "p1", "p2", "c1", "c2" });
                AssertByIndex(
                    4,
                    und.GetNativeEntry,
                    new[] {
                        ToEntry("p1", "abc"),
                        ToEntry("p2", 10),
                        ToEntry("c1", "def"),
                        ToEntry("c2", 20)
                    });
                Assert.That(und.NativeCount, Is.EqualTo(4));
                AssertByName(und, "p1,p2,c1,c2");
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

        private static KeyValuePair<string, object> ToEntry(
            string name,
            object value)
        {
            return new KeyValuePair<string, object>(name, value);
        }

        private static void AssertByName(
            JsonEventObjectBase und,
            string csv)
        {
            var split = csv.SplitCsv();
            for (var i = 0; i < split.Length; i++) {
                Assert.IsTrue(und.ContainsKey(split[i]));
                Assert.That(und.TryGetNativeKey(split[i], out var name), Is.True);
                Assert.That(name, Is.EqualTo(split[i]));
            }
        }

        private static void AssertByIndex<T>(
            int numFields,
            Func<int, T> indexFunction,
            T[] expected)
        {
            var actual = new T[numFields];
            for (var i = 0; i < numFields; i++) {
                actual[i] = indexFunction.Invoke(i);
            }

            EPAssertionUtil.AssertEqualsExactOrder(expected, actual);
        }

        private static void AssertNoSuchElement(Runnable runnable)
        {
            try {
                runnable.Invoke();
                Assert.Fail();
            }
            catch (NoSuchElementException) {
                // expected
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

            env.AssertIterator(
                "s0",
                itS0 => {
                    Assert.AreEqual(10, itS0.Advance().Get("c0"));
                    Assert.AreEqual("abc", itS0.Advance().Get("c0"));
                });

            env.AssertIterator(
                "s1",
                itS1 => {
                    AssertEventJson(itS1.Advance(), jsonOne, expectedOne);
                    AssertEventJson(itS1.Advance(), jsonTwo, expectedTwo);
                });
        }

        private static void SendAssertDynamicProp(
            RegressionEnvironment env,
            string json,
            string eventTypeName,
            object expected)
        {
            env.SendEventJson(json, eventTypeName);
            env.AssertEqualsNew("s0", "c0", expected);
            env.AssertEventNew("s1", @event => AssertEventJson(@event, json, expected));
        }

        private static void AssertEventJson(
            EventBean eventBean,
            string json,
            object expected)
        {
            var @event = (JsonEventObjectBase)eventBean.Underlying;
            SupportJsonEventTypeUtil.CompareDictionaries(Collections.SingletonMap("value", expected), @event);
            Assert.AreEqual(json, @event.ToString());
        }
    }
} // end of namespace