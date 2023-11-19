///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil; // AssertMessage

namespace com.espertech.esper.regressionlib.suite.@event.json
{
    public class EventJsonTypingClassParseWrite
    {
        public static readonly JValue NULL_VALUE = new JValue((object)null);

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithClassSimple(execs);
            WithListBuiltinType(execs);
            WithListEnumType(execs);
            WithVMClass(execs);
            WithClassWArrayAndColl(execs);
            WithNestedRecursive(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNestedRecursive(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingNestedRecursive());
            return execs;
        }

        public static IList<RegressionExecution> WithClassWArrayAndColl(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingClassWArrayAndColl());
            return execs;
        }

        public static IList<RegressionExecution> WithVMClass(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingVMClass());
            return execs;
        }

        public static IList<RegressionExecution> WithListEnumType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingListEnumType());
            return execs;
        }

        public static IList<RegressionExecution> WithListBuiltinType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingListBuiltinType());
            return execs;
        }

        public static IList<RegressionExecution> WithClassSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingClassSimple());
            return execs;
        }

        internal class EventJsonTypingNestedRecursive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent(local " +
                          typeof(MyLocalEventNestedRecursive).FullName +
                          ");\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var depthTwo = new JObject(
                    new JProperty("local", MakeNested("a,b")));
                env.SendEventJson(depthTwo.ToString(), "JsonEvent");
                env.AssertEventNew("s0", @event => AssertDepthTwo(env, @event, depthTwo));

                var depthThree = new JObject(
                    new JProperty("local", MakeNested("a,b,c")));
                env.SendEventJson(depthThree.ToString(), "JsonEvent");
                env.AssertEventNew("s0", @event => AssertDepthThree(env, @event, depthThree));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertDepthTwo(env, it.Advance(), depthTwo);
                        AssertDepthThree(env, it.Advance(), depthThree);
                    });
                env.UndeployAll();
            }

            private void AssertDepthTwo(
                RegressionEnvironment env,
                EventBean @event,
                JObject json)
            {
                var result = (MyLocalEventNestedRecursive)@event.Get("local");
                Assert.AreEqual("a", result.id);
                Assert.AreEqual("b", result.child.id);
                var rendered = env.Runtime.RenderEventService.GetJSONRenderer(@event.EventType).Render(@event);
                Assert.AreEqual(json.ToString(), rendered);
            }

            private void AssertDepthThree(
                RegressionEnvironment env,
                EventBean @event,
                JObject json)
            {
                var result = (MyLocalEventNestedRecursive)@event.Get("local");
                Assert.AreEqual("a", result.id);
                Assert.AreEqual("b", result.child.id);
                Assert.AreEqual("c", result.child.child.id);
                var rendered = env.Runtime.RenderEventService.GetJSONRenderer(@event.EventType).Render(@event);
                Assert.AreEqual(json.ToString(), rendered);
            }

            private JObject MakeNested(string csv)
            {
                var split = csv.Split(",");
                if (split.Length == 0) {
                    return new JObject();
                }

                var parent = new JObject(new JProperty("Id", split[0]));
                var current = parent;
                for (var i = 1; i < split.Length; i++) {
                    var child = new JObject(new JProperty("Id", split[i]));
                    current.Add("child", child);
                    current = child;
                }

                current.Add("child", NULL_VALUE);
                return parent;
            }
        }

        internal class EventJsonTypingClassWArrayAndColl : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent(local " +
                          typeof(MyLocalEventWArrayColl).FullName +
                          ");\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var localFilled = new JObject(
                    new JProperty("c0", MakeJson("E1", 1)),
                    new JProperty("c0Arr", new JArray(MakeJson("E2", 2))),
                    new JProperty("c0Arr2Dim", new JArray(new JArray(MakeJson("E3", 3)))),
                    new JProperty("c0Coll", new JArray(MakeJson("E4", 4))));
                var jsonFilled = new JObject(
                    new JProperty("local", localFilled)
                );

                env.SendEventJson(jsonFilled.ToString(), "JsonEvent");
                env.AssertEventNew("s0", @event => AssertFilled(env, @event, jsonFilled));

                var localNull = new JObject(
                    new JProperty("c0", NULL_VALUE),
                    new JProperty("c0Arr", new JArray(NULL_VALUE)),
                    new JProperty("c0Arr2Dim", new JArray(new JArray(NULL_VALUE))),
                    new JProperty("c0Coll", new JArray(NULL_VALUE)));
                var jsonNulled = new JObject(
                    new JProperty("local", localNull)
                );

                env.SendEventJson(jsonNulled.ToString(), "JsonEvent");
                env.AssertEventNew("s0", @event => AssertNulled(env, @event, jsonNulled));

                var localHalfFilled = new JObject(
                    new JProperty("c0", MakeJson("E1", 1)),
                    new JProperty(
                        "c0Arr",
                        new JArray(
                            NULL_VALUE,
                            MakeJson("E2", 2)
                        )),
                    new JProperty(
                        "c0Arr2Dim",
                        new JArray(
                            NULL_VALUE,
                            new JArray(
                                MakeJson("E3", 3),
                                NULL_VALUE),
                            NULL_VALUE
                        )),
                    new JProperty(
                        "c0Coll",
                        new JArray(
                            MakeJson("E4", 4),
                            NULL_VALUE)
                    ));

                var jsonHalfFilled = new JObject(
                    new JProperty("local", localHalfFilled));
                env.SendEventJson(jsonHalfFilled.ToString(), "JsonEvent");
                env.AssertEventNew("s0", @event => AssertHalfFilled(env, @event, jsonHalfFilled));

                var localEmpty = new JObject(
                    new JProperty("c0", NULL_VALUE),
                    new JProperty("c0Arr", new JArray()),
                    new JProperty("c0Arr2Dim", new JArray()),
                    new JProperty("c0Coll", new JArray()));
                var jsonEmpty = new JObject(
                    new JProperty("local", localEmpty));
                env.SendEventJson(jsonEmpty.ToString(), "JsonEvent");
                env.AssertEventNew("s0", @event => AssertEmpty(env, @event, jsonEmpty));

                var localFilledMultiple = new JObject(
                    new JProperty("c0", MakeJson("E1", 1)),
                    new JProperty(
                        "c0Arr",
                        new JArray(
                            MakeJson("E2", 10),
                            MakeJson("E2", 11),
                            MakeJson("E2", 12))),
                    new JProperty(
                        "c0Arr2Dim",
                        new JArray(
                            new JArray(
                                MakeJson("E3", 30),
                                MakeJson("E3", 31)),
                            new JArray(
                                MakeJson("E3", 32),
                                MakeJson("E3", 33))
                        )),
                    new JProperty(
                        "c0Coll",
                        new JArray(
                            MakeJson("E4", 40),
                            MakeJson("E4", 41)
                        )));
                var jsonFilledMultiple = new JObject(
                    new JProperty("local", localFilledMultiple));

                env.SendEventJson(jsonFilledMultiple.ToString(), "JsonEvent");
                env.AssertEventNew("s0", @event => AssertFilledMultiple(env, @event, jsonFilledMultiple));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertFilled(env, it.Advance(), jsonFilled);
                        AssertNulled(env, it.Advance(), jsonNulled);
                        AssertHalfFilled(env, it.Advance(), jsonHalfFilled);
                        AssertEmpty(env, it.Advance(), jsonEmpty);
                        AssertFilledMultiple(env, it.Advance(), jsonFilledMultiple);
                    });

                env.UndeployAll();
            }

            private void AssertEmpty(
                RegressionEnvironment env,
                EventBean @event,
                JObject json)
            {
                AssertEvent(
                    env,
                    @event,
                    json,
                    null,
                    Array.Empty<object>(),
                    Array.Empty<object[]>(),
                    Array.Empty<object>());
            }

            private void AssertNulled(
                RegressionEnvironment env,
                EventBean @event,
                JObject json)
            {
                AssertEvent(
                    env,
                    @event,
                    json,
                    null,
                    new object[] { null },
                    new object[][] { new object[] { null } },
                    new object[] { null });
            }

            private void AssertFilled(
                RegressionEnvironment env,
                EventBean @event,
                JObject json)
            {
                AssertEvent(
                    env,
                    @event,
                    json,
                    MakeLocal("E1", 1),
                    new object[] { MakeLocal("E2", 2) },
                    new object[][] { new object[] { MakeLocal("E3", 3) } },
                    new object[] { MakeLocal("E4", 4) });
            }

            private void AssertFilledMultiple(
                RegressionEnvironment env,
                EventBean @event,
                JObject json)
            {
                AssertEvent(
                    env,
                    @event,
                    json,
                    MakeLocal("E1", 1),
                    new object[] { MakeLocal("E2", 10), MakeLocal("E2", 11), MakeLocal("E2", 12) },
                    new object[][] {
                        new object[] { MakeLocal("E3", 30), MakeLocal("E3", 31) },
                        new object[] { MakeLocal("E3", 32), MakeLocal("E3", 33) }
                    },
                    new object[] { MakeLocal("E4", 40), MakeLocal("E4", 41) });
            }

            private void AssertHalfFilled(
                RegressionEnvironment env,
                EventBean @event,
                JObject json)
            {
                AssertEvent(
                    env,
                    @event,
                    json,
                    MakeLocal("E1", 1),
                    new object[] { null, MakeLocal("E2", 2) },
                    new object[][] { null, new object[] { MakeLocal("E3", 3), null }, null },
                    new object[] { MakeLocal("E4", 4), null });
            }

            private void AssertEvent(
                RegressionEnvironment env,
                EventBean @event,
                JObject json,
                object c0,
                object[] c0Arr,
                object[][] c0Arr2Dim,
                object[] c0Coll)
            {
                var result = (MyLocalEventWArrayColl)@event.Get("local");
                Assert.AreEqual(c0, result.c0);
                EPAssertionUtil.AssertEqualsExactOrder(c0Arr, result.c0Arr);
                EPAssertionUtil.AssertEqualsExactOrder(c0Arr2Dim, result.c0Arr2Dim);
                EPAssertionUtil.AssertEqualsExactOrder(c0Coll, result.c0Coll.ToArray());
                var rendered = env.Runtime.RenderEventService.GetJSONRenderer(@event.EventType).Render(@event);
                Assert.AreEqual(json.ToString(), rendered);
            }

            private JObject MakeJson(
                string theString,
                int intPrimitive)
            {
                return new JObject(
                    new JProperty("TheString", theString),
                    new JProperty("IntPrimitive", intPrimitive));
            }

            private MyLocalEvent MakeLocal(
                string theString,
                int intPrimitive)
            {
                return new MyLocalEvent(theString, intPrimitive);
            }
        }

        internal class EventJsonTypingVMClass : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var valueOfOne = new JValue(1);

                var uuid = Guid.Parse("b7dc7f66-4f6d-4f03-14d7-83da210dfba6");
                RunAssertion(
                    env,
                    typeof(MyLocalVMTypeUUID),
                    typeof(Guid),
                    uuid.ToString(),
                    uuid,
                    valueOfOne,
                    milestone);

                var dateTimeEx = DateTimeEx.NowUtc();
                var dateTimeOffset = dateTimeEx.DateTime;
                var dateTime = dateTimeOffset.DateTime;

                RunAssertion(
                    env,
                    typeof(MyLocalVMTypeDateTimeEx),
                    typeof(DateTimeEx),
                    dateTimeEx.ToString(),
                    dateTimeEx,
                    valueOfOne,
                    milestone);
                RunAssertion(
                    env,
                    typeof(MyLocalVMTypeDateTimeOffset),
                    typeof(DateTimeOffset),
                    dateTimeOffset.ToString(),
                    dateTimeOffset,
                    valueOfOne,
                    milestone);
                RunAssertion(
                    env,
                    typeof(MyLocalVMTypeDateTime),
                    typeof(DateTime),
                    dateTime.ToString(),
                    dateTime,
                    valueOfOne,
                    milestone);

                var uri = new Uri("ftp://ftp.is.co.za/rfc/rfc1808.txt");
                Assert.AreEqual(uri, new Uri(uri.ToString()));

                RunAssertion(
                    env,
                    typeof(MyLocalVMTypeURI),
                    typeof(Uri),
                    uri.ToString(),
                    uri,
                    new JValue("a b"),
                    milestone);
            }

            private void RunAssertion(
                RegressionEnvironment env,
                Type localType,
                Type fieldType,
                string jsonText,
                object expected,
                JValue invalidJson,
                AtomicLong milestone)
            {
                var epl = "@public @buseventtype create json schema JsonEvent(local " +
                          localType.FullName +
                          ");\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var localFilled = new JObject(
                    new JProperty("c0", jsonText),
                    new JProperty("c0Arr", new JArray(jsonText)),
                    new JProperty("c0Arr2Dim", new JArray(new JArray(jsonText))),
                    new JProperty("c0Coll", new JArray(jsonText)));
                var jsonFilledObject = new JObject(
                    new JProperty("local", localFilled));
                var jsonFilled = jsonFilledObject.ToString();
                env.SendEventJson(jsonFilled, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertEvent(env, @event, jsonFilled, expected));

                var localNull = new JObject(
                    new JProperty("c0", NULL_VALUE),
                    new JProperty("c0Arr", new JArray(NULL_VALUE)),
                    new JProperty("c0Arr2Dim", new JArray(new JArray(NULL_VALUE))),
                    new JProperty("c0Coll", new JArray(NULL_VALUE)));
                var jsonNullObject = new JObject(
                    new JProperty("local", localNull));
                var jsonNull = jsonNullObject.ToString();
                env.SendEventJson(jsonNull, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertEvent(env, @event, jsonNull, null));

                env.AssertThat(
                    () => {
                        try {
                            var localInvalid = new JObject(
                                new JProperty("c0", invalidJson));
                            var jsonInvalidObject = new JObject(
                                new JProperty("local", localInvalid));
                            env.SendEventJson(jsonInvalidObject.ToString(), "JsonEvent");
                            Assert.Fail();
                        }
                        catch (EPException ex) {
                            var value = invalidJson.ToString();
                            AssertMessage(
                                ex,
                                "Failed to parse json member name 'c0' as a " +
                                fieldType.Name +
                                "-type from value '" +
                                value +
                                "'");
                        }
                    });

                env.MilestoneInc(milestone);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertEvent(env, it.Advance(), jsonFilled, expected);
                        AssertEvent(env, it.Advance(), jsonNull, null);
                    });

                env.UndeployAll();
            }

            private void AssertEvent(
                RegressionEnvironment env,
                EventBean @event,
                string json,
                object expected)
            {
                var result = (MyLocalVMType)@event.Get("local");
                Assert.AreEqual(expected, result.C0);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { expected }, result.C0Array);
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[][] { new object[] { expected } },
                    result.C0Array2Dim);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { expected }, result.C0Collection);
                var rendered = env.Runtime.RenderEventService.GetJSONRenderer(@event.EventType).Render(@event);
                Assert.AreEqual(json, rendered);
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        internal class EventJsonTypingListEnumType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent(local " +
                          typeof(MyLocalEventCollectionEnumType).FullName +
                          ");\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";

                env.CompileDeploy(epl).AddListener("s0");

                var jsonFilled = "{ \"local\" : { \"c0\": [\"ENUM_VALUE_1\", \"ENUM_VALUE_2\"] } }\n";
                env.SendEventJson(jsonFilled, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertFilled(env, @event, jsonFilled));

                var jsonUnfilled = "{ \"local\" : {}}";
                env.SendEventJson(jsonUnfilled, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertUnfilled(env, @event));

                var jsonEmpty = "{ \"local\" : { \"c0\": []}}\n";
                env.SendEventJson(jsonEmpty, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertEmpty(env, @event, jsonEmpty));

                var jsonNull = "{ \"local\" : { \"c0\": null}}\n";
                env.SendEventJson(jsonNull, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertUnfilled(env, @event));

                var jsonPartiallyFilled = "{ \"local\" : { \"c0\": [\"ENUM_VALUE_3\", null] }}\n";
                env.SendEventJson(jsonPartiallyFilled, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertPartiallyFilled(env, @event, jsonPartiallyFilled));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertFilled(env, it.Advance(), jsonFilled);
                        AssertUnfilled(env, it.Advance());
                        AssertEmpty(env, it.Advance(), jsonEmpty);
                        AssertUnfilled(env, it.Advance());
                        AssertPartiallyFilled(env, it.Advance(), jsonPartiallyFilled);
                    });

                env.UndeployAll();
            }

            private void AssertFilled(
                RegressionEnvironment env,
                EventBean @event,
                string json)
            {
                AssertCollection(@event, local => local.c0, SupportEnum.ENUM_VALUE_1, SupportEnum.ENUM_VALUE_2);
                AssertJson(env, @event, json);
            }

            private void AssertPartiallyFilled(
                RegressionEnvironment env,
                EventBean @event,
                string json)
            {
                AssertCollection(@event, local => local.c0, SupportEnum.ENUM_VALUE_3, null);
                AssertJson(env, @event, json);
            }

            private void AssertEmpty(
                RegressionEnvironment env,
                EventBean @event,
                string json)
            {
                AssertCollection(@event, local => local.c0);
                AssertJson(env, @event, json);
            }

            private void AssertUnfilled(
                RegressionEnvironment env,
                EventBean @event)
            {
                Assert.IsNull(CollectionValue(@event, local => local.c0));
                AssertJson(env, @event, "{\"local\":{\"c0\":null}}");
            }

            private static void AssertCollection<T>(
                EventBean @event,
                Func<MyLocalEventCollectionEnumType, ICollection<T>> function,
                params object[] values)
            {
                EPAssertionUtil.AssertEqualsExactOrder(
                    values,
                    CollectionValue(@event, function).UnwrapIntoArray<object>());
            }

            private static ICollection<T> CollectionValue<T>(
                EventBean @event,
                Func<MyLocalEventCollectionEnumType, ICollection<T>> function)
            {
                var bt = (MyLocalEventCollectionEnumType)@event.Get("local");
                return function.Invoke(bt);
            }
        }

        internal class EventJsonTypingListBuiltinType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent(local " +
                          typeof(MyLocalEventCollectionBuiltinType).FullName +
                          ");\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";

                env.CompileDeploy(epl).AddListener("s0");

                var jsonFilled = "{ \"local\" : { " +
                                 "\"c0\": [\"abc\", \"def\"],\n" +
                                 "\"c1\": [\"x\", \"y\"],\n" +
                                 "\"c2\": [true, false],\n" +
                                 "\"c3\": [10, 11],\n" +
                                 "\"c4\": [20, 21],\n" +
                                 "\"c5\": [30, 31],\n" +
                                 "\"c6\": [40, 41],\n" +
                                 "\"c7\": [50.0, 51.0],\n" +
                                 "\"c8\": [60.0, 61.0],\n" +
                                 "\"c9\": [70, 71],\n" +
                                 "\"c10\": [80, 81]\n" +
                                 "}}\n";
                env.SendEventJson(jsonFilled, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertFilled(env, @event, jsonFilled));

                var jsonUnfilled = "{ \"local\" : {}}";
                env.SendEventJson(jsonUnfilled, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertUnfilled(env, @event));

                var jsonEmpty = "{ \"local\" : { " +
                                "\"c0\": [],\n" +
                                "\"c1\": [],\n" +
                                "\"c2\": [],\n" +
                                "\"c3\": [],\n" +
                                "\"c4\": [],\n" +
                                "\"c5\": [],\n" +
                                "\"c6\": [],\n" +
                                "\"c7\": [],\n" +
                                "\"c8\": [],\n" +
                                "\"c9\": [],\n" +
                                "\"c10\": []\n" +
                                "}}\n";
                env.SendEventJson(jsonEmpty, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertEmpty(env, @event, jsonEmpty));

                var jsonNull = "{ \"local\" : { " +
                               "\"c0\": null,\n" +
                               "\"c1\": null,\n" +
                               "\"c2\": null,\n" +
                               "\"c3\": null,\n" +
                               "\"c4\": null,\n" +
                               "\"c5\": null,\n" +
                               "\"c6\": null,\n" +
                               "\"c7\": null,\n" +
                               "\"c8\": null,\n" +
                               "\"c9\": null,\n" +
                               "\"c10\": null\n" +
                               "}}\n";
                env.SendEventJson(jsonNull, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertUnfilled(env, @event));

                var jsonPartiallyFilled = "{ \"local\" : { " +
                                          "\"c0\": [\"abc\", null],\n" +
                                          "\"c1\": [\"x\", null],\n" +
                                          "\"c2\": [true, null],\n" +
                                          "\"c3\": [10, null],\n" +
                                          "\"c4\": [20, null],\n" +
                                          "\"c5\": [30, null],\n" +
                                          "\"c6\": [40, null],\n" +
                                          "\"c7\": [50.0, null],\n" +
                                          "\"c8\": [60.0, null],\n" +
                                          "\"c9\": [70, null],\n" +
                                          "\"c10\": [80, null]\n" +
                                          "}}\n";
                env.SendEventJson(jsonPartiallyFilled, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertPartiallyFilled(env, @event, jsonPartiallyFilled));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertFilled(env, it.Advance(), jsonFilled);
                        AssertUnfilled(env, it.Advance());
                        AssertEmpty(env, it.Advance(), jsonEmpty);
                        AssertUnfilled(env, it.Advance());
                        AssertPartiallyFilled(env, it.Advance(), jsonPartiallyFilled);
                    });

                env.UndeployAll();
            }

            private void AssertFilled(
                RegressionEnvironment env,
                EventBean @event,
                string json)
            {
                AssertCollection(@event, local => local.c0, "abc", "def");
                AssertCollection(@event, local => local.c1, 'x', 'y');
                AssertCollection(@event, local => local.c2, true, false);
                AssertCollection(@event, local => local.c3, (byte)10, (byte)11);
                AssertCollection(@event, local => local.c4, (short)20, (short)21);
                AssertCollection(@event, local => local.c5, 30, 31);
                AssertCollection(@event, local => local.c6, 40L, 41L);
                AssertCollection(@event, local => local.c7, 50d, 51d);
                AssertCollection(@event, local => local.c8, 60f, 61f);
                AssertCollection(@event, local => local.c9, new BigInteger(70L), new BigInteger(71L));
                AssertCollection(@event, local => local.c10, 80m, 81m);
                AssertJson(env, @event, json);
            }

            private void AssertPartiallyFilled(
                RegressionEnvironment env,
                EventBean @event,
                string json)
            {
                AssertCollection(@event, local => local.c0, "abc", null);
                AssertCollection(@event, local => local.c1, 'x', null);
                AssertCollection(@event, local => local.c2, true, null);
                AssertCollection(@event, local => local.c3, (byte)10, null);
                AssertCollection(@event, local => local.c4, (short)20, null);
                AssertCollection(@event, local => local.c5, 30, null);
                AssertCollection(@event, local => local.c6, 40L, null);
                AssertCollection(@event, local => local.c7, 50d, null);
                AssertCollection(@event, local => local.c8, 60f, null);
                AssertCollection(@event, local => local.c9, new BigInteger(70L), null);
                AssertCollection(@event, local => local.c10, 80m, null);
                AssertJson(env, @event, json);
            }

            private void AssertEmpty(
                RegressionEnvironment env,
                EventBean @event,
                string json)
            {
                AssertCollection(@event, local => local.c0);
                AssertCollection(@event, local => local.c1);
                AssertCollection(@event, local => local.c2);
                AssertCollection(@event, local => local.c3);
                AssertCollection(@event, local => local.c4);
                AssertCollection(@event, local => local.c5);
                AssertCollection(@event, local => local.c6);
                AssertCollection(@event, local => local.c7);
                AssertCollection(@event, local => local.c8);
                AssertCollection(@event, local => local.c9);
                AssertCollection(@event, local => local.c10);
                AssertJson(env, @event, json);
            }

            private void AssertUnfilled(
                RegressionEnvironment env,
                EventBean @event)
            {
                Assert.IsNull(CollectionValue(@event, local => local.c0));
                Assert.IsNull(CollectionValue(@event, local => local.c1));
                Assert.IsNull(CollectionValue(@event, local => local.c2));
                Assert.IsNull(CollectionValue(@event, local => local.c3));
                Assert.IsNull(CollectionValue(@event, local => local.c4));
                Assert.IsNull(CollectionValue(@event, local => local.c5));
                Assert.IsNull(CollectionValue(@event, local => local.c6));
                Assert.IsNull(CollectionValue(@event, local => local.c7));
                Assert.IsNull(CollectionValue(@event, local => local.c8));
                Assert.IsNull(CollectionValue(@event, local => local.c9));
                Assert.IsNull(CollectionValue(@event, local => local.c10));
                AssertJson(
                    env,
                    @event,
                    "{\"local\":{\"c0\":null,\"c1\":null,\"c2\":null,\"c3\":null,\"c4\":null,\"c5\":null,\"c6\":null,\"c7\":null,\"c8\":null,\"c9\":null,\"c10\":null}}");
            }

            private static void AssertCollection<T>(
                EventBean @event,
                Func<MyLocalEventCollectionBuiltinType, ICollection<T>> function,
                params object[] values)
            {
                EPAssertionUtil.AssertEqualsExactOrder(
                    values,
                    CollectionValue<T>(@event, function).UnwrapIntoArray<object>());
            }

            private static ICollection<T> CollectionValue<T>(
                EventBean @event,
                Func<MyLocalEventCollectionBuiltinType, ICollection<T>> function)
            {
                var bt = (MyLocalEventCollectionBuiltinType)@event.Get("local");
                return function.Invoke(bt);
            }
        }

        internal class EventJsonTypingClassSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent(local " +
                          typeof(MyLocalEvent).FullName +
                          ");\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var json = "{\n" +
                           "  \"local\": {\n" +
                           "    \"TheString\": \"abc\",\n" +
                           "    \"IntPrimitive\" : 10\n" +
                           "  }\n" +
                           "}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertPropsNew("s0", "local".Split(","), new object[] { new MyLocalEvent("abc", 10) });

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    iterator => EPAssertionUtil.AssertProps(
                        iterator.Advance(),
                        "local".Split(","),
                        new object[] { new MyLocalEvent("abc", 10) }));

                env.UndeployAll();
            }
        }

        private static void AssertJson(
            RegressionEnvironment env,
            EventBean @event,
            string json)
        {
            var rendered = env.Runtime.RenderEventService.GetJSONRenderer(@event.EventType).Render(@event);
            Assert.AreEqual(
                json
                    .RegexReplaceAll(" ", "")
                    .RegexReplaceAll("\n", ""),
                rendered);
        }

        public class MyLocalEvent
        {
            public string theString;
            public int intPrimitive;

            public MyLocalEvent()
            {
            }

            public MyLocalEvent(
                string theString,
                int intPrimitive)
            {
                this.theString = theString;
                this.intPrimitive = intPrimitive;
            }

            protected bool Equals(MyLocalEvent other)
            {
                return theString == other.theString && intPrimitive == other.intPrimitive;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }

                if (ReferenceEquals(this, obj)) {
                    return true;
                }

                if (obj.GetType() != GetType()) {
                    return false;
                }

                return Equals((MyLocalEvent)obj);
            }

            public override int GetHashCode()
            {
                unchecked {
                    return ((theString != null ? theString.GetHashCode() : 0) * 397) ^ intPrimitive;
                }
            }
        }

        public class MyLocalEventCollectionBuiltinType
        {
            public IList<string> c0;
            public IList<char> c1;
            public IList<bool> c2;
            public IList<byte> c3;
            public IList<short> c4;
            public IList<int> c5;
            public IList<long> c6;
            public IList<double> c7;
            public IList<float> c8;
            public IList<BigInteger> c9;
            public IList<decimal> c10;
        }

        public class MyLocalEventCollectionEnumType
        {
            public IList<SupportEnum> c0;
        }

        public interface MyLocalVMType
        {
            object C0 { get; }

            object[] C0Array { get; }

            object[][] C0Array2Dim { get; }

            ICollection<object> C0Collection { get; }
        }

        public class MyLocalVMTypeUUID : MyLocalVMType
        {
            public Guid c0;
            public Guid[] c0Arr;
            public Guid[][] c0Arr2Dim;
            public IList<Guid> c0Coll;

            public object C0 => c0;

            public object[] C0Array => c0Arr.UnwrapIntoArray<object>();

            public object[][] C0Array2Dim => c0Arr2Dim
                .Select(array => array.Cast<object>().ToArray())
                .ToArray();

            public ICollection<object> C0Collection => c0Coll.Unwrap<object>();
        }


        public class MyLocalVMTypeDateTimeEx : MyLocalVMType
        {
            public DateTimeEx c0;
            public DateTimeEx[] c0Arr;
            public DateTimeEx[][] c0Arr2Dim;
            public IList<DateTimeEx> c0Coll;

            public object C0 => c0;

            public object[] C0Array => c0Arr.UnwrapIntoArray<object>();

            public object[][] C0Array2Dim => c0Arr2Dim;

            public ICollection<object> C0Collection => c0Coll.Unwrap<object>();
        }

        public class MyLocalVMTypeDateTimeOffset : MyLocalVMType
        {
            public DateTimeOffset c0;
            public DateTimeOffset[] c0Arr;
            public DateTimeOffset[][] c0Arr2Dim;
            public IList<DateTimeOffset> c0Coll;

            public object C0 => c0;

            public object[] C0Array => c0Arr.UnwrapIntoArray<object>();

            public object[][] C0Array2Dim => c0Arr2Dim
                .Select(array => array.Cast<object>().ToArray())
                .ToArray();

            public ICollection<object> C0Collection => c0Coll.Unwrap<object>();
        }

        public class MyLocalVMTypeDateTime : MyLocalVMType
        {
            public DateTime c0;
            public DateTime[] c0Arr;
            public DateTime[][] c0Arr2Dim;
            public IList<DateTime> c0Coll;

            public object C0 => c0;

            public object[] C0Array => c0Arr.UnwrapIntoArray<object>();

            public object[][] C0Array2Dim => c0Arr2Dim
                .Select(array => array.Cast<object>().ToArray())
                .ToArray();

            public ICollection<object> C0Collection => c0Coll.Unwrap<object>();
        }

        public class MyLocalVMTypeURI : MyLocalVMType
        {
            public Uri c0;
            public Uri[] c0Arr;
            public Uri[][] c0Arr2Dim;
            public IList<Uri> c0Coll;

            public object C0 => c0;

            public object[] C0Array => c0Arr.UnwrapIntoArray<object>();

            public object[][] C0Array2Dim => c0Arr2Dim;

            public ICollection<object> C0Collection => c0Coll.Unwrap<object>();
        }

        public class MyLocalEventWArrayColl
        {
            public MyLocalEvent c0;
            public MyLocalEvent[] c0Arr;
            public MyLocalEvent[][] c0Arr2Dim;
            public IList<MyLocalEvent> c0Coll;

            public object C0 => c0;

            public object[] C0Array => c0Arr.UnwrapIntoArray<object>();

            public object[][] C0Array2Dim => c0Arr2Dim;

            public ICollection<object> C0Collection => c0Coll.Unwrap<object>();
        }

        public class MyLocalEventNestedRecursive
        {
            public string id;
            public MyLocalEventNestedRecursive child;
        }
    }
} // end of namespace