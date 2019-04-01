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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.fromclausemethod
{
    public class ExecFromClauseMethodJoinPerformance : RegressionExecution {
        /// <summary>
        /// Configures the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
            configuration.AddEventType(typeof(SupportBeanInt));
    
            var configMethod = new ConfigurationMethodRef();
            configMethod.SetLRUCache(10);
            configuration.AddMethodRef(typeof(SupportJoinMethods).FullName, configMethod);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertion1Stream2HistInnerJoinPerformance(epService);
            RunAssertion1Stream2HistOuterJoinPerformance(epService);
            RunAssertion2Stream1HistTwoSidedEntryIdenticalIndex(epService);
            RunAssertion2Stream1HistTwoSidedEntryMixedIndex(epService);
        }
    
        private void RunAssertion1Stream2HistInnerJoinPerformance(EPServiceProvider epService) {
            string expression;
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                    "from SupportBeanInt#lastevent as s0, " +
                    "method:SupportJoinMethods.FetchVal('H0', 100) as h0, " +
                    "method:SupportJoinMethods.FetchVal('H1', 100) as h1 " +
                    "where h0.index = p00 and h1.index = p00";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "id,valh0,valh1".Split(',');
            var random = new Random();
    
            long start = PerformanceObserver.MilliTime;
            for (int i = 1; i < 5000; i++) {
                int num = random.Next(98) + 1;
                SendBeanInt(epService, "E1", num);
    
                var result = new object[][]{new object[] {"E1", "H0" + num, "H1" + num}};
                EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            stmt.Dispose();
            Assert.IsTrue(delta < 1000, "Delta to large, at " + delta + " msec");
        }
    
        private void RunAssertion1Stream2HistOuterJoinPerformance(EPServiceProvider epService) {
            string expression;
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                    "from SupportBeanInt#lastevent as s0 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H0', 100) as h0 " +
                    " on h0.index = p00 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H1', 100) as h1 " +
                    " on h1.index = p00";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "id,valh0,valh1".Split(',');
            var random = new Random();
    
            long start = PerformanceObserver.MilliTime;
            for (int i = 1; i < 5000; i++) {
                int num = random.Next(98) + 1;
                SendBeanInt(epService, "E1", num);
    
                var result = new object[][]{new object[] {"E1", "H0" + num, "H1" + num}};
                EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            stmt.Dispose();
            Assert.IsTrue(delta < 1000, "Delta to large, at " + delta + " msec");
        }
    
        private void RunAssertion2Stream1HistTwoSidedEntryIdenticalIndex(EPServiceProvider epService) {
            string expression;
    
            expression = "select s0.id as s0id, s1.id as s1id, h0.val as valh0 " +
                    "from SupportBeanInt(id like 'E%')#lastevent as s0, " +
                    "method:SupportJoinMethods.FetchVal('H0', 100) as h0, " +
                    "SupportBeanInt(id like 'F%')#lastevent as s1 " +
                    "where h0.index = s0.p00 and h0.index = s1.p00";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "s0id,s1id,valh0".Split(',');
            var random = new Random();
    
            long start = PerformanceObserver.MilliTime;
            for (int i = 1; i < 1000; i++) {
                int num = random.Next(98) + 1;
                SendBeanInt(epService, "E1", num);
                SendBeanInt(epService, "F1", num);
    
                var result = new object[][]{new object[] {"E1", "F1", "H0" + num}};
                EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
    
                // send reset events to avoid duplicate matches
                SendBeanInt(epService, "E1", 0);
                SendBeanInt(epService, "F1", 0);
                listener.Reset();
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            Assert.IsTrue(delta < 1000, "Delta to large, at " + delta + " msec");
            stmt.Dispose();
        }
    
        private void RunAssertion2Stream1HistTwoSidedEntryMixedIndex(EPServiceProvider epService) {
            string expression;
    
            expression = "select s0.id as s0id, s1.id as s1id, h0.val as valh0, h0.index as indexh0 from " +
                    "method:SupportJoinMethods.FetchVal('H0', 100) as h0, " +
                    "SupportBeanInt(id like 'H%')#lastevent as s1, " +
                    "SupportBeanInt(id like 'E%')#lastevent as s0 " +
                    "where h0.index = s0.p00 and h0.val = s1.id";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "s0id,s1id,valh0,indexh0".Split(',');
            var random = new Random();
    
            long start = PerformanceObserver.MilliTime;
            for (int i = 1; i < 1000; i++) {
                int num = random.Next(98) + 1;
                SendBeanInt(epService, "E1", num);
                SendBeanInt(epService, "H0" + num, num);
    
                var result = new object[][]{new object[] {"E1", "H0" + num, "H0" + num, num}};
                EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
    
                // send reset events to avoid duplicate matches
                SendBeanInt(epService, "E1", 0);
                SendBeanInt(epService, "F1", 0);
                listener.Reset();
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            stmt.Dispose();
            Assert.IsTrue(delta < 1000, "Delta to large, at " + delta + " msec");
        }
    
        private void SendBeanInt(EPServiceProvider epService, string id, int p00, int p01, int p02, int p03) {
            epService.EPRuntime.SendEvent(new SupportBeanInt(id, p00, p01, p02, p03, -1, -1));
        }
    
        private void SendBeanInt(EPServiceProvider epService, string id, int p00) {
            SendBeanInt(epService, id, p00, -1, -1, -1);
        }
    }
} // end of namespace
