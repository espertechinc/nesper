///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client.scopetest;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    public class InfraNamedWindowJoin : IndexBackingTableInfo
    {
        private static readonly ILog log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraJoinIndexChoice());
            execs.Add(new InfraRightOuterJoinLateStart());
            execs.Add(new InfraFullOuterJoinNamedAggregationLateStart());
            execs.Add(new InfraJoinNamedAndStream());
            execs.Add(new InfraJoinBetweenNamed());
            execs.Add(new InfraJoinBetweenSameNamed());
            execs.Add(new InfraJoinSingleInsertOneWindow());
            execs.Add(new InfraUnidirectional());
            execs.Add(new InfraWindowUnidirectionalJoin());
            execs.Add(new InfraInnerJoinLateStart());
            return execs;
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
            SupportListener listenerStmtOne,
            SupportBean[] beans,
            int[] indexesAll,
            int[] indexesWhere,
            string[] mapKeys,
            object[] mapValues)
        {
            var received = listenerStmtOne.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsExactOrder(
                SupportBean.GetBeansPerIndex(beans, indexesAll),
                (object[]) received.Get("c0"));
            EPAssertionUtil.AssertEqualsExactOrder(
                SupportBean.GetBeansPerIndex(beans, indexesWhere),
                (ICollection<object>) received.Get("c1"));
            EPAssertionUtil.AssertPropsMap((IDictionary<object, object>) received.Get("c2"), mapKeys, mapValues);
        }

        internal class InfraWindowUnidirectionalJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window MyWindowWUJ#keepall as SupportBean;\n" +
                          "insert into MyWindowWUJ select * from SupportBean;\n" +
                          "on SupportBean_S1 as S1 delete from MyWindowWUJ where S1.P10 = TheString;\n" +
                          "@Name('s0') select window(win.*) as c0," +
                          "window(win.*).where(v -> v.IntPrimitive < 2) as c1, " +
                          "window(win.*).toMap(k=>k.TheString,v->v.IntPrimitive) as c2 " +
                          "from SupportBean_S0 as S0 unidirectional, MyWindowWUJ as win";
                env.CompileDeploy(epl, path).AddListener("s0");

                var beans = new SupportBean[3];
                for (var i = 0; i < beans.Length; i++) {
                    beans[i] = new SupportBean("E" + i, i);
                }

                env.SendEventBean(beans[0]);
                env.SendEventBean(beans[1]);
                env.SendEventBean(new SupportBean_S0(10));
                AssertReceived(
                    env.Listener("s0"),
                    beans,
                    new[] {0, 1},
                    new[] {0, 1},
                    new [] { "E0","E1" },
                    new object[] {0, 1});

                // add bean
                env.SendEventBean(beans[2]);
                env.SendEventBean(new SupportBean_S0(10));
                AssertReceived(
                    env.Listener("s0"),
                    beans,
                    new[] {0, 1, 2},
                    new[] {0, 1},
                    new [] { "E0","E1","E2" },
                    new object[] {0, 1, 2});

                // delete bean
                env.SendEventBean(new SupportBean_S1(11, "E1"));
                env.SendEventBean(new SupportBean_S0(12));
                AssertReceived(
                    env.Listener("s0"),
                    beans,
                    new[] {0, 2},
                    new[] {0},
                    new [] { "E0","E2" },
                    new object[] {0, 2});

                // delete another bean
                env.SendEventBean(new SupportBean_S1(13, "E0"));
                env.SendEventBean(new SupportBean_S0(14));
                AssertReceived(env.Listener("s0"), beans, new[] {2}, new int[0], new [] { "E2" }, new object[] {2});

                // delete last bean
                env.SendEventBean(new SupportBean_S1(15, "E2"));
                env.SendEventBean(new SupportBean_S0(16));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                // compile a non-unidirectional query, join and subquery
                env.CompileDeploy("select window(win.*) from MyWindowWUJ as win", path);
                env.CompileDeploy(
                    "select window(win.*) as c0 from SupportBean_S0#lastevent as S0, MyWindowWUJ as win",
                    path);
                env.CompileDeploy("select (select window(win.*) from MyWindowWUJ as win) from SupportBean_S0", path);

                env.UndeployAll();
            }
        }

        internal class InfraJoinIndexChoice : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                object[] preloadedEventsOne =
                    {new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
                IndexAssertionEventSend eventSendAssertion = () => {
                    var fields = new [] { "ssb2.S2","ssb1.S1","ssb1.I1" };
                    env.SendEventBean(new SupportSimpleBeanTwo("E2", 50, 21, 22));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E2", "E2", 20});
                    env.SendEventBean(new SupportSimpleBeanTwo("E1", 60, 11, 12));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E1", "E1", 10});
                };

                // no index, since this is "unique(S1)" we don't need one
                string[] noindexes = { };
                AssertIndexChoice(
                    env,
                    noindexes,
                    preloadedEventsOne,
                    "std:unique(S1)",
                    new IndexAssertion(null, "S1 = S2", true, eventSendAssertion),
                    new IndexAssertion(null, "S1 = S2 and L1 = L2", true, eventSendAssertion));

                // single index one field (duplicate in essence, since "unique(S1)"
                string[] indexOneField = {"create unique index One on MyWindow (S1)"};
                AssertIndexChoice(
                    env,
                    indexOneField,
                    preloadedEventsOne,
                    "std:unique(S1)",
                    new IndexAssertion(null, "S1 = S2", true, eventSendAssertion),
                    new IndexAssertion(null, "S1 = S2 and L1 = L2", true, eventSendAssertion));

                // single index two field (includes "unique(S1)")
                string[] indexTwoField = {"create unique index One on MyWindow (S1, L1)"};
                AssertIndexChoice(
                    env,
                    indexTwoField,
                    preloadedEventsOne,
                    "std:unique(S1)",
                    new IndexAssertion(null, "S1 = S2", true, eventSendAssertion),
                    new IndexAssertion(null, "D1 = D2", false, eventSendAssertion),
                    new IndexAssertion(null, "S1 = S2 and L1 = L2", true, eventSendAssertion));

                // two index one unique ("unique(S1)")
                string[] indexSetTwo = {
                    "create index One on MyWindow (S1)",
                    "create unique index Two on MyWindow (S1, D1)"
                };
                AssertIndexChoice(
                    env,
                    indexSetTwo,
                    preloadedEventsOne,
                    "std:unique(S1)",
                    new IndexAssertion(null, "D1 = D2", false, eventSendAssertion),
                    new IndexAssertion(null, "S1 = S2", true, eventSendAssertion),
                    new IndexAssertion(null, "S1 = S2 and L1 = L2", true, eventSendAssertion),
                    new IndexAssertion(null, "S1 = S2 and D1 = D2 and L1 = L2", true, eventSendAssertion));

                // two index one unique ("win:keepall()")
                AssertIndexChoice(
                    env,
                    indexSetTwo,
                    preloadedEventsOne,
                    "win:keepall()",
                    new IndexAssertion(null, "D1 = D2", false, eventSendAssertion),
                    new IndexAssertion(null, "S1 = S2", false, eventSendAssertion),
                    new IndexAssertion(null, "S1 = S2 and L1 = L2", false, eventSendAssertion),
                    new IndexAssertion(null, "S1 = S2 and D1 = D2 and L1 = L2", true, eventSendAssertion),
                    new IndexAssertion(null, "D1 = D2 and S1 = S2", true, eventSendAssertion));
            }

            private static void AssertIndexChoice(
                RegressionEnvironment env,
                string[] indexes,
                object[] preloadedEvents,
                string datawindow,
                params IndexAssertion[] assertions)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindow." + datawindow + " as SupportSimpleBeanOne", path);
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
                        env.CompileDeploy("@Name('s0')" + epl, path).AddListener("s0");
                    }
                    catch (EPCompileExceptionItem ex) {
                        if (assertion.EventSendAssertion == null) {
                            // no assertion, expected
                            Assert.IsTrue(ex.Message.Contains("index hint busted"));
                            continue;
                        }

                        throw new EPException("Unexpected statement exception: " + ex.Message, ex);
                    }

                    // assert index and access
                    SupportQueryPlanIndexHook.AssertJoinOneStreamAndReset(assertion.Unique);
                    assertion.EventSendAssertion.Invoke();
                    env.UndeployModuleContaining("s0");
                }

                env.UndeployAll();
            }
        }

        internal class InfraInnerJoinLateStart : RegressionExecution
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
                var schemaEPL = eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedProduct>() +
                                "@Name('schema') create schema Product (product string, size int);\n" +
                                eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedPortfolio>() +
                                " create schema Portfolio (portfolio string, product string);\n";
                var path = new RegressionPath();
                env.CompileDeployWBusPublicType(schemaEPL, path);

                env.CompileDeploy("@Name('window') create window ProductWin#keepall as Product", path);

                Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("schema").EventType.UnderlyingType));
                Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("window").EventType.UnderlyingType));

                env.CompileDeploy("insert into ProductWin select * from Product", path);
                env.CompileDeploy("create window PortfolioWin#keepall as Portfolio", path);
                env.CompileDeploy("insert into PortfolioWin select * from Portfolio", path);

                SendProduct(env, eventRepresentationEnum, "productA", 1);
                SendProduct(env, eventRepresentationEnum, "productB", 2);
                sendPortfolio(env, eventRepresentationEnum, "Portfolio", "productA");

                var stmtText = "@Name(\"Query2\") select portfolio, ProductWin.product, size " +
                               "from PortfolioWin unidirectional inner join ProductWin on PortfolioWin.product=ProductWin.product";
                env.CompileDeploy(stmtText, path).AddListener("Query2");

                sendPortfolio(env, eventRepresentationEnum, "Portfolio", "productB");
                EPAssertionUtil.AssertProps(
                    env.Listener("Query2").AssertOneGetNewAndReset(),
                    new[] {"portfolio", "ProductWin.product", "size"},
                    new object[] {"Portfolio", "productB", 2});

                sendPortfolio(env, eventRepresentationEnum, "Portfolio", "productC");
                env.Listener("Query2").Reset();

                SendProduct(env, eventRepresentationEnum, "productC", 3);
                sendPortfolio(env, eventRepresentationEnum, "Portfolio", "productC");
                EPAssertionUtil.AssertProps(
                    env.Listener("Query2").AssertOneGetNewAndReset(),
                    new[] {"portfolio", "ProductWin.product", "size"},
                    new object[] {"Portfolio", "productC", 3});

                env.UndeployAll();
            }

            private static void SendProduct(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum,
                string product,
                int size)
            {
                if (eventRepresentationEnum.IsObjectArrayEvent()) {
                    env.SendEventObjectArray(new object[] {product, size}, "Product");
                }
                else if (eventRepresentationEnum.IsMapEvent()) {
                    var theEvent = new LinkedHashMap<string, object>();
                    theEvent.Put("product", product);
                    theEvent.Put("size", size);
                    env.SendEventMap(theEvent, "Product");
                }
                else if (eventRepresentationEnum.IsAvroEvent()) {
                    var theEvent = new GenericRecord(
                        SupportAvroUtil.GetAvroSchema(
                                env.Runtime.EventTypeService.GetEventTypePreconfigured("Product"))
                            .AsRecordSchema());
                    theEvent.Put("product", product);
                    theEvent.Put("size", size);
                    env.EventService.SendEventAvro(theEvent, "Product");
                }
                else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                    var @object = new JObject();
                    @object.Add("product", product);
                    @object.Add("size", size);
                    env.EventService.SendEventJson(@object.ToString(), "Product");
                }
                else {
                    Assert.Fail();
                }
            }

            private static void sendPortfolio(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum,
                string portfolio,
                string product)
            {
                if (eventRepresentationEnum.IsObjectArrayEvent()) {
                    env.SendEventObjectArray(new object[] {portfolio, product}, "Portfolio");
                }
                else if (eventRepresentationEnum.IsMapEvent()) {
                    var theEvent = new LinkedHashMap<string, object>();
                    theEvent.Put("portfolio", portfolio);
                    theEvent.Put("product", product);
                    env.SendEventMap(theEvent, "Portfolio");
                }
                else if (eventRepresentationEnum.IsAvroEvent()) {
                    var theEvent = new GenericRecord(
                        SupportAvroUtil.GetAvroSchema(
                                env.Runtime.EventTypeService.GetEventTypePreconfigured("Portfolio"))
                            .AsRecordSchema());
                    theEvent.Put("portfolio", portfolio);
                    theEvent.Put("product", product);
                    env.EventService.SendEventAvro(theEvent, "Portfolio");
                }
                else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                    var @object = new JObject();
                    @object.Add("portfolio", portfolio);
                    @object.Add("product", product);
                    env.EventService.SendEventJson(@object.ToString(), "Portfolio");
                }
                else {
                    Assert.Fail();
                }
            }
        }

        internal class InfraRightOuterJoinLateStart : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test for ESPER-186 Iterator not honoring order by clause for grouped join query with output-rate clause
                // Test for ESPER-187 Join of two or more named windows on late start may not return correct aggregation state on iterate

                var path = new RegressionPath();

                // create window for Leave events
                var epl =
                    "create window WindowLeave#time(6000) as select TimeLeave, Id, Location from SupportQueueLeave;\n" +
                    "insert into WindowLeave select TimeLeave, Id, Location from SupportQueueLeave;\n";
                env.CompileDeploy(epl, path);

                // create second window for enter events
                epl =
                    "create window WindowEnter#time(6000) as select Location, Sku, TimeEnter, Id from SupportQueueEnter;\n" +
                    "insert into WindowEnter select Location, Sku, TimeEnter, Id from SupportQueueEnter;\n";
                env.CompileDeploy(epl, path);

                // fill data
                for (var i = 0; i < 8; i++) {
                    var location = Convert.ToString(i / 2);
                    env.SendEventBean(new SupportQueueLeave(i + 1, location, 247));
                }

                for (var i = 0; i < 10; i++) {
                    var location = Convert.ToString(i / 2);
                    var sku = i % 2 == 0 ? "166583" : "169254";
                    env.SendEventBean(new SupportQueueEnter(i + 1, location, sku, 123));
                }

                var stmtTextOne =
                    "@Name('s1') select S1.Location as loc, Sku as sku, avg((coalesce(TimeLeave, 250) - TimeEnter)) as avgTime, " +
                    "count(TimeEnter) as cntEnter, count(TimeLeave) as cntLeave, (count(TimeEnter) - count(TimeLeave)) as diff " +
                    "from WindowLeave as S0 right outer join WindowEnter as S1 " +
                    "on S0.Id = S1.Id and S0.Location = S1.Location " +
                    "group by S1.Location, Sku " +
                    "output every 1.0 seconds " +
                    "order by S1.Location, Sku";
                env.CompileDeploy(stmtTextOne, path);

                var stmtTextTwo =
                    "@Name('s2') select S1.Location as loc, Sku as sku, avg((coalesce(TimeLeave, 250) - TimeEnter)) as avgTime, " +
                    "count(TimeEnter) as cntEnter, count(TimeLeave) as cntLeave, (count(TimeEnter) - count(TimeLeave)) as diff " +
                    "from WindowEnter as S1 left outer join WindowLeave as S0 " +
                    "on S0.Id = S1.Id and S0.Location = S1.Location " +
                    "group by S1.Location, Sku " +
                    "output every 1.0 seconds " +
                    "order by S1.Location, Sku";
                env.CompileDeploy(stmtTextTwo, path);

                object[][] expected = {
                    new object[] {"0", "166583", 124.0, 1L, 1L, 0L},
                    new object[] {"0", "169254", 124.0, 1L, 1L, 0L},
                    new object[] {"1", "166583", 124.0, 1L, 1L, 0L},
                    new object[] {"1", "169254", 124.0, 1L, 1L, 0L},
                    new object[] {"2", "166583", 124.0, 1L, 1L, 0L},
                    new object[] {"2", "169254", 124.0, 1L, 1L, 0L},
                    new object[] {"3", "166583", 124.0, 1L, 1L, 0L},
                    new object[] {"3", "169254", 124.0, 1L, 1L, 0L},
                    new object[] {"4", "166583", 127.0, 1L, 0L, 1L},
                    new object[] {"4", "169254", 127.0, 1L, 0L, 1L}
                };

                // assert iterator results
                var received = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s2"));
                EPAssertionUtil.AssertPropsPerRow(
                    received,
                    new [] { "loc","sku","avgTime","cntEnter","cntLeave","diff" },
                    expected);
                received = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s1"));
                EPAssertionUtil.AssertPropsPerRow(
                    received,
                    new [] { "loc","sku","avgTime","cntEnter","cntLeave","diff" },
                    expected);

                env.UndeployAll();
            }
        }

        internal class InfraFullOuterJoinNamedAggregationLateStart : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@Name('create') create window MyWindowFO#groupwin(TheString, IntPrimitive)#length(3) as select TheString, IntPrimitive, BoolPrimitive from SupportBean;\n" +
                    "insert into MyWindowFO select TheString, IntPrimitive, BoolPrimitive from SupportBean;\n";
                env.CompileDeploy(epl, path).AddListener("create");

                // fill window
                string[] stringValues = {"c0", "c1", "c2"};
                for (var i = 0; i < stringValues.Length; i++) {
                    for (var j = 0; j < 3; j++) {
                        for (var k = 0; k < 2; k++) {
                            env.SendEventBean(
                                new SupportBean(stringValues[i], j) {
                                    BoolPrimitive = true
                                });
                        }
                    }
                }

                env.SendEventBean(new SupportBean("c1", 2) {BoolPrimitive = true});

                var received = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("create"));
                Assert.AreEqual(19, received.Length);

                // create select stmt
                var stmtTextSelect =
                    "@Name('select') select TheString, IntPrimitive, count(BoolPrimitive) as cntBool, Symbol " +
                    "from MyWindowFO full outer join SupportMarketDataBean#keepall " +
                    "on TheString = Symbol " +
                    "group by TheString, IntPrimitive, Symbol order by TheString, IntPrimitive, Symbol";
                env.CompileDeploy(stmtTextSelect, path);

                // send outer join events
                SendMarketBean(env, "c0");
                SendMarketBean(env, "c3");

                // get iterator results
                received = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("select"));
                EPAssertionUtil.AssertPropsPerRow(
                    received,
                    new [] { "TheString","IntPrimitive","cntBool","Symbol" },
                    new[] {
                        new object[] {null, null, 0L, "c3"},
                        new object[] {"c0", 0, 2L, "c0"},
                        new object[] {"c0", 1, 2L, "c0"},
                        new object[] {"c0", 2, 2L, "c0"},
                        new object[] {"c1", 0, 2L, null},
                        new object[] {"c1", 1, 2L, null},
                        new object[] {"c1", 2, 3L, null},
                        new object[] {"c2", 0, 2L, null},
                        new object[] {"c2", 1, 2L, null},
                        new object[] {"c2", 2, 2L, null}
                    });

                env.UndeployModuleContaining("select");
                env.UndeployModuleContaining("create");
            }
        }

        internal class InfraJoinNamedAndStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@Name('create') create window MyWindowJNS#keepall as select TheString as a, IntPrimitive as b from SupportBean;\n" +
                    "on SupportBean_A delete from MyWindowJNS where Id = a;\n" +
                    "insert into MyWindowJNS select TheString as a, IntPrimitive as b from SupportBean;\n";
                env.CompileDeploy(epl, path);

                // create consumer
                string[] fields = {"Symbol", "a", "b"};
                epl = "@Name('s0') select irstream Symbol, a, b " +
                      " from SupportMarketDataBean#length(10) as S0," +
                      "MyWindowJNS as S1 where S1.a = Symbol";
                env.CompileDeploy(epl, path).AddListener("s0");

                EPAssertionUtil.AssertEqualsAnyOrder(
                    env.Statement("s0").EventType.PropertyNames,
                    new[] {"Symbol", "a", "b"});
                Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
                Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("a"));
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("b"));

                SendMarketBean(env, "S1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendSupportBean(env, "S1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", "S1", 1});

                SendSupportBean_A(env, "S1"); // deletes from window
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"S1", "S1", 1});

                SendMarketBean(env, "S1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendSupportBean(env, "S2", 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendMarketBean(env, "S2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S2", "S2", 2});

                SendSupportBean(env, "S3", 3);
                SendSupportBean(env, "S3", 4);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendMarketBean(env, "S3");
                Assert.AreEqual(2, env.Listener("s0").LastNewData.Length);
                env.Listener("s0").Reset();

                SendSupportBean_A(env, "S3"); // deletes from window
                Assert.AreEqual(2, env.Listener("s0").LastOldData.Length);
                env.Listener("s0").Reset();

                SendMarketBean(env, "S3");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class InfraJoinBetweenNamed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"a1", "b1", "a2", "b2"};

                var epl =
                    "@Name('createOne') create window MyWindowOne#keepall as select TheString as a1, IntPrimitive as b1 from SupportBean;\n" +
                    "@Name('createTwo') create window MyWindowTwo#keepall as select TheString as a2, IntPrimitive as b2 from SupportBean;\n" +
                    "on SupportMarketDataBean(Volume=1) delete from MyWindowOne where Symbol = a1;\n" +
                    "on SupportMarketDataBean(Volume=0) delete from MyWindowTwo where Symbol = a2;\n" +
                    "insert into MyWindowOne select TheString as a1, IntPrimitive as b1 from SupportBean(BoolPrimitive = true);\n" +
                    "insert into MyWindowTwo select TheString as a2, IntPrimitive as b2 from SupportBean(BoolPrimitive = false);\n" +
                    "@Name('s0') select irstream a1, b1, a2, b2 from MyWindowOne as S0, MyWindowTwo as S1 where S0.a1 = S1.a2;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendSupportBean(env, true, "S0", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendSupportBean(env, false, "S0", 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0", 1, "S0", 2});

                SendSupportBean(env, false, "S1", 3);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendSupportBean(env, true, "S1", 4);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", 4, "S1", 3});

                SendSupportBean(env, true, "S1", 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", 5, "S1", 3});

                SendSupportBean(env, false, "S1", 6);
                Assert.AreEqual(2, env.Listener("s0").LastNewData.Length);
                env.Listener("s0").Reset();

                // delete and insert back in
                SendMarketBean(env, "S0", 0);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"S0", 1, "S0", 2});

                SendSupportBean(env, false, "S0", 7);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0", 1, "S0", 7});

                // delete and insert back in
                SendMarketBean(env, "S0", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"S0", 1, "S0", 7});

                SendSupportBean(env, true, "S0", 8);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0", 8, "S0", 7});

                env.UndeployAll();
            }
        }

        internal class InfraJoinBetweenSameNamed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"a0", "b0", "a1", "b1"};

                var epl =
                    "@Name('create') create window MyWindowJSN#keepall as select TheString as a, IntPrimitive as b from SupportBean;\n" +
                    "on SupportMarketDataBean delete from MyWindowJSN where Symbol = a;\n" +
                    "insert into MyWindowJSN select TheString as a, IntPrimitive as b from SupportBean;\n" +
                    "@Name('s0') select irstream S0.a as a0, S0.b as b0, S1.a as a1, S1.b as b1 from MyWindowJSN as S0, MyWindowJSN as S1 where S0.a = S1.a;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendSupportBean(env, "E1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1, "E1", 1});

                SendSupportBean(env, "E2", 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2, "E2", 2});

                SendMarketBean(env, "E1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1", 1, "E1", 1});

                SendMarketBean(env, "E0", 0);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class InfraJoinSingleInsertOneWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"a1", "b1", "a2", "b2"};

                var epl =
                    "@Name('create') create window MyWindowJSIOne#keepall as select TheString as a1, IntPrimitive as b1 from SupportBean;\n" +
                    "@Name('createTwo') create window MyWindowJSITwo#keepall as select TheString as a2, IntPrimitive as b2 from SupportBean;\n" +
                    "on SupportMarketDataBean(Volume=1) delete from MyWindowJSIOne where Symbol = a1;\n" +
                    "on SupportMarketDataBean(Volume=0) delete from MyWindowJSITwo where Symbol = a2;\n" +
                    "insert into MyWindowJSIOne select TheString as a1, IntPrimitive as b1 from SupportBean(BoolPrimitive = true);\n" +
                    "insert into MyWindowJSITwo select TheString as a2, IntPrimitive as b2 from SupportBean(BoolPrimitive = false);\n" +
                    "@Name('select') select irstream a1, b1, a2, b2 from MyWindowJSIOne as S0, MyWindowJSITwo as S1 where S0.a1 = S1.a2;\n";
                env.CompileDeploy(epl).AddListener("select");

                SendSupportBean(env, true, "S0", 1);
                Assert.IsFalse(env.Listener("select").IsInvoked);

                SendSupportBean(env, false, "S0", 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0", 1, "S0", 2});

                SendSupportBean(env, false, "S1", 3);
                Assert.IsFalse(env.Listener("select").IsInvoked);

                SendSupportBean(env, true, "S1", 4);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", 4, "S1", 3});

                SendSupportBean(env, true, "S1", 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", 5, "S1", 3});

                SendSupportBean(env, false, "S1", 6);
                Assert.AreEqual(2, env.Listener("select").LastNewData.Length);
                env.Listener("select").Reset();

                // delete and insert back in
                SendMarketBean(env, "S0", 0);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"S0", 1, "S0", 2});

                SendSupportBean(env, false, "S0", 7);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0", 1, "S0", 7});

                // delete and insert back in
                SendMarketBean(env, "S0", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"S0", 1, "S0", 7});

                SendSupportBean(env, true, "S0", 8);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0", 8, "S0", 7});

                env.UndeployAll();
            }
        }

        internal class InfraUnidirectional : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindowU#keepall select * from SupportBean;\n" +
                          "insert into MyWindowU select * from SupportBean;\n" +
                          "@Name('select') select w.* from MyWindowU w unidirectional, SupportBean_A#lastevent s where s.Id = w.TheString;\n";
                env.CompileDeploy(epl).AddListener("select");

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsFalse(env.Listener("select").IsInvoked);
                env.SendEventBean(new SupportBean_A("E1"));
                Assert.IsFalse(env.Listener("select").IsInvoked);
                env.SendEventBean(new SupportBean_A("E2"));
                Assert.IsFalse(env.Listener("select").IsInvoked);

                env.SendEventBean(new SupportBean("E2", 1));
                Assert.IsTrue(env.Listener("select").IsInvoked);

                env.UndeployAll();
            }
        }

        [Serializable]
        public class MyLocalJsonProvidedProduct
        {
            public String product;
            public int size;
        }

        [Serializable]
        public class MyLocalJsonProvidedPortfolio
        {
            public String portfolio;
            public String product;
        }
    }
} // end of namespace