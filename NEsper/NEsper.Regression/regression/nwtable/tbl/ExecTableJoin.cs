///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableJoin : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBeanSimple)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionFromClause(epService);
            RunAssertionJoinIndexChoice(epService);
            RunAssertionCoercion(epService);
            RunAssertionUnkeyedTable(epService);
        }
    
        private void RunAssertionFromClause(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create table varaggFC as (" +
                    "key string primary key, total sum(int))");
            epService.EPAdministrator.CreateEPL("into table varaggFC " +
                    "select sum(IntPrimitive) as total from SupportBean group by TheString");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select total as value from SupportBean_S0 as s0, varaggFC as va " +
                    "where va.key = s0.p00").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 100));
            AssertValues(epService, listener, "G1,G2", new int?[]{100, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            AssertValues(epService, listener, "G1,G2", new int?[]{100, 200});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionJoinIndexChoice(EPServiceProvider epService) {
    
            string eplDeclare = "create table varagg as (k0 string primary key, k1 int primary key, v1 string, total sum(long))";
            string eplPopulate = "into table varagg select sum(LongPrimitive) as total from SupportBean group by TheString, IntPrimitive";
            string eplQuery = "select total as value from SupportBean_S0 as s0 unidirectional";
    
            var createIndexEmpty = new string[]{};
            var preloadedEventsTwo = new object[]{MakeEvent("G1", 10, 1000L), MakeEvent("G2", 20, 2000L),
                    MakeEvent("G3", 30, 3000L), MakeEvent("G4", 40, 4000L)};
            var listener = new SupportUpdateListener();
    
            var eventSendAssertionRangeTwoExpected = new IndexAssertionEventSend(() => {
                epService.EPRuntime.SendEvent(new SupportBean_S0(-1, null));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetNewDataListFlattened(), "value".Split(','),
                        new object[][]{new object[] {2000L}, new object[] {3000L}});
                listener.Reset();
            });
    
            var preloadedEventsHash = new object[]{MakeEvent("G1", 10, 1000L)};
            var eventSendAssertionHash = new IndexAssertionEventSend(() => {
                epService.EPRuntime.SendEvent(new SupportBean_S0(10, "G1"));
                EPAssertionUtil.AssertPropsPerRow(listener.GetNewDataListFlattened(), "value".Split(','),
                        new object[][]{new object[] {1000L}});
                listener.Reset();
            });
    
            // no secondary indexes
            AssertIndexChoice(epService, listener, eplDeclare, eplPopulate, eplQuery, createIndexEmpty, preloadedEventsHash,
                    new IndexAssertion[]{
                            // primary index found
                            new IndexAssertion("k1 = id and k0 = p00", "varagg", typeof(IndexedTableLookupPlanMulti), eventSendAssertionHash),
                            new IndexAssertion("k0 = p00 and k1 = id", "varagg", typeof(IndexedTableLookupPlanMulti), eventSendAssertionHash),
                            new IndexAssertion("k0 = p00 and k1 = id and v1 is null", "varagg", typeof(IndexedTableLookupPlanMulti), eventSendAssertionHash),
                            // no index found
                            new IndexAssertion("k1 = id", "varagg", typeof(FullTableScanUniquePerKeyLookupPlan), eventSendAssertionHash)
                    }
            );
    
            // one secondary hash index on single field
            var createIndexHashSingleK1 = new string[]{"create index idx_k1 on varagg (k1)"};
            AssertIndexChoice(epService, listener, eplDeclare, eplPopulate, eplQuery, createIndexHashSingleK1, preloadedEventsHash,
                    new IndexAssertion[]{
                            // primary index found
                            new IndexAssertion("k1 = id and k0 = p00", "varagg", typeof(IndexedTableLookupPlanMulti), eventSendAssertionHash),
                            // secondary index found
                            new IndexAssertion("k1 = id", "idx_k1", typeof(IndexedTableLookupPlanSingle), eventSendAssertionHash),
                            new IndexAssertion("id = k1", "idx_k1", typeof(IndexedTableLookupPlanSingle), eventSendAssertionHash),
                            // no index found
                            new IndexAssertion("k0 = p00", "varagg", typeof(FullTableScanUniquePerKeyLookupPlan), eventSendAssertionHash)
                    }
            );
    
            // two secondary hash indexes on one field each
            var createIndexHashTwoDiscrete = new string[]{"create index idx_k1 on varagg (k1)", "create index idx_k0 on varagg (k0)"};
            AssertIndexChoice(epService, listener, eplDeclare, eplPopulate, eplQuery, createIndexHashTwoDiscrete, preloadedEventsHash,
                    new IndexAssertion[]{
                            // primary index found
                            new IndexAssertion("k1 = id and k0 = p00", "varagg", typeof(IndexedTableLookupPlanMulti), eventSendAssertionHash),
                            // secondary index found
                            new IndexAssertion("k0 = p00", "idx_k0", typeof(IndexedTableLookupPlanSingle), eventSendAssertionHash),
                            new IndexAssertion("k1 = id", "idx_k1", typeof(IndexedTableLookupPlanSingle), eventSendAssertionHash),
                            new IndexAssertion("v1 is null and k1 = id", "idx_k1", typeof(IndexedTableLookupPlanSingle), eventSendAssertionHash),
                            // no index found
                            new IndexAssertion("1=1", "varagg", typeof(FullTableScanUniquePerKeyLookupPlan), eventSendAssertionHash)
                    }
            );
    
            // one range secondary index
            // no secondary indexes
            AssertIndexChoice(epService, listener, eplDeclare, eplPopulate, eplQuery, createIndexEmpty, preloadedEventsTwo,
                    new IndexAssertion[]{
                            // no index found
                            new IndexAssertion("k1 between 20 and 30", "varagg", typeof(FullTableScanUniquePerKeyLookupPlan), eventSendAssertionRangeTwoExpected)
                    }
            );
    
            // single range secondary index, expecting two events
            var createIndexRangeOne = new string[]{"create index b_k1 on varagg (k1 btree)"};
            AssertIndexChoice(epService, listener, eplDeclare, eplPopulate, eplQuery, createIndexRangeOne, preloadedEventsTwo,
                    new IndexAssertion[]{
                            new IndexAssertion("k1 between 20 and 30", "b_k1", typeof(SortedTableLookupPlan), eventSendAssertionRangeTwoExpected),
                            new IndexAssertion("(k0 = 'G3' or k0 = 'G2') and k1 between 20 and 30", "b_k1", typeof(SortedTableLookupPlan), eventSendAssertionRangeTwoExpected),
                    }
            );
    
            // single range secondary index, expecting single event
            var eventSendAssertionRangeOneExpected = new IndexAssertionEventSend(() => {
                epService.EPRuntime.SendEvent(new SupportBean_S0(-1, null));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetNewDataListFlattened(), "value".Split(','),
                        new object[][]{new object[] {2000L}});
                listener.Reset();
            });
            AssertIndexChoice(epService, listener, eplDeclare, eplPopulate, eplQuery, createIndexRangeOne, preloadedEventsTwo,
                    new IndexAssertion[]{
                            new IndexAssertion("k0 = 'G2' and k1 between 20 and 30", "b_k1", typeof(SortedTableLookupPlan), eventSendAssertionRangeOneExpected),
                            new IndexAssertion("k1 between 20 and 30 and k0 = 'G2'", "b_k1", typeof(SortedTableLookupPlan), eventSendAssertionRangeOneExpected),
                    }
            );
    
            // combined hash+range index
            var createIndexRangeCombined = new string[]{"create index h_k0_b_k1 on varagg (k0 hash, k1 btree)"};
            AssertIndexChoice(epService, listener, eplDeclare, eplPopulate, eplQuery, createIndexRangeCombined, preloadedEventsTwo,
                    new IndexAssertion[]{
                            new IndexAssertion("k0 = 'G2' and k1 between 20 and 30", "h_k0_b_k1", typeof(CompositeTableLookupPlan), eventSendAssertionRangeOneExpected),
                            new IndexAssertion("k1 between 20 and 30 and k0 = 'G2'", "h_k0_b_k1", typeof(CompositeTableLookupPlan), eventSendAssertionRangeOneExpected),
                    }
            );
    
            var createIndexHashSingleK0 = new string[]{"create index idx_k0 on varagg (k0)"};
            // in-keyword single-directional use
            AssertIndexChoice(epService, listener, eplDeclare, eplPopulate, eplQuery, createIndexHashSingleK0, preloadedEventsTwo,
                    new IndexAssertion[]{
                            new IndexAssertion("k0 in ('G2', 'G3')", "idx_k0", typeof(InKeywordTableLookupPlanSingleIdx), eventSendAssertionRangeTwoExpected),
                    }
            );
            // in-keyword multi-directional use
            AssertIndexChoice(epService, listener, eplDeclare, eplPopulate, eplQuery, createIndexHashSingleK0, preloadedEventsHash,
                    new IndexAssertion[]{
                            new IndexAssertion("'G1' in (k0)", "varagg", typeof(FullTableScanUniquePerKeyLookupPlan), eventSendAssertionHash),
                    }
            );
    
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }
    
        private void RunAssertionCoercion(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanRange));
            string eplDeclare = "create table varagg as (k0 int primary key, total sum(long))";
            string eplPopulate = "into table varagg select sum(LongPrimitive) as total from SupportBean group by IntPrimitive";
            string eplQuery = "select total as value from SupportBeanRange unidirectional";
    
            var createIndexEmpty = new string[]{};
            var preloadedEvents = new object[]{MakeEvent("G1", 10, 1000L), MakeEvent("G2", 20, 2000L),
                    MakeEvent("G3", 30, 3000L), MakeEvent("G4", 40, 4000L)};
            var listener = new SupportUpdateListener();
    
            var eventSendAssertion = new IndexAssertionEventSend(() => {
                epService.EPRuntime.SendEvent(new SupportBeanRange(20L));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetNewDataListFlattened(), "value".Split(','),
                        new object[][]{new object[] {2000L}});
                listener.Reset();
            });

            AssertIndexChoice(epService, listener, eplDeclare, eplPopulate, eplQuery, createIndexEmpty, preloadedEvents,
                    new IndexAssertion[]{
                            new IndexAssertion("k0 = keyLong", "varagg", typeof(FullTableScanUniquePerKeyLookupPlan), eventSendAssertion),
                            new IndexAssertion("k0 = keyLong", "varagg", typeof(FullTableScanUniquePerKeyLookupPlan), eventSendAssertion),
                    }
            );
        }
    
        private void RunAssertionUnkeyedTable(EPServiceProvider epService) {
            // Prepare
            epService.EPAdministrator.CreateEPL("create table MyTable (sumint sum(int))");
            epService.EPAdministrator.CreateEPL("@Name('into') into table MyTable select sum(IntPrimitive) as sumint from SupportBean");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 101));
            epService.EPAdministrator.GetStatement("into").Dispose();
    
            // join simple
            EPStatement stmtJoinOne = epService.EPAdministrator.CreateEPL("select sumint from MyTable, SupportBean");
            var listener = new SupportUpdateListener();
            stmtJoinOne.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(201, listener.AssertOneGetNewAndReset().Get("sumint"));
            stmtJoinOne.Dispose();
    
            // test regular columns inserted-into
            epService.EPAdministrator.CreateEPL("create table SecondTable (a string, b int)");
            epService.EPRuntime.ExecuteQuery("insert into SecondTable values ('a1', 10)");
            EPStatement stmtJoinTwo = epService.EPAdministrator.CreateEPL("select a, b from SecondTable, SupportBean");
            stmtJoinTwo.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b".Split(','), new object[]{"a1", 10});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertIndexChoice(EPServiceProvider epService, SupportUpdateListener listener, string eplDeclare, string eplPopulate, string eplQuery,
                                       string[] indexes, object[] preloadedEvents,
                                       IndexAssertion[] assertions) {
            AssertIndexChoice(epService, listener, eplDeclare, eplPopulate, eplQuery, indexes, preloadedEvents, assertions, false);
            AssertIndexChoice(epService, listener, eplDeclare, eplPopulate, eplQuery, indexes, preloadedEvents, assertions, true);
        }
    
        private void AssertIndexChoice(EPServiceProvider epService, SupportUpdateListener listener, string eplDeclare, string eplPopulate, string eplQuery,
                                       string[] indexes, object[] preloadedEvents,
                                       IndexAssertion[] assertions, bool multistream) {
    
            epService.EPAdministrator.CreateEPL(eplDeclare);
            epService.EPAdministrator.CreateEPL(eplPopulate);
    
            foreach (string index in indexes) {
                epService.EPAdministrator.CreateEPL(index);
            }
            foreach (Object @event in preloadedEvents) {
                epService.EPRuntime.SendEvent(@event);
            }
    
            int count = 0;
            foreach (IndexAssertion assertion in assertions) {
                Log.Info("======= Testing #" + count++);
                string epl = INDEX_CALLBACK_HOOK + (assertion.Hint == null ? "" : assertion.Hint) + eplQuery;
                epl += ", varagg as va";
                if (multistream) {
                    epl += ", SupportBeanSimple#lastevent";
                }
                epl += " where " + assertion.WhereClause;
    
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
    
                // send multistream seed event
                epService.EPRuntime.SendEvent(new SupportBeanSimple("", -1));
    
                // assert index and access
                assertion.EventSendAssertion.Invoke();
                QueryPlan plan = SupportQueryPlanIndexHook.AssertJoinAndReset();
    
                TableLookupPlan tableLookupPlan;
                if (plan.ExecNodeSpecs[0] is TableLookupNode) {
                    tableLookupPlan = ((TableLookupNode) plan.ExecNodeSpecs[0]).TableLookupPlan;
                } else {
                    LookupInstructionQueryPlanNode lqp = (LookupInstructionQueryPlanNode) plan.ExecNodeSpecs[0];
                    tableLookupPlan = lqp.LookupInstructions[0].LookupPlans[0];
                }
                Assert.AreEqual(assertion.ExpectedIndexName, tableLookupPlan.IndexNum[0].Name);
                Assert.AreEqual(assertion.ExpectedStrategy, tableLookupPlan.GetType());
                stmt.Dispose();
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private static void AssertValues(EPServiceProvider engine, SupportUpdateListener listener, string keys, int?[] values) {
            string[] keyarr = keys.Split(',');
            for (int i = 0; i < keyarr.Length; i++) {
                engine.EPRuntime.SendEvent(new SupportBean_S0(0, keyarr[i]));
                if (values[i] == null) {
                    Assert.IsFalse(listener.IsInvoked);
                } else {
                    EventBean @event = listener.AssertOneGetNewAndReset();
                    Assert.AreEqual(values[i], @event.Get("value"), "Failed for key '" + keyarr[i] + "'");
                }
            }
        }
    
        private static SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    }
} // end of namespace
