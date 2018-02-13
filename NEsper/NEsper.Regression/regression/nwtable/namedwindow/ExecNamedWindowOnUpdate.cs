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

// using static org.junit.Assert.assertEquals;

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
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("setBeanLongPrimitive999", GetType().FullName, "setBeanLongPrimitive999");
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.CreateEPL("create window MyWindowUNP#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowUNP select * from SupportBean");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("on SupportBean_S0 as sb " +
                    "update MyWindowUNP as mywin" +
                    " set Mywin.IntPrimitive = 10," +
                    "     SetBeanLongPrimitive999(mywin)");
            var listenerWindow = new SupportUpdateListener();
            stmt.AddListener(listenerWindow);
    
            string[] fields = "intPrimitive,longPrimitive".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(listenerWindow.GetAndResetLastNewData()[0], fields, new Object[]{10, 999L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMultipleDataWindowIntersect(EPServiceProvider epService) {
            string stmtTextCreate = "create window MyWindowMDW#unique(theString)#length(2) as select * from SupportBean";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.AddListener(listenerWindow);
    
            string stmtTextInsertOne = "insert into MyWindowMDW select * from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            string stmtTextUpdate = "on SupportBean_A update MyWindowMDW set intPrimitive=intPrimitive*100 where theString=id";
            epService.EPAdministrator.CreateEPL(stmtTextUpdate);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EventBean[] newevents = listenerWindow.LastNewData;
            EventBean[] oldevents = listenerWindow.LastOldData;
    
            Assert.AreEqual(1, newevents.Length);
            EPAssertionUtil.AssertProps(newevents[0], "intPrimitive".Split(','), new Object[]{300});
            Assert.AreEqual(1, oldevents.Length);
            oldevents = EPAssertionUtil.Sort(oldevents, "theString");
            EPAssertionUtil.AssertPropsPerRow(oldevents, "theString,intPrimitive".Split(','), new Object[][]{new object[] {"E2", 3}});
    
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "theString,intPrimitive".Split(','), new Object[][]{new object[] {"E1", 2}, new object[] {"E2", 300}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMultipleDataWindowUnion(EPServiceProvider epService) {
            string stmtTextCreate = "create window MyWindowMU#unique(theString)#length(2) retain-union as select * from SupportBean";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.AddListener(listenerWindow);
    
            string stmtTextInsertOne = "insert into MyWindowMU select * from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            string stmtTextUpdate = "on SupportBean_A update MyWindowMU mw set mw.intPrimitive=intPrimitive*100 where theString=id";
            epService.EPAdministrator.CreateEPL(stmtTextUpdate);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EventBean[] newevents = listenerWindow.LastNewData;
            EventBean[] oldevents = listenerWindow.LastOldData;
    
            Assert.AreEqual(1, newevents.Length);
            EPAssertionUtil.AssertProps(newevents[0], "intPrimitive".Split(','), new Object[]{300});
            Assert.AreEqual(1, oldevents.Length);
            EPAssertionUtil.AssertPropsPerRow(oldevents, "theString,intPrimitive".Split(','), new Object[][]{new object[] {"E2", 3}});
    
            EventBean[] events = EPAssertionUtil.Sort(stmtCreate.GetEnumerator(), "theString");
            EPAssertionUtil.AssertPropsPerRow(events, "theString,intPrimitive".Split(','), new Object[][]{new object[] {"E1", 2}, new object[] {"E2", 300}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSubclass(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowSC#keepall as select * from " + typeof(SupportBeanAbstractSub).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.AddListener(listenerWindow);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowSC select * from " + typeof(SupportBeanAbstractSub).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create update
            string stmtTextUpdate = "on " + typeof(SupportBean).FullName + " update MyWindowSC set v1=theString, v2=theString";
            epService.EPAdministrator.CreateEPL(stmtTextUpdate);
    
            epService.EPRuntime.SendEvent(new SupportBeanAbstractSub("value2"));
            listenerWindow.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], new string[]{"v1", "v2"}, new Object[]{"E1", "E1"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        // Don't delete me, dynamically-invoked
        public static void SetBeanLongPrimitive999(SupportBean @event) {
            @event.LongPrimitive = 999;
        }
    }
} // end of namespace
