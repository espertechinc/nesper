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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionInterval
    {
        private void SendTimer(long time, EPServiceProvider epService)
        {
            var theEvent = new CurrentTimeEvent(time);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }

        [Test]
        public void TestInterval()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            SendTimer(0, epService);
            String[] fields = "a,b0,b1,lastb".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
                          "match_recognize (" +
                          " measures A.TheString as a, B[0].TheString as b0, B[1].TheString as b1, last(B.TheString) as lastb" +
                          " pattern (A B*)" +
                          " interval 10 seconds" +
                          " define" +
                          " A as A.TheString like \"A%\"," +
                          " B as B.TheString like \"B%\"" +
                          ") order by a, b0, b1, lastb";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            RunAssertion(epService, listener, stmt);

            stmt.Dispose();
            var model = epService.EPAdministrator.CompileEPL(text);
            SerializableObjectCopier.Copy(model);
            Assert.AreEqual(text, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
            Assert.AreEqual(text, stmt.Text);

            RunAssertion(epService, listener, stmt);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        private void RunAssertion(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt)
        {
            String[] fields = "a,b0,b1,lastb".Split(',');

            SendTimer(1000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("A1", 1));
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(10999, epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[] {new Object[] {"A1", null, null, null}});

            SendTimer(11000, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[] {new Object[] {"A1", null, null, null}});
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"A1", null, null, null}});

            SendTimer(13000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("A2", 2));
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(15000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("B1", 3));
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(22999, epService);
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(23000, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[]
                                                          {
                                                              new Object[] {"A1", null, null, null},
                                                              new[] {"A2", "B1", null, "B1"}
                                                          });
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"A2", "B1", null, "B1"}});

            SendTimer(25000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("A3", 4));
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(26000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("B2", 5));
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(29000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("B3", 6));
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(34999, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("B4", 7));
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(35000, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[]
                                                          {
                                                              new Object[] {"A1", null, null, null},
                                                              new[] {"A2", "B1", null, "B1"},
                                                              new[] {"A3", "B2", "B3", "B4"}
                                                          });
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"A3", "B2", "B3", "B4"}});
        }

        [Test]
        public void TestMultiCompleted()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            SendTimer(0, epService);
            String[] fields = "a,b0,b1,lastb".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
                          "match_recognize (" +
                          "  measures A.TheString as a, B[0].TheString as b0, B[1].TheString as b1, Last(B.TheString) as lastb" +
                          "  pattern (A B*) " +
                          "  interval 10 seconds " +
                          "  define " +
                          "    A as A.TheString like 'A%'," +
                          "    B as B.TheString like 'B%'" +
                          ") order by a, b0, b1, lastb";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendTimer(1000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("A1", 1));
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(5000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("A2", 2));
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(10999, epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[]
                                                          {
                                                              new Object[] {"A1", null, null, null},
                                                              new[] {"A2", null, null, null}
                                                          });

            SendTimer(11000, epService);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"A1", null, null, null}});

            SendTimer(15000, epService);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"A2", null, null, null}});

            SendTimer(21000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("A3", 3));
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(22000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("A4", 4));
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(23000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("B1", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B2", 6));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B3", 7));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B4", 8));
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(31000, epService);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"A3", null, null, null}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[]
                                                          {
                                                              new Object[] {"A1", null, null, null},
                                                              new[] {"A2", null, null, null},
                                                              new[] {"A3", null, null, null},
                                                              new[] {"A4", "B1", "B2", "B4"}
                                                          });

            SendTimer(32000, epService);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"A4", "B1", "B2", "B4"}});

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestPartitioned()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            SendTimer(0, epService);
            String[] fields = "a,b0,b1,lastb".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
                          "match_recognize (" +
                          "  partition by Cat " +
                          "  measures A.TheString as a, B[0].TheString as b0, B[1].TheString as b1, Last(B.TheString) as lastb" +
                          "  pattern (A B*) " +
                          "  INTERVAL 10 seconds " +
                          "  define " +
                          "    A as A.TheString like 'A%'," +
                          "    B as B.TheString like 'B%'" +
                          ") order by a, b0, b1, lastb";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendTimer(1000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("A1", "C1", 1));

            SendTimer(1000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("A2", "C2", 2));

            SendTimer(2000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("A3", "C3", 3));

            SendTimer(3000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("A4", "C4", 4));

            epService.EPRuntime.SendEvent(new SupportRecogBean("B1", "C3", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B2", "C1", 6));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B3", "C1", 7));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B4", "C4", 7));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                              new Object[][]
                                              {
                                                  new[] {"A1", "B2", "B3", "B3"},
                                                  new[] {"A2", null, null, null},
                                                  new[] {"A3", "B1", null, "B1"},
                                                  new[] {"A4", "B4", null, "B4"}
                                              });

            SendTimer(10999, epService);
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(11000, epService);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                              new[]
                                              {
                                                  new Object[] {"A1", "B2", "B3", "B3"},
                                                  new[] {"A2", null, null, null}
                                              });

            SendTimer(11999, epService);
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(12000, epService);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"A3", "B1", null, "B1"}});

            SendTimer(13000, epService);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                new[] {new Object[] {"A4", "B4", null, "B4"}});

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestMonthScoped()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            SupportUpdateListener listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName);}

            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime(epService, "2002-02-01T9:00:00.000");
            String text = "select * from SupportBean " +
                    "match_recognize (" +
                    " measures A.TheString as a, B[0].TheString as b0, B[1].TheString as b1 " +
                    " pattern (A B*)" +
                    " interval 1 month" +
                    " define" +
                    " A as A.TheString like \"A%\"," +
                    " B as B.TheString like \"B%\"" +
                    ")";
            epService.EPAdministrator.CreateEPL(text).Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("A1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("B1", 0));
            SendCurrentTimeWithMinus(epService, "2002-03-01T9:00:00.000", 1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendCurrentTime(epService, "2002-03-01T9:00:00.000");
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), "a,b0,b1".Split(','),
                    new Object[][] { new object[] {"A1", "B1", null}});

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }

        private void SendCurrentTime(EPServiceProvider epService, String time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeHelper.ParseDefaultMSec(time)));
        }

        private void SendCurrentTimeWithMinus(EPServiceProvider epService, String time, long minus) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeHelper.ParseDefaultMSec(time) - minus));
        }
    }
}
