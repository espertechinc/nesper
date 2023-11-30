///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.common.@internal.support.SupportEnum;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
    public class EventJsonTypingCoreParse
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithBasicType(execs);
            WithBasicTypeArray(execs);
            WithBasicTypeArray2Dim(execs);
            WithEnumType(execs);
            WithDecimalBigInt(execs);
            WithObjectType(execs);
            WithObjectArrayType(execs);
            WithMapType(execs);
            WithDynamicPropJsonTypes(execs);
            WithDynamicPropMixedOjectArray(execs);
            WithDynamicPropNestedArray(execs);
            WithDynamicPropNumberFormat(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicPropNumberFormat(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingParseDynamicPropNumberFormat());
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicPropNestedArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingParseDynamicPropNestedArray());
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicPropMixedOjectArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingParseDynamicPropMixedOjectArray());
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicPropJsonTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingParseDynamicPropJsonTypes());
            return execs;
        }

        public static IList<RegressionExecution> WithMapType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingParseMapType());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectArrayType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingParseObjectArrayType());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingParseObjectType());
            return execs;
        }

        public static IList<RegressionExecution> WithDecimalBigInt(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingParseDecimalBigInt());
            return execs;
        }

        public static IList<RegressionExecution> WithEnumType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingParseEnumType());
            return execs;
        }

        public static IList<RegressionExecution> WithBasicTypeArray2Dim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingParseBasicTypeArray2Dim());
            return execs;
        }

        public static IList<RegressionExecution> WithBasicTypeArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingParseBasicTypeArray());
            return execs;
        }

        public static IList<RegressionExecution> WithBasicType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventJsonTypingParseBasicType());
            return execs;
        }

        private class EventJsonTypingParseMapType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (c0 Map);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var namesAndTypes = new object[][] { new object[] { "c0", typeof(IDictionary<string, object>) } };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        namesAndTypes,
                        statement.EventType,
                        SupportEventTypeAssertionEnum.NAME,
                        SupportEventTypeAssertionEnum.TYPE));

                SendAssertColumn(env, "{}", null);
                SendAssertColumn(env, "{\"c0\": {\"c1\" : 10}}", Collections.SingletonMap("c1", 10));
                SendAssertColumn(
                    env,
                    "{\"c0\": {\"c1\": {\"c2\": 20}}}",
                    Collections.SingletonMap("c1", Collections.SingletonMap("c2", 20)));
                Consumer<object> assertionOne = result => {
                    var oa = result.AsStringDictionary().Get("c1").UnwrapIntoArray<object>();
                    EPAssertionUtil.AssertEqualsExactOrder(new object[] { "c2", 20 }, oa);
                };
                SendAssert(env, "{\"c0\": {\"c1\": [\"c2\", 20]}}", assertionOne);

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertColumn(it.Advance(), null);
                        AssertColumn(it.Advance(), Collections.SingletonMap("c1", 10));
                        AssertColumn(it.Advance(), Collections.SingletonMap("c1", Collections.SingletonMap("c2", 20)));
                        JustAssert(it.Advance(), assertionOne);
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingParseObjectArrayType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (c0 Object[]);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var namesAndTypes = new object[][] { new object[] { "c0", typeof(object[]) } };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        namesAndTypes,
                        statement.EventType,
                        SupportEventTypeAssertionEnum.NAME,
                        SupportEventTypeAssertionEnum.TYPE));

                SendAssertColumn(env, "{}", null);
                SendAssertColumn(env, "{\"c0\": []}", Array.Empty<object>());
                SendAssertColumn(env, "{\"c0\": [1.0]}", new object[] { 1.0d });
                SendAssertColumn(env, "{\"c0\": [null]}", new object[] { null });
                SendAssertColumn(env, "{\"c0\": [true]}", new object[] { true });
                SendAssertColumn(env, "{\"c0\": [false]}", new object[] { false });
                SendAssertColumn(env, "{\"c0\": [\"abc\"]}", new object[] { "abc" });
                SendAssertColumn(env, "{\"c0\": [[\"abc\"]]}", new object[][] { new object[] { "abc" } });
                SendAssertColumn(
                    env,
                    "{\"c0\": [[]]}",
                    new object[] {
                        Array.Empty<object>()
                    });
                SendAssertColumn(env, "{\"c0\": [[\"abc\", 2]]}", new object[][] { new object[] { "abc", 2 } });
                SendAssertColumn(
                    env,
                    "{\"c0\": [[[\"abc\"], [5.0]]]}",
                    new object[][][] { new object[][] { new object[] { "abc" }, new object[] { 5d } } });
                SendAssertColumn(env, "{\"c0\": [{\"c1\": 10}]}", new object[] { Collections.SingletonMap("c1", 10) });
                SendAssertColumn(
                    env,
                    "{\"c0\": [{\"c1\": 10, \"c2\": \"abc\"}]}",
                    new object[] { CollectionUtil.BuildMap("c1", 10, "c2", "abc") });

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    en => {
                        AssertColumn(en.Advance(), null);
                        AssertColumn(en.Advance(), Array.Empty<object>());
                        AssertColumn(en.Advance(), new object[] { 1.0d });
                        AssertColumn(en.Advance(), new object[] { null });
                        AssertColumn(en.Advance(), new object[] { true });
                        AssertColumn(en.Advance(), new object[] { false });
                        AssertColumn(en.Advance(), new object[] { "abc" });
                        AssertColumn(en.Advance(), new object[][] { new object[] { "abc" } });
                        AssertColumn(
                            en.Advance(),
                            new object[] {
                                Array.Empty<object>()
                            });
                        AssertColumn(en.Advance(), new object[][] { new object[] { "abc", 2 } });
                        AssertColumn(
                            en.Advance(),
                            new object[][][] { new object[][] { new object[] { "abc" }, new object[] { 5d } } });
                        AssertColumn(en.Advance(), new object[] { Collections.SingletonMap("c1", 10) });
                        AssertColumn(en.Advance(), new object[] { CollectionUtil.BuildMap("c1", 10, "c2", "abc") });
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingParseObjectType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (c0 Object);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var namesAndTypes = new object[][] { new object[] { "c0", typeof(object) } };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        namesAndTypes,
                        statement.EventType,
                        SupportEventTypeAssertionEnum.NAME,
                        SupportEventTypeAssertionEnum.TYPE));

                SendAssertColumn(env, "{}", null);
                SendAssertColumn(env, "{\"c0\": 1}", 1);
                SendAssertColumn(env, "{\"c0\": 1.0}", 1.0d);
                SendAssertColumn(env, "{\"c0\": null}", null);
                SendAssertColumn(env, "{\"c0\": true}", true);
                SendAssertColumn(env, "{\"c0\": false}", false);
                SendAssertColumn(env, "{\"c0\": \"abc\"}", "abc");
                SendAssertColumn(env, "{\"c0\": [\"abc\"]}", new object[] { "abc" });
                SendAssertColumn(env, "{\"c0\": []}", Array.Empty<object>());
                SendAssertColumn(env, "{\"c0\": [\"abc\", 2]}", new object[] { "abc", 2 });
                SendAssertColumn(
                    env,
                    "{\"c0\": [[\"abc\"], [5.0]]}",
                    new object[][] { new object[] { "abc" }, new object[] { 5d } });
                SendAssertColumn(env, "{\"c0\": {\"c1\": 10}}", Collections.SingletonMap("c1", 10));
                SendAssertColumn(
                    env,
                    "{\"c0\": {\"c1\": 10, \"c2\": \"abc\"}}",
                    CollectionUtil.BuildMap("c1", 10, "c2", "abc"));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    en => {
                        AssertColumn(en.Advance(), null);
                        AssertColumn(en.Advance(), 1);
                        AssertColumn(en.Advance(), 1.0d);
                        AssertColumn(en.Advance(), null);
                        AssertColumn(en.Advance(), true);
                        AssertColumn(en.Advance(), false);
                        AssertColumn(en.Advance(), "abc");
                        AssertColumn(en.Advance(), new object[] { "abc" });
                        AssertColumn(en.Advance(), Array.Empty<object>());
                        AssertColumn(en.Advance(), new object[] { "abc", 2 });
                        AssertColumn(en.Advance(), new object[][] { new object[] { "abc" }, new object[] { 5d } });
                        AssertColumn(en.Advance(), Collections.SingletonMap("c1", 10));
                        AssertColumn(en.Advance(), CollectionUtil.BuildMap("c1", 10, "c2", "abc"));
                    });

                env.UndeployAll();
            }
        }

        private class EventJsonTypingParseDecimalBigInt : RegressionExecution
        {
            private static readonly BigInteger BI = BigInteger.Parse("123456789123456789123456789");
            private static readonly decimal BD = 123456789123456789123456789.1m;

            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (c0 BigInteger, c1 decimal," +
                          "c2 BigInteger[], c3 decimal[], c4 BigInteger[][], c5 decimal[][]);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var namesAndTypes = new object[][] {
                    new object[] { "c0", typeof(BigInteger) }, new object[] { "c1", typeof(decimal) },
                    new object[] { "c2", typeof(BigInteger[]) }, new object[] { "c3", typeof(decimal[]) },
                    new object[] { "c4", typeof(BigInteger[][]) }, new object[] { "c5", typeof(decimal[][]) }
                };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        namesAndTypes,
                        statement.EventType,
                        SupportEventTypeAssertionEnum.NAME,
                        SupportEventTypeAssertionEnum.TYPE));

                var json = "{\"c0\": 123456789123456789123456789, \"c1\": 123456789123456789123456789.1," +
                           "\"c2\": [123456789123456789123456789], \"c3\": [123456789123456789123456789.1]," +
                           "\"c4\": [[123456789123456789123456789]], \"c5\": [[123456789123456789123456789.1]]" +
                           "}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertFilled);

                json = "{}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertUnfilled);

                json = "{\"c0\": null, \"c1\": null, \"c2\": null, \"c3\": null, \"c4\": null, \"c5\": null}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertUnfilled);

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertFilled(it.Advance());
                        AssertUnfilled(it.Advance());
                        AssertUnfilled(it.Advance());
                    });

                env.UndeployAll();
            }

            private void AssertUnfilled(EventBean @event)
            {
                SendAssertFields(@event, null, null, null, null, null, null);
            }

            private void AssertFilled(EventBean @event)
            {
                SendAssertFields(
                    @event,
                    BI,
                    BD,
                    new BigInteger[] { BI },
                    new decimal?[] { BD },
                    new BigInteger[][] { new BigInteger[] { BI } },
                    new decimal?[][] { new decimal?[] { BD } });
            }

            private void SendAssertFields(
                EventBean @event,
                BigInteger? c0,
                decimal? c1,
                BigInteger[] c2,
                decimal?[] c3,
                BigInteger[][] c4,
                decimal?[][] c5)
            {
                EPAssertionUtil.AssertProps(
                    @event,
                    "c0,c1,c2,c3,c4,c5".Split(","),
                    new object[] { c0, c1, c2, c3, c4, c5 });
            }
        }

        private class EventJsonTypingParseEnumType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create json schema JsonEvent (c0 SupportEnum, c1 SupportEnum[], c2 SupportEnum[][]);\n" +
                    "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var namesAndTypes = new object[][] {
                    new object[] { "c0", typeof(SupportEnum) }, new object[] { "c1", typeof(SupportEnum[]) },
                    new object[] { "c2", typeof(SupportEnum[][]) }
                };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        namesAndTypes,
                        statement.EventType,
                        SupportEventTypeAssertionEnum.NAME,
                        SupportEventTypeAssertionEnum.TYPE));

                var json =
                    "{\"c0\": \"ENUM_VALUE_2\", \"c1\": [\"ENUM_VALUE_2\", \"ENUM_VALUE_1\"], \"c2\": [[\"ENUM_VALUE_2\"], [\"ENUM_VALUE_1\", \"ENUM_VALUE_3\"]]}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertFilled);

                json = "{}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertUnfilled);

                json = "{\"c0\": null, \"c1\": null, \"c2\": null}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertUnfilled);

                json = "{\"c1\": [], \"c2\": [[]]}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertEmptyArray);

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertFilled(it.Advance());
                        AssertUnfilled(it.Advance());
                        AssertUnfilled(it.Advance());
                        AssertEmptyArray(it.Advance());
                    });

                env.UndeployAll();
            }

            private void AssertFilled(EventBean @event)
            {
                AssertFields(
                    @event,
                    ENUM_VALUE_2,
                    new SupportEnum[] { ENUM_VALUE_2, ENUM_VALUE_1 },
                    new SupportEnum[][]
                        { new SupportEnum[] { ENUM_VALUE_2 }, new SupportEnum[] { ENUM_VALUE_1, ENUM_VALUE_3 } });
            }

            private void AssertEmptyArray(EventBean @event)
            {
                AssertFields(@event, null, Array.Empty<SupportEnum>(), new SupportEnum[][] { new SupportEnum[] { } });
            }

            private void AssertUnfilled(EventBean @event)
            {
                AssertFields(@event, null, null, null);
            }

            private void AssertFields(
                EventBean @event,
                SupportEnum? c0,
                SupportEnum[] c1,
                SupportEnum[][] c2)
            {
                EPAssertionUtil.AssertProps(@event, "c0,c1,c2".Split(","), new object[] { c0, c1, c2 });
            }
        }

        private class EventJsonTypingParseBasicTypeArray : RegressionExecution
        {
            private static readonly string[] FIELDS_ZERO = "c0,c1,c2,c3,c4,c5,c6".Split(",");
            private static readonly string[] FIELDS_ONE = "c7,c8,c9,c10,c11,c12".Split(",");
            private static readonly string[] FIELDS_TWO = "c13,c14,c15,c16".Split(",");

            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (" +
                          "c0 string[], " +
                          "c1 char[], c2 char[primitive], " +
                          "c3 bool[], c4 boolean[primitive], " +
                          "c5 byte[], c6 byte[primitive], " +
                          "c7 short[], c8 short[primitive], " +
                          "c9 int[], c10 int[primitive], " +
                          "c11 long[], c12 long[primitive], " +
                          "c13 double[], c14 double[primitive], " +
                          "c15 float[], c16 float[primitive]);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var namesAndTypes = new object[][] {
                    new object[] { "c0", typeof(string[]) }, new object[] { "c1", typeof(char[]) },
                    new object[] { "c2", typeof(char[]) }, new object[] { "c3", typeof(bool?[]) },
                    new object[] { "c4", typeof(bool[]) }, new object[] { "c5", typeof(byte?[]) },
                    new object[] { "c6", typeof(byte[]) }, new object[] { "c7", typeof(short?[]) },
                    new object[] { "c8", typeof(short[]) }, new object[] { "c9", typeof(int?[]) },
                    new object[] { "c10", typeof(int[]) }, new object[] { "c11", typeof(long?[]) },
                    new object[] { "c12", typeof(long[]) }, new object[] { "c13", typeof(double?[]) },
                    new object[] { "c14", typeof(double[]) }, new object[] { "c15", typeof(float?[]) },
                    new object[] { "c16", typeof(float[]) },
                };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        namesAndTypes,
                        statement.EventType,
                        SupportEventTypeAssertionEnum.NAME,
                        SupportEventTypeAssertionEnum.TYPE));

                var json = "{ \"c0\": [\"abc\", \"def\"],\n" +
                           "\"c1\": [\"xy\", \"z\"],\n" +
                           "\"c2\": [\"x\", \"yz\"],\n" +
                           "\"c3\": [true, false],\n" +
                           "\"c4\": [false, true],\n" +
                           "\"c5\": [10, 11],\n" +
                           "\"c6\": [12, 13],\n" +
                           "\"c7\": [20, 21],\n" +
                           "\"c8\": [22, 23],\n" +
                           "\"c9\": [30, 31],\n" +
                           "\"c10\": [32, 33],\n" +
                           "\"c11\": [40, 41],\n" +
                           "\"c12\": [42, 43],\n" +
                           "\"c13\": [50, 51],\n" +
                           "\"c14\": [52, 53],\n" +
                           "\"c15\": [60, 61],\n" +
                           "\"c16\": [62, 63]" +
                           "}\n";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertFilled);

                env.SendEventJson("[]", "JsonEvent");
                env.AssertEventNew("s0", this.AssertUnfilled);

                json = "{ \"c0\": [],\n" +
                       "\"c1\": [],\n" +
                       "\"c2\": [],\n" +
                       "\"c3\": [],\n" +
                       "\"c4\": [],\n" +
                       "\"c5\": [],\n" +
                       "\"c6\": [],\n" +
                       "\"c7\": [],\n" +
                       "\"c8\": [],\n" +
                       "\"c9\": [],\n" +
                       "\"c10\": [],\n" +
                       "\"c11\": [],\n" +
                       "\"c12\": [],\n" +
                       "\"c13\": [],\n" +
                       "\"c14\": [],\n" +
                       "\"c15\": [],\n" +
                       "\"c16\": []" +
                       "}\n";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertEmptyArray);

                json = "{ \"c0\": null,\n" +
                       "\"c1\": null,\n" +
                       "\"c2\": null,\n" +
                       "\"c3\": null,\n" +
                       "\"c4\": null,\n" +
                       "\"c5\": null,\n" +
                       "\"c6\": null,\n" +
                       "\"c7\": null,\n" +
                       "\"c8\": null,\n" +
                       "\"c9\": null,\n" +
                       "\"c10\": null,\n" +
                       "\"c11\": null,\n" +
                       "\"c12\": null,\n" +
                       "\"c13\": null,\n" +
                       "\"c14\": null,\n" +
                       "\"c15\": null,\n" +
                       "\"c16\": null" +
                       "}\n";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertUnfilled(@event));

                json = "{ \"c0\": [null, \"def\", null],\n" +
                       "\"c1\": [\"xy\", null],\n" +
                       "\"c2\": [\"x\"],\n" +
                       "\"c3\": [true, null, false],\n" +
                       "\"c4\": [true],\n" +
                       "\"c5\": [null, null, null],\n" +
                       "\"c6\": [12],\n" +
                       "\"c7\": [20, 21, null],\n" +
                       "\"c8\": [23],\n" +
                       "\"c9\": [null, 30, null, 31, null, 32],\n" +
                       "\"c10\": [32],\n" +
                       "\"c11\": [null, 40, 41, null],\n" +
                       "\"c12\": [42],\n" +
                       "\"c13\": [null, null, 51],\n" +
                       "\"c14\": [52],\n" +
                       "\"c15\": [null],\n" +
                       "\"c16\": [63]" +
                       "}\n";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertPartialFilled(@event));

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertFilled(it.Advance());
                        AssertUnfilled(it.Advance());
                        AssertEmptyArray(it.Advance());
                        AssertUnfilled(it.Advance());
                        AssertPartialFilled(it.Advance());
                    });

                env.UndeployAll();
            }

            private void AssertEmptyArray(EventBean @event)
            {
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ZERO,
                    new object[] {
                        Array.Empty<string>(), Array.Empty<char?>(),
                        Array.Empty<char>(), Array.Empty<bool?>(), Array.Empty<bool>(), Array.Empty<byte?>(),
                        Array.Empty<byte>()
                    });
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ONE,
                    new object[] {
                        Array.Empty<short?>(), Array.Empty<short>(),
                        Array.Empty<int?>(), Array.Empty<int>(), Array.Empty<long?>(), Array.Empty<long>()
                    });
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_TWO,
                    new object[]
                        { Array.Empty<double?>(), Array.Empty<double>(), Array.Empty<float?>(), Array.Empty<float>() });
            }

            private void AssertPartialFilled(EventBean @event)
            {
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ZERO,
                    new object[] {
                        new string[] { null, "def", null }, new char?[] { 'x', null },
                        new char[] { 'x' }, new bool?[] { true, null, false }, new bool[] { true },
                        new byte?[] { null, null, null }, new byte[] { 12 }
                    });
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ONE,
                    new object[] {
                        new short?[] { 20, 21, null }, new short[] { 23 },
                        new int?[] { null, 30, null, 31, null, 32 }, new int[] { 32 },
                        new long?[] { null, 40L, 41L, null }, new long[] { 42 }
                    });
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_TWO,
                    new object[] {
                        new double?[] { null, null, 51d }, new double[] { 52 },
                        new float?[] { null }, new float[] { 63 }
                    });
            }

            private void AssertFilled(EventBean @event)
            {
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ZERO,
                    new object[] {
                        new string[] { "abc", "def" }, new char?[] { 'x', 'z' },
                        new char[] { 'x', 'y' }, new bool?[] { true, false }, new bool[] { false, true },
                        new byte?[] { 10, 11 }, new byte[] { 12, 13 }
                    });
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ONE,
                    new object[] {
                        new short?[] { 20, 21 }, new short[] { 22, 23 },
                        new int?[] { 30, 31 }, new int[] { 32, 33 }, new long?[] { 40L, 41L }, new long[] { 42, 43 }
                    });
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_TWO,
                    new object[] {
                        new double?[] { 50d, 51d }, new double[] { 52, 53 },
                        new float?[] { 60f, 61f }, new float[] { 62, 63 }
                    });
            }

            private void AssertUnfilled(EventBean @event)
            {
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ZERO,
                    new object[] { null, null, null, null, null, null, null });
                EPAssertionUtil.AssertProps(@event, FIELDS_ONE, new object[] { null, null, null, null, null, null });
                EPAssertionUtil.AssertProps(@event, FIELDS_TWO, new object[] { null, null, null, null });
            }
        }

        private class EventJsonTypingParseBasicTypeArray2Dim : RegressionExecution
        {
            private static readonly string[] FIELDS_ZERO = "c0,c1,c2,c3,c4,c5,c6".Split(",");
            private static readonly string[] FIELDS_ONE = "c7,c8,c9,c10,c11,c12".Split(",");
            private static readonly string[] FIELDS_TWO = "c13,c14,c15,c16".Split(",");

            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (" +
                          "c0 string[][], " +
                          "c1 char[][], c2 char[primitive][], " +
                          "c3 bool[][], c4 boolean[primitive][], " +
                          "c5 byte[][], c6 byte[primitive][], " +
                          "c7 short[][], c8 short[primitive][], " +
                          "c9 int[][], c10 int[primitive][], " +
                          "c11 long[][], c12 long[primitive][], " +
                          "c13 double[][], c14 double[primitive][], " +
                          "c15 float[][], c16 float[primitive][]);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var namesAndTypes = new object[][] {
                    new object[] { "c0", typeof(string[][]) }, new object[] { "c1", typeof(char?[][]) },
                    new object[] { "c2", typeof(char[][]) }, new object[] { "c3", typeof(bool?[][]) },
                    new object[] { "c4", typeof(bool[][]) }, new object[] { "c5", typeof(byte?[][]) },
                    new object[] { "c6", typeof(byte[][]) }, new object[] { "c7", typeof(short?[][]) },
                    new object[] { "c8", typeof(short[][]) }, new object[] { "c9", typeof(int?[][]) },
                    new object[] { "c10", typeof(int[][]) }, new object[] { "c11", typeof(long?[][]) },
                    new object[] { "c12", typeof(long[][]) }, new object[] { "c13", typeof(double?[][]) },
                    new object[] { "c14", typeof(double[][]) }, new object[] { "c15", typeof(float?[][]) },
                    new object[] { "c16", typeof(float[][]) },
                };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        namesAndTypes,
                        statement.EventType,
                        SupportEventTypeAssertionEnum.NAME,
                        SupportEventTypeAssertionEnum.TYPE));

                var json = "{ \"c0\": [[\"a\", \"b\"],[\"c\"]],\n" +
                           "\"c1\": [[\"xy\", \"z\"],[\"n\"]],\n" +
                           "\"c2\": [[\"x\"], [\"y\", \"z\"]],\n" +
                           "\"c3\": [[], [true, false], []],\n" +
                           "\"c4\": [[false, true]],\n" +
                           "\"c5\": [[10], [11]],\n" +
                           "\"c6\": [[12, 13]],\n" +
                           "\"c7\": [[20, 21], [22, 23]],\n" +
                           "\"c8\": [[22], [23], []],\n" +
                           "\"c9\": [[], [], [30, 31]],\n" +
                           "\"c10\": [[32], [33, 34]],\n" +
                           "\"c11\": [[40], [], [41]],\n" +
                           "\"c12\": [[42, 43], [44]],\n" +
                           "\"c13\": [[50], [51, 52], [53]],\n" +
                           "\"c14\": [[54], [55, 56]],\n" +
                           "\"c15\": [[60, 61], []],\n" +
                           "\"c16\": [[62], [63]]" +
                           "}\n";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertFilled(@event));

                env.SendEventJson("[]", "JsonEvent");
                env.AssertEventNew("s0", @event => AssertUnfilled(@event));

                json = "{ \"c0\": [],\n" +
                       "\"c1\": [],\n" +
                       "\"c2\": [],\n" +
                       "\"c3\": [],\n" +
                       "\"c4\": [],\n" +
                       "\"c5\": [],\n" +
                       "\"c6\": [],\n" +
                       "\"c7\": [],\n" +
                       "\"c8\": [],\n" +
                       "\"c9\": [],\n" +
                       "\"c10\": [],\n" +
                       "\"c11\": [],\n" +
                       "\"c12\": [],\n" +
                       "\"c13\": [],\n" +
                       "\"c14\": [],\n" +
                       "\"c15\": [],\n" +
                       "\"c16\": []" +
                       "}\n";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertEmptyArray);

                json = "{ \"c0\": null,\n" +
                       "\"c1\": null,\n" +
                       "\"c2\": null,\n" +
                       "\"c3\": null,\n" +
                       "\"c4\": null,\n" +
                       "\"c5\": null,\n" +
                       "\"c6\": null,\n" +
                       "\"c7\": null,\n" +
                       "\"c8\": null,\n" +
                       "\"c9\": null,\n" +
                       "\"c10\": null,\n" +
                       "\"c11\": null,\n" +
                       "\"c12\": null,\n" +
                       "\"c13\": null,\n" +
                       "\"c14\": null,\n" +
                       "\"c15\": null,\n" +
                       "\"c16\": null" +
                       "}\n";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", @event => AssertUnfilled(@event));

                json = "{ \"c0\": [[null, \"a\"]],\n" +
                       "\"c1\": [[null], [\"xy\"]],\n" +
                       "\"c2\": [null, [\"x\"]],\n" +
                       "\"c3\": [[null], [true]],\n" +
                       "\"c4\": [[true], null],\n" +
                       "\"c5\": [null, null],\n" +
                       "\"c6\": [null, [12, 13]],\n" +
                       "\"c7\": [[21], null],\n" +
                       "\"c8\": [null, [23], null],\n" +
                       "\"c9\": [[30], null, [31]],\n" +
                       "\"c10\": [[]],\n" +
                       "\"c11\": [[], []],\n" +
                       "\"c12\": [[42]],\n" +
                       "\"c13\": [null, []],\n" +
                       "\"c14\": [[], null],\n" +
                       "\"c15\": [[null]],\n" +
                       "\"c16\": [[63]]" +
                       "}\n";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertSomeFilled);

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertFilled(it.Advance());
                        AssertUnfilled(it.Advance());
                        AssertEmptyArray(it.Advance());
                        AssertUnfilled(it.Advance());
                        AssertSomeFilled(it.Advance());
                    });

                env.UndeployAll();
            }

            private void AssertEmptyArray(EventBean @event)
            {
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ZERO,
                    new object[] {
                        Array.Empty<string[]>(), Array.Empty<char?[]>(),
                        Array.Empty<char[]>(), Array.Empty<bool?[]>(), Array.Empty<bool[]>(), Array.Empty<byte?[]>(),
                        Array.Empty<byte[]>()
                    });
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ONE,
                    new object[] {
                        Array.Empty<short?[]>(), Array.Empty<short[]>(),
                        Array.Empty<int?[]>(), Array.Empty<int[]>(), Array.Empty<long?[]>(), Array.Empty<long[]>()
                    });
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_TWO,
                    new object[] {
                        Array.Empty<double?[]>(), Array.Empty<double[]>(), Array.Empty<float?[]>(),
                        Array.Empty<float[]>()
                    });
            }

            private void AssertSomeFilled(EventBean @event)
            {
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ZERO,
                    new object[] {
                        new string[][] { new string[] { null, "a" } },
                        new char?[][] { new char?[] { null }, new char?[] { 'x' } },
                        new char[][] { null, new char[] { 'x' } },
                        new bool?[][] { new bool?[] { null }, new bool?[] { true } },
                        new bool[][] { new bool[] { true }, null },
                        new byte?[][] { null, null },
                        new byte[][] { null, new byte[] { 12, 13 } }
                    });
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ONE,
                    new object[] {
                        new short?[][] { new short?[] { 21 }, null },
                        new short[][] { null, new short[] { 23 }, null },
                        new int?[][] { new int?[] { 30 }, null, new int?[] { 31 } },
                        new int[][] { new int[] { } },
                        new long?[][] { new long?[] { }, new long?[] { } },
                        new long[][] { new long[] { 42 } }
                    });
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_TWO,
                    new object[] {
                        new double?[][] { null, new double?[] { } },
                        new double[][] { new double[] { }, null },
                        new float?[][] { new float?[] { null } },
                        new float[][] { new float[] { 63 } }
                    });
            }

            private void AssertUnfilled(EventBean @event)
            {
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ZERO,
                    new object[] { null, null, null, null, null, null, null });
                EPAssertionUtil.AssertProps(@event, FIELDS_ONE, new object[] { null, null, null, null, null, null });
                EPAssertionUtil.AssertProps(@event, FIELDS_TWO, new object[] { null, null, null, null });
            }

            private void AssertFilled(EventBean @event)
            {
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ZERO,
                    new object[] {
                        new string[][] { new string[] { "a", "b" }, new string[] { "c" } },
                        new char?[][] { new char?[] { 'x', 'z' }, new char?[] { 'n' } },
                        new char[][] { new char[] { 'x' }, new char[] { 'y', 'z' } },
                        new bool?[][] { new bool?[] { }, new bool?[] { true, false }, new bool?[] { } },
                        new bool[][] { new bool[] { false, true } },
                        new byte?[][] { new byte?[] { 10 }, new byte?[] { 11 } },
                        new byte[][] { new byte[] { 12, 13 } }
                    });
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ONE,
                    new object[] {
                        new short?[][] { new short?[] { 20, 21 }, new short?[] { 22, 23 } },
                        new short[][] { new short[] { 22 }, new short[] { 23 }, new short[] { } },
                        new int?[][] { new int?[] { }, new int?[] { }, new int?[] { 30, 31 } },
                        new int[][] { new int[] { 32 }, new int[] { 33, 34 } },
                        new long?[][] { new long?[] { 40L }, new long?[] { }, new long?[] { 41L } },
                        new long[][] { new long[] { 42, 43 }, new long[] { 44 } }
                    });
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_TWO,
                    new object[] {
                        new double?[][] { new double?[] { 50d }, new double?[] { 51d, 52d }, new double?[] { 53d } },
                        new double[][] { new double[] { 54 }, new double[] { 55, 56 } },
                        new float?[][] { new float?[] { 60f, 61f }, new float?[] { } },
                        new float[][] { new float[] { 62 }, new float[] { 63 } }
                    });
            }
        }

        private class EventJsonTypingParseBasicType : RegressionExecution
        {
            private static readonly string[] FIELDS_ONE = "c0,c1,c2,c3,c4,c5,c6".Split(",");
            private static readonly string[] FIELDS_TWO = "c7,c8,c9,c10,c11,c12".Split(",");

            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create json schema JsonEvent (" +
                          "c0 string, c1 char, c2 character, c3 bool, c4 boolean, " +
                          "c5 byte, c6 short, c7 int, c8 integer, c9 long, c10 double, c11 float, c12 null);\n" +
                          "@name('s0') select * from JsonEvent#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var namesAndTypes = new object[][] {
                    new object[] { "c0", typeof(string) }, new object[] { "c1", typeof(char?) },
                    new object[] { "c2", typeof(char?) }, new object[] { "c3", typeof(bool?) },
                    new object[] { "c4", typeof(bool?) }, new object[] { "c5", typeof(byte?) },
                    new object[] { "c6", typeof(short?) }, new object[] { "c7", typeof(int?) },
                    new object[] { "c8", typeof(int?) }, new object[] { "c9", typeof(long?) },
                    new object[] { "c10", typeof(double?) }, new object[] { "c11", typeof(float?) },
                    new object[] { "c12", null },
                };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        namesAndTypes,
                        statement.EventType,
                        SupportEventTypeAssertionEnum.NAME,
                        SupportEventTypeAssertionEnum.TYPE));

                var json = "{\n" +
                           "  \"c0\": \"abc\",\n" +
                           "  \"c1\": \"xy\",\n" +
                           "  \"c2\": \"z\",\n" +
                           "  \"c3\": true,\n" +
                           "  \"c4\": false,\n" +
                           "  \"c5\": 1,\n" +
                           "  \"c6\": 10,\n" +
                           "  \"c7\": 11,\n" +
                           "  \"c8\": 12,\n" +
                           "  \"c9\": 13,\n" +
                           "  \"c10\": 14,\n" +
                           "  \"c11\": 15E2,\n" +
                           "  \"c12\": null\n" +
                           "}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertEventFilled);

                env.SendEventJson("{}", "JsonEvent");
                env.AssertEventNew("s0", this.AssertEventNull);

                json = "{\n" +
                       "  \"c0\": null,\n" +
                       "  \"c1\": null,\n" +
                       "  \"c2\": null,\n" +
                       "  \"c3\": null,\n" +
                       "  \"c4\": null,\n" +
                       "  \"c5\": null,\n" +
                       "  \"c6\": null,\n" +
                       "  \"c7\": null,\n" +
                       "  \"c8\": null,\n" +
                       "  \"c9\": null,\n" +
                       "  \"c10\": null,\n" +
                       "  \"c11\": null,\n" +
                       "  \"c12\": null\n" +
                       "}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertEventNull);

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertEventFilled(it.Advance());
                        AssertEventNull(it.Advance());
                        AssertEventNull(it.Advance());
                    });

                env.UndeployAll();
            }

            private void AssertEventNull(EventBean @event)
            {
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ONE,
                    new object[] { null, null, null, null, null, null, null });
                EPAssertionUtil.AssertProps(@event, FIELDS_TWO, new object[] { null, null, null, null, null, null });
            }

            private void AssertEventFilled(EventBean @event)
            {
                EPAssertionUtil.AssertProps(
                    @event,
                    FIELDS_ONE,
                    new object[] { "abc", 'x', 'z', true, false, byte.Parse("1"), (short)10 });
                EPAssertionUtil.AssertProps(@event, FIELDS_TWO, new object[] { 11, 12, 13L, 14D, 15E2f, null });
            }
        }

        private class EventJsonTypingParseDynamicPropNumberFormat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent();\n" +
                        "@name('s0') select num1? as c0, num2? as c1, num3? as c2 from JsonEvent#keepall")
                    .AddListener("s0");

                var json = "{ \"num1\": 42, \"num2\": 42.0, \"num3\": 4.2E+1}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertFill);

                env.Milestone(0);

                env.AssertIterator("s0", it => AssertFill(it.Advance()));

                env.UndeployAll();
            }

            private void AssertFill(EventBean @event)
            {
                EPAssertionUtil.AssertProps(@event, "c0,c1,c2".Split(","), new object[] { 42, 42d, 42d });
            }
        }

        private class EventJsonTypingParseDynamicPropNestedArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent();\n" +
                        "@name('s0') select a_array? as c0 from JsonEvent#keepall")
                    .AddListener("s0");
                string json;

                json = "{\n" +
                       "  \"a_array\": [\n" +
                       "    [1,2],\n" +
                       "    [[3,4], 5]" +
                       "  ]\n" +
                       "}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertFillOne);

                json = "{\n" +
                       "  \"a_array\": [\n" +
                       "    [6, [ [7,8], [9], []]]\n" +
                       "  ]\n" +
                       "}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertFillTwo);

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertFillOne(it.Advance());
                        AssertFillTwo(it.Advance());
                    });

                env.UndeployAll();
            }

            private void AssertFillTwo(EventBean @event)
            {
                var array6To9 = (object[])@event.Get("c0");
                Assert.AreEqual(1, array6To9.Length);
                var array6plus = (object[])array6To9[0];
                Assert.AreEqual("6", array6plus[0].ToString());
                var array7plus = (object[])array6plus[1];
                EPAssertionUtil.AssertEqualsExactOrder((object[])array7plus[0], new object[] { 7, 8 });
                EPAssertionUtil.AssertEqualsExactOrder((object[])array7plus[1], new object[] { 9 });
                EPAssertionUtil.AssertEqualsExactOrder((object[])array7plus[2], new object[] { });
            }

            private void AssertFillOne(EventBean @event)
            {
                var array1To5 = (object[])@event.Get("c0");
                Assert.AreEqual(2, array1To5.Length);
                var array12 = (object[])array1To5[0];
                EPAssertionUtil.AssertEqualsExactOrder(array12, new object[] { 1, 2 });
                var array345 = (object[])array1To5[1];
                EPAssertionUtil.AssertEqualsExactOrder((object[])array345[0], new object[] { 3, 4 });
                Assert.AreEqual(5, array345[1]);
            }
        }

        private class EventJsonTypingParseDynamicPropMixedOjectArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent();\n" +
                        "@name('s0') select a_array? as c0 from JsonEvent#keepall")
                    .AddListener("s0");

                var json = "{\n" +
                           "  \"a_array\": [\n" +
                           "    \"a\",\n" +
                           "     1,\n" +
                           "    {\n" +
                           "      \"value\": \"def\"\n" +
                           "    },\n" +
                           "    false,\n" +
                           "    null\n" +
                           "  ]\n" +
                           "}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertFilled);

                env.Milestone(0);

                env.AssertIterator("s0", it => AssertFilled(it.Advance()));

                env.UndeployAll();
            }

            private void AssertFilled(EventBean @event)
            {
                var array = (object[])@event.Get("c0");
                Assert.AreEqual("a", array[0]);
                Assert.AreEqual(1, array[1]);
                var nested = (IDictionary<string, object>)array[2];
                Assert.AreEqual("{value=def}", nested.ToString());
                Assert.IsFalse((bool)array[3]);
                Assert.IsNull(array[4]);
            }
        }

        private class EventJsonTypingParseDynamicPropJsonTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@JsonSchema(Dynamic=true) @public @buseventtype create json schema JsonEvent();\n" +
                        "@name('s0') select a_string? as c0, exists(a_string?) as c1," +
                        "a_number? as c2, exists(a_number?) as c3," +
                        "a_boolean? as c4, exists(a_boolean?) as c5," +
                        "a_null? as c6, exists(a_null?) as c7," +
                        "a_object? as c8, exists(a_object?) as c9, " +
                        "a_array? as c10, exists(a_array?) as c11 " +
                        " from JsonEvent#keepall")
                    .AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => {
                        foreach (var prop in statement.EventType.PropertyDescriptors) {
                            Assert.AreEqual(
                                "c1,c3,c5,c7,c9,c11".Contains(prop.PropertyName) ? typeof(bool?) : typeof(object),
                                prop.PropertyType);
                        }
                    });

                var json = "{\n" +
                           "  \"a_string\": \"abc\",\n" +
                           "  \"a_number\": 1,\n" +
                           "  \"a_boolean\": true,\n" +
                           "  \"a_null\": null,\n" +
                           "  \"a_object\": {\n" +
                           "    \"value\": \"def\"\n" +
                           "  },\n" +
                           "  \"a_array\": [\n" +
                           "    \"a\",\n" +
                           "    \"b\"\n" +
                           "  ]\n" +
                           "}";
                env.SendEventJson(json, "JsonEvent");
                env.AssertEventNew("s0", this.AssertFilled);

                env.SendEventJson("{}", "JsonEvent");
                env.AssertEventNew("s0", this.AssertUnfilled);

                env.SendEventJson("{\"a_boolean\": false}", "JsonEvent");
                env.AssertEventNew("s0", this.AssertSomeFilled);

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    it => {
                        AssertFilled(it.Advance());
                        AssertUnfilled(it.Advance());
                        AssertSomeFilled(it.Advance());
                    });

                env.UndeployAll();
            }

            private void AssertFilled(EventBean eventBean)
            {
                EPAssertionUtil.AssertProps(
                    eventBean,
                    "c0,c1,c2,c3,c4,c5,c6,c7,c9,c11".Split(","),
                    new object[] { "abc", true, 1, true, true, true, null, true, true, true });
                var @object = (IDictionary<string, object>)eventBean.Get("c8");
                Assert.AreEqual("def", @object.Get("value"));
                var array = (object[])eventBean.Get("c10");
                Assert.AreEqual("a", array[0]);
            }

            private void AssertUnfilled(EventBean eventBean)
            {
                EPAssertionUtil.AssertProps(
                    eventBean,
                    "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9,c10,c11".Split(","),
                    new object[] { null, false, null, false, null, false, null, false, null, false, null, false });
            }

            private void AssertSomeFilled(EventBean eventBean)
            {
                EPAssertionUtil.AssertProps(
                    eventBean,
                    "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9,c10,c11".Split(","),
                    new object[] { null, false, null, false, false, true, null, false, null, false, null, false });
            }
        }

        private static void SendAssertColumn(
            RegressionEnvironment env,
            string json,
            object c0)
        {
            env.SendEventJson(json, "JsonEvent");
            env.AssertEventNew("s0", @event => AssertColumn(@event, c0));
        }

        private static void AssertColumn(
            EventBean @event,
            object c0)
        {
            EPAssertionUtil.AssertProps(@event, "c0".Split(","), new object[] { c0 });
        }

        private static void SendAssert(
            RegressionEnvironment env,
            string json,
            Consumer<object> assertion)
        {
            env.SendEventJson(json, "JsonEvent");
            env.AssertEventNew("s0", @event => JustAssert(@event, assertion));
        }

        private static void JustAssert(
            EventBean @event,
            Consumer<object> assertion)
        {
            assertion.Invoke(@event.Get("c0"));
        }
    }
} // end of namespace