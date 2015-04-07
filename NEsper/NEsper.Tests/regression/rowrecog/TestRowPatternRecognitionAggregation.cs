///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionAggregation
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        #endregion

        private EPServiceProvider _epService;

        [Test]
        public void TestMeasureAggregation()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();

            String text = "select * from MyEvent.win:keepall() " +
                          "match_recognize (" +
                          "  measures A.TheString as a_string, " +
                          "       C.TheString as c_string, " +
                          "       max(B.Value) as maxb, " +
                          "       min(B.Value) as minb, " +
                          "       2*min(B.Value) as minb2x, " +
                          "       last(B.Value) as lastb, " +
                          "       first(B.Value) as firstb," +
                          "       count(B.Value) as countb " +
                          "  all matches pattern (A B* C) " +
                          "  define " +
                          "   A as (A.Value = 0)," +
                          "   B as (B.Value != 1)," +
                          "   C as (C.Value = 1)" +
                          ") " +
                          "order by a_string";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            String[] fields = "a_string,c_string,maxb,minb,minb2x,firstb,lastb,countb".Split(',');
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 1));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields,
                new[]
                {
                    new Object[] {"E1", "E2", null, null, null, null, null, 0L}
                });
            EPAssertionUtil.AssertPropsPerRow(
                stmt.GetEnumerator(), fields,
                new[]
                {
                    new Object[] {"E1", "E2", null, null, null, null, null, 0L}
                });

            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 0));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 1));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields,
                new[]
                {
                    new Object[] {"E3", "E6", 5, 3, 6, 5, 3, 2L}
                });
            EPAssertionUtil.AssertPropsPerRow(
                stmt.GetEnumerator(), fields,
                new[]
                {
                    new Object[] {"E1", "E2", null, null, null, null, null, 0L},
                    new Object[] {"E3", "E6", 5, 3, 6, 5, 3, 2L}
                });

            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 0));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E9", -1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E10", 7));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E12", 1));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields,
                new[]
                {
                    new Object[] {"E7", "E12", 7, -1, -2, 4, 2, 4L}
                });
            EPAssertionUtil.AssertPropsPerRow(
                stmt.GetEnumerator(), fields,
                new[]
                {
                    new Object[] {"E1", "E2", null, null, null, null, null, 0L},
                    new Object[] {"E3", "E6", 5, 3, 6, 5, 3, 2L},
                    new Object[] {"E7", "E12", 7, -1, -2, 4, 2, 4L},
                });
        }

        [Test]
        public void TestMeasureAggregationPartitioned()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();

            String text = "select * from MyEvent.win:keepall() " +
                          "match_recognize (" +
                          "  partition by Cat" +
                          "  measures A.Cat as Cat, A.TheString as a_string, " +
                          "       D.TheString as d_string, " +
                          "       sum(C.Value) as sumc, " +
                          "       sum(B.Value) as sumb, " +
                          "       sum(B.Value + A.Value) as sumaplusb, " +
                          "       sum(C.Value + A.Value) as sumaplusc " +
                          "  all matches pattern (A B B C C D) " +
                          "  define " +
                          "   A as (A.Value >= 10)," +
                          "   B as (B.Value > 1)," +
                          "   C as (C.Value < -1)," +
                          "   D as (D.Value = 999)" +
                          ") order by Cat";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            String[] fields = "a_string,d_string,sumb,sumc,sumaplusb,sumaplusc".Split(',');
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", "x", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", "y", 20));

            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", "x", 7)); // B
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", "y", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", "x", 8));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", "y", 2));

            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", "x", -2)); // C
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", "y", -7));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E9", "x", -5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E10", "y", -4));

            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", "y", 999));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields,
                new[]
                {
                    new Object[] {"E2", "E11", 7, -11, 47, 29}
                });
            EPAssertionUtil.AssertPropsPerRow(
                stmt.GetEnumerator(), fields,
                new[]
                {
                    new Object[] {"E2", "E11", 7, -11, 47, 29}
                });

            epService.EPRuntime.SendEvent(new SupportRecogBean("E12", "x", 999));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields,
                new[]
                {
                    new Object[] {"E1", "E12", 15, -7, 35, 13}
                });
            EPAssertionUtil.AssertPropsPerRow(
                stmt.GetEnumerator(), fields,
                new[]
                {
                    new Object[] {"E1", "E12", 15, -7, 35, 13},
                    new Object[] {"E2", "E11", 7, -11, 47, 29}
                });
        }
    }
}
