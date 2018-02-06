///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.join.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraSubqCorrelIndex : IndexBackingTableInfo
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
        private EPRuntime _epRuntime;
        private SupportUpdateListener _listenerStmtOne;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Logging.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _epRuntime = _epService.EPRuntime;
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("ABean", typeof(SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddEventType("SB2", typeof(SupportBeanTwo));
            _listenerStmtOne = new SupportUpdateListener();
            SupportQueryPlanIndexHook.Reset();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listenerStmtOne = null;
            _epRuntime = null;
        }
    
        [Test]
        public void TestNoShare() {
            // named window tests
            RunAssertion(true, false, false, false, false); // testNoShare 
            RunAssertion(true, false, false, false, true); // testNoShareSetnoindex
            RunAssertion(true, false, false, true, false); // testNoShareCreate
            RunAssertion(true, true, false, false, false); // testShare
            RunAssertion(true, true, false, true, false); // testShareCreate
            RunAssertion(true, true, false, true, true); // testShareCreateSetnoindex
            RunAssertion(true, true, true, false, false); // testDisableShare
            RunAssertion(true, true, true, true, false); // testDisableShareCreate
    
            // table tests
            RunAssertion(false, false, false, false, false); // table no-index
            RunAssertion(false, false, false, true, false); // table yes-index
        }
    
        [Test]
        public void TestMultipleIndexHints() {
            _epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            _epService.EPAdministrator.Configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
    
            RunAssertionMultipleIndexHints(true);
            RunAssertionMultipleIndexHints(false);
        }
    
        [Test]
        public void TestIndexShareIndexChoice() {
            _epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            _epService.EPAdministrator.Configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
    
            RunAssertionIndexShareIndexChoice(true);
            RunAssertionIndexShareIndexChoice(false);
        }
    
        [Test]
        public void TestNoIndexShareIndexChoice() {
            _epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            _epService.EPAdministrator.Configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
    
            RunAssertionNoIndexShareIndexChoice(true);
            RunAssertionNoIndexShareIndexChoice(false);
        }
    
        public void RunAssertionNoIndexShareIndexChoice(bool namedWindow) {
    
            var backingUniqueS1 = "unique hash={S1(string)} btree={}";
    
            var preloadedEventsOne = new object[] {new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
            IndexAssertionEventSend eventSendAssertion = () => {
                var fields = "S2,ssb1[0].S1,ssb1[0].I1".Split(',');
                _epRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 50, 21, 22));
                EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E2", 20});
                _epRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 60, 11, 12));
                EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[] {"E1", "E1", 10});
            };
            IndexAssertionEventSend noAssertion = () =>  {};
    
            // unique-s1
            AssertIndexChoice(namedWindow, false, new string[0], preloadedEventsOne, "std:unique(S1)",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "S1 = ssb2.S2", namedWindow ? null : "MyInfra", namedWindow ? BACKING_SINGLE_UNIQUE : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "S1 = ssb2.S2 and L1 = ssb2.L2", namedWindow ? null : "MyInfra", namedWindow ? BACKING_SINGLE_UNIQUE : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "I1 between 1 and 10", null, namedWindow ? BACKING_SORTED : null, noAssertion),
                            new IndexAssertion(null, "L1 = ssb2.L2", null, namedWindow ? BACKING_SINGLE_DUPS : null, eventSendAssertion),
                    });
    
            // unique-s1+I1
            if (namedWindow) {
                AssertIndexChoice(namedWindow, false, new string[0], preloadedEventsOne, "std:unique(S1, D1)",
                        new IndexAssertion[] {
                                new IndexAssertion(null, "S1 = ssb2.S2", null, BACKING_SINGLE_DUPS, eventSendAssertion),
                                new IndexAssertion(null, "S1 = ssb2.S2 and L1 = ssb2.L2", null, BACKING_MULTI_DUPS, eventSendAssertion),
                                new IndexAssertion(null, "S1 = ssb2.S2 and D1 = ssb2.D2", null, BACKING_MULTI_UNIQUE, eventSendAssertion),
                                new IndexAssertion(null, "S1 = ssb2.S2 and L1 = ssb2.L2 and D1 = ssb2.D2", null, BACKING_MULTI_UNIQUE, eventSendAssertion),
                                new IndexAssertion(null, "D1 = ssb2.D2 and S1 = ssb2.S2 and L1 = ssb2.L2", null, BACKING_MULTI_UNIQUE, eventSendAssertion),
                                new IndexAssertion(null, "L1 = ssb2.L2 and S1 = ssb2.S2 and D1 = ssb2.D2", null, BACKING_MULTI_UNIQUE, eventSendAssertion),
                        });
            }
        }
    
        private void RunAssertionIndexShareIndexChoice(bool namedWindow) {
    
            var backingUniqueS1 = "unique hash={S1(string)} btree={}";
            var backingUniqueS1L1 = "unique hash={S1(string),L1(long)} btree={}";
            var backingUniqueS1D1 = "unique hash={S1(string),D1(double)} btree={}";
            var backingNonUniqueS1 = "non-unique hash={S1(string)} btree={}";
            var backingNonUniqueD1 = "non-unique hash={D1(double)} btree={}";
            var backingBtreeI1 = "non-unique hash={} btree={I1(int)}";
            var backingBtreeD1 = "non-unique hash={} btree={D1(double)}";
            var primaryIndexTable = namedWindow ? null : "MyInfra";
    
            var preloadedEventsOne = new object[] {new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
            IndexAssertionEventSend eventSendAssertion = () => {
                var fields = "S2,ssb1[0].S1,ssb1[0].I1".Split(',');
                _epRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 50, 21, 22));
                EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E2", 20});
                _epRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 60, 11, 12));
                EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[] {"E1", "E1", 10});
            };
    
            // no index one field (essentially duplicated since declared std:unique)
            var noindexes = new string[] {};
            AssertIndexChoice(namedWindow, true, noindexes, preloadedEventsOne, "std:unique(S1)",
                new IndexAssertion[] {
                    new IndexAssertion(null, "S1 = ssb2.S2", primaryIndexTable, backingUniqueS1, eventSendAssertion),
                    new IndexAssertion(null, "S1 = ssb2.S2 and L1 = ssb2.L2", primaryIndexTable, backingUniqueS1, eventSendAssertion),
                    new IndexAssertion("@Hint('index(One)')", "S1 = ssb2.S2 and L1 = ssb2.L2", primaryIndexTable, backingUniqueS1, eventSendAssertion),
                });
    
            // single index one field (essentially duplicated since declared std:unique)
            if (namedWindow) {
                var indexOneField = new string[] {"create unique index One on MyInfra (S1)"};
                AssertIndexChoice(namedWindow, true, indexOneField, preloadedEventsOne, "std:unique(S1)",
                        new IndexAssertion[] {
                                new IndexAssertion(null, "S1 = ssb2.S2", "One", backingUniqueS1, eventSendAssertion),
                                new IndexAssertion(null, "S1 = ssb2.S2 and L1 = ssb2.L2", "One", backingUniqueS1, eventSendAssertion),
                                new IndexAssertion("@Hint('index(One)')", "S1 = ssb2.S2 and L1 = ssb2.L2", "One", backingUniqueS1, eventSendAssertion),
                        });
            }
    
            // single index two field
            var indexTwoField = new string[] {"create unique index One on MyInfra (S1, L1)"};
            AssertIndexChoice(namedWindow, true, indexTwoField, preloadedEventsOne, "std:unique(S1)",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "S1 = ssb2.S2", primaryIndexTable, backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "S1 = ssb2.S2 and L1 = ssb2.L2", "One", backingUniqueS1L1, eventSendAssertion),
                    });
    
            // two index one unique with std:unique(S1)
            var indexSetTwo = new string[] {
                    "create index One on MyInfra (S1)",
                    "create unique index Two on MyInfra (S1, D1)"};
            AssertIndexChoice(namedWindow, true, indexSetTwo, preloadedEventsOne, "std:unique(S1)",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "D1 = ssb2.D2", null, namedWindow ? backingNonUniqueD1 : null, eventSendAssertion),
                            new IndexAssertion(null, "S1 = ssb2.S2", namedWindow ? "One" : "MyInfra", namedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "S1 = ssb2.S2 and L1 = ssb2.L2", namedWindow ? "One" : "MyInfra", namedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(One)')", "S1 = ssb2.S2 and L1 = ssb2.L2", "One", backingNonUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(Two,One)')", "S1 = ssb2.S2 and L1 = ssb2.L2", "One", backingNonUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(Two,bust)')", "S1 = ssb2.S2 and L1 = ssb2.L2"), // busted
                            new IndexAssertion("@Hint('index(explicit,bust)')", "S1 = ssb2.S2 and L1 = ssb2.L2", namedWindow ? "One" : "MyInfra", namedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "S1 = ssb2.S2 and D1 = ssb2.D2 and L1 = ssb2.L2", namedWindow ? "Two" : "MyInfra", namedWindow ? backingUniqueS1D1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(explicit,bust)')", "D1 = ssb2.D2 and L1 = ssb2.L2") // busted
                    });
    
            // two index one unique with keep-all
            AssertIndexChoice(namedWindow, true, indexSetTwo, preloadedEventsOne, "win:keepall()",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "D1 = ssb2.D2", null, namedWindow ? backingNonUniqueD1 : null, eventSendAssertion),
                            new IndexAssertion(null, "S1 = ssb2.S2", namedWindow ? "One" : "MyInfra", namedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "S1 = ssb2.S2 and L1 = ssb2.L2", namedWindow ? "One" : "MyInfra", namedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(One)')", "S1 = ssb2.S2 and L1 = ssb2.L2", "One", backingNonUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(Two,One)')", "S1 = ssb2.S2 and L1 = ssb2.L2", "One", backingNonUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(Two,bust)')", "S1 = ssb2.S2 and L1 = ssb2.L2"), // busted
                            new IndexAssertion("@Hint('index(explicit,bust)')", "S1 = ssb2.S2 and L1 = ssb2.L2", namedWindow ? "One" : "MyInfra", namedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "S1 = ssb2.S2 and D1 = ssb2.D2 and L1 = ssb2.L2", namedWindow ? "Two" : "MyInfra", namedWindow ? backingUniqueS1D1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(explicit,bust)')", "D1 = ssb2.D2 and L1 = ssb2.L2") // busted
                    });
    
            // range
            IndexAssertionEventSend noAssertion = () => {};
            var indexSetThree = new string[] {
                    "create index One on MyInfra (I1 btree)",
                    "create index Two on MyInfra (D1 btree)"};
            AssertIndexChoice(namedWindow, true, indexSetThree, preloadedEventsOne, "std:unique(S1)",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "I1 between 1 and 10", "One", backingBtreeI1, noAssertion),
                            new IndexAssertion(null, "D1 between 1 and 10", "Two", backingBtreeD1, noAssertion),
                            new IndexAssertion("@Hint('index(One, bust)')", "D1 between 1 and 10"), // busted
                    });
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionMultipleIndexHints(bool namedWindow) {
            var eplCreate = namedWindow ?
                    "@Hint('enable_window_subquery_indexshare') create window MyInfra#keepall as select * from SSB1" :
                    "create table MyInfra(S1 String primary key, I1 int  primary key, D1 double primary key, L1 long primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            _epService.EPAdministrator.CreateEPL("create unique index I1 on MyInfra (S1)");
            _epService.EPAdministrator.CreateEPL("create unique index I2 on MyInfra (I1)");
    
            _epService.EPAdministrator.CreateEPL(INDEX_CALLBACK_HOOK +
                    "@Hint('index(subquery(1), I1, bust)')\n" +
                    "@Hint('index(subquery(0), I2, bust)')\n" +
                    "select " +
                    "(select * from MyInfra where S1 = ssb2.S2 and I1 = ssb2.I2) as sub1," +
                    "(select * from MyInfra where I1 = ssb2.I2 and S1 = ssb2.S2) as sub2 " +
                    "from SSB2 ssb2");
            var subqueries = SupportQueryPlanIndexHook.GetAndResetSubqueries();
            Collections.SortInPlace(subqueries, (o1, o2) => o1.Tables[0].IndexName.CompareTo(o2.Tables[0].IndexName));
            SupportQueryPlanIndexHook.AssertSubquery(subqueries[0], 1, "I1", "unique hash={S1(string)} btree={}");
            SupportQueryPlanIndexHook.AssertSubquery(subqueries[1], 0, "I2", "unique hash={I1(int)} btree={}");
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void AssertIndexChoice(bool namedWindow, bool indexShare, string[] indexes, object[] preloadedEvents, string datawindow, IndexAssertion[] assertions)
        {
            var epl = namedWindow ?
                    "create window MyInfra." + datawindow + " as select * from SSB1" :
                    "create table MyInfra(S1 string primary key, I1 int, D1 double, L1 long)";
            if (indexShare) {
                epl = "@Hint('enable_window_subquery_indexshare') " + epl;
            }
            _epService.EPAdministrator.CreateEPL(epl);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra select * from SSB1");
            foreach (var index in indexes) {
                _epService.EPAdministrator.CreateEPL(index);
            }
            foreach (var @event in preloadedEvents) {
                _epService.EPRuntime.SendEvent(@event);
            }
    
            var count = 0;
            foreach (var assertion in assertions) {
                Log.Info("======= Testing #" + count++);
                var consumeEpl = INDEX_CALLBACK_HOOK +
                        (assertion.Hint ?? "") + "select *, " +
                        "(select * from MyInfra where " + assertion.WhereClause + ") @eventbean as ssb1 from SSB2 as ssb2";
    
                EPStatement consumeStmt = null;
                try {
                    consumeStmt = _epService.EPAdministrator.CreateEPL(consumeEpl);
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
                SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(0, assertion.ExpectedIndexName, assertion.IndexBackingClass);
                consumeStmt.AddListener(_listenerStmtOne);
                assertion.EventSendAssertion.Invoke();
                consumeStmt.Dispose();
            }
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertion(bool namedWindow, bool enableIndexShareCreate, bool disableIndexShareConsumer, bool createExplicitIndex, bool setNoindex) {
            var createEpl = namedWindow ?
                    "create window MyInfra#unique(TheString) as (TheString string, IntPrimitive int)" :
                    "create table MyInfra(TheString string primary key, IntPrimitive int)";
            if (enableIndexShareCreate) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }
            _epService.EPAdministrator.CreateEPL(createEpl);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra select TheString, IntPrimitive from SupportBean");
    
            EPStatement stmtIndex = null;
            if (createExplicitIndex) {
                stmtIndex = _epService.EPAdministrator.CreateEPL("create index MyIndex on MyInfra (TheString)");
            }
    
            var consumeEpl = "select status.*, (select * from MyInfra where TheString = ABean.p00) @eventbean as details from ABean as status";
            if (disableIndexShareConsumer) {
                consumeEpl = "@Hint('disable_window_subquery_indexshare') " + consumeEpl;
            }
            if (setNoindex) {
                consumeEpl = "@Hint('set_noindex') " + consumeEpl;
            }
            var consumeStmt = _epService.EPAdministrator.CreateEPL(consumeEpl);
            consumeStmt.AddListener(_listenerStmtOne);
    
            var fields = "id,details[0].TheString,details[0].IntPrimitive".Split(',');
    
            _epRuntime.SendEvent(new SupportBean("E1", 10));
            _epRuntime.SendEvent(new SupportBean("E2", 20));
            _epRuntime.SendEvent(new SupportBean("E3", 30));
    
            _epRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{1, "E1", 10});
    
            _epRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{2, "E2", 20});
    
            // test late start
            consumeStmt.Dispose();
            consumeStmt = _epService.EPAdministrator.CreateEPL(consumeEpl);
            consumeStmt.AddListener(_listenerStmtOne);
    
            _epRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{1, "E1", 10});
    
            _epRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            EPAssertionUtil.AssertProps(_listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{2, "E2", 20});
    
            if (stmtIndex != null) {
                stmtIndex.Dispose();
            }
            consumeStmt.Dispose();
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    }
}
