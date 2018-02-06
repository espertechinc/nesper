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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectIndex : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionIndexChoicesOverdefinedWhere(epService);
            RunAssertionUniqueIndexCorrelated(epService);
        }
    
        private void RunAssertionIndexChoicesOverdefinedWhere(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            epService.EPAdministrator.Configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
            var listener = new SupportUpdateListener();
    
            // test no where clause with unique
            var assertNoWhere = new IndexAssertionEventSend(() => {
                string[] fields = "c0,c1".Split(',');
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 1, 2, 3));
                epService.EPRuntime.SendEvent(new SupportSimpleBeanOne("EX", 10, 11, 12));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"EX", "E1"});
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 1, 2, 3));
                epService.EPRuntime.SendEvent(new SupportSimpleBeanOne("EY", 10, 11, 12));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"EY", null});
            });
            TryAssertion(epService, listener, false, "s2,i2", "", BACKING_UNINDEXED, assertNoWhere);
    
            // test no where clause with unique on multiple props, exact specification of where-clause
            var assertSendEvents = new IndexAssertionEventSend(() => {
                string[] fields = "c0,c1".Split(',');
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 1, 3, 10));
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 1, 2, 0));
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E3", 1, 3, 9));
                epService.EPRuntime.SendEvent(new SupportSimpleBeanOne("EX", 1, 3, 9));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"EX", "E3"});
            });
            TryAssertion(epService, listener, false, "d2,i2", "where ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1", BACKING_MULTI_UNIQUE, assertSendEvents);
            TryAssertion(epService, listener, false, "d2,i2", "where ssb2.d2 = ssb1.d1 and ssb2.i2 = ssb1.i1", BACKING_MULTI_UNIQUE, assertSendEvents);
            TryAssertion(epService, listener, false, "d2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.d2 = ssb1.d1 and ssb2.i2 = ssb1.i1", BACKING_MULTI_UNIQUE, assertSendEvents);
            TryAssertion(epService, listener, false, "d2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1", BACKING_MULTI_DUPS, assertSendEvents);
            TryAssertion(epService, listener, false, "d2,i2", "where ssb2.d2 = ssb1.d1", BACKING_SINGLE_DUPS, assertSendEvents);
            TryAssertion(epService, listener, false, "d2,i2", "where ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1 and ssb2.l2 between 1 and 1000", BACKING_MULTI_UNIQUE, assertSendEvents);
            TryAssertion(epService, listener, false, "d2,i2", "where ssb2.d2 = ssb1.d1 and ssb2.l2 between 1 and 1000", BACKING_COMPOSITE, assertSendEvents);
            TryAssertion(epService, listener, false, "i2,d2,l2", "where ssb2.l2 = ssb1.l1 and ssb2.d2 = ssb1.d1", BACKING_MULTI_DUPS, assertSendEvents);
            TryAssertion(epService, listener, false, "i2,d2,l2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1", BACKING_MULTI_UNIQUE, assertSendEvents);
            TryAssertion(epService, listener, false, "d2,l2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1", BACKING_MULTI_UNIQUE, assertSendEvents);
            TryAssertion(epService, listener, false, "d2,l2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1 and ssb2.s2 between 'E3' and 'E4'", BACKING_MULTI_UNIQUE, assertSendEvents);
            TryAssertion(epService, listener, false, "l2", "where ssb2.l2 = ssb1.l1", BACKING_SINGLE_UNIQUE, assertSendEvents);
            TryAssertion(epService, listener, true, "l2", "where ssb2.l2 = ssb1.l1", BACKING_SINGLE_DUPS, assertSendEvents);
            TryAssertion(epService, listener, false, "l2", "where ssb2.l2 = ssb1.l1 and ssb1.i1 between 1 and 20", BACKING_SINGLE_UNIQUE, assertSendEvents);
        }
    
        private void TryAssertion(EPServiceProvider epService, SupportUpdateListener listener, bool disableImplicitUniqueIdx, string uniqueFields, string whereClause, string backingTable, IndexAssertionEventSend assertion) {
            SupportQueryPlanIndexHook.Reset();
            string eplUnique = INDEX_CALLBACK_HOOK + "select s1 as c0, " +
                    "(select s2 from SSB2#unique(" + uniqueFields + ") as ssb2 " + whereClause + ") as c1 " +
                    "from SSB1 as ssb1";
            if (disableImplicitUniqueIdx) {
                eplUnique = "@Hint('DISABLE_UNIQUE_IMPLICIT_IDX')" + eplUnique;
            }
            EPStatement stmtUnique = epService.EPAdministrator.CreateEPL(eplUnique);
            stmtUnique.Events += listener.Update;
    
            SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(0, null, backingTable);
    
            assertion.Invoke();
    
            stmtUnique.Dispose();
        }
    
        private void RunAssertionUniqueIndexCorrelated(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            string[] fields = "c0,c1".Split(',');
            var listener = new SupportUpdateListener();
    
            // test std:unique
            SupportQueryPlanIndexHook.Reset();
            string eplUnique = INDEX_CALLBACK_HOOK + "select id as c0, " +
                    "(select IntPrimitive from SupportBean#unique(TheString) where TheString = s0.p00) as c1 " +
                    "from S0 as s0";
            EPStatement stmtUnique = epService.EPAdministrator.CreateEPL(eplUnique);
            stmtUnique.Events += listener.Update;
    
            SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(0, null, BACKING_SINGLE_UNIQUE);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 4});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(11, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{11, 3});
    
            stmtUnique.Dispose();
    
            // test std:firstunique
            SupportQueryPlanIndexHook.Reset();
            string eplFirstUnique = INDEX_CALLBACK_HOOK + "select id as c0, " +
                    "(select IntPrimitive from SupportBean#firstunique(TheString) where TheString = s0.p00) as c1 " +
                    "from S0 as s0";
            EPStatement stmtFirstUnique = epService.EPAdministrator.CreateEPL(eplFirstUnique);
            stmtFirstUnique.Events += listener.Update;
    
            SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(0, null, BACKING_SINGLE_UNIQUE);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 2});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(11, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{11, 1});
    
            stmtFirstUnique.Dispose();
    
            // test intersection std:firstunique
            SupportQueryPlanIndexHook.Reset();
            string eplIntersection = INDEX_CALLBACK_HOOK + "select id as c0, " +
                    "(select IntPrimitive from SupportBean#time(1)#unique(TheString) where TheString = s0.p00) as c1 " +
                    "from S0 as s0";
            EPStatement stmtIntersection = epService.EPAdministrator.CreateEPL(eplIntersection);
            stmtIntersection.Events += listener.Update;
    
            SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(0, null, BACKING_SINGLE_UNIQUE);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 4});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(11, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{11, 3});
    
            stmtIntersection.Dispose();
    
            // test grouped unique
            SupportQueryPlanIndexHook.Reset();
            string eplGrouped = INDEX_CALLBACK_HOOK + "select id as c0, " +
                    "(select LongPrimitive from SupportBean#groupwin(TheString)#unique(IntPrimitive) where TheString = s0.p00 and IntPrimitive = s0.id) as c1 " +
                    "from S0 as s0";
            EPStatement stmtGrouped = epService.EPAdministrator.CreateEPL(eplGrouped);
            stmtGrouped.Events += listener.Update;
    
            SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(0, null, BACKING_MULTI_UNIQUE);
    
            epService.EPRuntime.SendEvent(MakeBean("E1", 1, 100));
            epService.EPRuntime.SendEvent(MakeBean("E1", 2, 101));
            epService.EPRuntime.SendEvent(MakeBean("E1", 1, 102));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, 102L});
    
            stmtGrouped.Dispose();
        }
    
        private SupportBean MakeBean(string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    }
} // end of namespace
