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
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectMultirow : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("S0", typeof(SupportBean_S0));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionMultirowSingleColumn(epService);
            RunAssertionMultirowUnderlyingCorrelated(epService);
        }
    
        private void RunAssertionMultirowSingleColumn(EPServiceProvider epService) {
            // test named window as well as stream
            epService.EPAdministrator.CreateEPL("create window SupportWindow#length(3) as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into SupportWindow select * from SupportBean");
    
            string stmtText = "select p00, (select window(IntPrimitive) from SupportBean#keepall sb) as val from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "p00,val".Split(',');
    
            var rows = new object[][]{
                new object[] {"p00", typeof(string)},
                new object[] {"val", typeof(int[])}
            };
            for (int i = 0; i < rows.Length; i++) {
                string message = "Failed assertion for " + rows[i][0];
                EventPropertyDescriptor prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                Assert.AreEqual(rows[i][1], prop.PropertyType, message);
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("T1", 5));
            epService.EPRuntime.SendEvent(new SupportBean("T2", 10));
            epService.EPRuntime.SendEvent(new SupportBean("T3", 15));
            epService.EPRuntime.SendEvent(new SupportBean("T1", 6));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, new int[]{5, 10, 15, 6}});
    
            // test named window and late start
            stmt.Dispose();
    
            stmtText = "select p00, (select window(IntPrimitive) from SupportWindow) as val from S0 as s0";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, new int[]{10, 15, 6}});   // length window 3
    
            epService.EPRuntime.SendEvent(new SupportBean("T1", 5));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, new int[]{15, 6, 5}});   // length window 3
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultirowUnderlyingCorrelated(EPServiceProvider epService) {
            string stmtText = "select p00, " +
                    "(select window(sb.*) from SupportBean#keepall sb where TheString = s0.p00) as val " +
                    "from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var rows = new object[][]{
                new object[] {"p00", typeof(string)},
                new object[] {"val", typeof(SupportBean[])}
            };
            for (int i = 0; i < rows.Length; i++) {
                string message = "Failed assertion for " + rows[i][0];
                EventPropertyDescriptor prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                Assert.AreEqual(rows[i][1], prop.PropertyType, message);
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "T1"));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("val"));
    
            var sb1 = new SupportBean("T1", 10);
            epService.EPRuntime.SendEvent(sb1);
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "T1"));
    
            EventBean received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(typeof(SupportBean[]), received.Get("val").GetType());
            EPAssertionUtil.AssertEqualsAnyOrder((object[]) received.Get("val"), new object[]{sb1});
    
            var sb2 = new SupportBean("T2", 20);
            epService.EPRuntime.SendEvent(sb2);
            var sb3 = new SupportBean("T2", 30);
            epService.EPRuntime.SendEvent(sb3);
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "T2"));
    
            received = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsAnyOrder((object[]) received.Get("val"), new object[]{sb2, sb3});
    
            stmt.Dispose();
        }
    }
} // end of namespace
