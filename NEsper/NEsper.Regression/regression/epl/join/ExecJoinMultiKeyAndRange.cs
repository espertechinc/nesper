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


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoinMultiKeyAndRange : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionRangeNullAndDupAndInvalid(epService);
            RunAssertionMultiKeyed(epService);
        }
    
        private void RunAssertionRangeNullAndDupAndInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanComplexProps", typeof(SupportBeanComplexProps));
    
            string eplOne = "select sb.* from SupportBean#keepall sb, SupportBeanRange#lastevent where IntBoxed between rangeStart and rangeEnd";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(eplOne);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            string eplTwo = "select sb.* from SupportBean#keepall sb, SupportBeanRange#lastevent where TheString = key and IntBoxed in [rangeStart: rangeEnd]";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(eplTwo);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            // null join lookups
            SendEvent(epService, new SupportBeanRange("R1", "G", (int?) null, null));
            SendEvent(epService, new SupportBeanRange("R2", "G", null, 10));
            SendEvent(epService, new SupportBeanRange("R3", "G", 10, null));
            SendSupportBean(epService, "G", -1, null);
    
            // range invalid
            SendEvent(epService, new SupportBeanRange("R4", "G", 10, 0));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(listenerTwo.IsInvoked);
    
            // duplicates
            Object eventOne = SendSupportBean(epService, "G", 100, 5);
            Object eventTwo = SendSupportBean(epService, "G", 101, 5);
            SendEvent(epService, new SupportBeanRange("R4", "G", 0, 10));
            EventBean[] events = listener.GetAndResetLastNewData();
            EPAssertionUtil.AssertEqualsAnyOrder(new object[]{eventOne, eventTwo}, EPAssertionUtil.GetUnderlying(events));
            events = listenerTwo.GetAndResetLastNewData();
            EPAssertionUtil.AssertEqualsAnyOrder(new object[]{eventOne, eventTwo}, EPAssertionUtil.GetUnderlying(events));
    
            // test string compare
            string eplThree = "select sb.* from SupportBeanRange#keepall sb, SupportBean#lastevent where TheString in [rangeStartStr:rangeEndStr]";
            epService.EPAdministrator.CreateEPL(eplThree);
    
            SendSupportBean(epService, "P", 1, 1);
            SendEvent(epService, new SupportBeanRange("R5", "R5", "O", "Q"));
            Assert.IsTrue(listener.IsInvoked);
    
        }
    
        private void RunAssertionMultiKeyed(EPServiceProvider epService) {
    
            string eventClass = typeof(SupportBean).FullName;
    
            string joinStatement = "select * from " +
                    eventClass + "(TheString='A')#length(3) as streamA," +
                    eventClass + "(TheString='B')#length(3) as streamB" +
                    " where streamA.IntPrimitive = streamB.IntPrimitive " +
                    "and streamA.IntBoxed = streamB.IntBoxed";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyType("streamA"));
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyType("streamB"));
            Assert.AreEqual(2, stmt.EventType.PropertyNames.Length);
    
            int[][] eventData = {
                new []{1, 100},
                new []{2, 100},
                new []{1, 200},
                new []{2, 200}};
            var eventsA = new SupportBean[eventData.Length];
            var eventsB = new SupportBean[eventData.Length];
    
            for (int i = 0; i < eventData.Length; i++) {
                eventsA[i] = new SupportBean();
                eventsA[i].TheString = "A";
                eventsA[i].IntPrimitive = eventData[i][0];
                eventsA[i].IntBoxed = eventData[i][1];
    
                eventsB[i] = new SupportBean();
                eventsB[i].TheString = "B";
                eventsB[i].IntPrimitive = eventData[i][0];
                eventsB[i].IntBoxed = eventData[i][1];
            }
    
            SendEvent(epService, eventsA[0]);
            SendEvent(epService, eventsB[1]);
            SendEvent(epService, eventsB[2]);
            SendEvent(epService, eventsB[3]);
            Assert.IsNull(listener.LastNewData);    // No events expected
        }
    
        private void SendEvent(EPServiceProvider epService, Object theEvent) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private SupportBean SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive, int? intBoxed) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.IntBoxed = intBoxed;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
} // end of namespace
