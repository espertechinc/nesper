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
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.rowrecog;


using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogGreedyness : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("MyEvent", typeof(SupportRecogBean));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionReluctantZeroToOne(epService);
            RunAssertionReluctantZeroToMany(epService);
            RunAssertionReluctantOneToMany(epService);
        }
    
        private void RunAssertionReluctantZeroToOne(EPServiceProvider epService) {
            string[] fields = "a_string,b_string".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string, B.TheString as b_string " +
                    "  pattern (A?? B?) " +
                    "  define " +
                    "   A as A.value = 1," +
                    "   B as B.value = 1" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {null, "E1"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {null, "E1"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionReluctantZeroToMany(EPServiceProvider epService) {
            string[] fields = "a0,a1,a2,b,c".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A[0].TheString as a0, A[1].TheString as a1, A[2].TheString as a2, B.TheString as b, C.TheString as c" +
                    "  pattern (A*? B? C) " +
                    "  define " +
                    "   A as A.value = 1," +
                    "   B as B.value in (1, 2)," +
                    "   C as C.value = 3" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E1", "E2", null, "E3", "E4"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E12", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E13", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E14", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E15", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E11", "E12", "E13", "E14", "E15"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E16", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E17", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {null, null, null, "E16", "E17"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E18", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {null, null, null, null, "E18"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionReluctantOneToMany(EPServiceProvider epService) {
            string[] fields = "a0,a1,a2,b,c".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A[0].TheString as a0, A[1].TheString as a1, A[2].TheString as a2, B.TheString as b, C.TheString as c" +
                    "  pattern (A+? B? C) " +
                    "  define " +
                    "   A as A.value = 1," +
                    "   B as B.value in (1, 2)," +
                    "   C as C.value = 3" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E1", "E2", null, "E3", "E4"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E12", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E13", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E14", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E15", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E11", "E12", "E13", "E14", "E15"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E16", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E17", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E16", null, null, null, "E17"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E18", 3));
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    }
} // end of namespace
