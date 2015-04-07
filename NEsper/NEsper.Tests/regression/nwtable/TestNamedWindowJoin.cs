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

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowJoin : IndexBackingTableInfo
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _listenerWindow;
        private SupportUpdateListener _listenerWindowTwo;
        private SupportUpdateListener _listenerStmtOne;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _listenerWindow = new SupportUpdateListener();
            _listenerWindowTwo = new SupportUpdateListener();
            _listenerStmtOne = new SupportUpdateListener();
            SupportQueryPlanIndexHook.Reset();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listenerWindow = null;
            _listenerStmtOne = null;
            _listenerWindowTwo = null;
        }
    
        [Test]
        public void TestWindowUnidirectionalJoin() {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
    
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("on S1 as s1 delete from MyWindow where s1.p10 = TheString");
    
            var stmt = _epService.EPAdministrator.CreateEPL(
                    "select window(win.*) as c0," +
                    "window(win.*).where(v => v.IntPrimitive < 2) as c1, " +
                    "window(win.*).toMap(k=>k.TheString,v=>v.IntPrimitive) as c2 " +
                    "from S0 as s0 unidirectional, MyWindow as win");
            stmt.AddListener(_listenerStmtOne);
    
            var beans = new SupportBean[3];
            for (var i = 0; i < beans.Length; i++) {
                beans[i] = new SupportBean("E" + i, i);
            }
    
            _epService.EPRuntime.SendEvent(beans[0]);
            _epService.EPRuntime.SendEvent(beans[1]);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertReceived(beans, new int[]{0, 1}, new int[]{0, 1}, "E0,E1".Split(','), new object[] {0,1});
    
            // add bean
            _epService.EPRuntime.SendEvent(beans[2]);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertReceived(beans, new int[]{0, 1, 2}, new int[]{0, 1}, "E0,E1,E2".Split(','), new object[] {0,1, 2});
    
            // delete bean
            _epService.EPRuntime.SendEvent(new SupportBean_S1(11, "E1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(12));
            AssertReceived(beans, new int[]{0, 2}, new int[]{0}, "E0,E2".Split(','), new object[] {0,2});
    
            // delete another bean
            _epService.EPRuntime.SendEvent(new SupportBean_S1(13, "E0"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(14));
            AssertReceived(beans, new int[]{2}, new int[0], "E2".Split(','), new object[] {2});
    
            // delete last bean
            _epService.EPRuntime.SendEvent(new SupportBean_S1(15, "E2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(16));
            Assert.IsFalse(_listenerStmtOne.GetAndClearIsInvoked());
    
            // compile a non-unidirectional query, join and subquery
            _epService.EPAdministrator.CreateEPL("select window(win.*) from MyWindow as win");
            _epService.EPAdministrator.CreateEPL("select window(win.*) as c0 from S0.std:lastevent() as s0, MyWindow as win");
            _epService.EPAdministrator.CreateEPL("select (select window(win.*) from MyWindow as win) from S0");
        }
    
        private void AssertReceived(SupportBean[] beans, int[] indexesAll, int[] indexesWhere, string[] mapKeys, object[] mapValues) {
            var received = _listenerStmtOne.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsExactOrder(SupportBean.GetBeansPerIndex(beans, indexesAll), (object[]) received.Get("c0"));
            EPAssertionUtil.AssertEqualsExactOrder(SupportBean.GetBeansPerIndex(beans, indexesWhere), received.Get("c1").Unwrap<object>());
            EPAssertionUtil.AssertPropsMap((IDictionary<object, object>) received.Get("c2"), mapKeys, mapValues);
        }
    
        [Test]
        public void TestJoinIndexChoice() {
            _epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            _epService.EPAdministrator.Configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
    
            var preloadedEventsOne = new object[] {new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
            IndexAssertionEventSend eventSendAssertion = () =>
            {
                var fields = "ssb2.s2,ssb1.s1,ssb1.i1".Split(',');
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 50, 21, 22));
                EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E2", 20});
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 60, 11, 12));
                EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[] {"E1", "E1", 10});
            };
    
            // no index, since this is "unique(s1)" we don't need one
            var noindexes = new string[] {};
            AssertIndexChoice(noindexes, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "s1 = s2", true, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and l1 = l2", true, eventSendAssertion),
                    });
    
            // single index one field (duplicate in essence, since "unique(s1)"
            var indexOneField = new string[] {"create unique index One on MyWindow (s1)"};
            AssertIndexChoice(indexOneField, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "s1 = s2", true, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and l1 = l2", true, eventSendAssertion),
                    });
    
            // single index two field (includes "unique(s1)")
            var indexTwoField = new string[] {"create unique index One on MyWindow (s1, l1)"};
            AssertIndexChoice(indexTwoField, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "s1 = s2", true, eventSendAssertion),
                            new IndexAssertion(null, "d1 = d2", false, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and l1 = l2", true, eventSendAssertion),
                    });
    
            // two index one unique ("unique(s1)")
            var indexSetTwo = new string[] {
                    "create index One on MyWindow (s1)",
                    "create unique index Two on MyWindow (s1, d1)"};
            AssertIndexChoice(indexSetTwo, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "d1 = d2", false, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2", true, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and l1 = l2", true, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and d1 = d2 and l1 = l2", true, eventSendAssertion),
                    });
    
            // two index one unique ("win:keepall()")
            AssertIndexChoice(indexSetTwo, preloadedEventsOne, "win:keepall()",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "d1 = d2", false, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2", false, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and l1 = l2", false, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and d1 = d2 and l1 = l2", true, eventSendAssertion),
                            new IndexAssertion(null, "d1 = d2 and s1 = s2", true, eventSendAssertion),
                    });
        }
    
        private void AssertIndexChoice(string[] indexes, object[] preloadedEvents, string datawindow, params IndexAssertion[] assertions)
        {
            _epService.EPAdministrator.CreateEPL("create window MyWindow." + datawindow + " as SSB1");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SSB1");
            foreach (var index in indexes) {
                _epService.EPAdministrator.CreateEPL(index);
            }
            foreach (var @event in preloadedEvents) {
                _epService.EPRuntime.SendEvent(@event);
            }
    
            var count = 0;
            foreach (var assertion in assertions) {
                Log.Info("======= Testing #" + count++);
                var epl = INDEX_CALLBACK_HOOK +
                        (assertion.Hint ?? "") +
                        "select * " +
                        "from SSB2 as ssb2 unidirectional, MyWindow as ssb1 " +
                        "where " + assertion.WhereClause;
    
                EPStatement stmt;
                try {
                    stmt = _epService.EPAdministrator.CreateEPL(epl);
                    stmt.AddListener(_listenerStmtOne);
                }
                catch (EPStatementException ex) {
                    if (assertion.EventSendAssertion == null) {
                        // no assertion, expected
                        Assert.IsTrue(ex.Message.Contains("index hint busted"));
                        continue;
                    }
                    throw new EPRuntimeException("Unexpected statement exception: " + ex.Message, ex);
                }
    
                // assert index and access
                SupportQueryPlanIndexHook.AssertJoinOneStreamAndReset(assertion.IsUnique.GetValueOrDefault());
                assertion.EventSendAssertion.Invoke();
                stmt.Dispose();
            }
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        [Test]
        public void TestInnerJoinLateStart() {
            RunAssertionInnerJoinLateStart(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionInnerJoinLateStart(EventRepresentationEnum.MAP);
            RunAssertionInnerJoinLateStart(EventRepresentationEnum.DEFAULT);
        }
    
        private void RunAssertionInnerJoinLateStart(EventRepresentationEnum eventRepresentationEnum) {
    
            var stmtOne = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema Product (product string, size int)");
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmtOne.EventType.UnderlyingType);
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema Portfolio (portfolio string, product string)");
            var stmtTwo = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window ProductWin.win:keepall() as Product");
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmtTwo.EventType.UnderlyingType);
    
            _epService.EPAdministrator.CreateEPL("insert into ProductWin select * from Product");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window PortfolioWin.win:keepall() as Portfolio");
            _epService.EPAdministrator.CreateEPL("insert into PortfolioWin select * from Portfolio");
    
            SendProduct(eventRepresentationEnum, "productA", 1);
            SendProduct(eventRepresentationEnum, "productB", 2);
            SendPortfolio(eventRepresentationEnum, "Portfolio", "productA");
    
            var stmtText = "@Name(\"Query2\") select portfolio, ProductWin.product, size " +
                    "from PortfolioWin unidirectional inner join ProductWin on PortfolioWin.product=ProductWin.product";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(_listenerStmtOne);
    
            SendPortfolio(eventRepresentationEnum, "Portfolio", "productB");
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), new string[]{"portfolio", "ProductWin.product", "size"}, new object[]{"Portfolio", "productB", 2});
    
            SendPortfolio(eventRepresentationEnum, "Portfolio", "productC");
            _listenerStmtOne.Reset();
    
            SendProduct(eventRepresentationEnum, "productC", 3);
            SendPortfolio(eventRepresentationEnum, "Portfolio", "productC");
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), new string[]{"portfolio", "ProductWin.product", "size"}, new object[]{"Portfolio", "productC", 3});
    
            _epService.Initialize();
        }
    
        private void SendProduct(EventRepresentationEnum eventRepresentationEnum, string product, int size) {
            IDictionary<String, object> theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("product", product);
            theEvent.Put("size", size);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), "Product");
            }
            else {
                _epService.EPRuntime.SendEvent(theEvent, "Product");
            }
        }
    
        private void SendPortfolio(EventRepresentationEnum eventRepresentationEnum, string portfolio, string product) {
            IDictionary<String, object> theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("portfolio", portfolio);
            theEvent.Put("product", product);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), "Portfolio");
            }
            else {
                _epService.EPRuntime.SendEvent(theEvent, "Portfolio");
            }
        }
    
        [Test]
        public void TestRightOuterJoinLateStart()
        {
            // Test for ESPER-186 Iterator not honoring order by clause for grouped join query with output-rate clause
            // Test for ESPER-187 Join of two or more named windows on late start may not return correct aggregation state on iterate
    
            // create window for Leave events
            var stmtTextCreate = "create window WindowLeave.win:time(6000) as select timeLeave, id, location from " + typeof(SupportQueueLeave).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var stmtTextInsert = "insert into WindowLeave select timeLeave, id, location from " + typeof(SupportQueueLeave).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create second window for enter events
            stmtTextCreate = "create window WindowEnter.win:time(6000) as select location, sku, timeEnter, id from " + typeof(SupportQueueEnter).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtTextInsert = "insert into WindowEnter select location, sku, timeEnter, id from " + typeof(SupportQueueEnter).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // fill data
            for (var i = 0; i < 8; i++)
            {
                var location = Convert.ToString(i / 2);
                _epService.EPRuntime.SendEvent(new SupportQueueLeave(i + 1, location, 247));
            }

            // Console.WriteLine("Leave events:");
            // {
            //     EventBean event = it.next();
            //     Console.WriteLine(event.get("timeLeave") +
            //     " " + event.get("id") +
            //     " " + event.get("location"));
            // }
    
            for (var i = 0; i < 10; i++)
            {
                var location = Convert.ToString(i / 2);
                var sku = (i % 2 == 0) ? "166583" : "169254";
                _epService.EPRuntime.SendEvent(new SupportQueueEnter(i + 1, location, sku, 123));
            }

            // Console.WriteLine("Enter events:");
            // {
            //     EventBean event = it.next();
            //     Console.WriteLine(event.get("timeEnter") +
            //     " " + event.get("id") +
            //     " " + event.get("sku") +
            //     " " + event.get("location"));
            // }
    
            var stmtTextOne = "select s1.location as loc, sku, avg((coalesce(timeLeave, 250) - timeEnter)) as avgTime, " +
                              "count(timeEnter) as cntEnter, count(timeLeave) as cntLeave, (count(timeEnter) - count(timeLeave)) as diff " +
                              "from WindowLeave as s0 right outer join WindowEnter as s1 " +
                              "on s0.id = s1.id and s0.location = s1.location " +
                              "group by s1.location, sku " +
                              "output every 1.0 seconds " +
                              "order by s1.location, sku";
            var stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            var stmtTextTwo = "select s1.location as loc, sku, avg((coalesce(timeLeave, 250) - timeEnter)) as avgTime, " +
                              "count(timeEnter) as cntEnter, count(timeLeave) as cntLeave, (count(timeEnter) - count(timeLeave)) as diff " +
                              "from WindowEnter as s1 left outer join WindowLeave as s0 " +
                              "on s0.id = s1.id and s0.location = s1.location " +
                              "group by s1.location, sku " +
                              "output every 1.0 seconds " +
                              "order by s1.location, sku";
            var stmtTwo = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
    
            // Console.WriteLine("Statement 1");
            // {
            //     EventBean event = it.next();
            //     Console.WriteLine("loc " + event.get("loc") +
            //         " sku " + event.get("sku") +
            //         " avgTime " + event.get("avgTime") +
            //         " cntEnter " + event.get("cntEnter") +
            //         " cntLeave " + event.get("cntLeave") +
            //         " diff " + event.get("diff"));
            // }
            
            var expected = new object[][] {
                            new object[]{"0", "166583", 124.0, 1L, 1L, 0L},
                            new object[]{"0", "169254", 124.0, 1L, 1L, 0L},
                            new object[]{"1", "166583", 124.0, 1L, 1L, 0L},
                            new object[]{"1", "169254", 124.0, 1L, 1L, 0L},
                            new object[]{"2", "166583", 124.0, 1L, 1L, 0L},
                            new object[]{"2", "169254", 124.0, 1L, 1L, 0L},
                            new object[]{"3", "166583", 124.0, 1L, 1L, 0L},
                            new object[]{"3", "169254", 124.0, 1L, 1L, 0L},
                            new object[]{"4", "166583", 127.0, 1L, 0L, 1L},
                            new object[]{"4", "169254", 127.0, 1L, 0L, 1L}
                        };
    
            // assert iterator results
            var received = EPAssertionUtil.EnumeratorToArray(stmtTwo.GetEnumerator());
            EPAssertionUtil.AssertPropsPerRow(received, "loc,sku,avgTime,cntEnter,cntLeave,diff".Split(','), expected);
            received = EPAssertionUtil.EnumeratorToArray(stmtOne.GetEnumerator());
            EPAssertionUtil.AssertPropsPerRow(received, "loc,sku,avgTime,cntEnter,cntLeave,diff".Split(','), expected);
        }
    
        [Test]
        public void TestFullOuterJoinNamedAggregationLateStart()
        {
            // create window
            var stmtTextCreate = "create window MyWindow.std:groupwin(TheString, IntPrimitive).win:length(3) as select TheString, IntPrimitive, BoolPrimitive from " + typeof(SupportBean).FullName;
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create insert into
            var stmtTextInsert = "insert into MyWindow select TheString, IntPrimitive, BoolPrimitive from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // fill window
            var stringValues = new string[] {"c0", "c1", "c2"};
            for (var i = 0; i < stringValues.Length; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    for (var k = 0; k < 2; k++)
                    {
                        var beanX = new SupportBean(stringValues[i], j);
                        beanX.BoolPrimitive = true;
                        _epService.EPRuntime.SendEvent(beanX);
                    }
                }
            }
            var bean = new SupportBean("c1", 2);
            bean.BoolPrimitive = true;
            _epService.EPRuntime.SendEvent(bean);

            var received = EPAssertionUtil.EnumeratorToArray(stmtCreate.GetEnumerator());
            Assert.AreEqual(19, received.Length);
    
            // create select stmt
            var stmtTextSelect = "select TheString, IntPrimitive, count(BoolPrimitive) as cntBool, symbol " +
                                    "from MyWindow full outer join " + typeof(SupportMarketDataBean).FullName + ".win:keepall() " +
                                    "on TheString = symbol " +
                                    "group by TheString, IntPrimitive, symbol order by TheString, IntPrimitive, symbol";
            var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
    
            // send outer join events
            this.SendMarketBean("c0");
            this.SendMarketBean("c3");
    
            // get iterator results
            received = EPAssertionUtil.EnumeratorToArray(stmtSelect.GetEnumerator());
            EPAssertionUtil.AssertPropsPerRow(received, "TheString,IntPrimitive,cntBool,symbol".Split(','),
                    new object[][]{
                            new object[]{null, null, 0L, "c3"},
                            new object[]{"c0", 0, 2L, "c0"},
                            new object[]{"c0", 1, 2L, "c0"},
                            new object[]{"c0", 2, 2L, "c0"},
                            new object[]{"c1", 0, 2L, null},
                            new object[]{"c1", 1, 2L, null},
                            new object[]{"c1", 2, 3L, null},
                            new object[]{"c2", 0, 2L, null},
                            new object[]{"c2", 1, 2L, null},
                            new object[]{"c2", 2, 2L, null},
                    });
            /*
            for (int i = 0; i < received.length; i++)
            {
                Console.WriteLine("string=" + received[i].get("string") +
                        " IntPrimitive=" + received[i].get("IntPrimitive") +
                        " cntBool=" + received[i].get("cntBool") +
                        " symbol=" + received[i].get("symbol"));
            }
            */
    
            stmtSelect.Dispose();
            stmtCreate.Dispose();
    
        }
    
        [Test]
        public void TestJoinNamedAndStream()
        {
            // create window
            var stmtTextCreate = "create window MyWindow.win:keepall() as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create delete stmt
            var stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " delete from MyWindow where id = a";
            _epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyWindow select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            var fields = new string[] {"symbol", "a", "b"};
            var stmtTextSelectOne = "select irstream symbol, a, b " +
                                       " from " + typeof(SupportMarketDataBean).FullName + ".win:length(10) as s0," +
                                                 "MyWindow as s1 where s1.a = symbol";
            var stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
            EPAssertionUtil.AssertEqualsAnyOrder(stmtSelectOne.EventType.PropertyNames, new string[]{"symbol", "a", "b"});
            Assert.AreEqual(typeof(string), stmtSelectOne.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(string), stmtSelectOne.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int?), stmtSelectOne.EventType.GetPropertyType("b"));
    
            SendMarketBean("S1");
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendSupportBean("S1", 1);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", "S1", 1});
    
            SendSupportBean_A("S1"); // deletes from window
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"S1", "S1", 1});
    
            SendMarketBean("S1");
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendSupportBean("S2", 2);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendMarketBean("S2");
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S2", "S2", 2});
    
            SendSupportBean("S3", 3);
            SendSupportBean("S3", 4);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendMarketBean("S3");
            Assert.AreEqual(2, _listenerStmtOne.LastNewData.Length);
            _listenerStmtOne.Reset();
    
            SendSupportBean_A("S3"); // deletes from window
            Assert.AreEqual(2, _listenerStmtOne.LastOldData.Length);
            _listenerStmtOne.Reset();
    
            SendMarketBean("S3");
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
        }
    
        [Test]
        public void TestJoinBetweenNamed()
        {
            var fields = new string[] {"a1", "b1", "a2", "b2"};
    
            // create window
            var stmtTextCreateOne = "create window MyWindowOne.win:keepall() as select TheString as a1, IntPrimitive as b1 from " + typeof(SupportBean).FullName;
            var stmtCreateOne = _epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            stmtCreateOne.AddListener(_listenerWindow);
    
            // create window
            var stmtTextCreateTwo = "create window MyWindowTwo.win:keepall() as select TheString as a2, IntPrimitive as b2 from " + typeof(SupportBean).FullName;
            var stmtCreateTwo = _epService.EPAdministrator.CreateEPL(stmtTextCreateTwo);
            stmtCreateTwo.AddListener(_listenerWindowTwo);
    
            // create delete stmt
            var stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + "(volume=1) delete from MyWindowOne where symbol = a1";
            _epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + "(volume=0) delete from MyWindowTwo where symbol = a2";
            _epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            var stmtTextInsert = "insert into MyWindowOne select TheString as a1, IntPrimitive as b1 from " + typeof(SupportBean).FullName + "(BoolPrimitive = true)";
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            stmtTextInsert = "insert into MyWindowTwo select TheString as a2, IntPrimitive as b2 from " + typeof(SupportBean).FullName + "(BoolPrimitive = false)";
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumers
            var stmtTextSelectOne = "select irstream a1, b1, a2, b2 " +
                                       " from MyWindowOne as s0," +
                                             "MyWindowTwo as s1 where s0.a1 = s1.a2";
            var stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            SendSupportBean(true, "S0", 1);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendSupportBean(false, "S0", 2);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S0", 1, "S0", 2});
    
            SendSupportBean(false, "S1", 3);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendSupportBean(true, "S1", 4);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 4, "S1", 3});
    
            SendSupportBean(true, "S1", 5);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 5, "S1", 3});
    
            SendSupportBean(false, "S1", 6);
            Assert.AreEqual(2, _listenerStmtOne.LastNewData.Length);
            _listenerStmtOne.Reset();
    
            // delete and insert back in
            SendMarketBean("S0", 0);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"S0", 1, "S0", 2});
    
            SendSupportBean(false, "S0", 7);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S0", 1, "S0", 7});
    
            // delete and insert back in
            SendMarketBean("S0", 1);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"S0", 1, "S0", 7});
    
            SendSupportBean(true, "S0", 8);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S0", 8, "S0", 7});
        }
    
        [Test]
        public void TestJoinBetweenSameNamed()
        {
            var fields = new string[] {"a0", "b0", "a1", "b1"};
    
            // create window
            var stmtTextCreateOne = "create window MyWindow.win:keepall() as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            var stmtCreateOne = _epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            stmtCreateOne.AddListener(_listenerWindow);
    
            // create delete stmt
            var stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where symbol = a";
            _epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            var stmtTextInsert = "insert into MyWindow select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumers
            var stmtTextSelectOne = "select irstream s0.a as a0, s0.b as b0, s1.a as a1, s1.b as b1 " +
                                       " from MyWindow as s0," +
                                             "MyWindow as s1 where s0.a = s1.a";
            var stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            SendSupportBean("E1", 1);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1, "E1", 1});
    
            SendSupportBean("E2", 2);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2, "E2", 2});
    
            SendMarketBean("E1", 1);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1, "E1", 1});
    
            SendMarketBean("E0", 0);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
        }
    
        [Test]
        public void TestJoinSingleInsertOneWindow()
        {
            var fields = new string[] {"a1", "b1", "a2", "b2"};
    
            // create window
            var stmtTextCreateOne = "create window MyWindowOne.win:keepall() as select TheString as a1, IntPrimitive as b1 from " + typeof(SupportBean).FullName;
            var stmtCreateOne = _epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            stmtCreateOne.AddListener(_listenerWindow);
    
            // create window
            var stmtTextCreateTwo = "create window MyWindowTwo.win:keepall() as select TheString as a2, IntPrimitive as b2 from " + typeof(SupportBean).FullName;
            var stmtCreateTwo = _epService.EPAdministrator.CreateEPL(stmtTextCreateTwo);
            stmtCreateTwo.AddListener(_listenerWindowTwo);
    
            // create delete stmt
            var stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + "(volume=1) delete from MyWindowOne where symbol = a1";
            _epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + "(volume=0) delete from MyWindowTwo where symbol = a2";
            _epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            var stmtTextInsert = "insert into MyWindowOne select TheString as a1, IntPrimitive as b1 from " + typeof(SupportBean).FullName + "(BoolPrimitive = true)";
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            stmtTextInsert = "insert into MyWindowTwo select TheString as a2, IntPrimitive as b2 from " + typeof(SupportBean).FullName + "(BoolPrimitive = false)";
            _epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumers
            var stmtTextSelectOne = "select irstream a1, b1, a2, b2 " +
                                       " from MyWindowOne as s0," +
                                             "MyWindowTwo as s1 where s0.a1 = s1.a2";
            var stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listenerStmtOne);
    
            SendSupportBean(true, "S0", 1);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendSupportBean(false, "S0", 2);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S0", 1, "S0", 2});
    
            SendSupportBean(false, "S1", 3);
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            SendSupportBean(true, "S1", 4);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 4, "S1", 3});
    
            SendSupportBean(true, "S1", 5);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 5, "S1", 3});
    
            SendSupportBean(false, "S1", 6);
            Assert.AreEqual(2, _listenerStmtOne.LastNewData.Length);
            _listenerStmtOne.Reset();
    
            // delete and insert back in
            SendMarketBean("S0", 0);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"S0", 1, "S0", 2});
    
            SendSupportBean(false, "S0", 7);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S0", 1, "S0", 7});
    
            // delete and insert back in
            SendMarketBean("S0", 1);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"S0", 1, "S0", 7});
    
            SendSupportBean(true, "S0", 8);
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S0", 8, "S0", 7});
        }
    
        [Test]
        public void TestUnidirectional()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
    
            var stmtOne = _epService.EPAdministrator.CreateEPL("select w.* from MyWindow w unidirectional, SupportBean_A.std:lastevent() s where s.id = w.TheString");
            stmtOne.AddListener(_listenerStmtOne);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
            _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            Assert.IsFalse(_listenerStmtOne.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsTrue(_listenerStmtOne.IsInvoked);
        }
    
        private SupportBean_A SendSupportBean_A(string id)
        {
            var bean = new SupportBean_A(id);
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendSupportBean(string theString, int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendSupportBean(bool boolPrimitive, string theString, int intPrimitive)
        {
            var bean = new SupportBean();
            bean.BoolPrimitive = boolPrimitive;
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void SendMarketBean(string symbol)
        {
            var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketBean(string symbol, long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
