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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestSubselectIndex : IndexBackingTableInfo
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();        
            config.EngineDefaults.Logging.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
            SupportQueryPlanIndexHook.Reset();
        }
    
        [TearDown]
        public void TearDown() {
            _listener = null;
        }
    
        [Test]
        public void TestIndexChoicesOverdefinedWhere() {
            _epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            _epService.EPAdministrator.Configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
    
            // test no where clause with unique
            IndexAssertionEventSend assertNoWhere = () =>
            {
                String[] fields = "c0,c1".Split(',');
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 1, 2, 3));
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanOne("EX", 10, 11, 12));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"EX", "E1"});
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 1, 2, 3));
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanOne("EY", 10, 11, 12));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"EY", null});
            };
            RunAssertion(false, "s2,i2", "", BACKING_UNINDEXED, assertNoWhere);
    
            // test no where clause with unique on multiple props, exact specification of where-clause
            IndexAssertionEventSend assertSendEvents = () => 
            {
                String[] fields = "c0,c1".Split(',');
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 1, 3, 10));
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 1, 2, 0));
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E3", 1, 3, 9));
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanOne("EX", 1, 3, 9));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"EX", "E3"});
            };
            RunAssertion(false, "d2,i2", "where ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1", BACKING_MULTI_UNIQUE, assertSendEvents);
            RunAssertion(false, "d2,i2", "where ssb2.d2 = ssb1.d1 and ssb2.i2 = ssb1.i1", BACKING_MULTI_UNIQUE, assertSendEvents);
            RunAssertion(false, "d2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.d2 = ssb1.d1 and ssb2.i2 = ssb1.i1", BACKING_MULTI_UNIQUE, assertSendEvents);
            RunAssertion(false, "d2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1", BACKING_MULTI_DUPS, assertSendEvents);
            RunAssertion(false, "d2,i2", "where ssb2.d2 = ssb1.d1", BACKING_SINGLE_DUPS, assertSendEvents);
            RunAssertion(false, "d2,i2", "where ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1 and ssb2.l2 between 1 and 1000", BACKING_MULTI_UNIQUE, assertSendEvents);
            RunAssertion(false, "d2,i2", "where ssb2.d2 = ssb1.d1 and ssb2.l2 between 1 and 1000", BACKING_COMPOSITE, assertSendEvents);
            RunAssertion(false, "i2,d2,l2", "where ssb2.l2 = ssb1.l1 and ssb2.d2 = ssb1.d1", BACKING_MULTI_DUPS, assertSendEvents);
            RunAssertion(false, "i2,d2,l2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1", BACKING_MULTI_UNIQUE, assertSendEvents);
            RunAssertion(false, "d2,l2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1", BACKING_MULTI_UNIQUE, assertSendEvents);
            RunAssertion(false, "d2,l2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1 and ssb2.s2 between 'E3' and 'E4'", BACKING_MULTI_UNIQUE, assertSendEvents);
            RunAssertion(false, "l2", "where ssb2.l2 = ssb1.l1", BACKING_SINGLE_UNIQUE, assertSendEvents);
            RunAssertion(true, "l2", "where ssb2.l2 = ssb1.l1", BACKING_SINGLE_DUPS, assertSendEvents);
            RunAssertion(false, "l2", "where ssb2.l2 = ssb1.l1 and ssb1.i1 between 1 and 20", BACKING_SINGLE_UNIQUE, assertSendEvents);
        }
    
        private void RunAssertion(bool disableImplicitUniqueIdx, String uniqueFields, String whereClause, String backingTable, IndexAssertionEventSend assertion)
        {
            String eplUnique = INDEX_CALLBACK_HOOK + "select s1 as c0, " +
                    "(select s2 from SSB2#unique(" + uniqueFields + ") as ssb2 " + whereClause + ") as c1 " +
                    "from SSB1 as ssb1";
            if (disableImplicitUniqueIdx) {
                eplUnique = "@Hint('DISABLE_UNIQUE_IMPLICIT_IDX')" + eplUnique;
            }
            EPStatement stmtUnique = _epService.EPAdministrator.CreateEPL(eplUnique);
            stmtUnique.Events += _listener.Update;

            SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(0, null, backingTable);
    
            assertion.Invoke();
    
            stmtUnique.Dispose();
        }
    
        [Test]
        public void TestUniqueIndexCorrelated() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            String[] fields = "c0,c1".Split(',');
    
            // test std:unique
            String eplUnique = INDEX_CALLBACK_HOOK + "select id as c0, " +
                    "(select IntPrimitive from SupportBean#unique(TheString) where TheString = s0.p00) as c1 " +
                    "from S0 as s0";
            EPStatement stmtUnique = _epService.EPAdministrator.CreateEPL(eplUnique);
            stmtUnique.Events += _listener.Update;

            SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(0, null, BACKING_SINGLE_UNIQUE);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {10, 4});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(11, "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{11, 3});
    
            stmtUnique.Dispose();
    
            // test std:firstunique
            String eplFirstUnique = INDEX_CALLBACK_HOOK + "select id as c0, " +
                    "(select IntPrimitive from SupportBean#firstunique(TheString) where TheString = s0.p00) as c1 " +
                    "from S0 as s0";
            EPStatement stmtFirstUnique = _epService.EPAdministrator.CreateEPL(eplFirstUnique);
            stmtFirstUnique.Events += _listener.Update;

            SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(0, null, BACKING_SINGLE_UNIQUE);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {10, 2});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(11, "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{11, 1});
    
            stmtFirstUnique.Dispose();
    
            // test intersection std:firstunique
            String eplIntersection = INDEX_CALLBACK_HOOK + "select id as c0, " +
                    "(select IntPrimitive from SupportBean#time(1)#unique(TheString) where TheString = s0.p00) as c1 " +
                    "from S0 as s0";
            EPStatement stmtIntersection = _epService.EPAdministrator.CreateEPL(eplIntersection);
            stmtIntersection.Events += _listener.Update;

            SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(0, null, BACKING_SINGLE_UNIQUE);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {10, 4});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(11, "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{11, 3});
    
            stmtIntersection.Dispose();
    
            // test grouped unique
            String eplGrouped = INDEX_CALLBACK_HOOK + "select id as c0, " +
                    "(select LongPrimitive from SupportBean#groupwin(TheString)#unique(IntPrimitive) where TheString = s0.p00 and IntPrimitive = s0.id) as c1 " +
                    "from S0 as s0";
            EPStatement stmtGrouped = _epService.EPAdministrator.CreateEPL(eplGrouped);
            stmtGrouped.Events += _listener.Update;

            SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(0, null, BACKING_MULTI_UNIQUE);
    
            _epService.EPRuntime.SendEvent(MakeBean("E1", 1, 100));
            _epService.EPRuntime.SendEvent(MakeBean("E1", 2, 101));
            _epService.EPRuntime.SendEvent(MakeBean("E1", 1, 102));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {1, 102L});
    
            stmtGrouped.Dispose();
        }
    
        private SupportBean MakeBean(String theString, int intPrimitive, long longPrimitive)
        {
            SupportBean bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    }
}
