///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionGreedyness
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TestReluctantOneToMany()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            String[] fields = "a0,a1,a2,b,c".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
                          "match_recognize (" +
                          "  measures A[0].TheString as a0, A[1].TheString as a1, A[2].TheString as a2, B.TheString as b, C.TheString as c" +
                          "  pattern (A+? B? C) " +
                          "  define " +
                          "   A as A.Value = 1," +
                          "   B as B.Value in (1, 2)," +
                          "   C as C.Value = 3" +
                          ")";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E1", "E2", null, "E3", "E4"}});

            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E12", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E13", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E14", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E15", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E11", "E12", "E13", "E14", "E15"}});

            epService.EPRuntime.SendEvent(new SupportRecogBean("E16", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E17", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E16", null, null, null, "E17"}});

            epService.EPRuntime.SendEvent(new SupportRecogBean("E18", 3));
            Assert.IsFalse(listener.IsInvoked);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestReluctantZeroToMany()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            String[] fields = "a0,a1,a2,b,c".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
                          "match_recognize (" +
                          "  measures A[0].TheString as a0, A[1].TheString as a1, A[2].TheString as a2, B.TheString as b, C.TheString as c" +
                          "  pattern (A*? B? C) " +
                          "  define " +
                          "   A as A.Value = 1," +
                          "   B as B.Value in (1, 2)," +
                          "   C as C.Value = 3" +
                          ")";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E1", "E2", null, "E3", "E4"}});

            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E12", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E13", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E14", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E15", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E11", "E12", "E13", "E14", "E15"}});

            epService.EPRuntime.SendEvent(new SupportRecogBean("E16", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E17", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {null, null, null, "E16", "E17"}});

            epService.EPRuntime.SendEvent(new SupportRecogBean("E18", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {null, null, null, null, "E18"}});

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestReluctantZeroToOne()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            String[] fields = "a_string,b_string".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
                          "match_recognize (" +
                          "  measures A.TheString as a_string, B.TheString as b_string " +
                          "  pattern (A?? B?) " +
                          "  define " +
                          "   A as A.Value = 1," +
                          "   B as B.Value = 1" +
                          ")";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {null, "E1"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[] {new Object[] {null, "E1"}});

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    }
}
