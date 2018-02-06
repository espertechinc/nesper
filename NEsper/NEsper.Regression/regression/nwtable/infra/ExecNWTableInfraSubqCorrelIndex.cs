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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraSubqCorrelIndex : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("ABean", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("SB2", typeof(SupportBeanTwo));
            epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            epService.EPAdministrator.Configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
    
            // named window tests
            RunAssertion(epService, true, false, false, false, false); // testNoShare
            RunAssertion(epService, true, false, false, false, true); // testNoShareSetnoindex
            RunAssertion(epService, true, false, false, true, false); // testNoShareCreate
            RunAssertion(epService, true, true, false, false, false); // testShare
            RunAssertion(epService, true, true, false, true, false); // testShareCreate
            RunAssertion(epService, true, true, false, true, true); // testShareCreateSetnoindex
            RunAssertion(epService, true, true, true, false, false); // testDisableShare
            RunAssertion(epService, true, true, true, true, false); // testDisableShareCreate
    
            // table tests
            RunAssertion(epService, false, false, false, false, false); // table no-index
            RunAssertion(epService, false, false, false, true, false); // table yes-index
    
            RunAssertionMultipleIndexHints(epService, true);
            RunAssertionMultipleIndexHints(epService, false);
    
            RunAssertionIndexShareIndexChoice(epService, true);
            RunAssertionIndexShareIndexChoice(epService, false);
    
            RunAssertionNoIndexShareIndexChoice(epService, true);
            RunAssertionNoIndexShareIndexChoice(epService, false);
        }
    
        private void RunAssertionNoIndexShareIndexChoice(EPServiceProvider epService, bool namedWindow) {
    
            string backingUniqueS1 = "unique hash={s1(string)} btree={} advanced={}";
    
            var preloadedEventsOne = new object[]{new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
            var listenerStmtOne = new SupportUpdateListener();
            var eventSendAssertion = new IndexAssertionEventSend(() => {
                string[] fields = "s2,ssb1[0].s1,ssb1[0].i1".Split(',');
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 50, 21, 22));
                EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E2", 20});
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 60, 11, 12));
                EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 10});
            });
            var noAssertion = new IndexAssertionEventSend(() => { });
    
            // unique-s1
            AssertIndexChoice(epService, listenerStmtOne, namedWindow, false, new string[0], preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "s1 = ssb2.s2", namedWindow ? null : "MyInfra", namedWindow ? BACKING_SINGLE_UNIQUE : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2", namedWindow ? null : "MyInfra", namedWindow ? BACKING_SINGLE_UNIQUE : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "i1 between 1 and 10", null, namedWindow ? BACKING_SORTED : null, noAssertion),
                            new IndexAssertion(null, "l1 = ssb2.l2", null, namedWindow ? BACKING_SINGLE_DUPS : null, eventSendAssertion),
                    });
    
            // unique-s1+i1
            if (namedWindow) {
                AssertIndexChoice(epService, listenerStmtOne, namedWindow, false, new string[0], preloadedEventsOne, "std:unique(s1, d1)",
                        new IndexAssertion[]{
                                new IndexAssertion(null, "s1 = ssb2.s2", null, BACKING_SINGLE_DUPS, eventSendAssertion),
                                new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2", null, BACKING_MULTI_DUPS, eventSendAssertion),
                                new IndexAssertion(null, "s1 = ssb2.s2 and d1 = ssb2.d2", null, BACKING_MULTI_UNIQUE, eventSendAssertion),
                                new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2 and d1 = ssb2.d2", null, BACKING_MULTI_UNIQUE, eventSendAssertion),
                                new IndexAssertion(null, "d1 = ssb2.d2 and s1 = ssb2.s2 and l1 = ssb2.l2", null, BACKING_MULTI_UNIQUE, eventSendAssertion),
                                new IndexAssertion(null, "l1 = ssb2.l2 and s1 = ssb2.s2 and d1 = ssb2.d2", null, BACKING_MULTI_UNIQUE, eventSendAssertion),
                        });
            }
        }
    
        private void RunAssertionIndexShareIndexChoice(EPServiceProvider epService, bool namedWindow) {
    
            string backingUniqueS1 = "unique hash={s1(string)} btree={} advanced={}";
            string backingUniqueS1L1 = "unique hash={s1(string),l1(long)} btree={} advanced={}";
            string backingUniqueS1D1 = "unique hash={s1(string),d1(double)} btree={} advanced={}";
            string backingNonUniqueS1 = "non-unique hash={s1(string)} btree={} advanced={}";
            string backingNonUniqueD1 = "non-unique hash={d1(double)} btree={} advanced={}";
            string backingBtreeI1 = "non-unique hash={} btree={i1(int)} advanced={}";
            string backingBtreeD1 = "non-unique hash={} btree={d1(double)} advanced={}";
            string primaryIndexTable = namedWindow ? null : "MyInfra";
    
            var preloadedEventsOne = new object[]{new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
            var listener = new SupportUpdateListener();
            var eventSendAssertion = new IndexAssertionEventSend(() => {
                string[] fields = "s2,ssb1[0].s1,ssb1[0].i1".Split(',');
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 50, 21, 22));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E2", 20});
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 60, 11, 12));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 10});
            });
    
            // no index one field (essentially duplicated since declared std:unique)
            var noindexes = new string[]{};
            AssertIndexChoice(epService, listener, namedWindow, true, noindexes, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "s1 = ssb2.s2", primaryIndexTable, backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2", primaryIndexTable, backingUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", primaryIndexTable, backingUniqueS1, eventSendAssertion),
                    });
    
            // single index one field (essentially duplicated since declared std:unique)
            if (namedWindow) {
                var indexOneField = new string[]{"create unique index One on MyInfra (s1)"};
                AssertIndexChoice(epService, listener, namedWindow, true, indexOneField, preloadedEventsOne, "std:unique(s1)",
                        new IndexAssertion[]{
                                new IndexAssertion(null, "s1 = ssb2.s2", "One", backingUniqueS1, eventSendAssertion),
                                new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingUniqueS1, eventSendAssertion),
                                new IndexAssertion("@Hint('index(One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingUniqueS1, eventSendAssertion),
                        });
            }
    
            // single index two field
            var indexTwoField = new string[]{"create unique index One on MyInfra (s1, l1)"};
            AssertIndexChoice(epService, listener, namedWindow, true, indexTwoField, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "s1 = ssb2.s2", primaryIndexTable, backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingUniqueS1L1, eventSendAssertion),
                    });
    
            // two index one unique with std:unique(s1)
            var indexSetTwo = new string[]{
                    "create index One on MyInfra (s1)",
                    "create unique index Two on MyInfra (s1, d1)"};
            AssertIndexChoice(epService, listener, namedWindow, true, indexSetTwo, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "d1 = ssb2.d2", null, namedWindow ? backingNonUniqueD1 : null, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2", namedWindow ? "One" : "MyInfra", namedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2", namedWindow ? "One" : "MyInfra", namedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingNonUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(Two,One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingNonUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(Two,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2"), // busted
                            new IndexAssertion("@Hint('index(explicit,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2", namedWindow ? "One" : "MyInfra", namedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2 and d1 = ssb2.d2 and l1 = ssb2.l2", namedWindow ? "Two" : "MyInfra", namedWindow ? backingUniqueS1D1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(explicit,bust)')", "d1 = ssb2.d2 and l1 = ssb2.l2") // busted
                    });
    
            // two index one unique with keep-all
            AssertIndexChoice(epService, listener, namedWindow, true, indexSetTwo, preloadedEventsOne, "win:keepall()",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "d1 = ssb2.d2", null, namedWindow ? backingNonUniqueD1 : null, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2", namedWindow ? "One" : "MyInfra", namedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2", namedWindow ? "One" : "MyInfra", namedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingNonUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(Two,One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingNonUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(Two,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2"), // busted
                            new IndexAssertion("@Hint('index(explicit,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2", namedWindow ? "One" : "MyInfra", namedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2 and d1 = ssb2.d2 and l1 = ssb2.l2", namedWindow ? "Two" : "MyInfra", namedWindow ? backingUniqueS1D1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(explicit,bust)')", "d1 = ssb2.d2 and l1 = ssb2.l2") // busted
                    });
    
            // range
            var noAssertion = new IndexAssertionEventSend(() => {
            });

            var indexSetThree = new string[]{
                    "create index One on MyInfra (i1 btree)",
                    "create index Two on MyInfra (d1 btree)"};
            AssertIndexChoice(epService, listener, namedWindow, true, indexSetThree, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[]{
                            new IndexAssertion(null, "i1 between 1 and 10", "One", backingBtreeI1, noAssertion),
                            new IndexAssertion(null, "d1 between 1 and 10", "Two", backingBtreeD1, noAssertion),
                            new IndexAssertion("@Hint('index(One, bust)')", "d1 between 1 and 10"), // busted
                    });
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionMultipleIndexHints(EPServiceProvider epService, bool namedWindow) {
            string eplCreate = namedWindow ?
                    "@Hint('enable_window_subquery_indexshare') create window MyInfraMIH#keepall as select * from SSB1" :
                    "create table MyInfraMIH(s1 string primary key, i1 int  primary key, d1 double primary key, l1 long primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("create unique index I1 on MyInfraMIH (s1)");
            epService.EPAdministrator.CreateEPL("create unique index I2 on MyInfraMIH (i1)");
    
            epService.EPAdministrator.CreateEPL(INDEX_CALLBACK_HOOK +
                    "@Hint('index(subquery(1), I1, bust)')\n" +
                    "@Hint('index(subquery(0), I2, bust)')\n" +
                    "select " +
                    "(select * from MyInfraMIH where s1 = ssb2.s2 and i1 = ssb2.i2) as sub1," +
                    "(select * from MyInfraMIH where i1 = ssb2.i2 and s1 = ssb2.s2) as sub2 " +
                    "from SSB2 ssb2");
            List<QueryPlanIndexDescSubquery> subqueries = SupportQueryPlanIndexHook.GetAndResetSubqueries();
            subqueries.Sort((o1, o2) => o1.Tables[0].IndexName.CompareTo(o2.Tables[0].IndexName));
            SupportQueryPlanIndexHook.AssertSubquery(subqueries[0], 1, "I1", "unique hash={s1(string)} btree={} advanced={}");
            SupportQueryPlanIndexHook.AssertSubquery(subqueries[1], 0, "I2", "unique hash={i1(int)} btree={} advanced={}");
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraMIH", false);
        }
    
        private void AssertIndexChoice(EPServiceProvider epService, SupportUpdateListener listenerStmtOne, bool namedWindow, bool indexShare, string[] indexes, object[] preloadedEvents, string datawindow,
                                       IndexAssertion[] assertions) {
            string epl = namedWindow ?
                    "create window MyInfra." + datawindow + " as select * from SSB1" :
                    "create table MyInfra(s1 string primary key, i1 int, d1 double, l1 long)";
            if (indexShare) {
                epl = "@Hint('enable_window_subquery_indexshare') " + epl;
            }
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPAdministrator.CreateEPL("insert into MyInfra select * from SSB1");
            foreach (string index in indexes) {
                epService.EPAdministrator.CreateEPL(index);
            }
            foreach (Object @event in preloadedEvents) {
                epService.EPRuntime.SendEvent(@event);
            }
    
            int count = 0;
            foreach (IndexAssertion assertion in assertions) {
                Log.Info("======= Testing #" + count++);
                string consumeEpl = INDEX_CALLBACK_HOOK +
                        (assertion.Hint == null ? "" : assertion.Hint) + "select *, " +
                        "(select * from MyInfra where " + assertion.WhereClause + ") @eventbean as ssb1 from SSB2 as ssb2";
    
                EPStatement consumeStmt;
                try {
                    consumeStmt = epService.EPAdministrator.CreateEPL(consumeEpl);
                } catch (EPStatementException ex) {
                    if (assertion.EventSendAssertion == null) {
                        // no assertion, expected
                        Assert.IsTrue(ex.Message.Contains("index hint busted"));
                        continue;
                    }
                    throw new EPRuntimeException("Unexpected statement exception: " + ex.Message, ex);
                }
    
                // assert index and access
                SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(0, assertion.ExpectedIndexName, assertion.IndexBackingClass);
                consumeStmt.Events += listenerStmtOne.Update;
                assertion.EventSendAssertion.Invoke();
                consumeStmt.Dispose();
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertion(EPServiceProvider epService, bool namedWindow, bool enableIndexShareCreate, bool disableIndexShareConsumer, bool createExplicitIndex, bool setNoindex) {
            string createEpl = namedWindow ?
                    "create window MyInfraNWT#unique(TheString) as (TheString string, IntPrimitive int)" :
                    "create table MyInfraNWT(TheString string primary key, IntPrimitive int)";
            if (enableIndexShareCreate) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }
            epService.EPAdministrator.CreateEPL(createEpl);
            epService.EPAdministrator.CreateEPL("insert into MyInfraNWT select TheString, IntPrimitive from SupportBean");
    
            EPStatement stmtIndex = null;
            if (createExplicitIndex) {
                stmtIndex = epService.EPAdministrator.CreateEPL("create index MyIndex on MyInfraNWT (TheString)");
            }
    
            string consumeEpl = "select status.*, (select * from MyInfraNWT where TheString = ABean.p00) @eventbean as details from ABean as status";
            if (disableIndexShareConsumer) {
                consumeEpl = "@Hint('disable_window_subquery_indexshare') " + consumeEpl;
            }
            if (setNoindex) {
                consumeEpl = "@Hint('set_noindex') " + consumeEpl;
            }
            EPStatement consumeStmt = epService.EPAdministrator.CreateEPL(consumeEpl);
            var listener = new SupportUpdateListener();
            consumeStmt.Events += listener.Update;
    
            string[] fields = "id,details[0].TheString,details[0].IntPrimitive".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "E1", 10});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "E2", 20});
    
            // test late start
            consumeStmt.Dispose();
            consumeStmt = epService.EPAdministrator.CreateEPL(consumeEpl);
            consumeStmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "E1", 10});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "E2", 20});
    
            if (stmtIndex != null) {
                stmtIndex.Dispose();
            }
            consumeStmt.Dispose();
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraNWT", false);
        }
    }
} // end of namespace
