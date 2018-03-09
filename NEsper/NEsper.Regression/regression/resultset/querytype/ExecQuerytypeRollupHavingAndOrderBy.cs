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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeRollupHavingAndOrderBy : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
    
            RunAssertionHaving(epService, false);
            RunAssertionHaving(epService, true);
    
            RunAssertionIteratorWindow(epService, false);
            RunAssertionIteratorWindow(epService, true);
    
            RunAssertionOrderByTwoCriteriaAsc(epService, false);
            RunAssertionOrderByTwoCriteriaAsc(epService, true);
    
            RunAssertionUnidirectional(epService);
            RunAssertionOrderByOneCriteriaDesc(epService);
        }
    
        private void RunAssertionIteratorWindow(EPServiceProvider epService, bool join) {
    
            string[] fields = "c0,c1".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, sum(IntPrimitive) as c1 " +
                    "from SupportBean#length(3) " + (join ? ", SupportBean_S0#keepall " : "") +
                    "group by Rollup(TheString)");
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {null, 1}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {null, 3}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 4}, new object[] {"E2", 2}, new object[] {null, 6}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E2", 6}, new object[] {"E1", 3}, new object[] {null, 9}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionUnidirectional(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                    "from SupportBean_S0 unidirectional, SupportBean#keepall " +
                    "group by Cube(TheString, IntPrimitive)").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 200));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 300));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 400));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{
                            new object[] {"E1", 10, 100L},
                            new object[] {"E2", 20, 600L},
                            new object[] {"E1", 11, 300L},
                            new object[] {"E1", null, 400L},
                            new object[] {"E2", null, 600L},
                            new object[] {null, 10, 100L},
                            new object[] {null, 20, 600L},
                            new object[] {null, 11, 300L},
                            new object[] {null, null, 1000L}
                    });
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{
                            new object[] {"E1", 10, 101L},
                            new object[] {"E2", 20, 600L},
                            new object[] {"E1", 11, 300L},
                            new object[] {"E1", null, 401L},
                            new object[] {"E2", null, 600L},
                            new object[] {null, 10, 101L},
                            new object[] {null, 20, 600L},
                            new object[] {null, 11, 300L},
                            new object[] {null, null, 1001L}
                    });
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionHaving(EPServiceProvider epService, bool join) {
    
            // test having on the aggregation alone
            string[] fields = "c0,c1,c2".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                    "from SupportBean#keepall " + (join ? ", SupportBean_S0#lastevent " : "") +
                    "group by Rollup(TheString, IntPrimitive)" +
                    "having sum(LongPrimitive) > 1000").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 200));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 300));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 400));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 500));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {null, null, 1500L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 600));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E2", 20, 1200L}, new object[] {"E2", null, 1200L}, new object[] {null, null, 2100L}});
            epService.EPAdministrator.DestroyAllStatements();
    
            // test having on the aggregation alone
            string[] fieldsC0C1 = "c0,c1".Split(',');
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, sum(IntPrimitive) as c1 " +
                    "from SupportBean#keepall " + (join ? ", SupportBean_S0#lastevent " : "") +
                    "group by Rollup(TheString) " +
                    "having " +
                    "(TheString is null and sum(IntPrimitive) > 100) " +
                    "or " +
                    "(TheString is not null and sum(IntPrimitive) > 200)").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 50));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fieldsC0C1,
                    new object[][]{new object[] {null, 120}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", -300));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 200));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fieldsC0C1,
                    new object[][]{new object[] {"E1", 250}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 500));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fieldsC0C1,
                    new object[][]{new object[] {"E2", 570}, new object[] {null, 520}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOrderByTwoCriteriaAsc(EPServiceProvider epService, bool join) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            string[] fields = "c0,c1,c2".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                    "from SupportBean#time_batch(1 sec) " + (join ? ", SupportBean_S0#lastevent " : "") +
                    "group by Rollup(TheString, IntPrimitive) " +
                    "order by TheString, IntPrimitive").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 10, 100));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 200));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 300));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 400));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 1000L},
                            new object[] {"E1", null, 900L},
                            new object[] {"E1", 10, 300L},
                            new object[] {"E1", 11, 600L},
                            new object[] {"E2", null, 100L},
                            new object[] {"E2", 10, 100L},
                    },
                    new object[][]{new object[] {null, null, null},
                            new object[] {"E1", null, null},
                            new object[] {"E1", 10, null},
                            new object[] {"E1", 11, null},
                            new object[] {"E2", null, null},
                            new object[] {"E2", 10, null},
                    });
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 500));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 600));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 12, 700));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 1800L},
                            new object[] {"E1", null, 1800L},
                            new object[] {"E1", 10, 600L},
                            new object[] {"E1", 11, 500L},
                            new object[] {"E1", 12, 700L},
                            new object[] {"E2", null, null},
                            new object[] {"E2", 10, null},
                    },
                    new object[][]{new object[] {null, null, 1000L},
                            new object[] {"E1", null, 900L},
                            new object[] {"E1", 10, 300L},
                            new object[] {"E1", 11, 600L},
                            new object[] {"E1", 12, null},
                            new object[] {"E2", null, 100L},
                            new object[] {"E2", 10, 100L},
                    });
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOrderByOneCriteriaDesc(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            string[] fields = "c0,c1,c2".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean#time_batch(1 sec) " +
                    "group by Rollup(TheString, IntPrimitive) " +
                    "order by TheString desc").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 10, 100));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 200));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 300));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 400));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                            new object[] {"E2", 10, 100L},
                            new object[] {"E2", null, 100L},
                            new object[] {"E1", 11, 600L},
                            new object[] {"E1", 10, 300L},
                            new object[] {"E1", null, 900L},
                            new object[] {null, null, 1000L},
                    },
                    new object[][]{
                            new object[] {"E2", 10, null},
                            new object[] {"E2", null, null},
                            new object[] {"E1", 11, null},
                            new object[] {"E1", 10, null},
                            new object[] {"E1", null, null},
                            new object[] {null, null, null},
                    });
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 500));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 600));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 12, 700));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                            new object[] {"E2", 10, null},
                            new object[] {"E2", null, null},
                            new object[] {"E1", 11, 500L},
                            new object[] {"E1", 10, 600L},
                            new object[] {"E1", 12, 700L},
                            new object[] {"E1", null, 1800L},
                            new object[] {null, null, 1800L},
                    },
                    new object[][]{
                            new object[] {"E2", 10, 100L},
                            new object[] {"E2", null, 100L},
                            new object[] {"E1", 11, 600L},
                            new object[] {"E1", 10, 300L},
                            new object[] {"E1", 12, null},
                            new object[] {"E1", null, 900L},
                            new object[] {null, null, 1000L},
                    });
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive) {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            return sb;
        }
    }
} // end of namespace
