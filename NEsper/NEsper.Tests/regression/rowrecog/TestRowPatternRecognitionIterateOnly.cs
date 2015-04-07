///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionIterateOnly
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TestNoListenerMode()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            config.AddImport(typeof(SupportStaticMethodLib).FullName);
            config.AddImport(typeof(HintAttribute).FullName);
            config.AddVariable("mySleepDuration", typeof(long), 100); // msec
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            String[] fields = "a".Split(',');
            String text = "@Hint('iterate_only') select * from MyEvent.win:length(1) " +
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
            for (int i = 0; i < 50; i++)
            {
                epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            }
            long end = PerformanceObserver.MilliTime;
            Assert.IsTrue((end - start) <= 100);
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 2));
            epService.EPRuntime.SetVariableValue("mySleepDuration", 0);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[] {new Object[] {"E2"}});

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestPrev()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            String[] fields = "a".Split(',');
            String text = "@Hint('iterate_only') select * from MyEvent.std:lastevent() " +
                          "match_recognize (" +
                          "  measures A.TheString as a" +
                          "  all matches " +
                          "  pattern (A) " +
                          "  define A as prev(A.Value, 2) = Value" +
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
                                                      new[] {new Object[] {"E6"}});

            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 2));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[] {new Object[] {"E7"}});
            Assert.IsFalse(listener.IsInvoked);
        
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestPrevPartitioned()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            String[] fields = "a,Cat".Split(',');
            String text = "@Hint('iterate_only') select * from MyEvent.std:lastevent() " +
                          "match_recognize (" +
                          "  partition by Cat" +
                          "  measures A.TheString as a, A.Cat as Cat" +
                          "  all matches " +
                          "  pattern (A) " +
                          "  define A as prev(A.Value, 2) = Value" +
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
                                                      new[] {new Object[] {"E6", "A"}});

            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", "B", 3));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[] {new Object[] {"E7", "B"}});
            Assert.IsFalse(listener.IsInvoked);
        
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    }
}
