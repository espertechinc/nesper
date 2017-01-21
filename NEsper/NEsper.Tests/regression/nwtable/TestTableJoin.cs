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
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableJoin : IndexBackingTableInfo
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            foreach (var clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBeanSimple)}) {
                _epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestFromClause()
        {
            _epService.EPAdministrator.CreateEPL("create table varagg as (" +
                    "key string primary key, total sum(int))");
            _epService.EPAdministrator.CreateEPL("into table varagg " +
                    "select sum(IntPrimitive) as total from SupportBean group by TheString");
            _epService.EPAdministrator.CreateEPL("select total as value from SupportBean_S0 as s0, varagg as va " +
                    "where va.key = s0.p00").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 100));
            AssertValues(_epService, _listener, "G1,G2", new int?[] {100, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            AssertValues(_epService, _listener, "G1,G2", new int?[] {100, 200});
        }
    
        [Test]
        public void TestJoinIndexChoice()
        {
            const string eplDeclare = "create table varagg as (k0 string primary key, k1 int primary key, v1 string, total sum(long))";
            const string eplPopulate = "into table varagg select sum(LongPrimitive) as total from SupportBean group by TheString, IntPrimitive";
            const string eplQuery = "select total as value from SupportBean_S0 as s0 unidirectional";
    
            var createIndexEmpty = new string[] {};
            var preloadedEventsTwo = new object[] {MakeEvent("G1", 10, 1000L), MakeEvent("G2", 20, 2000L),
                    MakeEvent("G3", 30, 3000L), MakeEvent("G4", 40, 4000L)};
    
            IndexAssertionEventSend eventSendAssertionRangeTwoExpected = () =>
            {
                _epService.EPRuntime.SendEvent(new SupportBean_S0(-1, null));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetNewDataListFlattened(), "value".Split(','),
                        new object[][] { new object[] { 2000L }, new object[] { 3000L } });
                _listener.Reset();
            };
    
            var preloadedEventsHash = new object[] {MakeEvent("G1", 10, 1000L)};
            IndexAssertionEventSend eventSendAssertionHash = () =>
            {
                _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "G1"));
                EPAssertionUtil.AssertPropsPerRow(_listener.GetNewDataListFlattened(), "value".Split(','),
                        new object[][] { new object[] { 1000L } });
                _listener.Reset();
            };
    
            // no secondary indexes
            AssertIndexChoice(eplDeclare, eplPopulate, eplQuery, createIndexEmpty, preloadedEventsHash,
                    new IndexAssertion[] {
                        // primary index found
                        new IndexAssertion("k1 = id and k0 = p00", "varagg", typeof(IndexedTableLookupPlanMulti), eventSendAssertionHash),
                        new IndexAssertion("k0 = p00 and k1 = id", "varagg", typeof(IndexedTableLookupPlanMulti), eventSendAssertionHash),
                        new IndexAssertion("k0 = p00 and k1 = id and v1 is null", "varagg", typeof(IndexedTableLookupPlanMulti), eventSendAssertionHash),
                        // no index found
                        new IndexAssertion("k1 = id", "varagg", typeof(FullTableScanUniquePerKeyLookupPlan), eventSendAssertionHash)
                    }
                );
    
            // one secondary hash index on single field
            var createIndexHashSingleK1 = new string[] {"create index idx_k1 on varagg (k1)"};
            AssertIndexChoice(eplDeclare, eplPopulate, eplQuery, createIndexHashSingleK1, preloadedEventsHash,
                    new IndexAssertion[] {
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
            var createIndexHashTwoDiscrete = new string[] {"create index idx_k1 on varagg (k1)", "create index idx_k0 on varagg (k0)"};
            AssertIndexChoice(eplDeclare, eplPopulate, eplQuery, createIndexHashTwoDiscrete, preloadedEventsHash,
                    new IndexAssertion[] {
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
            AssertIndexChoice(eplDeclare, eplPopulate, eplQuery, createIndexEmpty, preloadedEventsTwo,
                    new IndexAssertion[] {
                        // no index found
                        new IndexAssertion("k1 between 20 and 30", "varagg", typeof(FullTableScanUniquePerKeyLookupPlan), eventSendAssertionRangeTwoExpected)
                    }
            );
    
            // single range secondary index, expecting two events
            var createIndexRangeOne = new string[] {"create index b_k1 on varagg (k1 btree)"};
            AssertIndexChoice(eplDeclare, eplPopulate, eplQuery, createIndexRangeOne, preloadedEventsTwo,
                    new IndexAssertion[] {
                        new IndexAssertion("k1 between 20 and 30", "b_k1", typeof(SortedTableLookupPlan), eventSendAssertionRangeTwoExpected),
                        new IndexAssertion("(k0 = 'G3' or k0 = 'G2') and k1 between 20 and 30", "b_k1", typeof(SortedTableLookupPlan), eventSendAssertionRangeTwoExpected),
                    }
            );
    
            // single range secondary index, expecting single event
            IndexAssertionEventSend eventSendAssertionRangeOneExpected = () =>
            {
                _epService.EPRuntime.SendEvent(new SupportBean_S0(-1, null));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetNewDataListFlattened(), "value".Split(','),
                        new object[][] { new object[] { 2000L } });
                _listener.Reset();
            };
            AssertIndexChoice(eplDeclare, eplPopulate, eplQuery, createIndexRangeOne, preloadedEventsTwo,
                    new IndexAssertion[] {
                            new IndexAssertion("k0 = 'G2' and k1 between 20 and 30", "b_k1", typeof(SortedTableLookupPlan), eventSendAssertionRangeOneExpected),
                            new IndexAssertion("k1 between 20 and 30 and k0 = 'G2'", "b_k1", typeof(SortedTableLookupPlan), eventSendAssertionRangeOneExpected),
                    }
            );
    
            // combined hash+range index
            var createIndexRangeCombined = new string[] {"create index h_k0_b_k1 on varagg (k0 hash, k1 btree)"};
            AssertIndexChoice(eplDeclare, eplPopulate, eplQuery, createIndexRangeCombined, preloadedEventsTwo,
                    new IndexAssertion[] {
                            new IndexAssertion("k0 = 'G2' and k1 between 20 and 30", "h_k0_b_k1", typeof(CompositeTableLookupPlan), eventSendAssertionRangeOneExpected),
                            new IndexAssertion("k1 between 20 and 30 and k0 = 'G2'", "h_k0_b_k1", typeof(CompositeTableLookupPlan), eventSendAssertionRangeOneExpected),
                    }
            );
    
            var createIndexHashSingleK0 = new string[] {"create index idx_k0 on varagg (k0)"};
            // in-keyword single-directional use
            AssertIndexChoice(eplDeclare, eplPopulate, eplQuery, createIndexHashSingleK0, preloadedEventsTwo,
                    new IndexAssertion[] {
                            new IndexAssertion("k0 in ('G2', 'G3')", "idx_k0", typeof(InKeywordTableLookupPlanSingleIdx), eventSendAssertionRangeTwoExpected),
                    }
            );
            // in-keyword multi-directional use
            AssertIndexChoice(eplDeclare, eplPopulate, eplQuery, createIndexHashSingleK0, preloadedEventsHash,
                    new IndexAssertion[] {
                            new IndexAssertion("'G1' in (k0)", "varagg", typeof(FullTableScanUniquePerKeyLookupPlan), eventSendAssertionHash),
                    }
            );
        }
    
        [Test]
        public void TestCoercion() {
    
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanRange));
            const string eplDeclare = "create table varagg as (k0 int primary key, total sum(long))";
            const string eplPopulate = "into table varagg select sum(LongPrimitive) as total from SupportBean group by IntPrimitive";
            const string eplQuery = "select total as value from SupportBeanRange unidirectional";
    
            var createIndexEmpty = new string[] {};
            var preloadedEvents = new object[] {MakeEvent("G1", 10, 1000L), MakeEvent("G2", 20, 2000L),
                    MakeEvent("G3", 30, 3000L), MakeEvent("G4", 40, 4000L)};
            IndexAssertionEventSend eventSendAssertion = () =>
            {
                _epService.EPRuntime.SendEvent(new SupportBeanRange(20L));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetNewDataListFlattened(), "value".Split(','),
                        new object[][] { new object[] { 2000L } });
                _listener.Reset();
            };
            AssertIndexChoice(eplDeclare, eplPopulate, eplQuery, createIndexEmpty, preloadedEvents,
                    new IndexAssertion[] {
                            new IndexAssertion("k0 = keyLong", "varagg", typeof(FullTableScanUniquePerKeyLookupPlan), eventSendAssertion),
                            new IndexAssertion("k0 = keyLong", "varagg", typeof(FullTableScanUniquePerKeyLookupPlan), eventSendAssertion),
                    }
            );
        }
    
        [Test]
        public void TestUnkeyedTable() {
            // Prepare
            _epService.EPAdministrator.CreateEPL("create table MyTable (sumint sum(int))");
            _epService.EPAdministrator.CreateEPL("@Name('into') into table MyTable select sum(IntPrimitive) as sumint from SupportBean");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 101));
            _epService.EPAdministrator.GetStatement("into").Dispose();
    
            // join simple
            var stmtJoinOne = _epService.EPAdministrator.CreateEPL("select sumint from MyTable, SupportBean");
            stmtJoinOne.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(201, _listener.AssertOneGetNewAndReset().Get("sumint"));
            stmtJoinOne.Dispose();
    
            // test regular columns inserted-into
            _epService.EPAdministrator.CreateEPL("create table SecondTable (a string, b int)");
            _epService.EPRuntime.ExecuteQuery("insert into SecondTable values ('a1', 10)");
            var stmtJoinTwo = _epService.EPAdministrator.CreateEPL("select a, b from SecondTable, SupportBean");
            stmtJoinTwo.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "a,b".Split(','), new object[] {"a1", 10});
        }
    
        private void AssertIndexChoice(string eplDeclare, string eplPopulate, string eplQuery,
                                       string[] indexes, object[] preloadedEvents,
                                       IndexAssertion[] assertions) {
            AssertIndexChoice(eplDeclare, eplPopulate, eplQuery, indexes, preloadedEvents, assertions, false);
            AssertIndexChoice(eplDeclare, eplPopulate, eplQuery, indexes, preloadedEvents, assertions, true);
        }
    
        private void AssertIndexChoice(string eplDeclare, string eplPopulate, string eplQuery,
                                   string[] indexes, object[] preloadedEvents,
                                   IndexAssertion[] assertions, bool multistream) {
    
            _epService.EPAdministrator.CreateEPL(eplDeclare);
            _epService.EPAdministrator.CreateEPL(eplPopulate);
    
            foreach (var index in indexes) {
                _epService.EPAdministrator.CreateEPL(index);
            }
            foreach (var @event in preloadedEvents) {
                _epService.EPRuntime.SendEvent(@event);
            }
    
            var count = 0;
            foreach (var assertion in assertions) {
                Log.Info("======= Testing #" + count++);
                var epl = INDEX_CALLBACK_HOOK + (assertion.Hint ?? "") + eplQuery;
                epl += ", varagg as va";
                if (multistream) {
                    epl += ", SupportBeanSimple.std:lastevent()";
                }
                epl += " where " + assertion.WhereClause;
    
                EPStatement stmt;
                try {
                    stmt = _epService.EPAdministrator.CreateEPL(epl);
                    stmt.Events += _listener.Update;
                }
                catch (EPStatementException ex) {
                    if (assertion.EventSendAssertion == null) {
                        // no assertion, expected
                        Assert.IsTrue(ex.Message.Contains("index hint busted"));
                        continue;
                    }
                    throw new EPRuntimeException("Unexpected statement exception: " + ex.Message, ex);
                }
    
                // send multistream seed event
                _epService.EPRuntime.SendEvent(new SupportBeanSimple("", -1));
    
                // assert index and access
                assertion.EventSendAssertion.Invoke();
                var plan = SupportQueryPlanIndexHook.AssertJoinAndReset();
    
                TableLookupPlan tableLookupPlan;
                if (plan.ExecNodeSpecs[0] is TableLookupNode) {
                    tableLookupPlan = ((TableLookupNode) plan.ExecNodeSpecs[0]).TableLookupPlan;
                }
                else {
                    var lqp = (LookupInstructionQueryPlanNode) plan.ExecNodeSpecs[0];
                    tableLookupPlan = lqp.LookupInstructions[0].LookupPlans[0];
                }
                Assert.AreEqual(assertion.ExpectedIndexName, tableLookupPlan.IndexNum[0].Name);
                Assert.AreEqual(assertion.ExpectedStrategy, tableLookupPlan.GetType());
                stmt.Dispose();
            }
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private static void AssertValues(EPServiceProvider engine, SupportUpdateListener listener, string keys, int?[] values) {
            var keyarr = keys.Split(',');
            for (var i = 0; i < keyarr.Length; i++) {
                engine.EPRuntime.SendEvent(new SupportBean_S0(0, keyarr[i]));
                if (values[i] == null) {
                    Assert.IsFalse(listener.IsInvoked);
                }
                else {
                    var @event = listener.AssertOneGetNewAndReset();
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
}
