///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.rowrecog;


using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogIterateOnly : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("MyEvent", typeof(SupportRecogBean));
            configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
            configuration.AddImport(typeof(HintAttribute).FullName);
            configuration.AddVariable("mySleepDuration", typeof(long), 100);    // msec
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionNoListenerMode(epService);
            RunAssertionPrev(epService);
            RunAssertionPrevPartitioned(epService);
        }
    
        private void RunAssertionNoListenerMode(EPServiceProvider epService) {
            string[] fields = "a".Split(',');
            string text = "@Hint('iterate_only') select * from MyEvent#length(1) " +
                    "match_recognize (" +
                    "  measures A.TheString as a" +
                    "  all matches " +
                    "  pattern (A) " +
                    "  define A as SupportStaticMethodLib.SleepReturnTrue(mySleepDuration)" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // this should not block
            long start = PerformanceObserver.MilliTime;
            for (int i = 0; i < 50; i++) {
                epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            }
            long end = PerformanceObserver.MilliTime;
            Assert.IsTrue((end - start) <= 100);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 2));
            epService.EPRuntime.SetVariableValue("mySleepDuration", 0);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionPrev(EPServiceProvider epService) {
            string[] fields = "a".Split(',');
            string text = "@Hint('iterate_only') select * from MyEvent#lastevent " +
                    "match_recognize (" +
                    "  measures A.TheString as a" +
                    "  all matches " +
                    "  pattern (A) " +
                    "  define A as prev(A.value, 2) = value" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 2));
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 4));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E6"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 2));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E7"}});
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPrevPartitioned(EPServiceProvider epService) {
            string[] fields = "a,cat".Split(',');
            string text = "@Hint('iterate_only') select * from MyEvent#lastevent " +
                    "match_recognize (" +
                    "  partition by cat" +
                    "  measures A.TheString as a, A.cat as cat" +
                    "  all matches " +
                    "  pattern (A) " +
                    "  define A as prev(A.value, 2) = value" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", "A", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", "B", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", "B", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", "A", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", "B", 2));
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", "A", 1));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E6", "A"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", "B", 3));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E7", "B"}});
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    }
} // end of namespace
