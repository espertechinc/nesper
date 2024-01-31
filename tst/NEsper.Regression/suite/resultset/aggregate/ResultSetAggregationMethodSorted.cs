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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public partial class ResultSetAggregationMethodSorted
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithNonTable(execs);
            WithTableAccess(execs);
            WithTableIdent(execs);
            WithCFHL(execs);
            WithCFHLEnumerationAndDot(execs);
            WithFirstLast(execs);
            WithFirstLastEnumerationAndDot(execs);
            WithGetContainsCounts(execs);
            WithSubmapEventsBetween(execs);
            WithOrderedDictionaryReference(execs);
            WithMultiCriteria(execs);
            WithGrouped(execs);
            WithInvalid(execs);
            WithDocSample(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDocSample(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedDocSample());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithGrouped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedGrouped());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiCriteria(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedMultiCriteria());
            return execs;
        }

        public static IList<RegressionExecution> WithOrderedDictionaryReference(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedOrderedDictionaryReference());
            return execs;
        }

        public static IList<RegressionExecution> WithSubmapEventsBetween(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedSubmapEventsBetween());
            return execs;
        }

        public static IList<RegressionExecution> WithGetContainsCounts(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedGetContainsCounts());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstLastEnumerationAndDot(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedFirstLastEnumerationAndDot());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedFirstLast());
            return execs;
        }

        public static IList<RegressionExecution> WithCFHLEnumerationAndDot(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedCFHLEnumerationAndDot());
            return execs;
        }

        public static IList<RegressionExecution> WithCFHL(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedCFHL());
            return execs;
        }

        public static IList<RegressionExecution> WithTableIdent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedTableIdent());
            return execs;
        }

        public static IList<RegressionExecution> WithTableAccess(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedTableAccess());
            return execs;
        }

        public static IList<RegressionExecution> WithNonTable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSortedNonTable());
            return execs;
        }

        private class ResultSetAggregateSortedDocSample : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@buseventtype @public create schema OrderEvent(OrderId string, Price double);\n" +
                    "@name('a') select sorted(Price).lowerKey(Price) as lowerPrice from OrderEvent#time(10 minutes);\n" +
                    "@name('b') select sorted(Price).lowerEvent(Price).OrderId as lowerPriceOrderId from OrderEvent#time(10 minutes);\n" +
                    "create table OrderPrices(prices sorted(Price) @type('OrderEvent'));\n" +
                    "into table OrderPrices select sorted(*) as prices from OrderEvent#time(10 minutes);\n" +
                    "@name('c') select OrderPrices.prices.firstKey() as lowestPrice, OrderPrices.prices.lastKey() as highestPrice from OrderEvent;\n" +
                    "@name('d') select (select prices.firstKey() from OrderPrices) as lowestPrice, * from OrderEvent;\n";
                env.CompileDeploy(epl).AddListener("a").AddListener("b").AddListener("c").AddListener("d");

                env.SendEventMap(CollectionUtil.BuildMap("OrderId", "A", "Price", 10d), "OrderEvent");
                env.AssertPropsNew("a", "lowerPrice".SplitCsv(), new object[] { null });
                env.AssertPropsNew("b", "lowerPriceOrderId".SplitCsv(), new object[] { null });
                env.AssertPropsNew("c", "lowestPrice,highestPrice".SplitCsv(), new object[] { 10d, 10d });
                env.AssertPropsNew("d", "lowestPrice".SplitCsv(), new object[] { 10d });

                env.Milestone(0);

                env.SendEventMap(CollectionUtil.BuildMap("OrderId", "B", "Price", 20d), "OrderEvent");
                env.AssertPropsNew("a", "lowerPrice".SplitCsv(), new object[] { 10d });
                env.AssertPropsNew("b", "lowerPriceOrderId".SplitCsv(), new object[] { "A" });
                env.AssertPropsNew("c", "lowestPrice,highestPrice".SplitCsv(), new object[] { 10d, 20d });
                env.AssertPropsNew("d", "lowestPrice".SplitCsv(), new object[] { 10d });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateSortedInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create table MyTable(sortcol sorted(IntPrimitive) @type('SupportBean'));\n",
                    path);

                env.TryInvalidCompile(
                    path,
                    "select MyTable.sortcol.notAnAggMethod() from SupportBean_S0",
                    "Failed to validate select-clause expression 'MyTable.sortcol.notAnAggMethod()': Could not find event property or method named 'notAnAggMethod' in collection of events of type ");

                env.TryInvalidCompile(
                    path,
                    "select MyTable.sortcol.floorKey() from SupportBean_S0",
                    "Failed to validate select-clause expression 'MyTable.sortcol.floorKey()': Parameters mismatch for aggregation method 'floorKey', the method requires an expression providing the key value");
                env.TryInvalidCompile(
                    path,
                    "select MyTable.sortcol.floorKey('a') from SupportBean_S0",
                    "Failed to validate select-clause expression 'MyTable.sortcol.floorKey(\"a\")': Method 'floorKey' for parameter 0 requires a key of type 'System.Nullable<System.Int32>' but receives 'System.String'");

                env.TryInvalidCompile(
                    path,
                    "select MyTable.sortcol.firstKey(Id) from SupportBean_S0",
                    "Failed to validate select-clause expression 'MyTable.sortcol.firstKey(Id)': Parameters mismatch for aggregation method 'firstKey', the method requires no parameters");

                env.TryInvalidCompile(
                    path,
                    "select MyTable.sortcol.submap(1, 2, 3, true) from SupportBean_S0",
                    "Failed to validate select-clause expression 'MyTable.sortcol.submap(1,2,3,true)': Failed to validate aggregation method 'submap', expected a boolean-type result for expression parameter 1 but received System.Int32");

                env.TryInvalidCompile(
                    path,
                    "select MyTable.sortcol.submap('a', true, 3, true) from SupportBean_S0",
                    "Failed to validate select-clause expression 'MyTable.sortcol.submap(\"a\",true,3,true)': Method 'submap' for parameter 0 requires a key of type 'System.Nullable<System.Int32>' but receives 'System.String'");

                env.TryInvalidCompile(
                    path,
                    "select MyTable.sortcol.submap(1, true, 'a', true) from SupportBean_S0",
                    "Failed to validate select-clause expression 'MyTable.sortcol.submap(1,true,\"a\",true)': Method 'submap' for parameter 2 requires a key of type 'System.Nullable<System.Int32>' but receives 'System.String'");

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateSortedGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create table MyTable(k0 string primary key, sortcol sorted(IntPrimitive) @type('SupportBean'));\n" +
                    "into table MyTable select sorted(*) as sortcol from SupportBean group by TheString;\n" +
                    "@name('s0') select " +
                    "MyTable[P00].sortcol.sorted() as sortcol," +
                    "MyTable[P00].sortcol.firstKey() as firstkey," +
                    "MyTable[P00].sortcol.lastKey() as lastkey" +
                    " from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertGrouped(env, "A", null, null);

                env.SendEventBean(new SupportBean("A", 10));
                env.SendEventBean(new SupportBean("A", 20));
                SendAssertGrouped(env, "A", 10, 20);

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A", 10));
                env.SendEventBean(new SupportBean("A", 21));
                SendAssertGrouped(env, "A", 10, 21);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("B", 100));
                SendAssertGrouped(env, "A", 10, 21);
                SendAssertGrouped(env, "B", 100, 100);

                env.UndeployAll();
            }

            private static void SendAssertGrouped(
                RegressionEnvironment env,
                string p00,
                int? firstKey,
                int? lastKey)
            {
                var fields = "firstkey,lastkey".SplitCsv();
                env.SendEventBean(new SupportBean_S0(-1, p00));
                env.AssertPropsNew("s0", fields, new object[] { firstKey, lastKey });
            }
        }

        private class ResultSetAggregateSortedMultiCriteria : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create table MyTable(sortcol sorted(TheString, IntPrimitive) @type('SupportBean'));\n" +
                          "into table MyTable select sorted(*) as sortcol from SupportBean;\n" +
                          "@name('s0') select " +
                          "MyTable.sortcol.firstKey() as firstkey," +
                          "MyTable.sortcol.lastKey() as lastkey," +
                          "MyTable.sortcol.lowerKey(new HashableMultiKey('E4', 1)) as lowerkey," +
                          "MyTable.sortcol.higherKey(new HashableMultiKey('E4b', -1)) as higherkey" +
                          " from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                AssertType(env, typeof(HashableMultiKey), "firstkey,lastkey,lowerkey");

                PrepareTestData(env, new OrderedListDictionary<int, IList<SupportBean>>()); // 1, 1, 4, 6, 6, 8, 9

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(-1));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        CompareKeys(@event.Get("firstkey"), "E1a", 1);
                        CompareKeys(@event.Get("lastkey"), "E9", 9);
                        CompareKeys(@event.Get("lowerkey"), "E1b", 1);
                        CompareKeys(@event.Get("higherkey"), "E4b", 4);
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateSortedSubmapEventsBetween : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema MySubmapEvent as " +
                          typeof(MySubmapEvent).MaskTypeName() +
                          ";\n" +
                          "create table MyTable(sortcol sorted(IntPrimitive) @type('SupportBean'));\n" +
                          "into table MyTable select sorted(*) as sortcol from SupportBean;\n" +
                          "@name('s0') select " +
                          "MyTable.sortcol.eventsBetween(FromKey, IsFromInclusive, ToKey, IsToInclusive) as eb," +
                          "MyTable.sortcol.eventsBetween(FromKey, IsFromInclusive, ToKey, IsToInclusive).lastOf() as eblastof," +
                          "MyTable.sortcol.subMap(FromKey, IsFromInclusive, ToKey, IsToInclusive) as sm" +
                          " from MySubmapEvent";
                env.CompileDeploy(epl).AddListener("s0");

                AssertType(env, typeof(SupportBean[]), "eb");
                AssertType(env, typeof(IOrderedDictionary<ICollection<object>, EventBean>), "sm");
                AssertType(env, typeof(SupportBean), "eblastof");

                var treemap = new OrderedListDictionary<int, IList<SupportBean>>();
                PrepareTestData(env, treemap); // 1, 1, 4, 6, 6, 8, 9

                for (var start = 0; start < 12; start++) {
                    for (var end = 0; end < 12; end++) {
                        if (start > end) {
                            continue;
                        }

                        foreach (var includeStart in new bool[] { false, true }) {
                            foreach (var includeEnd in new bool[] { false, true }) {
                                var sme = new MySubmapEvent(start, includeStart, end, includeEnd);
                                env.SendEventBean(sme);
                                env.AssertEventNew(
                                    "s0",
                                    @event => {
                                        var submap = @event.Get("sm")
                                            .AsObjectDictionary(MagicMarker.SingletonInstance)
                                            .TransformLeft<object, object, SupportBean[]>();

                                        AssertEventsBetween(
                                            treemap,
                                            sme,
                                            (SupportBean[])@event.Get("eb"),
                                            (SupportBean)@event.Get("eblastof"));
                                        AssertSubmap(treemap, sme, submap);
                                    });
                            }
                        }
                    }
                }

                env.Milestone(0);

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateSortedOrderedDictionaryReference : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create table MyTable(sortcol sorted(IntPrimitive) @type('SupportBean'));\n" +
                    "into table MyTable select sorted(*) as sortcol from SupportBean;\n" +
                    "@name('s0') select " +
                    "MyTable.sortcol.dictionaryReference() as nmr" +
                    " from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                AssertType(env, typeof(IOrderedDictionary<ICollection<object>, EventBean>), "nmr");

                var treemap = new OrderedListDictionary<int, IList<SupportBean>>();
                PrepareTestData(env, treemap); // 1, 1, 4, 6, 6, 8, 9

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(-1));
                env.AssertEventNew(
                    "s0",
                    @event => AssertOrderedDictionary(
                        treemap,
                        (IOrderedDictionary<object, ICollection<EventBean>>)@event.Get("nmr")));

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateSortedGetContainsCounts : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create table MyTable(sortcol sorted(IntPrimitive) @type('SupportBean'));\n" +
                    "into table MyTable select sorted(*) as sortcol from SupportBean;\n" +
                    "@name('s0') select " +
                    "MyTable.sortcol.getEvent(Id) as ge," +
                    "MyTable.sortcol.getEvents(Id) as ges," +
                    "MyTable.sortcol.containsKey(Id) as ck," +
                    "MyTable.sortcol.countEvents() as cnte," +
                    "MyTable.sortcol.countKeys() as cntk," +
                    "MyTable.sortcol.getEvent(Id).TheString as geid," +
                    "MyTable.sortcol.getEvent(Id).firstOf() as gefo," +
                    "MyTable.sortcol.getEvents(Id).lastOf() as geslo " +
                    " from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                AssertType(env, typeof(SupportBean), "ge,gefo,geslo");
                AssertType(env, typeof(SupportBean[]), "ges");
                AssertType(env, typeof(int?), "cnte,cntk");
                AssertType(env, typeof(bool?), "ck");
                AssertType(env, typeof(string), "geid");

                var treemap = new OrderedListDictionary<int, IList<SupportBean>>();
                PrepareTestData(env, treemap); // 1, 1, 4, 6, 6, 8, 9

                env.Milestone(0);

                for (var i = 0; i < 12; i++) {
                    env.SendEventBean(new SupportBean_S0(i));
                    var index = i;
                    env.AssertEventNew(
                        "s0",
                        @event => {
                            var message = "failed at " + index;
                            var valueAtIndex = treemap.Get(index);
                            ClassicAssert.AreEqual(FirstEvent(valueAtIndex), @event.Get("ge"), message);
                            EPAssertionUtil.AssertEqualsExactOrder(
                                AllEvents(valueAtIndex),
                                (SupportBean[])@event.Get("ges"));
                            ClassicAssert.AreEqual(treemap.ContainsKey(index), @event.Get("ck"), message);
                            ClassicAssert.AreEqual(7, @event.Get("cnte"), message);
                            ClassicAssert.AreEqual(5, @event.Get("cntk"), message);
                            ClassicAssert.AreEqual(FirstEventString(valueAtIndex), @event.Get("geid"), message);
                            ClassicAssert.AreEqual(FirstEvent(valueAtIndex), @event.Get("gefo"), message);
                            ClassicAssert.AreEqual(LastEvent(valueAtIndex), @event.Get("geslo"), message);
                        });
                }

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateSortedFirstLastEnumerationAndDot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create table MyTable(sortcol sorted(IntPrimitive) @type('SupportBean'));\n" +
                          "into table MyTable select sorted(*) as sortcol from SupportBean;\n" +
                          "@name('s0') select " +
                          "MyTable.sortcol.firstEvent().TheString as feid," +
                          "MyTable.sortcol.firstEvent().firstOf() as fefo," +
                          "MyTable.sortcol.firstEvents().lastOf() as feslo," +
                          "MyTable.sortcol.lastEvent().TheString() as leid," +
                          "MyTable.sortcol.lastEvent().firstOf() as lefo," +
                          "MyTable.sortcol.lastEvents().lastOf as leslo" +
                          " from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                AssertType(env, typeof(string), "feid,leid");
                AssertType(env, typeof(SupportBean), "fefo,feslo,lefo,leslo");

                var treemap = new OrderedListDictionary<int, IList<SupportBean>>();
                PrepareTestData(env, treemap); // 1, 1, 4, 6, 6, 8, 9

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(-1));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var treeMapFirst = treemap.First();
                        ClassicAssert.AreEqual(FirstEventString<IList<SupportBean>>(treeMapFirst), @event.Get("feid"));
                        ClassicAssert.AreEqual(FirstEvent<IList<SupportBean>>(treeMapFirst), @event.Get("fefo"));
                        ClassicAssert.AreEqual(LastEvent<IList<SupportBean>>(treeMapFirst), @event.Get("feslo"));

                        var treeMapLast = treemap.Last();
                        ClassicAssert.AreEqual(FirstEventString<IList<SupportBean>>(treeMapLast), @event.Get("leid"));
                        ClassicAssert.AreEqual(FirstEvent<IList<SupportBean>>(treeMapLast), @event.Get("lefo"));
                        ClassicAssert.AreEqual(LastEvent<IList<SupportBean>>(treeMapLast), @event.Get("leslo"));
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateSortedFirstLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create table MyTable(sortcol sorted(IntPrimitive) @type('SupportBean'));\n" +
                          "into table MyTable select sorted(*) as sortcol from SupportBean;\n" +
                          "@name('s0') select " +
                          "MyTable.sortcol.firstEvent() as fe," +
                          "MyTable.sortcol.minBy() as minb," +
                          "MyTable.sortcol.firstEvents() as fes," +
                          "MyTable.sortcol.firstKey() as fk," +
                          "MyTable.sortcol.lastEvent() as le," +
                          "MyTable.sortcol.maxBy() as maxb," +
                          "MyTable.sortcol.lastEvents() as les," +
                          "MyTable.sortcol.lastKey() as lk" +
                          " from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                AssertType(env, typeof(SupportBean), "fe,le,minb,maxb");
                AssertType(env, typeof(SupportBean[]), "fes,les");
                AssertType(env, typeof(int?), "fk,lk");

                var treemap = new OrderedListDictionary<int, IList<SupportBean>>();
                PrepareTestData(env, treemap); // 1, 1, 4, 6, 6, 8, 9

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(-1));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var treeMapFirst = treemap.First();
                        ClassicAssert.AreEqual(FirstEvent<IList<SupportBean>>(treeMapFirst), @event.Get("fe"));
                        ClassicAssert.AreEqual(FirstEvent<IList<SupportBean>>(treeMapFirst), @event.Get("minb"));
                        EPAssertionUtil.AssertEqualsExactOrder(
                            AllEvents<IList<SupportBean>>(treeMapFirst),
                            (SupportBean[])@event.Get("fes"));
                        ClassicAssert.AreEqual(treeMapFirst.Key, @event.Get("fk"));

                        var treeMapLast = treemap.Last();
                        ClassicAssert.AreEqual(FirstEvent<IList<SupportBean>>(treeMapLast), @event.Get("le"));
                        ClassicAssert.AreEqual(FirstEvent<IList<SupportBean>>(treeMapLast), @event.Get("maxb"));
                        EPAssertionUtil.AssertEqualsExactOrder(
                            AllEvents<IList<SupportBean>>(treeMapLast),
                            (SupportBean[])@event.Get("les"));
                        ClassicAssert.AreEqual(treeMapLast.Key, @event.Get("lk"));
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateSortedCFHLEnumerationAndDot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create table MyTable(sortcol sorted(IntPrimitive) @type('SupportBean'));\n" +
                          "into table MyTable select sorted(*) as sortcol from SupportBean;\n" +
                          "@name('s0') select " +
                          "MyTable.sortcol.ceilingEvent(Id).TheString as ceid," +
                          "MyTable.sortcol.ceilingEvent(Id).firstOf() as cefo," +
                          "MyTable.sortcol.ceilingEvents(Id).lastOf() as ceslo," +
                          "MyTable.sortcol.floorEvent(Id).TheString as feid," +
                          "MyTable.sortcol.floorEvent(Id).firstOf() as fefo," +
                          "MyTable.sortcol.floorEvents(Id).lastOf() as feslo," +
                          "MyTable.sortcol.higherEvent(Id).TheString as heid," +
                          "MyTable.sortcol.higherEvent(Id).firstOf() as hefo," +
                          "MyTable.sortcol.higherEvents(Id).lastOf() as heslo," +
                          "MyTable.sortcol.lowerEvent(Id).TheString as leid," +
                          "MyTable.sortcol.lowerEvent(Id).firstOf() as lefo," +
                          "MyTable.sortcol.lowerEvents(Id).lastOf() as leslo " +
                          " from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                AssertType(env, typeof(string), "ceid,feid,heid,leid");
                AssertType(env, typeof(SupportBean), "cefo,fefo,hefo,lefo,ceslo,feslo,heslo,leslo");

                var treemap = new OrderedListDictionary<int, IList<SupportBean>>();
                PrepareTestData(env, treemap); // 1, 1, 4, 6, 6, 8, 9

                env.Milestone(0);

                for (var i = 0; i < 12; i++) {
                    env.SendEventBean(new SupportBean_S0(i));
                    var index = i;
                    env.AssertEventNew(
                        "s0",
                        @event => {
                            var message = "failed at " + index;
                            ClassicAssert.AreEqual(FirstEventString(treemap.GreaterThanOrEqualTo(index)), @event.Get("ceid"), message);
                            ClassicAssert.AreEqual(FirstEvent(treemap.GreaterThanOrEqualTo(index)), @event.Get("cefo"), message);
                            ClassicAssert.AreEqual(LastEvent(treemap.GreaterThanOrEqualTo(index)), @event.Get("ceslo"), message);
                            ClassicAssert.AreEqual(FirstEventString(treemap.LessThanOrEqualTo(index)), @event.Get("feid"), message);
                            ClassicAssert.AreEqual(FirstEvent(treemap.LessThanOrEqualTo(index)), @event.Get("fefo"), message);
                            ClassicAssert.AreEqual(LastEvent(treemap.LessThanOrEqualTo(index)), @event.Get("feslo"), message);
                            ClassicAssert.AreEqual(FirstEventString(treemap.GreaterThan(index)), @event.Get("heid"), message);
                            ClassicAssert.AreEqual(FirstEvent(treemap.GreaterThan(index)), @event.Get("hefo"), message);
                            ClassicAssert.AreEqual(LastEvent(treemap.GreaterThan(index)), @event.Get("heslo"), message);
                            ClassicAssert.AreEqual(FirstEventString(treemap.LessThan(index)), @event.Get("leid"), message);
                            ClassicAssert.AreEqual(FirstEvent(treemap.LessThan(index)), @event.Get("lefo"), message);
                            ClassicAssert.AreEqual(LastEvent(treemap.LessThan(index)), @event.Get("leslo"), message);
                        });
                }

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateSortedCFHL : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create table MyTable(sortcol sorted(IntPrimitive) @type('SupportBean'));\n" +
                    "into table MyTable select sorted(*) as sortcol from SupportBean;\n";
                env.CompileDeploy(epl, path);

                var select = "@name('s0') select " +
                             "MyTable.sortcol as sortedItself, " +
                             "MyTable.sortcol.ceilingEvent(Id) as ce, " +
                             "MyTable.sortcol.ceilingEvents(Id) as ces, " +
                             "MyTable.sortcol.ceilingKey(Id) as ck, " +
                             "MyTable.sortcol.floorEvent(Id) as fe, " +
                             "MyTable.sortcol.floorEvents(Id) as fes, " +
                             "MyTable.sortcol.floorKey(Id) as fk, " +
                             "MyTable.sortcol.higherEvent(Id) as he, " +
                             "MyTable.sortcol.higherEvents(Id) as hes, " +
                             "MyTable.sortcol.higherKey(Id) as hk, " +
                             "MyTable.sortcol.lowerEvent(Id) as le, " +
                             "MyTable.sortcol.lowerEvents(Id) as les, " +
                             "MyTable.sortcol.lowerKey(Id) as lk" +
                             " from SupportBean_S0";
                env.EplToModelCompileDeploy(select, path).AddListener("s0");

                AssertType(env, typeof(SupportBean), "ce,fe,he,le");
                AssertType(env, typeof(SupportBean[]), "ces,fes,hes,les");
                AssertType(env, typeof(int?), "ck,fk,hk,lk");

                var treemap = new OrderedListDictionary<int, IList<SupportBean>>();
                PrepareTestData(env, treemap); // 1, 1, 4, 6, 6, 8, 9

                env.Milestone(0);

                for (var i = 0; i < 12; i++) {
                    env.SendEventBean(new SupportBean_S0(i));
                    var index = i;
                    env.AssertEventNew(
                        "s0",
                        @event => {
                            ClassicAssert.AreEqual(FirstEvent(treemap.GreaterThanOrEqualTo(index)), @event.Get("ce"));
                            EPAssertionUtil.AssertEqualsExactOrder(AllEvents(treemap.GreaterThanOrEqualTo(index)), (SupportBean[])@event.Get("ces"));
                            ClassicAssert.AreEqual(treemap.GreaterThanOrEqualTo(index)?.Key, @event.Get("ck"));
                            ClassicAssert.AreEqual(FirstEvent(treemap.LessThanOrEqualTo(index)), @event.Get("fe"));
                            EPAssertionUtil.AssertEqualsExactOrder(AllEvents(treemap.LessThanOrEqualTo(index)), (SupportBean[])@event.Get("fes"));
                            ClassicAssert.AreEqual(treemap.LessThanOrEqualTo(index)?.Key, @event.Get("fk"));
                            ClassicAssert.AreEqual(FirstEvent(treemap.GreaterThan(index)), @event.Get("he"));
                            EPAssertionUtil.AssertEqualsExactOrder(AllEvents(treemap.GreaterThan(index)), (SupportBean[])@event.Get("hes"));
                            ClassicAssert.AreEqual(treemap.GreaterThan(index)?.Key, @event.Get("hk"));
                            ClassicAssert.AreEqual(FirstEvent(treemap.LessThan(index)), @event.Get("le"));
                            EPAssertionUtil.AssertEqualsExactOrder(AllEvents(treemap.LessThan(index)), (SupportBean[])@event.Get("les"));
                            ClassicAssert.AreEqual(treemap.LessThan(index)?.Key, @event.Get("lk"));
                        });
                }

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateSortedNonTable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();
				var treemap = new OrderedListDictionary<int, IList<SupportBean>>();

                var epl =
                    "@name('s0') select sorted(IntPrimitive).floorEvent(IntPrimitive-1) as c0 from SupportBean#length(3) as sb";
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                MakeSendBean(env, treemap, "E1", 10);
                env.AssertPropsNew("s0", fields, new object[] { LessThanOrEqualToFirstEvent(treemap, 10 - 1) });

                MakeSendBean(env, treemap, "E2", 20);
                env.AssertPropsNew("s0", fields, new object[] { LessThanOrEqualToFirstEvent(treemap, 20 - 1) });

                env.Milestone(0);

                MakeSendBean(env, treemap, "E3", 15);
                env.AssertPropsNew("s0", fields, new object[] { LessThanOrEqualToFirstEvent(treemap, 15 - 1) });

                MakeSendBean(env, treemap, "E3", 17);
                env.AssertPropsNew("s0", fields, new object[] { LessThanOrEqualToFirstEvent(treemap, 17 - 1) });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateSortedTableAccess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create table MyTable(sortcol sorted(IntPrimitive) @type('SupportBean'));\n" +
                          "into table MyTable select sorted(*) as sortcol from SupportBean;\n" +
                          "@name('s0') select MyTable.sortcol.floorEvent(Id) as c0 from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                var treemap = new OrderedListDictionary<int, IList<SupportBean>>();
                MakeSendBean(env, treemap, "E1", 10);
                MakeSendBean(env, treemap, "E2", 20);
                MakeSendBean(env, treemap, "E3", 30);

                env.SendEventBean(new SupportBean_S0(15));
                env.AssertEventNew(
                    "s0",
                    @event => ClassicAssert.AreEqual(LessThanOrEqualToFirstEvent(treemap, 15), @event.Get("c0")));

                env.Milestone(0);

                for (var i = 0; i < 40; i++) {
                    env.SendEventBean(new SupportBean_S0(i));
                    var index = i;
                    env.AssertEventNew(
                        "s0",
                        @event => ClassicAssert.AreEqual(LessThanOrEqualToFirstEvent(treemap, index), @event.Get("c0")));
                }

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateSortedTableIdent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@public create table MyTable(sortcol sorted(IntPrimitive) @type('SupportBean'));\n" +
                    "into table MyTable select sorted(*) as sortcol from SupportBean;\n";
                env.CompileDeploy(epl, path);

                env.EplToModelCompileDeploy(
                        "@name('s0') select sortcol.floorEvent(Id) as c0 from SupportBean_S0, MyTable",
                        path)
                    .AddListener("s0");

                var treemap = new OrderedListDictionary<int, IList<SupportBean>>();
                MakeSendBean(env, treemap, "E1", 10);
                MakeSendBean(env, treemap, "E2", 20);
                MakeSendBean(env, treemap, "E3", 30);

                env.Milestone(0);

                for (var i = 0; i < 40; i++) {
                    env.SendEventBean(new SupportBean_S0(i));
                    var index = i;
                    env.AssertEventNew(
                        "s0",
                        @event => ClassicAssert.AreEqual(LessThanOrEqualToFirstEvent(treemap, index), @event.Get("c0")));
                }

                env.UndeployAll();
            }
        }

        private static SupportBean FirstEvent<T>(KeyValuePair<int, T>? entry)
            where T : ICollection<SupportBean>
        {
            return entry?.Value.First();
        }

        private static string FirstEventString<T>(KeyValuePair<int, T>? entry)
            where T : ICollection<SupportBean>
        {
            return entry?.Value.First().TheString;
        }

        private static string FirstEventString(IList<SupportBean> list)
        {
            return list?[0].TheString;
        }

        private static SupportBean[] AllEvents<T>(KeyValuePair<int, T>? entry)
            where T : ICollection<SupportBean>
        {
            return entry?.Value.ToArray();
        }

        private static SupportBean[] AllEvents(IList<SupportBean> list)
        {
            return list?.ToArray();
        }

        private static SupportBean LastEvent<T>(KeyValuePair<int, T>? entry)
            where T : ICollection<SupportBean>
        {
            return entry?.Value.Last();
        }

        private static SupportBean LastEvent(IList<SupportBean> list)
        {
            return list?[^1];
        }

        private static SupportBean FirstEvent(IList<SupportBean> list)
        {
            return list?[0];
        }

        private static void MakeSendBean(
            RegressionEnvironment env,
            IOrderedDictionary<int, IList<SupportBean>> treemap,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            env.SendEventBean(bean);
            var existing = treemap.Get(intPrimitive);
            if (existing == null) {
                existing = new List<SupportBean>();
                treemap.Put(intPrimitive, existing);
            }

            existing.Add(bean);
            treemap.Put(bean.IntPrimitive, existing);
        }

        private static SupportBean LessThanOrEqualToFirstEvent(
            IOrderedDictionary<int, IList<SupportBean>> treemap,
            int key)
        {
            return treemap.LessThanOrEqualTo(key)?.Value.First();
        }

        private static void PrepareTestData(
            RegressionEnvironment env,
            IOrderedDictionary<int, IList<SupportBean>> treemap)
        {
            MakeSendBean(env, treemap, "E1a", 1);
            MakeSendBean(env, treemap, "E1b", 1);
            MakeSendBean(env, treemap, "E4b", 4);
            MakeSendBean(env, treemap, "E6a", 6);
            MakeSendBean(env, treemap, "E6b", 6);
            MakeSendBean(env, treemap, "E8", 8);
            MakeSendBean(env, treemap, "E9", 9);
        }

        internal static void AssertType(
            RegressionEnvironment env,
            Type expected,
            string csvProps)
        {
            var props = csvProps.SplitCsv();
            env.AssertStatement(
                "s0",
                statement => {
                    var eventType = statement.EventType;
                    foreach (var prop in props) {
                        ClassicAssert.AreEqual(expected, eventType.GetPropertyType(prop), "failed for prop '" + prop + "'");
                    }
                });
        }

        private static void AssertEventsBetween<T>(
            IOrderedDictionary<int, T> treemap,
            MySubmapEvent sme,
            SupportBean[] events,
            SupportBean lastOf)
            where T : ICollection<SupportBean>
        {
            var submap = treemap.Between(sme.FromKey, sme.IsFromInclusive, sme.ToKey, sme.IsToInclusive);
            var all = new List<SupportBean>();
            foreach (var entry in submap) {
                all.AddAll(entry.Value);
            }

            EPAssertionUtil.AssertEqualsExactOrder(all.ToArray(), events);
            if (all.IsEmpty()) {
                ClassicAssert.IsNull(lastOf);
            }
            else {
                ClassicAssert.AreEqual(all[^1], lastOf);
            }
        }

        private static void AssertSubmap<T>(
            IOrderedDictionary<int, T> treemap,
            MySubmapEvent sme,
            IDictionary<object, SupportBean[]> actual)
            where T : ICollection<SupportBean>
        {
            var expected = treemap.Between(
                sme.FromKey,
                sme.IsFromInclusive,
                sme.ToKey,
                sme.IsToInclusive);
            ClassicAssert.AreEqual(expected.Count, actual.Count);
            foreach (var key in expected.Keys) {
                var expectedEvents = expected.Get(key).ToArray();
                var actualEvents = actual.Get(key);
                EPAssertionUtil.AssertEqualsExactOrder(expectedEvents, actualEvents);
            }
        }

        private static void AssertOrderedDictionary(
            IOrderedDictionary<int, IList<SupportBean>> treemap,
            IOrderedDictionary<object, ICollection<EventBean>> actual)
        {
            ClassicAssert.AreEqual(treemap.Count, actual.Count);
            foreach (var key in treemap.Keys) {
                var expectedEvents = treemap.Get(key).ToArray();
                EPAssertionUtil.AssertEqualsExactOrder(expectedEvents, ToArrayOfUnderlying(actual.Get(key)));
            }

            CompareEntry(treemap.First(), actual.FirstEntry);
            CompareEntry(treemap.Last(), actual.LastEntry);
            CompareEntry(treemap.LessThanOrEqualTo(5), actual.LessThanOrEqualTo(5));
            CompareEntry(treemap.GreaterThanOrEqualTo(5), actual.GreaterThanOrEqualTo(5));
            CompareEntry(treemap.LessThan(5), actual.LessThan(5));
            CompareEntry(treemap.GreaterThan(5), actual.GreaterThan(5));

            ClassicAssert.AreEqual(treemap.Keys.First(), actual.FirstEntry.Key);
            ClassicAssert.AreEqual(treemap.Keys.Last(), actual.LastEntry.Key);
            ClassicAssert.AreEqual(treemap.LessThanOrEqualTo(5)?.Key, actual.LessThanOrEqualTo(5)?.Key);
            ClassicAssert.AreEqual(treemap.GreaterThanOrEqualTo(5)?.Key, actual.GreaterThanOrEqualTo(5)?.Key);
            ClassicAssert.AreEqual(treemap.LessThan(5)?.Key, actual.LessThan(5)?.Key);
            ClassicAssert.AreEqual(treemap.GreaterThan(5)?.Key, actual.GreaterThan(5)?.Key);

            ClassicAssert.AreEqual(treemap.ContainsKey(5), actual.ContainsKey(5));
            ClassicAssert.AreEqual(treemap.IsEmpty(), actual.IsEmpty());

            EPAssertionUtil.AssertEqualsExactOrder(new object[] { 1, 4, 6, 8, 9 }, actual.Keys.ToArray());

            ClassicAssert.AreEqual(1, actual.Between(9, true, 9, true).Count);
            ClassicAssert.AreEqual(1, actual.Tail(9).Count);
            ClassicAssert.AreEqual(1, actual.Tail(9, true).Count);
            ClassicAssert.AreEqual(1, actual.Head(2).Count);
            ClassicAssert.AreEqual(1, actual.Head(2, false).Count);

            ClassicAssert.AreEqual(5, actual.Count);
            ClassicAssert.AreEqual(5, actual.Values.Count);

            // values
            var values = actual.Values;
            Assert.That(values.Count, Is.EqualTo(5));
            Assert.That(values.IsEmpty(), Is.False);

            var valuesEnum = values.GetEnumerator();
            Assert.That(valuesEnum, Is.Not.Null);
            Assert.That(valuesEnum.MoveNext, Is.True);

            CollectionAssert.AreEqual(
                treemap.Get(1).ToArray(),
                ToArrayOfUnderlying(valuesEnum.Current));

            Assert.That(valuesEnum.MoveNext, Is.True);
            Assert.That(values.ToArray(), Has.Length.EqualTo(5));

            CollectionAssert.AreEqual(
                treemap.Get(1).ToArray(),
                ToArrayOfUnderlying((ICollection<EventBean>)values.ToArray()[0]));

            // ordered key set
            var oks = actual.OrderedKeys;

            //Assert.That(oks.Comparator());
            Assert.That(oks.FirstEntry, Is.EqualTo(1));
            Assert.That(oks.LastEntry, Is.EqualTo(9));
            Assert.That(oks.Count, Is.EqualTo(5));
            Assert.That(oks.IsEmpty(), Is.False);
            Assert.That(oks.Contains(6), Is.True);
            Assert.That(oks.ToArray(), Is.Not.Null);

            Assert.That(oks.LessThan(5), Is.EqualTo(4));
            Assert.That(oks.GreaterThan(5), Is.EqualTo(6));
            Assert.That(oks.LessThanOrEqualTo(5), Is.EqualTo(4));
            Assert.That(oks.GreaterThanOrEqualTo(5), Is.EqualTo(6));

            Assert.That(oks.Between(1, true, 100, true), Is.Not.Null);
            Assert.That(oks.Head(100, true), Is.Not.Null);
            Assert.That(oks.Tail(1, true), Is.Not.Null);

            // ordered key set - enumerator
            var oksit = oks.GetEnumerator();
            Assert.That(oksit, Is.Not.Null);
            Assert.That(oksit.MoveNext(), Is.True);
            Assert.That(oksit.Current, Is.EqualTo(1));
            Assert.That(oksit.MoveNext(), Is.True);

            // entry set
            ICollection<KeyValuePair<object, ICollection<EventBean>>> set = actual;
            ClassicAssert.IsFalse(set.IsEmpty());
            var setit = set.GetEnumerator();
            var entry = setit.Advance();
            ClassicAssert.AreEqual(1, entry.Key);
            ClassicAssert.IsTrue(setit.MoveNext());
            EPAssertionUtil.AssertEqualsExactOrder(treemap.Get(1).ToArray(), ToArrayOfUnderlying(entry.Value));
            var array = set.ToArray();
            ClassicAssert.AreEqual(5, array.Length);
            ClassicAssert.AreEqual(1, array[0].Key);
            EPAssertionUtil.AssertEqualsExactOrder(treemap.Get(1).ToArray(), ToArrayOfUnderlying(array[0].Value));
            ClassicAssert.IsNotNull(set.ToArray());

            // sorted map
            var events = actual.Head(100);
            ClassicAssert.AreEqual(5, events.Count);
        }

        private static void CompareEntry(
            KeyValuePair<int, IList<SupportBean>>? expected,
            KeyValuePair<object, ICollection<EventBean>>? actual)
        {
            Assert.That(expected, Is.Not.Null);
            Assert.That(actual, Is.Not.Null);
            ClassicAssert.AreEqual(expected.Value.Key, actual.Value.Key);
            EPAssertionUtil.AssertEqualsExactOrder(
                expected.Value.Value.ToArray(),
                ToArrayOfUnderlying(actual.Value.Value));
        }

        private static SupportBean[] ToArrayOfUnderlying(ICollection<EventBean> eventBeans)
        {
            var events = new SupportBean[eventBeans.Count];
            var index = 0;
            foreach (var @event in eventBeans) {
                events[index++] = (SupportBean)@event.Underlying;
            }

            return events;
        }

        private static void CompareKeys(
            object key,
            params object[] keys)
        {
            EPAssertionUtil.AssertEqualsExactOrder(((HashableMultiKey)key).Keys, keys);
        }
    }
} // end of namespace