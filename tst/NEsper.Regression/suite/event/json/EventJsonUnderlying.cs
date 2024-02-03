///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.json;

//compareMaps

namespace com.espertech.esper.regressionlib.suite.@event.json
{
    public class EventJsonUnderlying
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithDynamicZeroDeclared(execs);
            WithDynamicOneDeclared(execs);
            WithDynamicTwoDeclared(execs);
            WithNonDynamicZeroDeclared(execs);
            WithNonDynamicOneDeclared(execs);
            WithNonDynamicTwoDeclared(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNonDynamicTwoDeclared(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonUnderlyingMapNonDynamicTwoDeclared());
            return execs;
        }

        public static IList<RegressionExecution> WithNonDynamicOneDeclared(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonUnderlyingMapNonDynamicOneDeclared());
            return execs;
        }

        public static IList<RegressionExecution> WithNonDynamicZeroDeclared(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonUnderlyingMapNonDynamicZeroDeclared());
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicTwoDeclared(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonUnderlyingMapDynamicTwoDeclared());
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicOneDeclared(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonUnderlyingMapDynamicOneDeclared());
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicZeroDeclared(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonUnderlyingMapDynamicZeroDeclared());
            return execs;
        }

        private class EventJsonUnderlyingMapNonDynamicZeroDeclared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@public @buseventtype create json schema JsonEvent();\n" +
                        "@name('s0') select *  from JsonEvent#keepall")
                    .AddListener("s0");

                env.SendEventJson("{\"a\" : 1, \"b\": 2, \"c\": 3}\n", "JsonEvent");
                env.AssertEventNew("s0", @event => CompareMapWBean(new LinkedHashMap<string, object>(), @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    enumerator => CompareMapWBean(new LinkedHashMap<string, object>(), enumerator.Advance()));

                env.UndeployAll();
            }
        }

        private class EventJsonUnderlyingMapNonDynamicOneDeclared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@public @buseventtype create json schema JsonEvent(a int);\n" +
                        "@name('s0') select *  from JsonEvent#keepall")
                    .AddListener("s0");

                env.SendEventJson("{\"a\" : 1, \"b\": 2, \"c\": 3}\n", "JsonEvent");
                IDictionary<string, object> expectedOne = new LinkedHashMap<string, object>();
                expectedOne.Put("a", 1);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedOne, @event));

                env.SendEventJson("{\"a\" : 10}\n", "JsonEvent");
                IDictionary<string, object> expectedTwo = new LinkedHashMap<string, object>();
                expectedTwo.Put("a", 10);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedTwo, @event));

                env.SendEventJson("{}\n", "JsonEvent");
                IDictionary<string, object> expectedThree = new LinkedHashMap<string, object>();
                expectedThree.Put("a", null);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedThree, @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    en => {
                        CompareMapWBean(expectedOne, en.Advance());
                        CompareMapWBean(expectedTwo, en.Advance());
                        CompareMapWBean(expectedThree, en.Advance());
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonUnderlyingMapNonDynamicTwoDeclared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@public @buseventtype create json schema JsonEvent(a int, b int);\n" +
                        "@name('s0') select *  from JsonEvent#keepall")
                    .AddListener("s0");

                env.SendEventJson("{\"a\" : 1, \"b\": 2, \"c\": 3}\n", "JsonEvent");
                IDictionary<string, object> expectedOne = new LinkedHashMap<string, object>();
                expectedOne.Put("a", 1);
                expectedOne.Put("b", 2);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedOne, @event));

                env.SendEventJson("{\"a\" : 10}\n", "JsonEvent");
                IDictionary<string, object> expectedTwo = new LinkedHashMap<string, object>();
                expectedTwo.Put("a", 10);
                expectedTwo.Put("b", null);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedTwo, @event));

                env.SendEventJson("{}\n", "JsonEvent");
                IDictionary<string, object> expectedThree = new LinkedHashMap<string, object>();
                expectedThree.Put("a", null);
                expectedThree.Put("b", null);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedThree, @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        CompareMapWBean(expectedOne, it.Advance());
                        CompareMapWBean(expectedTwo, it.Advance());
                        CompareMapWBean(expectedThree, it.Advance());
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonUnderlyingMapDynamicZeroDeclared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent();\n" +
                        "@name('s0') select *  from JsonEvent#keepall")
                    .AddListener("s0");

                env.SendEventJson("{\"a\" : 1, \"b\": 2, \"c\": 3}\n", "JsonEvent");
                IDictionary<string, object> expectedOne = new LinkedHashMap<string, object>();
                expectedOne.Put("a", 1);
                expectedOne.Put("b", 2);
                expectedOne.Put("c", 3);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedOne, @event));

                env.SendEventJson("{\"a\" : 10}\n", "JsonEvent");
                IDictionary<string, object> expectedTwo = new LinkedHashMap<string, object>();
                expectedTwo.Put("a", 10);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedTwo, @event));

                env.SendEventJson("{\"a\" : null, \"c\": 101, \"d\": 102}\n", "JsonEvent");
                IDictionary<string, object> expectedThree = new LinkedHashMap<string, object>();
                expectedThree.Put("a", null);
                expectedThree.Put("c", 101);
                expectedThree.Put("d", 102);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedThree, @event));

                env.SendEventJson("{}\n", "JsonEvent");
                env.AssertEventNew("s0", @event => CompareMapWBean(new LinkedHashMap<string, object>(), @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        CompareMapWBean(expectedOne, it.Advance());
                        CompareMapWBean(expectedTwo, it.Advance());
                        CompareMapWBean(expectedThree, it.Advance());
                        CompareMapWBean(new LinkedHashMap<string, object>(), it.Advance());
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonUnderlyingMapDynamicOneDeclared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent(a int);\n" +
                        "@name('s0') select *  from JsonEvent#keepall")
                    .AddListener("s0");

                env.SendEventJson("{\"a\" : 1, \"b\": 2, \"c\": 3}\n", "JsonEvent");
                IDictionary<string, object> expectedOne = new LinkedHashMap<string, object>();
                expectedOne.Put("a", 1);
                expectedOne.Put("b", 2);
                expectedOne.Put("c", 3);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedOne, @event));

                env.SendEventJson("{\"a\" : 10}\n", "JsonEvent");
                IDictionary<string, object> expectedTwo = new LinkedHashMap<string, object>();
                expectedTwo.Put("a", 10);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedTwo, @event));

                env.SendEventJson("{\"a\" : null, \"c\": 101, \"d\": 102}\n", "JsonEvent");
                IDictionary<string, object> expectedThree = new LinkedHashMap<string, object>();
                expectedThree.Put("a", null);
                expectedThree.Put("c", 101);
                expectedThree.Put("d", 102);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedThree, @event));

                env.SendEventJson("{}\n", "JsonEvent");
                IDictionary<string, object> expectedFour = new LinkedHashMap<string, object>();
                expectedFour.Put("a", null);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedFour, @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        CompareMapWBean(expectedOne, it.Advance());
                        CompareMapWBean(expectedTwo, it.Advance());
                        CompareMapWBean(expectedThree, it.Advance());
                        CompareMapWBean(expectedFour, it.Advance());
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonUnderlyingMapDynamicTwoDeclared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent(a int, b int);\n" +
                        "@name('s0') select *  from JsonEvent#keepall")
                    .AddListener("s0");

                env.SendEventJson("{\"a\" : 1, \"b\": 2, \"c\": 3}\n", "JsonEvent");
                IDictionary<string, object> expectedOne = new LinkedHashMap<string, object>();
                expectedOne.Put("a", 1);
                expectedOne.Put("b", 2);
                expectedOne.Put("c", 3);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedOne, @event));

                env.SendEventJson("{\"a\" : 10}\n", "JsonEvent");
                IDictionary<string, object> expectedTwo = new LinkedHashMap<string, object>();
                expectedTwo.Put("a", 10);
                expectedTwo.Put("b", null);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedTwo, @event));

                env.SendEventJson("{\"a\" : null, \"c\": 101, \"d\": 102}\n", "JsonEvent");
                IDictionary<string, object> expectedThree = new LinkedHashMap<string, object>();
                expectedThree.Put("a", null);
                expectedThree.Put("b", null);
                expectedThree.Put("c", 101);
                expectedThree.Put("d", 102);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedThree, @event));

                env.SendEventJson("{}\n", "JsonEvent");
                IDictionary<string, object> expectedFour = new LinkedHashMap<string, object>();
                expectedFour.Put("a", null);
                expectedFour.Put("b", null);
                env.AssertEventNew("s0", @event => CompareMapWBean(expectedFour, @event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        CompareMapWBean(expectedOne, it.Advance());
                        CompareMapWBean(expectedTwo, it.Advance());
                        CompareMapWBean(expectedThree, it.Advance());
                        CompareMapWBean(expectedFour, it.Advance());
                    });

                env.UndeployAll();
            }
        }

        private static void CompareMapWBean(
            IDictionary<string, object> expected,
            EventBean @event)
        {
            SupportJsonEventTypeUtil.CompareDictionaries(expected, (IDictionary<string, object>)@event.Underlying);
        }
    }
} // end of namespace