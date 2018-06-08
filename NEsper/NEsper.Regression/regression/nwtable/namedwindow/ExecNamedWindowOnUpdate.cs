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
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowOnUpdate : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
    
            RunAssertionUpdateNonPropertySet(epService);
            RunAssertionMultipleDataWindowIntersect(epService);
            RunAssertionMultipleDataWindowUnion(epService);
            RunAssertionSubclass(epService);
        }
    
        private void RunAssertionUpdateNonPropertySet(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("setBeanLongPrimitive999", GetType(), "SetBeanLongPrimitive999");
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.CreateEPL("create window MyWindowUNP#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowUNP select * from SupportBean");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("on SupportBean_S0 as sb " +
                    "update MyWindowUNP as mywin" +
                    " set mywin.IntPrimitive = 10," +
                    "     setBeanLongPrimitive999(mywin)");
            var listenerWindow = new SupportUpdateListener();
            stmt.Events += listenerWindow.Update;
    
            string[] fields = "IntPrimitive,LongPrimitive".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(listenerWindow.GetAndResetLastNewData()[0], fields, new object[]{10, 999L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMultipleDataWindowIntersect(EPServiceProvider epService) {
            string stmtTextCreate = "create window MyWindowMDW#unique(TheString)#length(2) as select * from SupportBean";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            string stmtTextInsertOne = "insert into MyWindowMDW select * from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            string stmtTextUpdate = "on SupportBean_A update MyWindowMDW set IntPrimitive=IntPrimitive*100 where TheString=id";
            epService.EPAdministrator.CreateEPL(stmtTextUpdate);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EventBean[] newevents = listenerWindow.LastNewData;
            EventBean[] oldevents = listenerWindow.LastOldData;
    
            Assert.AreEqual(1, newevents.Length);
            EPAssertionUtil.AssertProps(newevents[0], "IntPrimitive".Split(','), new object[]{300});
            Assert.AreEqual(1, oldevents.Length);
            oldevents = EPAssertionUtil.Sort(oldevents, "TheString");
            EPAssertionUtil.AssertPropsPerRow(oldevents, "TheString,IntPrimitive".Split(','), new object[][]{new object[] {"E2", 3}});
    
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "TheString,IntPrimitive".Split(','), new object[][]{new object[] {"E1", 2}, new object[] {"E2", 300}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMultipleDataWindowUnion(EPServiceProvider epService) {
            string stmtTextCreate = "create window MyWindowMU#unique(TheString)#length(2) retain-union as select * from SupportBean";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            string stmtTextInsertOne = "insert into MyWindowMU select * from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            string stmtTextUpdate = "on SupportBean_A update MyWindowMU mw set mw.IntPrimitive=IntPrimitive*100 where TheString=id";
            epService.EPAdministrator.CreateEPL(stmtTextUpdate);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EventBean[] newevents = listenerWindow.LastNewData;
            EventBean[] oldevents = listenerWindow.LastOldData;
    
            Assert.AreEqual(1, newevents.Length);
            EPAssertionUtil.AssertProps(newevents[0], "IntPrimitive".Split(','), new object[]{300});
            Assert.AreEqual(1, oldevents.Length);
            EPAssertionUtil.AssertPropsPerRow(oldevents, "TheString,IntPrimitive".Split(','), new object[][]{new object[] {"E2", 3}});
    
            EventBean[] events = EPAssertionUtil.Sort(stmtCreate.GetEnumerator(), "TheString");
            EPAssertionUtil.AssertPropsPerRow(events, "TheString,IntPrimitive".Split(','), new object[][]{new object[] {"E1", 2}, new object[] {"E2", 300}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSubclass(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowSC#keepall as select * from " + typeof(SupportBeanAbstractSub).MaskTypeName();
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowSC select * from " + typeof(SupportBeanAbstractSub).MaskTypeName();
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create update
            string stmtTextUpdate = "on " + typeof(SupportBean).MaskTypeName() + " update MyWindowSC set V1=TheString, V2=TheString";
            epService.EPAdministrator.CreateEPL(stmtTextUpdate);
    
            epService.EPRuntime.SendEvent(new SupportBeanAbstractSub("value2"));
            listenerWindow.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], new string[]{ "V1", "V2"}, new object[]{"E1", "E1"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        // Don't delete me, dynamically-invoked
        public static void SetBeanLongPrimitive999(SupportBean @event) {
            @event.LongPrimitive = 999;
        }
    }
} // end of namespace
