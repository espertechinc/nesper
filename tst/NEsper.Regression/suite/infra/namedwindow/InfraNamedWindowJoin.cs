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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.IndexBackingTableInfo;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    /// NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowJoin : IndexBackingTableInfo
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InfraNamedWindowJoin));

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithJoinIndexChoice(execs);
            WithRightOuterJoinLateStart(execs);
            WithFullOuterJoinNamedAggregationLateStart(execs);
            WithJoinNamedAndStream(execs);
            WithJoinBetweenNamed(execs);
            WithJoinBetweenSameNamed(execs);
            WithJoinSingleInsertOneWindow(execs);
            WithUnidirectional(execs);
            WithWindowUnidirectionalJoin(execs);
            WithInnerJoinLateStart(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInnerJoinLateStart(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInnerJoinLateStart());
            return execs;
        }

        public static IList<RegressionExecution> WithWindowUnidirectionalJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraWindowUnidirectionalJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithUnidirectional(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraUnidirectional());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinSingleInsertOneWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraJoinSingleInsertOneWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinBetweenSameNamed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraJoinBetweenSameNamed());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinBetweenNamed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraJoinBetweenNamed());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinNamedAndStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraJoinNamedAndStream());
            return execs;
        }

        public static IList<RegressionExecution> WithFullOuterJoinNamedAggregationLateStart(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFullOuterJoinNamedAggregationLateStart());
            return execs;
        }

        public static IList<RegressionExecution> WithRightOuterJoinLateStart(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraRightOuterJoinLateStart());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinIndexChoice(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraJoinIndexChoice());
            return execs;
        }

        private class InfraWindowUnidirectionalJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@public create window MyWindowWUJ#keepall as SupportBean;\n" +
                          "insert into MyWindowWUJ select * from SupportBean;\n" +
                          "on SupportBean_S1 as s1 delete from MyWindowWUJ where s1.p10 = theString;\n" +
                          "@name('s0') select window(win.*) as c0," +
                          "window(win.*).where(v => v.intPrimitive < 2) as c1, " +
                          "window(win.*).toMap(k=>k.theString,v=>v.intPrimitive) as c2 " +
                          "from SupportBean_S0 as s0 unidirectional, MyWindowWUJ as win";
                env.CompileDeploy(epl, path).AddListener("s0");

                var beans = new SupportBean[3];
                for (var i = 0; i < beans.Length; i++) {
                    beans[i] = new SupportBean("E" + i, i);
                }

                env.SendEventBean(beans[0]);
                env.SendEventBean(beans[1]);
                env.SendEventBean(new SupportBean_S0(10));
                AssertReceived(
                    env,
                    beans,
                    new int[] { 0, 1 },
                    new int[] { 0, 1 },
                    "E0,E1".SplitCsv(),
                    new object[] { 0, 1 });

                // add bean
                env.SendEventBean(beans[2]);
                env.SendEventBean(new SupportBean_S0(10));
                AssertReceived(
                    env,
                    beans,
                    new int[] { 0, 1, 2 },
                    new int[] { 0, 1 },
                    "E0,E1,E2".SplitCsv(),
                    new object[] { 0, 1, 2 });

                // delete bean
                env.SendEventBean(new SupportBean_S1(11, "E1"));
                env.SendEventBean(new SupportBean_S0(12));
                AssertReceived(
                    env,
                    beans,
                    new int[] { 0, 2 },
                    new int[] { 0 },
                    "E0,E2".SplitCsv(),
                    new object[] { 0, 2 });

                // delete another bean
                env.SendEventBean(new SupportBean_S1(13, "E0"));
                env.SendEventBean(new SupportBean_S0(14));
                AssertReceived(env, beans, new int[] { 2 }, Array.Empty<int>(), "E2".SplitCsv(), new object[] { 2 });

                // delete last bean
                env.SendEventBean(new SupportBean_S1(15, "E2"));
                env.SendEventBean(new SupportBean_S0(16));
                env.AssertListenerNotInvoked("s0");

                // compile a non-unidirectional query, join and subquery
                env.CompileDeploy("select window(win.*) from MyWindowWUJ as win", path);
                env.CompileDeploy(
                    "select window(win.*) as c0 from SupportBean_S0#lastevent as s0, MyWindowWUJ as win",
                    path);
                env.CompileDeploy("select (select window(win.*) from MyWindowWUJ as win) from SupportBean_S0", path);

                env.UndeployAll();
            }
        }

        private class InfraJoinIndexChoice : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var preloadedEventsOne = new object[]
                    { new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22) };
                IndexAssertionEventSend eventSendAssertion = () => {
                    var fields = "ssb2.s2,ssb1.s1,ssb1.i1".SplitCsv();
                    env.SendEventBean(new SupportSimpleBeanTwo("E2", 50, 21, 22));
                    env.AssertPropsNew("s0", fields, new object[] { "E2", "E2", 20 });
                    env.SendEventBean(new SupportSimpleBeanTwo("E1", 60, 11, 12));
                    env.AssertPropsNew("s0", fields, new object[] { "E1", "E1", 10 });
                };

                // no index, since this is "unique(s1)" we don't need one
                var noindexes = new string[] { };
                AssertIndexChoice(
                    env,
                    noindexes,
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new IndexAssertion[] {
                        new IndexAssertion(null, "s1 = s2", true, eventSendAssertion),
                        new IndexAssertion(null, "s1 = s2 and l1 = l2", true, eventSendAssertion),
                    });

                // single index one field (duplicate in essence, since "unique(s1)"
                var indexOneField = new string[] { "create unique index One on MyWindow (s1)" };
                AssertIndexChoice(
                    env,
                    indexOneField,
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new IndexAssertion[] {
                        new IndexAssertion(null, "s1 = s2", true, eventSendAssertion),
                        new IndexAssertion(null, "s1 = s2 and l1 = l2", true, eventSendAssertion),
                    });

                // single index two field (includes "unique(s1)")
                var indexTwoField = new string[] { "create unique index One on MyWindow (s1, l1)" };
                AssertIndexChoice(
                    env,
                    indexTwoField,
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new IndexAssertion[] {
                        new IndexAssertion(null, "s1 = s2", true, eventSendAssertion),
                        new IndexAssertion(null, "d1 = d2", false, eventSendAssertion),
                        new IndexAssertion(null, "s1 = s2 and l1 = l2", true, eventSendAssertion),
                    });

                // two index one unique ("unique(s1)")
                var indexSetTwo = new string[] {
                    "create index One on MyWindow (s1)",
                    "create unique index Two on MyWindow (s1, d1)"
                };
                AssertIndexChoice(
                    env,
                    indexSetTwo,
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new IndexAssertion[] {
                        new IndexAssertion(null, "d1 = d2", false, eventSendAssertion),
                        new IndexAssertion(null, "s1 = s2", true, eventSendAssertion),
                        new IndexAssertion(null, "s1 = s2 and l1 = l2", true, eventSendAssertion),
                        new IndexAssertion(null, "s1 = s2 and d1 = d2 and l1 = l2", true, eventSendAssertion),
                    });

                // two index one unique ("win:keepall()")
                AssertIndexChoice(
                    env,
                    indexSetTwo,
                    preloadedEventsOne,
                    "win:keepall()",
                    new IndexAssertion[] {
                        new IndexAssertion(null, "d1 = d2", false, eventSendAssertion),
                        new IndexAssertion(null, "s1 = s2", false, eventSendAssertion),
                        new IndexAssertion(null, "s1 = s2 and l1 = l2", false, eventSendAssertion),
                        new IndexAssertion(null, "s1 = s2 and d1 = d2 and l1 = l2", true, eventSendAssertion),
                        new IndexAssertion(null, "d1 = d2 and s1 = s2", true, eventSendAssertion),
                    });
            }

            private static void AssertIndexChoice(
                RegressionEnvironment env,
                string[] indexes,
                object[] preloadedEvents,
                string datawindow,
                params IndexAssertion[] assertions)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyWindow." + datawindow + " as SupportSimpleBeanOne", path);
                env.CompileDeploy("insert into MyWindow select * from SupportSimpleBeanOne", path);
                foreach (var index in indexes) {
                    env.CompileDeploy(index, path);
                }

                foreach (var @event in preloadedEvents) {
                    env.SendEventBean(@event);
                }

                var count = 0;
                foreach (var assertion in assertions) {
                    log.Info("======= Testing #" + count);
                    count++;

                    var epl = INDEX_CALLBACK_HOOK +
                              (assertion.Hint == null ? "" : assertion.Hint) +
                              "select * " +
                              "from SupportSimpleBeanTwo as ssb2 unidirectional, MyWindow as ssb1 " +
                              "where " +
                              assertion.WhereClause;

                    try {
                        env.CompileDeploy("@name('s0')" + epl, path).AddListener("s0");
                    }
                    catch (Exception ex) {
                        if (assertion.EventSendAssertion == null) {
                            // no assertion, expected
                            Assert.IsTrue(ex.Message.Contains("index hint busted"));
                            continue;
                        }

                        throw new EPRuntimeException("Unexpected statement exception: " + ex.Message, ex);
                    }

                    // assert index and access
                    SupportQueryPlanIndexHook.AssertJoinOneStreamAndReset(assertion.Unique);
                    assertion.EventSendAssertion.Invoke();
                    env.UndeployModuleContaining("s0");
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        private class InfraInnerJoinLateStart : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionInnerJoinLateStart(env, rep);
                }
            }

            private static void TryAssertionInnerJoinLateStart(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum)
            {
                var schemaEPL =
                    eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedProduct)) +
                    "@name('schema') @public @buseventtype create schema Product (product string, size int);\n" +
                    eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedPortfolio)) +
                    " @public @buseventtype create schema Portfolio (portfolio string, product string);\n";
                var path = new RegressionPath();
                env.CompileDeploy(schemaEPL, path);

                env.CompileDeploy("@name('window') @public create window ProductWin#keepall as Product", path);

                env.AssertStatement(
                    "schema",
                    statement =>
                        Assert.IsTrue(eventRepresentationEnum.MatchesClass(statement.EventType.UnderlyingType)));
                env.AssertStatement(
                    "window",
                    statement =>
                        Assert.IsTrue(eventRepresentationEnum.MatchesClass(statement.EventType.UnderlyingType)));

                env.CompileDeploy("insert into ProductWin select * from Product", path);
                env.CompileDeploy("@public create window PortfolioWin#keepall as Portfolio", path);
                env.CompileDeploy("insert into PortfolioWin select * from Portfolio", path);

                SendProduct(env, eventRepresentationEnum, "productA", 1);
                SendProduct(env, eventRepresentationEnum, "productB", 2);
                SendPortfolio(env, eventRepresentationEnum, "Portfolio", "productA");

                var stmtText = "@name(\"Query2\") select portfolio, ProductWin.product, size " +
                               "from PortfolioWin unidirectional inner join ProductWin on PortfolioWin.product=ProductWin.product";
                env.CompileDeploy(stmtText, path).AddListener("Query2");

                SendPortfolio(env, eventRepresentationEnum, "Portfolio", "productB");
                env.AssertPropsNew(
                    "Query2",
                    new string[] { "portfolio", "ProductWin.product", "size" },
                    new object[] { "Portfolio", "productB", 2 });

                SendPortfolio(env, eventRepresentationEnum, "Portfolio", "productC");
                env.ListenerReset("Query2");

                SendProduct(env, eventRepresentationEnum, "productC", 3);
                SendPortfolio(env, eventRepresentationEnum, "Portfolio", "productC");
                env.AssertPropsNew(
                    "Query2",
                    new string[] { "portfolio", "ProductWin.product", "size" },
                    new object[] { "Portfolio", "productC", 3 });

                env.UndeployAll();
            }

            private static void SendProduct(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum,
                string product,
                int size)
            {
                if (eventRepresentationEnum.IsObjectArrayEvent()) {
                    env.SendEventObjectArray(new object[] { product, size }, "Product");
                }
                else if (eventRepresentationEnum.IsMapEvent()) {
                    IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
                    theEvent.Put("product", product);
                    theEvent.Put("size", size);
                    env.SendEventMap(theEvent, "Product");
                }
                else if (eventRepresentationEnum.IsAvroEvent()) {
                    var schema = env.RuntimeAvroSchemaPreconfigured("Product");
                    var theEvent = new GenericRecord(schema.AsRecordSchema());
                    theEvent.Put("product", product);
                    theEvent.Put("size", size);
                    env.SendEventAvro(theEvent, "Product");
                }
                else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                    var @object = new JObject();
                    @object.Add("product", product);
                    @object.Add("size", size);
                    env.SendEventJson(@object.ToString(), "Product");
                }
                else {
                    Assert.Fail();
                }
            }

            private static void SendPortfolio(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum,
                string portfolio,
                string product)
            {
                if (eventRepresentationEnum.IsObjectArrayEvent()) {
                    env.SendEventObjectArray(new object[] { portfolio, product }, "Portfolio");
                }
                else if (eventRepresentationEnum.IsMapEvent()) {
                    IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
                    theEvent.Put("portfolio", portfolio);
                    theEvent.Put("product", product);
                    env.SendEventMap(theEvent, "Portfolio");
                }
                else if (eventRepresentationEnum.IsAvroEvent()) {
                    var theEvent =
                        new GenericRecord(env.RuntimeAvroSchemaPreconfigured("Portfolio").AsRecordSchema());
                    theEvent.Put("portfolio", portfolio);
                    theEvent.Put("product", product);
                    env.SendEventAvro(theEvent, "Portfolio");
                }
                else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                    var @object = new JObject();
                    @object.Add("portfolio", portfolio);
                    @object.Add("product", product);
                    env.SendEventJson(@object.ToString(), "Portfolio");
                }
                else {
                    Assert.Fail();
                }
            }
        }

        private class InfraRightOuterJoinLateStart : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test for ESPER-186 Iterator not honoring order by clause for grouped join query with output-rate clause
                // Test for ESPER-187 Join of two or more named windows on late start may not return correct aggregation state on iterate

                var path = new RegressionPath();

                // create window for Leave events
                var epl =
                    "@public create window WindowLeave#time(6000) as select timeLeave, id, location from SupportQueueLeave;\n" +
                    "insert into WindowLeave select timeLeave, id, location from SupportQueueLeave;\n";
                env.CompileDeploy(epl, path);

                // create second window for enter events
                epl =
                    "@public create window WindowEnter#time(6000) as select location, sku, timeEnter, id from SupportQueueEnter;\n" +
                    "insert into WindowEnter select location, sku, timeEnter, id from SupportQueueEnter;\n";
                env.CompileDeploy(epl, path);

                // fill data
                for (var i = 0; i < 8; i++) {
                    var location = (i / 2).ToString();
                    env.SendEventBean(new SupportQueueLeave(i + 1, location, 247));
                }

                // Console.WriteLine("Leave events:");
                // {
                // EventBean event = it.next();
                // Console.WriteLine(event.get("timeLeave") +
                // " " + event.get("id") +
                // " " + event.get("location"));
                // }

                for (var i = 0; i < 10; i++) {
                    var location = (i / 2).ToString();
                    var sku = (i % 2 == 0) ? "166583" : "169254";
                    env.SendEventBean(new SupportQueueEnter(i + 1, location, sku, 123));
                }

                // Console.WriteLine("Enter events:");
                // {
                // EventBean event = it.next();
                // Console.WriteLine(event.get("timeEnter") +
                // " " + event.get("id") +
                // " " + event.get("sku") +
                // " " + event.get("location"));
                // }

                var stmtTextOne =
                    "@name('s1') select s1.location as loc, sku, avg((coalesce(timeLeave, 250) - timeEnter)) as avgTime, " +
                    "count(timeEnter) as cntEnter, count(timeLeave) as cntLeave, (count(timeEnter) - count(timeLeave)) as diff " +
                    "from WindowLeave as s0 right outer join WindowEnter as s1 " +
                    "on s0.id = s1.id and s0.location = s1.location " +
                    "group by s1.location, sku " +
                    "output every 1.0 seconds " +
                    "order by s1.location, sku";
                env.CompileDeploy(stmtTextOne, path);

                var stmtTextTwo =
                    "@name('s2') select s1.location as loc, sku, avg((coalesce(timeLeave, 250) - timeEnter)) as avgTime, " +
                    "count(timeEnter) as cntEnter, count(timeLeave) as cntLeave, (count(timeEnter) - count(timeLeave)) as diff " +
                    "from WindowEnter as s1 left outer join WindowLeave as s0 " +
                    "on s0.id = s1.id and s0.location = s1.location " +
                    "group by s1.location, sku " +
                    "output every 1.0 seconds " +
                    "order by s1.location, sku";
                env.CompileDeploy(stmtTextTwo, path);

                // Console.WriteLine("Statement 1");
                // {
                // EventBean event = it.next();
                // Console.WriteLine("loc " + event.get("loc") +
                // " sku " + event.get("sku") +
                // " avgTime " + event.get("avgTime") +
                // " cntEnter " + event.get("cntEnter") +
                // " cntLeave " + event.get("cntLeave") +
                // " diff " + event.get("diff"));
                // }

                var expected = new object[][] {
                    new object[] { "0", "166583", 124.0, 1L, 1L, 0L },
                    new object[] { "0", "169254", 124.0, 1L, 1L, 0L },
                    new object[] { "1", "166583", 124.0, 1L, 1L, 0L },
                    new object[] { "1", "169254", 124.0, 1L, 1L, 0L },
                    new object[] { "2", "166583", 124.0, 1L, 1L, 0L },
                    new object[] { "2", "169254", 124.0, 1L, 1L, 0L },
                    new object[] { "3", "166583", 124.0, 1L, 1L, 0L },
                    new object[] { "3", "169254", 124.0, 1L, 1L, 0L },
                    new object[] { "4", "166583", 127.0, 1L, 0L, 1L },
                    new object[] { "4", "169254", 127.0, 1L, 0L, 1L }
                };

                // assert iterator results
                env.AssertIterator(
                    "s2",
                    iterator => {
                        var received = EPAssertionUtil.EnumeratorToArray(iterator);
                        EPAssertionUtil.AssertPropsPerRow(
                            received,
                            "loc,sku,avgTime,cntEnter,cntLeave,diff".SplitCsv(),
                            expected);
                    });
                env.AssertIterator(
                    "s1",
                    iterator => {
                        var received = EPAssertionUtil.EnumeratorToArray(iterator);
                        EPAssertionUtil.AssertPropsPerRow(
                            received,
                            "loc,sku,avgTime,cntEnter,cntLeave,diff".SplitCsv(),
                            expected);
                    });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        private class InfraFullOuterJoinNamedAggregationLateStart : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@name('create') @public create window MyWindowFO#groupwin(theString, intPrimitive)#length(3) as select theString, intPrimitive, boolPrimitive from SupportBean;\n" +
                    "insert into MyWindowFO select theString, intPrimitive, boolPrimitive from SupportBean;\n";
                env.CompileDeploy(epl, path).AddListener("create");

                // fill window
                var stringValues = new string[] { "c0", "c1", "c2" };
                for (var i = 0; i < stringValues.Length; i++) {
                    for (var j = 0; j < 3; j++) {
                        for (var k = 0; k < 2; k++) {
                            var beanX = new SupportBean(stringValues[i], j);
                            beanX.BoolPrimitive = true;
                            env.SendEventBean(beanX);
                        }
                    }
                }

                var bean = new SupportBean("c1", 2);
                bean.BoolPrimitive = true;
                env.SendEventBean(bean);

                env.AssertIterator(
                    "create",
                    iterator => {
                        var received = EPAssertionUtil.EnumeratorToArray(iterator);
                        Assert.AreEqual(19, received.Length);
                    });

                // create select stmt
                var stmtTextSelect =
                    "@name('select') select theString, intPrimitive, count(boolPrimitive) as cntBool, symbol " +
                    "from MyWindowFO full outer join SupportMarketDataBean#keepall " +
                    "on theString = symbol " +
                    "group by theString, intPrimitive, symbol order by theString, intPrimitive, symbol";
                env.CompileDeploy(stmtTextSelect, path);

                // send outer join events
                SendMarketBean(env, "c0");
                SendMarketBean(env, "c3");

                // get iterator results
                env.AssertIterator(
                    "select",
                    en => {
                        var received = EPAssertionUtil.EnumeratorToArray(en);
                        EPAssertionUtil.AssertPropsPerRow(
                            received,
                            "theString,intPrimitive,cntBool,symbol".SplitCsv(),
                            new object[][] {
                                new object[] { null, null, 0L, "c3" },
                                new object[] { "c0", 0, 2L, "c0" },
                                new object[] { "c0", 1, 2L, "c0" },
                                new object[] { "c0", 2, 2L, "c0" },
                                new object[] { "c1", 0, 2L, null },
                                new object[] { "c1", 1, 2L, null },
                                new object[] { "c1", 2, 3L, null },
                                new object[] { "c2", 0, 2L, null },
                                new object[] { "c2", 1, 2L, null },
                                new object[] { "c2", 2, 2L, null },
                            });
                    });

                /*
                for (int i = 0; i < received.length; i++)
                {
                    Console.WriteLine("string=" + received[i].get("string") +
                            " intPrimitive=" + received[i].get("intPrimitive") +
                            " cntBool=" + received[i].get("cntBool") +
                            " symbol=" + received[i].get("symbol"));
                }
                */

                env.UndeployModuleContaining("select");
                env.UndeployModuleContaining("create");
            }
        }

        private class InfraJoinNamedAndStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@name('create') @public create window MyWindowJNS#keepall as select theString as a, intPrimitive as b from SupportBean;\n" +
                    "on SupportBean_A delete from MyWindowJNS where id = a;\n" +
                    "insert into MyWindowJNS select theString as a, intPrimitive as b from SupportBean;\n";
                env.CompileDeploy(epl, path);

                // create consumer
                var fields = new string[] { "symbol", "a", "b" };
                epl = "@name('s0') select irstream symbol, a, b " +
                      " from SupportMarketDataBean#length(10) as s0," +
                      "MyWindowJNS as s1 where s1.a = symbol";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        EPAssertionUtil.AssertEqualsAnyOrder(
                            statement.EventType.PropertyNames,
                            new string[] { "symbol", "a", "b" });
                        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("symbol"));
                        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("a"));
                        Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("b"));
                    });

                SendMarketBean(env, "S1");
                env.AssertListenerNotInvoked("s0");

                SendSupportBean(env, "S1", 1);
                env.AssertPropsNew("s0", fields, new object[] { "S1", "S1", 1 });

                SendSupportBean_A(env, "S1"); // deletes from window
                env.AssertPropsOld("s0", fields, new object[] { "S1", "S1", 1 });

                SendMarketBean(env, "S1");
                env.AssertListenerNotInvoked("s0");

                SendSupportBean(env, "S2", 2);
                env.AssertListenerNotInvoked("s0");

                SendMarketBean(env, "S2");
                env.AssertPropsNew("s0", fields, new object[] { "S2", "S2", 2 });

                SendSupportBean(env, "S3", 3);
                SendSupportBean(env, "S3", 4);
                env.AssertListenerNotInvoked("s0");

                SendMarketBean(env, "S3");
                env.AssertListener("s0", listener => Assert.AreEqual(2, listener.GetAndResetLastNewData().Length));

                SendSupportBean_A(env, "S3"); // deletes from window
                env.AssertListener("s0", listener => Assert.AreEqual(2, listener.GetAndResetLastOldData().Length));

                SendMarketBean(env, "S3");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class InfraJoinBetweenNamed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "a1", "b1", "a2", "b2" };

                var epl =
                    "@name('createOne') @public create window MyWindowOne#keepall as select theString as a1, intPrimitive as b1 from SupportBean;\n" +
                    "@name('createTwo') @public create window MyWindowTwo#keepall as select theString as a2, intPrimitive as b2 from SupportBean;\n" +
                    "on SupportMarketDataBean(volume=1) delete from MyWindowOne where symbol = a1;\n" +
                    "on SupportMarketDataBean(volume=0) delete from MyWindowTwo where symbol = a2;\n" +
                    "insert into MyWindowOne select theString as a1, intPrimitive as b1 from SupportBean(boolPrimitive = true);\n" +
                    "insert into MyWindowTwo select theString as a2, intPrimitive as b2 from SupportBean(boolPrimitive = false);\n" +
                    "@name('s0') select irstream a1, b1, a2, b2 from MyWindowOne as s0, MyWindowTwo as s1 where s0.a1 = s1.a2;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendSupportBean(env, true, "S0", 1);
                env.AssertListenerNotInvoked("s0");

                SendSupportBean(env, false, "S0", 2);
                env.AssertPropsNew("s0", fields, new object[] { "S0", 1, "S0", 2 });

                SendSupportBean(env, false, "S1", 3);
                env.AssertListenerNotInvoked("s0");

                SendSupportBean(env, true, "S1", 4);
                env.AssertPropsNew("s0", fields, new object[] { "S1", 4, "S1", 3 });

                SendSupportBean(env, true, "S1", 5);
                env.AssertPropsNew("s0", fields, new object[] { "S1", 5, "S1", 3 });

                SendSupportBean(env, false, "S1", 6);
                env.AssertListener("s0", listener => Assert.AreEqual(2, listener.GetAndResetLastNewData().Length));

                // delete and insert back in
                SendMarketBean(env, "S0", 0);
                env.AssertPropsOld("s0", fields, new object[] { "S0", 1, "S0", 2 });

                SendSupportBean(env, false, "S0", 7);
                env.AssertPropsNew("s0", fields, new object[] { "S0", 1, "S0", 7 });

                // delete and insert back in
                SendMarketBean(env, "S0", 1);
                env.AssertPropsOld("s0", fields, new object[] { "S0", 1, "S0", 7 });

                SendSupportBean(env, true, "S0", 8);
                env.AssertPropsNew("s0", fields, new object[] { "S0", 8, "S0", 7 });

                env.UndeployAll();
            }
        }

        private class InfraJoinBetweenSameNamed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "a0", "b0", "a1", "b1" };

                var epl =
                    "@name('create') create window MyWindowJSN#keepall as select theString as a, intPrimitive as b from SupportBean;\n" +
                    "on SupportMarketDataBean delete from MyWindowJSN where symbol = a;\n" +
                    "insert into MyWindowJSN select theString as a, intPrimitive as b from SupportBean;\n" +
                    "@name('s0') select irstream s0.a as a0, s0.b as b0, s1.a as a1, s1.b as b1 from MyWindowJSN as s0, MyWindowJSN as s1 where s0.a = s1.a;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendSupportBean(env, "E1", 1);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1, "E1", 1 });

                SendSupportBean(env, "E2", 2);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 2, "E2", 2 });

                SendMarketBean(env, "E1", 1);
                env.AssertPropsOld("s0", fields, new object[] { "E1", 1, "E1", 1 });

                SendMarketBean(env, "E0", 0);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class InfraJoinSingleInsertOneWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "a1", "b1", "a2", "b2" };

                var epl =
                    "@name('create') create window MyWindowJSIOne#keepall as select theString as a1, intPrimitive as b1 from SupportBean;\n" +
                    "@name('createTwo') create window MyWindowJSITwo#keepall as select theString as a2, intPrimitive as b2 from SupportBean;\n" +
                    "on SupportMarketDataBean(volume=1) delete from MyWindowJSIOne where symbol = a1;\n" +
                    "on SupportMarketDataBean(volume=0) delete from MyWindowJSITwo where symbol = a2;\n" +
                    "insert into MyWindowJSIOne select theString as a1, intPrimitive as b1 from SupportBean(boolPrimitive = true);\n" +
                    "insert into MyWindowJSITwo select theString as a2, intPrimitive as b2 from SupportBean(boolPrimitive = false);\n" +
                    "@name('select') select irstream a1, b1, a2, b2 from MyWindowJSIOne as s0, MyWindowJSITwo as s1 where s0.a1 = s1.a2;\n";
                env.CompileDeploy(epl).AddListener("select");

                SendSupportBean(env, true, "S0", 1);
                env.AssertListenerNotInvoked("select");

                SendSupportBean(env, false, "S0", 2);
                env.AssertPropsNew("select", fields, new object[] { "S0", 1, "S0", 2 });

                SendSupportBean(env, false, "S1", 3);
                env.AssertListenerNotInvoked("select");

                SendSupportBean(env, true, "S1", 4);
                env.AssertPropsNew("select", fields, new object[] { "S1", 4, "S1", 3 });

                SendSupportBean(env, true, "S1", 5);
                env.AssertPropsNew("select", fields, new object[] { "S1", 5, "S1", 3 });

                SendSupportBean(env, false, "S1", 6);
                env.AssertListener(
                    "select",
                    listener => {
                        Assert.AreEqual(2, listener.LastNewData.Length);
                        listener.Reset();
                    });

                // delete and insert back in
                SendMarketBean(env, "S0", 0);
                env.AssertPropsOld("select", fields, new object[] { "S0", 1, "S0", 2 });

                SendSupportBean(env, false, "S0", 7);
                env.AssertPropsNew("select", fields, new object[] { "S0", 1, "S0", 7 });

                // delete and insert back in
                SendMarketBean(env, "S0", 1);
                env.AssertPropsOld("select", fields, new object[] { "S0", 1, "S0", 7 });

                SendSupportBean(env, true, "S0", 8);
                env.AssertPropsNew("select", fields, new object[] { "S0", 8, "S0", 7 });

                env.UndeployAll();
            }
        }

        private class InfraUnidirectional : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindowU#keepall select * from SupportBean;\n" +
                          "insert into MyWindowU select * from SupportBean;\n" +
                          "@name('select') select w.* from MyWindowU w unidirectional, SupportBean_A#lastevent s where s.id = w.theString;\n";
                env.CompileDeploy(epl).AddListener("select");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertListenerNotInvoked("select");
                env.SendEventBean(new SupportBean_A("E1"));
                env.AssertListenerNotInvoked("select");
                env.SendEventBean(new SupportBean_A("E2"));
                env.AssertListenerNotInvoked("select");

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertListenerInvoked("select");

                env.UndeployAll();
            }
        }

        private static void SendSupportBean_A(
            RegressionEnvironment env,
            string id)
        {
            var bean = new SupportBean_A(id);
            env.SendEventBean(bean);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            bool boolPrimitive,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.BoolPrimitive = boolPrimitive;
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendMarketBean(
            RegressionEnvironment env,
            string symbol)
        {
            var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
            env.SendEventBean(bean);
        }

        private static void SendMarketBean(
            RegressionEnvironment env,
            string symbol,
            long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            env.SendEventBean(bean);
        }

        private static void AssertReceived(
            RegressionEnvironment env,
            SupportBean[] beans,
            int[] indexesAll,
            int[] indexesWhere,
            string[] mapKeys,
            object[] mapValues)
        {
            env.AssertEventNew(
                "s0",
                received => {
                    EPAssertionUtil.AssertEqualsExactOrder(
                        SupportBean.GetBeansPerIndex(beans, indexesAll),
                        (object[])received.Get("c0"));
                    EPAssertionUtil.AssertEqualsExactOrder(
                        SupportBean.GetBeansPerIndex(beans, indexesWhere),
                        received.Get("c1").Unwrap<SupportBean>());
                    EPAssertionUtil.AssertPropsMap(received.Get("c2").AsStringDictionary(), mapKeys, mapValues);
                });
        }

        [Serializable]
        public class MyLocalJsonProvidedProduct
        {
            public string product;
            public int size;
        }

        [Serializable]
        public class MyLocalJsonProvidedPortfolio
        {
            public string portfolio;
            public string product;
        }
    }
} // end of namespace