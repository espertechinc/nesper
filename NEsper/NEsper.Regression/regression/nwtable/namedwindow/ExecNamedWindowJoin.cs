///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avro.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    using Map = IDictionary<string, object>;

    public class ExecNamedWindowJoin : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionWindowUnidirectionalJoin(epService);
            RunAssertionJoinIndexChoice(epService);
            RunAssertionInnerJoinLateStart(epService);
            RunAssertionRightOuterJoinLateStart(epService);
            RunAssertionFullOuterJoinNamedAggregationLateStart(epService);
            RunAssertionJoinNamedAndStream(epService);
            RunAssertionJoinBetweenNamed(epService);
            RunAssertionJoinBetweenSameNamed(epService);
            RunAssertionJoinSingleInsertOneWindow(epService);
            RunAssertionUnidirectional(epService);
        }
    
        private void RunAssertionWindowUnidirectionalJoin(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
    
            epService.EPAdministrator.CreateEPL("create window MyWindowWUJ#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowWUJ select * from SupportBean");
            epService.EPAdministrator.CreateEPL("on S1 as s1 delete from MyWindowWUJ where s1.p10 = TheString");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select window(win.*) as c0," +
                            "window(win.*).where(v => v.IntPrimitive < 2) as c1, " +
                            "window(win.*).ToMap(k=>k.TheString,v=>v.IntPrimitive) as c2 " +
                            "from S0 as s0 unidirectional, MyWindowWUJ as win");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var beans = new SupportBean[3];
            for (int i = 0; i < beans.Length; i++) {
                beans[i] = new SupportBean("E" + i, i);
            }
    
            epService.EPRuntime.SendEvent(beans[0]);
            epService.EPRuntime.SendEvent(beans[1]);
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertReceived(listener, beans, new int[]{0, 1}, new int[]{0, 1}, "E0,E1".Split(','), new object[]{0, 1});
    
            // add bean
            epService.EPRuntime.SendEvent(beans[2]);
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertReceived(listener, beans, new int[]{0, 1, 2}, new int[]{0, 1}, "E0,E1,E2".Split(','), new object[]{0, 1, 2});
    
            // delete bean
            epService.EPRuntime.SendEvent(new SupportBean_S1(11, "E1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(12));
            AssertReceived(listener, beans, new int[]{0, 2}, new int[]{0}, "E0,E2".Split(','), new object[]{0, 2});
    
            // delete another bean
            epService.EPRuntime.SendEvent(new SupportBean_S1(13, "E0"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(14));
            AssertReceived(listener, beans, new int[]{2}, new int[0], "E2".Split(','), new object[]{2});
    
            // delete last bean
            epService.EPRuntime.SendEvent(new SupportBean_S1(15, "E2"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(16));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            // compile a non-unidirectional query, join and subquery
            epService.EPAdministrator.CreateEPL("select window(win.*) from MyWindowWUJ as win");
            epService.EPAdministrator.CreateEPL("select window(win.*) as c0 from S0#lastevent as s0, MyWindowWUJ as win");
            epService.EPAdministrator.CreateEPL("select (select window(win.*) from MyWindowWUJ as win) from S0");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertReceived(SupportUpdateListener listenerStmtOne, SupportBean[] beans, int[] indexesAll, int[] indexesWhere, string[] mapKeys, object[] mapValues) {
            EventBean received = listenerStmtOne.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsExactOrder(SupportBean.GetBeansPerIndex(beans, indexesAll), received.Get("c0").UnwrapIntoArray<object>());
            EPAssertionUtil.AssertEqualsExactOrder(SupportBean.GetBeansPerIndex(beans, indexesWhere), received.Get("c1").UnwrapIntoList<object>());
            EPAssertionUtil.AssertPropsMap((IDictionary<object, object>) received.Get("c2"), mapKeys, mapValues);
        }
    
        private void RunAssertionJoinIndexChoice(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            epService.EPAdministrator.Configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
    
            var preloadedEventsOne = new object[]{new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
            var listener = new SupportUpdateListener();
            var eventSendAssertion = new IndexAssertionEventSend(() => {
                string[] fields = "ssb2.s2,ssb1.s1,ssb1.i1".Split(',');
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 50, 21, 22));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E2", 20});
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 60, 11, 12));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 10});
            });
    
            // no index, since this is "Unique(s1)" we don't need one
            var noindexes = new string[]{};
            AssertIndexChoice(epService, listener, noindexes, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "s1 = s2", true, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and l1 = l2", true, eventSendAssertion),
                    });
    
            // single index one field (duplicate in essence, since "Unique(s1)"
            var indexOneField = new string[]{"create unique index One on MyWindow (s1)"};
            AssertIndexChoice(epService, listener, indexOneField, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "s1 = s2", true, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and l1 = l2", true, eventSendAssertion),
                    });
    
            // single index two field (includes "Unique(s1)")
            var indexTwoField = new string[]{"create unique index One on MyWindow (s1, l1)"};
            AssertIndexChoice(epService, listener, indexTwoField, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "s1 = s2", true, eventSendAssertion),
                            new IndexAssertion(null, "d1 = d2", false, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and l1 = l2", true, eventSendAssertion),
                    });
    
            // two index one unique ("Unique(s1)")
            var indexSetTwo = new string[]{
                    "create index One on MyWindow (s1)",
                    "create unique index Two on MyWindow (s1, d1)"};
            AssertIndexChoice(epService, listener, indexSetTwo, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "d1 = d2", false, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2", true, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and l1 = l2", true, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and d1 = d2 and l1 = l2", true, eventSendAssertion),
                    });
    
            // two index one unique ("win:keepall()")
            AssertIndexChoice(epService, listener, indexSetTwo, preloadedEventsOne, "win:keepall()",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "d1 = d2", false, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2", false, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and l1 = l2", false, eventSendAssertion),
                            new IndexAssertion(null, "s1 = s2 and d1 = d2 and l1 = l2", true, eventSendAssertion),
                            new IndexAssertion(null, "d1 = d2 and s1 = s2", true, eventSendAssertion),
                    });
        }
    
        private void AssertIndexChoice(EPServiceProvider epService, SupportUpdateListener listener, string[] indexes, object[] preloadedEvents, string datawindow,
                                       params IndexAssertion[] assertions) {
            epService.EPAdministrator.CreateEPL("create window MyWindow." + datawindow + " as SSB1");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SSB1");
            foreach (string index in indexes) {
                epService.EPAdministrator.CreateEPL(index);
            }
            foreach (Object @event in preloadedEvents) {
                epService.EPRuntime.SendEvent(@event);
            }
    
            int count = 0;
            foreach (IndexAssertion assertion in assertions) {
                Log.Info("======= Testing #" + count++);
                string epl = INDEX_CALLBACK_HOOK +
                        (assertion.Hint == null ? "" : assertion.Hint) +
                        "select * " +
                        "from SSB2 as ssb2 unidirectional, MyWindow as ssb1 " +
                        "where " + assertion.WhereClause;
    
                EPStatement stmt;
                try {
                    stmt = epService.EPAdministrator.CreateEPL(epl);
                    stmt.Events += listener.Update;
                } catch (EPStatementException ex) {
                    if (assertion.EventSendAssertion == null) {
                        // no assertion, expected
                        Assert.IsTrue(ex.Message.Contains("index hint busted"));
                        continue;
                    }
                    throw new EPRuntimeException("Unexpected statement exception: " + ex.Message, ex);
                }
    
                // assert index and access
                SupportQueryPlanIndexHook.AssertJoinOneStreamAndReset(assertion.IsUnique);
                assertion.EventSendAssertion.Invoke();
                stmt.Dispose();
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInnerJoinLateStart(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionInnerJoinLateStart(epService, rep);
            }
        }
    
        private void TryAssertionInnerJoinLateStart(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema Product (product string, size int)");
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtOne.EventType.UnderlyingType));
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema Portfolio (portfolio string, product string)");
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window ProductWin#keepall as Product");
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtTwo.EventType.UnderlyingType));
    
            epService.EPAdministrator.CreateEPL("insert into ProductWin select * from Product");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window PortfolioWin#keepall as Portfolio");
            epService.EPAdministrator.CreateEPL("insert into PortfolioWin select * from Portfolio");
    
            SendProduct(epService, eventRepresentationEnum, "productA", 1);
            SendProduct(epService, eventRepresentationEnum, "productB", 2);
            SendPortfolio(epService, eventRepresentationEnum, "Portfolio", "productA");
    
            string stmtText = "@Name(\"Query2\") select portfolio, ProductWin.product, size " +
                    "from PortfolioWin unidirectional inner join ProductWin on PortfolioWin.product=ProductWin.product";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendPortfolio(epService, eventRepresentationEnum, "Portfolio", "productB");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), new string[]{"portfolio", "ProductWin.product", "size"}, new object[]{"Portfolio", "productB", 2});
    
            SendPortfolio(epService, eventRepresentationEnum, "Portfolio", "productC");
            listener.Reset();
    
            SendProduct(epService, eventRepresentationEnum, "productC", 3);
            SendPortfolio(epService, eventRepresentationEnum, "Portfolio", "productC");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), new string[]{"portfolio", "ProductWin.product", "size"}, new object[]{"Portfolio", "productC", 3});
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in "Product,Portfolio,ProductWin,PortfolioWin".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void SendProduct(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string product, int size) {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{product, size}, "Product");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var theEvent = new LinkedHashMap<string, Object>();
                theEvent.Put("product", product);
                theEvent.Put("size", size);
                epService.EPRuntime.SendEvent(theEvent, "Product");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, "Product").AsRecordSchema());
                theEvent.Put("product", product);
                theEvent.Put("size", size);
                epService.EPRuntime.SendEventAvro(theEvent, "Product");
            } else {
                Assert.Fail();
            }
        }
    
        private void SendPortfolio(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string portfolio, string product) {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{portfolio, product}, "Portfolio");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var theEvent = new LinkedHashMap<string, Object>();
                theEvent.Put("portfolio", portfolio);
                theEvent.Put("product", product);
                epService.EPRuntime.SendEvent(theEvent, "Portfolio");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, "Portfolio").AsRecordSchema());
                theEvent.Put("portfolio", portfolio);
                theEvent.Put("product", product);
                epService.EPRuntime.SendEventAvro(theEvent, "Portfolio");
            } else {
                Assert.Fail();
            }
        }
    
        private void RunAssertionRightOuterJoinLateStart(EPServiceProvider epService) {
            // Test for ESPER-186 Iterator not honoring order by clause for grouped join query with output-rate clause
            // Test for ESPER-187 Join of two or more named windows on late start may not return correct aggregation state on iterate
    
            // create window for Leave events
            string stmtTextCreate = "create window WindowLeave#time(6000) as select timeLeave, id, location from " + typeof(SupportQueueLeave).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextCreate);
            string stmtTextInsert = "insert into WindowLeave select timeLeave, id, location from " + typeof(SupportQueueLeave).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create second window for enter events
            stmtTextCreate = "create window WindowEnter#time(6000) as select location, sku, timeEnter, id from " + typeof(SupportQueueEnter).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtTextInsert = "insert into WindowEnter select location, sku, timeEnter, id from " + typeof(SupportQueueEnter).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // fill data
            for (int i = 0; i < 8; i++) {
                string location = Convert.ToString(i / 2);
                epService.EPRuntime.SendEvent(new SupportQueueLeave(i + 1, location, 247));
            }
    
            for (int i = 0; i < 10; i++) {
                string location = Convert.ToString(i / 2);
                string sku = (i % 2 == 0) ? "166583" : "169254";
                epService.EPRuntime.SendEvent(new SupportQueueEnter(i + 1, location, sku, 123));
            }

            string stmtTextOne = "select s1.location as loc, sku, avg((coalesce(timeLeave, 250) - timeEnter)) as avgTime, " +
                    "count(timeEnter) as cntEnter, count(timeLeave) as cntLeave, (count(timeEnter) - count(timeLeave)) as diff " +
                    "from WindowLeave as s0 right outer join WindowEnter as s1 " +
                    "on s0.id = s1.id and s0.location = s1.location " +
                    "group by s1.location, sku " +
                    "output every 1.0 seconds " +
                    "order by s1.location, sku";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            string stmtTextTwo = "select s1.location as loc, sku, avg((coalesce(timeLeave, 250) - timeEnter)) as avgTime, " +
                    "count(timeEnter) as cntEnter, count(timeLeave) as cntLeave, (count(timeEnter) - count(timeLeave)) as diff " +
                    "from WindowEnter as s1 left outer join WindowLeave as s0 " +
                    "on s0.id = s1.id and s0.location = s1.location " +
                    "group by s1.location, sku " +
                    "output every 1.0 seconds " +
                    "order by s1.location, sku";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTextTwo);
    
            var expected = new object[][]{
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
            EventBean[] received = EPAssertionUtil.EnumeratorToArray(stmtTwo.GetEnumerator());
            EPAssertionUtil.AssertPropsPerRow(received, "loc,sku,avgTime,cntEnter,cntLeave,diff".Split(','), expected);
            received = EPAssertionUtil.EnumeratorToArray(stmtOne.GetEnumerator());
            EPAssertionUtil.AssertPropsPerRow(received, "loc,sku,avgTime,cntEnter,cntLeave,diff".Split(','), expected);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFullOuterJoinNamedAggregationLateStart(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowFO#groupwin(TheString, IntPrimitive)#length(3) as select TheString, IntPrimitive, BoolPrimitive from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowFO select TheString, IntPrimitive, BoolPrimitive from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // fill window
            var stringValues = new string[]{"c0", "c1", "c2"};
            for (int i = 0; i < stringValues.Length; i++) {
                for (int j = 0; j < 3; j++) {
                    for (int k = 0; k < 2; k++) {
                        var beanX = new SupportBean(stringValues[i], j);
                        beanX.BoolPrimitive = true;
                        epService.EPRuntime.SendEvent(beanX);
                    }
                }
            }
            var bean = new SupportBean("c1", 2);
            bean.BoolPrimitive = true;
            epService.EPRuntime.SendEvent(bean);
    
            EventBean[] received = EPAssertionUtil.EnumeratorToArray(stmtCreate.GetEnumerator());
            Assert.AreEqual(19, received.Length);
    
            // create select stmt
            string stmtTextSelect = "select TheString, IntPrimitive, count(BoolPrimitive) as cntBool, symbol " +
                    "from MyWindowFO full outer join " + typeof(SupportMarketDataBean).FullName + "#keepall " +
                    "on TheString = symbol " +
                    "group by TheString, IntPrimitive, symbol order by TheString, IntPrimitive, symbol";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
    
            // send outer join events
            this.SendMarketBean(epService, "c0");
            this.SendMarketBean(epService, "c3");
    
            // get iterator results
            received = EPAssertionUtil.EnumeratorToArray(stmtSelect.GetEnumerator());
            EPAssertionUtil.AssertPropsPerRow(received, "TheString,IntPrimitive,cntBool,symbol".Split(','),
                    new object[][]{
                        new object[] {null, null, 0L, "c3"},
                        new object[] {"c0", 0, 2L, "c0"},
                        new object[] {"c0", 1, 2L, "c0"},
                        new object[] {"c0", 2, 2L, "c0"},
                        new object[] {"c1", 0, 2L, null},
                        new object[] {"c1", 1, 2L, null},
                        new object[] {"c1", 2, 3L, null},
                        new object[] {"c2", 0, 2L, null},
                        new object[] {"c2", 1, 2L, null},
                        new object[] {"c2", 2, 2L, null},
                    });
            /*
            for (int i = 0; i < received.Length; i++)
            {
                Log.Info("string=" + received[i].Get("string") +
                        " IntPrimitive=" + received[i].Get("IntPrimitive") +
                        " cntBool=" + received[i].Get("cntBool") +
                        " symbol=" + received[i].Get("symbol"));
            }
            */
    
            stmtSelect.Dispose();
            stmtCreate.Dispose();
        }
    
        private void RunAssertionJoinNamedAndStream(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowJNS#keepall as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " delete from MyWindowJNS where id = a";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowJNS select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            var fields = new string[]{"symbol", "a", "b"};
            string stmtTextSelectOne = "select irstream symbol, a, b " +
                    " from " + typeof(SupportMarketDataBean).FullName + "#length(10) as s0," +
                    "MyWindowJNS as s1 where s1.a = symbol";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listener = new SupportUpdateListener();
            stmtSelectOne.Events += listener.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(stmtSelectOne.EventType.PropertyNames, new string[]{"symbol", "a", "b"});
            Assert.AreEqual(typeof(string), stmtSelectOne.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(string), stmtSelectOne.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int), stmtSelectOne.EventType.GetPropertyType("b"));
    
            SendMarketBean(epService, "S1");
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "S1", 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S1", "S1", 1});
    
            SendSupportBean_A(epService, "S1"); // deletes from window
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"S1", "S1", 1});
    
            SendMarketBean(epService, "S1");
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "S2", 2);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketBean(epService, "S2");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S2", "S2", 2});
    
            SendSupportBean(epService, "S3", 3);
            SendSupportBean(epService, "S3", 4);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketBean(epService, "S3");
            Assert.AreEqual(2, listener.LastNewData.Length);
            listener.Reset();
    
            SendSupportBean_A(epService, "S3"); // deletes from window
            Assert.AreEqual(2, listener.LastOldData.Length);
            listener.Reset();
    
            SendMarketBean(epService, "S3");
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void RunAssertionJoinBetweenNamed(EPServiceProvider epService) {
            var fields = new string[]{"a1", "b1", "a2", "b2"};
    
            // create window
            string stmtTextCreateOne = "create window MyWindowOne#keepall as select TheString as a1, IntPrimitive as b1 from " + typeof(SupportBean).FullName;
            EPStatement stmtCreateOne = epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            var listenerWindow = new SupportUpdateListener();
            stmtCreateOne.Events += listenerWindow.Update;
    
            // create window
            string stmtTextCreateTwo = "create window MyWindowTwo#keepall as select TheString as a2, IntPrimitive as b2 from " + typeof(SupportBean).FullName;
            EPStatement stmtCreateTwo = epService.EPAdministrator.CreateEPL(stmtTextCreateTwo);
            var listenerWindowTwo = new SupportUpdateListener();
            stmtCreateTwo.Events += listenerWindowTwo.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + "(volume=1) delete from MyWindowOne where symbol = a1";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + "(volume=0) delete from MyWindowTwo where symbol = a2";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowOne select TheString as a1, IntPrimitive as b1 from " + typeof(SupportBean).FullName + "(BoolPrimitive = true)";
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            stmtTextInsert = "insert into MyWindowTwo select TheString as a2, IntPrimitive as b2 from " + typeof(SupportBean).FullName + "(BoolPrimitive = false)";
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumers
            string stmtTextSelectOne = "select irstream a1, b1, a2, b2 " +
                    " from MyWindowOne as s0," +
                    "MyWindowTwo as s1 where s0.a1 = s1.a2";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listener = new SupportUpdateListener();
            stmtSelectOne.Events += listener.Update;
    
            SendSupportBean(epService, true, "S0", 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, false, "S0", 2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0", 1, "S0", 2});
    
            SendSupportBean(epService, false, "S1", 3);
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, true, "S1", 4);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S1", 4, "S1", 3});
    
            SendSupportBean(epService, true, "S1", 5);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S1", 5, "S1", 3});
    
            SendSupportBean(epService, false, "S1", 6);
            Assert.AreEqual(2, listener.LastNewData.Length);
            listener.Reset();
    
            // delete and insert back in
            SendMarketBean(epService, "S0", 0);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"S0", 1, "S0", 2});
    
            SendSupportBean(epService, false, "S0", 7);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0", 1, "S0", 7});
    
            // delete and insert back in
            SendMarketBean(epService, "S0", 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"S0", 1, "S0", 7});
    
            SendSupportBean(epService, true, "S0", 8);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0", 8, "S0", 7});
        }
    
        private void RunAssertionJoinBetweenSameNamed(EPServiceProvider epService) {
            var fields = new string[]{"a0", "b0", "a1", "b1"};
    
            // create window
            string stmtTextCreateOne = "create window MyWindowJSN#keepall as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtCreateOne = epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            var listenerWindow = new SupportUpdateListener();
            stmtCreateOne.Events += listenerWindow.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindowJSN where symbol = a";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowJSN select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumers
            string stmtTextSelectOne = "select irstream s0.a as a0, s0.b as b0, s1.a as a1, s1.b as b1 " +
                    " from MyWindowJSN as s0," +
                    "MyWindowJSN as s1 where s0.a = s1.a";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            SendSupportBean(epService, "E1", 1);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1, "E1", 1});
    
            SendSupportBean(epService, "E2", 2);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2, "E2", 2});
    
            SendMarketBean(epService, "E1", 1);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1, "E1", 1});
    
            SendMarketBean(epService, "E0", 0);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
        }
    
        private void RunAssertionJoinSingleInsertOneWindow(EPServiceProvider epService) {
            var fields = new string[]{"a1", "b1", "a2", "b2"};
    
            // create window
            string stmtTextCreateOne = "create window MyWindowJSIOne#keepall as select TheString as a1, IntPrimitive as b1 from " + typeof(SupportBean).FullName;
            EPStatement stmtCreateOne = epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            var listenerWindow = new SupportUpdateListener();
            stmtCreateOne.Events += listenerWindow.Update;
    
            // create window
            string stmtTextCreateTwo = "create window MyWindowJSITwo#keepall as select TheString as a2, IntPrimitive as b2 from " + typeof(SupportBean).FullName;
            EPStatement stmtCreateTwo = epService.EPAdministrator.CreateEPL(stmtTextCreateTwo);
            var listenerWindowTwo = new SupportUpdateListener();
            stmtCreateTwo.Events += listenerWindowTwo.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + "(volume=1) delete from MyWindowJSIOne where symbol = a1";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + "(volume=0) delete from MyWindowJSITwo where symbol = a2";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowJSIOne select TheString as a1, IntPrimitive as b1 from " + typeof(SupportBean).FullName + "(BoolPrimitive = true)";
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            stmtTextInsert = "insert into MyWindowJSITwo select TheString as a2, IntPrimitive as b2 from " + typeof(SupportBean).FullName + "(BoolPrimitive = false)";
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumers
            string stmtTextSelectOne = "select irstream a1, b1, a2, b2 " +
                    " from MyWindowJSIOne as s0," +
                    "MyWindowJSITwo as s1 where s0.a1 = s1.a2";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            SendSupportBean(epService, true, "S0", 1);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            SendSupportBean(epService, false, "S0", 2);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S0", 1, "S0", 2});
    
            SendSupportBean(epService, false, "S1", 3);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            SendSupportBean(epService, true, "S1", 4);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 4, "S1", 3});
    
            SendSupportBean(epService, true, "S1", 5);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 5, "S1", 3});
    
            SendSupportBean(epService, false, "S1", 6);
            Assert.AreEqual(2, listenerStmtOne.LastNewData.Length);
            listenerStmtOne.Reset();
    
            // delete and insert back in
            SendMarketBean(epService, "S0", 0);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"S0", 1, "S0", 2});
    
            SendSupportBean(epService, false, "S0", 7);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S0", 1, "S0", 7});
    
            // delete and insert back in
            SendMarketBean(epService, "S0", 1);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"S0", 1, "S0", 7});
    
            SendSupportBean(epService, true, "S0", 8);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S0", 8, "S0", 7});
        }
    
        private void RunAssertionUnidirectional(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            epService.EPAdministrator.CreateEPL("create window MyWindowU#keepall select * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowU select * from SupportBean");
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("select w.* from MyWindowU w unidirectional, SupportBean_A#lastevent s where s.id = w.TheString");
            var listenerStmtOne = new SupportUpdateListener();
            stmtOne.Events += listenerStmtOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsTrue(listenerStmtOne.IsInvoked);
        }
    
        private void SendSupportBean_A(EPServiceProvider epService, string id) {
            var bean = new SupportBean_A(id);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBean(EPServiceProvider epService, bool boolPrimitive, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.BoolPrimitive = boolPrimitive;
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketBean(EPServiceProvider epService, string symbol) {
            var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketBean(EPServiceProvider epService, string symbol, long volume) {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
